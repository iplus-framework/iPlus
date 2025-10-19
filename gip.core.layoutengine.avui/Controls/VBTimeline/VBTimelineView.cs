using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using gip.core.datamodel;
using gip.core.layoutengine.avui.timeline;

namespace gip.core.layoutengine.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTimelineView'}de{'VBTimelineView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.NotStorable, true, false)]
    public class VBTimelineView : VBTimelineViewBase
    {
        #region c'tors
        /// <summary>
        /// Creates a new instance of VBGanttChartView.
        /// </summary>
        public VBTimelineView() : base()
        {
            TreeListViewColumns = new GridViewColumnCollection();
            ItemToolTip = new List<Control>();
        }
        #endregion

        public override void InitVBControl()
        {
            base.InitVBControl();
        }
    }
}
