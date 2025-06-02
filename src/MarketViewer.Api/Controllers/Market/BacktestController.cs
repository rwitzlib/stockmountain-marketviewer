using System.Net;
using MarketViewer.Api.Authorization;
using MarketViewer.Application.Handlers.Market.Backtest;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Requests.Market.Backtest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers.Market;

[ApiController]
[Authorize]
[Route("api/backtest")]
public class BacktestController(IHttpContextAccessor contextAccessor, BacktestHandler handler, ILogger<BacktestController> logger) : ControllerBase
{
    [HttpPost]
    [RequiredPermissions([UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
    public async Task<IActionResult> StartBacktest([FromBody] BacktestCreateRequest request)
    {
        request.UserId = contextAccessor.HttpContext.Items["UserId"].ToString();

        var response = await handler.Create(request);

        return response.Status switch
        {
            HttpStatusCode.OK => Ok(response.Data),
            HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
        };
    }

    [HttpGet]
    [Route("{id}")]
    [RequiredPermissions([UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
    public async Task<IActionResult> GetBacktestEntry(string id)
    {
        var response = await handler.GetEntry(id);

        return response.Status switch
        {
            HttpStatusCode.OK => Ok(response.Data),
            HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
        };
    }

    [HttpGet]
    [RequiredPermissions([UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
    public async Task<IActionResult> ListBacktestEntries()
    {
        var userId = contextAccessor.HttpContext.Items["UserId"].ToString();

        var response = await handler.List(userId);

        return response.Status switch
        {
            HttpStatusCode.OK => Ok(response.Data),
            HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
        };
    }
}
