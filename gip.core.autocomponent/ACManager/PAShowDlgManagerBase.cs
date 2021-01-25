using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAShowDlgManagerBase'}de{'PAShowDlgManagerBase'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, false, false)]
    public abstract class PAShowDlgManagerBase : PARole
    {
        #region c´tors
        public  PAShowDlgManagerBase(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        public const string C_DefaultServiceACIdentifier = "DlgManager";
        #endregion

        #region static Methods
        public static PAShowDlgManagerBase GetServiceInstance(ACComponent requester)
        {
            return GetServiceInstance<PAShowDlgManagerBase>(requester, C_DefaultServiceACIdentifier, CreationBehaviour.OnlyLocal);
        }

        public static ACRef<PAShowDlgManagerBase> ACRefToServiceInstance(ACComponent requester)
        {
            PAShowDlgManagerBase serviceInstance = GetServiceInstance(requester);
            if (serviceInstance != null)
                return new ACRef<PAShowDlgManagerBase>(serviceInstance, requester);
            return null;
        }

        public static PAOrderInfo QueryOrderInfo(IACComponent caller)
        {
            if (caller == null)
                return null;
            PAOrderInfo orderInfo = null;
            string[] accessedFromVBGroupACUrl = caller.ACUrlCommand("!SemaphoreAccessedFrom") as string[];
            if (accessedFromVBGroupACUrl != null && accessedFromVBGroupACUrl.Any())
            {
                string firstModule = accessedFromVBGroupACUrl[0];
                if (!String.IsNullOrEmpty(firstModule))
                    orderInfo = caller.ACUrlCommand(accessedFromVBGroupACUrl[0] + "!GetPAOrderInfo") as PAOrderInfo;
            }
            return orderInfo;
        }

        public static bool HasOrderInfo(IACComponent caller, out IACContainerTNet<String> orderInfoProp)
        {
            orderInfoProp = null;
            if (caller == null)
                return false;
            orderInfoProp = caller.GetProperty("OrderInfo") as IACContainerTNet<String>;
            if (orderInfoProp == null)
                return false;
            if (String.IsNullOrEmpty(orderInfoProp.ValueT))
                return false;
            return true;
        }
        #endregion

        #region Public Methods

        public abstract void ShowDialogOrder(IACComponent caller, PAOrderInfo orderInfo = null);

        public abstract bool IsEnabledShowDialogOrder(IACComponent caller);

        public abstract void ShowProgramLogViewer(IACComponent caller, ACValueList param);

        public abstract string BuildAndSetOrderInfo(PAProcessModule pm);

        public abstract string BuildOrderInfo(PWBase pw);

        #endregion

    }
}
