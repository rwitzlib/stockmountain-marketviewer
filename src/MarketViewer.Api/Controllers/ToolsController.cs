using MarketViewer.Api.Authorization;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarketViewer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ToolsController(IHttpContextAccessor contextAccessor, IMarketCache marketCache, IMemoryCache memoryCache, ILogger<StocksController> logger) : ControllerBase
{
    [HttpGet]
    [Route("aggregate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [RequiredPermissions([UserRole.Admin])]
    public IActionResult Aggregate([FromQuery] ToolsAggregateRequest request)
    {
        try
        {
            request.UserId = contextAccessor.HttpContext.Items["UserId"].ToString();

            var response = marketCache.GetStocksResponse(request.Ticker, request.Timespan, DateTimeOffset.Now);

            return Ok(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error." });
        }
    }

    [HttpGet]
    [Route("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [RequiredPermissions([UserRole.Admin])]
    public IActionResult Stats()
    {
        try
        {
            contextAccessor.HttpContext.Items["UserId"].ToString();

            var stats = memoryCache.GetCurrentStatistics();
            var tickers = marketCache.GetTickers();

            var response = new StatsResponse
            {
                CacheStatistics = stats,
                TickerCount = tickers is null ? 0 : tickers.Count(),
                StocksResponseCount = 0
            };

            foreach (var ticker in tickers)
            {
                var minuteResponse = marketCache.GetStocksResponse(ticker, Timespan.minute, DateTimeOffset.Now);
                var hourResponse = marketCache.GetStocksResponse(ticker, Timespan.hour, DateTimeOffset.Now);

                if (minuteResponse is not null)
                {
                    response.StocksResponseCount++;      
                }

                if (hourResponse is not null)
                {
                    response.StocksResponseCount++;
                }
            }

            return Ok(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error." });
        }
    }
}
