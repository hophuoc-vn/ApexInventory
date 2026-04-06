using AutoMapper;
using Apex.Domain.Entities;
using Apex.Application.DTOs;

namespace Apex.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map Order -> OrderResponseDto
        CreateMap<Order, OrderResponseDto>();

        // Map OrderItem -> OrderItemResponseDto
        CreateMap<OrderItem, OrderItemResponseDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Product.Sku));
    }
}