namespace MyPortfolio.Core.DTOs.Auth;

/// <summary>
/// Response DTO for successful login
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

/// <summary>
/// DTO for user information (without sensitive data)
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
