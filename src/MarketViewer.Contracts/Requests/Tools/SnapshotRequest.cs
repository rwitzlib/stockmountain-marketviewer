using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Tools;

[ExcludeFromCodeCoverage]
public class SnapshotRequest : BaseRequest
{
    /// <summary>
    /// The ticker symbol of the stock/equity.
    /// </summary>
    [Required]
    [StringLength(6)]
    public string Ticker { get; set; }
}
