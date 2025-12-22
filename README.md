# MyPortfolio API - ASP.NET Core Backend

A complete ASP.NET Core Web API with JWT Authentication, PostgreSQL database, and deployment guide.

## Table of Contents

1. [Project Overview](#project-overview)
2. [Prerequisites](#prerequisites)
3. [Project Setup from Scratch](#project-setup-from-scratch)
4. [Project Structure](#project-structure)
5. [NuGet Packages](#nuget-packages)
6. [Database Setup](#database-setup)
7. [JWT Authentication Setup](#jwt-authentication-setup)
8. [CORS Configuration](#cors-configuration)
9. [Running Locally](#running-locally)
10. [Deployment Guide](#deployment-guide)
11. [API Endpoints](#api-endpoints)
12. [Environment Variables](#environment-variables)
13. [Troubleshooting](#troubleshooting)

---

## Project Overview

This is a 3-layer architecture ASP.NET Core Web API:

```
┌─────────────────────────────────────────────────────────────┐
│                    MyPortfolio.Api                          │
│              (Controllers, Program.cs)                      │
│                   Handles HTTP requests                     │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   MyPortfolio.Core                          │
│           (Entities, DTOs, Interfaces)                      │
│              Business logic contracts                       │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                MyPortfolio.Infrastructure                   │
│        (DbContext, Repositories, Services)                  │
│              Data access & implementations                  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                     PostgreSQL                              │
│                      Database                               │
└─────────────────────────────────────────────────────────────┘
```

**Features:**
- JWT Authentication (Login, Register)
- PostgreSQL database with Entity Framework Core
- BCrypt password hashing
- CORS for Angular frontend
- Swagger API documentation
- Docker support for deployment

---

## Prerequisites

Before starting, ensure you have:

- [.NET 9 SDK](https://dotnet.microsoft.com/download) (or .NET 8+)
- [PostgreSQL](https://www.postgresql.org/download/) (local) or [Neon](https://neon.tech) (cloud)
- [Git](https://git-scm.com/)
- Code editor (VS Code, Visual Studio, or Rider)

Verify .NET installation:
```bash
dotnet --version
# Should output: 9.0.x or 8.0.x
```

---

## Project Setup from Scratch

### Step 1: Create Solution and Projects

```bash
# Create project directory
mkdir my-portfolio-api
cd my-portfolio-api

# Create solution file
dotnet new sln -n MyPortfolio.Api

# Create the 3 projects
dotnet new webapi -n MyPortfolio.Api -controllers
dotnet new classlib -n MyPortfolio.Core
dotnet new classlib -n MyPortfolio.Infrastructure

# Add projects to solution
dotnet sln add MyPortfolio.Api/MyPortfolio.Api.csproj
dotnet sln add MyPortfolio.Core/MyPortfolio.Core.csproj
dotnet sln add MyPortfolio.Infrastructure/MyPortfolio.Infrastructure.csproj

# Set up project references
cd MyPortfolio.Api
dotnet add reference ../MyPortfolio.Core/MyPortfolio.Core.csproj
dotnet add reference ../MyPortfolio.Infrastructure/MyPortfolio.Infrastructure.csproj
cd ../MyPortfolio.Infrastructure
dotnet add reference ../MyPortfolio.Core/MyPortfolio.Core.csproj
cd ..
```

### Step 2: Install NuGet Packages

```bash
# MyPortfolio.Api packages
cd MyPortfolio.Api
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.1
dotnet add package Swashbuckle.AspNetCore --version 7.2.0

# MyPortfolio.Infrastructure packages
cd ../MyPortfolio.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore --version 9.0.1
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.1
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.1
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package MailKit --version 4.8.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.0.1

cd ..
```

### Step 3: Install EF Core Tools (Local to Project)

```bash
# Create tool manifest
dotnet new tool-manifest

# Install EF Core tools locally
dotnet tool install dotnet-ef --version 9.0.0

# Verify installation
dotnet ef --version
```

---

## Project Structure

```
my-portfolio-api/
├── MyPortfolio.Api/
│   ├── Controllers/
│   │   ├── AuthController.cs      # Login, Register, GetCurrentUser
│   │   ├── PostsController.cs     # Blog posts CRUD
│   │   ├── ContactController.cs   # Contact form submissions
│   │   ├── ProjectsController.cs  # Portfolio projects
│   │   ├── SkillsController.cs    # Technical skills
│   │   ├── TodosController.cs     # Todo items
│   │   └── UsersController.cs     # User management
│   ├── Program.cs                 # App configuration & startup
│   ├── appsettings.json           # Configuration (JWT, DB, etc.)
│   └── MyPortfolio.Api.csproj
│
├── MyPortfolio.Core/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Post.cs
│   │   ├── Project.cs
│   │   ├── Skill.cs
│   │   ├── Todo.cs
│   │   └── ContactSubmission.cs
│   ├── DTOs/
│   │   ├── Auth/
│   │   │   ├── LoginRequest.cs
│   │   │   ├── LoginResponse.cs
│   │   │   └── RegisterRequest.cs
│   │   ├── PostDto.cs
│   │   ├── ProjectDto.cs
│   │   └── TodoDto.cs
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IEmailService.cs
│   │   └── IRepository.cs
│   └── MyPortfolio.Core.csproj
│
├── MyPortfolio.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs        # EF Core database context
│   │   ├── DataSeeder.cs          # Initial data seeding
│   │   └── Migrations/            # EF Core migrations
│   ├── Repositories/
│   │   └── Repository.cs          # Generic repository
│   ├── Services/
│   │   ├── AuthService.cs         # JWT authentication logic
│   │   └── EmailService.cs        # Email sending (MailKit)
│   └── MyPortfolio.Infrastructure.csproj
│
├── Dockerfile                     # Docker configuration for deployment
├── .gitignore
└── MyPortfolio.Api.sln
```

---

## NuGet Packages

| Package | Version | Project | Purpose |
|---------|---------|---------|---------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.1 | Api | JWT token validation middleware |
| Swashbuckle.AspNetCore | 7.2.0 | Api | Swagger/OpenAPI documentation |
| Microsoft.EntityFrameworkCore | 9.0.1 | Infrastructure | ORM for database access |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.1 | Infrastructure | PostgreSQL provider for EF Core |
| Microsoft.EntityFrameworkCore.Design | 9.0.1 | Infrastructure | EF Core migrations support |
| BCrypt.Net-Next | 4.0.3 | Infrastructure | Password hashing |
| MailKit | 4.8.0 | Infrastructure | SMTP email sending |
| System.IdentityModel.Tokens.Jwt | 8.0.1 | Infrastructure | JWT token generation |

---

## Database Setup

### Option A: Local PostgreSQL

1. Install PostgreSQL from https://www.postgresql.org/download/
2. Create a database:
   ```sql
   CREATE DATABASE portfolio_db;
   ```
3. Update `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=5432;Database=portfolio_db;User Id=postgres;Password=YOUR_PASSWORD"
     }
   }
   ```

### Option B: Neon (Cloud PostgreSQL - FREE)

1. Sign up at https://neon.tech
2. Create a new project:
   - Project name: `my-portfolio-api`
   - Database name: `portfolio_db`
   - Region: Choose closest to you
   - PostgreSQL version: 16
3. Copy the connection string from the dashboard
4. For deployment, use the connection string as `DATABASE_URL` environment variable

### Create Database Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate --project MyPortfolio.Infrastructure --startup-project MyPortfolio.Api

# Apply migration (optional - app does this on startup)
dotnet ef database update --project MyPortfolio.Infrastructure --startup-project MyPortfolio.Api
```

---

## JWT Authentication Setup

### Configuration (appsettings.json)

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyHere_AtLeast32CharactersLong!",
    "Issuer": "MyPortfolioApi",
    "Audience": "MyPortfolioApp",
    "ExpirationMinutes": 60
  }
}
```

### How JWT Works in This Project

```
┌──────────────────────────────────────────────────────────────────────────┐
│                         JWT AUTHENTICATION FLOW                          │
└──────────────────────────────────────────────────────────────────────────┘

1. LOGIN REQUEST
   ┌─────────────┐         POST /api/auth/login           ┌─────────────┐
   │   Angular   │  ──────────────────────────────────►   │   ASP.NET   │
   │   Frontend  │  { "username": "x", "password": "y" }  │   Backend   │
   └─────────────┘                                        └─────────────┘

2. SERVER VALIDATES & GENERATES TOKEN
   ┌─────────────┐                                        ┌─────────────┐
   │   ASP.NET   │  1. Find user in database              │  PostgreSQL │
   │   Backend   │  2. Verify password (BCrypt)           │   Database  │
   │             │  3. Generate JWT token                 │             │
   └─────────────┘                                        └─────────────┘

3. TOKEN RETURNED TO CLIENT
   ┌─────────────┐         { token: "eyJ...", user: {...} }    ┌─────────────┐
   │   Angular   │  ◄──────────────────────────────────────   │   ASP.NET   │
   │   Frontend  │                                             │   Backend   │
   └─────────────┘                                             └─────────────┘

4. CLIENT STORES TOKEN (localStorage)

5. SUBSEQUENT REQUESTS WITH TOKEN
   ┌─────────────┐         GET /api/posts                 ┌─────────────┐
   │   Angular   │  ──────────────────────────────────►   │   ASP.NET   │
   │   Frontend  │  Header: "Authorization: Bearer eyJ.." │   Backend   │
   └─────────────┘                                        └─────────────┘

6. SERVER VALIDATES TOKEN & PROCESSES REQUEST
   - Verifies signature (not tampered)
   - Checks expiration
   - Extracts user claims (id, role, etc.)
   - Processes request if valid
```

### JWT Token Structure

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
|_____________HEADER______________|._______PAYLOAD_______|.__________SIGNATURE__________|

HEADER:  { "alg": "HS256", "typ": "JWT" }
PAYLOAD: { "sub": "1", "name": "Admin", "role": "admin", "exp": 1699999999 }
SIGNATURE: HMACSHA256(base64(header) + "." + base64(payload), secret_key)
```

---

## CORS Configuration

CORS (Cross-Origin Resource Sharing) allows your Angular frontend to call the API from a different domain.

```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",           // Local Angular
                "https://your-app.vercel.app"      // Production Angular
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Apply CORS middleware (before authentication!)
app.UseCors("AllowAngular");
```

---

## Running Locally

### Step 1: Configure Database Connection

Update `MyPortfolio.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=portfolio_db;User Id=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyHere_AtLeast32CharactersLong_ChangeThis!",
    "Issuer": "MyPortfolioApi",
    "Audience": "MyPortfolioApp",
    "ExpirationMinutes": 60
  }
}
```

### Step 2: Run the API

```bash
cd MyPortfolio.Api
dotnet run
```

### Step 3: Access the API

- **Swagger UI:** http://localhost:5022/swagger
- **API Base URL:** http://localhost:5022/api

### Default Test Users (Seeded Automatically)

| Email | Password | Role |
|-------|----------|------|
| admin@example.com | admin123 | admin |
| john@example.com | password123 | user |
| jane@example.com | password123 | user |

---

## Deployment Guide

### Hosting Options (Free Tier)

| Service | Purpose | Free Tier |
|---------|---------|-----------|
| **Render** | API Hosting | Free (sleeps after 15 min inactivity) |
| **Neon** | PostgreSQL Database | Free forever (0.5 GB storage) |
| **Vercel** | Angular Frontend | Free (auto-deploys from GitHub) |

### Step 1: Set Up Neon Database

1. Go to https://neon.tech and sign up
2. Create a new project:
   - Project name: `my-portfolio-api`
   - Database name: `portfolio_db`
   - PostgreSQL version: 16
   - Region: Choose closest to your users
3. Copy the connection string (looks like):
   ```
   postgresql://user:password@ep-xxx.region.neon.tech/portfolio_db?sslmode=require
   ```

### Step 2: Create Dockerfile

Create `Dockerfile` in the root directory:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY MyPortfolio.Api.sln ./
COPY MyPortfolio.Api/MyPortfolio.Api.csproj MyPortfolio.Api/
COPY MyPortfolio.Core/MyPortfolio.Core.csproj MyPortfolio.Core/
COPY MyPortfolio.Infrastructure/MyPortfolio.Infrastructure.csproj MyPortfolio.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src/MyPortfolio.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port (Render uses PORT env variable)
EXPOSE 8080

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MyPortfolio.Api.dll"]
```

### Step 3: Push to GitHub

```bash
git init
git add .
git commit -m "Initial commit"
git remote add origin https://github.com/YOUR_USERNAME/my-portfolio-api.git
git push -u origin main
```

### Step 4: Deploy to Render

1. Go to https://render.com and sign up with GitHub
2. Click **"New +"** → **"Web Service"**
3. Connect your `my-portfolio-api` repository
4. Configure:
   - **Name:** `my-portfolio-api`
   - **Region:** Singapore (or closest to your Neon DB)
   - **Branch:** `main`
   - **Runtime:** `Docker`
   - **Instance Type:** `Free`

5. Add **Environment Variables**:

   | Key | Value |
   |-----|-------|
   | `DATABASE_URL` | `postgresql://user:pass@ep-xxx.neon.tech/portfolio_db?sslmode=require` |
   | `JWT_KEY` | `YourSecureRandomKey_AtLeast32Characters_GenerateNew!` |
   | `ALLOWED_ORIGINS` | `https://your-angular-app.vercel.app,http://localhost:4200` |

6. Click **"Create Web Service"**

### Step 5: Update Angular Frontend

Update `src/environments/environment.production.ts`:
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-api-name.onrender.com/api',
  // ... other config
};
```

---

## API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/login` | Login and get JWT token | No |
| GET | `/api/auth/me` | Get current user info | Yes |

### Posts (Blog)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/posts` | Get all posts | No |
| GET | `/api/posts/{id}` | Get post by ID | No |
| POST | `/api/posts` | Create new post | Yes |
| PUT | `/api/posts/{id}` | Update post | Yes |
| DELETE | `/api/posts/{id}` | Delete post | Yes |

### Contact

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/contact` | Submit basic contact form | No |
| POST | `/api/contact/advanced` | Submit detailed contact form | No |
| GET | `/api/contact` | Get all submissions | Yes (Admin) |

### Projects

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/projects` | Get all projects | No |
| GET | `/api/projects/{id}` | Get project by ID | No |
| POST | `/api/projects` | Create project | Yes |
| PUT | `/api/projects/{id}` | Update project | Yes |
| DELETE | `/api/projects/{id}` | Delete project | Yes |

### Skills

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/skills` | Get all skills | No |
| GET | `/api/skills/{id}` | Get skill by ID | No |
| POST | `/api/skills` | Create skill | Yes |
| PUT | `/api/skills/{id}` | Update skill | Yes |
| DELETE | `/api/skills/{id}` | Delete skill | Yes |

---

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `DATABASE_URL` | PostgreSQL connection string (Neon format) | `postgresql://user:pass@host/db` |
| `JWT_KEY` | Secret key for signing JWT tokens (min 32 chars) | `kmLJL381p2+KFt7Y...` |
| `ALLOWED_ORIGINS` | Comma-separated list of allowed CORS origins | `https://app.vercel.app,http://localhost:4200` |

### Generate Secure JWT Key

```bash
# Using OpenSSL
openssl rand -base64 48

# Using PowerShell
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }) -as [byte[]])
```

---

## Troubleshooting

### Common Issues

**1. CORS Error in Browser**
```
Access to XMLHttpRequest has been blocked by CORS policy
```
**Solution:** Add your frontend URL to `ALLOWED_ORIGINS` environment variable.

---

**2. 401 Unauthorized on Protected Endpoints**
**Solution:**
- Ensure you're including the `Authorization: Bearer {token}` header
- Check if token is expired (default: 60 minutes)
- Verify JWT_KEY matches in both creation and validation

---

**3. Database Connection Failed**
```
Npgsql.NpgsqlException: Failed to connect
```
**Solution:**
- Verify PostgreSQL is running
- Check connection string format
- For Neon: Ensure `?sslmode=require` is included

---

**4. Build Fails with "File is locked"**
```
error MSB3027: Could not copy ... The file is locked
```
**Solution:** Stop the running `dotnet run` process before building.

---

**5. Render Deploy Fails**
**Solution:**
- Check Render logs for specific error
- Verify Dockerfile exists in root
- Ensure all environment variables are set
- Check if DATABASE_URL format is correct

---

## Useful Commands

```bash
# Build project
dotnet build

# Run project
dotnet run --project MyPortfolio.Api

# Add new migration
dotnet ef migrations add MigrationName --project MyPortfolio.Infrastructure --startup-project MyPortfolio.Api

# Apply migrations
dotnet ef database update --project MyPortfolio.Infrastructure --startup-project MyPortfolio.Api

# Remove last migration
dotnet ef migrations remove --project MyPortfolio.Infrastructure --startup-project MyPortfolio.Api

# Watch mode (auto-restart on changes)
dotnet watch run --project MyPortfolio.Api
```

---

## Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT.io - Token Debugger](https://jwt.io/)
- [Neon Documentation](https://neon.tech/docs)
- [Render Documentation](https://render.com/docs)

---

## License

This project is for learning purposes. Feel free to use and modify.

---

*Generated with [Claude Code](https://claude.com/claude-code)*
