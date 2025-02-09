using MarketViewer.Contracts.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MarketViewer.Contracts.Interfaces;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Requests;
using MarketViewer.Studies;
using System.Linq;
using MarketViewer.Contracts.Models.Study;
using System.Reflection.Metadata;
using Polygon.Client.Models;

namespace MarketViewer.Application.Handlers;

public class StocksHandler(IMarketDataRepository repository) : IRequestHandler<StocksRequest, OperationResult<StocksResponse>>
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

        var response = await repository.GetStockDataAsync(request);

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
            var candles = response.Results;

            foreach (var study in request.Studies)
            {
                var result = StudyService.ComputeStudy(study.Type, study.Parameters, candles);

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

        return errorMessages.Count == 0;
    }

    private static StudyResponse ComputeStudy(StudyFields study, List<Bar> candles)
    {
        var result = StudyService.ComputeStudy(study.Type, study.Parameters, candles);

        if (result is not null && result.Results.Count > 0)
        {
            return result;
        }

        return null;
    }
    #endregion
}
