using System.Net;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Requests.Scan;
using MarketViewer.Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers
{
    [ApiController]
    [Route("api/scan")]
    public class ScanController(IAmazonS3 s3Client, IMarketCache _marketCache, ILogger<ScanController> _logger, IMediator _mediator) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HandleScanRequest([FromBody] ScanRequest request)
        {
            try
            {
                var response = await _mediator.Send(request);

                return response.Status switch
                {
                    HttpStatusCode.OK => Ok(response.Data),
                    HttpStatusCode.BadRequest => BadRequest(response.ErrorMessages),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, response.ErrorMessages)
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error."});
            }
        }

        [HttpPost("v2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Scan([FromBody] ScanV2Request request)
        {
            try
            {
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
        [Route("v2/print")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Print()
        {
            var minuteTickers = _marketCache.GetTickersByTimespan(Timespan.minute, DateTimeOffset.Now);
            var hourTickers = _marketCache.GetTickersByTimespan(Timespan.hour, DateTimeOffset.Now);

            if (!(minuteTickers.Any() || hourTickers.Any()))
            {
                return NotFound();
            }

            List<StocksResponse> minuteStocks = [];
            foreach (var ticker in minuteTickers)
            {
                var stocksResponse = _marketCache.GetStocksResponse(ticker, Timespan.minute, DateTimeOffset.Now);
                var adjustedStocksResponse = new StocksResponse
                {
                    Ticker = stocksResponse.Ticker,
                    Results = stocksResponse.Results.Where(x => DateTimeOffset.FromUnixTimeMilliseconds(x.Timestamp).ToOffset(DateTimeOffset.Now.Offset).Date == DateTimeOffset.Now.Date).ToList()
                };
                minuteStocks.Add(adjustedStocksResponse);
            }

            List<StocksResponse> hourStocks = [];
            foreach (var ticker in hourTickers)
            {
                var stocksResponse = _marketCache.GetStocksResponse(ticker, Timespan.hour, DateTimeOffset.Now);
                var adjustedStocksResponse = new StocksResponse
                {
                    Ticker = stocksResponse.Ticker,
                    Results = stocksResponse.Results.Where(x => DateTimeOffset.FromUnixTimeMilliseconds(x.Timestamp).ToOffset(DateTimeOffset.Now.Offset).Date == DateTimeOffset.Now.Date).ToList()
                };
                hourStocks.Add(adjustedStocksResponse);
            }

            if (!(minuteTickers.Any() || hourTickers.Any()))
            {
                return NotFound();
            }

            List<(string, string)> items =
            [
                ($"{DateTimeOffset.Now.Date:yyyy-MM-dd}-m-ticker.json", JsonSerializer.Serialize(minuteTickers)),
                ($"{DateTimeOffset.Now.Date:yyyy-MM-dd}-h-ticker.json", JsonSerializer.Serialize(hourTickers)),
                ($"{DateTimeOffset.Now.Date:yyyy-MM-dd}-m-stocks.json", JsonSerializer.Serialize(minuteStocks)),
                ($"{DateTimeOffset.Now.Date:yyyy-MM-dd}-h-stocks.json", JsonSerializer.Serialize(hourStocks))
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
            return Ok();
        }
    }
}
