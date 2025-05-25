using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Study;
using MarketViewer.Contracts.Responses.Market;
using MarketViewer.Studies.Studies;

namespace MarketViewer.Studies;

public class StudyFactory(
    SMA sma,
    EMA ema,
    MACD macd,
    RSI rsi,
    VWAP vwap,
    RVOL rvol)
{
    public StudyResponse Compute(StudyType studyType, string[] parameters, StocksResponse stocksResponse)
    {
        var results = studyType switch
        {
            StudyType.sma => sma.Compute(parameters, ref stocksResponse),
            StudyType.ema => ema.Compute(parameters, ref stocksResponse),
            StudyType.vwap => vwap.Compute(parameters, ref stocksResponse),
            StudyType.macd => macd.Compute(parameters, ref stocksResponse),
            StudyType.rsi => rsi.Compute(parameters, ref stocksResponse),
            StudyType.rvol => rvol.Compute(parameters, ref stocksResponse),
            _ => []
        };

        if (results.Count == 0)
        {
            return null;
        }

        var response = new StudyResponse
        {
            Name = studyType.ToString().ToUpperInvariant(),
            Parameters = parameters,
            Results = results
        };

        return response;
    }
}