using System.ComponentModel.DataAnnotations;

namespace MyPortfolio.Core.DTOs;

/// <summary>
/// DTO for portfolio project
/// </summary>
public class ProjectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Technologies { get; set; } = new();
    public int Rating { get; set; }
    public string? GithubLink { get; set; }
    public string? LiveLink { get; set; }
}

/// <summary>
/// Request DTO for creating/updating a project
/// </summary>
public class CreateProjectRequest
{
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    public List<string> Technologies { get; set; } = new();

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [Url(ErrorMessage = "Invalid URL format")]
    public string? GithubLink { get; set; }

    [Url(ErrorMessage = "Invalid URL format")]
    public string? LiveLink { get; set; }
}
