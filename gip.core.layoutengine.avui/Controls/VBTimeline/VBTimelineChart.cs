using Avalonia.Controls.Primitives;
using gip.core.datamodel;
using gip.core.layoutengine.avui.timeline;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    public class VBTimelineChart : VBTimelineChartBase
    {
        #region c'tors

        /// <summary>
        /// Creates a new instance of VBGanttChart.
        /// </summary>
        public VBTimelineChart()
        {
            Items = new ObservableCollection<IACTimeLog>();
        }
        #endregion

        #region Loaded-Event


        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _ItemsPresenter = e.NameScope.Find("PART_ItemsPresenter") as TimelineItemsPresenter;
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
            (_ItemsPresenter as TimelineItemsPresenter)?.DeInitControl();
        }

        #endregion
    }
}
