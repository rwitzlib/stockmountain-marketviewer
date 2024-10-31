using Amazon.DynamoDBv2.DataModel;
using MarketViewer.Contracts.Enums.Trade;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketViewer.Contracts.Models.Trade
{
    [ExcludeFromCodeCoverage]
    [DynamoDBTable("lad-dev-marketviewer-trade-store")]
    public class TradeRecord
    {
        [DynamoDBHashKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [DynamoDBGlobalSecondaryIndexHashKey]
        public string CustomerId { get; set; }
        public TradeStatus Status { get; set; }
        public string Ticker { get; set; }
        public string EntryDate { get; set; }
        public string CloseDate { get; set; }
        public float EntryPrice { get; set; }
        public float ClosePrice { get; set; }
        public float EntryPosition { get; set; }
        public float ClosePosition { get; set; }
        public float Profit { get; set; }
    }
}
