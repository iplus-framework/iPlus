using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Avalonia.Headless;

namespace avtest;

public class TestCheckBox : CheckBox
{
    public static readonly StyledProperty<double> BoxSizeFactorProperty =
        AvaloniaProperty.Register<TestCheckBox, double>(nameof(BoxSizeFactor), 0.8);

    public double BoxSizeFactor
    {
        get => GetValue(BoxSizeFactorProperty);
        set => SetValue(BoxSizeFactorProperty, value);
    }

    public static readonly StyledProperty<double> BoxSizeProperty =
        AvaloniaProperty.Register<TestCheckBox, double>(nameof(BoxSize), 0.0);

    public double BoxSize
    {
        get => GetValue(BoxSizeProperty);
        set => SetValue(BoxSizeProperty, value);
    }

    private Border _boxBorder;

    static TestCheckBox()
    {
        MaxHeightProperty.OverrideDefaultValue<TestCheckBox>(50.0);
        AffectsMeasure<TestCheckBox>(BoxSizeFactorProperty);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _boxBorder = e.NameScope.Find<Border>("PART_Border");
        Console.WriteLine($"OnApplyTemplate: _boxBorder={_boxBorder != null}");
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Size desiredSize = base.MeasureOverride(availableSize);
        double availableHeight = availableSize.Height;
        double baseHeight = double.IsInfinity(availableHeight) ? desiredSize.Height : Math.Min(availableHeight, MaxHeight);
        if (!double.IsInfinity(availableSize.Width) && availableSize.Width < baseHeight)
            baseHeight = availableSize.Width;
        double boxSize = baseHeight * BoxSizeFactor;
        Console.WriteLine($"Measure: avail={availableSize} baseMeasure={desiredSize} boxSize={boxSize} prevBoxSize={BoxSize}");
        if (Math.Abs(boxSize - BoxSize) > 0.1)
        {
            SetCurrentValue(BoxSizeProperty, boxSize);
            if (_boxBorder != null)
            {
                _boxBorder.Width = boxSize;
                _boxBorder.Height = boxSize;
            }
            // Avalonia does not propagate measure invalidation to parents, so the
            // template root must be invalidated explicitly to re-measure with the new box size.
            var templateRoot = this.GetVisualChildren().OfType<Layoutable>().FirstOrDefault();
            templateRoot?.InvalidateMeasure();
            desiredSize = base.MeasureOverride(availableSize);
        }
        if (boxSize > desiredSize.Height)
            desiredSize = new Size(desiredSize.Width, boxSize);
        Console.WriteLine($"Measure result: {desiredSize}");
        return desiredSize;
    }
}

public class TestApp : Application
{
    public override void Initialize()
    {
        var theme = new ControlTheme(typeof(TestCheckBox));
        theme.Setters.Add(new Setter(TemplatedControl.TemplateProperty,
            new FuncControlTemplate<TestCheckBox>((c, ns) => BuildTemplate(c, ns))));
        theme.Setters.Add(new Setter(TemplatedControl.PaddingProperty, new Thickness(5, 0, 0, 0)));
        Resources[typeof(TestCheckBox)] = theme;
    }

    private static Control BuildTemplate(TestCheckBox c, INameScope ns)
    {
        var boxBorder = new Border
        {
            Name = "PART_Border",
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(1),
            Background = Brushes.LightGray,
            CornerRadius = new CornerRadius(2),
        };
        var viewbox = new Viewbox { Stretch = Stretch.Uniform };
        var panel = new Panel();
        panel.Children.Add(new Panel { Width = 16, Height = 16 });
        var path = new Avalonia.Controls.Shapes.Path
        {
            Name = "CheckGlyph",
            Data = StreamGeometry.Parse("F1 M 0,9L 4.5,16L 16,2L 4.5,12L 0,9.0 Z"),
            Fill = Brushes.Red,
            Stretch = Stretch.Uniform,
            Stroke = Brushes.DarkRed,
            StrokeThickness = 1,
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round,
        };
        panel.Children.Add(path);
        viewbox.Child = panel;
        boxBorder.Child = viewbox;

        var grid = new Grid();
        grid.Name = "PART_Root";
        ns.Register("PART_Root", grid);
        ns.Register("PART_Border", boxBorder);
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        Grid.SetColumn(boxBorder, 0);
        grid.Children.Add(boxBorder);
        var presenter = new ContentPresenter { Name = "PART_ContentPresenter", Margin = new Thickness(5, 0, 0, 0) };
        presenter.Bind(ContentPresenter.ContentProperty, c.GetObservable(TestCheckBox.ContentProperty));
        Grid.SetColumn(presenter, 1);
        grid.Children.Add(presenter);
        return grid;
    }
}

public static class Program
{
    private static ControlTheme Theme => ((TestApp)Application.Current).Resources[typeof(TestCheckBox)] as ControlTheme;

    public static void Main()
    {
        AppBuilder.Configure<TestApp>().UseHeadless(new Avalonia.Headless.AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).UseSkia().SetupWithoutStarting();

        // Scenario 1: tree-like, available {30, 28}
        var cb = new TestCheckBox { Theme = Theme };
        cb.Measure(new Size(30, 28));
        cb.Arrange(new Rect(0, 0, 30, 28));
        Console.WriteLine($"S1 tree: DesiredSize={cb.DesiredSize} BoxSize={cb.BoxSize}");
        DumpVisual(cb, "S1");

        // Scenario 2: datagrid-like, available {45, 50}
        var cb2 = new TestCheckBox { Theme = Theme };
        cb2.Measure(new Size(45, 50));
        cb2.Arrange(new Rect(0, 0, 45, 50));
        Console.WriteLine($"S2 grid: DesiredSize={cb2.DesiredSize} BoxSize={cb2.BoxSize}");
        DumpVisual(cb2, "S2");

        // Scenario 3: caption + RTL
        var cb3 = new TestCheckBox { Theme = Theme, Content = "Active", FlowDirection = FlowDirection.LeftToRight };
        Render(cb3, "/tmp/avtest_s3_ltr.png", 160, 30);
        DumpVisual(cb3, "S3-LTR");
        var cb4 = new TestCheckBox { Theme = Theme, Content = "Active", FlowDirection = FlowDirection.RightToLeft };
        Render(cb4, "/tmp/avtest_s4_rtl.png", 160, 30);
        DumpVisual(cb4, "S4-RTL");

        // Render scenarios to PNG
        Render(cb, "/tmp/avtest_s1.png", 60, 40);
        Render(cb2, "/tmp/avtest_s2.png", 80, 60);
        for (int h = 20; h <= 60; h += 10)
        {
            var cbx = new TestCheckBox { Theme = Theme };
            Render(cbx, $"/tmp/avtest_h{h}.png", h + 30, h);
        }
        TreeScenario.Run(Theme);
    }

    private static void Render(TestCheckBox cb, string path, double w, double h)
    {
        cb.Measure(new Size(w, h));
        cb.Arrange(new Rect(0, 0, w, h));
        var rtb = new Avalonia.Media.Imaging.RenderTargetBitmap(new Avalonia.PixelSize((int)w, (int)h));
        rtb.Render(cb);
        rtb.Save(path);
        Console.WriteLine($"Saved {path}");
    }

    private static void DumpVisual(TestCheckBox cb, string tag)
    {
        var root = cb.GetVisualChildren().OfType<Layoutable>().FirstOrDefault();
        Console.WriteLine($"{tag} root desired={root.DesiredSize} bounds={root.Bounds}");
        var border = root.GetVisualChildren().OfType<Border>().FirstOrDefault();
        Console.WriteLine($"{tag} border desired={border.DesiredSize} bounds={border.Bounds}");
        var presenter = root.GetVisualChildren().OfType<ContentPresenter>().FirstOrDefault();
        Console.WriteLine($"{tag} presenter content={presenter?.Content} desired={presenter?.DesiredSize} bounds={presenter?.Bounds}");
    }
}

public static class TreeScenario
{
    public static void Run(ControlTheme theme)
    {
        // Mimic VBTreeViewItem header: StackPanel -> Border -> Grid -> ContentPresenter -> Grid(30/*) -> checkbox(Width=30)
        var cb = new TestCheckBox { Theme = theme, Width = 30, MinWidth = 30 };
        var inner = new Grid();
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30), MinWidth = 30 });
        inner.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        Grid.SetColumn(cb, 0);
        inner.Children.Add(cb);
        var presenter = new ContentPresenter { Content = inner };
        var header = new Grid();
        header.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 19 });
        header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        Grid.SetColumn(presenter, 1);
        header.Children.Add(presenter);
        var border = new Border { BorderThickness = new Thickness(1) };
        border.Child = header;
        var sp = new StackPanel();
        sp.Children.Add(border);
        sp.Measure(new Size(400, double.PositiveInfinity));
        sp.Arrange(new Rect(0, 0, 400, sp.DesiredSize.Height));
        Console.WriteLine($"TREE: cb.Desired={cb.DesiredSize} BoxSize={cb.BoxSize} sp.Desired={sp.DesiredSize}");
    }
}
