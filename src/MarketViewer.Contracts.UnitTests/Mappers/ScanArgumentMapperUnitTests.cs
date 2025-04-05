using FluentAssertions;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Mappers;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Scan.Operands;
using MarketViewer.Contracts.Requests.Scan;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.UnitTests.Mappers;

public class ScanArgumentMapperUnitTests
{
    [Fact]
    public void Map_ShouldReturnCorrectScanArgument()
    {
        // Arrange
        var scanArgumentRequest = new ScanArgumentRequest
        {
            Operator = "AND",
            Filters =
            [
                new()
                {
                    CollectionModifier = "ALL",
                    FirstOperand = new OperandRequest
                    {
                        Type = OperandType.PriceAction,
                        Name = "Close",
                        Modifier = OperandModifier.Value,
                        Timeframe = new Timeframe(1, Timespan.minute)
                    },
                    Operator = FilterOperator.gt,
                    SecondOperand = new OperandRequest
                    {
                        Type = OperandType.PriceAction,
                        Name = "Open",
                        Modifier = OperandModifier.Slope,
                        Timeframe = new Timeframe(1, Timespan.minute)
                    },
                    Timeframe = new Timeframe(1, Timespan.minute)
                },
                new()
                {
                    FirstOperand = new OperandRequest
                    {
                        Type = OperandType.Study,
                        Name = "macd",
                        Parameters = "12,26,9,ema",
                        Modifier = OperandModifier.Value,
                        Timeframe = new Timeframe(1, Timespan.minute)
                    },
                    Operator = FilterOperator.gt,
                    SecondOperand = new OperandRequest
                    {
                        Type = OperandType.Fixed,
                        Value = 10000
                    },
                    Timeframe = new Timeframe(1, Timespan.minute)
                }
            ]
        };

        // Act
        var result = ScanArgumentMapper.ConvertFromRequest(scanArgumentRequest);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ScanArgument>();

        result.Operator.Should().Be("AND");
        result.Filters.Should().NotBeNull();
        result.Filters.Should().HaveCount(2);

        result.Filters[0].CollectionModifier.Should().Be("ALL");
        result.Filters[0].FirstOperand.Should().NotBeNull();
        result.Filters[0].FirstOperand.Should().BeOfType<PriceActionOperand>();
        result.Filters[0].FirstOperand.Should().BeEquivalentTo(new PriceActionOperand
        {
            PriceAction = PriceActionType.Close,
            Modifier = OperandModifier.Value,
            Timeframe = new Timeframe(1, Timespan.minute)
        });
        result.Filters[0].Operator.Should().Be(FilterOperator.gt);
        result.Filters[0].SecondOperand.Should().NotBeNull();
        result.Filters[0].SecondOperand.Should().BeOfType<PriceActionOperand>();
        result.Filters[0].SecondOperand.Should().BeEquivalentTo(new PriceActionOperand
        {
            PriceAction = PriceActionType.Open,
            Modifier = OperandModifier.Slope,
            Timeframe = new Timeframe(1, Timespan.minute)
        });
        result.Filters[0].Timeframe.Should().BeEquivalentTo(new Timeframe(1, Timespan.minute));

        result.Filters[1].FirstOperand.Should().NotBeNull();
        result.Filters[1].FirstOperand.Should().BeOfType<StudyOperand>();
        result.Filters[1].FirstOperand.Should().BeEquivalentTo(new StudyOperand
        {
            Study = StudyType.macd,
            Parameters = "12,26,9,ema",
            Modifier = OperandModifier.Value,
            Timeframe = new Timeframe(1, Timespan.minute)
        });
        result.Filters[1].Operator.Should().Be(FilterOperator.gt);
        result.Filters[1].SecondOperand.Should().NotBeNull();
        result.Filters[1].SecondOperand.Should().BeOfType<FixedOperand>();
        result.Filters[1].SecondOperand.Should().BeEquivalentTo(new FixedOperand
        {
            Value = 10000
        });
        result.Filters[1].Timeframe.Should().BeEquivalentTo(new Timeframe(1, Timespan.minute));
    }

    [Fact]
    public void Map_From_Json()
    {
        var json = "{\r\n          \"type\": \"Study\",\r\n          \"name\": \"rsi\",\r\n          \"parameters\": \"14,70,30,ema\",\r\n          \"modifier\": \"Value\",\r\n          \"timeframe\": {\r\n            \"multiplier\": 1,\r\n            \"timespan\": \"minute\"\r\n          }\r\n        }";
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<OperandRequest>(json, options);
    }
}