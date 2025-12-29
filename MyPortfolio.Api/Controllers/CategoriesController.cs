using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Core.DTOs.Ecommerce;
using MyPortfolio.Core.Entities;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Api.Controllers;

/// <summary>
/// Product categories controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories([FromQuery] bool includeInactive = false)
    {
        var query = _context.Categories.AsQueryable();

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        var categories = await query
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Get a category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound();

        return Ok(category);
    }

    /// <summary>
    /// Create a new category (admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            ProductCount = 0
        };

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, dto);
    }

    /// <summary>
    /// Update a category (admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        category.Name = request.Name;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Delete a category (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        if (category.Products.Any())
            return BadRequest("Cannot delete category with existing products");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
