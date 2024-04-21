using Blazored.Modal;
using MarketViewer.Web;
using MarketViewer.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var url = builder.Configuration.GetSection("Urls").GetValue<string>("MarketViewerApi");

        builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(url) })
            .AddSingleton<ChartService>()
            .AddSingleton<ScannerService>()
            .AddSingleton<BacktestService>()
            .AddBlazoredModal()
            .AddMudServices();

        await builder.Build().RunAsync();
    }
}