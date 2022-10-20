using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.layoutengine.ganttchart;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using gip.core.layoutengine.timeline;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents the control which connects <see cref="VBTreeListView"/> control with <see cref="VBGanttChart"/> control.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt das Control dar, das <see cref="VBTreeListView"/> Control mit dem <see cref="VBGanttChart"/> Control verbindet.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBGanttChartView'}de{'VBGanttChartView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBGanttChartView : VBTimelineViewBase
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo>
        {
            new CustomControlStyleInfo {wpfTheme = eWpfTheme.Gip, styleName = "GanttChartViewStyleGip",
                                        styleUri = "/gip.core.layoutengine; Component/Controls/VBGanttChart/Themes/GanttChartViewStyleGip.xaml",
                                        hasImplicitStyles = false},
            new CustomControlStyleInfo {wpfTheme = eWpfTheme.Aero, styleName = "GanttChartViewStyleAero",
                                        styleUri = "/gip.core.layoutengine; Component/Controls/VBGanttChart/Themes/GanttChartViewStyleAero.xaml",
                                        hasImplicitStyles = false},
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

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBGanttChartView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBGanttChartView), new FrameworkPropertyMetadata(typeof(VBTimelineViewBase)));
        }

        /// <summary>
        /// Creates a new instance of VBGanttChartView.
        /// </summary>
        public VBGanttChartView() : base()
        {
            TreeListViewColumns = new GridViewColumnCollection();
            ItemToolTip = new List<FrameworkElement>();
        }
        #endregion

        #region Loaded-Event

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
        public override  void InitVBControl()
        {
            base.InitVBControl();
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        #endregion
    }
}
