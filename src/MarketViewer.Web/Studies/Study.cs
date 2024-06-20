using MarketViewer.Web.Contracts.Studies;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using MarketViewer.Contracts.Responses;
using MarketViewer.Contracts.Models.Study;

namespace MarketViewer.Web.Studies
{
    [ExcludeFromCodeCoverage]
    public abstract class Study
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        protected List<Line> Lines { get; set; }
        public StudyParams SavedParameters { get; set; }
        public int Pane { get; set; } = 0;

        public abstract void Compute(StocksResponse response);

        public async Task AddStudyToChart(IJSRuntime jsRuntime, string chartId)
        {
            foreach (var line in Lines)
            {
                var json = JsonSerializer.Serialize(line);
                
                await jsRuntime.InvokeVoidAsync("CreateLineChart", chartId, json, Pane);

            }
            await jsRuntime.InvokeVoidAsync("AddStudyToLegend", chartId, Title, Lines[0].Color);
        }

        public async Task UpdateStudy(IJSRuntime jsRuntime, string chartId)
        {
            foreach (var json in Lines.Select(chartLine => JsonSerializer.Serialize(chartLine)))
            {
                await jsRuntime.InvokeVoidAsync("UpdateLineChart", chartId, json);
            }
            
        }

        public async Task RemoveStudyFromChart(IJSRuntime jsRuntime, string chartId)
        {
            foreach (var json in Lines.Select(chartLine => JsonSerializer.Serialize(chartLine)))
            {
                await jsRuntime.InvokeVoidAsync("DeleteLineChart", chartId, json);
            }
            await jsRuntime.InvokeVoidAsync("RemoveStudyFromLegend", chartId, Title);
        }
    }
}
