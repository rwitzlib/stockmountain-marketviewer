using MarketViewer.Contracts.Entities.Scan;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Scan.Operands;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MarketViewer.Contracts.Converters;

public class FilterConverter : JsonConverter<Filter>
{
    public override Filter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var document = JsonDocument.ParseValue(ref reader);
        var jsonElement = document.RootElement;

        var argument = ParseFilter(jsonElement, options);

        return argument;
    }

    public override void Write(Utf8JsonWriter writer, Filter value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.CollectionModifier is not null)
        {
            writer.WriteString("CollectionModifier", value.CollectionModifier);
        }

        if (value.FirstOperand is not null)
        {
            writer.WritePropertyName("FirstOperand");

            switch (value.FirstOperand)
            {
                case FixedOperand:
                    JsonSerializer.Serialize(writer, value.FirstOperand as FixedOperand, options);
                    break;
                case PriceActionOperand:
                    JsonSerializer.Serialize(writer, value.FirstOperand as PriceActionOperand, options);
                    break;
                case PropertyOperand:
                    JsonSerializer.Serialize(writer, value.FirstOperand as PropertyOperand, options);
                    break;
                case StudyOperand:
                    JsonSerializer.Serialize(writer, value.FirstOperand as StudyOperand, options);
                    break;
                default:
                    break;
            }
        }

        writer.WriteString("Operator", value.Operator.ToString());

        if (value.SecondOperand is not null)
        {
            writer.WritePropertyName("SecondOperand");

            switch (value.SecondOperand)
            {
                case FixedOperand:
                    JsonSerializer.Serialize(writer, value.SecondOperand as FixedOperand, options);
                    break;
                case PriceActionOperand:
                    JsonSerializer.Serialize(writer, value.SecondOperand as PriceActionOperand, options);
                    break;
                case PropertyOperand:
                    JsonSerializer.Serialize(writer, value.SecondOperand as PropertyOperand, options);
                    break;
                case StudyOperand:
                    JsonSerializer.Serialize(writer, value.SecondOperand as StudyOperand, options);
                    break;
                default:
                    break;
            }
        }

        if (value.Timeframe is not null)
        {
            writer.WritePropertyName("Timeframe");
            JsonSerializer.Serialize(writer, value.Timeframe, options);
        }

        writer.WriteEndObject();
    }

    public static Filter ParseFilter(JsonElement jsonElement, JsonSerializerOptions options)
    {
        var filter = new Filter();

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