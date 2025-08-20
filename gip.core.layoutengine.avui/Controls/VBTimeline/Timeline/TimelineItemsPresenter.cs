using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using gip.core.layoutengine.avui;

namespace gip.core.layoutengine.avui.timeline
{
    /// <summary>
    /// The presenter for timeline items.
    /// </summary>
    public class TimelineItemsPresenter : ItemsControl
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TimelineItemsPresenterStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTimeline/Themes/TimelineItemsPresenterStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TimelineItemsPresenterStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTimeline/Themes/TimelineItemsPresenterStyleAero.xaml" },
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

        static TimelineItemsPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineItemsPresenter),
                new FrameworkPropertyMetadata(typeof(TimelineItemsPresenter)));

            ItemsPanelTemplate defaultPanel =
                new ItemsPanelTemplate(
                    new FrameworkElementFactory(typeof(TimelineItemPanel))
                    );

            ItemsPanelProperty.OverrideMetadata(typeof(TimelineItemsPresenter),
                new FrameworkPropertyMetadata(defaultPanel));

            ItemContainerStyleSelectorProperty.AddOwner(typeof(TimelineItemsPresenter),
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
            return (item is TimelineItem);
        }

        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            TimelineItem timelineItem = new TimelineItem();
            return timelineItem;
        }

        internal TimelineItem ContainerFromItem(object item)
        {
            return base.ItemContainerGenerator.ContainerFromItem(item)
                as TimelineItem;
        }
    }
}
