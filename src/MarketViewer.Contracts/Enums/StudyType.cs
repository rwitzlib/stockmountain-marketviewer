using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace MarketViewer.Contracts.Enums;

public enum StudyType
{
    vwap,
    macd,
    ema,
    sma,
    rsi
}