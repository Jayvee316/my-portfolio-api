using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Core.Entities;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Api.Controllers;

/// <summary>
/// Skills controller - list and manage skills
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SkillsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// DTO for skill response grouped by category
    /// </summary>
    public class SkillCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public List<SkillDto> Skills { get; set; } = new();
    }

    public class SkillDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    /// <summary>
    /// Get all skills grouped by category
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SkillCategoryDto>>> GetSkills()
    {
        var skills = await _context.Skills.ToListAsync();

        var grouped = skills
            .GroupBy(s => s.Category)
            .Select(g => new SkillCategoryDto
            {
                Category = g.Key,
                Skills = g.Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Level = s.Level,
                    Color = s.Color
                }).ToList()
            })
            .ToList();

        return Ok(grouped);
    }

    /// <summary>
    /// Get all skills as flat list
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<Skill>>> GetAllSkills()
    {
        return Ok(await _context.Skills.ToListAsync());
    }

    /// <summary>
    /// Create a new skill (admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<Skill>> CreateSkill([FromBody] Skill skill)
    {
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllSkills), new { id = skill.Id }, skill);
    }

    /// <summary>
    /// Delete a skill (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var skill = await _context.Skills.FindAsync(id);

        if (skill == null)
            return NotFound();

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
