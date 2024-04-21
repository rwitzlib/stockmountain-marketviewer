using AutoMapper;
using MarketDataProvider.Contracts.Dtos;
using MarketDataProvider.Contracts.Requests;
using MarketDataProvider.Contracts.Responses;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;
using Polygon.Client.Requests;
using Polygon.Client.Responses;
using System.Collections.Generic;

namespace MarketViewer.Infrastructure.Mapping
{
    public class AggregateProfile : Profile
    {
        public AggregateProfile()
        {
            CreateMap<StocksRequest, AggregateRequest>();

            CreateMap<AggregateResponse, StocksResponse>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Studies, opt => opt.Ignore())
                .ForMember(dest => dest.TickerDetails, opt => opt.Ignore());

            CreateMap<IEnumerable<AggregateDto>, StocksResponse>()
                .ConvertUsing<AggregateDtoToResponseConverter>();

            CreateMap<AggregateDto, StocksResponse>()
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Studies, opt => opt.Ignore())
                .ForMember(dest => dest.TickerDetails, opt => opt.Ignore());

            CreateMap<AggregateRequest, PolygonAggregateRequest>()
                .ForMember(dest => dest.Ticker, opt => opt.MapFrom(src => src.Ticker.ToUpperInvariant()))
                .ForMember(dest => dest.Timespan, opt => opt.MapFrom(src => src.Timespan.ToString().ToLowerInvariant()))
                .ForMember(dest => dest.From, opt => opt.MapFrom(src => src.From.ToUnixTimeMilliseconds()))
                .ForMember(dest => dest.To, opt => opt.MapFrom(src => src.To.ToUnixTimeMilliseconds()))
                .ForMember(dest => dest.Adjusted, opt => opt.Ignore())
                .ForMember(dest => dest.Sort, opt => opt.Ignore())
                .ForMember(dest => dest.Limit, opt => opt.Ignore());

            CreateMap<PolygonAggregateResponse, AggregateResponse>();

            CreateMap<PolygonAggregateResponse, StocksResponse>()
                .ForMember(dest => dest.Studies, opt => opt.Ignore())
                .ForMember(dest => dest.TickerDetails, opt => opt.Ignore());
        }
    }
}
