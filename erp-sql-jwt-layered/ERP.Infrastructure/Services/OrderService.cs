using AutoMapper;
using ERP.Application.DTOs;
using ERP.Application.Interfaces;
using ERP.Application.Sepicifications;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        
        
        public OrderService(AppDbContext db, IMapper mapper, IDataShaper<OrderReadDto> dataShaper) 
        {
             _db = db;
        }
        public async Task<int> CreateAsync(OrderCreateDto dto)
        {
            // Müşteri kontrolü
            var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
            if (!customerExists)
                throw new InvalidOperationException("Müşteri bulunamadı.");

            // Ürünleri getir (büyük/küçük harf duyarlılığını kaldır)
            var productNames = dto.Items.Select(i => i.ProductName.ToLower()).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productNames.Contains(p.Name.ToLower()))
                .ToListAsync();

            // Ürün kontrolü
            foreach (var name in productNames)
            {
                if (!products.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"Ürün bulunamadı: {name}");
            }

            // Sipariş oluştur
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                Status = "Pending",
                Items = new List<OrderItem>()
            };

            foreach (var item in dto.Items)
            {
                var product = products.FirstOrDefault(p => p.Name.Equals(item.ProductName, StringComparison.OrdinalIgnoreCase));

                if (product == null)
                    throw new InvalidOperationException($"Ürün bulunamadı: {item.ProductName}");

                

                // Rezervasyon ekle
               

                // Sipariş kalemi ekle
                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.UnitPrice
                });
            }

            // Toplam tutarı hesapla
            order.TotalAmount = order.Items.Sum(i => i.LineTotal);

            // Veritabanına ekle
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return order.Id;
        }



        public async Task DeleteAsync(int id)
        {
            // Siparişi ve kalemlerini getir
            var entity = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (entity is null)
                throw new KeyNotFoundException("Sipariş bulunamadı.");

            // Sipariş kalemlerini sil
            _db.OrderItems.RemoveRange(entity.Items);

            // Siparişi sil
            _db.Orders.Remove(entity);

            await _db.SaveChangesAsync();
        }


        public async Task CompleteAsync(int id)
        {
            // Siparişi ve ürünlerini getir
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null)
                throw new KeyNotFoundException("Sipariş bulunamadı.");

            if (order.Status == "Completed")
                return;

            // İlgili ürünleri çek
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            // Stok ve IsActive kontrolü yap
            foreach (var item in order.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);

                // IsActive kontrolü
                if (!product.IsActive)
                    throw new InvalidOperationException($"Bu ürün satışta değil: {product.Name}");

                // Sadece gerçek stok kontrolü
                if (product.Stock < item.Quantity)
                    throw new InvalidOperationException($"Yeterli stok yok: {product.Name} (Lütfen sparişinizi güncelleyin)");
            }

            // Stokları düş (reserved stock'a dokunma)
            foreach (var item in order.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.Stock -= item.Quantity;

                // Eğer stok < 0 olmamalıysa ek kontrol
                if (product.Stock < 0)
                    product.Stock = 0;
            }

            // Siparişi Completed yap
            order.Status = "Completed";
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }




        public async Task<IEnumerable<OrderReadDto>> GetAllAsync()
        {
            var list = await _db.Orders.Include(o => o.Items).ThenInclude(i => i.Product).AsNoTracking().ToListAsync();
            var dto = list.Select(o => new OrderReadDto(
                o.Id, o.CustomerId, o.TotalAmount, o.Status,
                o.Items.Select(i => new OrderItemReadDto(
                    i.ProductId, i.Product!.Name, i.Quantity, i.UnitPrice, i.LineTotal
                )).ToList()
            ));
            return dto;
        }

        public async Task<OrderReadDto?> GetAsync(int id)
        {
            var o = await _db.Orders.Include(x => x.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(x => x.Id == id);
            if (o is null) return null;
            return new OrderReadDto(
                o.Id, o.CustomerId, o.TotalAmount, o.Status,
                o.Items.Select(i => new OrderItemReadDto(
                    i.ProductId, i.Product!.Name, i.Quantity, i.UnitPrice, i.LineTotal
                )).ToList()
            );
        }

        public async Task<bool> UpdateAsync(int orderId, OrderItemUpdateDto request)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            // Sadece Pending olan siparişler güncellenebilir
            if (order.Status != "Pending")
                throw new InvalidOperationException("Sadece Pending durumundaki siparişler güncellenebilir.");

            // Mevcut sipariş kalemlerini dictionary ile hazırla (productId => OrderItem)
            var existingItems = order.Items.ToDictionary(i => i.ProductId);

            foreach (var newItem in request.Items)
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Name == newItem.ProductName);
                if (product == null)
                    throw new Exception($"Product '{newItem.ProductName}' bulunamadı.");

                if (existingItems.TryGetValue(product.Id, out var oldItem))
                {
                    // ReservedStock farkını uygulama, sadece quantity ve fiyatı güncelle
                    oldItem.Quantity = newItem.Quantity;
                    oldItem.UnitPrice = product.UnitPrice;
                }
                else
                {
                    // Yeni ürün ekle (reserved stock’a dokunma)
                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = newItem.Quantity,
                        UnitPrice = product.UnitPrice
                    });
                }
            }

            // CustomerId güncelle
            order.CustomerId = request.CustomerId;

            // TotalAmount güncelle
            order.TotalAmount = order.Items.Sum(i => i.LineTotal);

            // UpdatedAt güncelle (status aynı Pending)
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

    }
}

