namespace MyPortfolio.Core.DTOs;

/// <summary>
/// Response DTO for contact form submission
/// </summary>
public class ContactResponse
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
