// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Threading.Tasks;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Scanner'}de{'Scanner'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEScanner : PAModule
    {
        private static Dictionary<string, ACEventArgs> _SVirtualEventArgs;

        protected ACRef<PAEScannerDecoder> _Decoder = null;

        #region Properties

        /// <summary>
        /// TODO: Configuration-Property
        /// </summary>
        [ACPropertyInfo(9999)]
        public string DeviceID { get; set; }

        /// <summary>
        /// TODO: Configuration-Property
        /// </summary>
        [ACPropertyInfo(9999)]
        public ACRef<IACComponent> ScannerBus { get; set; }

        [ACPropertyInfo(9999)]
        public string ScannerBusURL { get; set; }

        [ACPropertyBindingSource(9999, "", "", "", true)]
        public IACContainerTNet<ACRef<VBUser>> User { get; set; }
        public void OnSetUser(IACPropertyNetValueEvent valueEvent)
        {
        }

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

        public PAEScannerDecoder Decoder
        {
            get
            {
                return _Decoder?.ValueT;
            }
        }

        #endregion

        #region Constructors

        static PAEScanner()
        {
            ACEventArgs TMP;

            _SVirtualEventArgs = new Dictionary<string,ACEventArgs>(PAModule.SVirtualEventArgs, StringComparer.OrdinalIgnoreCase);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("DeviceID", typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue("ScanData", typeof(string), null, Global.ParamOption.Required));
            _SVirtualEventArgs.Add("ScanEvent", TMP);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("DeviceID", typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue("User", typeof(VBUser), null, Global.ParamOption.Required));
            TMP.Add(new ACValue(nameof(PAEScannerDecoder.ScanSequenceList), typeof(BindingList<PAEScannerData>), null, Global.ParamOption.Required));
            TMP.Add(new ACValue(nameof(PAOrderInfo), typeof(PAOrderInfo), null, Global.ParamOption.Optional));
            _SVirtualEventArgs.Add(nameof(ScanSequenceCompleteEvent), TMP);

            RegisterExecuteHandler(typeof(PAEScanner), HandleExecuteACMethod_PAEScanner);
        }

        public PAEScanner(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ScanEvent = new ACPointEvent(this, nameof(ScanEvent), 0);
            _ScanSequenceCompleteEvent = new ACPointEvent(this, nameof(ScanSequenceCompleteEvent), 0);
            _EventSubscr = new ACPointEventSubscr(this, nameof(EventSubscr), 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            if (String.IsNullOrEmpty(DeviceID))
                DeviceID = this.ACIdentifier;

            if (String.IsNullOrEmpty(ScannerBusURL))
            {
                if (ParentACComponent is PAMBus)
                    ScannerBusURL = ParentACComponent.GetACUrl();
            }

            return true;
        }

        public override bool ACPostInit()
        {
            PAEScannerDecoder decoder = FindChildComponents< PAEScannerDecoder>(c => c is PAEScannerDecoder, null, 1).FirstOrDefault();
            if (decoder != null)
            {
                _Decoder = new ACRef<PAEScannerDecoder>(decoder,this);
                EventSubscr.SubscribeEvent(decoder, nameof(PAEScannerDecoder.ScanSequenceCompleteEvent), EventCallback);
            }
            if (!String.IsNullOrEmpty(ScannerBusURL))
            {
                ScannerBus = new ACRef<IACComponent>(ScannerBusURL,this);
                if ((ScannerBus != null) && ScannerBus.IsObjLoaded)
                {
                    EventSubscr.SubscribeEvent(ScannerBus.ValueT, nameof(PAMBus.ReadOnBusEvent), EventCallback);
                }
            }

            return base.ACPostInit();
        }


        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            if (_Decoder != null)
            {
                _Decoder.Detach();
                _Decoder = null;
            }

            if ((ScannerBus != null) && ScannerBus.IsObjLoaded)
            {
                EventSubscr.UnSubscribeAllEvents(ScannerBus.ValueT);
                ScannerBus.Detach();
                ScannerBus = null;
            }
            return await base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Public

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(AnalyzeScanEvent):
                    AnalyzeScanEvent((System.String)acParameter[0]);
                    return true;
                case nameof(EventCallback):
                    EventCallback((gip.core.datamodel.IACPointNetBase)acParameter[0], (gip.core.datamodel.ACEventArgs)acParameter[1], (gip.core.datamodel.IACObject)acParameter[2]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        [ACMethodInfo("Function", "en{'AnalyzeScanEvent'}de{'AnalyzeScanEvent'}", 9999)]
        public void AnalyzeScanEvent(string data)
        {
            if (_Decoder == null)
                return;
            if (!_Decoder.IsObjLoaded)
                return;

            _Decoder.ValueT.AnalyzeScanEvent(data);
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("ScanEvent", VirtualEventArgs);
 
            eventArgs.GetACValue("DeviceID").Value = DeviceID;
            eventArgs.GetACValue("ScanData").Value = data;
            
            ScanEvent.Raise(eventArgs);
        }


#endregion
        
#region Points and Events

        protected ACPointEvent _ScanEvent;
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent ScanEvent
        {
            get
            {
                return _ScanEvent;
            }
            set
            {
                _ScanEvent = value;
            }
        }

        protected ACPointEvent _ScanSequenceCompleteEvent;
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent ScanSequenceCompleteEvent
        {
            get
            {
                return _ScanSequenceCompleteEvent;
            }
            set
            {
                _ScanSequenceCompleteEvent = value;
            }
        }


        ACPointEventSubscr _EventSubscr;
        [ACPropertyEventPointSubscr(9999, false)]
        public ACPointEventSubscr EventSubscr
        {
            get
            {
                return _EventSubscr;
            }
        }

        [ACMethodInfo("Function", "en{'EventCallback'}de{'EventCallback'}", 9999)]
        public void EventCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                // Event vom Bus
                if (sender.ACIdentifier == nameof(PAMBus.ReadOnBusEvent))
                {
                    string deviceID = "";
                    string data = "";
                    foreach (ACValue param in e)
                    {
                        if (param.ACIdentifier == "DeviceID")
                        {
                            if (param.Value is string)
                                deviceID = (string)param.Value;
                        }
                        else if (param.ACIdentifier == "ScanData")
                        {
                            if (param.Value is string)
                                data = (string)param.Value;
                        }
                    }
                    if ((deviceID == "") || (data == ""))
                        return;
                    if (deviceID != this.DeviceID)
                        return;
                    AnalyzeScanEvent(data);
                }
                // Event arrived from Decoder
                else if (sender.ACIdentifier == nameof(PAEScannerDecoder.ScanSequenceCompleteEvent))
                {
                    ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs(nameof(PAEScannerDecoder.ScanSequenceCompleteEvent), VirtualEventArgs);

                    eventArgs.GetACValue("DeviceID").Value = DeviceID;
                    if (User != null && User.ValueT != null)
                        eventArgs.GetACValue("User").Value = User.ValueT;

                    ACValue acValue = e.GetACValue(nameof(PAEScannerDecoder.ScanSequenceList));
                    ACValue acValue2 = eventArgs.GetACValue(nameof(PAEScannerDecoder.ScanSequenceList));
                    if (acValue != null)
                        acValue2.Value = acValue.Value;

                    acValue = e.GetACValue(nameof(PAOrderInfo));
                    acValue2 = eventArgs.GetACValue(nameof(PAOrderInfo));
                    if (acValue != null)
                        acValue2.Value = acValue.Value;

                    ScanSequenceCompleteEvent.Raise(eventArgs);
                }
            }
        }

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEScanner(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAModule(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }

}
