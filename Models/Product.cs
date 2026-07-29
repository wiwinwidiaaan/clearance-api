namespace ClearanceAPI.Models;

// Produk yang dijual di platform clearance sale (barang surplus/retur/overstock)
public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    // Kondisi barang khas clearance sale
    public ProductCondition Condition { get; set; } = ProductCondition.Overstock;

    public decimal OriginalPrice { get; set; }
    public decimal CurrentPrice { get; set; } // harga setelah diskon aktif dihitung

    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relasi
    public Inventory? Inventory { get; set; }
    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
}

public enum ProductCondition
{
    New,
    Overstock,
    Returned,
    Refurbished,
    Damaged
}
