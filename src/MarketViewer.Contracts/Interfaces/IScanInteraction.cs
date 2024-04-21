using System.Threading.Tasks;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Contracts.Interfaces
{
    public interface IScanInteraction
    {
        public Task<OperationResult<ScanResponse>> ScanAsync(ScanRequest request);
    }
}
