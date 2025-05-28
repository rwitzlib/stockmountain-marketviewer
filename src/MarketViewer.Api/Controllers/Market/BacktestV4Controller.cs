using System.Net;
using MarketViewer.Api.Authorization;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Requests.Market.Backtest;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers.Market;

[ApiController]
[Authorize]
[Route("api/backtest/v4")]
public class BacktestV4Controller(IHttpContextAccessor contextAccessor, IMediator mediator, ILogger<BacktestV4Controller> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [RequiredPermissions([UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
    public async Task<IActionResult> StartBacktest([FromBody] StartBacktestRequest request)
    {
        try
        {
            request.UserId = contextAccessor.HttpContext.Items["UserId"].ToString();

            var response = await mediator.Send(request);

            return response.Status switch
            {
                HttpStatusCode.OK => Ok(response.Data),
                HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
                _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
            };
        }
        catch (Exception e)
        {
            logger.LogError("Exception: {message}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error." });
        }
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [RequiredPermissions([UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
    public async Task<IActionResult> GetBacktest(string id)
    {
        try
        {
            var userId = contextAccessor.HttpContext.Items["UserId"].ToString();

            var response = await mediator.Send(new GetBacktestRequest
            {
                Id = id,
                UserId = userId
            });

            return response.Status switch
            {
                HttpStatusCode.OK => Ok(response.Data),
                HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
                _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
            };
        }
        catch (Exception e)
        {
            logger.LogError("Exception: {message}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error." });
        }
    }
}
