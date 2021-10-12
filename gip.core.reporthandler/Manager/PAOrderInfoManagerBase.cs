using gip.core.autocomponent;
using gip.core.datamodel;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAOrderInfoManagerBase'}de{'PAOrderInfoManagerBase'}", Global.ACKinds.TPARole, Global.ACStorableTypes.NotStorable, false, false)]

    public abstract class PAOrderInfoManagerBase : PAClassAlarmingBase
    {

        #region ctor's
        public PAOrderInfoManagerBase(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
           : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        #endregion

        #region Methods

        public abstract bool IsResponsibleFor(PAOrderInfo orderInfo);

        public abstract PAOrderInfoDestination GetOrderInfoDestination(PAOrderInfo pAOrderInfo);
        public abstract PrinterInfo GetPrinterInfo(PAOrderInfo pAOrderInfo);

        #endregion
    }
}
