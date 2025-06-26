using Amazon;
using Amazon.S3;
using FluentAssertions;
using MarketViewer.Application.Handlers.Market.Scan;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Dtos;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Requests.Market.Scan;
using MarketViewer.Contracts.Responses.Market;
using MarketViewer.Core.Scan;
using MarketViewer.Core.Scan.Filters;
using MarketViewer.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Polygon.Client.Models;
using Polygon.Client.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MarketViewer.Application.UnitTests.Handlers;

public class ScanHandlerUnitTests
{
    private readonly ScanHandler _classUnderTest;
    private readonly MemoryMarketCache _marketCache;

    public ScanHandlerUnitTests()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var s3Client = new AmazonS3Client(RegionEndpoint.USEast2);

        _marketCache = new MemoryMarketCache(memoryCache, s3Client);

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<ScanFilterFactoryV2>()
            .AddSingleton<PriceActionFilter>()
            .AddSingleton<StudyFilter>()
            .AddSingleton<ValueFilter>();

        var serviceProvider = services.BuildServiceProvider();

        var scanFilterFactory = new ScanFilterFactoryV2(serviceProvider);

        _classUnderTest = new ScanHandler(scanFilterFactory, _marketCache, new NullLogger<ScanHandler>());
    }

    [Fact]
    public async Task LatestBarIsIncludedAndPassesFilters()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        SetupMarketCacheForMinuteTimeframe(now, true, true);

        var request = new ScanRequest
        {
            Timestamp = now,
            Argument = new ScanArgumentDto
            {
                Operator = "AND",
                Filters =
                [
                    new FilterDto {
                        CollectionModifier = "ALL",
                        FirstOperand = new OperandDto
                        {
                            Type = OperandType.PriceAction,
                            Name = "VWAP",
                            Modifier = OperandModifier.Value,
                            Timeframe = new Timeframe(1, Timespan.minute),
                        },
                        Operator = FilterOperator.gt,
                        SecondOperand = new OperandDto
                        {
                            Type = OperandType.Fixed,
                            Value = 5
                        },
                        Timeframe = new Timeframe(1, Timespan.minute)
                    }
                ]
            }
        };

        // Act
        var response = await _classUnderTest.Handle(request, default);

        // Assert
        response.Data.Items.Count().Should().Be(1);
    }

    [Fact]
    public async Task LatestBarIsIncludedAndDoesntPassFilters()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        SetupMarketCacheForMinuteTimeframe(now, true, false);

        var request = new ScanRequest
        {
            Timestamp = now,
            Argument = new ScanArgumentDto
            {
                Operator = "AND",
                Filters =
                [
                    new FilterDto {
                        CollectionModifier = "ALL",
                        FirstOperand = new OperandDto
                        {
                            Type = OperandType.PriceAction,
                            Name = "VWAP",
                            Modifier = OperandModifier.Value,
                            Timeframe = new Timeframe(1, Timespan.minute),
                        },
                        Operator = FilterOperator.gt,
                        SecondOperand = new OperandDto
                        {
                            Type = OperandType.Fixed,
                            Value = 5
                        },
                        Timeframe = new Timeframe(1, Timespan.minute)
                    }
                ]
            }
        };

        // Act
        var response = await _classUnderTest.Handle(request, default);

        // Assert
        response.Data.Items.Count().Should().Be(0);
    }

    [Fact]
    public async Task LatestBarIsNotIncluded_ShouldReturnItems()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        SetupMarketCacheForMinuteTimeframe(now, false);

        var request = new ScanRequest
        {
            Timestamp = now,
            Argument = new ScanArgumentDto
            {
                Operator = "AND",
                Filters =
                [
                    new FilterDto {
                        CollectionModifier = "ALL",
                        FirstOperand = new OperandDto
                        {
                            Type = OperandType.PriceAction,
                            Name = "VWAP",
                            Modifier = OperandModifier.Value,
                            Timeframe = new Timeframe(1, Timespan.minute),
                        },
                        Operator = FilterOperator.ge,
                        SecondOperand = new OperandDto
                        {
                            Type = OperandType.Fixed,
                            Value = 5
                        },
                        Timeframe = new Timeframe(1, Timespan.minute)
                    }
                ]
            }
        };

        // Act
        var response = await _classUnderTest.Handle(request, default);

        // Assert
        response.Data.Items.Count().Should().Be(1);
    }

    [Fact]
    public async Task LatestBarIsNotIncluded_ShouldNotReturnItems()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        SetupMarketCacheForMinuteTimeframe(now, false);

        var request = new ScanRequest
        {
            Timestamp = now,
            Argument = new ScanArgumentDto
            {
                Operator = "AND",
                Filters =
                [
                    new FilterDto {
                        CollectionModifier = "ALL",
                        FirstOperand = new OperandDto
                        {
                            Type = OperandType.PriceAction,
                            Name = "VWAP",
                            Modifier = OperandModifier.Value,
                            Timeframe = new Timeframe(1, Timespan.minute),
                        },
                        Operator = FilterOperator.gt,
                        SecondOperand = new OperandDto
                        {
                            Type = OperandType.Fixed,
                            Value = 5
                        },
                        Timeframe = new Timeframe(1, Timespan.minute)
                    }
                ]
            }
        };

        // Act
        var response = await _classUnderTest.Handle(request, default);

        // Assert
        response.Data.Items.Count().Should().Be(0);
    }

    [Fact]
    public async Task LatestBarIsIncluded_HourTimeframe()
    {
        // Arrange
        var now = DateTimeOffset.Now;
        SetupMarketCacheForHourTimeframe(now, true, true);

        var request = new ScanRequest
        {
            Timestamp = now,
            Argument = new ScanArgumentDto
            {
                Operator = "AND",
                Filters =
                [
                    new FilterDto {
                        CollectionModifier = "ALL",
                        FirstOperand = new OperandDto
                        {
                            Type = OperandType.PriceAction,
                            Name = "volume",
                            Modifier = OperandModifier.Value,
                            Timeframe = new Timeframe(1, Timespan.hour),
                        },
                        Operator = FilterOperator.gt,
                        SecondOperand = new OperandDto
                        {
                            Type = OperandType.Fixed,
                            Value = 1000
                        },
                        Timeframe = new Timeframe(1, Timespan.minute)
                    }
                ]
            }
        };

        // Act
        var response = await _classUnderTest.Handle(request, default);

        // Assert
        response.Data.Items.Count().Should().Be(1);
    }

    #region Private Methods

    private void SetupMarketCacheForMinuteTimeframe(DateTimeOffset timestamp, bool includeLatestBar, bool latestBarShouldPassFilters = true)
    {
        _marketCache.SetTickers(["SPY"]);
        _marketCache.SetTickersByTimeframe(timestamp, new Timeframe(1, Timespan.minute), ["SPY"]);

        List<Bar> results = [];
        for (int i = 30; i > 0; i--)
        {
            results.Add(new Bar
            {
                Timestamp = timestamp.AddMinutes(-i).ToUnixTimeMilliseconds(),
                Vwap = 5
            });
        }

        _marketCache.SetStocksResponse(new StocksResponse
        {
            Ticker = "SPY",
            Results = results
        }, new Timeframe(1, Timespan.minute), timestamp);

        if (!includeLatestBar)
        {
            return;
        }

        _marketCache.AddLiveBar(new PolygonWebsocketAggregateResponse
        {
            Ticker = "SPY",
            TickVwap = latestBarShouldPassFilters ? 1000 : 0,
            TickStart = timestamp.ToUnixTimeMilliseconds()
        });
    }

    private void SetupMarketCacheForHourTimeframe(DateTimeOffset timestamp, bool includeLatestBar, bool latestBarShouldPassFilters = true)
    {
        _marketCache.SetTickers(["SPY"]);
        _marketCache.SetTickersByTimeframe(timestamp, new Timeframe(1, Timespan.hour), ["SPY"]);

        var nearestHour = new DateTimeOffset(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0, timestamp.Offset);

        List<Bar> results = [];
        for (int i = 30; i >= 0; i--)
        {
            results.Add(new Bar
            {
                Timestamp = nearestHour.AddHours(-i).ToUnixTimeMilliseconds(),
                Volume = 1
            });
        }

        _marketCache.SetStocksResponse(new StocksResponse
        {
            Ticker = "SPY",
            Results = results
        }, new Timeframe(1, Timespan.hour), timestamp);

        if (!includeLatestBar)
        {
            return;
        }

        _marketCache.AddLiveBar(new PolygonWebsocketAggregateResponse
        {
            Ticker = "SPY",
            Volume = latestBarShouldPassFilters ? 1000 : 0,
            TickStart = timestamp.ToUnixTimeMilliseconds()
        });
    }
    #endregion
}
