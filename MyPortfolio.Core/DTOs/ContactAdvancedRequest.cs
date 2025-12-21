using System.ComponentModel.DataAnnotations;

namespace MyPortfolio.Core.DTOs;

/// <summary>
/// Request DTO for advanced contact form
/// </summary>
public class ContactAdvancedRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? Phone { get; set; }

    public string? Company { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public string Category { get; set; } = "general";

    [Required(ErrorMessage = "Subject is required")]
    [MinLength(5, ErrorMessage = "Subject must be at least 5 characters")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [MinLength(10, ErrorMessage = "Message must be at least 10 characters")]
    [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public string Message { get; set; } = string.Empty;

    public bool Urgent { get; set; }
    public bool Newsletter { get; set; }
}
