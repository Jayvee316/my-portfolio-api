using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Infrastructure.Data;
using Stripe;

namespace MyPortfolio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _stripeSecretKey;
    private readonly string _stripeWebhookSecret;

    public PaymentController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
            ?? _configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe Secret Key not configured");
        _stripeWebhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")
            ?? _configuration["Stripe:WebhookSecret"]
            ?? "";

        StripeConfiguration.ApiKey = _stripeSecretKey;
    }

    /// <summary>
    /// Create a payment intent for checkout
    /// </summary>
    [HttpPost("create-payment-intent")]
    [Authorize]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        try
        {
            // Get user ID from JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            // Get cart items for the user
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return BadRequest(new { error = "Cart is empty" });
            }

            // Calculate total amount (in cents for Stripe)
            var subtotal = cartItems.Sum(c => (c.Product.SalePrice ?? c.Product.Price) * c.Quantity);
            var tax = subtotal * 0.1m; // 10% tax
            var shipping = request.ShippingCost ?? 5.00m; // Default shipping
            var totalAmount = (long)((subtotal + tax + shipping) * 100); // Convert to cents

            // Create payment intent
            var options = new PaymentIntentCreateOptions
            {
                Amount = totalAmount,
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() },
                    { "subtotal", subtotal.ToString("F2") },
                    { "tax", tax.ToString("F2") },
                    { "shipping", shipping.ToString("F2") }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return Ok(new
            {
                clientSecret = paymentIntent.ClientSecret,
                paymentIntentId = paymentIntent.Id,
                amount = totalAmount / 100.0m,
                subtotal,
                tax,
                shipping
            });
        }
        catch (StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create payment intent", details = ex.Message });
        }
    }

    /// <summary>
    /// Get Stripe publishable key (for frontend)
    /// </summary>
    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        var publishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY")
            ?? _configuration["Stripe:PublishableKey"]
            ?? "";

        return Ok(new { publishableKey });
    }

    /// <summary>
    /// Handle Stripe webhooks for payment events
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = string.IsNullOrEmpty(_stripeWebhookSecret)
                ? EventUtility.ParseEvent(json)
                : EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _stripeWebhookSecret);

            switch (stripeEvent.Type)
            {
                case EventTypes.PaymentIntentSucceeded:
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentSucceeded(paymentIntent);
                    break;

                case EventTypes.PaymentIntentPaymentFailed:
                    var failedPayment = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentFailed(failedPayment);
                    break;
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Confirm payment and create order (called after successful payment on frontend)
    /// </summary>
    [HttpPost("confirm-payment")]
    [Authorize]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            // Verify payment intent status
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(request.PaymentIntentId);

            if (paymentIntent.Status != "succeeded")
            {
                return BadRequest(new { error = "Payment not successful", status = paymentIntent.Status });
            }

            // Get cart items
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return BadRequest(new { error = "Cart is empty" });
            }

            // Calculate amounts
            var subtotal = cartItems.Sum(c => (c.Product.SalePrice ?? c.Product.Price) * c.Quantity);
            var tax = subtotal * 0.1m;
            var shipping = request.ShippingCost ?? 5.00m;
            var total = subtotal + tax + shipping;

            // Create order
            var order = new Core.Entities.Order
            {
                UserId = userId,
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                SubTotal = subtotal,
                Tax = tax,
                ShippingCost = shipping,
                TotalAmount = total,
                Status = "confirmed",
                PaymentStatus = "paid",
                PaymentMethod = "stripe",
                PaymentTransactionId = paymentIntent.Id,
                ShippingName = request.ShippingAddress.Name,
                ShippingAddress = request.ShippingAddress.Address,
                ShippingCity = request.ShippingAddress.City,
                ShippingState = request.ShippingAddress.State,
                ShippingZipCode = request.ShippingAddress.ZipCode,
                ShippingCountry = request.ShippingAddress.Country,
                ShippingPhone = request.ShippingAddress.Phone,
                CreatedAt = DateTime.UtcNow,
                OrderItems = cartItems.Select(c => new Core.Entities.OrderItem
                {
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    ProductImageUrl = c.Product.ImageUrl,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.SalePrice ?? c.Product.Price,
                    TotalPrice = (c.Product.SalePrice ?? c.Product.Price) * c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);

            // Update product stock
            foreach (var item in cartItems)
            {
                item.Product.StockQuantity -= item.Quantity;
            }

            // Clear cart
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                orderId = order.Id,
                orderNumber = order.OrderNumber,
                message = "Order created successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to confirm payment", details = ex.Message });
        }
    }

    private async Task HandlePaymentSucceeded(PaymentIntent? paymentIntent)
    {
        if (paymentIntent == null) return;

        // Log successful payment (order creation is handled by confirm-payment endpoint)
        Console.WriteLine($"Payment succeeded: {paymentIntent.Id}");
        await Task.CompletedTask;
    }

    private async Task HandlePaymentFailed(PaymentIntent? paymentIntent)
    {
        if (paymentIntent == null) return;

        // Log failed payment
        Console.WriteLine($"Payment failed: {paymentIntent.Id}");
        await Task.CompletedTask;
    }
}

// Request DTOs
public class CreatePaymentIntentRequest
{
    public decimal? ShippingCost { get; set; }
}

public class ConfirmPaymentRequest
{
    public string PaymentIntentId { get; set; } = "";
    public decimal? ShippingCost { get; set; }
    public ShippingAddressDto ShippingAddress { get; set; } = new();
}

public class ShippingAddressDto
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public string Country { get; set; } = "";
    public string? Phone { get; set; }
}
