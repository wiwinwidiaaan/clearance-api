using System.Security.Claims;
using ClearanceAPI.Data;
using ClearanceAPI.DTOs;
using ClearanceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClearanceAPI.Controllers;

[Authorize] // semua endpoint di sini wajib login (kirim Bearer token)
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db)
    {
        _db = db;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!;

    // POST /api/orders/checkout
    [HttpPost("checkout")]
    public async Task<ActionResult<OrderResponseDto>> Checkout(CheckoutRequestDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest(new { message = "Keranjang kosong." });

        // Pakai transaction: kalau ada 1 item gagal (stok habis), semua dibatalkan
        using var transaction = await _db.Database.BeginTransactionAsync();

        var order = new Order
        {
            UserId = CurrentUserId,
            ShippingAddress = dto.ShippingAddress,
            Status = OrderStatus.Pending
        };

        decimal total = 0;

        foreach (var item in dto.Items)
        {
            var product = await _db.Products
                .Include(p => p.Inventory)
                .Include(p => p.Discounts)
                .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.IsActive);

            if (product == null || product.Inventory == null)
                return BadRequest(new { message = $"Produk id {item.ProductId} tidak ditemukan." });

            if (product.Inventory.QuantityAvailable < item.Quantity)
                return BadRequest(new
                {
                    message = $"Stok '{product.Name}' tidak cukup. Sisa: {product.Inventory.QuantityAvailable}"
                });

            // Hitung harga final dengan diskon aktif (ambil diskon terbesar yang berlaku)
            var activeDiscount = product.Discounts
                .Where(d => d.IsCurrentlyActive)
                .OrderByDescending(d => d.Type == DiscountType.Percentage
                    ? product.OriginalPrice * (d.Value / 100)
                    : d.Value)
                .FirstOrDefault();

            decimal unitPrice = product.OriginalPrice;
            if (activeDiscount != null)
            {
                unitPrice = activeDiscount.Type == DiscountType.Percentage
                    ? product.OriginalPrice * (1 - activeDiscount.Value / 100)
                    : Math.Max(0, product.OriginalPrice - activeDiscount.Value);
            }

            // Kurangi stok
            product.Inventory.QuantityAvailable -= item.Quantity;
            product.Inventory.LastUpdated = DateTime.UtcNow;

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPriceAtPurchase = unitPrice
            };

            order.Items.Add(orderItem);
            total += unitPrice * item.Quantity;
        }

        order.TotalAmount = total;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        // Load nama produk untuk response
        var itemsWithNames = await _db.OrderItems
            .Where(oi => oi.OrderId == order.Id)
            .Include(oi => oi.Product)
            .Select(oi => new OrderItemResponseDto
            {
                ProductName = oi.Product!.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPriceAtPurchase
            }).ToListAsync();

        return Ok(new OrderResponseDto
        {
            OrderId = order.Id,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = itemsWithNames
        });
    }

    // GET /api/orders  -> riwayat order milik user yang login
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetMyOrders()
    {
        var orders = await _db.Orders
            .Where(o => o.UserId == CurrentUserId)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderResponseDto
            {
                OrderId = o.Id,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new OrderItemResponseDto
                {
                    ProductName = i.Product!.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPriceAtPurchase
                }).ToList()
            }).ToListAsync();

        return Ok(orders);
    }
}
