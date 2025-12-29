using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Core.DTOs.Ecommerce;
using MyPortfolio.Core.Entities;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Api.Controllers;

/// <summary>
/// Shopping cart controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    /// <summary>
    /// Get current user's cart
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetUserId();

        var cartItems = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .Select(ci => new CartItemDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                ProductImageUrl = ci.Product.ImageUrl,
                UnitPrice = ci.Product.Price,
                SalePrice = ci.Product.SalePrice,
                Quantity = ci.Quantity,
                TotalPrice = (ci.Product.SalePrice ?? ci.Product.Price) * ci.Quantity,
                StockQuantity = ci.Product.StockQuantity
            })
            .ToListAsync();

        var cart = new CartDto
        {
            Items = cartItems,
            SubTotal = cartItems.Sum(i => i.TotalPrice),
            TotalItems = cartItems.Sum(i => i.Quantity)
        };

        return Ok(cart);
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = GetUserId();

        // Verify product exists and is active
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null || !product.IsActive)
            return BadRequest("Product not available");

        if (product.StockQuantity < request.Quantity)
            return BadRequest("Insufficient stock");

        // Check if item already in cart
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == request.ProductId);

        if (existingItem != null)
        {
            // Update quantity
            var newQuantity = existingItem.Quantity + request.Quantity;
            if (newQuantity > product.StockQuantity)
                return BadRequest("Cannot add more than available stock");

            existingItem.Quantity = newQuantity;
        }
        else
        {
            // Add new item
            var cartItem = new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };
            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();

        return await GetCart();
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    [HttpPut("{itemId}")]
    public async Task<ActionResult<CartDto>> UpdateCartItem(int itemId, [FromBody] UpdateCartItemRequest request)
    {
        var userId = GetUserId();

        var cartItem = await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.UserId == userId);

        if (cartItem == null)
            return NotFound();

        if (request.Quantity <= 0)
        {
            _context.CartItems.Remove(cartItem);
        }
        else
        {
            if (request.Quantity > cartItem.Product.StockQuantity)
                return BadRequest("Cannot add more than available stock");

            cartItem.Quantity = request.Quantity;
        }

        await _context.SaveChangesAsync();

        return await GetCart();
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    [HttpDelete("{itemId}")]
    public async Task<ActionResult<CartDto>> RemoveFromCart(int itemId)
    {
        var userId = GetUserId();

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.UserId == userId);

        if (cartItem == null)
            return NotFound();

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return await GetCart();
    }

    /// <summary>
    /// Clear entire cart
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();

        var cartItems = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
