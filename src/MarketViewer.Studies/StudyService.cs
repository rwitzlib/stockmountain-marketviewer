using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Study;
using Polygon.Client.Models;

namespace MarketViewer.Studies;

public static class StudyService
{
    public static StudyResponse ComputeStudy(StudyType studyType, string[] parameters, List<Bar> candles)
    {
        var response = new StudyResponse
        {
            Name = studyType.ToString().ToUpperInvariant(),
            Parameters = parameters,
            Results = studyType switch
            {
                StudyType.sma => SimpleMovingAverage.Compute(candles, parameters).Lines,
                StudyType.ema => ExponentialMovingAverage.Compute(candles, parameters).Lines,
                StudyType.vwap => VolumeWeightedAveragePrice.Compute(candles, null).Lines,
                StudyType.macd => MovingAverageConvergenceDivergence.Compute(candles, parameters).Lines,
                StudyType.rsi => RelativeStrengthIndex.Compute(candles, parameters).Lines,
                _ => null
            }
        };

        if (response.Results is null || !response.Results.Any())
        {
            return null;
        }

        return response;
    }
}