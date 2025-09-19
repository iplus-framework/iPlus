// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui.Extensions;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace gip.ext.designer.avui.Extensions
{
    [ExtensionFor(typeof(Control))]
    [ExtensionServer(typeof(PrimarySelectionExtensionServer))]
    public class MarginHandleExtension : AdornerProvider
    {
        private MarginHandle[] _handles;
        private MarginHandle _leftHandle, _topHandle, _rightHandle, _bottomHandle;
        private Grid _grid;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (this.ExtendedItem.Parent != null)
            {
                if (this.ExtendedItem.Parent.ComponentType == typeof(Grid))
                {
                    Control extendedControl = (Control)this.ExtendedItem.Component;
                    LayoutTransformControl layoutTransformControl = extendedControl as LayoutTransformControl;
                    AdornerPanel adornerPanel = new AdornerPanel();

                    // If the Element is rotated/skewed in the grid, then margin handles do not appear
                    if ((layoutTransformControl != null && layoutTransformControl.LayoutTransform.Value == Matrix.Identity)
                        || extendedControl.RenderTransform.Value == Matrix.Identity)
                    {
                        _grid = this.ExtendedItem.Parent.View as Grid;
                        _handles = new[]
                        {
                            _leftHandle = new MarginHandle(ExtendedItem, adornerPanel, HandleOrientation.Left),
                            _topHandle = new MarginHandle(ExtendedItem, adornerPanel, HandleOrientation.Top),
                            _rightHandle = new MarginHandle(ExtendedItem, adornerPanel, HandleOrientation.Right),
                            _bottomHandle = new MarginHandle(ExtendedItem, adornerPanel, HandleOrientation.Bottom),
                        };
                        foreach (var handle in _handles)
                        {
                            handle.PointerPressed += OnMouseDown;
                            handle.Stub.PointerPressed += OnMouseDownPreview;
                        }


                    }

                    if (adornerPanel != null)
                        this.Adorners.Add(adornerPanel);
                }
            }
        }

        #region Change margin through handle/stub
        private void OnMouseDownPreview(object sender, PointerPressedEventArgs e)
        {
            if (e.Route != RoutingStrategies.Tunnel)
                return;
            OnMouseDown(sender, e);
        }

        private void OnMouseDown(object sender, PointerPressedEventArgs e)
        {
            if (!e.Properties.IsLeftButtonPressed)
                return;

            e.Handled = true;
            var row = (int)this.ExtendedItem.Properties.GetAttachedProperty(Grid.RowProperty).ValueOnInstance;
            var rowSpan = (int)this.ExtendedItem.Properties.GetAttachedProperty(Grid.RowSpanProperty).ValueOnInstance;

            var column = (int)this.ExtendedItem.Properties.GetAttachedProperty(Grid.ColumnProperty).ValueOnInstance;
            var columnSpan = (int)this.ExtendedItem.Properties.GetAttachedProperty(Grid.ColumnSpanProperty).ValueOnInstance;

            var margin = (Thickness)this.ExtendedItem.Properties[Layoutable.MarginProperty].ValueOnInstance;
            double left = margin.Left;
            double top = margin.Top;
            double right = margin.Right;
            double bottom = margin.Bottom;

            var point = this.ExtendedItem.View.TranslatePoint(new Point(), _grid);
            var position = new Rect(point.Value, this.ExtendedItem.View.Bounds.Size);
            MarginHandle handle = null;
            if (sender is MarginHandle)
                handle = sender as MarginHandle;
            if (sender is MarginStub)
                handle = ((MarginStub)sender).Handle;
            if (handle != null)
            {
                switch (handle.Orientation)
                {
                    case HandleOrientation.Left:
                        if (_rightHandle.IsVisible)
                        {
                            if (_leftHandle.IsVisible)
                            {
                                left = 0;
                                this.ExtendedItem.Properties[Layoutable.WidthProperty].SetValue(position.Width);
                                this.ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Right);
                            }
                            else
                            {
                                var leftMargin = position.Left - GetColumnOffset(column);
                                left = leftMargin;
                                this.ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty].Reset();
                            }
                        }
                        else
                        {
                            if (_leftHandle.IsVisible)
                            {
                                left = 0;
                                var rightMargin = GetColumnOffset(column + columnSpan) - position.Right;
                                right = rightMargin;

                                this.ExtendedItem.Properties[Layoutable.WidthProperty].SetValue(position.Width);
                                this.ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Right);
                            }
                            else
                            {
                                var leftMargin = position.Left - GetColumnOffset(column);
                                left = leftMargin;
                                this.ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Left);
                            }
                        }
                        break;
                    case HandleOrientation.Top:
                        if (_bottomHandle.IsVisible)
                        {
                            if (_topHandle.IsVisible)
                            {
                                top = 0;
                                this.ExtendedItem.Properties[Layoutable.HeightProperty].SetValue(position.Height);
                                this.ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty].SetValue(VerticalAlignment.Bottom);
                            }
                            else
                            {
                                var topMargin = position.Top - GetRowOffset(row);
                                top = topMargin;
                                this.ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty].Reset();
                            }
                        }
                        else
                        {
                            if (_topHandle.IsVisible)
                            {
                                top = 0;
                                var bottomMargin = GetRowOffset(row + rowSpan) - position.Bottom;
                                bottom = bottomMargin;

                                this.ExtendedItem.Properties[Layoutable.HeightProperty].SetValue(position.Height);
                                this.ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty].SetValue(VerticalAlignment.Bottom);
                            }
                            else
                            {
                                var topMargin = position.Top - GetRowOffset(row);
                                top = topMargin;
                                this.ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty].SetValue(VerticalAlignment.Top);
                            }
                        }
                        break;
                    case HandleOrientation.Right:
                        if (_leftHandle.IsVisible)
                        {
                            if (_rightHandle.IsVisible)
                            {
                                right = 0;
                                this.ExtendedItem.Properties[Layoutable.WidthProperty].SetValue(position.Width);
                                this.ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Left);
                            }
                            else
                            {
                                var rightMargin = GetColumnOffset(column + columnSpan) - position.Right;
                                right = rightMargin;
                                this.ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty].Reset();
                            }
                        }
                        else
                        {
                            if (_rightHandle.IsVisible)
                            {
                                right = 0;
                                var leftMargin = position.Left - GetColumnOffset(column);
                                left = leftMargin;

                                this.ExtendedItem.Properties[Layoutable.WidthProperty].SetValue(position.Width);
                                this.ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Left);
                            }
                            else
                            {
                                var rightMargin = GetColumnOffset(column + columnSpan) - position.Right;
                                right = rightMargin;
                                this.ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty].SetValue(HorizontalAlignment.Right);
                            }
                        }
                        break;
                    case HandleOrientation.Bottom:
                        if (_topHandle.IsVisible)
                        {
                            if (_bottomHandle.IsVisible)
                            {
                                bottom = 0;
                                this.ExtendedItem.Properties[Layoutable.HeightProperty].SetValue(position.Height);
                                this.ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty].SetValue(VerticalAlignment.Top);
                            }
                            else
                            {
                                var bottomMargin = GetRowOffset(row + rowSpan) - position.Bottom;
                                bottom = bottomMargin;
                                this.ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty].Reset();
                            }
                        }
                        else
                        {
                            if (_bottomHandle.IsVisible)
                            {
                                bottom = 0;
                                var topMargin = position.Top - GetRowOffset(row);
                                top = topMargin;

                                this.ExtendedItem.Properties[Layoutable.HeightProperty].SetValue(position.Height);
                                this.ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty].SetValue(VerticalAlignment.Top);
                            }
                            else
                            {
                                var bottomMargin = GetRowOffset(row + rowSpan) - position.Bottom;
                                bottom = bottomMargin;
                                this.ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty].SetValue(VerticalAlignment.Bottom);
                            }
                        }
                        break;
                }
            }
            margin = new Thickness(left, top, right, bottom);
            this.ExtendedItem.Properties[Layoutable.MarginProperty].SetValue(margin);
        }

        private double GetColumnOffset(int index)
        {
            if (_grid != null)
            {
                // when the grid has no columns, we still need to return 0 for index=0 and grid.Width for index=1
                if (index == 0)
                    return 0;
                if (index < _grid.ColumnDefinitions.Count)
                    return 0; // _grid.ColumnDefinitions[index].Offset;
                return _grid.Bounds.Width;
            }
            return 0;
        }

        private double GetRowOffset(int index)
        {
            if (_grid != null)
            {
                if (index == 0)
                    return 0;
                if (index < _grid.RowDefinitions.Count)
                    return 0; // _grid.RowDefinitions[index].Offset;
                return _grid.Bounds.Height;
            }
            return 0;
        }

        #endregion

        public void HideHandles()
        {
            if (_handles != null)
            {
                foreach (var handle in _handles)
                {
                    handle.ShouldBeVisible = false;
                    handle.IsVisible = false;
                }
            }
        }

        public void ShowHandles()
        {
            if (_handles != null)
            {
                foreach (var handle in _handles)
                {
                    handle.ShouldBeVisible = true;
                    handle.IsVisible = true;
                    handle.DecideVisiblity(handle.HandleLength);
                }
            }
        }
    }
}
