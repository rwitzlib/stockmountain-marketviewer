using MarketViewer.Contracts.Models.Scan;
using FluentAssertions;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan.Operands;
using MarketViewer.Contracts.Entities.Scan;

namespace MarketViewer.Contracts.UnitTests.Models.Scan;

public class ScanArgumentUnitTests
{
    [Fact]
    public void GetTimeframes_Returns_EmptyList_When_ScanArgument_Is_Null()
    {
        // Arrange
        ScanArgument scanArgument = null;

        // Act
        var result = scanArgument?.GetTimeframes();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTimeframes_Returns_EmptyList_When_Filters_Are_Empty()
    {
        // Arrange
        var scanArgument = new ScanArgument
        {
            Filters = []
        };

        // Act
        var result = scanArgument.GetTimeframes();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetTimeframes_Returns_Timeframes_From_Filters()
    {
        // Arrange
        var scanArgument = new ScanArgument
        {
            Filters = [
                new Filter
                {
                    FirstOperand = new PriceActionOperand
                    {
                        Timeframe = new Timeframe(1, Timespan.minute)
                    },
                    SecondOperand = new FixedOperand
                    {
                        Value = 1
                    }
                },
                new Filter
                {
                    FirstOperand = new PriceActionOperand
                    {
                        Timeframe = new Timeframe(1, Timespan.hour)
                    },
                    SecondOperand = new FixedOperand
                    {
                        Value = 1
                    }
                }
            ]
        };

        // Act
        var result = scanArgument.GetTimeframes();

        // Assert
        result.Should().HaveCount(2);
        result[0].Multiplier.Should().Be(1);
        result[1].Timespan.Should().Be(Timespan.hour);
    }

    [Fact]
    public void GetTimeframes_Returns_Distinct_Timeframes()
    {
        // Arrange
        var scanArgument = new ScanArgument
        {
            Filters = [
                new Filter
                {
                    FirstOperand = new PriceActionOperand
                    {
                        Timeframe = new Timeframe(1, Timespan.minute)
                    },
                    SecondOperand = new FixedOperand
                    {
                        Value = 1
                    }
                },
                new Filter
                {
                    FirstOperand = new PriceActionOperand
                    {
                        Timeframe = new Timeframe(1, Timespan.minute)
                    },
                    SecondOperand = new FixedOperand
                    {
                        Value = 1
                    }
                }
            ]
        };

        // Act
        var result = scanArgument.GetTimeframes();

        // Assert
        result.Should().HaveCount(1);
        result.First().Multiplier.Should().Be(1);
        result.First().Timespan.Should().Be(Timespan.minute);
    }

    [Fact]
    public void GetTimeframes_Returns_Timeframes_From_Nested_Arguments()
    {
        // Arrange
        var nestedArgument = new ScanArgument
        {
            Filters = [
                new Filter
                {
                    FirstOperand = new PriceActionOperand
                    {
                        Timeframe = new Timeframe(1, Timespan.minute)
                    },
                    SecondOperand = new FixedOperand
                    {
                        Value = 1
                    }
                }
            ]
        };

        var scanArgument = new ScanArgument
        {
            Filters = [],
            Argument = nestedArgument
        };

        // Act
        var result = scanArgument.GetTimeframes();

        // Assert
        result.Should().HaveCount(1);
        result[0].Multiplier.Should().Be(1);
    }
}
