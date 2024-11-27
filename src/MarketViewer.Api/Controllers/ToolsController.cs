using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers;

[ApiController]
[Route("api/tools/s3")]
public class ToolsController(IAmazonS3 s3Client, ILogger<ToolsController> logger) : ControllerBase
{
    [HttpGet("{bucketName}/{objectKey}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Backtest(string bucketName, string objectKey)
    {
        try
        {
            var response = await s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey
            });

            if (response.ContentLength <= 0)
            {
                return NotFound("Not Found.");
            }

            using var streamReader = new StreamReader(response.ResponseStream);

            var json = await streamReader.ReadToEndAsync();

            return Ok(json);
        }
        catch (Exception e)
        {
            logger.LogError("Exception: {message}", e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new List<string> { "Internal error."});
        }
    }
}
