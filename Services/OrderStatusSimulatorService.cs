using ClearanceAPI.Data;
using ClearanceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClearanceAPI.Services;

/// <summary>
/// Simulasi progres pengiriman untuk keperluan demo/portfolio — belum ada
/// integrasi kurir/payment gateway beneran, jadi status pesanan dinaikkan
/// otomatis berdasarkan berapa lama waktu berlalu sejak order dibuat:
///
///   0    - 30 detik   : Pending
///   30   - 90 detik   : Processing
///   90   - 180 detik  : Shipped
///   180+ detik        : Delivered
///
/// Status Cancelled tidak pernah disentuh oleh service ini (biar kalau ada
/// fitur pembatalan pesanan nanti, statusnya tidak "dihidupkan" lagi).
/// </summary>
public class OrderStatusSimulatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderStatusSimulatorService> _logger;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10);

    // Ubah angka-angka ini kalau mau progresnya lebih cepat/lambat pas demo.
    private static readonly TimeSpan ProcessingAfter = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ShippedAfter = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan DeliveredAfter = TimeSpan.FromSeconds(180);

    public OrderStatusSimulatorService(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderStatusSimulatorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Delay kecil di awal biar tidak rebutan resource pas aplikasi baru start.
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AdvanceOrderStatusesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Jangan sampai loop background service mati gara-gara 1 error;
                // cukup dicatat, lanjut cek lagi di interval berikutnya.
                _logger.LogError(ex, "Gagal menjalankan simulasi status pesanan.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task AdvanceOrderStatusesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var activeStatuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.Processing,
            OrderStatus.Shipped
        };

        var candidates = await db.Orders
            .Where(o => activeStatuses.Contains(o.Status))
            .ToListAsync(stoppingToken);

        if (candidates.Count == 0) return;

        var now = DateTime.UtcNow;
        var changed = 0;

        foreach (var order in candidates)
        {
            var elapsed = now - order.CreatedAt;
            var targetStatus = ResolveStatus(elapsed);

            // Cuma boleh maju, tidak pernah mundur.
            if (targetStatus > order.Status)
            {
                order.Status = targetStatus;
                changed++;
            }
        }

        if (changed > 0)
        {
            await db.SaveChangesAsync(stoppingToken);
            _logger.LogInformation(
                "Simulasi status pesanan: {Count} order diperbarui.", changed);
        }
    }

    private static OrderStatus ResolveStatus(TimeSpan elapsed)
    {
        if (elapsed >= DeliveredAfter) return OrderStatus.Delivered;
        if (elapsed >= ShippedAfter) return OrderStatus.Shipped;
        if (elapsed >= ProcessingAfter) return OrderStatus.Processing;
        return OrderStatus.Pending;
    }
}