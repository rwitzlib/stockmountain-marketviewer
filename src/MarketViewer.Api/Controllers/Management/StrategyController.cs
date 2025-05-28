using MarketViewer.Api.Authorization;
using MarketViewer.Application.Handlers.Management;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Requests.Management.Strategy;
using MarketViewer.Contracts.Responses.Management;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MarketViewer.Api.Controllers.Management;

[ApiController]
[Route("api/[controller]")]
public class StrategyController(StrategyHandler handler) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [RequiredPermissions([UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
    public async Task<ActionResult<StrategyResponse>> Create(StrategyPutRequest request)
    {
        var strategy = await handler.Create(request);

        return strategy.Status switch
        {
            HttpStatusCode.OK => CreatedAtAction(nameof(Get), new { id = strategy.Data.Id }, strategy.Data),
            HttpStatusCode.BadRequest => BadRequest(strategy.ErrorMessages),
            HttpStatusCode.NotFound => NotFound(strategy.ErrorMessages),
            _ => StatusCode(StatusCodes.Status500InternalServerError, strategy.ErrorMessages)
        };
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StrategyResponse>> Get(string id)
    {
        var strategy = await handler.Get(id);

        return strategy.Status switch
        {
            HttpStatusCode.OK => Ok(strategy.Data),
            HttpStatusCode.BadRequest => BadRequest(strategy.ErrorMessages),
            HttpStatusCode.NotFound => NotFound(strategy.ErrorMessages),
            _ => StatusCode(StatusCodes.Status500InternalServerError, strategy.ErrorMessages)
        };
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StrategyResponse>>> List([FromQuery] StrategyListRequest request)
    {
        var strategies = await handler.List(request);

        return strategies.Status switch
        {
            HttpStatusCode.OK => Ok(strategies.Data),
            HttpStatusCode.BadRequest => BadRequest(strategies.ErrorMessages),
            HttpStatusCode.NotFound => NotFound(strategies.ErrorMessages),
            _ => StatusCode(StatusCodes.Status500InternalServerError, strategies.ErrorMessages)
        };
    }

    [HttpPut("{id}")]
    [Authorize]
    [RequiredPermissions([UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
    public async Task<IActionResult> Update(string id, StrategyPutRequest request)
    {
        var strategy = await handler.Update(id, request);

        return strategy.Status switch
        {
            HttpStatusCode.OK => Ok(strategy.Data),
            HttpStatusCode.BadRequest => BadRequest(strategy.ErrorMessages),
            HttpStatusCode.NotFound => NotFound(strategy.ErrorMessages),
            _ => StatusCode(StatusCodes.Status500InternalServerError, strategy.ErrorMessages)
        };
    }

    [HttpDelete("{id}")]
    [Authorize]
    [RequiredPermissions([UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await handler.Delete(id);

        return result.Status switch {
            HttpStatusCode.NoContent => NoContent(),
            HttpStatusCode.NotFound => NotFound("Strategy not found."),
            _ => BadRequest(result.ErrorMessages)
        };
    }
}