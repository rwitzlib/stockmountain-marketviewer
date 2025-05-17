using System.Net;
using MarketViewer.Api.Authorization;
using MarketViewer.Contracts.Requests.Backtest;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/backtest")]
public class BacktestController(IHttpContextAccessor contextAccessor, IMediator mediator, ILogger<BacktestController> logger) : ControllerBase
{
    [HttpPost]
    [Route("v3")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [RequiredPermissions([UserRole.Admin])]
    public async Task<IActionResult> BacktestV3([FromBody] BacktestRequestV3 request)
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
}
