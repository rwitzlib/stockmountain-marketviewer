using MarketViewer.Contracts.Converters;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Requests;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace MarketViewer.Api.Binders
{
    public class ScanRequestBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var modelName = bindingContext.ModelName;

            using var streamReader = new StreamReader(bindingContext.HttpContext.Request.Body);
            var json = await streamReader.ReadToEndAsync();

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            var request = new ScanRequestV2();

            if (jsonElement.TryGetProperty("Argument", out var argument))
            {
                request.Argument = ParseArgument(argument);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(modelName, "Must include argument.");
                return;
            }

            if (jsonElement.TryGetProperty("Timestamp", out var timestamp))
            {
                request.Timestamp = timestamp.GetDateTimeOffset();
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(modelName, "Must include timestamp.");
                return;
            }

            bindingContext.Result = ModelBindingResult.Success(request);
            return;
        }

        private static ScanArgument ParseArgument(JsonElement jsonElement)
        {
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

                scanArgument.Filters = new FilterV2[count];
                for(int i = 0; i < count; i++)
                {
                    enumerator.MoveNext();
                    scanArgument.Filters[i] = ParseFilter(enumerator.Current);
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

            if (jsonElement.TryGetProperty("Operator", out var operatorElement) && Enum.TryParse<FilterOperator>(operatorElement.GetString(), out var filterOperator))
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

            if (typeof(PriceActionOperand).GetProperties()
                .Select(q => q.Name)
                .All(prop => jsonElement.TryGetProperty(prop, out var result) == true))
            {
                var operand = JsonSerializer.Deserialize<PriceActionOperand>(json);
                return operand;
            }

            if (typeof(StudyOperand).GetProperties()
                .Select(q => q.Name)
                .All(prop => jsonElement.TryGetProperty(prop, out var result) == true))
            {
                var operand = JsonSerializer.Deserialize<StudyOperand>(json);
                return operand;
            }

            if (typeof(ValueOperand).GetProperties()
                .Select(q => q.Name)
                .All(prop => jsonElement.TryGetProperty(prop, out var result) == true))
            {
                var operand = JsonSerializer.Deserialize<ValueOperand>(json);
                return operand;
            }

            throw new NotImplementedException();
        }
    }
}
