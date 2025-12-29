namespace MyPortfolio.Core.Entities;

/// <summary>
/// Individual item within an order
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Snapshot of product info at time of order (in case product changes later)
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }

    // Foreign keys
    public int OrderId { get; set; }
    public int ProductId { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
