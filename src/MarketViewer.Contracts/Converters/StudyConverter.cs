using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Study;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Converters;

public class StudyConverter : JsonConverter<StudyFields>
{
    public override StudyFields Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var document = JsonDocument.ParseValue(ref reader);
        var jsonElement = document.RootElement;

        var studyFields = new StudyFields();

        var studyParts = jsonElement.GetString().Split(':');

        if (Enum.TryParse<StudyType>(studyParts[0], out var studyType))
        {
            studyFields.Type = studyType;
        }
        else
        {
            throw new JsonException($"Invalid study type: {studyParts[0]}");
        }

        if (studyParts.Length < 2)
        {
            return studyFields;
        }

        studyFields.Parameters = studyParts[1].Split(',');

        return studyFields;
    }

    public override void Write(Utf8JsonWriter writer, StudyFields value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteStringValue($"{value.Type}:{string.Join(',', value.Parameters)}");

        writer.WriteEndObject();
    }
}
