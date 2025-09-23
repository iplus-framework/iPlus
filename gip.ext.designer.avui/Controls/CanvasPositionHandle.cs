// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Diagnostics;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui;
using Avalonia.Controls.Shapes;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Reactive;
using Avalonia.Controls.Primitives;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// Adorner that displays the margin of a control in a Grid.
	/// </summary>
	public class CanvasPositionHandle : MarginHandle
	{
		static CanvasPositionHandle()
		{
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(CanvasPositionHandle), new FrameworkPropertyMetadata(typeof(CanvasPositionHandle)));
			HandleLengthOffset=2;
		}

		private Path line1;
		private Path line2;
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
			line1 = e.NameScope.Find<Path>("line1") as Path;
			line2 = e.NameScope.Find<Path>("line2") as Path;

            base.OnApplyTemplate(e);
        }

		readonly Canvas _Canvas;
		//readonly DesignItem _AdornedControlItem;
		readonly AdornerPanel _AdornerPanel;
		readonly HandleOrientation _Orientation;
		readonly Control _AdornedControl;

		/// <summary> This grid contains the handle line and the endarrow.</summary>
//		Grid lineArrow;

		public CanvasPositionHandle(DesignItem adornedControlItem, AdornerPanel adornerPanel, HandleOrientation orientation)
		{
			Debug.Assert(adornedControlItem != null);
			//_AdornedControlItem = adornedControlItem;
            // base:
            //this.adornedControlItem = adornedControlItem;
            //this.adornerPanel = adornerPanel;
            //this.orientation = orientation;
            //Angle = (double) orientation;

            _Canvas = (Canvas) adornedControlItem.Parent.Component;
			_AdornedControl = (Control) adornedControlItem.Component;
			_AdornerPanel = adornerPanel;
			_Orientation = orientation;

            //base:
            //Stub = new MarginStub(this);
            //ShouldBeVisible = true;

            _AdornedControl.GetObservable(Canvas.LeftProperty).Subscribe(_ => BindAndPlaceHandle());
            _AdornedControl.GetObservable(Canvas.RightProperty).Subscribe(_ => BindAndPlaceHandle());
            _AdornedControl.GetObservable(Canvas.TopProperty).Subscribe(_ => BindAndPlaceHandle());
            _AdornedControl.GetObservable(Canvas.BottomProperty).Subscribe(_ => BindAndPlaceHandle());
            _AdornedControl.GetObservable(Layoutable.WidthProperty).Subscribe(_ => BindAndPlaceHandle());
            _AdornedControl.GetObservable(Layoutable.HeightProperty).Subscribe(_ => BindAndPlaceHandle());
			BindAndPlaceHandle();
		}
	
		/// <summary>
		/// Binds the <see cref="MarginHandle.HandleLength"/> to the margin and place the handles.
		/// </summary>
		void BindAndPlaceHandle()
		{
			if (!_AdornerPanel.Children.Contains(this))
                _AdornerPanel.Children.Add(this);
			if (!_AdornerPanel.Children.Contains(Stub))
                _AdornerPanel.Children.Add(Stub);
			RelativePlacement placement=new RelativePlacement();
			switch (_Orientation)
			{
				case HandleOrientation.Left:
					{
                        _AdornedControl.GetValue(Canvas.LeftProperty);
                        var wr = (double) _AdornedControl.GetValue(Canvas.LeftProperty);
                        if (double.IsNaN(wr))
						{
							wr = (double) _AdornedControl.GetValue(Canvas.RightProperty);
							wr = _Canvas.Width - (PlacementOperation.GetRealElementSize(_AdornedControl).Width + wr);
						}
						else
						{
							if (line1 != null)
							{
								line1.StrokeDashArray.Clear();
								line2.StrokeDashArray.Clear();
							}
						}
						this.HandleLength = wr;
						placement = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Center);
						placement.XOffset = -HandleLengthOffset;
						break;
					}
				case HandleOrientation.Top:
					{
						var wr = (double) _AdornedControl.GetValue(Canvas.TopProperty);
						if (double.IsNaN(wr))
						{
							wr = (double)_AdornedControl.GetValue(Canvas.BottomProperty);
                            wr = _Canvas.Height - (PlacementOperation.GetRealElementSize(_AdornedControl).Height + wr);
						}
						else
						{
							if (line1 != null)
							{
								line1.StrokeDashArray.Clear();
								line2.StrokeDashArray.Clear();
							}
						}
						this.HandleLength = wr;
						placement = new RelativePlacement(HorizontalAlignment.Center, VerticalAlignment.Top);
						placement.YOffset = -HandleLengthOffset;
						break;
					}
				case HandleOrientation.Right:
					{
						var wr = (double)_AdornedControl.GetValue(Canvas.RightProperty);
						if (double.IsNaN(wr))
						{
							wr = (double) _AdornedControl.GetValue(Canvas.LeftProperty);
                            wr = _Canvas.Width - (PlacementOperation.GetRealElementSize(_AdornedControl).Width + wr);
						}
						else
						{
							if (line1 != null)
							{
								line1.StrokeDashArray.Clear();
								line2.StrokeDashArray.Clear();
							}
						}
						this.HandleLength = wr;
						placement = new RelativePlacement(HorizontalAlignment.Right, VerticalAlignment.Center);
						placement.XOffset = HandleLengthOffset;
						break;
					}
				case HandleOrientation.Bottom:
					{
						var wr = (double)_AdornedControl.GetValue(Canvas.BottomProperty);
                        if (double.IsNaN(wr))
						{
							wr = (double)_AdornedControl.GetValue(Canvas.TopProperty);
                            wr = _Canvas.Height - (PlacementOperation.GetRealElementSize(_AdornedControl).Height + wr);
						}
						else
						{
							if (line1 != null)
							{
								line1.StrokeDashArray.Clear();
								line2.StrokeDashArray.Clear();
							}
						}
						this.HandleLength = wr;
						placement = new RelativePlacement(HorizontalAlignment.Center, VerticalAlignment.Bottom);
						placement.YOffset = HandleLengthOffset;
						break;
					}
			}

			AdornerPanel.SetPlacement(this, placement);
			this.IsVisible = true;
		}
	}
}
