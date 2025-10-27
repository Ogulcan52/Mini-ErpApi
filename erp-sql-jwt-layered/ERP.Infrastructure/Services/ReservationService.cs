using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services
{
    public class ReservationService : IReservationService
    {
        
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public ReservationService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // Rezervasyon oluşturma
        public async Task<int> CreateReservationAsync(int orderId, CreateReservationRequestDto dto)
        {
            var order = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException("Sipariş bulunamadı.");

            if (order.Status != "Pending")
                throw new InvalidOperationException("Sadece Pending siparişler rezerve edilebilir.");

            if (dto.ItemQuantities == null || !dto.ItemQuantities.Any())
                throw new InvalidOperationException("Rezervasyon için ürün ve adet belirtilmelidir.");

            if (!TimeSpan.TryParse(dto.Duration, out TimeSpan duration))
                throw new InvalidOperationException("Geçersiz duration formatı.");

            var reservation = new Reservation
            {
                OrderId = order.Id,
                CustomerId = dto.CustomerId,
                ReservedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.Add(duration),
                Status = ReservationStatus.Pending,
                Items = new List<ReservationItem>()
            };

            foreach (var item in order.Items)
            {
                var key = dto.ItemQuantities.Keys
                    .FirstOrDefault(k => string.Equals(k, item.Product!.Name, StringComparison.OrdinalIgnoreCase));

                if (key == null) continue;

                int qty = dto.ItemQuantities[key];
                if (qty <= 0) throw new InvalidOperationException($"'{item.Product.Name}' için geçersiz rezervasyon adeti.");

                // Diğer aktif rezervasyonlardan toplam rezerve miktarı
                int totalReserved = await _db.ReservationItems
                    .Where(ri => ri.ProductId == item.ProductId && ri.Reservation.ExpireAt > DateTime.UtcNow)
                    .SumAsync(ri => ri.Quantity);

                int available = item.Quantity - totalReserved;
                if (qty > available)
                    throw new InvalidOperationException($"'{item.Product.Name}' için maksimum rezervasyon {available} olabilir.");

                item.Product.ReservedStock += qty;

                reservation.Items.Add(new ReservationItem
                {
                    ProductId = item.ProductId,
                    Quantity = qty
                });
            }

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            return reservation.Id;
        }






        // Tüm rezervasyonları listele
        public async Task<IEnumerable<ReservationReadDto>> GetAllReservationsAsync()
        {
            var reservations = await _db.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Items).ThenInclude(i => i.Product)
                .ToListAsync();

            return reservations.Select(r => _mapper.Map<ReservationReadDto>(r));
        }

        // ID ile rezervasyon getir
        public async Task<ReservationReadDto?> GetReservationByIdAsync(int id)
        {
            var reservation = await _db.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            return reservation == null ? null : _mapper.Map<ReservationReadDto>(reservation);
        }

        // Rezervasyon güncelle
        public async Task UpdateReservationAsync(int id, UpdateReservationRequestDto dto)
        {
            var reservation = await _db.Reservations
                .Include(r => r.Items).ThenInclude(i => i.Product)
                .Include(r => r.Order).ThenInclude(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                throw new KeyNotFoundException("Rezervasyon bulunamadı.");

            if (reservation.Status == ReservationStatus.Completed)
                throw new InvalidOperationException("Tamamlanan rezervasyon güncellenemez.");

            if (reservation.Status == ReservationStatus.Expired || DateTime.UtcNow > reservation.ExpireAt)
                throw new InvalidOperationException("Süresi dolmuş rezervasyon güncellenemez.");

            if (!TimeSpan.TryParse(dto.Duration, out TimeSpan duration))
                throw new InvalidOperationException("Geçersiz duration formatı.");

            reservation.ExpireAt = DateTime.UtcNow.Add(duration);

            foreach (var orderItem in reservation.Order.Items)
            {
                var key = dto.ItemQuantities.Keys
                    .FirstOrDefault(k => string.Equals(k, orderItem.Product!.Name, StringComparison.OrdinalIgnoreCase));

                if (key == null) continue;

                int newQty = dto.ItemQuantities[key];
                if (newQty <= 0) throw new InvalidOperationException($"'{orderItem.Product.Name}' için geçersiz rezervasyon adeti.");

                int totalReserved = await _db.ReservationItems
                    .Where(ri => ri.ProductId == orderItem.ProductId &&
                                 ri.Reservation.ExpireAt > DateTime.UtcNow &&
                                 ri.ReservationId != reservation.Id)
                    .SumAsync(ri => ri.Quantity);

                var existing = reservation.Items.FirstOrDefault(i => i.ProductId == orderItem.ProductId);
                int available = orderItem.Quantity - totalReserved + (existing?.Quantity ?? 0);

                if (newQty > available)
                    throw new InvalidOperationException($"'{orderItem.Product.Name}' için maksimum rezervasyon {available} olabilir.");

                if (existing != null)
                {
                    orderItem.Product.ReservedStock -= existing.Quantity;
                    if (orderItem.Product.ReservedStock < 0) orderItem.Product.ReservedStock = 0;

                    existing.Quantity = newQty;
                    orderItem.Product.ReservedStock += newQty;
                }
                else
                {
                    reservation.Items.Add(new ReservationItem
                    {
                        ProductId = orderItem.ProductId,
                        Quantity = newQty,
                        ReservationId = reservation.Id
                    });
                    orderItem.Product.ReservedStock += newQty;
                }
            }

            reservation.Status = ReservationStatus.Pending;

            await _db.SaveChangesAsync();
        }







        // Rezervasyon sil
        public async Task DeleteReservationAsync(int id)
        {
            var reservation = await _db.Reservations
                .Include(r => r.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                throw new KeyNotFoundException("Rezervasyon bulunamadı.");

            // Sadece Active (Pending) rezervasyon silinince stoktan düş
            if (reservation.Status == ReservationStatus.Pending)
            {
                foreach (var item in reservation.Items)
                {
                    item.Product.ReservedStock -= item.Quantity;
                    if (item.Product.ReservedStock < 0)
                        item.Product.ReservedStock = 0; // negatif kalmasın
                }
            }

            _db.Reservations.Remove(reservation);
            await _db.SaveChangesAsync();
        }


        // Rezervasyonu onayla ve siparişi güncelle
        public async Task ConfirmReservationAsync(int reservationId)
        {
            var reservation = await _db.Reservations
                .Include(r => r.Items)
                    .ThenInclude(ri => ri.Product)
                .Include(r => r.Order)
                    .ThenInclude(o => o.Items)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                throw new KeyNotFoundException("Rezervasyon bulunamadı.");

            if (reservation.Status == ReservationStatus.Completed)
                throw new InvalidOperationException("Rezervasyon zaten onaylanmış.");

            if (DateTime.UtcNow > reservation.ExpireAt)
                throw new InvalidOperationException("Rezervasyon süresi dolmuş.");

            foreach (var ri in reservation.Items)
            {
                var product = ri.Product!;
                if (product.Stock < ri.Quantity)
                    throw new InvalidOperationException($"'{product.Name}' için yeterli stok yok.");

                int soldQuantity = ri.Quantity;

                // Stok ve ReservedStock düş
                product.Stock -= soldQuantity;
                product.ReservedStock -= soldQuantity;
                if (product.ReservedStock < 0) product.ReservedStock = 0;

                // OrderItem Quantity güncelle
                var orderItem = reservation.Order!.Items.First(oi => oi.ProductId == ri.ProductId);
                orderItem.Quantity -= soldQuantity;
                if (orderItem.Quantity < 0) orderItem.Quantity = 0;

                // LineTotal read-only olduğu için manuel set yok
                
            }

            // Rezervasyon durumu güncelle
            reservation.Status = ReservationStatus.Completed;

            // Order.TotalAmount hesapla (LineTotal read-only)
            reservation.Order!.TotalAmount = reservation.Order.Items.Sum(i => i.LineTotal);

            await _db.SaveChangesAsync();
        }





    }
}
