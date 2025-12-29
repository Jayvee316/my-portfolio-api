namespace MyPortfolio.Core.Entities;

/// <summary>
/// User wishlist item
/// </summary>
public class WishlistItem
{
    public int Id { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int UserId { get; set; }
    public int ProductId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
