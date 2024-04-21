using System.Net.Http.Json;
using System.Text.Json;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Web.Services
{
    public class ScannerService(HttpClient httpClient, ILogger<ScannerService> logger)
    {
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
                var scanResponse = JsonSerializer.Deserialize<ScanResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

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
