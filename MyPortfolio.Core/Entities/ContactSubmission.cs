namespace MyPortfolio.Core.Entities;

/// <summary>
/// Represents a contact form submission
/// </summary>
public class ContactSubmission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string Category { get; set; } = "general"; // general, job, collaboration, feedback
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Urgent { get; set; }
    public bool Newsletter { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "received"; // received, processing, completed
}
