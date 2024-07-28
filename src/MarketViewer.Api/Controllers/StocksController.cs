using System.Net;
using MarketViewer.Contracts.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StocksController(ILogger<StocksController> logger, IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HandleAggregateRequest([FromBody] StocksRequest request)
        {
            try
            {
                var response = await mediator.Send(request);

                foreach (var errorMessage in response.ErrorMessages)
                {
                    logger.LogInformation("Error: {error}", errorMessage);
                }

                return response.Status switch
                {
                    HttpStatusCode.OK => Ok(response.Data),
                    HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error." });
            }
        }
    }
}
