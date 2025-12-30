using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyPortfolio.Core.Interfaces;
using MyPortfolio.Infrastructure.Data;
using MyPortfolio.Infrastructure.Services;

// ==========================================================================================================
// ASP.NET CORE WEB API - PROGRAM.CS
// ==========================================================================================================
// This is the entry point of your ASP.NET Core application.
// It configures all the services (dependency injection) and middleware (request pipeline).
//
// The file is organized into two main sections:
// 1. SERVICE CONFIGURATION (builder.Services) - What your app can do
// 2. MIDDLEWARE PIPELINE (app.Use...) - How requests are processed
// ==========================================================================================================

var builder = WebApplication.CreateBuilder(args);

// ==========================================================================================================
// SECTION 1: SERVICE CONFIGURATION (Dependency Injection Container)
// ==========================================================================================================
// Services are classes that provide functionality to your app.
// They are registered here and then "injected" into controllers/other services.
//
// Lifetimes:
// - AddSingleton: One instance for the entire app lifetime
// - AddScoped:    One instance per HTTP request (most common for DB contexts)
// - AddTransient: New instance every time it's requested
// ==========================================================================================================

// ----------------------------------------------------------------------------------------------------------
// DATABASE CONFIGURATION (Entity Framework Core with PostgreSQL)
// ----------------------------------------------------------------------------------------------------------
// Entity Framework Core (EF Core) is an ORM (Object-Relational Mapper).
// It lets you work with database tables as C# classes (no SQL needed!).
//
// How it works:
// - You define C# classes (Entities) like User, Post, etc.
// - EF Core maps them to database tables
// - You use LINQ queries instead of raw SQL
// - Example: _context.Users.Where(u => u.Email == "test@example.com")
//            becomes: SELECT * FROM Users WHERE Email = 'test@example.com'
// ----------------------------------------------------------------------------------------------------------
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrEmpty(connectionString))
{
    // LOCAL DEVELOPMENT: Use connection string from appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}
else
{
    // PRODUCTION (Render/Neon): Convert PostgreSQL URI format to Npgsql format
    // URI format:   postgresql://user:pass@host/database?sslmode=require
    // Npgsql format: Host=host;Database=database;Username=user;Password=pass;SSL Mode=Require
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}

// Register the DbContext with PostgreSQL provider
// AddDbContext registers it as "Scoped" (one instance per request)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ----------------------------------------------------------------------------------------------------------
// JWT AUTHENTICATION CONFIGURATION
// ----------------------------------------------------------------------------------------------------------
// This section configures how ASP.NET Core validates incoming JWT tokens.
//
// AUTHENTICATION vs AUTHORIZATION:
// - Authentication: "Who are you?" (validating the JWT token)
// - Authorization:  "What can you do?" (checking roles/permissions)
//
// JWT VALIDATION PROCESS:
// 1. Client sends request with header: "Authorization: Bearer eyJhbGc..."
// 2. Middleware extracts the token from the header
// 3. Token is validated (signature, expiration, issuer, audience)
// 4. If valid, claims are extracted and User object is populated
// 5. Request continues to the controller
// 6. If invalid, 401 Unauthorized is returned automatically
// ----------------------------------------------------------------------------------------------------------

// Get the secret key for signing/validating tokens
// This MUST match the key used in AuthService.GenerateJwtToken()
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key not configured");
var key = Encoding.UTF8.GetBytes(jwtKey);

// Configure authentication to use JWT Bearer tokens
builder.Services.AddAuthentication(options =>
{
    // Set JWT as the default authentication scheme
    // This means [Authorize] attribute will use JWT validation
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // ----------------------------------------------------------------------------------------------------------
    // JWT BEARER OPTIONS
    // ----------------------------------------------------------------------------------------------------------
    // These settings control how tokens are validated.
    // IMPORTANT: These must match the settings used when CREATING the token!
    // ----------------------------------------------------------------------------------------------------------

    // In development, we don't require HTTPS (set to true in production with real certificates)
    options.RequireHttpsMetadata = false;

    // Store the token in AuthenticationProperties (useful for accessing it later)
    options.SaveToken = true;

    // Token validation parameters - THE MOST IMPORTANT PART!
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // ======================================
        // SIGNATURE VALIDATION
        // ======================================
        // Verify the token was signed with our secret key
        // This prevents attackers from creating fake tokens
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        // ======================================
        // ISSUER VALIDATION
        // ======================================
        // Verify the token was created by our API (not some other server)
        // The issuer is embedded in the token and must match this value
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],  // e.g., "MyPortfolioApi"

        // ======================================
        // AUDIENCE VALIDATION
        // ======================================
        // Verify the token was intended for our application
        // Useful when multiple apps share an auth server
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],  // e.g., "MyPortfolioApp"

        // ======================================
        // LIFETIME VALIDATION
        // ======================================
        // Verify the token hasn't expired
        // This is ALWAYS enabled by default, just being explicit
        ValidateLifetime = true,

        // ======================================
        // CLOCK SKEW
        // ======================================
        // Tolerance for clock differences between servers
        // Default is 5 minutes, we set to 0 for stricter validation
        // In production, you might want 1-2 minutes tolerance
        ClockSkew = TimeSpan.Zero
    };
});

// Register the Authorization service (works with [Authorize] attribute)
builder.Services.AddAuthorization();

// ----------------------------------------------------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing) CONFIGURATION
// ----------------------------------------------------------------------------------------------------------
// CORS is a security feature that controls which websites can call your API.
//
// WHY IS THIS NEEDED?
// Browsers block requests from one domain to another by default (Same-Origin Policy).
// Your Angular app (localhost:4200 or Vercel) is a different origin than your API.
// CORS tells the browser "it's okay, allow requests from these origins".
//
// WITHOUT CORS: Browser blocks the request, you see CORS errors in console
// WITH CORS:    Browser allows the request to proceed
//
// CORS HEADERS SENT BY SERVER:
// - Access-Control-Allow-Origin: https://your-angular-app.vercel.app
// - Access-Control-Allow-Methods: GET, POST, PUT, DELETE
// - Access-Control-Allow-Headers: Content-Type, Authorization
// - Access-Control-Allow-Credentials: true
// ----------------------------------------------------------------------------------------------------------
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',')
    ?? new[] {
        "http://localhost:4200",
        "http://localhost:4201",
        "https://localhost:4200",
        "http://localhost:53870"  // jayvee-dashboard-1 dev port
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(allowedOrigins)  // Which origins can access
            .AllowAnyMethod()               // GET, POST, PUT, DELETE, etc.
            .AllowAnyHeader()               // Content-Type, Authorization, etc.
            .AllowCredentials();            // Allow cookies/auth headers
    });
});

// ----------------------------------------------------------------------------------------------------------
// DEPENDENCY INJECTION - REGISTER APPLICATION SERVICES
// ----------------------------------------------------------------------------------------------------------
// This is where we tell ASP.NET Core how to create our services.
// When a controller needs IAuthService, ASP.NET will create an AuthService instance.
//
// AddScoped means: One instance per HTTP request
// - Request 1 gets AuthService instance A
// - Request 2 gets AuthService instance B (different instance)
// ----------------------------------------------------------------------------------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Register HttpClient factory for external API calls (GitHub, etc.)
builder.Services.AddHttpClient();

// Register controllers (the classes that handle HTTP requests)
builder.Services.AddControllers();

// ----------------------------------------------------------------------------------------------------------
// SWAGGER/OPENAPI CONFIGURATION
// ----------------------------------------------------------------------------------------------------------
// Swagger generates interactive API documentation.
// It creates the /swagger page where you can test your endpoints.
//
// Benefits:
// - Auto-generated documentation from your code
// - Interactive "Try it out" feature
// - Shows request/response formats
// - Great for testing and sharing API specs with frontend devs
// ----------------------------------------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // API information shown in Swagger UI
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyPortfolio API",
        Version = "v1",
        Description = "Backend API for Portfolio Angular Application"
    });

    // ----------------------------------------------------------------------------------------------------------
    // ADD JWT AUTHENTICATION TO SWAGGER
    // ----------------------------------------------------------------------------------------------------------
    // This adds the "Authorize" button in Swagger UI.
    // You can paste your JWT token there to test protected endpoints.
    //
    // Flow:
    // 1. Call /api/auth/login to get a token
    // 2. Click "Authorize" button in Swagger
    // 3. Enter: Bearer eyJhbGciOiJIUzI1NiIs... (with "Bearer " prefix)
    // 4. Now you can test [Authorize] endpoints
    // ----------------------------------------------------------------------------------------------------------
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Build the application (finalize service configuration)
var app = builder.Build();

// ==========================================================================================================
// SECTION 2: AUTO-APPLY DATABASE MIGRATIONS
// ==========================================================================================================
// Automatically applies any pending Entity Framework migrations when the app starts.
// This ensures the database schema is always up-to-date with your entity models.
//
// Benefits:
// - No need to manually run "dotnet ef database update" after deployment
// - Database schema stays in sync with code automatically
//
// Note: For large production apps with multiple instances, consider using a separate
// migration job or CI/CD pipeline instead to avoid race conditions.
// ==========================================================================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// ==========================================================================================================
// SECTION 3: DATABASE SEEDING
// ==========================================================================================================
// Seeds initial data into the database (users, posts, etc.)
// This runs once when the app starts (after migrations are applied).
// ==========================================================================================================
await DataSeeder.SeedAsync(app.Services);

// ==========================================================================================================
// SECTION 4: MIDDLEWARE PIPELINE CONFIGURATION
// ==========================================================================================================
// Middleware are components that process HTTP requests and responses.
// They execute in ORDER - the order you add them matters!
//
// Request flow:
// Client Request → Swagger → CORS → Authentication → Authorization → Controller → Response
//
// Each middleware can:
// - Process the request and pass to next middleware
// - Short-circuit and return a response immediately (e.g., 401 Unauthorized)
// - Modify the request or response
// ==========================================================================================================

// Swagger middleware - serves the /swagger UI page
// Usually only enabled in development, but we keep it for learning purposes
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MyPortfolio API v1");
    options.RoutePrefix = "swagger";  // Access at /swagger
});

// CORS middleware - must be BEFORE authentication
// Handles preflight OPTIONS requests and adds CORS headers
app.UseCors("AllowAngular");

// ----------------------------------------------------------------------------------------------------------
// AUTHENTICATION & AUTHORIZATION MIDDLEWARE
// ----------------------------------------------------------------------------------------------------------
// ORDER MATTERS! Authentication must come before Authorization.
//
// UseAuthentication():
// - Reads the Authorization header
// - Validates the JWT token
// - Populates the User object with claims
//
// UseAuthorization():
// - Checks if the user has required roles/permissions
// - Enforces [Authorize] attributes on controllers
// ----------------------------------------------------------------------------------------------------------
app.UseAuthentication();  // "Who are you?"
app.UseAuthorization();   // "What can you do?"

// Map controller routes
// This connects HTTP requests to your controller methods
// e.g., GET /api/posts → PostsController.GetAll()
app.MapControllers();

// Default route - redirect root URL to Swagger documentation
app.MapGet("/", () => Results.Redirect("/swagger"));

// Start the application and listen for requests
app.Run();
