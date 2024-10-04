using System.Net;
using MarketViewer.Contracts.Requests.Backtest;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers
{
    [ApiController]
    [Route("api/backtest")]
    public class BacktestV3Controller(IMediator mediator, ILogger<BacktestV3Controller> logger) : ControllerBase
    {
        [HttpPost]
        [Route("v3")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Backtest([FromBody] BacktestV3Request request)
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
                return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error."});
            }
        }
    }
}
