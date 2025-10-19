using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace gip.core.layoutengine.avui.ganttchart
{
    /// <summary>
    /// The presenter for timeline items.
    /// </summary>
    public class TimelineGanttItemsPresenter : ItemsControl
    {
        #region c'tors

        private static readonly FuncTemplate<Panel> DefaultPanel = new(() => new TimelineCompactPanel());
        static TimelineGanttItemsPresenter()
        {
            ItemsPanelProperty.OverrideMetadata(typeof(TimelineGanttItemsPresenter), new StyledPropertyMetadata<ITemplate<Panel>>(DefaultPanel));

            ItemContainerStyleSelectorProperty.AddOwner(typeof(TimelineGanttItemsPresenter),
                new FrameworkPropertyMetadata(null, CoerceItemContainerStyleSelector));
        }

        #endregion

        public void DeInitControl()
        {
        }

        private static object CoerceItemContainerStyleSelector(DependencyObject d, object baseValue)
        {
            object value = baseValue ?? new StyleSelectorByItemType();
            return value;
        }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new TimelineGanttItem();
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<TimelineGanttItem>(item, out recycleKey);
        }


        internal new TimelineGanttItem ContainerFromItem(object item)
        {
            return base.ContainerFromItem(item) as TimelineGanttItem;
        }
    }

}
