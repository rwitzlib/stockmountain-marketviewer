using MarketViewer.Contracts.Models;
using Polygon.Client.Models;

namespace MarketViewer.Studies;

public class MovingAverageConvergenceDivergence : Study<MovingAverageConvergenceDivergence>
{
    private static int FastWeight { get; set; } = 9;
    private static int SlowWeight { get; set; } = 26;
    private static int SignalWeight { get; set; } = 9;
    private static string Type { get; set; } = "EMA";
    private static string[] ValidTypes { get; set; } = ["SMA", "EMA"];

    protected override List<List<LineEntry>> Initialize(Bar[] data)
    {
        if (data.Length < FastWeight 
            || data.Length < SlowWeight 
            || data.Length < SignalWeight)
        {
            ErrorMessages.Add("Not enough candle data.");
            return null;
        }

        var macdSeries = new List<LineEntry>();
        var signalSeries = new List<LineEntry>();

        var offset = SlowWeight - FastWeight;
        
        switch (Type.ToUpperInvariant())
        {
            case "SMA":
                {
                    var fastSeries = Study<SimpleMovingAverage>.Compute(data, FastWeight).Lines[0];
                    var slowSeries = Study<SimpleMovingAverage>.Compute(data, SlowWeight).Lines[0];

                    if (fastSeries.Count - slowSeries.Count != offset)
                    {
                        break;
                    }

                    for (var i = 0; i < slowSeries.Count; i++)
                    {
                        macdSeries.Add(new LineEntry
                        {
                            Value = fastSeries[i + offset].Value - slowSeries[i].Value,
                            Timestamp = fastSeries[i + offset].Timestamp
                        });
                    }
                
                    if (macdSeries.Count < SignalWeight)
                    {
                        return null;
                    }
                
                    // Calculate SMA on MACD for Signal
                    for (var i = SignalWeight - 1; i < macdSeries.Count; i++)
                    {
                        float movingAvgValue = 0;
                        for (var j = 0; j < SignalWeight; j++)
                        {
                            movingAvgValue += macdSeries[i - j].Value;
                        }
                        movingAvgValue /= SignalWeight;

                        signalSeries.Add(new LineEntry
                        {
                            Timestamp = macdSeries[i].Timestamp,
                            Value = movingAvgValue
                        });
                    }
                
                    break;
                }

            case "EMA":
                {                
                    var fastSeries = ExponentialMovingAverage.Compute(data, FastWeight).Lines[0];
                    var slowSeries = ExponentialMovingAverage.Compute(data, SlowWeight).Lines[0];
 
                    if (fastSeries.Count - slowSeries.Count != offset)
                    {
                        break;
                    }

                    for (var i = 0; i < slowSeries.Count; i++)
                    {
                        macdSeries.Add(new LineEntry
                        { 
                            Value = fastSeries[i + offset].Value - slowSeries[i].Value,
                            Timestamp = fastSeries[i + offset].Timestamp
                        });
                    }

                    if (macdSeries.Count < SignalWeight)
                    {
                        return null;
                    }

                    // Get Initial SMA
                    float simpleMovingAverage = 0;
                    for (var i = 0; i < SignalWeight; i++)
                    {
                        simpleMovingAverage += macdSeries[i].Value;
                    }
                    simpleMovingAverage /= SignalWeight;

                    // Set Initial SMA as first EMA
                    signalSeries.Add(new LineEntry
                    {
                        Timestamp = macdSeries[SignalWeight - 1].Timestamp,
                        Value = simpleMovingAverage
                    });

                    // Calculate Constant
                    var weightFactor = 2f / (SignalWeight + 1);

                    // Calculate EMA series
                    for (var i = SignalWeight; i < macdSeries.Count; i++)
                    {
                        var previousExponentialMovingAvg = signalSeries[^1].Value;

                        var currentExponentialMovingAvg = weightFactor * (macdSeries[i].Value - previousExponentialMovingAvg) + previousExponentialMovingAvg;

                        signalSeries.Add(new LineEntry
                        {
                            Timestamp = macdSeries[i].Timestamp,
                            Value = currentExponentialMovingAvg
                        });
                    }
                
                    break;
                }
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

        if (ValidTypes.Contains(parameters[3].ToString(), StringComparer.InvariantCultureIgnoreCase))
        {
            Type = parameters[3].ToString() ?? string.Empty;
        }
        else
        {
            ErrorMessages.Add("Fourth parameter (moving average type) must be EMA or SMA.");
            return false;
        }

        return true;
    }
}