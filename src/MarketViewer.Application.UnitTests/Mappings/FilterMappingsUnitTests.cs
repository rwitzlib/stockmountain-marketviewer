using Xunit;
using AutoMapper;
using MarketViewer.Application.Mapping;

namespace MarketViewer.Application.UnitTests.Mappings
{
    public class FilterMappingsUnitTests
    {
        [Fact]
        public void Check_Aggregate_Mapping_Configuration()
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.AddProfile<FilterProfile>());

            configuration.AssertConfigurationIsValid();
        }
    }
}
