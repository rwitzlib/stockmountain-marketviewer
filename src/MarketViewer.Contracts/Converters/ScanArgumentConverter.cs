using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Models.ScanV2.Operands;
using Newtonsoft.Json;
using System.Text.Json;

namespace MarketViewer.Contracts.Converters;

public class ScanArgumentConverter : System.Text.Json.Serialization.JsonConverter<ScanArgument>
{
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
                scanArgument.Filters.Add(FilterConverter.ParseFilter(enumerator.Current));
            }
        }

        return scanArgument;
    }
}
