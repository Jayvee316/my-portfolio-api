using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Core.DTOs;
using MyPortfolio.Core.Entities;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Api.Controllers;

/// <summary>
/// Todos controller - CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly AppDbContext _context;

    public TodosController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all todos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoDto>>> GetTodos()
    {
        var todos = await _context.Todos
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TodoDto
            {
                Id = t.Id,
                UserId = t.UserId,
                Title = t.Title,
                Completed = t.Completed
            })
            .ToListAsync();

        return Ok(todos);
    }

    /// <summary>
    /// Get a single todo by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoDto>> GetTodo(int id)
    {
        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
            return NotFound();

        return Ok(new TodoDto
        {
            Id = todo.Id,
            UserId = todo.UserId,
            Title = todo.Title,
            Completed = todo.Completed
        });
    }

    /// <summary>
    /// Create a new todo (requires authentication)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TodoDto>> CreateTodo([FromBody] CreateTodoRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var todo = new Todo
        {
            UserId = userId,
            Title = request.Title,
            Completed = request.Completed,
            CreatedAt = DateTime.UtcNow
        };

        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        var dto = new TodoDto
        {
            Id = todo.Id,
            UserId = todo.UserId,
            Title = todo.Title,
            Completed = todo.Completed
        };

        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, dto);
    }

    /// <summary>
    /// Update a todo (requires authentication)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTodo(int id, [FromBody] UpdateTodoRequest request)
    {
        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
            return NotFound();

        // Check ownership or admin role
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (todo.UserId != userId && role != "admin")
            return Forbid();

        todo.Title = request.Title;
        todo.Completed = request.Completed;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Delete a todo (requires authentication)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var todo = await _context.Todos.FindAsync(id);

        if (todo == null)
            return NotFound();

        // Check ownership or admin role
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (todo.UserId != userId && role != "admin")
            return Forbid();

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
