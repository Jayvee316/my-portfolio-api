namespace MyPortfolio.Core.DTOs.Ecommerce;

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public int TotalItems { get; set; }
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public int StockQuantity { get; set; }
}

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}
