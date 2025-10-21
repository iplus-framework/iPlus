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
        }
        #endregion

        /// <summary>
        /// Gets or sets the theme selector for item containers.
        /// This replaces WPF's ItemContainerStyleSelector.
        /// </summary>
        public static readonly StyledProperty<IItemThemeSelector> ItemThemeSelectorProperty =
            AvaloniaProperty.Register<TimelineItemsPresenter, IItemThemeSelector>(nameof(ItemThemeSelector), new ItemTypeThemeSelector());

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
            return new TimelineItem();
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<TimelineItem>(item, out recycleKey);
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
    }
}
