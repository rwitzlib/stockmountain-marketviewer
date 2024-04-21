using System.Text;
using System.Text.Json.Nodes;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Requests;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MarketViewer.Api.Binders;

public class AggregateModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null || bindingContext.ModelState is null)
        {
            throw new ArgumentNullException("Invalid request format.");
        }

        if (bindingContext.ValueProvider is null)
        {
            bindingContext.ModelState.AddModelError("RequestFormat", "Invalid request format.");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var request = new StocksRequest();
        
        if (KeyExists(bindingContext, "ticker"))
        {
            request.Ticker = bindingContext.ValueProvider.GetValue("ticker").FirstValue;
        }
        else
        {
            bindingContext.ModelState.AddModelError("Parameter Error", "Must include ticker. Ex. \"TSLA\"");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }
        
        if (KeyExists(bindingContext, "Multiplier") && int.TryParse(bindingContext.ValueProvider.GetValue("Multiplier").FirstValue, out var multiplier))
        {
            request.Multiplier = multiplier;
        }
        else
        {
            bindingContext.ModelState.AddModelError("Parameter Error", "Must include multiplier. Ex. \"1\"");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }
        
        if (KeyExists(bindingContext, "Timespan") && Enum.TryParse(bindingContext.ValueProvider.GetValue("Timespan").FirstValue, out Timespan timespan))
        {
            request.Timespan = timespan;
        }
        else
        {
            bindingContext.ModelState.AddModelError("Parameter Error", "Must include Timespan. Ex. \"minute\"");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        if (KeyExists(bindingContext, "From") && DateTimeOffset.TryParse(bindingContext.ValueProvider.GetValue("From").FirstValue, out var from))
        {
            request.From = from;
        }
        else
        {
            bindingContext.ModelState.AddModelError("Parameter Error", "Must include \"From\" date. Ex. \"12-24-2020\"");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        if (KeyExists(bindingContext, "To") && DateTimeOffset.TryParse(bindingContext.ValueProvider.GetValue("To").FirstValue, out var to))
        {
            request.To = to;
        }
        else
        {
            bindingContext.ModelState.AddModelError("Parameter Error", "Must include \"To\" date. Ex. \"12-25-2020\"");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        if (KeyExists(bindingContext, "_study"))
        {
            request.Studies = new List<StudyFields>();
            var studies = bindingContext.ValueProvider.GetValue("_study").Values;

            foreach (var studyParts in studies.Select(study => study.Split(':')))
            {
                if (Enum.TryParse(studyParts[0], out StudyType type))
                {
                    switch (studyParts.Length)
                    {
                        case 1:
                            request.Studies.Add(new StudyFields
                            {
                                Type = type,
                            });
                            break;
                        case 2:
                            request.Studies.Add(new StudyFields
                            {
                                Type = type,
                                Parameters = studyParts[1].Split(',')
                            });
                            break;
                        default:
                            bindingContext.ModelState.AddModelError("StudyError", "Study should be formatted like: [name of study]:[comma separated list of parameters for study].  Ex. EMA:9");
                            bindingContext.Result = ModelBindingResult.Failed();
                            return Task.CompletedTask;
                    }
                }
                else
                {
                    bindingContext.ModelState.AddModelError("StudyError", "Study should be formatted like: [name of study]:[comma separated list of parameters for study].  Ex. EMA:9");
                    bindingContext.Result = ModelBindingResult.Failed();
                    return Task.CompletedTask;
                }
            }
        }
        
        bindingContext.Result = ModelBindingResult.Success(request);
        return Task.CompletedTask;
    }

    private static bool KeyExists(ModelBindingContext context, string keyName)
    {
        return context.ValueProvider.GetValue(keyName).FirstValue != null;
    }
}