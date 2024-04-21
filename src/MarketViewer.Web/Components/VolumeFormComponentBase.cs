using MarketViewer.Web.Contracts;
using MarketViewer.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace MarketViewer.Web.Components
{
    public class VolumeFormComponentBase : ComponentBase
    {
        protected VolumeParams VolumeParams = new VolumeParams();

        [Inject]
        protected ChartService AggregateService { get; set; }

        [Inject]
        protected IJSRuntime JsRuntime { get; set; } = null!;

        protected async Task PlotVolumeChart()
        {
            //var volumeResponse = AggregateService.CreateVolumeChart(VolumeParams );

            //var volumeJson = JsonSerializer.Serialize(volumeResponse);
            //await JsRuntime.InvokeVoidAsync("PlotVolume", volumeJson);
        }
    }
}
