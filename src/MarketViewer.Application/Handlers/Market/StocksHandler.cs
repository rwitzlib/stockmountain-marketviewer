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
            response = marketCache.GetStocksResponse(request.Ticker, timeframe, DateTimeOffset.Now);
            
            // If cache miss, fall back to repository
            if (response == null)
            {
                response = await repository.GetStockDataAsync(request);
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
            }
        }

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
    #endregion
}
