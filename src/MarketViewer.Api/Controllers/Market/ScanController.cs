using System.Net;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using MarketViewer.Api.Authorization;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Requests.Market.Scan;
using MarketViewer.Contracts.Responses.Tools;
using MarketViewer.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarketViewer.Api.Controllers.Market
{
    [ApiController]
    [Authorize]
    [Route("api/scan")]
    public class ScanController(IHttpContextAccessor contextAccessor, IMemoryCache memoryCache, IAmazonS3 s3Client, IMarketCache _marketCache, ILogger<ScanController> _logger, IMediator _mediator) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [RequiredPermissions([UserRole.None, UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
        public async Task<IActionResult> Scan([FromBody] ScanRequest request)
        {
            try
            {
                request.UserId = contextAccessor.HttpContext.Items["UserId"].ToString();

                var response = await _mediator.Send(request);

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
                _logger.LogError(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error." });
            }
        }

        [HttpGet]
        [Route("print")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [RequiredPermissions([UserRole.None, UserRole.Basic, UserRole.Advanced, UserRole.Premium, UserRole.Admin])]
        public async Task<IActionResult> Print()
        {
            //var minuteTickers = _marketCache.GetTickersByTimeframe(new Timeframe(1, Timespan.minute), DateTimeOffset.Now);
            //var hourTickers = _marketCache.GetTickersByTimeframe(new Timeframe(1, Timespan.hour), DateTimeOffset.Now);

            //if (!(minuteTickers.Any() || hourTickers.Any()))
            //{
            //    return NotFound();
            //}

            //List<StocksResponse> minuteStocks = [];
            //foreach (var ticker in minuteTickers)
            //{
            //    var stocksResponse = _marketCache.GetStocksResponse(ticker, new Timeframe(1, Timespan.minute), DateTimeOffset.Now);
            //    minuteStocks.Add(stocksResponse);
            //}

            //List<StocksResponse> hourStocks = [];
            //foreach (var ticker in hourTickers)
            //{
            //    var stocksResponse = _marketCache.GetStocksResponse(ticker, new Timeframe(1, Timespan.hour), DateTimeOffset.Now);
            //    hourStocks.Add(stocksResponse);
            //}

            //if (!(minuteTickers.Any() || hourTickers.Any()))
            //{
            //    return NotFound();
            //}

            List<(string, string)> items =
            [
                //($"{DateTimeOffset.Now.Date:yyyy-MM-dd}-m-ticker.json", JsonSerializer.Serialize(minuteTickers)),
                //($"{DateTimeOffset.Now.Date:yyyy-MM-dd}-h-ticker.json", JsonSerializer.Serialize(hourTickers)),
                ($"{DateTimeOffset.Now.Date:yyyy-MM-dd}-m-stocks.json", JsonSerializer.Serialize(memoryCache.Get<PolygonFidelity>("SPY_minute"))),
                ($"{DateTimeOffset.Now.Date:yyyy-MM-dd}-h-stocks.json", JsonSerializer.Serialize(memoryCache.Get<PolygonFidelity>("SPY_hour")))
            ];

            foreach (var path in items)
            {
                var response = await s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = "lad-dev-marketviewer",
                    Key = path.Item1,
                    ContentBody = path.Item2
                });
            }

            return Ok(new PolygonFidelityResponse
            {
                Minute = memoryCache.Get<PolygonFidelity>("SPY_minute"),
                Hour = memoryCache.Get<PolygonFidelity>("SPY_hour")
            });
        }
    }
}
