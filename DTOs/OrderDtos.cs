namespace ClearanceAPI.DTOs;

public class CartItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CheckoutRequestDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public string ShippingAddress { get; set; } = string.Empty;
}

public class OrderResponseDto
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
}

public class OrderItemResponseDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}
