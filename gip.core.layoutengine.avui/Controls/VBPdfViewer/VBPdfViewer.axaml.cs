using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using gip.core.layoutengine.avui.Internals;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using SkiaSharp;
using PdfConvert = PDFtoImage.Conversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gip.core.layoutengine.avui;

public partial class VBPdfViewer : UserControl, IDisposable
{
    public static readonly DirectProperty<VBPdfViewer, string> SourceProperty =
        AvaloniaProperty.RegisterDirect<VBPdfViewer, string>(nameof(Source), o => o.Source, (o, v) => o.Source = v);

    public static readonly DirectProperty<VBPdfViewer, bool> ShowSidebarProperty =
        AvaloniaProperty.RegisterDirect<VBPdfViewer, bool>(nameof(ShowSidebar), o => o.ShowSidebar, (o, v) => o.ShowSidebar = v);

    public static readonly DirectProperty<VBPdfViewer, PdfZoom?> ZoomProperty =
        AvaloniaProperty.RegisterDirect<VBPdfViewer, PdfZoom?>(nameof(PdfZoom), o => o.PdfZoom, (o, v) => o.PdfZoom = v);
    static VBPdfViewer()
    {
        SourceProperty.Changed.AddClassHandler<VBPdfViewer>((x, e) => x.Load(e));
    }
    public VBPdfViewer()
    {
        _thumbnailImagesCache.Connect()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .SortAndBind(out _thumbnailImages, SortExpressionComparer<DrawableThumbnailImage>.Ascending(i => i.Index))
            .DisposeMany()
            .Subscribe();
        
        //add default zoom levels
        _zoomLevelsCache.AddRange(_defaultZoomLevels);
        
        _zoomLevelsCache.Connect()
            .Sort(SortExpressionComparer<PdfZoom>.Ascending(z => z))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Bind(out _zoomLevels)
            .Subscribe();
        
        InitializeComponent();
    }
    
    private string _source;
    public string Source
    {
        get => _source;
        set => SetAndRaise(SourceProperty, ref _source, value);
    }
    
    public bool ShowSidebar
    {
        get => ExpandSidebarButton.IsChecked ?? false;
        set
        {
            ExpandSidebarButton.IsChecked = value;
            RaisePropertyChanged(ShowSidebarProperty, oldValue: !value, value);
        }
    }

    private readonly List<PdfZoom> _defaultZoomLevels =
    [
        gip.core.layoutengine.avui.PdfZoom.Automatic,
        0.25,
        0.5,
        0.75,
        1,
        1.25,
        1.5,
        1.75,
        2,
        2.5,
        3,
        3.5,
        4,
        4.5,
        5
    ];
    
    // ReSharper disable once MemberCanBePrivate.Global
    public int ThumbnailCacheSize { get; set; } = 10;
    
    private readonly SourceCache<DrawableThumbnailImage, int> _thumbnailImagesCache = new(i => i.PageNumber);
    private readonly ReadOnlyObservableCollection<DrawableThumbnailImage> _thumbnailImages;
    // ReSharper disable once UnusedMember.Local
    private ReadOnlyObservableCollection<DrawableThumbnailImage> ThumbnailImages => _thumbnailImages;
    
    private readonly SourceList<PdfZoom> _zoomLevelsCache = new();
    private readonly ReadOnlyObservableCollection<PdfZoom> _zoomLevels;
    private ReadOnlyObservableCollection<PdfZoom> ZoomLevels => _zoomLevels;
    
    private Stream _fileStream = null;
    private DisposingLimitCache<int, SKBitmap> _bitmapCache;
    private bool _loading;

    private void Load(AvaloniaPropertyChangedEventArgs args)
    {
        try
        {
            _loading = true;
            CleanUp();

            var source = args.NewValue as string;
            if (string.IsNullOrWhiteSpace(source) || !File.Exists(source)) return;

            _fileStream = File.OpenRead(source);

            var doc = PdfDocumentReflection.Load(_fileStream, null, false);
            var sizes = PdfDocumentReflection.PageSizes(doc);

            PageSelector.Value = 1;
            PageSelector.Maximum = sizes.Count;
            PageCount.Text = sizes.Count.ToString();

            _bitmapCache ??= new DisposingLimitCache<int, SKBitmap>(ThumbnailCacheSize);

            _thumbnailImagesCache.Edit(edit =>
            {
                edit.Load(sizes.Select((size, index) =>
                    new DrawableThumbnailImage(size, _fileStream, index, _bitmapCache)));
            });

#pragma warning disable CA1416
            var skbitMap = PdfConvert.ToImage(_fileStream, new Index(0), leaveOpen: true);
#pragma warning restore CA1416

            MainImage.Source = skbitMap.ToAvaloniaImage();
            ApplyZoom();
        }
        finally
        {
            _loading = false;
        }
    }

    private void ThumbnailListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(_loading) return;
        PageSelector.Value = ThumbnailListBox.SelectedIndex + 1;
        SetPage(ThumbnailListBox.SelectedIndex);
    }

    private void PageSelector_OnValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_loading || e.NewValue == null) return;
        var pageNumber = (int)e.NewValue;
        ThumbnailListBox.SelectedIndex = pageNumber - 1;
        SetPage(pageNumber - 1);
    }

    private void SetPage(int index)
    {
        CleanUpMainImage();
        if (_fileStream == null || index < 0) return;
#pragma warning disable CA1416
        MainImage.Source = PdfConvert.ToImage(_fileStream, new Index(index), leaveOpen: true).ToAvaloniaImage();
#pragma warning restore CA1416
        ApplyZoom();
    }

    private void CleanUp()
    {
        CleanUpMainImage();
        foreach (var drawableThumbnailImage in _thumbnailImagesCache.Items)
        {
            drawableThumbnailImage.Dispose();
        }
        _thumbnailImagesCache.Clear();
        _bitmapCache?.Dispose();
        _bitmapCache = null;
        _fileStream?.Dispose();
        PageSelector.Value = null;
        PageSelector.Maximum = 0;
        PageCount.Text = "0";
    }
    
    private void CleanUpMainImage()
    {
        if (MainImage.Source is IDisposable disposable)
        {
            MainImage.Source = null;
            disposable.Dispose();
        }
    }
    
    public void Dispose()
    {
        CleanUp();
        _thumbnailImagesCache.Dispose();
    }

    private void ApplyZoom()
    {
        if (ZoomCombobox.SelectedItem is not PdfZoom zoom) return;
        if (zoom > 0)
        {
            PercentageZoom(zoom);
        }
        else
        {
            AutomaticZoom();
        }
    }

    private void AutomaticZoom()
    {
        var width = MainImageScrollViewer.Bounds.Size.Width;
        var height = MainImageScrollViewer.Bounds.Size.Height;
        if (ZoomCombobox.SelectedItem is not PdfZoom currentZoom) return;
        if (width <= 0 || height <= 0) return;
        MainImage.Width = width;
        MainImage.Height = height;
        RaisePropertyChanged(ZoomProperty, oldValue: currentZoom, 0);
    }
    
    public PdfZoom? PdfZoom
    {
        get => ZoomCombobox.SelectedItem as PdfZoom?;
        set => ZoomTo(value ?? 0);
    }

    private void ZoomTo(PdfZoom zoom)
    {
        if (ZoomCombobox.SelectedItem is not PdfZoom currentZoom) return;
        if (currentZoom == 0)
        {
            //todo calculate the current percentage zoom based on the relative size of the image
            if (PageSelector.Value == null) return;
            var viewWidth = MainImageScrollViewer.Bounds.Size.Width;
            var viewHeight = MainImageScrollViewer.Bounds.Size.Height;
            var index = ((int)PageSelector.Value) - 1;
            var imageSize = ThumbnailImages[index].Size;
            
            //todo clamp to nearest 25% increment
        }
        
        if (!_defaultZoomLevels.Contains(currentZoom))
        {
            _zoomLevelsCache.Remove(currentZoom);
        }
        if (zoom > 0)
        {
            if (!_defaultZoomLevels.Contains(zoom))
            {
                _zoomLevelsCache.Add(zoom);
            }
            
            ZoomCombobox.SelectedIndex = ZoomLevels.IndexOf(zoom);
        }
        
        RaisePropertyChanged(ZoomProperty, oldValue: currentZoom, zoom);
    }
    private void PercentageZoom(double percentage)
    {
        if (percentage <= 0) return;
        if (PageSelector.Value == null) return;
        if (ZoomCombobox.SelectedItem is not PdfZoom currentZoom) return;
        var index = ((int)PageSelector.Value) - 1;
        var imageSize = ThumbnailImages[index].Size;
        MainImage.Width = imageSize.Width * percentage;
        MainImage.Height = imageSize.Height * percentage;
        RaisePropertyChanged(ZoomProperty, oldValue: currentZoom, percentage);
    }

    private void ZoomCombobox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(ZoomCombobox == null) return;
        ApplyZoom();
    }

    private void ZoomOutButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ZoomCombobox.SelectedItem is not PdfZoom currentZoom) return;
        var zoom = currentZoom - 0.25;
        ZoomTo(zoom);
    }

    private void ZoomInButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ZoomCombobox.SelectedItem is not PdfZoom currentZoom) return;
        var zoom = currentZoom + 0.25;
        ZoomTo(zoom);
    }

    private void MainImageScrollViewer_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ZoomCombobox.SelectedItem is not PdfZoom zoom || zoom != 0) return;
        AutomaticZoom();
    }

    private bool _pointerDown;
    private Point? _lastPosition;
    private void MainImageScrollViewer_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            if (ZoomCombobox.SelectedItem is not PdfZoom currentZoom) return;
            var zoom = currentZoom - 0.25;
            ZoomTo(zoom);
            var position = e.GetPosition(MainImageScrollViewer);
            MainImageScrollViewer.Offset = position;
        }
        else if(e.ClickCount > 1 || e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            if (ZoomCombobox.SelectedItem is not PdfZoom currentZoom) return;
            var zoom = currentZoom + 0.25;
            ZoomTo(zoom);
            var position = e.GetPosition(MainImageScrollViewer);
            MainImageScrollViewer.Offset = position;
        }
        else
        {
            _pointerDown = true;
        }
    }

    private void MainImageScrollViewer_OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        _pointerDown = false;
        _lastPosition = null;
    }
    
    private void MainImageScrollViewer_OnPointerMoved(object sender, PointerEventArgs e)
    {
        if(!_pointerDown) return;
        var currentPosition = e.GetPosition(MainImageScrollViewer);
        if (_lastPosition != null)
        {
            var delta = currentPosition - _lastPosition.Value;
            MainImageScrollViewer.Offset -= delta;
        }
        _lastPosition = currentPosition;
    }
}