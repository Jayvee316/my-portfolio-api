using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyPortfolio.Core.DTOs.Auth;
using MyPortfolio.Core.Entities;
using MyPortfolio.Core.Interfaces;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Infrastructure.Services;

/// <summary>
/// Authentication service implementation with JWT (JSON Web Token) support.
///
/// ========================================
/// WHAT IS JWT (JSON Web Token)?
/// ========================================
/// JWT is a compact, URL-safe way to represent claims between two parties.
/// It's commonly used for authentication in web APIs.
///
/// A JWT consists of 3 parts separated by dots (.):
/// 1. HEADER    - Contains the algorithm used (e.g., HS256) and token type
/// 2. PAYLOAD   - Contains the claims (user data like id, email, role)
/// 3. SIGNATURE - Verifies the token hasn't been tampered with
///
/// Example JWT:
/// eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4ifQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
/// |_____________HEADER______________|._____________PAYLOAD_______________|.____________SIGNATURE______________|
///
/// ========================================
/// HOW JWT AUTHENTICATION WORKS:
/// ========================================
/// 1. User sends login credentials (email/password) to the server
/// 2. Server validates credentials against the database
/// 3. If valid, server generates a JWT token containing user info
/// 4. Server sends the token back to the client (Angular app)
/// 5. Client stores the token (usually in localStorage)
/// 6. For subsequent requests, client sends token in Authorization header:
///    Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
/// 7. Server validates the token and extracts user info from it
///
/// ========================================
/// WHY USE JWT INSTEAD OF SESSIONS?
/// ========================================
/// - STATELESS: Server doesn't need to store session data in memory
/// - SCALABLE:  Works well with multiple servers (load balancing)
/// - MOBILE:    Works great for mobile apps and SPAs
/// - SECURE:    Token is signed, so it can't be tampered with
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token if successful.
    ///
    /// FLOW:
    /// 1. Find user by email or username in database
    /// 2. Verify the password using BCrypt
    /// 3. If valid, generate and return a JWT token
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        // ========================================
        // STEP 1: Find user in database
        // ========================================
        // We search by both email and name to allow flexible login
        // ToLower() makes the search case-insensitive (JOHN@email.com == john@email.com)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Username.ToLower()
                                   || u.Name.ToLower() == request.Username.ToLower());

        // If no user found, return null (invalid credentials)
        if (user == null)
            return null;

        // ========================================
        // STEP 2: Verify password using BCrypt
        // ========================================
        // WHY BCRYPT?
        // - We NEVER store plain text passwords in the database (security risk!)
        // - BCrypt is a one-way hashing algorithm - you can't reverse it
        // - BCrypt automatically handles "salting" - adding random data to prevent
        //   rainbow table attacks (pre-computed hash tables)
        // - BCrypt.Verify() hashes the input password and compares it to the stored hash
        //
        // Example:
        // Password: "admin123"
        // Stored Hash: "$2a$11$K5X3N9N8X..." (looks nothing like the password!)
        // BCrypt.Verify("admin123", "$2a$11$K5X3N9N8X...") → true
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        // ========================================
        // STEP 3: Generate JWT token
        // ========================================
        // If credentials are valid, create a token for the user
        var token = GenerateJwtToken(user);

        // Return the token and user info (excluding sensitive data like password)
        return new LoginResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    /// <summary>
    /// Registers a new user account.
    ///
    /// SECURITY CONSIDERATIONS:
    /// - Email uniqueness is enforced (can't have duplicate accounts)
    /// - Password is hashed before storing (never store plain text!)
    /// - New users get "user" role by default (principle of least privilege)
    /// </summary>
    public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
    {
        // ========================================
        // STEP 1: Check if email already exists
        // ========================================
        // Prevent duplicate accounts with the same email
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            return null;

        // ========================================
        // STEP 2: Create new user with hashed password
        // ========================================
        // BCrypt.HashPassword() creates a secure hash:
        // - Automatically generates a random "salt"
        // - Uses a "work factor" to make hashing slow (prevents brute force attacks)
        // - Returns a string like: "$2a$11$K5X3N9N8X..."
        //   where $2a$ = algorithm, $11$ = work factor, rest = salt+hash
        var user = new User
        {
            Name = request.Name,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Name.ToLower() == "admin" ? "admin" : "user", // Demo: admin role for "admin" name
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // ========================================
        // STEP 3: Generate JWT token for immediate login
        // ========================================
        // After registration, user is automatically logged in
        var token = GenerateJwtToken(user);

        return new LoginResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    /// <summary>
    /// Generates a JWT token for the authenticated user.
    ///
    /// ========================================
    /// ANATOMY OF A JWT TOKEN
    /// ========================================
    /// The token contains "claims" - pieces of information about the user.
    /// These claims are encoded (not encrypted!) in the token payload.
    ///
    /// IMPORTANT: Anyone can READ the claims by decoding the token (try jwt.io)
    /// So NEVER put sensitive data like passwords in claims!
    /// The signature only prevents TAMPERING, not reading.
    /// </summary>
    public string GenerateJwtToken(User user)
    {
        // ========================================
        // STEP 1: Create the signing key
        // ========================================
        // The key is used to create the signature (HMAC-SHA256)
        // This key must be:
        // - At least 32 characters (256 bits) for HS256
        // - Kept SECRET (only the server knows it)
        // - Same key is used to create AND validate tokens
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));

        // ========================================
        // STEP 2: Create signing credentials
        // ========================================
        // HmacSha256 = HMAC (Hash-based Message Authentication Code) with SHA-256
        // This creates a signature that proves the token was created by us
        // and hasn't been modified
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // ========================================
        // STEP 3: Define the claims (user data in token)
        // ========================================
        // Claims are key-value pairs that describe the user
        // Standard claim types are defined in ClaimTypes class
        //
        // Common claims:
        // - NameIdentifier: Unique user ID (used to look up user in database)
        // - Name: Display name
        // - Email: User's email
        // - Role: User's role for authorization (admin, user, etc.)
        // - Jti: JWT ID - unique identifier for this specific token
        var claims = new[]
        {
            // User's unique ID - MOST IMPORTANT claim for identifying the user
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

            // User's display name
            new Claim(ClaimTypes.Name, user.Name),

            // User's email address
            new Claim(ClaimTypes.Email, user.Email),

            // User's role - used for authorization (e.g., [Authorize(Roles = "admin")])
            new Claim(ClaimTypes.Role, user.Role),

            // Unique token ID - useful for token revocation/blacklisting
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // ========================================
        // STEP 4: Set token expiration
        // ========================================
        // Tokens should expire to limit damage if stolen
        // Short expiration = more secure but user has to login more often
        // Common values: 15 min - 24 hours depending on security needs
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        // ========================================
        // STEP 5: Create the token
        // ========================================
        // The JwtSecurityToken combines all the pieces:
        // - Issuer: Who created the token (your API)
        // - Audience: Who the token is intended for (your Angular app)
        // - Claims: User data
        // - Expires: When the token becomes invalid
        // - SigningCredentials: How to sign the token
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],      // e.g., "MyPortfolioApi"
            audience: _configuration["Jwt:Audience"],  // e.g., "MyPortfolioApp"
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        // ========================================
        // STEP 6: Serialize to string
        // ========================================
        // Convert the token object to the actual JWT string
        // Result: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIx..."
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token and extracts the user ID.
    ///
    /// This is used when you need to manually validate a token
    /// (ASP.NET Core middleware usually does this automatically).
    ///
    /// Returns the user ID if valid, null if invalid/expired.
    /// </summary>
    public int? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "");

            // Validate the token using the same parameters used to create it
            // If ANY validation fails, an exception is thrown
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                // Verify the signature using our secret key
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                // Verify the issuer (who created the token)
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],

                // Verify the audience (who the token is for)
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],

                // No tolerance for expired tokens
                // ClockSkew handles minor time differences between servers
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            // Extract the user ID from the validated token
            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

            return userId;
        }
        catch
        {
            // Token is invalid (expired, tampered, wrong signature, etc.)
            return null;
        }
    }
}
