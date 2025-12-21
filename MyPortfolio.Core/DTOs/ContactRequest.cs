using System.ComponentModel.DataAnnotations;

namespace MyPortfolio.Core.DTOs;

/// <summary>
/// Request DTO for basic contact form
/// </summary>
public class ContactRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject is required")]
    [MinLength(5, ErrorMessage = "Subject must be at least 5 characters")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [MinLength(10, ErrorMessage = "Message must be at least 10 characters")]
    [MaxLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
    public string Message { get; set; } = string.Empty;
}
