namespace ClearanceAPI.Models;

public class Discount
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string Label { get; set; } = string.Empty; // contoh: "Flash Sale 24 Jam"
    public DiscountType Type { get; set; } = DiscountType.Percentage;
    public decimal Value { get; set; } // 20 (%) atau 50000 (nominal)

    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    public bool IsFlashSale { get; set; } = false; // dipakai trigger push notification di mobile

    public bool IsCurrentlyActive =>
        DateTime.UtcNow >= StartsAt && DateTime.UtcNow <= EndsAt;
}

public enum DiscountType
{
    Percentage,
    FixedAmount
}
