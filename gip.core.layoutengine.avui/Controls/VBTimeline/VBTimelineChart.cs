using gip.core.datamodel;
using gip.core.layoutengine.avui.timeline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace gip.core.layoutengine.avui
{
    public class VBTimelineChart : VBTimelineChartBase
    {
        #region c'tors

        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> {
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip,
                                         styleName = "GanttChartStyleGip",
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTimeline/Themes/TimelineChartStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero,
                                         styleName = "GanttChartStyleAero",
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBGanttChart/Themes/TimelineChartStyleAero.xaml" },
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

        bool _themeApplied = false;

        static VBTimelineChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTimelineChart), new FrameworkPropertyMetadata(typeof(VBTimelineChart)));
        }

        /// <summary>
        /// Creates a new instance of VBGanttChart.
        /// </summary>
        public VBTimelineChart()
        {
            Items = new ObservableCollection<IACTimeLog>();
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
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        #endregion

        #region Loaded-Event

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);

            itemsPresenter = Template.FindName("PART_ItemsPresenter", this) as TimelineItemsPresenter;
        }

        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
        public override void InitVBControl()
        {
            base.InitVBControl();
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public override void DeInitVBControl(IACComponent bso)
        {
            base.DeInitVBControl(bso);
            (itemsPresenter as TimelineItemsPresenter)?.DeInitControl();
        }

        #endregion
    }
}
