using MarketViewer.Contracts.Responses;
using System.Collections.Generic;

namespace MarketViewer.Application.Utilities
{
    public class ScanResponseItemComparer : IEqualityComparer<ScanResponse.Item>
    {
        public bool Equals(ScanResponse.Item x, ScanResponse.Item y)
        {
            if (x.Ticker is null || y.Ticker is null)
            {
                return false;
            }

            return x.Ticker == y.Ticker;
        }

        public int GetHashCode(ScanResponse.Item obj)
        {
            return obj.Ticker.GetHashCode();
        }
    }
}
