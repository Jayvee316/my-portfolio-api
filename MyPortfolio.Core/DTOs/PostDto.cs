using System.ComponentModel.DataAnnotations;

namespace MyPortfolio.Core.DTOs;

/// <summary>
/// DTO for blog post
/// </summary>
public class PostDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for creating/updating a post
/// </summary>
public class CreatePostRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Body is required")]
    [MinLength(10, ErrorMessage = "Body must be at least 10 characters")]
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for updating a post
/// </summary>
public class UpdatePostRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Body is required")]
    [MinLength(10, ErrorMessage = "Body must be at least 10 characters")]
    public string Body { get; set; } = string.Empty;
}
