using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using gip.core.layoutengine.avui;

namespace gip.core.layoutengine.avui.timeline
{
    /// <summary>
    /// The presenter for timeline items.
    /// </summary>
    public class TimelineItemsPresenter : ItemsControl
    {
        #region c'tors

        private static readonly FuncTemplate<Panel> DefaultPanel = new(() => new TimelineItemPanel());

        static TimelineItemsPresenter()
        {
            ItemsPanelProperty.OverrideMetadata(typeof(TimelineItemsPresenter), new StyledPropertyMetadata<ITemplate<Panel>>(DefaultPanel));

            ItemContainerStyleSelectorProperty.AddOwner(typeof(TimelineItemsPresenter),
                new FrameworkPropertyMetadata(null, CoerceItemContainerStyleSelector));
        }
        #endregion

        public void DeInitControl()
        {
        }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new TimelineItem();
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<TimelineItem>(item, out recycleKey);
        }

        private static object CoerceItemContainerStyleSelector(DependencyObject d, object baseValue)
        {
            object value = baseValue ?? new StyleSelectorByItemType();
            return value;
        }
    }
}
