using MarketViewer.Contracts.Requests.Backtest;
using MarketViewer.Contracts.Responses.Backtest;
using System.Net.Http.Json;
using System.Text.Json;

namespace MarketViewer.Web.Services
{
    public class BacktestService(HttpClient httpClient, ILogger<BacktestService> logger)
    {
        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<BacktestResponse> BacktestAsync(BacktestRequest request)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/backtest", request);

                var json = await response.Content.ReadAsStringAsync();
                var strategyResponse = JsonSerializer.Deserialize<BacktestResponse>(json, _options);

                return strategyResponse;
            }
            catch (Exception ex)
            {
                logger.LogError("Backtesting error: {message}", ex.Message);
                return new BacktestResponse
                {
                    Results = [],
                    ResultsCount = 0
                };
            }
        }

        public async Task<BacktestV3Response> BacktestV3Async(BacktestV3Request request)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("api/backtest/v3", request);

                var json = await response.Content.ReadAsStringAsync();
                var strategyResponse = JsonSerializer.Deserialize<BacktestV3Response>(json, _options);

                return strategyResponse;
            }
            catch (Exception ex)
            {
                logger.LogError("Backtesting error: {message}", ex.Message);
                return new BacktestV3Response
                {
                    Results = [],
                };
            }
        }
    }
}
