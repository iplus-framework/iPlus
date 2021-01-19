using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using gip.core.layoutengine;
using gip.core.layoutengine.timeline;

namespace gip.core.layoutengine.ganttchart
{
    /// <summary>
    /// The presenter for timeline items.
    /// </summary>
    public class TimelineGanttItemsPresenter : ItemsControl
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TimelineItemsPresenterStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBGanttChart/Themes/TimelineGanttItemsPresenterStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TimelineItemsPresenterStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBGanttChart/Themes/TimelineGanttItemsPresenterStyleAero.xaml" },
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

        static TimelineGanttItemsPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineGanttItemsPresenter),
                new FrameworkPropertyMetadata(typeof(TimelineGanttItemsPresenter)));

            ItemsPanelTemplate defaultPanel =
                new ItemsPanelTemplate(
                    new FrameworkElementFactory(typeof(TimelineCompactPanel))
                    );

            ItemsPanelProperty.OverrideMetadata(typeof(TimelineGanttItemsPresenter),
                new FrameworkPropertyMetadata(defaultPanel));

            ItemContainerStyleSelectorProperty.AddOwner(typeof(TimelineGanttItemsPresenter),
                new FrameworkPropertyMetadata(null, CoerceItemContainerStyleSelector));
        }

        bool _themeApplied = false;
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

        #endregion

        public void DeInitControl()
        {
        }

        private static object CoerceItemContainerStyleSelector(DependencyObject d, object baseValue)
        {
            object value = baseValue ?? new StyleSelectorByItemType();
            return value;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is TimelineGanttItem);
        }

        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            TimelineGanttItem timelineItem = new TimelineGanttItem();
            return timelineItem;
        }

        internal TimelineGanttItem ContainerFromItem(object item)
        {
            return base.ItemContainerGenerator.ContainerFromItem(item)
                as TimelineGanttItem;
        }
    }
}
