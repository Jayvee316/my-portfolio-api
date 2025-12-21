using Microsoft.AspNetCore.Mvc;
using MyPortfolio.Core.DTOs;
using MyPortfolio.Core.Entities;
using MyPortfolio.Core.Interfaces;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Api.Controllers;

/// <summary>
/// Contact form controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(
        AppDbContext context,
        IEmailService emailService,
        ILogger<ContactController> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Submit basic contact form
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ContactResponse>> SubmitContact([FromBody] ContactRequest request)
    {
        var submission = new ContactSubmission
        {
            Name = request.Name,
            Email = request.Email,
            Subject = request.Subject,
            Message = request.Message,
            Category = "general",
            CreatedAt = DateTime.UtcNow,
            Status = "received"
        };

        _context.ContactSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        // Send email notification (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendContactNotificationAsync(
                    request.Name, request.Email, request.Subject, request.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact notification email");
            }
        });

        return Ok(new ContactResponse
        {
            Id = submission.Id,
            CreatedAt = submission.CreatedAt,
            Status = submission.Status,
            Message = "Thank you for your message! We'll get back to you soon."
        });
    }

    /// <summary>
    /// Submit advanced contact form
    /// </summary>
    [HttpPost("advanced")]
    public async Task<ActionResult<ContactResponse>> SubmitAdvancedContact([FromBody] ContactAdvancedRequest request)
    {
        var submission = new ContactSubmission
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Category = request.Category,
            Subject = request.Subject,
            Message = request.Message,
            Urgent = request.Urgent,
            Newsletter = request.Newsletter,
            CreatedAt = DateTime.UtcNow,
            Status = "received"
        };

        _context.ContactSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        // Send email notification (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var urgentPrefix = request.Urgent ? "[URGENT] " : "";
                await _emailService.SendContactNotificationAsync(
                    $"{request.Name} ({request.Company ?? "N/A"})",
                    request.Email,
                    $"{urgentPrefix}{request.Subject}",
                    $"Category: {request.Category}\nPhone: {request.Phone ?? "N/A"}\n\n{request.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send advanced contact notification email");
            }
        });

        return Ok(new ContactResponse
        {
            Id = submission.Id,
            CreatedAt = submission.CreatedAt,
            Status = submission.Status,
            Message = request.Urgent
                ? "Your urgent message has been received! We'll prioritize your request."
                : "Thank you for your message! We'll get back to you soon."
        });
    }
}
