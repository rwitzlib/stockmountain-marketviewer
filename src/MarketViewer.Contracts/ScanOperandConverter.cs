using MarketViewer.Contracts.Models.Scan;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarketViewer.Application.Utilities
{
    public class ScanOperandConverter : JsonConverter<IScanOperand>
    {
        public ScanOperandConverter() { }

        public override IScanOperand Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var document = JsonDocument.ParseValue(ref reader);
            var json = document.RootElement.GetRawText();

            if (TryParse<PriceActionOperand>(json, out var priceActionOperand))
            {
                return priceActionOperand;
            }
            else if (TryParse<StudyOperand>(json, out var studyOperand))
            {
                return studyOperand;
            }
            else if (TryParse<ValueOperand>(json, out var valueOperand))
            {
                return valueOperand;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, IScanOperand value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        private static bool TryParse<T>(string json, out T operand)
        {
            operand = default;

            try
            {
                var result = JsonSerializer.Deserialize<T>(json);

                operand = result;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
