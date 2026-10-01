using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.VisualTree;
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

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            // The ItemsPanel set on VBTimelineChart (TimelineItemPanel via the
            // chart's ItemsPanelTemplate) does not propagate through this nested
            // ItemsControl - the inner ItemsPresenter falls back to the default
            // StackPanel, which cannot position timeline bars. Enforce the panel.
            //
            // IMPORTANT: use the CHART's ItemsPanel (which carries the view's
            // ItemsPanelTemplate with the bound RowHeight/RowVerticalMargin), NOT a
            // freshly constructed default TimelineItemPanel - the latter discards
            // the configured row dimensions and the rows fall back to the
            // hardcoded defaults (18/5).
            var chart = this.GetVisualAncestors().OfType<VBTimelineChartBase>().FirstOrDefault();
            var innerPresenter = this.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ItemsPresenter>().FirstOrDefault();
            if (innerPresenter != null && !(innerPresenter.Panel is TimelineItemPanel))
            {
                if (chart?.ItemsPanel != null)
                {
                    innerPresenter.ItemsPanel = chart.ItemsPanel;
                    ItemsPanel = chart.ItemsPanel;
                }
                else
                {
                    innerPresenter.ItemsPanel = new FuncTemplate<Panel>(() => new TimelineItemPanel());
                }
                innerPresenter.InvalidateMeasure();
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var result = base.MeasureOverride(availableSize);
            var innerPresenter = this.GetVisualDescendants().OfType<Avalonia.Controls.Presenters.ItemsPresenter>().FirstOrDefault();
            return result;
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
