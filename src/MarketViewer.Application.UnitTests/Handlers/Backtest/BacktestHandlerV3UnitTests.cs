using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using AutoFixture;
using FluentAssertions;
using MarketViewer.Application.Handlers.Backtest;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Requests.Backtest;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MarketViewer.Application.UnitTests.Handlers.Backtest;

public class BacktestHandlerV3UnitTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IAmazonLambda> _lambdaClient;
    private readonly Mock<IDynamoDBContext> _dynamodbClient;
    private readonly BacktestHandlerV3 _classUnderTest;
    private readonly AutoMocker _autoMocker;

    public BacktestHandlerV3UnitTests()
    {
        _fixture = new Fixture();
        _autoMocker = new AutoMocker();

        _lambdaClient = new Mock<IAmazonLambda>();
        _dynamodbClient = new Mock<IDynamoDBContext>();

        _classUnderTest = new BacktestHandlerV3(_lambdaClient.Object, _dynamodbClient.Object, new NullLogger<BacktestHandlerV3>());
    }

    [Fact]
    public async Task BasicRequest()
    {
        // Arrange
        var request = new BacktestV3Request
        {
            Start = DateTimeOffset.Parse("2024-10-1"),
            End = DateTimeOffset.Parse("2024-10-3"),
            PositionInfo = new BacktestPosition
            {
                StartingBalance = 10000,
                MaxConcurrentPositions = 10,
                PositionSize = 1000
            },
            Exit = new BacktestExit
            {
                Timeframe = new Timeframe
                {
                    Multiplier = 5,
                    Timespan = Timespan.day
                }
            },
            Argument = new ScanArgument()
        };

        GivenLambdaReturnsEntriesForDates([
            DateTimeOffset.Parse("2024-10-01").ToOffset(TimeSpan.FromHours(-5)).AddHours(9),
            DateTimeOffset.Parse("2024-10-02").ToOffset(TimeSpan.FromHours(-5)).AddHours(10),
            DateTimeOffset.Parse("2024-10-03").ToOffset(TimeSpan.FromHours(-5)).AddHours(11)
        ]);

        // Act
        //var response = await _classUnderTest.Handle(request, default);

        //// Assert
        //response.Data.HoldBalance.Should().Be(10150);
        //response.Data.HighBalance.Should().Be(10450);
    }

    [Fact]
    public async Task MaxConcurrentPositions_Is_Hit()
    {
        // Arrange
        var request = new BacktestV3Request
        {
            Start = DateTimeOffset.Parse("2024-10-1"),
            End = DateTimeOffset.Parse("2024-10-3"),
            PositionInfo = new BacktestPosition
            {
                StartingBalance = 10000,
                MaxConcurrentPositions = 2,
                PositionSize = 1000
            },
            Exit = new BacktestExit
            {
                Timeframe = new Timeframe
                {
                    Multiplier = 5,
                    Timespan = Timespan.day
                }
            },
            Argument = new ScanArgument()
        };

        GivenLambdaReturnsEntriesForDates([
            DateTimeOffset.Parse("2024-10-01").ToOffset(TimeSpan.FromHours(-5)).AddHours(9),
            DateTimeOffset.Parse("2024-10-02").ToOffset(TimeSpan.FromHours(-5)).AddHours(10),
            DateTimeOffset.Parse("2024-10-03").ToOffset(TimeSpan.FromHours(-5)).AddHours(11)
        ]);

        // Act
        //var response = await _classUnderTest.Handle(request, default);

        //// Assert
        //response.Data.HoldBalance.Should().Be(10100);
        //response.Data.HighBalance.Should().Be(10300);
    }

    private void GivenLambdaReturnsEntriesForDates(List<DateTimeOffset> dates)
    {
        _lambdaClient.SetupSequence(method => method.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvokeResponse
            {
                StatusCode = 200,
                Payload = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(GivenValidBacktestEntry(dates[0]))))
            })
            .ReturnsAsync(new InvokeResponse
            {
                StatusCode = 200,
                Payload = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(GivenValidBacktestEntry(dates[1]))))
            })
            .ReturnsAsync(new InvokeResponse
            {
                StatusCode = 200,
                Payload = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(GivenValidBacktestEntry(dates[2]))))
            });
    }

    private static BacktestEntryV3 GivenValidBacktestEntry(DateTimeOffset date)
    {
        return new BacktestEntryV3
        {
            Date = date.Date,
            Hold = new BackTestEntryStatsV3
            {
                PositiveTrendRatio = 1,
                AvgWin = 50,
                AvgLoss = 0,
                AvgProfit = 50,
                SumProfit = 50
            },
            High = new BackTestEntryStatsV3
            {
                SumProfit = 150
            },
            Results = new List<BackTestEntryResultCollection>
            {
                new BackTestEntryResultCollection()
                {
                    Ticker = "SPY",
                    StartPrice = 550,
                    Shares = 1,
                    StartPosition = 550,
                    BoughtAt = date,
                    Hold = new BackTestEntryResultV3
                    {
                        EndPrice = 600,
                        SoldAt = GetEndDate(date, 5, Timespan.day),
                        StoppedOut = false,
                        EndPosition = 600,
                        Profit = 50
                    },
                    High = new BackTestEntryResultV3
                    {
                        EndPrice = 700,
                        SoldAt = GetEndDate(date, 3, Timespan.day),
                        StoppedOut = false,
                        EndPosition = 700,
                        Profit = 150
                    }
                }
            }
        };
    }

    public static DateTimeOffset GetEndDate(DateTimeOffset start, int multiplier, Timespan timespan)
    {
        var totalDays = timespan switch
        {
            Timespan.minute => multiplier / 1440,
            Timespan.hour => multiplier / 24,
            Timespan.day => multiplier,
            Timespan.week => multiplier * 7,
            Timespan.month => throw new NotImplementedException(),
            Timespan.quarter => throw new NotImplementedException(),
            Timespan.year => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };

        var weekends = (((int)start.DayOfWeek + totalDays) / 5) * 2;

        var businessDays = timespan switch
        {
            Timespan.minute => TimeSpan.FromMinutes(multiplier),
            Timespan.hour => TimeSpan.FromHours(multiplier),
            Timespan.day => TimeSpan.FromDays(multiplier),
            Timespan.week => TimeSpan.FromDays(multiplier * 7),
            Timespan.month => throw new NotImplementedException(),
            Timespan.quarter => throw new NotImplementedException(),
            Timespan.year => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };

        var end = start.Add(businessDays).AddDays(weekends);

        if (end.DayOfWeek is DayOfWeek.Saturday)
        {
            end = end.AddDays(-1);
        }
        if (end.DayOfWeek is DayOfWeek.Sunday)
        {
            end = end.AddDays(-2);
        }

        return end;
    }
}
