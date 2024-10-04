using System.Collections.Generic;
using System.Threading.Tasks;
using MarketViewer.Contracts.Requests.Scan;
using Polygon.Client.Responses;

namespace MarketViewer.Contracts.Interfaces;

public interface IAggregateCache
{
    Task<IEnumerable<PolygonAggregateResponse>> RetrieveAggregateResponses(ScanRequest request);
}