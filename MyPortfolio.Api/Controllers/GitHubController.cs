using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyPortfolio.Api.Controllers;

[ApiController]
[Route("api")]
public class GitHubController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _githubUsername;

    public GitHubController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyPortfolio-API");
        _configuration = configuration;
        _githubUsername = Environment.GetEnvironmentVariable("GITHUB_USERNAME")
            ?? _configuration["GitHub:Username"]
            ?? "Jayvee316"; // Default username
    }

    /// <summary>
    /// Get GitHub profile information
    /// </summary>
    [HttpGet("github-profile")]
    public async Task<IActionResult> GetGitHubProfile()
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://api.github.com/users/{_githubUsername}");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { error = "Failed to fetch GitHub profile" });
            }

            var json = await response.Content.ReadAsStringAsync();
            var githubUser = JsonSerializer.Deserialize<GitHubUser>(json);

            if (githubUser == null)
            {
                return NotFound(new { error = "GitHub user not found" });
            }

            var profile = new GitHubProfileDto
            {
                Login = githubUser.Login ?? "",
                Name = githubUser.Name ?? githubUser.Login ?? "",
                AvatarUrl = githubUser.AvatarUrl ?? "",
                Bio = githubUser.Bio ?? "",
                PublicRepos = githubUser.PublicRepos,
                Followers = githubUser.Followers,
                Following = githubUser.Following
            };

            return Ok(profile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to fetch GitHub profile", details = ex.Message });
        }
    }

    /// <summary>
    /// Get GitHub repositories
    /// </summary>
    [HttpGet("github-repos")]
    public async Task<IActionResult> GetGitHubRepos()
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"https://api.github.com/users/{_githubUsername}/repos?sort=updated&per_page=10"
            );

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, new { error = "Failed to fetch GitHub repos" });
            }

            var json = await response.Content.ReadAsStringAsync();
            var githubRepos = JsonSerializer.Deserialize<List<GitHubRepoResponse>>(json);

            if (githubRepos == null)
            {
                return Ok(new List<GitHubRepoDto>());
            }

            var repos = githubRepos.Select(r => new GitHubRepoDto
            {
                Id = r.Id,
                Name = r.Name ?? "",
                Description = r.Description ?? "",
                HtmlUrl = r.HtmlUrl ?? "",
                Language = r.Language ?? "",
                StargazersCount = r.StargazersCount,
                ForksCount = r.ForksCount,
                UpdatedAt = r.UpdatedAt?.ToString("o") ?? ""
            }).ToList();

            return Ok(repos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to fetch GitHub repos", details = ex.Message });
        }
    }
}

// DTOs for API response
public class GitHubProfileDto
{
    public string Login { get; set; } = "";
    public string Name { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public string Bio { get; set; } = "";
    public int PublicRepos { get; set; }
    public int Followers { get; set; }
    public int Following { get; set; }
}

public class GitHubRepoDto
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string HtmlUrl { get; set; } = "";
    public string Language { get; set; } = "";
    public int StargazersCount { get; set; }
    public int ForksCount { get; set; }
    public string UpdatedAt { get; set; } = "";
}

// GitHub API response models
public class GitHubUser
{
    [JsonPropertyName("login")]
    public string? Login { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("bio")]
    public string? Bio { get; set; }

    [JsonPropertyName("public_repos")]
    public int PublicRepos { get; set; }

    [JsonPropertyName("followers")]
    public int Followers { get; set; }

    [JsonPropertyName("following")]
    public int Following { get; set; }
}

public class GitHubRepoResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("stargazers_count")]
    public int StargazersCount { get; set; }

    [JsonPropertyName("forks_count")]
    public int ForksCount { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
