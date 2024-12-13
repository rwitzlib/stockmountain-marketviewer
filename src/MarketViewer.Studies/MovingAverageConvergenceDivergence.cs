using MarketViewer.Contracts.Models.Study;
using Polygon.Client.Models;

namespace MarketViewer.Studies;

public class MovingAverageConvergenceDivergence : Study<MovingAverageConvergenceDivergence>
{
    private static int FastWeight { get; set; } = 9;
    private static int SlowWeight { get; set; } = 26;
    private static int SignalWeight { get; set; } = 9;
    private static string Type { get; set; } = "ema";
    private static string[] ValidTypes { get; set; } = ["sma", "ema", "wilders"];

    #region Protected Methods

    protected override List<List<LineEntry>> Initialize(Bar[] candles)
    {
        var macdSeries = new List<LineEntry>();
        var signalSeries = new List<LineEntry>();

        if (candles.Length < FastWeight || candles.Length < SlowWeight || candles.Length < SignalWeight)
        {
            ErrorMessages.Add("Not enough candle data.");
            return [
                macdSeries,
                signalSeries
            ];
        }

        List<float> fastValues = [];
        List<float> slowValues = [];
        List<float> macdValues = [];
        List<float> signalValues = [];

        for (int i = 0; i < candles.Length; i++)
        {
            if (i < FastWeight - 1)
            {
                continue;
            }

            var fastValue = Type.ToLowerInvariant() switch
            {
                "sma" => GetSimpleMovingAverage(candles, i, FastWeight),
                "ema" => GetExponentialMovingAverage(candles, fastValues, i, FastWeight),
                "wilders" => GetWildersMovingAverage(candles, fastValues, i, FastWeight),
                _ => throw new NotImplementedException()
            };
            fastValues.Add(fastValue);

            if (i < SlowWeight - 1)
            {
                continue;
            }

            var slowValue = Type.ToLowerInvariant() switch
            {
                "sma" => GetSimpleMovingAverage(candles, i, SlowWeight),
                "ema" => GetExponentialMovingAverage(candles, slowValues, i, SlowWeight),
                "wilders" => GetWildersMovingAverage(candles, slowValues, i, SlowWeight),
                _ => throw new NotImplementedException()
            };
            slowValues.Add(slowValue);

            var macdValue = fastValue - slowValue;
            macdValues.Add(macdValue);

            macdSeries.Add(new LineEntry
            {
                Value = macdValue,
                Timestamp = candles[i].Timestamp,
            });

            if (macdValues.Count < SignalWeight)
            {
                continue;
            }

            var signalOffsetIndex = SlowWeight - 1;
            var signalValue = Type.ToLowerInvariant() switch
            {
                "sma" => GetSimpleMovingAverage(macdValues, i - signalOffsetIndex, SignalWeight),
                "ema" => GetExponentialMovingAverage(macdValues, signalValues, i - signalOffsetIndex, SignalWeight),
                "wilders" => GetWildersMovingAverage(macdValues, signalValues, i - signalOffsetIndex, SignalWeight),
                _ => throw new NotImplementedException()
            };
            signalValues.Add(signalValue);

            signalSeries.Add(new LineEntry
            {
                Value = signalValue,
                Timestamp = candles[i].Timestamp,
            });
        }

        return
        [
            macdSeries,
            signalSeries
        ];
    }

    protected override bool ValidateParameters(IReadOnlyList<object> parameters)
    {
        if (parameters?.Count != 4)
        {
            ErrorMessages.Add("Invalid parameter count.");
            return false;
        }

        if (int.TryParse(parameters[0].ToString(), out var fastWeight))
        {
            FastWeight = fastWeight;
        }
        else
        {
            ErrorMessages.Add("First parameter (fast weight) must be an integer.");
            return false;
        }

        if (int.TryParse(parameters[1].ToString(), out var slowWeight))
        {
            SlowWeight = slowWeight;
        }
        else
        {
            ErrorMessages.Add("Second parameter (slow weight) must be an integer.");
            return false;
        }

        if (int.TryParse(parameters[2].ToString(), out var signalWeight))
        {
            SignalWeight = signalWeight;
        }
        else
        {
            ErrorMessages.Add("Third parameter (signal weight) must be an integer.");
            return false;
        }

        if (ValidTypes.Contains(parameters[3].ToString().ToLowerInvariant()))
        {
            Type = parameters[3].ToString().ToLowerInvariant() ?? string.Empty;
        }
        else
        {
            ErrorMessages.Add("Fourth parameter (moving average type) must be EMA or SMA.");
            return false;
        }

        return true;
    }

    #endregion

    #region Private Methods

    private static float GetSimpleMovingAverage(IEnumerable<Bar> candles, int index, int weight)
    {
        var value = candles.ToList().GetRange(index - (weight - 1), weight).Sum(q => q.Close) / weight;

        return value;
    }

    private static float GetSimpleMovingAverage(IEnumerable<float> candles, int index, int weight)
    {
        var value = candles.ToList().GetRange(index - (weight - 1), weight).Sum(q => q) / weight;

        return value;
    }

    private static float GetExponentialMovingAverage(IEnumerable<Bar> candles, List<float> series, int index, int weight)
    {
        if (!series.Any())
        {
            return GetSimpleMovingAverage(candles, index, weight);
        }

        var smoothingFactor = 2f / (weight + 1);

        var value = (candles.ToArray()[index].Close * smoothingFactor) + (series.Last() * (1 - smoothingFactor));

        return value;
    }

    private static float GetExponentialMovingAverage(IEnumerable<float> candles, List<float> series, int index, int weight)
    {
        if (!series.Any())
        {
            return GetSimpleMovingAverage(candles, index, weight);
        }

        var smoothingFactor = 2f / (weight + 1);

        var value = (candles.ToArray()[index] * smoothingFactor) + (series.Last() * (1 - smoothingFactor));

        return value;
    }

    private static float GetWildersMovingAverage(IEnumerable<Bar> candles, List<float> series, int index, int weight)
    {
        if (!series.Any())
        {
            return GetSimpleMovingAverage(candles, index, weight);
        }

        var smoothingFactor = 1f / weight;

        var value = (candles.ToArray()[index].Close * smoothingFactor) + (series.Last() * (1 - smoothingFactor));

        return value;
    }

    private static float GetWildersMovingAverage(IEnumerable<float> candles, List<float> series, int index, int weight)
    {
        if (!series.Any())
        {
            return GetSimpleMovingAverage(candles, index, weight);
        }

        var smoothingFactor = 1f / weight;

        var value = (candles.ToArray()[index] * smoothingFactor) + (series.Last() * (1 - smoothingFactor));

        return value;
    }

    #endregion
}