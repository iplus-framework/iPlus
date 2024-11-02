// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Xml;

namespace gip.core.autocomponent
{
    /// <summary>
    /// The PWBaseInOut  class is a combination of PWNodeStart and PWNodeEnd. 
    /// It can receive events (input point of type PWPointIn) as well as send an output event (starting point of type ACPointEvent). 
    /// It is therefore the base class for all other workflow classes.
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PWBase" />
    /// <seealso cref="gip.core.autocomponent.IPWNodeIn" />
    /// <seealso cref="gip.core.autocomponent.IPWNodeOut" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWInOut'}de{'PWInOut'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.Optional, false, false)]
    public abstract class PWBaseInOut : PWBase, IPWNodeIn, IPWNodeOut
    {

        private static Dictionary<string, ACEventArgs> _SVirtualEventArgs;

        #region Properties

        public static new Dictionary<string, ACEventArgs> SVirtualEventArgs
        {
            get { return _SVirtualEventArgs; }
        }

        public override Dictionary<string, ACEventArgs> VirtualEventArgs
        {
            get
            {
                return SVirtualEventArgs;
            }
        }

        #endregion

        #region Constructors

        static PWBaseInOut()
        {
            ACEventArgs TMP;

            _SVirtualEventArgs = new Dictionary<string, ACEventArgs>(PWBase.SVirtualEventArgs, StringComparer.OrdinalIgnoreCase);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("TimeInfo", typeof(PATimeInfo), null, Global.ParamOption.Required));
            _SVirtualEventArgs.Add(Const.PWPointOut, TMP);
        }

        public PWBaseInOut(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PWPointIn = new PWPointIn(this, Const.PWPointIn, 0);
            _PWPointOut = new ACPointEvent(this, Const.PWPointOut, 0);
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

        protected ACPointEvent _PWPointOut;
        [ACPropertyEventPoint(9999, true)]
        public ACPointEvent PWPointOut
        {
            get
            {
                return _PWPointOut;
            }
        }

        #endregion

        #region Properties
        public PWGroup ParentPWGroup
        {
            get
            {
                return ParentACComponent as PWGroup;
            }
        }
        #endregion

        #region Public

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(RaiseOutEvent):
                    RaiseOutEvent();
                    return true;
                case nameof(ResetInEvents):
                    ResetInEvents();
                    return true;
                case nameof(ReloadInEvents):
                    ReloadInEvents();
                    return true;
                case nameof(IsEnabledRaiseOutEvent):
                    result = IsEnabledRaiseOutEvent();
                    return true;
                case nameof(IsEnabledResetInEvents):
                    result = IsEnabledResetInEvents();
                    return true;
                case nameof(IsEnabledReloadInEvents):
                    result = IsEnabledReloadInEvents();
                    return true;
                case nameof(PWPointInCallback):
                    PWPointInCallback(acParameter[0] as IACPointNetBase, acParameter[1] as ACEventArgs, acParameter[2] as IACObject);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        [ACMethodInfo("Function", "en{'PWPointInCallback'}de{'PWPointInCallback'}", 9999)]
        public abstract void PWPointInCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject);


        /// <summary>
        /// Raises the the PWPointOut-Event.
        /// </summary>
        [ACMethodInteraction("Process", "en{'Raise output event'}de{'Ausgangs-Ereignis auslösen'}", 2003, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void RaiseOutEvent()
        {
            if (!IsEnabledRaiseOutEvent())
                return;
            ResetInEvents();
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs(Const.PWPointOut, VirtualEventArgs);
            eventArgs.GetACValue("TimeInfo").Value = RecalcTimeInfo();
            PWPointOut.Raise(eventArgs);
        }

        public virtual bool IsEnabledRaiseOutEvent()
        {
            return true;
        }

        /// <summary>Resets all received Events (sets ACPointEventSubscrWrap&lt;ACComponent&gt;.IsActive false at all event-subscriptions in the connectionlist)</summary>
        [ACMethodInteraction("Process", "en{'Reset input events'}de{'Eingangs-Ereignisse zurücksetzen'}", 2000, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
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

        #region Diagnostics and Dump
        protected override void DumpPointList(XmlDocument doc, XmlElement xmlACPropertyList)
        {
            base.DumpPointList(doc, xmlACPropertyList);

            XmlElement wfInfos = xmlACPropertyList["PWPointInInfo"];
            if (wfInfos == null && ContentACClassWF != null)
            {
                wfInfos = doc.CreateElement("PWPointInInfo");
                if (wfInfos != null)
                {
                    wfInfos.InnerText = PWPointIn.DumpStateInfo();
                }
                xmlACPropertyList.AppendChild(wfInfos);
            }

            wfInfos = xmlACPropertyList["PWPointOutInfo"];
            if (wfInfos == null && ContentACClassWF != null)
            {
                wfInfos = doc.CreateElement("PWPointOutInfo");
                if (wfInfos != null)
                {
                    wfInfos.InnerText = PWPointOut.DumpStateInfo();
                }
                xmlACPropertyList.AppendChild(wfInfos);
            }
        }
        #endregion
    }
}
