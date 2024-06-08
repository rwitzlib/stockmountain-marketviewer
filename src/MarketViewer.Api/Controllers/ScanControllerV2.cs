using System.Net;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScannerController(ILogger<ScannerController> logger, IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [Route("v2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Scan([FromBody] ScanRequestV2 request)
        {
            try
            {
                var asdf = new ScanRequestV2
                {
                    Argument = new ScanArgument
                    {
                        Operator = "AND",
                        Filters = [
                            new FilterV2
                            {
                                FirstOperand = new PriceActionOperand
                                {
                                    Multiplier = 5,
                                    Timespan = Timespan.minute
                                },
                                Operator = FilterOperator.gt,
                                SecondOperand = new ValueOperand
                                {
                                    Value = 5
                                },
                                Timeframe = new Timeframe
                                {
                                    Multiplier = 5,
                                    Timespan = Timespan.minute
                                }
                            }
                        ],
                        Argument = new ScanArgument
                        {
                            Operator = "OR",
                            Filters = [
                                new FilterV2{

                                }
                            ]
                        }
                    },
                    Timestamp = DateTimeOffset.Now
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
