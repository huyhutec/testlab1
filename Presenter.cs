using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MangaReader2026.DomainCommon;

namespace MangaReader2026.MangaList;

public class Presenter
{
    private readonly Domain domain;
    private IView? view;
    private CancellationTokenSource? cts;
    private Task? task;
    private int currentPageIndex = 1;
    private int totalPageNumber = 0;
    private bool isLoading;

    private List<Item> currentItems = new();

    public Presenter(Domain domain) => this.domain = domain;

    public void AttachView(IView view)
    {
        if (this.view != null) return;
        this.view = view;
        this.Load();
    }

    private void ShowLoading() => view?.SetLoadingVisible(true);

    private void ShowError(string errorMessage)
    {
        view?.SetLoadingVisible(false);
        view?.SetMainContentVisible(false);
        view?.SetErrorMessage(errorMessage);
        view?.SetErrorPanelVisible(true);
    }

    private void ShowNoManga()
    {
        view?.SetTotalMangaNumber("No manga");
        view?.SetCurrentPageButtonContent("No page");
        view?.SetCurrentPageButtonEnabled(false);
        view?.SetFirstButtonAndPrevButtonEnabled(false);
        view?.SetLastButtonAndNextButtonEnabled(false);
        view?.SetListBoxContent(Enumerable.Empty<Item>());
        view?.SetLoadingVisible(false);
        view?.SetMainContentVisible(true);
        view?.SetErrorPanelVisible(false);
    }

    private void ShowMangaList(MangaReader2026.MangaList.MangaList list)
    {
        currentItems = list.CurrentPage
            .Select(m => new Item(m.Title, m.Category, m.Status, m.Url))
            .ToList();

        view?.SetTotalMangaNumber(list.TotalMangaNumber + " mangas");
        view?.SetFirstButtonAndPrevButtonEnabled(currentPageIndex > 1);
        view?.SetCurrentPageButtonContent($"Page {currentPageIndex} of {list.TotalPageNumber}");
        view?.SetCurrentPageButtonEnabled(true);
        view?.SetNumericUpDownValue(currentPageIndex);
        view?.SetNumericUpDownMaximum(list.TotalPageNumber);
        view?.SetLastButtonAndNextButtonEnabled(currentPageIndex < list.TotalPageNumber);
        view?.SetListBoxContent(currentItems);
        view?.SetMainContentVisible(true);
        view?.SetErrorPanelVisible(false);
        view?.SetLoadingVisible(false);
    }

    public async void Load()
    {
        if (isLoading) return;
        isLoading = true;
        ShowLoading();

        if (cts != null)
        {
            cts.Cancel();
            if (task != null) await task;
            cts = null;
        }

        MangaReader2026.MangaList.MangaList? list = null;
        string? errorMessage = null;

        try
        {
            list = await domain.LoadMangaList(currentPageIndex, view?.GetFilterText() ?? "");
        }
        catch (NetworkException ex)
        {
            errorMessage = "Network error: " + ex.Message;
        }
        catch (ParseException)
        {
            errorMessage = "Oops! Something went wrong.";
        }
        catch (Exception ex)
        {
            errorMessage = "Unknown error: " + ex.Message;
        }

        if (list == null)
            ShowError(errorMessage ?? "Unknown error");
        else if (list.TotalMangaNumber <= 0 || list.TotalPageNumber <= 0)
            ShowNoManga();
        else
        {
            totalPageNumber = list.TotalPageNumber;
            if (currentPageIndex > totalPageNumber) currentPageIndex = totalPageNumber;

            ShowMangaList(list);

            cts = new CancellationTokenSource();
            var coverUrls = list.CurrentPage.Select(m => m.CoverUrl);
            task = LoadCovers(coverUrls, cts.Token);
        }

        isLoading = false;
    }

    // ==================== ALL PAGING METHODS (required by your View.axaml.cs) ====================

    public void GoNextPage()
    {
        if (isLoading || currentPageIndex >= totalPageNumber) return;
        currentPageIndex++;
        view?.SetNumericUpDownValue(currentPageIndex);
        Load();
    }

    public void GoPrevPage()
    {
        if (isLoading || currentPageIndex <= 1) return;
        currentPageIndex--;
        view?.SetNumericUpDownValue(currentPageIndex);
        Load();
    }

    public void GoFirstPage()
    {
        if (isLoading || currentPageIndex <= 1) return;
        currentPageIndex = 1;
        view?.SetNumericUpDownValue(currentPageIndex);
        Load();
    }

    public void GoLastPage()
    {
        if (isLoading || currentPageIndex >= totalPageNumber) return;
        currentPageIndex = totalPageNumber;
        view?.SetNumericUpDownValue(currentPageIndex);
        Load();
    }

    public void GoSpecificPage()
    {
        if (isLoading || view == null) return;
        view.HideFlyout();

        var pageIndex = view.GetNumericUpDownValue();
        if (pageIndex < 1 || pageIndex > totalPageNumber) return;

        currentPageIndex = pageIndex;
        Load();
    }

    public void ApplyFilter()
    {
        currentPageIndex = 1;
        Load();
    }

    public void SelectManga(int index)
    {
        if (index < 0 || index >= currentItems.Count) return;
        var manga = currentItems[index];
        view?.OpenMangaDetail(manga.Url);
    }

    // ==================== COVER LOADER ====================

    private async Task LoadCovers(IEnumerable<string> urls, CancellationToken token)
    {
        var index = -1;
        foreach (var url in urls)
        {
            index++;
            byte[]? bytes = null;
            try { bytes = await domain.LoadBytes(url, token); }
            catch { }
            if (token.IsCancellationRequested) break;
            view?.SetCover(index, bytes);
        }
    }
}