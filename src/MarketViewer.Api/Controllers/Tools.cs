using MarketViewer.Api.Authorization;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tools")]
public class ToolsController(IHttpContextAccessor contextAccessor, IMarketCache marketCache, ILogger<StocksController> logger) : ControllerBase
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
}
