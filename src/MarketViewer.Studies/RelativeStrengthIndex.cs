using MarketViewer.Contracts.Models.Study;
using Polygon.Client.Models;

namespace MarketViewer.Studies;

public class RelativeStrengthIndex : Study<RelativeStrengthIndex>
{
    private static int Weight { get; set; } = 14;
    private static int OverboughtLevel { get; set; } = 70;
    private static int OversoldLevel { get; set; } = 30;
    private static string Type { get; set; } = "ema";
    private static string[] ValidTypes { get; set; } = ["sma", "ema", "wilders"];

    protected override bool ValidateParameters(IReadOnlyList<object> parameters)
    {
        if (parameters?.Count != 4)
        {
            ErrorMessages.Add("Invalid parameter count.");
            return false;
        }

        if (int.TryParse(parameters[0].ToString(), out var fastWeight))
        {
            Weight = fastWeight;
        }
        else
        {
            ErrorMessages.Add("First parameter (moving average weight) must be an integer.");
            return false;
        }

        if (int.TryParse(parameters[1].ToString(), out var slowWeight))
        {
            OverboughtLevel = slowWeight;
        }
        else
        {
            ErrorMessages.Add("Second parameter (overbought level) must be an integer.");
            return false;
        }

        if (int.TryParse(parameters[2].ToString(), out var signalWeight))
        {
            OversoldLevel = signalWeight;
        }
        else
        {
            ErrorMessages.Add("Third parameter (oversold level) must be an integer.");
            return false;
        }

        if (ValidTypes.Contains(parameters[3].ToString().ToLowerInvariant()))
        {
            Type = parameters[3].ToString().ToLowerInvariant() ?? string.Empty;
        }
        else
        {
            ErrorMessages.Add("Fourth parameter (moving average type) must be 'SMA', 'EMA', or 'Wilders'.");
            return false;
        }

        return true;
    }
    
    protected override List<List<LineEntry>> Initialize(List<Bar> candles)
    {
        var series = new List<LineEntry>();
        var overbought = new List<LineEntry>();
        var oversold = new List<LineEntry>();

        if (candles.Count < Weight + 1)
        {
            return [series, overbought, oversold];
        }

        List<float> upMoves = [];
        List<float> downMoves = [];
        List<float> avgUps = [];
        List<float> avgDowns = [];
        for (int i = 1; i < candles.Count; i++)
        {
            var value = candles[i].Close - candles[i - 1].Close;
            if (value > 0)
            {
                upMoves.Add(value);
                downMoves.Add(0);
            }
            else if (value == 0)
            {
                upMoves.Add(0);
                downMoves.Add(0);
            }
            else
            {
                upMoves.Add(0);
                downMoves.Add(Math.Abs(value));
            }

            if (upMoves.Count < Weight || downMoves.Count < Weight)
            {
                continue;
            }

            var (avgUp, avgDown) = Type.ToLowerInvariant() switch
            {
                "sma" => GetSimpleMovingAverageRSI(upMoves, downMoves, i),
                "ema" => GetExponentialMovingAverageRSI(series, upMoves, downMoves, avgUps, avgDowns, i),
                "wilders" => GetWildersMovingAverageRSI(series, upMoves, downMoves, avgUps, avgDowns, i),
                _ => throw new NotImplementedException()
            };
            avgUps.Add(avgUp);
            avgDowns.Add(avgDown);

            var rsi = 100 - (100 / (1 + (avgUp / avgDown)));

            series.Add(new LineEntry
            {
                Value = rsi,
                Timestamp = candles[i].Timestamp
            });

            overbought.Add(new LineEntry
            {
                Value = OverboughtLevel,
                Timestamp = candles[i].Timestamp
            });

            oversold.Add(new LineEntry
            {
                Value = OversoldLevel,
                Timestamp = candles[i].Timestamp
            });
        }

        return [series, overbought, oversold];
    }

    private static (float, float) GetSimpleMovingAverageRSI(List<float> upMoves, List<float> downMoves, int index)
    {
        var avgUp = upMoves.GetRange(index - Weight, Weight).Sum() / Weight;
        var avgDown = downMoves.GetRange(index - Weight, Weight).Sum() / Weight;

        return (avgUp, avgDown);
    }

    private static (float, float) GetExponentialMovingAverageRSI(
        List<LineEntry> series,
        List<float> upMoves,
        List<float> downMoves,
        List<float> avgUps,
        List<float> avgDowns,
        int index)
    {
        if (series.Count == 0)
        {
            return GetSimpleMovingAverageRSI(upMoves, downMoves, index);
        }

        var a = 2f / (Weight + 1);

        var avgUp = a * upMoves.Last() + (1 - a) * avgUps.Last();
        var avgDown = a * downMoves.Last() + (1 - a) * avgDowns.Last();

        return (avgUp, avgDown);
    }

    private static (float, float) GetWildersMovingAverageRSI(List<LineEntry> series,
        List<float> upMoves,
        List<float> downMoves,
        List<float> avgUps,
        List<float> avgDowns,
        int index)
    {
        if (series.Count == 0)
        {
            return GetSimpleMovingAverageRSI(upMoves, downMoves, index);
        }

        var a = 1f / Weight;

        var avgUp = a * upMoves.Last() + (1 - a) * avgUps.Last();
        var avgDown = a * downMoves.Last() + (1 - a) * avgDowns.Last();

        return (avgUp, avgDown);
    }
}