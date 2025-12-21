namespace MyPortfolio.Core.Entities;

/// <summary>
/// Represents a technical skill
/// </summary>
public class Skill
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } // 0-100
    public string Color { get; set; } = "#3498db";
}
