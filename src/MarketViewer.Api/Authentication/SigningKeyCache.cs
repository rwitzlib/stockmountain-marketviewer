using Microsoft.Extensions.Caching.Memory;

namespace MarketViewer.Api.Authentication;

public class SigningKeyCache
{
    public string Keys { get; set; }

    public void InitializeKeys(string url)
    {
        var client = new HttpClient();
        var keys = client.GetStringAsync(url).Result;

        Keys = keys;
    }

    public string GetKeys()
    {
        if (string.IsNullOrWhiteSpace(Keys))
        {
            InitializeKeys("https://auth.stockmountain.io/.well-known/jwks.json");
        }

        return Keys;
    }
}
