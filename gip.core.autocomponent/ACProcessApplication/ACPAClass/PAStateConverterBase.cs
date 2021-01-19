using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.ComponentModel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Baseclass for converting State and Types between Standard-Model-Components and DataAccess-/Vendor Model
    /// </summary>
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'PAStateConverterBase'}de{'PAStateConverterBase'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, false, true)]
    public abstract class PAStateConverterBase : PAClassAlarmingBase
    {
        #region c'tors
        public PAStateConverterBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            if (ParentACComponent != null)
            {
                IACPropertyNetServer[] query = null;

                using (ACMonitor.Lock(ParentACComponent.LockMemberList_20020))
                {
                    query = ParentACComponent.ACMemberList.Where(c => c is IACPropertyNetServer).Select(c => c as IACPropertyNetServer).ToArray();
                }
                if (query != null && query.Any())
                {
                    foreach (IACPropertyNetServer targetProperty in query)
                    {
                        if (OnParentServerPropertyFound(targetProperty))
                            targetProperty.ValueUpdatedOnReceival += ModelProperty_ValueUpdatedOnReceival;
                    }
                }
            }
            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (ParentACComponent != null)
            {
                IACPropertyNetServer[] query = null;

                using (ACMonitor.Lock(ParentACComponent.LockMemberList_20020))
                {
                    query = ParentACComponent.ACMemberList.Where(c => c is IACPropertyNetServer).Select(c => c as IACPropertyNetServer).ToArray();
                }
                if (query != null && query.Any())
                {
                    foreach (IACPropertyNetServer targetProperty in query)
                    {
                        targetProperty.ValueUpdatedOnReceival -= ModelProperty_ValueUpdatedOnReceival;
                    }
                }
            }
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region abstract methods
        protected abstract bool OnParentServerPropertyFound(IACPropertyNetServer parentProperty);
        protected abstract void ModelProperty_ValueUpdatedOnReceival(object sender, ACPropertyChangedEventArgs e, ACPropertyChangedPhase phase);

        protected PropBindingBindingResult BindProperty(string subscriptionName, IACPropertyNetTarget targetProp, string acIdentifierProp, 
            out IACPropertyNetTarget newTarget, bool bindInDBIfConverterNeeded = true)
        {
            string message;
            PropBindingBindingResult result = BindProperty(Session as ACComponent, subscriptionName, targetProp, acIdentifierProp, out newTarget, out message, bindInDBIfConverterNeeded);
            if (!String.IsNullOrEmpty(message))
            {
                if (   result.HasFlag(PropBindingBindingResult.NotPossible)
                    || result.HasFlag(PropBindingBindingResult.NotCompatibleTypes)
                    || result.HasFlag(PropBindingBindingResult.AlreadyBound))
                    Messages.LogError(this.GetACUrl(), "BindProperty()", message);
                else
                    Messages.LogDebug(this.GetACUrl(), "BindProperty()", message);
            }
            return result;
        }

        public static PropBindingBindingResult BindProperty(IACComponent session, string subscriptionName, IACPropertyNetTarget targetProp, string acIdentifierProp, 
            out IACPropertyNetTarget newTarget, out string message, 
            bool bindInDBIfConverterNeeded = true)
        {
            newTarget = null;
            if (String.IsNullOrEmpty(subscriptionName) || String.IsNullOrEmpty(acIdentifierProp) || targetProp == null)
            {
                message = "Wrong parameters passed";
                return PropBindingBindingResult.NotPossible;
            }
            ACComponent subscription = session.ACUrlCommand(subscriptionName) as ACComponent;
            if (subscription != null)
            {
                IACPropertyNetSource sourceProp = subscription.GetPropertyNet(acIdentifierProp) as IACPropertyNetSource;
                if (sourceProp != null)
                    return targetProp.BindPropertyToSource(sourceProp, out newTarget, out message, bindInDBIfConverterNeeded);
                else
                    message = String.Format("Property {0} doesn't exist in subscription {1} at {2}.", acIdentifierProp, subscriptionName, session.GetACUrl());
            }
            else
                message = String.Format("Subscription {0} not found at {1}.", subscriptionName, session.GetACUrl());
            return PropBindingBindingResult.NotPossible;
        }

        #endregion
    }
}
