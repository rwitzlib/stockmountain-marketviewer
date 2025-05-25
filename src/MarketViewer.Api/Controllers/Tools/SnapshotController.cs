using System.Net;
using MarketViewer.Api.Authorization;
using MarketViewer.Contracts.Requests.Tools;
using MarketViewer.Contracts.Responses.Tools;
using MarketViewer.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarketViewer.Api.Controllers.Tools;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SnapshotController(IMemoryCache memoryCache, ILogger<SnapshotController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [RequiredPermissions([UserRole.Admin])]
    public IActionResult HandleSnapshot([FromQuery] SnapshotRequest request)
    {
        try
        {
            var snapshotResponse = memoryCache.Get<SnapshotResponse>("snapshot");

            return Ok(new SnapshotResponse
            {
                Entries = snapshotResponse != null ? snapshotResponse.Entries.Where(q => q.Ticker == request.Ticker).ToList() : []
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error." });
        }
    }
}