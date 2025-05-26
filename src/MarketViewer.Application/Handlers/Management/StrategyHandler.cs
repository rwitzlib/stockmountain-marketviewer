using MarketViewer.Contracts.Dtos;
using MarketViewer.Contracts.Mappers;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Requests.Management.Strategy;
using MarketViewer.Contracts.Responses.Management;
using MarketViewer.Core.Auth;
using MarketViewer.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MarketViewer.Application.Handlers.Management;

public class StrategyHandler(AuthContext authContext, IStrategyRepository repository, ILogger<StrategyHandler> logger)
{

    public async Task<OperationResult<StrategyResponse>> Create(StrategyPutRequest request)
    {
        try
        {
            //TODO: add fluent validation

            var strategy = new StrategyDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = authContext.UserId.ToString(),
                Name = request.Name,
                Enabled = request.Enabled,
                IsPublic = request.IsPublic,
                Type = request.Type,
                Integration = request.Integration,
                PositionInfo = request.PositionInfo,
                ExitInfo = new BacktestExitInformation
                {
                    ProfitTarget = request.ExitInfo?.ProfitTarget,
                    StopLoss = request.ExitInfo?.StopLoss,
                    Other = ScanArgumentMapper.ConvertFromScanArgumentDto(request.ExitInfo?.Other),
                    Timeframe = request.ExitInfo?.Timeframe,
                },
                Argument = ScanArgumentMapper.ConvertFromScanArgumentDto(request.Argument)
            };

            var strategyDto = await repository.Put(strategy);

            if (strategyDto == null)
            {
                logger.BeginScope("Failed to create strategy for user {UserId}", authContext.UserId);
                return new OperationResult<StrategyResponse>
                {
                    Status = HttpStatusCode.InternalServerError,
                    ErrorMessages = ["Failed to create strategy."]
                };
            }

            var response = new StrategyResponse
            {
                Id = strategyDto.Id,
                Name = strategyDto.Name,
                Integration = strategyDto.Integration,
                Type = strategyDto.Type,
                IsPublic = strategyDto.IsPublic,
                Enabled = strategyDto.Enabled,
                PositionInfo = strategyDto.PositionInfo,
                ExitInfo = new ExitInformationDto
                {
                    ProfitTarget = strategyDto.ExitInfo?.ProfitTarget,
                    StopLoss = strategyDto.ExitInfo?.StopLoss,
                    Other = ScanArgumentMapper.ConvertToScanArgumentDto(strategy.ExitInfo?.Other),
                    Timeframe = strategyDto.ExitInfo?.Timeframe,
                },
                Argument = ScanArgumentMapper.ConvertToScanArgumentDto(strategyDto.Argument)
            };

            return new OperationResult<StrategyResponse>
            {
                Status = HttpStatusCode.OK,
                Data = response,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create strategy for user {UserId}", authContext.UserId);
            return new OperationResult<StrategyResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Failed to create strategy."]
            };
        }
    }

    public async Task<OperationResult<StrategyResponse>> Get(string strategyId)
    {
        try
        {
            var strategy = await repository.Get(strategyId);

            if (strategy == null || (strategy.UserId != authContext.UserId && !strategy.IsPublic))
            {
                return new OperationResult<StrategyResponse>
                {
                    Status = HttpStatusCode.NotFound,
                    ErrorMessages = ["Strategy not found."]
                };
            }

            var response = new StrategyResponse
            {
                Id = strategy.Id,
                Name = strategy.Name,
                Integration = strategy.Integration,
                Type = strategy.Type,
                IsPublic = strategy.IsPublic,
                Enabled = strategy.Enabled,
                PositionInfo = strategy.PositionInfo,
                ExitInfo = new ExitInformationDto
                {
                    StopLoss = strategy.ExitInfo?.StopLoss,
                    ProfitTarget = strategy.ExitInfo?.ProfitTarget,
                    Other = ScanArgumentMapper.ConvertToScanArgumentDto(strategy.ExitInfo?.Other),
                    Timeframe = strategy.ExitInfo?.Timeframe,
                },
                Argument = ScanArgumentMapper.ConvertToScanArgumentDto(strategy.Argument)
            };

            return new OperationResult<StrategyResponse>
            {
                Status = HttpStatusCode.OK,
                Data = response,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get strategy for user {UserId}", authContext.UserId);
            return new OperationResult<StrategyResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Failed to get strategy."]
            };
        }
    }

    public async Task<OperationResult<IEnumerable<StrategyResponse>>> List(StrategyListRequest request)
    {
        try
        {
            if (!authContext.IsAuthenticated && !request.IsPublic)
            {
                return new OperationResult<IEnumerable<StrategyResponse>>
                {
                    Status = HttpStatusCode.OK,
                    Data = []
                };
            }

            var strategies = request.IsPublic switch
            {
                true => await repository.ListByPublic(request.IsPublic),
                false => await repository.ListByUser(authContext.UserId)
            };

            var response = new List<StrategyResponse>();
            foreach (var strategy in strategies)
            {
                response.Add(new StrategyResponse
                {
                    Id = strategy.Id,
                    Name = strategy.Name,
                    Integration = strategy.Integration,
                    Type = strategy.Type,
                    Enabled = strategy.Enabled,
                    IsPublic = strategy.IsPublic,
                    PositionInfo = strategy.PositionInfo,
                    ExitInfo = new ExitInformationDto
                    {
                        StopLoss = strategy.ExitInfo?.StopLoss,
                        ProfitTarget = strategy.ExitInfo?.ProfitTarget,
                        Other = ScanArgumentMapper.ConvertToScanArgumentDto(strategy.ExitInfo?.Other),
                        Timeframe = strategy.ExitInfo?.Timeframe,
                    },
                    Argument = ScanArgumentMapper.ConvertToScanArgumentDto(strategy.Argument)
                });
            }

            return new OperationResult<IEnumerable<StrategyResponse>>
            {
                Status = HttpStatusCode.OK,
                Data = response,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list strategies for user {UserId}", authContext.UserId);
            return new OperationResult<IEnumerable<StrategyResponse>>
            {
                Status = HttpStatusCode.OK,
                Data = []
            };
        }
    }

    public async Task<OperationResult<StrategyResponse>> Update(string id, StrategyPutRequest request)
    {
        try
        {
            var existingStrategy = await repository.Get(id);

            if (existingStrategy == null || existingStrategy.UserId != authContext.UserId)
            {
                return new OperationResult<StrategyResponse>
                {
                    Status = HttpStatusCode.NotFound,
                    ErrorMessages = ["Strategy not found."]
                };
            }

            var updatedstrategy = new StrategyDto
            {
                Id = id,
                UserId = authContext.UserId,
                Name = request.Name,
                Enabled = request.Enabled,
                IsPublic = request.IsPublic,
                Type = request.Type,
                Integration = request.Integration,
                PositionInfo = request.PositionInfo,
                ExitInfo = new BacktestExitInformation
                {
                    StopLoss = request.ExitInfo?.StopLoss,
                    ProfitTarget = request.ExitInfo?.ProfitTarget,
                    Other = ScanArgumentMapper.ConvertFromScanArgumentDto(request.ExitInfo?.Other),
                    Timeframe = request.ExitInfo?.Timeframe,
                },
                Argument = ScanArgumentMapper.ConvertFromScanArgumentDto(request.Argument)
            };

            var strategy = await repository.Put(updatedstrategy);

            if (strategy == null)
            {
                return new OperationResult<StrategyResponse>
                {
                    Status = HttpStatusCode.InternalServerError,
                    ErrorMessages = ["Failed to update strategy."]
                };
            }

            var response = new StrategyResponse
            {
                Id = strategy.Id,
                Name = strategy.Name,
                Integration = strategy.Integration,
                Type = strategy.Type,
                Enabled = strategy.Enabled,
                IsPublic = strategy.IsPublic,
                PositionInfo = strategy.PositionInfo,
                ExitInfo = new ExitInformationDto
                {
                    StopLoss = strategy.ExitInfo?.StopLoss,
                    ProfitTarget = strategy.ExitInfo?.ProfitTarget,
                    Other = ScanArgumentMapper.ConvertToScanArgumentDto(strategy.ExitInfo?.Other),
                    Timeframe = strategy.ExitInfo?.Timeframe,
                },
                Argument = ScanArgumentMapper.ConvertToScanArgumentDto(strategy.Argument)
            };

            return new OperationResult<StrategyResponse>
            {
                Status = HttpStatusCode.OK,
                Data = response,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update strategy for user {UserId}", authContext.UserId);
            return new OperationResult<StrategyResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Failed to update strategy."]
            };
        }
    }

    public async Task<OperationResult<bool>> Delete(string strategyId)
    {
        var strategy = await repository.Get(strategyId);

        if (strategy == null || strategy.UserId != authContext.UserId)
        {
            return new OperationResult<bool>
            {
                Status = HttpStatusCode.NotFound,
                ErrorMessages = ["Strategy not found."]
            };
        }

        var result = await repository.Delete(strategyId);

        if (!result)
        {
            return new OperationResult<bool>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Failed to delete strategy."]
            };
        }

        return new OperationResult<bool>
        {
            Status = HttpStatusCode.NoContent,
            Data = true
        };
    }
}
