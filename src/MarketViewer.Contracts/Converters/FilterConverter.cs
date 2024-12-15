using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Models.ScanV2.Operands;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MarketViewer.Contracts.Converters;

public class FilterConverter : JsonConverter<FilterV2>
{
    public override FilterV2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var document = JsonDocument.ParseValue(ref reader);
        var jsonElement = document.RootElement;

        var argument = ParseFilter(jsonElement, options);

        return argument;
    }

    public override void Write(Utf8JsonWriter writer, FilterV2 value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            var json = JsonSerializer.Serialize(value, options);
            writer.WriteRawValue(json);
        }
    }

    public static FilterV2 ParseFilter(JsonElement jsonElement, JsonSerializerOptions options)
    {
        var filter = new FilterV2();

        if (jsonElement.TryGetProperty("CollectionModifier", out var modifierElement))
        {
            filter.CollectionModifier = modifierElement.GetString();
        }

        if (jsonElement.TryGetProperty("FirstOperand", out var firstOperand))
        {
            filter.FirstOperand = ParseOperand(firstOperand, options);
        }

        if (jsonElement.TryGetProperty("Operator", out var operatorElement) && Enum.TryParse<FilterOperator>(operatorElement.ToString(), out var filterOperator))
        {
            filter.Operator = filterOperator;
        }

        if (jsonElement.TryGetProperty("SecondOperand", out var secondOperand))
        {
            filter.SecondOperand = ParseOperand(secondOperand, options);
        }

        if (jsonElement.TryGetProperty("Timeframe", out var timeframe))
        {
            filter.Timeframe = JsonSerializer.Deserialize<Timeframe>(timeframe.GetRawText(), options);
        }

        return filter;
    }

    private static IScanOperand ParseOperand(JsonElement jsonElement, JsonSerializerOptions options)
    {
        var json = jsonElement.GetRawText();

        if (jsonElement.TryGetProperty("Study", out var studyElement))
        {
            return JsonSerializer.Deserialize<StudyOperand>(json, options);
        }

        if (jsonElement.TryGetProperty("PriceAction", out var priceAction))
        {
            return JsonSerializer.Deserialize<PriceActionOperand>(json, options);
        }

        if (jsonElement.TryGetProperty("Property", out var property))
        {
            return JsonSerializer.Deserialize<PropertyOperand>(json, options);
        }

        if (jsonElement.TryGetProperty("Value", out var value))
        {
            return JsonSerializer.Deserialize<FixedOperand>(json, options);
        }

        return null;
    }
}
