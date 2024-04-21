using System.Text.Json;
using Xunit;
using FluentAssertions;
using System.Diagnostics;
using Xunit.Abstractions;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net;
using Polygon.Client.Responses;

namespace MarketViewer.Infrastructure.UnitTests.Performance
{
    public class PerformanceUnitTests
    {
        private readonly ITestOutputHelper _output;

        #region Constructor
        public PerformanceUnitTests(ITestOutputHelper output)
        {
            _output = output;
        }
        #endregion

        // [Fact]
        // public async Task Read_And_Serialize_S3_Backtest_File()
        // {
        //     var sp = new Stopwatch();
        //     sp.Start();

        //     using var client = new AmazonS3Client(Amazon.RegionEndpoint.USEast2);

        //     var response = await client.GetObjectAsync(new GetObjectRequest
        //     {
        //         BucketName = "lad-dev-marketviewer",
        //         Key = "backtest/2023/9/9-29.json"
        //     });

        //     response.HttpStatusCode.Should().Be(HttpStatusCode.OK);

        //     using var reader = new StreamReader(response.ResponseStream);
        //     var responseBody = reader.ReadToEnd();

        //     responseBody.Should().NotBeNullOrWhiteSpace();

        //     var aggregates = JsonSerializer.Deserialize<List<PolygonAggregateResponse>>(responseBody);

        //     sp.Stop();

        //     sp.ElapsedMilliseconds.Should().BeLessThan(15000);
        //     aggregates.Should().NotBeNull();
        // }
    }
}
