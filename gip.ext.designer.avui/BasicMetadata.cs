// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Platform;
using AvRichTextBox;
using gip.ext.design.avui;

namespace gip.ext.designer.avui
{
	public static class BasicMetadata
	{
		static bool registered;

		public static void Register()
		{
			if (registered) return;
			registered = true;

			Metadata.AddStandardValues(typeof(Brush), typeof(Brushes));
			Metadata.AddStandardValues(typeof(Color), typeof(Colors));
            // Enums are returned as enum when calling GetStandardValues
            //Metadata.AddStandardValues(typeof(FontStretch), typeof(FontStretches));
            //Metadata.AddStandardValues(typeof(FontWeight), typeof(FontWeights));
            //Metadata.AddStandardValues(typeof(FontStyle), typeof(FontStyles));
            Metadata.AddStandardValues(typeof(Cursor), typeof(StandardCursorType));
			Metadata.AddStandardValues(typeof(PixelFormat), typeof(PixelFormats));
			Metadata.AddStandardValues(typeof(TextDecorationCollection), typeof(TextDecorations));
			Metadata.AddStandardValues(typeof(FontFamily), FontManager.Current.SystemFonts);

			Metadata.AddStandardValues(typeof(ICommand), typeof(ApplicationCommands));
			Metadata.AddStandardValues(typeof(ICommand), typeof(EditingCommands));
			Metadata.AddStandardValues(typeof(ICommand), typeof(NavigationCommands));
			Metadata.AddStandardValues(typeof(ICommand), typeof(ComponentCommands));
			Metadata.AddStandardValues(typeof(ICommand), typeof(MediaCommands));

			Metadata.AddPopularProperty(Line.StartPointProperty);
			//Metadata.AddPopularProperty(NavigationWindow.ShowsNavigationUIProperty);
			//Metadata.AddPopularProperty(FlowDocumentScrollViewer.DocumentProperty);
			//Metadata.AddPopularProperty(GridViewRowPresenterBase.ColumnsProperty);
			//Metadata.AddPopularProperty(ListView.ViewProperty);
			//Metadata.AddPopularProperty(DocumentPageView.PageNumberProperty);
			Metadata.AddPopularProperty(Popup.PlacementProperty);
			Metadata.AddPopularProperty(Popup.PlacementAnchorProperty);
			Metadata.AddPopularProperty(ScrollBar.ViewportSizeProperty);
			Metadata.AddPopularProperty(UniformGrid.RowsProperty);
			Metadata.AddPopularProperty(TabControl.TabStripPlacementProperty);
			Metadata.AddPopularProperty(Line.EndPointProperty);
			Metadata.AddPopularProperty(Polygon.PointsProperty);
			Metadata.AddPopularProperty(Polyline.PointsProperty);
			Metadata.AddPopularProperty(Path.DataProperty);
			Metadata.AddPopularProperty(HeaderedContentControl.HeaderProperty);
			//Metadata.AddPopularProperty(MediaElement.UnloadedBehaviorProperty);
			Metadata.AddPopularProperty(Shape.FillProperty);
			//Metadata.AddPopularProperty(Page.TitleProperty);
			Metadata.AddPopularProperty(ItemsControl.ItemsSourceProperty);
			Metadata.AddPopularProperty(Image.SourceProperty);
			Metadata.AddPopularProperty(TextBlock.TextProperty);
			Metadata.AddPopularProperty(DockPanel.LastChildFillProperty);
			Metadata.AddPopularProperty(Expander.IsExpandedProperty);
			Metadata.AddPopularProperty(Shape.StrokeProperty);
			Metadata.AddPopularProperty(RangeBase.ValueProperty);
            //Metadata.AddPopularProperty(SelectingItemsControl.SelectedItemProperty);
            //Metadata.AddPopularProperty(ItemsControl.ItemContainerStyleProperty);
			Metadata.AddPopularProperty(ToggleButton.IsCheckedProperty);
			Metadata.AddPopularProperty(Window.TitleProperty);
			//Metadata.AddPopularProperty(Viewport3DVisual.CameraProperty);
			//Metadata.AddPopularProperty(Frame.SourceProperty);
			Metadata.AddPopularProperty(Rectangle.RadiusXProperty);
			Metadata.AddPopularProperty(Rectangle.RadiusYProperty);
			Metadata.AddPopularProperty(Layoutable.HeightProperty);
			Metadata.AddPopularProperty(Layoutable.WidthProperty);
			Metadata.AddPopularProperty(UniformGrid.ColumnsProperty);
			Metadata.AddPopularProperty(RangeBase.MinimumProperty);
			Metadata.AddPopularProperty(RangeBase.MaximumProperty);
			Metadata.AddPopularProperty(ScrollBar.OrientationProperty);
			Metadata.AddPopularProperty(ContentControl.ContentProperty);
			Metadata.AddPopularProperty(Popup.IsOpenProperty);
			Metadata.AddPopularProperty(TextElement.FontSizeProperty);
			Metadata.AddPopularProperty(StyledElement.NameProperty);
			Metadata.AddPopularProperty(Popup.HorizontalOffsetProperty);
			Metadata.AddPopularProperty(Popup.VerticalOffsetProperty);
			Metadata.AddPopularProperty(Window.WindowStateProperty);
			Metadata.AddPopularProperty(Shape.StrokeThicknessProperty);
			Metadata.AddPopularProperty(TextElement.ForegroundProperty);
			Metadata.AddPopularProperty(Layoutable.VerticalAlignmentProperty);
			Metadata.AddPopularProperty(Button.IsDefaultProperty);
			Metadata.AddPopularProperty(Visual.RenderTransformOriginProperty);
			Metadata.AddPopularProperty(TextElement.FontFamilyProperty);
			Metadata.AddPopularProperty(Layoutable.HorizontalAlignmentProperty);
            // https://github.com/Tulesha/Avalonia.Controls.ToolBar
            //Metadata.AddPopularProperty(ToolBar.BandProperty);
            //Metadata.AddPopularProperty(ToolBar.BandIndexProperty);
            Metadata.AddPopularProperty(ItemsControl.ItemTemplateProperty);
			Metadata.AddPopularProperty(TextBlock.TextWrappingProperty);
			Metadata.AddPopularProperty(Layoutable.MarginProperty);
			Metadata.AddPopularProperty(RangeBase.LargeChangeProperty);
			Metadata.AddPopularProperty(RangeBase.SmallChangeProperty);
			Metadata.AddPopularProperty(Panel.BackgroundProperty);
			//Metadata.AddPopularProperty(Shape.StrokeMiterLimitProperty);
			Metadata.AddPopularProperty(TextElement.FontWeightProperty);
			Metadata.AddPopularProperty(StackPanel.OrientationProperty);
			Metadata.AddPopularProperty(ListBox.SelectionModeProperty);
			Metadata.AddPopularProperty(StyledElement.ThemeProperty);
			Metadata.AddPopularProperty(TextBox.TextProperty);
			Metadata.AddPopularProperty(Window.SizeToContentProperty);
			//Metadata.AddPopularProperty(Window.ResizeModeProperty);
			Metadata.AddPopularProperty(TextBlock.TextTrimmingProperty);
			Metadata.AddPopularProperty(Window.ShowInTaskbarProperty);
			Metadata.AddPopularProperty(Window.IconProperty);
			Metadata.AddPopularProperty(Visual.RenderTransformProperty);
			Metadata.AddPopularProperty(Button.IsCancelProperty);
			Metadata.AddPopularProperty(Border.BorderBrushProperty);
			//Metadata.AddPopularProperty(Block.TextAlignmentProperty);
			Metadata.AddPopularProperty(Border.CornerRadiusProperty);
			Metadata.AddPopularProperty(Border.BorderThicknessProperty);
			Metadata.AddPopularProperty(TreeViewItem.IsSelectedProperty);
			Metadata.AddPopularProperty(Border.PaddingProperty);
			Metadata.AddPopularProperty(Shape.StretchProperty);
            //Metadata.AddPopularProperty(Layoutable.VerticalContentAlignmentProperty);
            //Metadata.AddPopularProperty(Layoutable.HorizontalContentAlignmentProperty);

            Metadata.AddPopularProperty(Grid.RowProperty);
			Metadata.AddPopularProperty(Grid.RowSpanProperty);
			Metadata.AddPopularProperty(Grid.ColumnProperty);
			Metadata.AddPopularProperty(Grid.ColumnSpanProperty);
			Metadata.AddPopularProperty(DockPanel.DockProperty);
			Metadata.AddPopularProperty(Canvas.LeftProperty);
			Metadata.AddPopularProperty(Canvas.TopProperty);
			Metadata.AddPopularProperty(Canvas.RightProperty);
			Metadata.AddPopularProperty(Canvas.BottomProperty);

			Metadata.AddPopularProperty(typeof(Binding), "Path");
			Metadata.AddPopularProperty(typeof(Binding), "Source");
			Metadata.AddPopularProperty(typeof(Binding), "Mode");
			Metadata.AddPopularProperty(typeof(Binding), "RelativeSource");
			Metadata.AddPopularProperty(typeof(Binding), "ElementName");
			Metadata.AddPopularProperty(typeof(Binding), "Converter");
			Metadata.AddPopularProperty(typeof(Binding), "XPath");

            Metadata.AddPopularProperty(typeof(ItemsControl), "Items");


            //Metadata.AddValueRange(Block.LineHeightProperty, double.Epsilon, double.MaxValue);
			Metadata.AddValueRange(Canvas.BottomProperty, double.MinValue, double.MaxValue);
			Metadata.AddValueRange(Canvas.LeftProperty, double.MinValue, double.MaxValue);
			Metadata.AddValueRange(Canvas.TopProperty, double.MinValue, double.MaxValue);
			Metadata.AddValueRange(Canvas.RightProperty, double.MinValue, double.MaxValue);
			Metadata.AddValueRange(ColumnDefinition.MaxWidthProperty, 0, double.PositiveInfinity);
			//Metadata.AddValueRange(DocumentViewer.MaxPagesAcrossProperty, double.Epsilon, double.MaxValue);
			//Metadata.AddValueRange(Figure.HorizontalOffsetProperty, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(Figure.VerticalOffsetProperty, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(FlowDocument.MaxPageWidthProperty, 0, double.PositiveInfinity);
			//Metadata.AddValueRange(FlowDocument.MaxPageHeightProperty, 0, double.PositiveInfinity);
			//Metadata.AddValueRange(FlowDocumentPageViewer.ZoomProperty, double.Epsilon, double.MaxValue);
			//Metadata.AddValueRange(FlowDocumentPageViewer.ZoomIncrementProperty, double.Epsilon, double.MaxValue);
			//Metadata.AddValueRange(FlowDocumentPageViewer.MinZoomProperty, double.Epsilon, double.MaxValue);
			//Metadata.AddValueRange(FlowDocumentPageViewer.MaxZoomProperty, double.Epsilon, double.MaxValue);
			Metadata.AddValueRange(Layoutable.MaxHeightProperty, 0, double.PositiveInfinity);
			Metadata.AddValueRange(Layoutable.MaxWidthProperty, 0, double.PositiveInfinity);
			Metadata.AddValueRange(Grid.ColumnSpanProperty, double.Epsilon, double.MaxValue);
			Metadata.AddValueRange(Grid.RowSpanProperty, double.Epsilon, double.MaxValue);
			Metadata.AddValueRange(GridSplitter.KeyboardIncrementProperty, double.Epsilon, double.MaxValue);
			Metadata.AddValueRange(GridSplitter.DragIncrementProperty, double.Epsilon, double.MaxValue);
			//Metadata.AddValueRange(InkCanvas.BottomProperty, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(InkCanvas.TopProperty, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(InkCanvas.RightProperty, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(InkCanvas.LeftProperty, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(Line.Y2Property, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(Line.X1Property, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(Line.Y1Property, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(Line.X2Property, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(List.MarkerOffsetProperty, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(List.StartIndexProperty, double.Epsilon, double.MaxValue);
			//Metadata.AddValueRange(Paragraph.TextIndentProperty, double.MinValue, double.MaxValue);
			Metadata.AddValueRange(RangeBase.ValueProperty, double.MinValue, double.MaxValue);
			Metadata.AddValueRange(RangeBase.MaximumProperty, double.MinValue, double.MaxValue);
			Metadata.AddValueRange(RangeBase.MinimumProperty, double.MinValue, double.MaxValue);
			Metadata.AddValueRange(RepeatButton.IntervalProperty, double.Epsilon, double.MaxValue);
			Metadata.AddValueRange(RowDefinition.MaxHeightProperty, 0, double.PositiveInfinity);
			Metadata.AddValueRange(SelectingItemsControl.SelectedIndexProperty, double.MinValue, double.MaxValue);
			Metadata.AddValueRange(Slider.TickFrequencyProperty, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(Slider.SelectionStartProperty, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(Slider.SelectionEndProperty, double.MinValue, double.MaxValue);
			//Metadata.AddValueRange(TableCell.RowSpanProperty, double.Epsilon, double.MaxValue);
			//Metadata.AddValueRange(TableCell.ColumnSpanProperty, double.Epsilon, double.MaxValue);
			Metadata.AddValueRange(TextBox.MinLinesProperty, double.Epsilon, double.MaxValue);
			Metadata.AddValueRange(TextBox.MaxLinesProperty, double.Epsilon, double.MaxValue);
			Metadata.AddValueRange(TextBox.UndoLimitProperty, double.MinValue, double.MaxValue);
			Metadata.AddValueRange(TextElement.FontSizeProperty, double.Epsilon, double.MaxValue);
			//Metadata.AddValueRange(Timeline.SpeedRatioProperty, double.Epsilon, double.MaxValue);
			//Metadata.AddValueRange(Timeline.DecelerationRatioProperty, 0, 1);
			//Metadata.AddValueRange(Timeline.AccelerationRatioProperty, 0, 1);
			Metadata.AddValueRange(Track.ViewportSizeProperty, 0, double.PositiveInfinity);
			Metadata.AddValueRange(Visual.OpacityProperty, 0, 1);


            Metadata.HideProperty(typeof(Visual), "Bounds");
			Metadata.HideProperty(StyledElement.NameProperty);
            //Metadata.HideProperty(typeof(FrameworkElement), "Resources");
			Metadata.HideProperty(typeof(Window), "Owner");

			//Metadata.DisablePlacement(typeof(Button));

			Metadata.AddPopularControl(typeof(Button));
			Metadata.AddPopularControl(typeof(Border));
			Metadata.AddPopularControl(typeof(Canvas));
			Metadata.AddPopularControl(typeof(CheckBox));
			Metadata.AddPopularControl(typeof(ComboBox));	
			Metadata.AddPopularControl(typeof(DataGrid));
			Metadata.AddPopularControl(typeof(DockPanel));
			Metadata.AddPopularControl(typeof(Expander));
			Metadata.AddPopularControl(typeof(Grid));
			//Metadata.AddPopularControl(typeof(GroupBox));
			Metadata.AddPopularControl(typeof(Image));			
			//Metadata.AddPopularControl(typeof(InkCanvas));			
			Metadata.AddPopularControl(typeof(Label));
			Metadata.AddPopularControl(typeof(ListBox));
			//Metadata.AddPopularControl(typeof(ListView));
			//Metadata.AddPopularControl(typeof(MediaElement));
			Metadata.AddPopularControl(typeof(Menu));
			//Metadata.AddPopularControl(typeof(PasswordBox));
			Metadata.AddPopularControl(typeof(ProgressBar));
			Metadata.AddPopularControl(typeof(RadioButton));
			Metadata.AddPopularControl(typeof(RichTextBox));
			Metadata.AddPopularControl(typeof(StackPanel));
			Metadata.AddPopularControl(typeof(ScrollViewer));
			Metadata.AddPopularControl(typeof(Slider));			
			Metadata.AddPopularControl(typeof(TabControl));
			Metadata.AddPopularControl(typeof(TextBlock));
			Metadata.AddPopularControl(typeof(TextBox));
			Metadata.AddPopularControl(typeof(TreeView));
			//Metadata.AddPopularControl(typeof(ToolBar));
			Metadata.AddPopularControl(typeof(Viewbox));
			//Metadata.AddPopularControl(typeof(Viewport3D));
			Metadata.AddPopularControl(typeof(WrapPanel));
            Metadata.AddPopularControl(typeof(Line));
            Metadata.AddPopularControl(typeof(Polyline));
            Metadata.AddPopularControl(typeof(Ellipse));
            Metadata.AddPopularControl(typeof(Rectangle));
            Metadata.AddPopularControl(typeof(Path));

            //Basic Metadata Size of double.NaN, means no Size should be set.
            Metadata.AddDefaultSize(typeof(TextBlock), new Size(double.NaN, double.NaN));
            Metadata.AddDefaultSize(typeof(CheckBox), new Size(double.NaN, double.NaN));
            Metadata.AddDefaultSize(typeof(Image), new Size(double.NaN, double.NaN));

            Metadata.AddDefaultSize(typeof(Control), new Size(120, 100));
            Metadata.AddDefaultSize(typeof(ContentControl), new Size(120, 20));
            Metadata.AddDefaultSize(typeof(Button), new Size(75, 23));
            Metadata.AddDefaultSize(typeof(ToggleButton), new Size(75, 23));

            var s1 = new Size(120, 20);
            Metadata.AddDefaultSize(typeof(Slider), s1);
            Metadata.AddDefaultSize(typeof(TextBox), s1);
            //Metadata.AddDefaultSize(typeof(PasswordBox), s1);
            Metadata.AddDefaultSize(typeof(ComboBox), s1);
            Metadata.AddDefaultSize(typeof(ProgressBar), s1);

            //Metadata.AddDefaultSize(typeof(ToolBar), s1);
            Metadata.AddDefaultSize(typeof(Menu), s1);
            
            var s2=new Size(120,120);
            //Metadata.AddDefaultSize(typeof(InkCanvas),s2);
            Metadata.AddDefaultSize(typeof(Image),s2);
            Metadata.AddDefaultSize(typeof(TreeView),s2);
            
            var s3=new Size(130,120);
            Metadata.AddDefaultSize(typeof(Label),s3);
            Metadata.AddDefaultSize(typeof(Expander),s3);

            Metadata.AddDefaultPropertyValue(typeof(Line), Line.StartPointProperty, new Point(0,0));
            Metadata.AddDefaultPropertyValue(typeof(Line), Line.EndPointProperty, new Point(20.0, 20.0));
            Metadata.AddDefaultPropertyValue(typeof(Line), Line.StrokeProperty, Brushes.Black);
            Metadata.AddDefaultPropertyValue(typeof(Line), Line.StrokeThicknessProperty, 2d);
            Metadata.AddDefaultPropertyValue(typeof(Line), Line.StretchProperty, Stretch.None);

            Metadata.AddDefaultPropertyValue(typeof(Polyline), Polyline.PointsProperty, new List<Point>() { new Point(0, 0), new Point(20, 0), new Point(20, 20) });
            Metadata.AddDefaultPropertyValue(typeof(Polyline), Polyline.StrokeProperty, Brushes.Black);
            Metadata.AddDefaultPropertyValue(typeof(Polyline), Polyline.StrokeThicknessProperty, 2d);
            Metadata.AddDefaultPropertyValue(typeof(Polyline), Polyline.StretchProperty, Stretch.None);

            Metadata.AddDefaultPropertyValue(typeof(Polygon), Polygon.PointsProperty, new List<Point>() { new Point(0, 20), new Point(20, 20), new Point(10, 0) });
            Metadata.AddDefaultPropertyValue(typeof(Polygon), Polygon.StrokeProperty, Brushes.Black);
            Metadata.AddDefaultPropertyValue(typeof(Polygon), Polygon.StrokeThicknessProperty, 2d);
            Metadata.AddDefaultPropertyValue(typeof(Polygon), Polygon.StretchProperty, Stretch.None);

            Metadata.AddDefaultPropertyValue(typeof(Path), Path.StrokeProperty, Brushes.Black);
            Metadata.AddDefaultPropertyValue(typeof(Path), Path.StrokeThicknessProperty, 2d);
            Metadata.AddDefaultPropertyValue(typeof(Path), Path.StretchProperty, Stretch.None);

            Metadata.AddDefaultPropertyValue(typeof(Rectangle), Rectangle.FillProperty, Brushes.Transparent);
            Metadata.AddDefaultPropertyValue(typeof(Rectangle), Rectangle.StrokeProperty, Brushes.Black);
            Metadata.AddDefaultPropertyValue(typeof(Rectangle), Rectangle.StrokeThicknessProperty, 2d);

            Metadata.AddDefaultPropertyValue(typeof(Ellipse), Ellipse.FillProperty, Brushes.Transparent);
            Metadata.AddDefaultPropertyValue(typeof(Ellipse), Ellipse.StrokeProperty, Brushes.Black);
            Metadata.AddDefaultPropertyValue(typeof(Ellipse), Ellipse.StrokeThicknessProperty, 2d);
        }
	}
}
