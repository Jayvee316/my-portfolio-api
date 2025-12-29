using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Core.DTOs.Ecommerce;
using MyPortfolio.Core.Entities;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Api.Controllers;

/// <summary>
/// Orders controller - manages customer orders
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    private bool IsAdmin()
    {
        return User.IsInRole("admin");
    }

    /// <summary>
    /// Get user's orders (or all orders for admin)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderListDto>>> GetOrders([FromQuery] string? status = null)
    {
        var userId = GetUserId();
        var isAdmin = IsAdmin();

        var query = _context.Orders
            .Include(o => o.OrderItems)
            .AsQueryable();

        // Non-admin users can only see their own orders
        if (!isAdmin)
            query = query.Where(o => o.UserId == userId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                ItemCount = o.OrderItems.Sum(oi => oi.Quantity),
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return Ok(orders);
    }

    /// <summary>
    /// Get a specific order
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var userId = GetUserId();
        var isAdmin = IsAdmin();

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Id == id)
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound();

        // Non-admin users can only view their own orders
        if (!isAdmin && order.UserId != userId)
            return Forbid();

        var dto = new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            SubTotal = order.SubTotal,
            Tax = order.Tax,
            ShippingCost = order.ShippingCost,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            ShippingInfo = new ShippingInfoDto
            {
                Name = order.ShippingName,
                Address = order.ShippingAddress,
                City = order.ShippingCity,
                State = order.ShippingState,
                ZipCode = order.ShippingZipCode,
                Country = order.ShippingCountry,
                Phone = order.ShippingPhone
            },
            CustomerNotes = order.CustomerNotes,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                ProductImageUrl = oi.ProductImageUrl,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList(),
            CreatedAt = order.CreatedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Create order from cart (checkout)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetUserId();

        // Get cart items
        var cartItems = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            return BadRequest("Cart is empty");

        // Validate stock
        foreach (var item in cartItems)
        {
            if (item.Product.StockQuantity < item.Quantity)
                return BadRequest($"Insufficient stock for {item.Product.Name}");
        }

        // Calculate totals
        var subTotal = cartItems.Sum(ci => (ci.Product.SalePrice ?? ci.Product.Price) * ci.Quantity);
        var tax = subTotal * 0.1m; // 10% tax
        var shippingCost = subTotal >= 100 ? 0 : 10m; // Free shipping over $100
        var totalAmount = subTotal + tax + shippingCost;

        // Generate order number
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        // Create order
        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            SubTotal = subTotal,
            Tax = tax,
            ShippingCost = shippingCost,
            TotalAmount = totalAmount,
            PaymentMethod = request.PaymentMethod,
            CustomerNotes = request.CustomerNotes,
            ShippingName = request.ShippingInfo.Name,
            ShippingAddress = request.ShippingInfo.Address,
            ShippingCity = request.ShippingInfo.City,
            ShippingState = request.ShippingInfo.State,
            ShippingZipCode = request.ShippingInfo.ZipCode,
            ShippingCountry = request.ShippingInfo.Country,
            ShippingPhone = request.ShippingInfo.Phone
        };

        // Create order items and update stock
        foreach (var cartItem in cartItems)
        {
            var orderItem = new OrderItem
            {
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.Product.SalePrice ?? cartItem.Product.Price,
                TotalPrice = (cartItem.Product.SalePrice ?? cartItem.Product.Price) * cartItem.Quantity,
                ProductName = cartItem.Product.Name,
                ProductImageUrl = cartItem.Product.ImageUrl
            };
            order.OrderItems.Add(orderItem);

            // Reduce stock
            cartItem.Product.StockQuantity -= cartItem.Quantity;
        }

        _context.Orders.Add(order);

        // Clear cart
        _context.CartItems.RemoveRange(cartItems);

        await _context.SaveChangesAsync();

        return await GetOrder(order.Id);
    }

    /// <summary>
    /// Update order status (admin only)
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
            return NotFound();

        var validStatuses = new[] { "pending", "processing", "shipped", "delivered", "cancelled" };
        if (!validStatuses.Contains(request.Status.ToLower()))
            return BadRequest("Invalid status");

        order.Status = request.Status.ToLower();
        order.AdminNotes = request.AdminNotes;
        order.UpdatedAt = DateTime.UtcNow;

        if (request.Status.ToLower() == "shipped")
            order.ShippedAt = DateTime.UtcNow;
        else if (request.Status.ToLower() == "delivered")
            order.DeliveredAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { status = order.Status });
    }

    /// <summary>
    /// Update payment status (admin only)
    /// </summary>
    [HttpPatch("{id}/payment")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusRequest request)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
            return NotFound();

        var validStatuses = new[] { "unpaid", "paid", "refunded" };
        if (!validStatuses.Contains(request.PaymentStatus.ToLower()))
            return BadRequest("Invalid payment status");

        order.PaymentStatus = request.PaymentStatus.ToLower();
        order.PaymentTransactionId = request.PaymentTransactionId;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { paymentStatus = order.PaymentStatus });
    }

    /// <summary>
    /// Cancel an order (user can only cancel pending orders)
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userId = GetUserId();
        var isAdmin = IsAdmin();

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        // Non-admin can only cancel their own orders
        if (!isAdmin && order.UserId != userId)
            return Forbid();

        // Non-admin can only cancel pending orders
        if (!isAdmin && order.Status != "pending")
            return BadRequest("Can only cancel pending orders");

        // Restore stock
        foreach (var item in order.OrderItems)
        {
            item.Product.StockQuantity += item.Quantity;
        }

        order.Status = "cancelled";
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Order cancelled successfully" });
    }
}
