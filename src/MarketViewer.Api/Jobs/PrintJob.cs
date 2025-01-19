using MarketViewer.Api.Controllers;
using Quartz;

namespace MarketViewer.Api.Jobs;

public class PrintJob(
    ScanController controller,
    ILogger<PrintJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await controller.Print();
    }
}
