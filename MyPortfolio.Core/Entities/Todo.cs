namespace MyPortfolio.Core.Entities;

/// <summary>
/// Represents a todo item
/// </summary>
public class Todo
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User? User { get; set; }
}
