using MarketViewer.Contracts.Models.Backtest;
using System;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace MarketViewer.Infrastructure.Utilities;

public static class BacktestUtilities
{
    private static JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string CompressRequestDetails(BacktestParameters parameters)
    {
        var json = JsonSerializer.Serialize(parameters, Options);
        var bytes = Encoding.UTF8.GetBytes(json);
        using var outputStream = new MemoryStream();
        using (var compressionStream = new GZipStream(outputStream, CompressionMode.Compress))
        {
            compressionStream.Write(bytes, 0, bytes.Length);
        }
        return Convert.ToBase64String(outputStream.ToArray());
    }

    public static BacktestParameters DecompressRequestDetails(string compressedDetails)
    {
        if (string.IsNullOrWhiteSpace(compressedDetails))
        {
            return null;
        }
        var bytes = Convert.FromBase64String(compressedDetails);
        using var inputStream = new MemoryStream(bytes);
        using var decompressionStream = new GZipStream(inputStream, CompressionMode.Decompress);
        using var reader = new StreamReader(decompressionStream, Encoding.UTF8);
        var decompressedData = reader.ReadToEnd();
        return JsonSerializer.Deserialize<BacktestParameters>(decompressedData, Options);
    }
}
