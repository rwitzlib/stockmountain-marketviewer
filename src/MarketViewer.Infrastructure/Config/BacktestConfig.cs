using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Infrastructure.Config;

[ExcludeFromCodeCoverage]
public class BacktestConfig
{
    public string TableName { get; set; }
    public string RequestIndexName { get; set; }
    public string LambdaName { get; set; }
    public string S3BucketName { get; set; }
}
