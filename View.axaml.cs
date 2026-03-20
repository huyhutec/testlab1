using System;
using System.Collections.Generic;
using System.Threading.Tasks;           // ← FIXES "Task" error
using Avalonia.Controls;
using Avalonia.Interactivity;
using MangaReader2026.MangaList;       // your namespace

namespace MangaReader2026.MangaList;

public partial class View : Window, IView
{
    private readonly Presenter? presenter;
    private readonly List<ItemControl> itemControls = new();

    // Default constructor (Avalonia needs this)
    public View() => InitializeComponent();

    // Constructor used by App.axaml.cs or main window
    public View(Presenter presenter) : this()
    {
        this.presenter = presenter;
        AttachEvents();                 // ← wires all buttons
    }

    private void AttachEvents()
    {
        // Wire all your buttons (replace names if yours are different)
        this.FindControl<Button>("btnFirst")!.Click += (_, _) => presenter?.GoFirstPage();
        this.FindControl<Button>("btnPrev")!.Click  += (_, _) => presenter?.GoPrevPage();
        this.FindControl<Button>("btnNext")!.Click  += (_, _) => presenter?.GoNextPage();
        this.FindControl<Button>("btnLast")!.Click  += (_, _) => presenter?.GoLastPage();

        var numeric = this.FindControl<NumericUpDown>("numericPage")!;
        numeric.ValueChanged += (_, _) => presenter?.GoSpecificPage();

        var txtFilter = this.FindControl<TextBox>("txtFilter")!;
        txtFilter.TextChanged += (_, _) => presenter?.ApplyFilter();

        // Double-click on any ItemControl opens detail (Lab 3)
        // (your ItemControl already has the event or you can add it here)
        foreach (var ctrl in itemControls)
            ctrl.DoubleTapped += (_, _) => presenter?.SelectManga(itemControls.IndexOf(ctrl));
    }

    // ==================== IView IMPLEMENTATION (your original methods) ====================
    public void SetLoadingVisible(bool value) => FindControl<Panel>("loadingPanel")!.IsVisible = value;
    public void SetErrorPanelVisible(bool value) => FindControl<Panel>("errorPanel")!.IsVisible = value;
    public void SetMainContentVisible(bool value) => FindControl<Panel>("mainPanel")!.IsVisible = value;
    public void SetTotalMangaNumber(string text) => FindControl<TextBlock>("totalText")!.Text = text;
    public void SetCurrentPageButtonContent(string content) => FindControl<Button>("currentPageBtn")!.Content = content;
    public void SetCurrentPageButtonEnabled(bool value) => FindControl<Button>("currentPageBtn")!.IsEnabled = value;
    public void SetNumericUpDownMaximum(int value) => FindControl<NumericUpDown>("numericPage")!.Maximum = value;
    public void SetNumericUpDownValue(int value) => FindControl<NumericUpDown>("numericPage")!.Value = value;
    public int GetNumericUpDownValue() => (int)(FindControl<NumericUpDown>("numericPage")!.Value ?? 1);
    public void SetListBoxContent(IEnumerable<Item> items)
    {
        itemControls.Clear();
        var container = FindControl<StackPanel>("itemsContainer")!; // or your ItemsControl
        container.Children.Clear();

        foreach (var item in items)
        {
            var ctrl = new ItemControl { DataContext = item };
            itemControls.Add(ctrl);
            container.Children.Add(ctrl);
        }
    }
    public void SetCover(int index, byte[]? bytes)
    {
        if (index < 0 || index >= itemControls.Count) return;
        itemControls[index].SetCover(bytes);
    }
    public void SetFirstButtonAndPrevButtonEnabled(bool value)
    {
        FindControl<Button>("btnFirst")!.IsEnabled = value;
        FindControl<Button>("btnPrev")!.IsEnabled = value;
    }
    public void SetLastButtonAndNextButtonEnabled(bool value)
    {
        FindControl<Button>("btnNext")!.IsEnabled = value;
        FindControl<Button>("btnLast")!.IsEnabled = value;
    }
    public void HideFlyout() { }
    public void SetErrorMessage(string text) => FindControl<TextBlock>("errorText")!.Text = text;
    public string? GetFilterText() => FindControl<TextBox>("txtFilter")!.Text?.Trim();

    public void OpenMangaDetail(string mangaUrl)
    {
        new MangaDetail.View(mangaUrl).Show();
    }
}