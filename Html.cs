using System.Web;

namespace MangaReader2026.DomainCommon;

public static class Html
{
    public static string Decode(string input) => HttpUtility.HtmlDecode(input);
}