using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace gip.core.layoutengine.avui
{
    public class VBSelectedThumb : Thumb
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "SelectedThumbStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBCanvas/VBSelectedThumb/Themes/SelectedThumbStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "SelectedThumbStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBCanvas/VBSelectedThumb/Themes/SelectedThumbStyleAero.xaml" },
        };
        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBSelectedThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBSelectedThumb), new FrameworkPropertyMetadata(typeof(VBSelectedThumb)));
        }

        bool _themeApplied = false;
        public VBSelectedThumb()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        //void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        //{
        //    VBVisualWF designerItem = this.DataContext as VBVisualWF;
        //    VBCanvas designer = VisualTreeHelper.GetParent(designerItem) as VBCanvas;

        //    if (designerItem != null && designer != null && designerItem.IsSelected)
        //    {
        //        double minLeft, minTop, minDeltaHorizontal, minDeltaVertical;
        //        double dragDeltaVertical, dragDeltaHorizontal, scale;

        //        IEnumerable<VBVisualWF> selectedDesignerItems = designer.SelectionService.CurrentSelection.OfType<VBVisualWF>();

        //        CalculateDragLimits(selectedDesignerItems, out minLeft, out minTop,
        //                            out minDeltaHorizontal, out minDeltaVertical);

        //        foreach (VBVisualWF item in selectedDesignerItems)
        //        {
        //            if (item != null && item.ParentID == Guid.Empty)
        //            {
        //                switch (base.VerticalAlignment)
        //                {
        //                    case VerticalAlignment.Bottom:
        //                        dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
        //                        scale = (item.ActualHeight - dragDeltaVertical) / item.ActualHeight;
        //                        DragBottom(scale, item, designer.SelectionService);
        //                        break;
        //                    case VerticalAlignment.Top:
        //                        double top = Canvas.GetTop(item);
        //                        dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
        //                        scale = (item.ActualHeight - dragDeltaVertical) / item.ActualHeight;
        //                        DragTop(scale, item, designer.SelectionService);
        //                        break;
        //                    default:
        //                        break;
        //                }

        //                switch (base.HorizontalAlignment)
        //                {
        //                    case HorizontalAlignment.Left:
        //                        double left = Canvas.GetLeft(item);
        //                        dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
        //                        scale = (item.ActualWidth - dragDeltaHorizontal) / item.ActualWidth;
        //                        DragLeft(scale, item, designer.SelectionService);
        //                        break;
        //                    case HorizontalAlignment.Right:
        //                        dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
        //                        scale = (item.ActualWidth - dragDeltaHorizontal) / item.ActualWidth;
        //                        DragRight(scale, item, designer.SelectionService);
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }
        //        }
        //        e.Handled = true;
        //    }
        //}

        //#region Helper methods

        //private void DragLeft(double scale, VBVisualWF item, SelectionService selectionService)
        //{
        //    IEnumerable<VBVisualWF> groupItems = selectionService.GetGroupMembers(item).Cast<VBVisualWF>();

        //    double groupLeft = Canvas.GetLeft(item) + item.Width;
        //    foreach (VBVisualWF groupItem in groupItems)
        //    {
        //        groupItem.DragLeft(scale, groupLeft);
        //        //double groupItemLeft = Canvas.GetLeft(groupItem);
        //        //double delta = (groupLeft - groupItemLeft) * (scale - 1);
        //        //Canvas.SetLeft(groupItem, groupItemLeft - delta);
        //        //groupItem.Width = groupItem.ActualWidth * scale;
        //    }
        //}

        //private void DragTop(double scale, VBVisualWF item, SelectionService selectionService)
        //{
        //    IEnumerable<VBVisualWF> groupItems = selectionService.GetGroupMembers(item).Cast<VBVisualWF>();
        //    double groupBottom = Canvas.GetTop(item) + item.Height;
        //    foreach (VBVisualWF groupItem in groupItems)
        //    {
        //        groupItem.DragTop(scale, groupBottom);

        //        //double groupItemTop = Canvas.GetTop(groupItem);
        //        //double delta = (groupBottom - groupItemTop) * (scale - 1);
        //        //Canvas.SetTop(groupItem, groupItemTop - delta);
        //        //groupItem.Height = groupItem.ActualHeight * scale;
        //    }
        //}

        //private void DragRight(double scale, VBVisualWF item, SelectionService selectionService)
        //{
        //    IEnumerable<VBVisualWF> groupItems = selectionService.GetGroupMembers(item).Cast<VBVisualWF>();

        //    double groupLeft = Canvas.GetLeft(item);
        //    foreach (VBVisualWF groupItem in groupItems)
        //    {
        //        groupItem.DragRight(scale, groupLeft);
        //        //double groupItemLeft = Canvas.GetLeft(groupItem);
        //        //double delta = (groupItemLeft - groupLeft) * (scale - 1);
        //        //Canvas.SetLeft(groupItem, groupItemLeft + delta);
        //        //groupItem.Width = groupItem.ActualWidth * scale;
        //    }
        //}

        //private void DragBottom(double scale, VBVisualWF item, SelectionService selectionService)
        //{
        //    IEnumerable<VBVisualWF> groupItems = selectionService.GetGroupMembers(item).Cast<VBVisualWF>();
        //    double groupTop = Canvas.GetTop(item);
        //    foreach (VBVisualWF groupItem in groupItems)
        //    {
        //        groupItem.DragBottom(scale, groupTop);

        //        //double groupItemTop = Canvas.GetTop(groupItem);
        //        //double delta = (groupItemTop - groupTop) * (scale - 1);
        //        //Canvas.SetTop(groupItem, groupItemTop + delta);
        //        //groupItem.Height = groupItem.ActualHeight * scale;
        //    }
        //}

        //private void CalculateDragLimits(IEnumerable<VBVisualWF> selectedItems, out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
        //{
        //    minLeft = double.MaxValue;
        //    minTop = double.MaxValue;
        //    minDeltaHorizontal = double.MaxValue;
        //    minDeltaVertical = double.MaxValue;

        //    // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
        //    // calculate min value for each parameter for each item
        //    foreach (VBVisualWF item in selectedItems)
        //    {
        //        double left = Canvas.GetLeft(item);
        //        double top = Canvas.GetTop(item);

        //        minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
        //        minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

        //        minDeltaVertical = Math.Min(minDeltaVertical, item.ActualHeight - item.MinHeight);
        //        minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.ActualWidth - item.MinWidth);
        //    }
        //}

        //#endregion
    }
}
