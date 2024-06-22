using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Study;
using Polygon.Client.Models;

namespace MarketViewer.Studies;

public static class StudyService
{
    public static StudyResponse ComputeStudy(StudyType studyType, string[] parameters, Bar[] candleData)
    {
        var response = new StudyResponse
        {
            Name = studyType.ToString().ToUpperInvariant(),
            Parameters = parameters,
            Results = studyType switch
            {
                StudyType.ema => ExponentialMovingAverage.Compute(candleData, parameters).Lines,
                StudyType.macd => MovingAverageConvergenceDivergence.Compute(candleData, parameters).Lines,
                StudyType.sma => SimpleMovingAverage.Compute(candleData, parameters).Lines,
                StudyType.rsi => RelativeStrengthIndex.Compute(candleData, parameters).Lines,
                StudyType.vwap => VolumeWeightedAveragePrice.Compute(candleData, null).Lines,
                _ => null
            }
        };

        if (response.Results is null || response.Results.Count == 0)
        {
            return null;
        }

        return response;
    }
}