using MarketViewer.Contracts.Responses;
using MarketViewer.Web.Contracts;
using MarketViewer.Web.Models;
using System.Text.Json;
using MarketViewer.Contracts.Requests;

namespace MarketViewer.Web.Services
{
    public class ChartService(HttpClient httpClient, ILogger<ChartService> logger)
    {
        public async Task<StocksResponse> GetStockData(StocksRequest request)
        {
            try
            {
                var parameters = new List<KeyValuePair<string, string>>
                {
                    new("Ticker", request.Ticker),
                    new("Multiplier", request.Multiplier.ToString()),
                    new("Timespan", request.Timespan.ToString()),
                    new("From", request.From.ToString()),
                    new("To", request.To.ToString())
                };

                if (request.Studies is not null)
                {
                    parameters.AddRange(from study in request.Studies 
                        let value = (string)null 
                        select study.Parameters is null ? study.Type.ToString() : $"{study.Type}:{string.Join(',', study.Parameters)}" 
                        into value 
                        select new KeyValuePair<string, string>("_study", value));
                }

                var formUrlEncodedContent = new FormUrlEncodedContent(parameters);

                var response = await httpClient.PostAsync("api/stocks", formUrlEncodedContent);

                var json = await response.Content.ReadAsStringAsync();
                var stocksResponse = JsonSerializer.Deserialize<StocksResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var qwer = DateTimeOffset.FromUnixTimeMilliseconds(stocksResponse.Results.First().Timestamp);
                var asdf = DateTimeOffset.FromUnixTimeMilliseconds(stocksResponse.Results.Last().Timestamp);

                foreach (var candle in stocksResponse.Results)
                {
                    candle.Timestamp = AdjustForLocalTime(candle.Timestamp);
                }

                if (stocksResponse.Studies is not null)
                {
                    foreach (var study in stocksResponse.Studies)
                    {
                        foreach (var line in study.Results)
                        {
                            foreach (var entry in line)
                            {
                                entry.Timestamp = AdjustForLocalTime(entry.Timestamp);
                            }
                        }
                    }
                }

                var first = DateTimeOffset.FromUnixTimeSeconds(stocksResponse.Results.First().Timestamp);
                var last = DateTimeOffset.FromUnixTimeSeconds(stocksResponse.Results.Last().Timestamp);

                return stocksResponse;
            }
            catch (Exception e)
            {
                logger.LogInformation($"Exception: {e.Message}");
                return null;
            }
        }

        private static long AdjustForLocalTime(long timestamp)
        {
            var date = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);

            var isDaylightSavings = TimeZoneInfo.Local.IsDaylightSavingTime(date);

            if (isDaylightSavings)
            {
                var offset = (long)DateTimeOffset.Now.Offset.TotalSeconds;

                var adjustedTimestamp = timestamp / 1000 + offset;
                return adjustedTimestamp;
            }
            else
            {
                var offset = (long)TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds;

                var adjustedTimestamp = timestamp / 1000 + offset;
                return adjustedTimestamp;
            }
        }

        public Volume CreateVolumeChart(VolumeParams volumeParams, StocksResponse stocksResponse)
        {
            var volumeChart = new Volume();

            for (var i = 1; i < stocksResponse.Results.Count(); i++)
            {
                var barColor = "#c7cbcf";
                if (volumeParams.UseDifferentColors)
                {
                    var currentClose = stocksResponse.Results.ToArray()[i].Close;
                    var previousClose = stocksResponse.Results.ToArray()[i - 1].Close;

                    if (currentClose > previousClose)
                    {
                        barColor = volumeParams.ColorUp.ToLowerInvariant();
                    }
                    else if (currentClose < previousClose)
                    {
                        barColor = volumeParams.ColorDown.ToLowerInvariant();
                    }
                }
                else
                {
                    barColor = volumeParams.UniformColor.ToLowerInvariant();
                }

                volumeChart.VolumeSeries.Add(new VolumeEntry
                {
                    Color = barColor,
                    Timestamp = stocksResponse.Results.ToArray()[i].Timestamp,
                    Value = stocksResponse.Results.ToArray()[i].Volume
                });
            }

            return volumeChart;
        }
    }
}
