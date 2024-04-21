using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models;

[ExcludeFromCodeCoverage]
public class Feature
{
    public FeatureType Type { get; set; } 

    public string Value { get; set; }
}
