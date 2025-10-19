using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.layoutengine.avui.ganttchart;
using System.ComponentModel;
using gip.core.layoutengine.avui.timeline;
using Avalonia.Controls;

namespace gip.core.layoutengine.avui
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


        /// <summary>
        /// Creates a new instance of VBGanttChartView.
        /// </summary>
        public VBGanttChartView() : base()
        {
            TreeListViewColumns = new GridViewColumnCollection();
            ItemToolTip = new List<Control>();
        }
        #endregion
    }
}
