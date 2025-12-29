namespace MyPortfolio.Core.Entities;

/// <summary>
/// Shopping cart item
/// </summary>
public class CartItem
{
    public int Id { get; set; }
    public int Quantity { get; set; } = 1;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Foreign keys
    public int UserId { get; set; }
    public int ProductId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
