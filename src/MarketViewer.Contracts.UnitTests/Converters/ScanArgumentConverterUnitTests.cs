using FluentAssertions;
using MarketViewer.Contracts.Converters;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Scan.Operands;
using System.Text.Json;

namespace MarketViewer.Contracts.UnitTests.Converters;

public class ScanArgumentConverterUnitTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public ScanArgumentConverterUnitTests()
    {
        _options.Converters.Add(new ScanArgumentConverter());
        _options.Converters.Add(new FilterConverter());
    }

    [Fact]
    public void Serialize()
    {
        // Arrange
        var json = "{\r\n    \"Operator\": \"AND\",\r\n    \"Filters\": [\r\n      {\r\n        \"FirstOperand\": {\r\n          \"Property\": \"Float\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 1000000\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ANY\",\r\n        \"FirstOperand\": {\r\n          \"Study\": \"rsi\",\r\n          \"Modifier\": \"Slope\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"lt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 0\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 4,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"Study\": \"rsi\",\r\n          \"Modifier\": \"Slope\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 0\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 3,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"Study\": \"macd\",\r\n          \"Modifier\": \"Slope\",\r\n          \"Parameters\": \"12,26,9,ema\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 0\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 3,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"Study\": \"macd\",\r\n          \"Modifier\": \"Value\",\r\n          \"Parameters\": \"12,26,9,ema\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"lt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 0\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 2,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"PriceAction\": \"Volume\",\r\n          \"Modifier\": \"Value\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"minute\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 100000\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 5,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"PriceAction\": \"Vwap\",\r\n          \"Modifier\": \"Value\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 2\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 5,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"PriceAction\": \"Vwap\",\r\n          \"Modifier\": \"Value\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"lt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 25\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 5,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      }\r\n    ]\r\n  }";

        var deserialized = JsonSerializer.Deserialize<ScanArgument>(json, _options);
        var serialized = JsonSerializer.Serialize(deserialized, _options);

        // Act
        var argument = JsonSerializer.Deserialize<ScanArgument>(serialized, _options);

        // Assert
        argument.Operator.Should().Be("AND");

        argument.Filters.Should().NotBeNull();
        argument.Filters.Count.Should().Be(8);

        var priceActionOperand = argument.Filters[7].FirstOperand.Should().BeOfType<PriceActionOperand>().Subject;
        priceActionOperand.Modifier.Should().Be(OperandModifier.Value);
        priceActionOperand.PriceAction.Should().Be(PriceActionType.Vwap);

        argument.Argument.Should().BeNull();
    }

    [Fact]
    public void Deserialization()
    {
        // Arrange
        var json = "{\r\n    \"Operator\": \"AND\",\r\n    \"Filters\": [\r\n      {\r\n        \"FirstOperand\": {\r\n          \"Property\": \"Float\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 1000000\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ANY\",\r\n        \"FirstOperand\": {\r\n          \"Study\": \"rsi\",\r\n          \"Modifier\": \"Slope\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"lt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 0\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 4,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"Study\": \"rsi\",\r\n          \"Modifier\": \"Slope\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 0\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 3,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"Study\": \"macd\",\r\n          \"Modifier\": \"Slope\",\r\n          \"Parameters\": \"12,26,9,ema\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 0\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 3,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"Study\": \"macd\",\r\n          \"Modifier\": \"Value\",\r\n          \"Parameters\": \"12,26,9,ema\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"lt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 0\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 2,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"PriceAction\": \"Volume\",\r\n          \"Modifier\": \"Value\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"minute\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 100000\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 5,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"PriceAction\": \"Vwap\",\r\n          \"Modifier\": \"Value\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 2\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 5,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"PriceAction\": \"Vwap\",\r\n          \"Modifier\": \"Value\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"hour\"\r\n        },\r\n        \"Operator\": \"lt\",\r\n        \"SecondOperand\": {\r\n          \"Value\": 25\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 5,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      }\r\n    ]\r\n  }";

        // Act
        var argument = JsonSerializer.Deserialize<ScanArgument>(json, _options);

        // Assert
        argument.Operator.Should().Be("AND");

        argument.Filters.Should().NotBeNull();
        argument.Filters.Count.Should().Be(8);

        var priceActionOperand = argument.Filters[7].FirstOperand.Should().BeOfType<PriceActionOperand>().Subject;
        priceActionOperand.Modifier.Should().Be(OperandModifier.Value);
        priceActionOperand.PriceAction.Should().Be(PriceActionType.Vwap);

        argument.Argument.Should().BeNull();
    }
}