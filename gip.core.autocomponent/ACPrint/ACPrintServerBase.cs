using gip.core.datamodel;
using System;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPrintServerBase'}de{'ACPrintServerBase'}", Global.ACKinds.TACApplicationManager, Global.ACStorableTypes.Required, false, "", false)]
    public class  ACPrintServerBase : PAClassAlarmingBase
    {

        #region c´tors

        public ACPrintServerBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        protected static IACEntityObjectContext _CommonManagerContext;
        /// <summary>
        /// Returns a seperate and shared Database-Context "StaticACComponentManager".
        /// Because Businessobjects also inherit from this class all BSO's get this shared database context.
        /// If some custom BSO's needs its own context, then they have to override this property.
        /// Application-Managers that also inherit this class should override this property an use their own context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_CommonManagerContext == null)
                    _CommonManagerContext = ACObjectContextManager.GetOrCreateContext<Database>("StaticACComponentManager");
                return _CommonManagerContext;
            }
        }
        #endregion

        #region Test

        [ACMethodInfo("Print", "en{'Print on server'}de{'Auf Server drucken'}", 9999, true, Global.ACKinds.MSMethodPrePost)]
        public virtual void Print(ACClassDesign acClassDesign, ReportData data, int copies)
        {
            
        }

        #endregion
    }
}
