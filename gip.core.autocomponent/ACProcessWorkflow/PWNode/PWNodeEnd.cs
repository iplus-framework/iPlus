// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// The PWNodeEnd class  is always the last workflow node within a node that implements the IACComponentPWGroup interface (these are the PWProcessFunction and PWGroup classes). PWNodeEnd implements the interface  IPWNodeIn , which specifies the subscription point "PWPointIn" of type  PWPointIn . The program flow within a group must be programmed so that it inevitably ends in "PWPointIn". PWNodeEnd then calls the GroupComplete() method in the associated group, which in turn triggers your initial event point.
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PWBase" />
    /// <seealso cref="gip.core.autocomponent.IPWNodeIn" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWNodeEnd'}de{'PWNodeEnd'}", Global.ACKinds.TPWNodeEnd, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeEnd : PWBase, IPWNodeIn
    {
        public const string PWClassName = "PWNodeEnd";

        #region c´tors
        public PWNodeEnd(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PWPointIn = new PWPointIn(this, Const.PWPointIn, 0);
        }

        public override bool ACPostInit()
        {
            PWPointIn.SubscribeACClassWFEdgeEvents(PWPointInCallback);
            return base.ACPostInit();
        }

        public override bool ACPreDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACPreDeInit(deleteACClassTask);
            if (!result)
                return result;
            if (deleteACClassTask)
                PWPointIn.UnSubscribeAllEvents();
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Points
        PWPointIn _PWPointIn;
        [ACPropertyEventPointSubscr(9999, true, "PWPointInCallback")]
        public PWPointIn PWPointIn
        {
            get
            {
                return _PWPointIn;
            }
        }
        #endregion

        #region Methods

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "SMStarting":
                    SMStarting();
                    return true;
                case "PWPointInCallback":
                    PWPointInCallback(acParameter[0] as IACPointNetBase, acParameter[1] as ACEventArgs, acParameter[2] as IACObject);
                    return true;
                case "ResetInEvents":
                    ResetInEvents();
                    return true;
                case "IsEnabledResetInEvents":
                    result = IsEnabledResetInEvents();
                    return true;
                case "ReloadInEvents":
                    ReloadInEvents();
                    return true;
                case "IsEnabledReloadInEvents":
                    result = IsEnabledReloadInEvents();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }


        #region ACState
        public void Start()
        {
            CurrentACState = ACStateEnum.SMStarting;
        }

        [ACMethodState("en{'Executing'}de{'Ausführend'}", 20, true)]
        public void SMStarting()
        {
            Reset();
            GroupPWComponent.GroupComplete();
        }
        #endregion

        #region Callbacks
        [ACMethodInfo("Function", "en{'PWPointInCallback'}de{'PWPointInCallback'}", 9999)]
        public void PWPointInCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                PWPointIn.UpdateActiveState(wrapObject);
                if (PWPointIn.IsActive)
                {
                    PWPointIn.ResetActiveStates();
                    Start();
                }
            }
        }

        /// <summary>Resets all received Events (sets ACPointEventSubscrWrap&lt;ACComponent&gt;.IsActive false at all event-subscriptions in the connectionlist)</summary>
        [ACMethodInteraction("Process", "en{'Reset Inputevents'}de{'Eingangsevents resetten'}", 2000, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void ResetInEvents()
        {
            if (!IsEnabledResetInEvents())
                return;
            PWPointIn.ResetActiveStates();
        }

        public virtual bool IsEnabledResetInEvents()
        {
            return true;
        }


        /// <summary>
        /// Unsubscribes all events from Source-Points. Afterwards all events are subscribed again.
        /// </summary>
        [ACMethodInteraction("Process", "en{'Reload Inputevent-Relations'}de{'Eingangsevent-Beziehungen neu laden'}", 2002, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void ReloadInEvents()
        {
            if (!IsEnabledReloadInEvents())
                return;
            PWPointIn.UnSubscribeAllEvents();
            PWPointIn.SubscribeACClassWFEdgeEvents();
        }

        public virtual bool IsEnabledReloadInEvents()
        {
            return true;
        }

        #endregion

        #region Planning and Testing
        protected override TimeSpan GetPlannedDuration()
        {
            return TimeSpan.Zero;
        }
        #endregion

        #endregion
    }
}
