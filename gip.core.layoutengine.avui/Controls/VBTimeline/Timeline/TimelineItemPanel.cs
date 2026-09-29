using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui.timeline
{
    public class TimelineItemPanel : TimelinePanel
    {
        public TimelineItemPanel()
        {
            Margin = new Thickness(0, 0, 0, 22);
        }

        private VBTimelineChart _VBTimelineChart
        {
            get
            {
                return VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBTimelineChart)) as VBTimelineChart;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // There can be MULTIPLE TimelineItemPanel instances alive (one per
            // ItemsPresenter recreation). Pushing TickTimeSpan onto a single
            // instance leaves the others stale - the visible panel then keeps the
            // old pixels-per-tick until a resize. Pull the authoritative values
            // from the owning chart instead: whatever panel measures, it always
            // uses the chart's CURRENT scale.
            var chart = _VBTimelineChart;
            if (chart != null)
            {
                if (chart.TickTimeSpan != TickTimeSpan)
                    TickTimeSpan = chart.TickTimeSpan;
                if (chart.MinimumDate != MinimumDate)
                    MinimumDate = chart.MinimumDate;
                if (chart.MaximumDate != MaximumDate)
                    MaximumDate = chart.MaximumDate;
            }

            rowsCount = -1;
            int firstLogged = 0;
            ObserveChildren();
            List<Control> measuredChildren = new List<Control>();
            Dictionary<Control, int> logicalToActualMap = new Dictionary<Control, int>();

            //Children.OfType<Control>().All(c => c.ApplyTemplate());

            var q =
                from c in Children.OfType<Control>()
                let row = GetRowIndex(c)
                orderby row
                select new { Child = c, Row = row };


            int lastItemRowIndex = 0, nextActualRowIndex = 0;

            foreach (var childAndRow in q)
            {
                ((Control)childAndRow.Child).ApplyTemplate();
                ClearActualRowIndex(childAndRow.Child);
                childAndRow.Child.ClearValue(Control.IsVisibleProperty);

                TimelineItem tlChild = (TimelineItem)childAndRow.Child;

                tlChild.IsDisplayAsZero = false; // ShouldDisplayAsZero(tlChild);
                tlChild.ApplyTemplate();

                Rect calcChildSize = CalcChildRect(tlChild, nextActualRowIndex);
                childAndRow.Child.Measure(calcChildSize.Size);

                if (firstLogged < 3)
                {
                    firstLogged++;
                    //System.Diagnostics.Debug.WriteLine($"[TLITEM] Panel: row={childAndRow.Row}, start={TimelinePanel.GetStartDate(tlChild):HH:mm:ss}, end={TimelinePanel.GetEndDate(tlChild):HH:mm:ss}, collapsed={tlChild.IsCollapsed}, rect={calcChildSize}");
                }

                if (tlChild.IsCollapsed)
                    childAndRow.Child.IsVisible = false;
                else
                {
                    if(childAndRow.Row > lastItemRowIndex)
                    {
                        nextActualRowIndex++;
                        lastItemRowIndex = childAndRow.Row;
                    }
                    SetActualRowIndex(childAndRow.Child, nextActualRowIndex);
                }
            }

            TimeSpan totalTimeSpan = TimeSpan.Zero;
            if (MaximumDate.HasValue && MinimumDate.HasValue)
                totalTimeSpan = MaximumDate.Value - MinimumDate.Value;

            double totalWidth = Math.Max(0, totalTimeSpan.Ticks * PixelsPerTick);
            double totalHeight = nextActualRowIndex * RowHeight + nextActualRowIndex * RowVerticalMargin;

            _contentWidth = totalWidth;
            _contentHeight = Math.Max(totalHeight, RowHeight > 0 ? RowHeight : 1);

            //System.Diagnostics.Debug.WriteLine($"[TL] Panel measure: children={Children.Count}, rows={nextActualRowIndex}, min={(MinimumDate?.ToString("HH:mm") ?? "null")}, max={(MaximumDate?.ToString("HH:mm") ?? "null")}, ppt={PixelsPerTick}, totalWidth={totalWidth}, totalHeight={totalHeight}");
            // Never return a degenerate size: a zero/near-zero height collapses the
            // ScrollViewer content to ~0px even when the width is valid.
            if (totalWidth <= 0)
                return new Size();
            return new Size(_contentWidth, _contentHeight);
        }

        private double _contentWidth, _contentHeight;

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (Control child in Children)
            {
                ArrangeChild(child);
            }
            // Report the full content size, not the viewport-constrained finalSize,
            // otherwise the ScrollViewer extent never exceeds the viewport and all
            // bars beyond the first viewport width are unreachable/clipped.
            var result = new Size(Math.Max(finalSize.Width, _contentWidth), Math.Max(finalSize.Height, _contentHeight));
            //System.Diagnostics.Debug.WriteLine($"[TL] Panel arrange: finalSize={finalSize}, result={result}, bounds={Bounds}");

            // Diagnostics: dump visual state of first few arranged children.
            int dumped = 0;
            foreach (Control child in Children)
            {
                if (dumped >= 3) break;
                var tl = child as TimelineItem;
                if (tl == null || tl.IsCollapsed) continue;
                dumped++;
                //System.Diagnostics.Debug.WriteLine($"[TLVIS] item: isVisible={child.IsVisible}, bounds={child.Bounds}, opacity={child.Opacity}, content={tl.Content?.GetType().Name}, contentTemplate={(tl.ContentTemplate != null ? "SET" : "NULL")}, presenter={child.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ContentPresenter>().FirstOrDefault()?.GetType().Name ?? "NONE"}");
                if (dumped == 1)
                {
                    foreach (var desc in child.GetVisualDescendants().Take(8))
                    {
                        //System.Diagnostics.Debug.WriteLine($"[TLVIS]   desc: {desc.GetType().Name}, bounds={desc.Bounds}, visible={(desc as Control)?.IsVisible.ToString() ?? "?"}");
                    }
                }
            }
            return result;
        }

        private bool IsChildHasNoStartAndEndTime(Control child)
        {
            DateTime? childStartDate = GetStartDate(child);
            DateTime? childEndDate = GetEndDate(child);

            bool noTimes = childStartDate == null && childEndDate == null;
            return noTimes;
        }
    }
}
