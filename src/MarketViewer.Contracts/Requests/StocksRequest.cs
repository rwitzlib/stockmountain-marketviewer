using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Study;
using MarketViewer.Contracts.Responses;
using MediatR;

namespace MarketViewer.Contracts.Requests;

[ExcludeFromCodeCoverage]
public class StocksRequest : IRequest<OperationResult<StocksResponse>>
{
    /// <summary>
    /// The ticker symbol of the stock/equity.
    /// </summary>
    [Required]
    [StringLength(6)]
    public string Ticker { get; set; }

    /// <summary>
    /// The size of the timespan multiplier.
    /// </summary>
    [Required]
    public int Multiplier { get; set; }

    /// <summary>
    /// The size of the time window.
    /// </summary>
    [Required]
    public Timespan Timespan { get; set; }

    /// <summary>
    /// The start of the aggregate time window. Either a date with the format YYYY-MM-DD or 
    /// a millisecond timestamp.
    /// </summary>
    [Required]
    public DateTimeOffset From { get; set; }

    /// <summary>
    /// The end of the aggregate time window. Either a date with the format YYYY-MM-DD or 
    /// a millisecond timestamp.
    /// </summary>
    [Required]
    public DateTimeOffset To { get; set; }

    public List<StudyFields> Studies { get; set; }
}
