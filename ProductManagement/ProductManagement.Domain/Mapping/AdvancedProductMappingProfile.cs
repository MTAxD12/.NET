namespace ProductManagement.Domain.Mapping;

using System;
using System.Globalization;
using AutoMapper;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.DTO;
using ProductManagement.Domain.Mapping.Resolvers;
using ProductManagement.Domain.Enums;

public class AdvancedProductMappingProfile : Profile
{
    public AdvancedProductMappingProfile()
    {
    
        CreateMap<CreateProductProfileRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsAvailable, opt => opt.Ignore());

        CreateMap<Product, ProductProfileDto>()
            .ForMember(dest => dest.CategoryDisplayName, 
                opt => opt.MapFrom(src => CategoryDisplayResolver.GetDisplayName(src.Category)))
            .ForMember(dest => dest.ProductAge, 
                opt => opt.MapFrom(src => ProductAgeResolver.Resolve(src.ReleaseDate)))
            .ForMember(dest => dest.BrandInitials, 
                opt => opt.MapFrom(src => BrandInitialsResolver.GetInitials(src.Brand)))
            .ForMember(dest => dest.AvailabilityStatus, 
                opt => opt.MapFrom(src => AvailabilityStatusResolver.Resolve(src.IsAvailable, src.StockQuantity)))
            .ForMember(dest => dest.ImageUrl,
                opt => opt.MapFrom(src => src.Category == ProductCategory.Home ? null : src.ImageUrl))
            .ForMember(dest => dest.Price,
                opt => opt.MapFrom(src => src.Category == ProductCategory.Home ? src.Price * 0.9m : src.Price))
            .ForMember(dest => dest.FormattedPrice, 
                opt => opt.MapFrom(src => PriceFormatterResolver.Format(
                    src.Category == ProductCategory.Home ? src.Price * 0.9m : src.Price)));
    }
}