using System.Net.Http.Json;
using System.Text.Json;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Web.Services
{
    public class ScannerService(HttpClient httpClient, ILogger<ScannerService> logger)
    {
        readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<ScanResponse> ScanAsync(ScanRequest request)
        {
            try
            {
                logger.LogInformation($"Initial date: {request.Timestamp}");


                var date = (DateTimeOffset)request.Timestamp.UtcDateTime;
                request.Timestamp = date;

                logger.LogInformation($"Modified date: {request.Timestamp}");
                var response = await httpClient.PostAsJsonAsync("api/scan", request);

                var json = await response.Content.ReadAsStringAsync();
                var scanResponse = JsonSerializer.Deserialize<ScanResponse>(json, _options);

                return scanResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ScanResponse
                {
                    Items = Enumerable.Empty<ScanResponse.Item>()
                };
            }
        }

        public async Task<ScanResponse> ScanV2Async(ScanV2Request request)
        {
            try
            {
                logger.LogInformation("Initial date: {timestamp}", request.Timestamp);


                var date = (DateTimeOffset)request.Timestamp.UtcDateTime;
                request.Timestamp = date;

                logger.LogInformation("Modified date: {timestamp}", request.Timestamp);
                var response = await httpClient.PostAsJsonAsync("api/scan/v2", request);

                var json = await response.Content.ReadAsStringAsync();
                var scanResponse = JsonSerializer.Deserialize<ScanResponse>(json, _options);

                return scanResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ScanResponse
                {
                    Items = Enumerable.Empty<ScanResponse.Item>()
                };
            }
        }
    }
}
