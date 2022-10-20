using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'BitAccess DK800-State'}de{'Bitzugriff DK800-State'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    [ACPropertyEntity(100, "Bit00", "en{'Min.load(Bit00)'}de{'Mindestlast(Bit00)'}")]
    [ACPropertyEntity(101, "Bit01", "en{'Unrest(Bit01)'}de{'Unruhe(Bit01)'}")]
    [ACPropertyEntity(102, "Bit02", "en{'1/10d(Bit02)'}de{'1/10d(Bit02)'}")]
    [ACPropertyEntity(103, "Bit03", "en{'Empty(Bit03)'}de{'Leer(Bit03)'}")]
    [ACPropertyEntity(104, "Bit04", "en{'Overfill(Bit04)'}de{'Übervoll(Bit04)'}")]
    [ACPropertyEntity(109, "Bit09", "en{'Keyboard blocked(Bit09)'}de{'Tastatur gesperrt(Bit09)'}")]
    [ACPropertyEntity(110, "Bit10", "en{'Tolerance(Bit10)'}de{'Toleranz(Bit10)'}")]
    [ACPropertyEntity(111, "Bit11", "en{'Weighing value is dosing value(Bit11)'}de{'Gewichtswert ist Dosierwert(Bit11)'}")]
    [ACPropertyEntity(112, "Bit12", "en{'Net-Mode(Bit12)'}de{'Nettomodus(Bit12)'}")]
    public class PAEScaleDK800State : BitAccessForInt16
    {
        #region c'tors
        public PAEScaleDK800State()
        {
        }

        public PAEScaleDK800State(IACType acValueType)
            : base(acValueType)
        {
        }

        public bool Bit00_MinLoad
        {
            get { return Bit00; }
            set { Bit00 = value; }
        }

        public bool Bit01_Unrest
        {
            get { return Bit01; }
            set { Bit01 = value; }
        }

        public bool Bit03_IsEmpty
        {
            get { return Bit03; }
            set { Bit03 = value; }
        }

        public bool Bit04_Overfill
        {
            get { return Bit04; }
            set { Bit04 = value; }
        }

        public bool Bit09_Keyboard
        {
            get { return Bit09; }
            set { Bit09 = value; }
        }

        public bool Bit10_Tolerance
        {
            get { return Bit10; }
            set { Bit10 = value; }
        }

        public bool Bit11_IsDosingValue
        {
            get { return Bit11; }
            set { Bit11 = value; }
        }

        public bool Bit12_IsNetMode
        {
            get { return Bit12; }
            set { Bit12 = value; }
        }

        #endregion
    }


    [ACClassInfo(Const.PackName_VarioSystem, "en{'DK800'}de{'DK800'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEScaleDK800 : PAModule
    {
        #region c'tors/Init

        static PAEScaleDK800()
        {
            RegisterExecuteHandler(typeof(PAEScaleDK800), HandleExecuteACMethod_PAEScaleDK800);
        }

        public PAEScaleDK800(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _EventSubscr = new ACPointEventSubscr(this, "EventSubscr", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            if (String.IsNullOrEmpty(DeviceID))
                DeviceID = this.ACIdentifier;

            if (String.IsNullOrEmpty(ScaleBusURL))
            {
                if (ParentACComponent is PAMSerialBusDK800)
                    ScaleBusURL = ParentACComponent.GetACUrl();
            }

            return true;
        }

        public override bool ACPostInit()
        {
            if (!String.IsNullOrEmpty(ScaleBusURL))
            {
                ScaleBus = new ACRef<PAMSerialBusDK800>(ScaleBusURL, this);
                if ((ScaleBus != null) && ScaleBus.IsObjLoaded)
                {
                    EventSubscr.SubscribeEvent(ScaleBus.ValueT, "ReadOnBusEvent", EventCallback);
                }
            }

            return base.ACPostInit();
        }


        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if ((ScaleBus != null) && ScaleBus.IsObjLoaded)
            {
                EventSubscr.UnSubscribeAllEvents(ScaleBus.ValueT);
                ScaleBus.Detach();
                ScaleBus = null;
            }
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region const
        public const byte STX = 0x02;
        public const byte ETB = 0x17;
        #endregion

        #region Methods
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "EventCallback":
                    EventCallback((gip.core.datamodel.IACPointNetBase)acParameter[0], (gip.core.datamodel.ACEventArgs)acParameter[1], (gip.core.datamodel.IACObject)acParameter[2]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public void OnFullWeightDataRead(byte[] arrRespData)
        {
            if (arrRespData[0] == STX && arrRespData[19] == ETB)
            {
                byte[] checksum = Checksum(arrRespData, 1, 16);
                if (checksum[0] == arrRespData[17] && checksum[1] == arrRespData[18])
                {
                    if (ScaleState.ValueT == null)
                        ScaleState.ValueT = new PAEScaleDK800State();
                    ScaleState.ValueT.ValueT = BitConverter.ToUInt16(new byte[] { arrRespData[9], arrRespData[10] }, 0);

                    string sWeight = Encoding.ASCII.GetString(arrRespData, 2, 6);
                    Int32 iWeight = Int32.Parse(sWeight);
                    double dWeight = Convert.ToDouble(iWeight);
                    if (arrRespData[1] == 0x2D) // "-"
                    {
                        dWeight *= -1;
                    }
                    else if (arrRespData[1] == 0x20) // " "
                    {
                        dWeight = 0;
                    }
                    if (arrRespData[8] == 0x31)
                        dWeight *= 0.1;
                    else if (arrRespData[8] == 0x32)
                        dWeight *= 0.01;
                    else if (arrRespData[8] == 0x33)
                        dWeight *= 0.001;
                    else if (arrRespData[8] == 0x34)
                        dWeight *= 0.0001;
                    else if (arrRespData[8] == 0x35)
                        dWeight *= 0.00001;
                    else if (arrRespData[8] == 0x36)
                        dWeight *= 0.000001;
                    ActualWeight.ValueT = dWeight;
                }
            }
        }

        public static byte[] Checksum(byte[] arr, int start, int length)
        {
            byte bcc = 0;
            byte bcc0 = 0;
            byte bcc1 = 0;
            for (int i = start; i < (start + length); i++)
            {
                bcc += arr[i];
            }
            bcc0 = bcc;
            bcc1 = bcc;
            bcc1 = (byte)(bcc & 0x0f);
            bcc1 = (byte)(bcc1 + 0x30);
            bcc0 >>= 4;
            bcc0 = (byte)(bcc0 & 0x0f);
            bcc0 = (byte)(bcc0 + 0x30);
            return new byte[] { bcc0, bcc1 };
        }

        #endregion

        #region Points and Events

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
                if (sender.ACIdentifier == "ReadOnBusEvent")
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
                    //AnalyzeScanEvent(data);
                }
            }
        }
        #endregion

        #region Properties

        #region Configuration

        [ACPropertyInfo(true, 200, "Configuration", "en{'DeviceID'}de{'GeräteID'}")]
        public string DeviceID { get; set; }

        [ACPropertyInfo(9999)]
        public ACRef<PAMSerialBusDK800> ScaleBus { get; set; }

        [ACPropertyInfo(false, 201, "Configuration", "en{'URL to Bus'}de{'Url vom Bus'}")]
        public string ScaleBusURL { get; set; }
        
        #endregion

        #region Source
        
        [ACPropertyBindingSource(210, "Read from DK800", "en{'Actual-/Netweight'}de{'Ist-/Nettogewicht'}", "", false, false)]
        public IACContainerTNet<Double> ActualWeight { get; set; }

        [ACPropertyBindingSource(211, "Read from DK800", "en{'Scale state'}de{'Waagenstatus'}", "", false, false)]
        public IACContainerTNet<PAEScaleDK800State> ScaleState { get; set; }


        #endregion


        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEScaleDK800(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAModule(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }

}
