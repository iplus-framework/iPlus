using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.IO;
using System.Xml;
using gip.core.datamodel;
using gip.core.autocomponent;


namespace gip.core.processapplication
{
    [DataContract]
    [ACSerializeableInfo(new Type[] {typeof(BindingList<PAEScannerData>)})]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAEScannerData'}de{'PAEScannerData'}", Global.ACKinds.TACClass)]
    public class PAEScannerData
    {
        [DataMember]
        public int BarcodeType { get; set; }

        private string[] _Fields = null;
        [DataMember]
        public string[] Fields
        {
            get
            {
                if (_Fields == null)
                    _Fields = new string[] { };
                return _Fields;
            }
            set
            {
                _Fields = value;
            }
        }
    }


    [ACClassInfo(Const.PackName_VarioSystem, "en{'Scannerdecoder'}de{'Scannerdecoder'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEScannerDecoder : PAModule
    {

        private static Dictionary<string, ACEventArgs> _SVirtualEventArgs;

        #region Properties

        [ACPropertyBindingSource]
        public IACContainerTNet<BindingList<PAEScannerData>> ScanSequenceList { get; set; }
        public void OnSetScanSequenceList(IACPropertyNetValueEvent valueEvent)
        {
        }

        [ACPropertyInfo(9999)]
        protected int ScanSequenceMode { get; set; }

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

        static PAEScannerDecoder()
        {
            ACEventArgs TMP;

            _SVirtualEventArgs = new Dictionary<string,ACEventArgs>(PAModule.SVirtualEventArgs, StringComparer.OrdinalIgnoreCase);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("ScanSequenceList", typeof(BindingList<PAEScannerData>), null, Global.ParamOption.Required));
            _SVirtualEventArgs.Add("ScanSequenceCompleteEvent", TMP);

            RegisterExecuteHandler(typeof(PAEScannerDecoder), HandleExecuteACMethod_PAEScannerDecoder);
        }

        public PAEScannerDecoder(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ScanSequenceCompleteEvent = new ACPointEvent(this, "ScanSequenceCompleteEvent", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }


        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        [ACMethodInfo("Function", "en{'AnalyzeScanEvent'}de{'AnalyzeScanEvent'}", 9999)]
        public void AnalyzeScanEvent(string data)
        {
            if (ScanSequenceList == null)
                return;
            PAEScannerData scannerData = null;
            try
            {
                scannerData = (PAEScannerData)ACUrlCommand("!DecodeScanData", new object[] { data });
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException(this.GetACUrl(), "PAEScannerDecoder.AnalyzeScanEvent(0)", "Exception in Script !DecodeScanData, Exception:"+msg);
            }
            if (scannerData == null)
                return;
            if (ScanSequenceList.ValueT == null)
                ScanSequenceList.ValueT = new BindingList<PAEScannerData>();
            ScanSequenceList.ValueT.Add(scannerData);
            bool sequenceFinished = false;
            try
            {
                sequenceFinished = (bool)ACUrlCommand("!AnalyzeScanSequence", new object[] { ScanSequenceList.ValueT });
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException(this.GetACUrl(), "PAEScannerDecoder.AnalyzeScanEvent(1)", "Exception in Script !AnalyzeScanSequence, Exception:"+msg);
            }
            if (sequenceFinished && (ScanSequenceCompleteEvent != null))
            {
                ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("ScanSequenceCompleteEvent", VirtualEventArgs);

                eventArgs.GetACValue("ScanSequenceList").Value = ScanSequenceList.ValueT;

                ScanSequenceCompleteEvent.Raise(eventArgs);
                ScanSequenceList.ValueT = new BindingList<PAEScannerData>();
            }
        }

        [ACMethodInfo("Function", "en{'DecodeScanData'}de{'DecodeScanData'}", 9999)]
        public virtual PAEScannerData DecodeScanData(string data)
        {
            return null;
        }

        /// <summary>
        /// AnalyzeScanSequence is called after Decoding an inserting PAEScannerDecoderData to SequenceList
        /// </summary>
        /// <returns>True, if SequenceList should be emptied.</returns>
        [ACMethodInfo("Function", "en{'AnalyzeScanSequence'}de{'AnalyzeScanSequence'}", 9999)]
        public virtual bool AnalyzeScanSequence(BindingList<PAEScannerData> ScanSequenceList)
        {
            /*foreach (gip.core.processapplication.PAEScannerData scanData in ScanSequenceList)
            {
                if (scanData.BarcodeType == 3)
                {
                    scanData.Fields[0]
                }
            }*/
            return true;
        }

#endregion

#region Points

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
        #endregion

        #region Public

        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEScannerDecoder(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAModule(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }

}
