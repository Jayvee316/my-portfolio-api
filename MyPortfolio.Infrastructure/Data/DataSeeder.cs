using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyPortfolio.Core.Entities;

namespace MyPortfolio.Infrastructure.Data;

/// <summary>
/// Seeds initial data into the database
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Seed the database with initial data
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Apply pending migrations
        await context.Database.MigrateAsync();

        // Seed data if tables are empty
        await SeedUsersAsync(context);
        await SeedPostsAsync(context);
        await SeedProjectsAsync(context);
        await SeedSkillsAsync(context);
        await SeedTodosAsync(context);
    }

    private static async Task SeedUsersAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var users = new List<User>
        {
            new()
            {
                Name = "Admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "admin",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "John Doe",
                Email = "john@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "user",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Jane Smith",
                Email = "jane@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "user",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPostsAsync(AppDbContext context)
    {
        if (await context.Posts.AnyAsync()) return;

        var posts = new List<Post>
        {
            new()
            {
                UserId = 1,
                Title = "Getting Started with Angular",
                Body = "Angular is a powerful framework for building web applications. In this post, we'll explore the basics of Angular and how to get started with your first project.",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                UserId = 1,
                Title = "Understanding C# Async/Await",
                Body = "Asynchronous programming is essential for building responsive applications. Learn how to use async/await in C# effectively.",
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new()
            {
                UserId = 2,
                Title = "Building REST APIs with ASP.NET Core",
                Body = "REST APIs are the backbone of modern web applications. This guide will walk you through creating a RESTful API using ASP.NET Core.",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                UserId = 2,
                Title = "Database Design Best Practices",
                Body = "A well-designed database is crucial for application performance. Learn the best practices for designing efficient database schemas.",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                UserId = 3,
                Title = "TypeScript Tips and Tricks",
                Body = "TypeScript adds static typing to JavaScript. Here are some tips and tricks to make the most of TypeScript in your projects.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProjectsAsync(AppDbContext context)
    {
        if (await context.Projects.AnyAsync()) return;

        var projects = new List<Project>
        {
            new()
            {
                Title = "Portfolio Website",
                Description = "A modern portfolio website built with Angular and .NET Core backend. Features include authentication, dark mode, and responsive design.",
                Technologies = new List<string> { "Angular", "TypeScript", ".NET Core", "PostgreSQL" },
                Rating = 5,
                GithubLink = "https://github.com/username/portfolio",
                LiveLink = "https://portfolio.example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Title = "Task Management App",
                Description = "A full-stack task management application with real-time updates, team collaboration features, and mobile support.",
                Technologies = new List<string> { "React", "Node.js", "MongoDB", "Socket.io" },
                Rating = 4,
                GithubLink = "https://github.com/username/task-app",
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new()
            {
                Title = "E-Commerce Platform",
                Description = "A scalable e-commerce platform with shopping cart, payment integration, and admin dashboard.",
                Technologies = new List<string> { "Vue.js", "C#", "SQL Server", "Stripe" },
                Rating = 5,
                GithubLink = "https://github.com/username/ecommerce",
                LiveLink = "https://shop.example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-90)
            },
            new()
            {
                Title = "Weather Dashboard",
                Description = "A weather dashboard that displays current conditions and forecasts using data from multiple weather APIs.",
                Technologies = new List<string> { "Angular", "Python", "FastAPI", "Redis" },
                Rating = 4,
                GithubLink = "https://github.com/username/weather",
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            }
        };

        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSkillsAsync(AppDbContext context)
    {
        if (await context.Skills.AnyAsync()) return;

        var skills = new List<Skill>
        {
            // Frontend
            new() { Category = "Frontend", Name = "Angular", Level = 90, Color = "#dd0031" },
            new() { Category = "Frontend", Name = "TypeScript", Level = 88, Color = "#3178c6" },
            new() { Category = "Frontend", Name = "HTML/CSS", Level = 95, Color = "#e34c26" },
            new() { Category = "Frontend", Name = "JavaScript", Level = 85, Color = "#f7df1e" },
            new() { Category = "Frontend", Name = "React", Level = 70, Color = "#61dafb" },

            // Backend
            new() { Category = "Backend", Name = "C#", Level = 85, Color = "#68217a" },
            new() { Category = "Backend", Name = "ASP.NET Core", Level = 82, Color = "#512bd4" },
            new() { Category = "Backend", Name = "Node.js", Level = 75, Color = "#339933" },
            new() { Category = "Backend", Name = "Python", Level = 70, Color = "#3776ab" },

            // Database
            new() { Category = "Database", Name = "PostgreSQL", Level = 80, Color = "#336791" },
            new() { Category = "Database", Name = "SQL Server", Level = 78, Color = "#cc2927" },
            new() { Category = "Database", Name = "MongoDB", Level = 72, Color = "#47a248" },

            // Tools
            new() { Category = "Tools", Name = "Git", Level = 88, Color = "#f05032" },
            new() { Category = "Tools", Name = "Docker", Level = 75, Color = "#2496ed" },
            new() { Category = "Tools", Name = "Azure", Level = 70, Color = "#0078d4" }
        };

        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTodosAsync(AppDbContext context)
    {
        if (await context.Todos.AnyAsync()) return;

        var todos = new List<Todo>
        {
            new() { UserId = 1, Title = "Complete project documentation", Completed = true, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { UserId = 1, Title = "Review pull requests", Completed = false, CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new() { UserId = 1, Title = "Update dependencies", Completed = false, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { UserId = 2, Title = "Write unit tests", Completed = true, CreatedAt = DateTime.UtcNow.AddDays(-4) },
            new() { UserId = 2, Title = "Fix bug in login page", Completed = false, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { UserId = 3, Title = "Design new feature", Completed = false, CreatedAt = DateTime.UtcNow }
        };

        context.Todos.AddRange(todos);
        await context.SaveChangesAsync();
    }
}
