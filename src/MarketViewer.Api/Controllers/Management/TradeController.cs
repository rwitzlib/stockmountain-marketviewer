using MarketViewer.Api.Authorization;
using MarketViewer.Application.Handlers.Management;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Requests.Management.Trade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MarketViewer.Api.Controllers.Management;

[ApiController]
[Route("api/trade")]
public class TradeController(TradeHandler handler, ILogger<TradeController> logger) : Controller
{
    [HttpPost]
    [Authorize]
    [RequiredPermissions([UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
    public async Task<IActionResult> Open([FromBody] TradeOpenRequest request)
    {
        var response = await handler.Open(request);
        return response.Status switch
        {
            HttpStatusCode.OK => Ok(response.Data),
            HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
        };
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] TradeListRequest request)
    {
        var response = await handler.List(request);

        return response.Status switch
        {
            HttpStatusCode.OK => Ok(response.Data),
            HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
        };
    }

    [HttpPut("{id}")]
    [Authorize]
    [RequiredPermissions([UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
    public async Task<IActionResult> Close(string id, [FromBody] TradeCloseRequest request)
    {
        var response = await handler.Close(id, request);
        return response.Status switch
        {
            HttpStatusCode.OK => Ok(response.Data),
            HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
        };
    }
}
