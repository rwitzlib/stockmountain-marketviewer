using System.Collections.Generic;
using System.Threading.Tasks;
using MarketViewer.Contracts.Requests;

namespace MarketViewer.Contracts.Interfaces
{
    public interface IScanService
    {
        Task<List<string>> ScanAsync(ScanRequest request);
    }
}
