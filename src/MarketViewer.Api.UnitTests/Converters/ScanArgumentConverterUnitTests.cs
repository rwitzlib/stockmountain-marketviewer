using FluentAssertions;
using MarketViewer.Contracts.Converters;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Models.ScanV2.Operands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MarketViewer.Api.UnitTests.Converters;

public class ScanArgumentConverterUnitTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        
    };

    public ScanArgumentConverterUnitTests()
    {
        _options.Converters.Add(new ScanArgumentConverter()); 
    }

    [Fact]
    public void Serialize()
    {
        // Arrange
        var json = "{\r\n    \"Operator\": \"AND\",\r\n    \"Filters\": [\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"Name\": \"PriceAction\",\r\n          \"Modifier\": \"Value\",\r\n          \"Type\": \"Close\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"minute\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Name\": \"Value\",\r\n          \"Value\": 5\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 5,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"Name\": \"Study\",\r\n          \"Modifier\": \"Value\",\r\n          \"Type\": \"macd\",\r\n          \"Parameters\": \"12,26,9,EMA\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"minute\"\r\n        },\r\n        \"Operator\": \"lt\",\r\n        \"SecondOperand\": {\r\n          \"Name\": \"Value\",\r\n          \"Value\": 0\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 5,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      }\r\n      \r\n    ],\r\n    \"Argument\": {\r\n      \"Operator\": \"AND\",\r\n      \"Filters\": [\r\n        {\r\n          \"CollectionModifier\": \"ALL\",\r\n          \"FirstOperand\": {\r\n            \"Name\": \"PriceAction\",\r\n            \"ValueType\": \"Value\",\r\n            \"Type\": \"Close\",\r\n            \"Multiplier\": 1,\r\n            \"Timespan\": \"minute\"\r\n          },\r\n          \"Operator\": \"lt\",\r\n          \"SecondOperand\": {\r\n            \"Name\": \"Value\",\r\n            \"Value\": 10      \r\n          },\r\n          \"Timeframe\": {\r\n            \"Multiplier\": 5,\r\n            \"Timespan\": \"minute\"\r\n          }\r\n        }\r\n      ]\r\n    }\r\n  }";
        var deserialized = JsonSerializer.Deserialize<ScanArgument>(json, _options);
        var serialized = JsonSerializer.Serialize(deserialized, _options);

        // Act
        var argument = JsonSerializer.Deserialize<ScanArgument>(serialized, _options);

        // Assert
        argument.Operator.Should().Be("AND");

        argument.Filters.Should().NotBeNull();
        argument.Filters.Count.Should().Be(2);

        var firstOperand = argument.Filters[0].FirstOperand.Should().BeOfType<PriceActionOperand>().Subject;
        firstOperand.Modifier.Should().Be(OperandModifier.Value);
        firstOperand.PriceAction.Should().Be(PriceActionType.Close);
        firstOperand.Multiplier.Should().Be(1);
        firstOperand.Timespan.Should().Be(Timespan.minute);

        argument.Argument.Should().NotBeNull();
    }

    [Fact]
    public void Deserialization()
    {
        // Arrange
        var json = "{\r\n    \"Operator\": \"AND\",\r\n    \"Filters\": [\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"Name\": \"PriceAction\",\r\n          \"Modifier\": \"Value\",\r\n          \"Type\": \"Close\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"minute\"\r\n        },\r\n        \"Operator\": \"gt\",\r\n        \"SecondOperand\": {\r\n          \"Name\": \"Value\",\r\n          \"Value\": 5\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 5,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      },\r\n      {\r\n        \"CollectionModifier\": \"ALL\",\r\n        \"FirstOperand\": {\r\n          \"Name\": \"Study\",\r\n          \"Modifier\": \"Value\",\r\n          \"Type\": \"macd\",\r\n          \"Parameters\": \"12,26,9,EMA\",\r\n          \"Multiplier\": 1,\r\n          \"Timespan\": \"minute\"\r\n        },\r\n        \"Operator\": \"lt\",\r\n        \"SecondOperand\": {\r\n          \"Name\": \"Value\",\r\n          \"Value\": 0\r\n        },\r\n        \"Timeframe\": {\r\n          \"Multiplier\": 5,\r\n          \"Timespan\": \"minute\"\r\n        }\r\n      }\r\n      \r\n    ],\r\n    \"Argument\": {\r\n      \"Operator\": \"AND\",\r\n      \"Filters\": [\r\n        {\r\n          \"CollectionModifier\": \"ALL\",\r\n          \"FirstOperand\": {\r\n            \"Name\": \"PriceAction\",\r\n            \"ValueType\": \"Value\",\r\n            \"Type\": \"Close\",\r\n            \"Multiplier\": 1,\r\n            \"Timespan\": \"minute\"\r\n          },\r\n          \"Operator\": \"lt\",\r\n          \"SecondOperand\": {\r\n            \"Name\": \"Value\",\r\n            \"Value\": 10      \r\n          },\r\n          \"Timeframe\": {\r\n            \"Multiplier\": 5,\r\n            \"Timespan\": \"minute\"\r\n          }\r\n        }\r\n      ]\r\n    }\r\n  }";

        // Act
        var argument = JsonSerializer.Deserialize<ScanArgument>(json, _options);

        // Assert
        argument.Operator.Should().Be("AND");

        argument.Filters.Should().NotBeNull();
        argument.Filters.Count.Should().Be(2);

        var firstOperand = argument.Filters[0].FirstOperand.Should().BeOfType<PriceActionOperand>().Subject;
        firstOperand.Modifier.Should().Be(OperandModifier.Value);
        firstOperand.PriceAction.Should().Be(PriceActionType.Close);
        firstOperand.Multiplier.Should().Be(1);
        firstOperand.Timespan.Should().Be(Timespan.minute);

        argument.Argument.Should().NotBeNull();
    }
}
