using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.ISOonTCP;

namespace gip.core.communication
{
    public class S7TCPItem : IACObject, INotifyPropertyChanged
    {
        #region c'tors
        public S7TCPItem(IACPropertyNetServer acProperty, string itemAddr, S7TCPSubscr parentSubscription)
        {
            _ItemAddr = itemAddr;
            _ACProperty = acProperty;
            //_ParentSubscription = new ACRef<S7TCPSubscr>(parentSubscription, this);
            _ParentSubscription = parentSubscription;
            _ACProperty.ValueUpdatedOnReceival += OnSendValueToPLC;
            _IsValidItemSyntax = ItemSyntaxResolver.Resolve(_ItemAddr, out _ItemDataType, out _ItemVarType, out _ItemDBNo, out _ItemStartByteAddr, out _ItemLength, out _ItemBitNo);
        }
        #endregion

        #region Properties
        //protected ACRef<S7TCPSubscr> _ParentSubscription = null; ACRef nicht notwendig, da bei Restart sowieso Items neu geladen werden müssen
        protected S7TCPSubscr _ParentSubscription = null;
        public S7TCPSubscr ParentSubscription
        {
            get
            {
                if (_ParentSubscription == null)
                    return null;
                //return _ParentSubscription.Obj;
                return _ParentSubscription;
            }
        }

        public Type RequestedDatatype
        {
            get
            {
                if ((_ACProperty.Value != null) && (_ACProperty.Value is ACCustomTypeBase))
                    return (_ACProperty.Value as ACCustomTypeBase).TypeOfValueT;
                else
                    return _ACProperty.ACType.ObjectType;
            }
        }

        private string _ItemAddr = "";
        public string ItemAddr
        {
            get
            {
                return _ItemAddr;
            }
        }

        private IACPropertyNetServer _ACProperty;
        public IACPropertyNetServer ACProperty
        {
            get
            {
                return _ACProperty;
            }
        }

        private bool _IsValidItemSyntax = false;
        public bool IsValidItemSyntax
        {
            get
            {
                return _IsValidItemSyntax;
            }
        }

        private DataType _ItemDataType;
        public DataType ItemDataType
        {
            get
            {
                return _ItemDataType;
            }
        }

        private VarType _ItemVarType;
        public VarType ItemVarType
        {
            get
            {
                return _ItemVarType;
            }
        }

        private int _ItemDBNo = -1;
        public int ItemDBNo
        {
            get
            {
                return _ItemDBNo;
            }
        }

        private int _ItemStartByteAddr = -1;
        public int ItemStartByteAddr
        {
            get
            {
                return _ItemStartByteAddr;
            }
        }

        private int _ItemLength = 1;
        public int ItemLength
        {
            get
            {
                // S5- und S7-Strings sind unterschiedlich aufgebaut. 
                // Während S5-Strings keine Längenangaben enthalten, 
                // sind in einem S7-String die ersten beiden Bytes mit entsprechenden Angaben belegt:
                // enthält im 1. Byte maximale Länge und im 2. Byte tatsächliche Länge
                if (ItemVarType == VarType.String || ItemVarType == VarType.Base64String)
                    return _ItemLength + 2;
                return _ItemLength;
            }
        }

        private int _StringLen = -1;
        public int StringLen
        {
            get
            {
                if (_StringLen <= -1)
                    return _ItemLength;
                return _ItemLength;
            }
        }

        private short _ItemBitNo = 0;
        public short ItemBitNo
        {
            get
            {
                return _ItemBitNo;
            }
        }

        public int ItemEndByteAddr
        {
            get
            {
                return ItemStartByteAddr + ItemLength - 1;
            }
        }

        private int _ReadUpdates = 0;
        private int _WriteUpdates = 0;
        #endregion

        #region Methods
        public void DeInit()
        {
            if (_ACProperty != null)
            {
                _ACProperty.ValueUpdatedOnReceival -= OnSendValueToPLC;
                _ACProperty = null;
            }
        }

        public void Reconnected()
        {
            _ReadUpdates = 0;
        }

        public void Refresh(S7TCPDataBlock dataBlock, ref byte[] readResult, int atIndex, int dbReadStartIndex)
        {
            bool isEqualInRAM = true;
            byte[] readExtract = new byte[ItemLength];
            byte[] currentRAMExtract = new byte[ItemLength];
            int toIndex = atIndex + ItemLength;
            for (int i = 0; i < ItemLength; i++)
            {
                readExtract[i] = readResult[atIndex + i];
                if ((dataBlock.RAMinPLC == null)
                    || (readResult[atIndex + i] != dataBlock.RAMinPLC[dbReadStartIndex + atIndex + i]))
                {
                    // Check on Bit-Level if RAM in PLC has changed, beacus of Concurrency-Problems in
                    // method OnSetValueFromPLC() and OnSendValueToPLC():
                    // e.G. there are two SPSItems which points to the same byte. (ItemA: Bit00, ItemB: Bit01)
                    // if ItemB is changed and another thread changes the ValueT of ItemA an calls OnValueUpdatedOnReceival()
                    // When this thread is faster, then OnSetValueFromPLC will be called for ItemA because Bit01 is changed an from the same byte
                    // this.OnSetValueFromPLC changes the ValueT-Value. When the sceduler now switches back to the method OnValueUpdatedOnReceival() which calls this.OnSendValueToPLC()
                    // then the old read-Value from PLC will be send instead of new value of ItemA
                    if (ItemVarType == VarType.Bit && dataBlock.RAMinPLC != null)
                    {
                        BitAccessForByte bitANew = new BitAccessForByte() { ValueT = readResult[atIndex + i] };
                        BitAccessForByte bitARam = new BitAccessForByte() { ValueT = dataBlock.RAMinPLC[dbReadStartIndex + atIndex + i] };
                        if (bitARam.GetBitValue(ItemBitNo) != bitANew.GetBitValue(ItemBitNo))
                        {
                            isEqualInRAM = false;
                            break;
                        }
                    }
                    else
                        isEqualInRAM = false;
                }
            }

            if ((ACProperty == null))
                return;

            switch (ItemVarType)
            {
                case VarType.Bit:
                    BitAccessForByte bitAccess = new BitAccessForByte() { ValueT = readExtract[0] };
                    OnSetValueFromPLC(bitAccess.GetBitValue(ItemBitNo), isEqualInRAM);
                    break;
                case VarType.Byte:
                    OnSetValueFromPLC(readExtract[0], isEqualInRAM);
                    break;
                case VarType.Word:
                    OnSetValueFromPLC(gip.core.communication.ISOonTCP.Types.Word.FromByteArray(readExtract, ParentSubscription.Endianess), isEqualInRAM);
                    break;
                case VarType.Int:
                    OnSetValueFromPLC(gip.core.communication.ISOonTCP.Types.Int.FromByteArray(readExtract, ParentSubscription.Endianess), isEqualInRAM);
                    break;
                case VarType.DWord:
                    OnSetValueFromPLC(gip.core.communication.ISOonTCP.Types.DWord.FromByteArray(readExtract, ParentSubscription.Endianess), isEqualInRAM);
                    break;
                case VarType.DInt:
                    OnSetValueFromPLC(gip.core.communication.ISOonTCP.Types.DInt.FromByteArray(readExtract, ParentSubscription.Endianess), isEqualInRAM);
                    break;
                case VarType.Real:
                    OnSetValueFromPLC(gip.core.communication.ISOonTCP.Types.Real.FromByteArray(readExtract, ParentSubscription.Endianess), isEqualInRAM);
                    break;
                case VarType.String:
                case VarType.Base64String:
                    _StringLen = readExtract[0];
                    OnSetValueFromPLC(gip.core.communication.ISOonTCP.Types.String.FromByteArray(readExtract, this.ItemVarType == VarType.Base64String), isEqualInRAM);
                    break;
                case VarType.Timer:
                    OnSetValueFromPLC(gip.core.communication.ISOonTCP.Types.Timer.FromByteArray(readExtract), isEqualInRAM);
                    break;
                case VarType.Counter:
                    OnSetValueFromPLC(gip.core.communication.ISOonTCP.Types.Counter.FromByteArray(readExtract), isEqualInRAM);
                    break;
            }
        }

        private byte _ReadLockCounter = 0;
        //private bool _ReSendLock = false;
        private readonly ACMonitorObject _30150_ReadItemLock = new ACMonitorObject(30150);
        private void OnSetValueFromPLC(object value, bool isEqualInRAM)
        {
            // Falls Konfigurationsvariable: (Beschreiben von der SPS nicht erlaubt und Target-Property)
            if (ACProperty == null || !ACProperty.PropertyInfo.IsInput && ACProperty is IACPropertyNetTarget)
                return;

            using (ACMonitor.Lock(_30150_ReadItemLock))
            {
                // Writing has a higher priority, so doesn't update
                if (_ReadLockCounter <= 0)
                {
                    try
                    {
                        bool changeValue = true;
                        object newValue = Convert.ChangeType(value, RequestedDatatype);
                        if (_ReadUpdates > 0 && _ParentSubscription.Root.Initialized && isEqualInRAM && !ACProperty.ForceBroadcast)
                            changeValue = false;
                        if (!ACProperty.PropertyInfo.IsInput && ACProperty is IACPropertyNetServer && _ParentSubscription.Root.Initialized && _ReadUpdates >= 2)
                            changeValue = false;

                        //if (isEqualInRAM && !ACProperty.ForceBroadcast)
                        //{
                        //    if ((ACProperty.Value != null) && ACProperty.Value is ACCustomTypeBase)
                        //    {
                        //        if ((ACProperty.Value as ACCustomTypeBase).Value == newValue)
                        //            changeValue = false;
                        //    }
                        //    else
                        //    {
                        //        if (ACProperty.Value == newValue)
                        //            changeValue = false;
                        //    }
                        //}

                        if (changeValue)
                        {
                            _ReadUpdates++;
                            //_ReSendLock = true;
                            if ((ACProperty.Value != null) && ACProperty.Value is ACCustomTypeBase)
                                (ACProperty.Value as ACCustomTypeBase).ChangeValueServer(newValue, true, this);
                            else
                                ACProperty.ChangeValueServer(newValue, ACProperty.ForceBroadcast, this);
                        }
                    }
                    catch (OverflowException oex)
                    {
                        string message = String.Format("_ItemAddr: {0}, _ACProperty: {1}, Message: {2}", _ItemAddr, _ACProperty.GetACUrl(), oex.Message);
                        _ParentSubscription.Messages.LogException(_ParentSubscription.GetACUrl(), "S7TCPItem.OnSetValueFromPLC(1)", message);
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("S7TCPItem", "OnSetValueFromPLC", String.Format("_ItemAddr: {0}, _ACProperty: {1}, Message: {2}", _ItemAddr, _ACProperty.GetACUrl(), msg));
                    }
                    finally
                    {
                        //_ReSendLock = false;
                    }
                }
                else
                {
                    _ReadLockCounter--; // If something was wrong and method MarkItemAsWritten() was not called properly
                }
            }
        }

        void OnSendValueToPLC(object sender, ACPropertyChangedEventArgs e, ACPropertyChangedPhase phase)
        {
            if (phase == ACPropertyChangedPhase.AfterBroadcast)
                return;
            if (ACProperty == null || !ParentSubscription.IsReadyForWriting)
                return;
            // Don't resend when new value receive over OnSetValueFromPLC()
            if (e.ValueEvent.InvokerInfo != null
                && e.ValueEvent.InvokerInfo == this
                && e.ValueEvent.EventType == EventTypes.ValueChangedInSource)
                return;

            using (ACMonitor.Lock(_30150_ReadItemLock))
            {
                //if (_ReSendLock)
                //    return;
                _ReadLockCounter = 2;
                _WriteUpdates++;
                (ParentSubscription.ParentACComponent as S7TCPSession).SendItem(this);
            }
        }

        internal void MarkItemAsWritten()
        {
            _ReadLockCounter = 0;
        }
#endregion

#region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region IACObject Member
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return this.ReflectACType(); }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return null; }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return null;
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return false;
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return _ACProperty; }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return _ACProperty.GetACUrl() + "\\" + ACIdentifier;
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return ItemAddr; }
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }
#endregion
    }
}
