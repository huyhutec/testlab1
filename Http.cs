using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MangaReader2026.DomainCommon;

public class Http
{
    private static readonly HttpClient Client = new();

    static Http()
    {
        Client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public Task<string> GetStringAsync(string url) => Client.GetStringAsync(url);

    public async Task<byte[]> GetBytesAsync(string url, CancellationToken token = default)
    {
        using var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(token);
    }
}