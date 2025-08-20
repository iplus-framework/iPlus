using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using gip.core.datamodel;
using gip.core.layoutengine.avui.timeline;

namespace gip.core.layoutengine.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTimelineView'}de{'VBTimelineView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.NotStorable, true, false)]
    public class VBTimelineView : VBTimelineViewBase
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo>
        {
            new CustomControlStyleInfo {wpfTheme = eWpfTheme.Gip, styleName = "TimelineViewStyleGip",
                                        styleUri = "/gip.core.layoutengine.avui; Component/Controls/VBGanttChart/Themes/TimelineViewStyleGip.xaml",
                                        hasImplicitStyles = false},
            new CustomControlStyleInfo {wpfTheme = eWpfTheme.Aero, styleName = "TimelineViewStyleAero",
                                        styleUri = "/gip.core.layoutengine.avui; Component/Controls/VBGanttChart/Themes/TimelineViewStyleAero.xaml",
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

        static VBTimelineView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTimelineView), new FrameworkPropertyMetadata(typeof(VBTimelineViewBase)));
        }

        /// <summary>
        /// Creates a new instance of VBGanttChartView.
        /// </summary>
        public VBTimelineView() : base()
        {
            TreeListViewColumns = new System.Windows.Controls.GridViewColumnCollection();
            ItemToolTip = new List<FrameworkElement>();
        }
        #endregion

        public override void InitVBControl()
        {
            base.InitVBControl();
        }
    }
}
