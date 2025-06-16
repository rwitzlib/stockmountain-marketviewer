using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MarketViewer.Contracts.Interfaces;
using MarketViewer.Studies;
using System.Linq;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Study;
using MarketViewer.Contracts.Requests.Market;
using MarketViewer.Contracts.Responses.Market;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using Polygon.Client.Models;
using Amazon.Runtime.Internal;

namespace MarketViewer.Application.Handlers.Market;

public class StocksHandler(IMarketDataRepository repository, IMarketCache marketCache, StudyFactory studyFactory) : IRequestHandler<StocksRequest, OperationResult<StocksResponse>>
{
    public async Task<OperationResult<StocksResponse>> Handle(StocksRequest request, CancellationToken cancellationToken)
    {
        if (!ValidateAggregateRequest(request, out var errorMessages))
        {
            return new OperationResult<StocksResponse>
            {
                Status = HttpStatusCode.BadRequest,
                ErrorMessages = errorMessages
            };
        }

        StocksResponse response;

        // Check if we should use cache: minute timespan and date range within last 5 days
        if (ShouldUseCache(request))
        {
            var timeframe = new Timeframe(1, request.Timespan);
            var cacheResponse = marketCache.GetStocksResponse(request.Ticker, timeframe, DateTimeOffset.Now);
            
            // If cache miss, fall back to repository
            if (cacheResponse is null)
            {
                response = await repository.GetStockDataAsync(request);
            }
            else
            {
                response = cacheResponse.Clone();
            }

            if (request.To.Date == DateTimeOffset.Now.Date)
            {
                // If the request is for today's data, we need to ensure we have the latest live bar
                var latestBar = marketCache.GetLiveBar(request.Ticker);

                TryAddBarToResponse(request.Multiplier, request.Timespan, latestBar, response);
            }
        }
        else
        {
            response = await repository.GetStockDataAsync(request);
        }

        if (response is null)
        {
            errorMessages.Add("Query returned invalid result.");

            return new OperationResult<StocksResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = errorMessages
            };
        }

        if (response.Results is null || response.Results.Count() == 0)
        {
            errorMessages.Add("Query returned no results.");

            return new OperationResult<StocksResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = errorMessages
            };
        }

        if (request.Studies is not null)
        {
            var studies = new List<StudyResponse>();

            foreach (var study in request.Studies)
            {
                var result = studyFactory.Compute(study.Type, study.Parameters, response);

                if (result is not null && result.Results.Count > 0)
                {
                    studies.Add(result);
                }
            }

            if (studies.Count > 0)
            {
                response.Studies = studies;

                for (int i = 0; i < response.Studies.Count; i++)
                {
                    var study = response.Studies[i];

                    for (int j = 0; j < study.Results.Count; j++)
                    {
                        if (study.Results[j].Count > request.Limit)
                        {
                            study.Results[j] = study.Results[j].TakeLast(request.Limit).ToList();
                        }
                    }
                }
            }
        }

        response.Results = response.Results.TakeLast(request.Limit).ToList();

        return new OperationResult<StocksResponse>
        {
            Status = HttpStatusCode.OK,
            Data = response
        };
    }

    #region Private Methods
    private static bool ShouldUseCache(StocksRequest request)
    {
        return request.Timespan switch
        {
            Timespan.minute => request.From >= DateTimeOffset.Now.AddDays(-5),
            Timespan.hour => request.From >= DateTimeOffset.Now.AddDays(-30),
            Timespan.day => request.From >= DateTimeOffset.Now.AddDays(-365),
            Timespan.week => throw new NotImplementedException(),
            Timespan.month => throw new NotImplementedException(),
            Timespan.quarter => throw new NotImplementedException(),
            Timespan.year => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }

    private static bool ValidateAggregateRequest(StocksRequest request, out List<string> errorMessages)
    {
        errorMessages = [];

        if (string.IsNullOrWhiteSpace(request.Ticker))
        {
            errorMessages.Add("Must include a valid Ticker.");
        }

        if (request.From > DateTimeOffset.Now)
        {
            errorMessages.Add($"'From' date must be earlier than {DateTimeOffset.Now:yyyy-MM-dd}.");
        }

        if (request.From < DateTimeOffset.UnixEpoch)
        {
            errorMessages.Add($"'From' date must be later than {DateTimeOffset.UnixEpoch:yyyy-MM-dd}.");
        }

        if (request.From > request.To)
        {
            errorMessages.Add("'From' date must be earlier than 'To' date.");
        }

        request.To = request.To.AddHours(23).AddMinutes(59);

        return errorMessages.Count == 0;
    }

    private static void TryAddBarToResponse(int multiplier, Timespan timespan, Bar latestBar, StocksResponse response)
    {
        if (latestBar is null || response.Results?.Count == 0 || latestBar.Timestamp <= response.Results.Last().Timestamp)
        {
            return;
        }

        switch (timespan)
        {
            case Timespan.minute:
                if (multiplier != 1)
                {
                    return; // Only add live bar for 1 minute aggregates
                }
                response.Results.Add(latestBar);
                break;
            case Timespan.hour:
                if (multiplier != 1)
                {
                    return; // Only add live bar for 1 hour aggregates
                }
                var last = response.Results.Last();

                if (last.Timestamp + (60 * 60000) < latestBar.Timestamp)
                {
                    response.Results.Add(latestBar);
                }
                else
                {
                    // Update the last bar with the latest data
                    last.Close = latestBar.Close;
                    last.High = Math.Max(last.High, latestBar.High);
                    last.Low = Math.Min(last.Low, latestBar.Low);
                    last.Volume += latestBar.Volume;
                    last.Vwap = (last.Close + last.High + last.Low) / 3;
                }
                break;
            case Timespan.day:
            case Timespan.week:
            case Timespan.month:
            case Timespan.quarter:
            case Timespan.year:
                return;
            default:
                throw new NotImplementedException();
        }
    }

    #endregion
}
