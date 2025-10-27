using System;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ERP.Infrastructure.Services
{
    public class ReservationExpireService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(15); // 1 dk'da bir kontrol et

        public ReservationExpireService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.UtcNow;

                    // Pending rezervasyonlardan süresi dolanları çek
                    var expiredReservations = await db.Reservations
                        .Include(r => r.Items)
                            .ThenInclude(ri => ri.Product)
                        .Where(r => r.Status == ReservationStatus.Pending && r.ExpireAt <= now)
                        .ToListAsync(stoppingToken);

                    foreach (var reservation in expiredReservations)
                    {
                        foreach (var item in reservation.Items)
                        {
                            if (item.Product != null)
                            {
                                // ✅ ReservedStock geri al
                                item.Product.ReservedStock -= item.Quantity;
                                if (item.Product.ReservedStock < 0)
                                    item.Product.ReservedStock = 0;
                            }
                        }

                        // ✅ Status'u Expired yap
                        reservation.Status = ReservationStatus.Expired;
                    }

                    if (expiredReservations.Any())
                        await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ReservationExpireService Error: {ex.Message}");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}










