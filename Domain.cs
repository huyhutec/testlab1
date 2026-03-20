using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MangaReader2026.DomainCommon;

namespace MangaReader2026.MangaList;

public class Domain
{
    public readonly string baseUrl = "https://nhattruyenviet.com";
    public readonly Http http = new();

    private async Task<string> DownloadHtml(int page, string filterText = "")
    {
        if (page < 1) page = 1;
        string url = string.IsNullOrWhiteSpace(filterText)
            ? $"{baseUrl}/danh-sach-truyen?page={page}"
            : $"{baseUrl}/tim-kiem?keyword={Uri.EscapeDataString(filterText)}&page={page}";

        return await http.GetStringAsync(url);
    }

    private int FindTotalPageNumber(string html)
    {
        try
        {
            var s = html.Substring(0, html.LastIndexOf("trang=", StringComparison.OrdinalIgnoreCase));
            s = s.Substring(s.LastIndexOf("trang=", StringComparison.OrdinalIgnoreCase));
            s = s.Substring(s.IndexOf("\">") + 2);
            s = s.Substring(0, s.IndexOf("</a></li>"));
            return int.Parse(s.Trim());
        }
        catch
        {
            try
            {
                int idx = html.LastIndexOf("page=", StringComparison.OrdinalIgnoreCase);
                var s = html.Substring(idx + 5);
                s = s.Substring(0, s.IndexOfAny(new[] { '&', '"', '\'' }));
                return int.Parse(s.Trim());
            }
            catch { return 1; }
        }
    }

    private List<Manga> ParseMangaList(string html)
    {
        var mangas = new List<Manga>();
        int pos = 0;

        while ((pos = html.IndexOf("class=\"item\"", pos, StringComparison.OrdinalIgnoreCase)) != -1 ||
               (pos = html.IndexOf("class=\"manga-item\"", pos, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            string title = Extract(html, pos, "title=\"", "\"") ?? "Unknown";
            string category = Extract(html, pos, "genre", "</") ?? "Unknown";
            string status = Extract(html, pos, "status", "</") ?? "Đang cập nhật";
            string cover = Extract(html, pos, "src=\"", "\"") ?? Extract(html, pos, "data-src=\"", "\"") ?? "";
            string url = Extract(html, pos, "href=\"", "\"") ?? "";

            if (!url.StartsWith("http")) url = baseUrl + url;
            if (!string.IsNullOrEmpty(cover) && !cover.StartsWith("http")) cover = baseUrl + cover;

            mangas.Add(new Manga(
                Html.Decode(title),
                Html.Decode(category),
                cover,
                Html.Decode(status),
                url));

            pos += 200;
        }
        return mangas;
    }

    private string? Extract(string html, int startPos, string startTag, string endTag)
    {
        int s = html.IndexOf(startTag, startPos);
        if (s == -1) return null;
        s += startTag.Length;
        int e = html.IndexOf(endTag, s);
        return e == -1 ? null : html.Substring(s, e - s).Trim();
    }

    private MangaList Parse(string html)
    {
        try
        {
            File.WriteAllText("Before.html", html);
            int totalPages = FindTotalPageNumber(html);
            var currentPage = ParseMangaList(html);
            File.WriteAllText("After.html", html);
            return new MangaList(currentPage.Count * totalPages, totalPages, currentPage);
        }
        catch
        {
            throw new ParseException();
        }
    }

    // FIXED: Full qualification to avoid namespace conflict
    public async Task<MangaReader2026.MangaList.MangaList> LoadMangaList(int page, string filterText = "")
    {
        var html = await DownloadHtml(page, filterText);
        return Parse(html);
    }

    public Task<byte[]> LoadBytes(string url, CancellationToken token)
    {
        return http.GetBytesAsync(url, token);
    }
}