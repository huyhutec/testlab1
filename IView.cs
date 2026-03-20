using System.Collections.Generic;

namespace MangaReader2026.MangaList;

public class Item
{
    public string Title { get; }
    public string Category { get; }
    public string Status { get; }
    
    public string Url { get; }

    public Item(string title, string category, string status, string url)
    {
        Title = title; Category = category; Status = status;
        Url = url;
    }

    public string ToolTip => this.Title + "-" + this.Category + "-" + this.Status;
}

public interface IView
{
    void SetLoadingVisible(bool value);
    void SetErrorPanelVisible(bool value);
    void SetMainContentVisible(bool value);
    void SetTotalMangaNumber(string text);
    void SetCurrentPageButtonContent(string content);
    void SetCurrentPageButtonEnabled(bool value);
    void SetNumericUpDownMaximum(int value);
    void SetNumericUpDownValue(int value);
    int GetNumericUpDownValue();
    void SetListBoxContent(IEnumerable<Item> items);
    void SetCover(int index, byte[]? bytes);
    void SetFirstButtonAndPrevButtonEnabled(bool value);
    void SetLastButtonAndNextButtonEnabled(bool value);
    void HideFlyout();
    void SetErrorMessage(string text);
    string? GetFilterText();
    
    void OpenMangaDetail(string mangaUrl);
}