using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.ScanV2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MarketViewer.Contracts.Converters;

public class ScanArgumentConverter : System.Text.Json.Serialization.JsonConverter<ScanArgument>
{
    public ScanArgumentConverter() { }

    public override ScanArgument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var document = JsonDocument.ParseValue(ref reader);
        var jsonElement = document.RootElement;

        var argument = ParseArgument(jsonElement);

        return argument;
    }

    public override void Write(Utf8JsonWriter writer, ScanArgument value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            var json = JsonConvert.SerializeObject(value);
            writer.WriteRawValue(json);
        }
    }

    private static ScanArgument ParseArgument(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var scanArgument = new ScanArgument();

        if (jsonElement.TryGetProperty("Argument", out var argument))
        {
            scanArgument.Argument = ParseArgument(argument);
        }

        if (jsonElement.TryGetProperty("Operator", out var argOperator))
        {
            scanArgument.Operator = argOperator.GetString();
        }

        if (jsonElement.TryGetProperty("Filters", out var filters))
        {
            var enumerator = filters.EnumerateArray();

            var count = filters.GetArrayLength();

            scanArgument.Filters = [];
            for (int i = 0; i < count; i++)
            {
                enumerator.MoveNext();
                scanArgument.Filters.Add(ParseFilter(enumerator.Current));
            }
        }

        return scanArgument;
    }

    private static FilterV2 ParseFilter(JsonElement jsonElement)
    {
        var filter = new FilterV2();

        if (jsonElement.TryGetProperty("CollectionModifier", out var modifierElement))
        {
            filter.CollectionModifier = modifierElement.GetString();
        }

        if (jsonElement.TryGetProperty("FirstOperand", out var firstOperand))
        {
            filter.FirstOperand = ParseOperand(firstOperand);
        }

        if (jsonElement.TryGetProperty("Operator", out var operatorElement) && Enum.TryParse<FilterOperator>(operatorElement.ToString(), out var filterOperator))
        {
            filter.Operator = filterOperator;
        }

        if (jsonElement.TryGetProperty("SecondOperand", out var secondOperand))
        {
            filter.SecondOperand = ParseOperand(secondOperand);
        }

        if (jsonElement.TryGetProperty("Timeframe", out var timeframe))
        {
            filter.Timeframe = JsonSerializer.Deserialize<Timeframe>(timeframe.GetRawText());
        }

        return filter;
    }

    private static IScanOperand ParseOperand(JsonElement jsonElement)
    {
        var json = jsonElement.GetRawText();


        if (jsonElement.TryGetProperty("Name", out var name))
        {
            return name.ToString() switch
            {
                "PriceAction" => JsonSerializer.Deserialize<PriceActionOperand>(json),
                "Study" => JsonSerializer.Deserialize<StudyOperand>(json),
                "Value" => JsonSerializer.Deserialize<ValueOperand>(json),
                _ => null
            };
        }

        return null;
    }
}
