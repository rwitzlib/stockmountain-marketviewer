using System.Net;
using MarketViewer.Contracts.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers
{
    [ApiController]
    [Route("api/scan")]
    public class ScanControllerV2(ILogger<ScanControllerV2> logger, IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [Route("v2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Scan([FromBody] ScanV2Request request)
        {
            try
            {
                var response = await mediator.Send(request);

                return response.Status switch
                {
                    HttpStatusCode.OK => Ok(response.Data),
                    HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
                    HttpStatusCode.NotFound => NotFound(response.ErrorMessages),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error."});
            }
        }
    }
}
