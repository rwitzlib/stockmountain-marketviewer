using System.Collections.Generic;
using System.Net;
using MarketDataProvider.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NRedisStack.Graph;

namespace MarketViewer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScannerController(ILogger<ScannerController> logger, IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [Route("v1")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Scan([FromBody] ScannerRequest request)
        {
            try
            {
                var asdf = new ScannerRequest
                {
                    Arguments = new ScanArgument
                    {
                        Operator = "AND",
                        Filters = [
                            new ScanFilter 
                            {
                                First = new PriceActionOperand
                                {
                                    Multiplier = 5,
                                    Timespan = Timespan.minute
                                },
                                Operator = "GT",
                                Second = new ValueOperand
                                {
                                    Value = 5
                                }
                            }
                        ]
                    },
                    Timestamp = "asdf"
                };

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
                logger.LogError(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error."});
            }
        }
    }
}
