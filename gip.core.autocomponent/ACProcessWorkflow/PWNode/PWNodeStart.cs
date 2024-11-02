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
    /// The PWNodeStart class is always the first workflow node within a node that implements the IACComponentPWGroup interface (these are the PWProcessFunction and PWGroup classes). PWNodeStart implements the interface  IPWNodeOut, which specifies the output event point " PWPointOut " of type ACPointEvent . Subsequent nodes within the group must be connected to this event point. The trigger point is triggered when the surrounding group has called the Start() method.
    /// </summary>
    /// <seealso cref="gip.core.autocomponent.PWBase" />
    /// <seealso cref="gip.core.autocomponent.IPWNodeOut" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWNodeStart'}de{'PWNodeStart'}", Global.ACKinds.TPWNodeStart, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeStart : PWBase, IPWNodeOut
    {
        public const string PWClassName = "PWNodeStart";
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

        static PWNodeStart()
        {
            ACEventArgs TMP;

            _SVirtualEventArgs = new Dictionary<string, ACEventArgs>(PWBase.SVirtualEventArgs, StringComparer.OrdinalIgnoreCase);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("TimeInfo", typeof(PATimeInfo), null, Global.ParamOption.Required));
            _SVirtualEventArgs.Add(Const.PWPointOut, TMP);
        }

        public PWNodeStart(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PWPointOut = new ACPointEvent(this, Const.PWPointOut, 0);
        }

        #endregion

        #region Points

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

        #region Public
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Start":
                    Start();
                    return true;
                case "SMStarting":
                    SMStarting();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }


        [ACMethodInteraction("Process", "en{'Start'}de{'Start'}", 301, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public void Start()
        {
            CurrentACState = ACStateEnum.SMStarting;
        }

        [ACMethodState("en{'Executing'}de{'Ausführend'}", 20, true)]
        public virtual void SMStarting()
        {
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs(Const.PWPointOut, VirtualEventArgs);
            eventArgs.GetACValue("TimeInfo").Value = RecalcTimeInfo();

            this.PWPointOut.Raise(eventArgs);

            if (IsACStateMethodConsistent(ACStateEnum.SMStarting) < ACStateCompare.WrongACStateMethod) // Vergleich notwendig, da durch Callbacks im selben Callstack, der Status evtl. schon weitergesetzt worden ist
                Reset();
        }


        #endregion

        #region Protected

        protected override TimeSpan GetPlannedDuration()
        {
            return TimeSpan.Zero;
        }

        #endregion

    }
}
