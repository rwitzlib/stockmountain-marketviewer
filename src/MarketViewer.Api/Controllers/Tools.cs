using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers;

[ApiController]
[Route("api/tools")]
public class ToolsController(IMarketCache marketCache, ILogger<StocksController> logger) : ControllerBase
{
    [HttpGet]
    [Route("aggregate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Aggregate([FromQuery] ToolsAggregateRequest request)
    {
        try
        {
            var response = marketCache.GetStocksResponse(request.Ticker, request.Timespan, DateTimeOffset.Now);

            return Ok(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error." });
        }
    }
}
