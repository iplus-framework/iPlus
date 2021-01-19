using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using gip.core.layoutengine;
using gip.core.layoutengine.timeline;

namespace gip.core.layoutengine.ganttchart
{
    public class ConnectionsPresenter : ItemsControl
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ConnectionsPresenterStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBGanttChart/Themes/ConnectionsPresenterStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ConnectionsPresenterStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBGanttChart/Themes/ConnectionsPresenterStyleAero.xaml" },
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

        static ConnectionsPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ConnectionsPresenter),
                new FrameworkPropertyMetadata(typeof(ConnectionsPresenter)));

            ItemContainerStyleSelectorProperty.AddOwner(typeof(ConnectionsPresenter),
                new FrameworkPropertyMetadata(null, CoerceItemContainerStyleSelector));

        }

        private static object CoerceItemContainerStyleSelector(DependencyObject d, object baseValue)
        {
            object value = baseValue ?? new StyleSelectorByItemType();
            return value;
        }

        bool _themeApplied = false;
        public ConnectionsPresenter()
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
        #endregion

        #region Properties
        public VBGanttChart Timeline { get; set; }
        #endregion

        #region Attached Properties

        #region From
        // Using a DependencyProperty as the backing store for FromItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FromItemProperty =
                DependencyProperty.RegisterAttached("FromItem", typeof(object), typeof(ConnectionsPresenter), new FrameworkPropertyMetadata(null));
        public static object GetFromItem(DependencyObject obj)
        {
            return (object)obj.GetValue(FromItemProperty);
        }

        public static void SetFromItem(DependencyObject obj, object value)
        {
            obj.SetValue(FromItemProperty, value);
        }
        #endregion

        #region To
        // Using a DependencyProperty as the backing store for ToItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToItemProperty =
                DependencyProperty.RegisterAttached("ToItem", typeof(object), typeof(ConnectionsPresenter), new FrameworkPropertyMetadata(null));

        public static object GetToItem(DependencyObject obj)
        {
            return (object)obj.GetValue(ToItemProperty);
        }

        public static void SetToItem(DependencyObject obj, object value)
        {
            obj.SetValue(ToItemProperty, value);
        }
        #endregion

        #endregion

        #region Methods
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is Connection;
        }

        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new Connection();
        }

        protected override void PrepareContainerForItemOverride(
            DependencyObject element, object item)
        {

            base.PrepareContainerForItemOverride(element, item);

            if (Timeline != null)
            {
                object fromItem = GetFromItem(element);
                object toItem = GetToItem(element);

                if (fromItem != null && toItem != null)
                {
                    TimelineGanttItem tlFromItem = Timeline.ContainerFromItem(fromItem);
                    TimelineGanttItem tlToItem = Timeline.ContainerFromItem(toItem);

                    if (tlFromItem != null && tlToItem != null)
                    {

                        Connection conn = (Connection)element;
                        conn.SourceItem = tlFromItem;
                        conn.SinkItem = tlToItem;
                    }
                }
            }
        }
        #endregion
    }

}
