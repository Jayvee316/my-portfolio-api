using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Core.DTOs;
using MyPortfolio.Core.Entities;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Api.Controllers;

/// <summary>
/// Portfolio projects controller - CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all projects
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
    {
        var projects = await _context.Projects
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Technologies = p.Technologies,
                Rating = p.Rating,
                GithubLink = p.GithubLink,
                LiveLink = p.LiveLink
            })
            .ToListAsync();

        return Ok(projects);
    }

    /// <summary>
    /// Get a single project by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
            return NotFound();

        return Ok(new ProjectDto
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            Technologies = project.Technologies,
            Rating = project.Rating,
            GithubLink = project.GithubLink,
            LiveLink = project.LiveLink
        });
    }

    /// <summary>
    /// Create a new project (admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectRequest request)
    {
        var project = new Project
        {
            Title = request.Title,
            Description = request.Description,
            Technologies = request.Technologies,
            Rating = request.Rating,
            GithubLink = request.GithubLink,
            LiveLink = request.LiveLink,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var dto = new ProjectDto
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            Technologies = project.Technologies,
            Rating = project.Rating,
            GithubLink = project.GithubLink,
            LiveLink = project.LiveLink
        };

        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, dto);
    }

    /// <summary>
    /// Update a project (admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] CreateProjectRequest request)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
            return NotFound();

        project.Title = request.Title;
        project.Description = request.Description;
        project.Technologies = request.Technologies;
        project.Rating = request.Rating;
        project.GithubLink = request.GithubLink;
        project.LiveLink = request.LiveLink;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Delete a project (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
            return NotFound();

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
