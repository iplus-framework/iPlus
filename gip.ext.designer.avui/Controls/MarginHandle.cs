// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Data.Converters;
using Avalonia;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Controls.Primitives;

namespace gip.ext.designer.avui.Controls
{
    /// <summary>
    /// Adorner that displays the margin of a control in a Grid.
    /// </summary>
    public class MarginHandle : TemplatedControl
    {
        /// <summary>
        /// Places the Handle with a certain offset so the Handle does not interfere with selection outline.
        /// </summary>
        public static double HandleLengthOffset;

        static MarginHandle()
        {
            HandleLengthOffset = 2;
            HandleLengthProperty.Changed.AddClassHandler<MarginHandle>((x, e) => x.OnHandleLengthChanged(e));
        }

        /// <summary>
        /// Dependency property for <see cref="HandleLength"/>.
        /// </summary>
        public static readonly StyledProperty<double> HandleLengthProperty = AvaloniaProperty.Register<MarginHandle, double>(nameof(HandleLength), 0.0, false, Avalonia.Data.BindingMode.TwoWay);


        /// <summary>
        /// Gets/Sets the length of Margin Handle.
        /// </summary>
        public double HandleLength
        {
            get { return (double)GetValue(HandleLengthProperty); }
            set { SetValue(HandleLengthProperty, value); }
        }

        readonly Grid grid;
        readonly DesignItem adornedControlItem;
        readonly AdornerPanel adornerPanel;
        readonly HandleOrientation orientation;
        readonly Control adornedControl;

        /// <summary> This grid contains the handle line and the endarrow.</summary>
        Grid lineArrow;

        /// <summary>
        /// Gets the Stub for this handle
        /// </summary>
        public MarginStub Stub { get; set; }

        /// <summary>
        /// Gets/Sets the angle by which handle rotates.
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// Gets/Sets the angle by which the Margin display has to be rotated
        /// </summary>
        public virtual double TextTransform
        {
            get
            {
                if ((double)orientation == 90 || (double)orientation == 180)
                    return 180;
                if ((double)orientation == 270)
                    return 0;
                return (double)orientation;
            }
            set { }
        }

        /// <summary>
        /// Decides the visiblity of handle/stub when <see cref="HandleLength"/> changes
        /// </summary>
        /// <param name="e"></param>
        private void OnHandleLengthChanged(AvaloniaPropertyChangedEventArgs e)
        {
            DecideVisiblity((double)e.NewValue);
        }       /// <summary>
                /// Decides whether to permanently display the handle or not.
                /// </summary>
        public bool ShouldBeVisible { get; set; }

        /// <summary>
        /// Decides whether stub has to be only displayed.
        /// </summary>
        public bool DisplayOnlyStub { get; set; }

        /// <summary>
        /// Gets the orientation of the handle.
        /// </summary>
        public HandleOrientation Orientation
        {
            get { return orientation; }
        }

        protected MarginHandle()
        {
        }

        public MarginHandle(DesignItem adornedControlItem, AdornerPanel adornerPanel, HandleOrientation orientation)
        {
            Debug.Assert(adornedControlItem != null);
            this.adornedControlItem = adornedControlItem;
            this.adornerPanel = adornerPanel;
            this.orientation = orientation;
            Angle = (double)orientation;
            grid = (Grid)adornedControlItem.Parent.Component;
            adornedControl = (Control)adornedControlItem.Component;
            Stub = new MarginStub(this);
            ShouldBeVisible = true;
            BindAndPlaceHandle();

            adornedControlItem.PropertyChanged += OnPropertyChanged;
            OnPropertyChanged(this.adornedControlItem, new PropertyChangedEventArgs("HorizontalAlignment"));
            OnPropertyChanged(this.adornedControlItem, new PropertyChangedEventArgs("VerticalAlignment"));
        }

        /// <summary>
        /// Binds the <see cref="HandleLength"/> to the margin and place the handles.
        /// </summary>
        void BindAndPlaceHandle()
        {
            if (!adornerPanel.Children.Contains(this))
                adornerPanel.Children.Add(this);
            if (!adornerPanel.Children.Contains(Stub))
                adornerPanel.Children.Add(Stub);
            RelativePlacement placement = new RelativePlacement();
            Binding binding = new Binding();
            binding.Source = adornedControl;
            switch (orientation)
            {
                case HandleOrientation.Left:
                    binding.Path = "Margin.Left";
                    placement = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Center);
                    placement.XOffset = -HandleLengthOffset;
                    break;
                case HandleOrientation.Top:
                    binding.Path = "Margin.Top";
                    placement = new RelativePlacement(HorizontalAlignment.Center, VerticalAlignment.Top);
                    placement.YOffset = -HandleLengthOffset;
                    break;
                case HandleOrientation.Right:
                    binding.Path = "Margin.Right";
                    placement = new RelativePlacement(HorizontalAlignment.Right, VerticalAlignment.Center);
                    placement.XOffset = HandleLengthOffset;
                    break;
                case HandleOrientation.Bottom:
                    binding.Path = "Margin.Bottom";
                    placement = new RelativePlacement(HorizontalAlignment.Center, VerticalAlignment.Bottom);
                    placement.YOffset = HandleLengthOffset;
                    break;
            }

            binding.Mode = BindingMode.TwoWay;
            this.Bind(HandleLengthProperty, binding);

            AdornerPanel.SetPlacement(this, placement);
            AdornerPanel.SetPlacement(Stub, placement);

            DecideVisiblity(this.HandleLength);
        }

        /// <summary>
        /// Decides the visibllity of Handle or stub,whichever is set and hides the line-endarrow if the control is near the Grid or goes out of it.
        /// </summary>		
        public void DecideVisiblity(double handleLength)
        {
            if (ShouldBeVisible)
            {
                if (!DisplayOnlyStub)
                {
                    this.IsVisible = handleLength != 0.0;
                    if (this.lineArrow != null)
                    {
                        lineArrow.IsVisible = handleLength >= 25;
                    }
                    Stub.IsVisible = !this.IsVisible;
                }
                else
                {
                    this.IsVisible = false;
                    Stub.IsVisible = true;
                }
            }
            else
            {
                this.IsVisible = false;
                Stub.IsVisible = false;
            }
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HorizontalAlignment" && (orientation == HandleOrientation.Left || orientation == HandleOrientation.Right))
            {
                var ha = (HorizontalAlignment)adornedControlItem.Properties[Control.HorizontalAlignmentProperty].ValueOnInstance;
                if (ha == HorizontalAlignment.Stretch)
                {
                    DisplayOnlyStub = false;
                }
                else if (ha == HorizontalAlignment.Center)
                {
                    DisplayOnlyStub = true;
                }
                else
                    DisplayOnlyStub = ha.ToString() != orientation.ToString();
            }

            if (e.PropertyName == "VerticalAlignment" && (orientation == HandleOrientation.Top || orientation == HandleOrientation.Bottom))
            {
                var va = (VerticalAlignment)adornedControlItem.Properties[Control.VerticalAlignmentProperty].ValueOnInstance;

                if (va == VerticalAlignment.Stretch)
                {
                    DisplayOnlyStub = false;
                }
                else if (va == VerticalAlignment.Center)
                {
                    DisplayOnlyStub = true;
                }
                else
                    DisplayOnlyStub = va.ToString() != orientation.ToString();
            }
            DecideVisiblity(this.HandleLength);
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            this.Cursor = new Cursor(StandardCursorType.Hand);
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            this.Cursor = new Cursor(StandardCursorType.Arrow);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            lineArrow = e.NameScope.Find<Grid>("lineArrow");
            Debug.Assert(lineArrow != null);
        }

    }

    /// <summary>
    /// Display a stub indicating that the margin is not set.
    /// </summary>
    public class MarginStub : TemplatedControl
    {
        MarginHandle marginHandle;

        /// <summary>
        /// Gets the margin handle using this stub.
        /// </summary>
        public MarginHandle Handle
        {
            get { return marginHandle; }
        }

        public MarginStub(MarginHandle marginHandle)
        {
            this.marginHandle = marginHandle;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                marginHandle.DecideVisiblity(marginHandle.HandleLength);
            }
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            this.Cursor = new Cursor(StandardCursorType.Hand);
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            this.Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }

    /// <summary>
    /// Specifies the Handle orientation
    /// </summary>
    public enum HandleOrientation
    {
        /*  Rotation of the handle is done with respect to right orientation and in clockwise direction*/

        /// <summary>
        /// Indicates that the margin handle is left-oriented and rotated 180 degrees with respect to <see cref="Right"/>.
        /// </summary>
        Left = 180,
        /// <summary>
        /// Indicates that the margin handle is top-oriented and rotated 270 degrees with respect to <see cref="Right"/>.
        /// </summary>
        Top = 270,
        /// <summary>
        /// Indicates that the margin handle is right.
        /// </summary>
        Right = 0,
        /// <summary>
        /// Indicates that the margin handle is left-oriented and rotated 180 degrees with respect to <see cref="Right"/>.
        /// </summary>
        Bottom = 90
    }
}

namespace gip.ext.designer.avui.Controls.Converters
{
    /// <summary>
    /// Offset the Handle Length with MarginHandle.HandleLengthOffset
    /// </summary>
    public class HandleLengthWithOffset : IValueConverter
    {
        public static HandleLengthWithOffset Instance = new HandleLengthWithOffset();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                return Math.Max((double)value - MarginHandle.HandleLengthOffset, 0);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
