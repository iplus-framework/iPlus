using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using gip.core.layoutengine.avui.timeline;

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
        }

        #endregion

        /// <summary>
        /// Gets or sets the theme selector for item containers.
        /// This replaces WPF's ItemContainerStyleSelector.
        /// </summary>
        public static readonly StyledProperty<IItemThemeSelector> ItemThemeSelectorProperty =
            AvaloniaProperty.Register<TimelineGanttItemsPresenter, IItemThemeSelector>(nameof(ItemThemeSelector), new ItemTypeThemeSelector());

        public IItemThemeSelector ItemThemeSelector
        {
            get => GetValue(ItemThemeSelectorProperty);
            set => SetValue(ItemThemeSelectorProperty, value);
        }

        public void DeInitControl()
        {
        }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new TimelineGanttItem();
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<TimelineGanttItem>(item, out recycleKey);
        }

        protected override void PrepareContainerForItemOverride(Control container, object item, int index)
        {
            base.PrepareContainerForItemOverride(container, item, index);
            
            // Apply theme based on item type if selector is available
            var themeSelector = ItemThemeSelector;
            if (themeSelector != null)
            {
                var theme = themeSelector.SelectTheme(item, container);
                if (theme != null)
                {
                    container.Theme = theme;
                }
            }
        }

        internal new TimelineGanttItem ContainerFromItem(object item)
        {
            return base.ContainerFromItem(item) as TimelineGanttItem;
        }
    }
}
