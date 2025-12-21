using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Core.DTOs;
using MyPortfolio.Core.Entities;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Api.Controllers;

/// <summary>
/// Blog posts controller - CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PostsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all posts
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts()
    {
        var posts = await _context.Posts
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Title = p.Title,
                Body = p.Body
            })
            .ToListAsync();

        return Ok(posts);
    }

    /// <summary>
    /// Get a single post by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetPost(int id)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post == null)
            return NotFound();

        return Ok(new PostDto
        {
            Id = post.Id,
            UserId = post.UserId,
            Title = post.Title,
            Body = post.Body
        });
    }

    /// <summary>
    /// Create a new post (requires authentication)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var post = new Post
        {
            UserId = userId,
            Title = request.Title,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var dto = new PostDto
        {
            Id = post.Id,
            UserId = post.UserId,
            Title = post.Title,
            Body = post.Body
        };

        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, dto);
    }

    /// <summary>
    /// Update an existing post (requires authentication)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostRequest request)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post == null)
            return NotFound();

        // Check ownership or admin role
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (post.UserId != userId && role != "admin")
            return Forbid();

        post.Title = request.Title;
        post.Body = request.Body;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Delete a post (requires authentication)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post == null)
            return NotFound();

        // Check ownership or admin role
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (post.UserId != userId && role != "admin")
            return Forbid();

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
