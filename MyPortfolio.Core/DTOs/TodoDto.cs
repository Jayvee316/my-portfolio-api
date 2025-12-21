using System.ComponentModel.DataAnnotations;

namespace MyPortfolio.Core.DTOs;

/// <summary>
/// DTO for todo item
/// </summary>
public class TodoDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Completed { get; set; }
}

/// <summary>
/// Request DTO for creating a todo
/// </summary>
public class CreateTodoRequest
{
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;

    public bool Completed { get; set; }
}

/// <summary>
/// Request DTO for updating a todo
/// </summary>
public class UpdateTodoRequest
{
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;

    public bool Completed { get; set; }
}
