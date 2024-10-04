using AutoMapper;
using MarketViewer.Contracts.Models.ScanV2;

namespace MarketViewer.Application.Mapping
{
    public class FilterProfile : Profile
    {
        public FilterProfile()
        {
            CreateMap<Filter, Filter>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ReverseMap();
        }
    }
}
