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
        await SeedCategoriesAsync(context);
        await SeedProductsAsync(context);
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

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            new()
            {
                Name = "Electronics",
                Description = "Gadgets, devices, and electronic accessories",
                ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661?w=400",
                IsActive = true
            },
            new()
            {
                Name = "Clothing",
                Description = "Fashion apparel for men and women",
                ImageUrl = "https://images.unsplash.com/photo-1445205170230-053b83016050?w=400",
                IsActive = true
            },
            new()
            {
                Name = "Home & Garden",
                Description = "Furniture, decor, and garden essentials",
                ImageUrl = "https://images.unsplash.com/photo-1484101403633-562f891dc89a?w=400",
                IsActive = true
            },
            new()
            {
                Name = "Sports",
                Description = "Sports equipment and fitness gear",
                ImageUrl = "https://images.unsplash.com/photo-1461896836934- voices-of-the-earth?w=400",
                IsActive = true
            },
            new()
            {
                Name = "Books",
                Description = "Books, e-books, and educational materials",
                ImageUrl = "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=400",
                IsActive = true
            }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(AppDbContext context)
    {
        if (await context.Products.AnyAsync()) return;

        var products = new List<Product>
        {
            // Electronics
            new()
            {
                Name = "Wireless Bluetooth Headphones",
                Description = "Premium noise-canceling wireless headphones with 30-hour battery life and superior sound quality.",
                Price = 199.99m,
                SalePrice = 149.99m,
                StockQuantity = 50,
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400",
                Images = new List<string> { "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400" },
                SKU = "ELEC-001",
                IsActive = true,
                IsFeatured = true,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Smart Watch Pro",
                Description = "Advanced smartwatch with health monitoring, GPS, and 7-day battery life.",
                Price = 349.99m,
                StockQuantity = 30,
                ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400",
                Images = new List<string> { "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400" },
                SKU = "ELEC-002",
                IsActive = true,
                IsFeatured = true,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Portable Bluetooth Speaker",
                Description = "Waterproof portable speaker with 360-degree sound and 12-hour playtime.",
                Price = 79.99m,
                SalePrice = 59.99m,
                StockQuantity = 100,
                ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400",
                Images = new List<string> { "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400" },
                SKU = "ELEC-003",
                IsActive = true,
                IsFeatured = false,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            },
            // Clothing
            new()
            {
                Name = "Classic Denim Jacket",
                Description = "Timeless denim jacket with modern fit. Perfect for casual occasions.",
                Price = 89.99m,
                StockQuantity = 45,
                ImageUrl = "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=400",
                Images = new List<string> { "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=400" },
                SKU = "CLTH-001",
                IsActive = true,
                IsFeatured = true,
                CategoryId = 2,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Premium Cotton T-Shirt",
                Description = "Soft, breathable 100% organic cotton t-shirt available in multiple colors.",
                Price = 29.99m,
                SalePrice = 24.99m,
                StockQuantity = 200,
                ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400",
                Images = new List<string> { "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400" },
                SKU = "CLTH-002",
                IsActive = true,
                IsFeatured = false,
                CategoryId = 2,
                CreatedAt = DateTime.UtcNow
            },
            // Home & Garden
            new()
            {
                Name = "Modern Table Lamp",
                Description = "Elegant minimalist table lamp with adjustable brightness and warm lighting.",
                Price = 69.99m,
                StockQuantity = 35,
                ImageUrl = "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=400",
                Images = new List<string> { "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=400" },
                SKU = "HOME-001",
                IsActive = true,
                IsFeatured = true,
                CategoryId = 3,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Indoor Plant Set",
                Description = "Set of 3 low-maintenance indoor plants with decorative pots.",
                Price = 49.99m,
                SalePrice = 39.99m,
                StockQuantity = 25,
                ImageUrl = "https://images.unsplash.com/photo-1459411552884-841db9b3cc2a?w=400",
                Images = new List<string> { "https://images.unsplash.com/photo-1459411552884-841db9b3cc2a?w=400" },
                SKU = "HOME-002",
                IsActive = true,
                IsFeatured = false,
                CategoryId = 3,
                CreatedAt = DateTime.UtcNow
            },
            // Sports
            new()
            {
                Name = "Yoga Mat Premium",
                Description = "Extra thick non-slip yoga mat with carrying strap. Perfect for all yoga styles.",
                Price = 45.99m,
                StockQuantity = 60,
                ImageUrl = "https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=400",
                Images = new List<string> { "https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=400" },
                SKU = "SPRT-001",
                IsActive = true,
                IsFeatured = true,
                CategoryId = 4,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Resistance Bands Set",
                Description = "Complete set of 5 resistance bands with different strengths for home workouts.",
                Price = 24.99m,
                StockQuantity = 80,
                ImageUrl = "https://images.unsplash.com/photo-1598289431512-b97b0917affc?w=400",
                Images = new List<string> { "https://images.unsplash.com/photo-1598289431512-b97b0917affc?w=400" },
                SKU = "SPRT-002",
                IsActive = true,
                IsFeatured = false,
                CategoryId = 4,
                CreatedAt = DateTime.UtcNow
            },
            // Books
            new()
            {
                Name = "The Art of Programming",
                Description = "Comprehensive guide to modern programming practices and design patterns.",
                Price = 39.99m,
                SalePrice = 29.99m,
                StockQuantity = 40,
                ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400",
                Images = new List<string> { "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400" },
                SKU = "BOOK-001",
                IsActive = true,
                IsFeatured = true,
                CategoryId = 5,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Business Strategy Essentials",
                Description = "Learn the fundamentals of business strategy from industry experts.",
                Price = 34.99m,
                StockQuantity = 55,
                ImageUrl = "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400",
                Images = new List<string> { "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400" },
                SKU = "BOOK-002",
                IsActive = true,
                IsFeatured = false,
                CategoryId = 5,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}
