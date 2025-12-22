using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.Core.DTOs.Auth;
using MyPortfolio.Core.Interfaces;

namespace MyPortfolio.Api.Controllers;

/// <summary>
/// Authentication controller handling login, registration, and user info.
///
/// ========================================
/// REST API AUTHENTICATION FLOW
/// ========================================
///
/// 1. REGISTRATION (POST /api/auth/register)
///    Client sends: { name, email, password }
///    Server returns: { token, user }
///
/// 2. LOGIN (POST /api/auth/login)
///    Client sends: { username, password }
///    Server returns: { token, user }
///
/// 3. PROTECTED REQUESTS (any endpoint with [Authorize])
///    Client sends: Header "Authorization: Bearer {token}"
///    Server validates token and processes request
///
/// ========================================
/// ATTRIBUTE EXPLANATIONS
/// ========================================
/// [ApiController]     - Enables API-specific behaviors (auto model validation, binding source inference)
/// [Route]            - Defines the URL path for this controller
/// [HttpPost/Get]     - Specifies the HTTP method for the action
/// [Authorize]        - Requires valid JWT token to access
/// [FromBody]         - Binds request body JSON to parameter
/// </summary>
[ApiController]
[Route("api/[controller]")]  // Results in /api/auth (controller name without "Controller" suffix)
public class AuthController : ControllerBase
{
    // ========================================
    // DEPENDENCY INJECTION
    // ========================================
    // Instead of creating AuthService directly (new AuthService()),
    // we receive it through the constructor. This is called
    // "Dependency Injection" (DI).
    //
    // Benefits of DI:
    // - Loose coupling (controller doesn't know HOW auth works)
    // - Easy to test (can inject mock services)
    // - Single instance management (configured in Program.cs)
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account.
    ///
    /// POST /api/auth/register
    ///
    /// Request body:
    /// {
    ///   "name": "John Doe",
    ///   "email": "john@example.com",
    ///   "password": "securePassword123"
    /// }
    ///
    /// Success response (200 OK):
    /// {
    ///   "token": "eyJhbGciOiJIUzI1NiIs...",
    ///   "user": { "id": 1, "name": "John Doe", "email": "john@example.com", "role": "user" }
    /// }
    ///
    /// Error response (400 Bad Request):
    /// { "message": "Email already exists" }
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        // Call the auth service to create the user
        var result = await _authService.RegisterAsync(request);

        // If null, email already exists
        if (result == null)
            return BadRequest(new { message = "Email already exists" });

        // Return the JWT token and user info
        // Ok() = HTTP 200 status code
        return Ok(result);
    }

    /// <summary>
    /// Login with username/email and password.
    ///
    /// POST /api/auth/login
    ///
    /// Request body:
    /// {
    ///   "username": "john@example.com",  // Can be email OR name
    ///   "password": "securePassword123"
    /// }
    ///
    /// Success response (200 OK):
    /// {
    ///   "token": "eyJhbGciOiJIUzI1NiIs...",
    ///   "user": { "id": 1, "name": "John Doe", "email": "john@example.com", "role": "user" }
    /// }
    ///
    /// Error response (401 Unauthorized):
    /// { "message": "Invalid username or password" }
    ///
    /// ========================================
    /// SECURITY NOTE
    /// ========================================
    /// We return the same error message for both "user not found" and
    /// "wrong password". This prevents attackers from discovering which
    /// emails are registered (enumeration attack).
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // Attempt to authenticate
        var result = await _authService.LoginAsync(request);

        // If null, credentials are invalid
        if (result == null)
            return Unauthorized(new { message = "Invalid username or password" });

        // Return the JWT token and user info
        return Ok(result);
    }

    /// <summary>
    /// Get the currently authenticated user's info.
    ///
    /// GET /api/auth/me
    ///
    /// ========================================
    /// [Authorize] ATTRIBUTE EXPLAINED
    /// ========================================
    /// This attribute means:
    /// 1. Request MUST include "Authorization: Bearer {token}" header
    /// 2. Token MUST be valid (not expired, correct signature)
    /// 3. If invalid, ASP.NET automatically returns 401 Unauthorized
    ///
    /// You can also require specific roles:
    /// [Authorize(Roles = "admin")]        - Only admins can access
    /// [Authorize(Roles = "admin,user")]   - Admins OR users can access
    ///
    /// ========================================
    /// HOW THE USER IS EXTRACTED FROM TOKEN
    /// ========================================
    /// When the JWT middleware validates the token, it automatically:
    /// 1. Extracts the claims from the token
    /// 2. Creates a ClaimsPrincipal (the User property)
    /// 3. Makes it available in the controller
    ///
    /// So User.FindFirst(ClaimTypes.NameIdentifier) gets the user ID
    /// that was embedded in the token during login.
    /// </summary>
    [HttpGet("me")]
    [Authorize]  // This endpoint requires authentication
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        // ========================================
        // Extract user ID from the JWT token claims
        // ========================================
        // The "User" property is automatically populated by ASP.NET
        // from the validated JWT token. It contains all the claims
        // that were added when the token was created.
        //
        // ClaimTypes.NameIdentifier = the user's ID (we set this in AuthService)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Validate the claim exists and is a valid integer
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        // ========================================
        // Fetch fresh user data from database
        // ========================================
        // Why fetch from DB instead of just using claims?
        // - Claims might be outdated (token was created hours ago)
        // - User might have updated their profile
        // - Some data shouldn't be in the token (too large, sensitive)
        var user = await _authService.GetUserByIdAsync(userId);

        if (user == null)
            return NotFound();

        // Return user data (without sensitive fields like password hash)
        return Ok(new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        });
    }
}
