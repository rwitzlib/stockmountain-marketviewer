using MarketViewer.Contracts.Enums.Strategy;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Records;
using MarketViewer.Contracts.Requests.Management.Trade;
using MarketViewer.Core.Auth;
using MarketViewer.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MarketViewer.Application.Handlers.Management;

public class TradeHandler(AuthContext authContext, ITradeRepository tradeRepository, IUserRepository userRepository, IStrategyRepository strategyRepository, ILogger<TradeHandler> logger)
{
    public async Task<OperationResult<bool>> Open(TradeOpenRequest request)
    {
        try
        {
            //TODO fluent validation

            var tradeRecord = new TradeRecord
            {
                Id = Guid.NewGuid().ToString(),
                UserId = authContext.UserId,
                StrategyId = request.StrategyId,
                Ticker = request.Ticker,
                Type = request.Type,
                OrderStatus = TradeStatus.Open,
                OpenedAt = DateTimeOffset.Now.ToString(),
                EntryPrice = request.EntryPrice,
                EntryPosition = request.EntryPosition,
                Shares = request.Shares,
            };

            var result = await tradeRepository.Put(tradeRecord);

            if (!result)
            {
                return new OperationResult<bool>
                {
                    Status = HttpStatusCode.InternalServerError,
                    ErrorMessages = ["An error occurred while opening the trade."]
                };
            }

            return new OperationResult<bool>
            {
                Status = HttpStatusCode.OK,
                Data = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError("Error creating trade: {Message}", ex.Message);
            return new OperationResult<bool>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["An error occurred while opening the trade."]
            };
        }
    }

    public async Task<OperationResult<IEnumerable<TradeRecord>>> List(TradeListRequest request)
    {
        try
        {
            // TODO add fluent validation
            if (request.User is not null)
            {
                if (authContext.UserId != request.User)
                {
                    var user = await userRepository.Get(request.User);

                    if (user == null || (!user.IsPublic && user.Id != authContext.UserId))
                    {
                        return new OperationResult<IEnumerable<TradeRecord>>
                        {
                            Status = HttpStatusCode.NotFound,
                            ErrorMessages = ["No trades found."]
                        };
                    }
                }

                var trades = await tradeRepository.ListTradesByUser(request.User, request.Type, request.Status);

                if (trades == null || !trades.Any())
                {
                    return new OperationResult<IEnumerable<TradeRecord>>
                    {
                        Status = HttpStatusCode.NotFound,
                        ErrorMessages = ["No trades found."]
                    };
                }

                return new OperationResult<IEnumerable<TradeRecord>>
                {
                    Status = HttpStatusCode.OK,
                    Data = trades
                };
            }
            else if (request.Strategy is not null)
            {
                var strategy = await strategyRepository.Get(request.Strategy);

                if (strategy == null || (!strategy.IsPublic && strategy.UserId != authContext.UserId))
                {
                    return new OperationResult<IEnumerable<TradeRecord>>
                    {
                        Status = HttpStatusCode.NotFound,
                        ErrorMessages = ["No trades found."]
                    };
                }

                var trades = await tradeRepository.ListTradesByStrategy(request.Strategy, request.Type, request.Status);

                if (trades == null || !trades.Any())
                {
                    return new OperationResult<IEnumerable<TradeRecord>>
                    {
                        Status = HttpStatusCode.NotFound,
                        ErrorMessages = ["No trades found."]
                    };
                }

                return new OperationResult<IEnumerable<TradeRecord>>
                {
                    Status = HttpStatusCode.OK,
                    Data = trades
                };
            }
            else
            {
                return new OperationResult<IEnumerable<TradeRecord>>
                {
                    Status = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Invalid request. Please provide a user or strategy."]
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error listing trades: {Message}", ex.Message);
            return new OperationResult<IEnumerable<TradeRecord>>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["An error occurred while listing trades."]
            };
        }
    }

    public async Task<OperationResult<bool>> Close(string tradeId, TradeCloseRequest request)
    {
        try
        {
            var existingTrade = await tradeRepository.Get(tradeId);

            if (existingTrade == null)
            {
                return new OperationResult<bool>
                {
                    Status = HttpStatusCode.NotFound,
                    ErrorMessages = ["Trade not found."]
                };
            }

            if (existingTrade.UserId != authContext.UserId)
            {
                return new OperationResult<bool>
                {
                    Status = HttpStatusCode.Forbidden,
                    ErrorMessages = ["Trade not found."]
                };
            }

            var trade = new TradeRecord
            {
                Id = tradeId,
                UserId = authContext.UserId,
                StrategyId = existingTrade.StrategyId,
                Ticker = existingTrade.Ticker,
                Type = existingTrade.Type,
                OrderStatus = TradeStatus.Closed,
                OpenedAt = existingTrade.OpenedAt,
                ClosedAt = DateTimeOffset.Now.ToString(),
                EntryPrice = existingTrade.EntryPrice,
                EntryPosition = existingTrade.EntryPosition,
                Shares = existingTrade.Shares,
                ClosePrice = request.ClosePrice,
                ClosePosition = request.ClosePosition,
                Profit = request.Profit
            };

            var result = await tradeRepository.Put(trade);

            if (!result)
            {
                return new OperationResult<bool>
                {
                    Status = HttpStatusCode.InternalServerError,
                    ErrorMessages = ["An error occurred while closing the trade."]
                };
            }

            return new OperationResult<bool>
            {
                Status = HttpStatusCode.OK,
                Data = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError("Error creating trade: {Message}", ex.Message);
            return new OperationResult<bool>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["An error occurred while closing the trade."]
            };
        }
    }
}