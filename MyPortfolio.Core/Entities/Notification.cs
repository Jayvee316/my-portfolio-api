namespace MyPortfolio.Core.Entities;

/// <summary>
/// User notification
/// </summary>
public class Notification
{
    public int Id { get; set; }
    public string Type { get; set; } = "system"; // order_status, price_drop, back_in_stock, review_reply, system
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; } // JSON data
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int UserId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
