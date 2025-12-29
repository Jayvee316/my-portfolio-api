using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Core.DTOs.Ecommerce;
using MyPortfolio.Core.Entities;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Api.Controllers;

/// <summary>
/// Products controller - CRUD operations for e-commerce products
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all products with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetProducts(
        [FromQuery] int? categoryId = null,
        [FromQuery] bool? featured = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] bool descending = false)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        // Apply filters
        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (featured.HasValue)
            query = query.Where(p => p.IsFeatured == featured.Value);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

        if (minPrice.HasValue)
            query = query.Where(p => (p.SalePrice ?? p.Price) >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => (p.SalePrice ?? p.Price) <= maxPrice.Value);

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "price" => descending ? query.OrderByDescending(p => p.SalePrice ?? p.Price) : query.OrderBy(p => p.SalePrice ?? p.Price),
            "date" => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        var products = await query
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                SalePrice = p.SalePrice,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                CategoryName = p.Category.Name
            })
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>
    /// Get featured products
    /// </summary>
    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> GetFeaturedProducts([FromQuery] int limit = 8)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                SalePrice = p.SalePrice,
                ImageUrl = p.ImageUrl,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                CategoryName = p.Category.Name
            })
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>
    /// Get a single product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                SalePrice = p.SalePrice,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                Images = p.Images,
                SKU = p.SKU,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    /// <summary>
    /// Create a new product (admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        // Verify category exists
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            return BadRequest("Invalid category ID");

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            SalePrice = request.SalePrice,
            StockQuantity = request.StockQuantity,
            ImageUrl = request.ImageUrl,
            Images = request.Images,
            SKU = request.SKU,
            IsFeatured = request.IsFeatured,
            CategoryId = request.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Load the category for the response
        await _context.Entry(product).Reference(p => p.Category).LoadAsync();

        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            SalePrice = product.SalePrice,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            Images = product.Images,
            SKU = product.SKU,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            CreatedAt = product.CreatedAt
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, dto);
    }

    /// <summary>
    /// Update a product (admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        // Verify category exists
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            return BadRequest("Invalid category ID");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.SalePrice = request.SalePrice;
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = request.ImageUrl;
        product.Images = request.Images;
        product.SKU = request.SKU;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Delete a product (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        // Check if product is in any orders
        var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
        if (hasOrders)
        {
            // Soft delete - just deactivate
            product.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Product deactivated (has existing orders)" });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Update product stock (admin only)
    /// </summary>
    [HttpPatch("{id}/stock")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int quantity)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        product.StockQuantity = quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { stockQuantity = product.StockQuantity });
    }
}
