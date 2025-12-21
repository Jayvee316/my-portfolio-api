namespace MyPortfolio.Core.Entities;

/// <summary>
/// Represents a portfolio project
/// </summary>
public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Technologies { get; set; } = new();
    public int Rating { get; set; }
    public string? GithubLink { get; set; }
    public string? LiveLink { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
