using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace gip.core.layoutengine.avui
{
    public class FlipViewItem : ListBoxItem
    {
        private VBFlipView _flipView;

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);

            _flipView = e.Parent as VBFlipView;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            AddHandler(RequestBringIntoViewEvent, BroughtIntoView);
        }

        private void BroughtIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            if (_flipView is { } parent)
            {
                Height = parent.GetDesiredItemHeight();
                Width = parent.GetDesiredItemWidth();
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            RemoveHandler(RequestBringIntoViewEvent, BroughtIntoView);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);

            _flipView = null;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_flipView is { } parent)
            {
                Height = parent.GetDesiredItemHeight();
                Width = parent.GetDesiredItemWidth();
            }
            return base.MeasureOverride(availableSize);
        }
    }
}
