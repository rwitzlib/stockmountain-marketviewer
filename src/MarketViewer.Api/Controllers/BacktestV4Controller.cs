using System.Net;
using MarketViewer.Contracts.Requests.Backtest;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Quartz.Impl.AdoJobStore.Common;

namespace MarketViewer.Api.Controllers;

[ApiController]
[Route("api/backtest/v4")]
public class BacktestV4Controller(IMediator mediator, ILogger<BacktestV4Controller> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartBacktest([FromBody] BacktestRequestV3 request)
    {
        try
        {
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

    [HttpPost]
    [Route("asdf/asdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> asdf([FromBody] BacktestRequestV3 request)
    {
        try
        {
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
    public async Task<IActionResult> GetBacktestResult(string id)
    {
        var asdf = Request;

        return Ok();
        var request = new GetBacktestResultRequest();
        try
        {
            request.Id = id;

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

    [HttpPost]
    [Route("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListBacktestResults([FromBody] BacktestRequestV3 request) // TODO: add a new request type for listing backtest results
    {
        try
        {
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
