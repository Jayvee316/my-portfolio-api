namespace MyPortfolio.Core.Entities;

/// <summary>
/// Product review from a customer
/// </summary>
public class Review
{
    public int Id { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Foreign keys
    public int ProductId { get; set; }
    public int UserId { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
}
