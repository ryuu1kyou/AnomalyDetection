using AnomalyDetection.MultiTenancy.Dtos;
using AutoMapper;

namespace AnomalyDetection.MultiTenancy;

public class MultiTenancyApplicationAutoMapperProfile : Profile
{
    public MultiTenancyApplicationAutoMapperProfile()
    {
        // OemMaster mappings
        CreateMap<OemMaster, OemMasterDto>()
            .ForMember(dest => dest.OemCode, opt => opt.MapFrom(src => src.OemCode.Code))
            .ForMember(dest => dest.OemName, opt => opt.MapFrom(src => src.OemCode.Name));
            
        CreateMap<OemFeature, OemFeatureDto>();

        // ExtendedTenant mappings
        CreateMap<ExtendedTenant, ExtendedTenantDto>()
            .ForMember(dest => dest.OemCode, opt => opt.MapFrom(src => src.OemCode.Code))
            .ForMember(dest => dest.OemName, opt => opt.MapFrom(src => src.OemCode.Name));
            
        CreateMap<TenantFeature, TenantFeatureDto>();
    }
}