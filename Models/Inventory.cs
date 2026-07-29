namespace ClearanceAPI.Models;

public class Inventory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; } // dipesan tapi belum checkout selesai
    public int ReorderThreshold { get; set; } = 5; // untuk trigger flash-sale notif saat stok menipis

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
