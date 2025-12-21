using MyPortfolio.Core.DTOs.Auth;
using MyPortfolio.Core.Entities;

namespace MyPortfolio.Core.Interfaces;

/// <summary>
/// Interface for authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user and generate JWT token
    /// </summary>
    Task<LoginResponse?> LoginAsync(LoginRequest request);

    /// <summary>
    /// Register a new user
    /// </summary>
    Task<LoginResponse?> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetUserByIdAsync(int id);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Generate JWT token for a user
    /// </summary>
    string GenerateJwtToken(User user);

    /// <summary>
    /// Validate JWT token and return user ID
    /// </summary>
    int? ValidateToken(string token);
}
