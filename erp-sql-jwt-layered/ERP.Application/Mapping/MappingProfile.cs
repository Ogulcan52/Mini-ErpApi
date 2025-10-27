using AutoMapper;
using ERP.Application.DTOs;
using ERP.Domain.Entities;
using System;
using System.Linq;

namespace ERP.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product
            CreateMap<Product, ProductReadDto>();
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();

            // Customer
            CreateMap<Customer, CustomerReadDto>();
            CreateMap<CustomerCreateDto, Customer>();
            CreateMap<CustomerUpdateDto, Customer>();

            // Order
            CreateMap<Order, OrderReadDto>()
                .ForCtorParam("Items", opt => opt.MapFrom(o => o.Items.Select(i => new OrderItemReadDto(
                    i.ProductId, i.Product!.Name, i.Quantity, i.UnitPrice, i.LineTotal
                )).ToList()));

            // ReservationItem
            CreateMap<ReservationItem, ReservationItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

            // Reservation
            CreateMap<Reservation, ReservationReadDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.StatusMessage, opt => opt.MapFrom(src =>
                    src.Status == ReservationStatus.Completed
                    ? "Reservation Completed"
                    : (src.Status == ReservationStatus.Expired || DateTime.UtcNow > src.ExpireAt)
                    ? "Reservation Expired"
                    : "Reservation Active")
                );
        }
    }
}
