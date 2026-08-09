using AutoMapper;
using FleetCarePro.Models;
using FleetCarePro.ViewModels;

namespace FleetCarePro.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ServiceCenterViewModel, ServiceCenter>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceRecords, opt => opt.Ignore())
                .ForMember(dest => dest.VendorServices, opt => opt.Ignore());

            CreateMap<ServiceCenter, ServiceCenterViewModel>()
                .ForMember(dest => dest.SelectedServiceCategoryIds, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceCategories, opt => opt.Ignore());

            CreateMap<ServiceCategoryViewModel, ServiceCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VendorServices, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceLineItems, opt => opt.Ignore());

            CreateMap<ServiceCategory, ServiceCategoryViewModel>();

            CreateMap<VehicleViewModel, Vehicle>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleImageURL, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceRecords, opt => opt.Ignore());

            CreateMap<Vehicle, VehicleViewModel>()
                .ForMember(dest => dest.VehicleImage, opt => opt.Ignore())
                .ForMember(dest => dest.Drivers, opt => opt.Ignore());

            CreateMap<ServiceLineItemViewModel, ServiceLineItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceRecordId, opt => opt.Ignore());
        }
    }
}