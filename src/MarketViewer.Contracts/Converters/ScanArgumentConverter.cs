using MarketViewer.Contracts.Models.Scan;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Converters;

public class ScanArgumentConverter : JsonConverter<ScanArgument>
{
    public override ScanArgument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var document = JsonDocument.ParseValue(ref reader);
        var jsonElement = document.RootElement;

        var argument = ParseArgument(jsonElement, options);

        return argument;
    }

    public override void Write(Utf8JsonWriter writer, ScanArgument value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("Operator", value.Operator);

        if (value.Filters is not null)
        {
            writer.WritePropertyName("Filters");
            writer.WriteStartArray();
            foreach (var filter in value.Filters)
            {
                JsonSerializer.Serialize(writer, filter, options);
            }
            writer.WriteEndArray();
        }

        if (value.Argument is not null)
        {
            writer.WritePropertyName("Argument");
            JsonSerializer.Serialize(writer, value.Argument, options);
        }

        writer.WriteEndObject();
    }

    private static ScanArgument ParseArgument(JsonElement jsonElement, JsonSerializerOptions options)
    {
        if (jsonElement.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var scanArgument = new ScanArgument();

        if (jsonElement.TryGetProperty("Argument", out var argument))
        {
            scanArgument.Argument = ParseArgument(argument, options);
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
                scanArgument.Filters.Add(FilterConverter.ParseFilter(enumerator.Current, options));
            }
        }

        return scanArgument;
    }
}
