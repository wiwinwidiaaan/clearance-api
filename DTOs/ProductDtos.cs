using ClearanceAPI.Models;

namespace ClearanceAPI.DTOs;

public class ProductListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ProductCondition Condition { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int StockAvailable { get; set; }
    public bool HasActiveFlashSale { get; set; }
}

public class ProductDetailDto : ProductListItemDto
{
    public string Sku { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DiscountDto> ActiveDiscounts { get; set; } = new();
}

public class DiscountDto
{
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsFlashSale { get; set; }
}

public class CreateProductDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ProductCondition Condition { get; set; }
    public decimal OriginalPrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int InitialStock { get; set; }
}
