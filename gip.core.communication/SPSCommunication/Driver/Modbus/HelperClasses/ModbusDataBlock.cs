using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.modbus;
using System.Threading;

namespace gip.core.communication
{
    public class ModbusDataBlockReadSegment
    {
        //public const int PDUMaxDataSize = 242; //In bytes ModbusMaster.ReadWriteMultipleRegisters() : 121 * 2
        public const int PDUMaxDataSize = 200; //In bytes ModbusMaster.ReadWriteMultipleRegisters() : 121 * 2

        public ModbusDataBlockReadSegment(int startIndex, int readLength)
        {
            _StartIndex = startIndex;
            _ReadLength = readLength;
        }

        private int _StartIndex = 0;
        /// <summary>
        /// bytewise index
        /// </summary>
        public int StartIndex
        {
            get
            {
                return _StartIndex;
            }
        }

        private int _ReadLength = 0;
        /// <summary>
        /// Read length in bytes
        /// </summary>
        public int ReadLength
        {
            get
            {
                return _ReadLength;
            }
            internal set
            {
                _ReadLength = value;
            }
        }

        short? _FirstItemBitNo;
        public short? FirstItemBitNo
        {
            get
            {
                return _FirstItemBitNo;
            }
            internal set
            {
                _FirstItemBitNo = value;
            }
        }

        short? _LastItemBitNo;
        public short? LastItemBitNo
        {
            get
            {
                return _LastItemBitNo;
            }
            internal set
            {
                _LastItemBitNo = value;
            }
        }
    }

    public class ModbusDataBlock : SortedDictionary<Int32, ModbusDataBlockItems>, INotifyPropertyChanged
    {
        #region c'tors
        public ModbusDataBlock(int nDBNo)
        {
            _DBNo = nDBNo;
        }
        #endregion

        #region Properties

        private int _DBNo = 0;
        public int DBNo
        {
            get
            {
                return _DBNo;
            }
        }

        public TableType TableType
        {
            get
            {
                return (TableType)DBNo;
            }
        }

        public int _RequestedSize = -1;
        public int RequestedSize
        {
            get
            {
                if (_RequestedSize >= 0)
                    return _RequestedSize;
                _RequestedSize = 0;
                if (ItemList.Any())
                {
                    ModbusDataBlockItems itemList = ItemList.Last();
                    int nMaxLength = 0;
                    int nIndex = 0;
                    foreach (ModbusItem s7Item in itemList)
                    {
                        nIndex = s7Item.ItemStartByteAddr;
                        if (s7Item.ItemLength > nMaxLength)
                            nMaxLength = s7Item.ItemLength;
                    }
                    _RequestedSize = nIndex + nMaxLength;
                }
                return _RequestedSize;
            }
        }

        private List<ModbusDataBlockReadSegment> _ReadSegmentsList = null;
        public IEnumerable<ModbusDataBlockReadSegment> ReadSegmentsList
        {
            get
            {
                if (_ReadSegmentsList != null)
                    return _ReadSegmentsList;
                _ReadSegmentsList = new List<ModbusDataBlockReadSegment>();
                if (ItemList.Any())
                {
                    ModbusDataBlockReadSegment lastSegment = null;
                    //_ReadSegmentsList.Add(lastSegment);
                    int nLastStartIndex = ItemList.FirstOrDefault().FirstOrDefault().ItemStartByteAddr;
                    foreach (KeyValuePair<Int32, ModbusDataBlockItems> kvp in this)
                    {
                        int endByteAddr = kvp.Key + kvp.Value.MaxLength - 1;
                        int segmentLength = endByteAddr - nLastStartIndex + 1; // End-Adresse
                        if (segmentLength > ModbusDataBlockReadSegment.PDUMaxDataSize)
                        {
                            nLastStartIndex = kvp.Key;
                            segmentLength = endByteAddr - nLastStartIndex + 1;
                            lastSegment = new ModbusDataBlockReadSegment(nLastStartIndex, segmentLength);
                            // Falls Bits gelesen werden müssen
                            if (TableType == modbus.TableType.Input || TableType == modbus.TableType.Output)
                            {
                                lastSegment.FirstItemBitNo = kvp.Value.FirstItemBitNo;
                                lastSegment.LastItemBitNo = kvp.Value.LastItemBitNo;
                            }
                            _ReadSegmentsList.Add(lastSegment);
                        }
                        else
                        {
                            if (lastSegment == null)
                            {
                                nLastStartIndex = kvp.Key;
                                lastSegment = new ModbusDataBlockReadSegment(nLastStartIndex, ModbusDataBlockReadSegment.PDUMaxDataSize);
                                // Falls Bits gelesen werden müssen
                                if (TableType == modbus.TableType.Input || TableType == modbus.TableType.Output)
                                {
                                    lastSegment.FirstItemBitNo = kvp.Value.FirstItemBitNo;
                                    lastSegment.LastItemBitNo = kvp.Value.LastItemBitNo;
                                }

                                _ReadSegmentsList.Add(lastSegment);
                            }
                            if (TableType == modbus.TableType.Input || TableType == modbus.TableType.Output)
                            {
                                lastSegment.LastItemBitNo = kvp.Value.LastItemBitNo;
                            }
                            lastSegment.ReadLength = segmentLength;
                        }
                    }
                }
                return _ReadSegmentsList;
            }
        }

        public IEnumerable<ModbusDataBlockItems> ItemList
        {
            get
            {
                return this.Select(c => c.Value);
            }
        }

        /// <summary>
        /// The Send and Receive/Poll ist done by seperate Threads
        /// The Send-Thread updates the RAM with the values from the Items which should be send
        /// The Read-Thread updates the RAM with the values from PLC
        /// If the Send and Read happens at the same time and the Write was not sucessful
        /// then then the RAM will be overridden with the old values form PLC
        /// The method S7TCPItem.Refresh() won't detect any change an the IACProperty-Value will not be updated with the value from PLC
        /// The lock ensures, that the read will be rejected an reread after writing 
        /// It the write in the PLC was not successful. The next read comes with the old values and a change is detected
        /// 
        /// Also another scenario is prevented:
        /// Two bits which points to the same byte will be changed sequentially by Thread A
        /// The Send-Thread begins to prepare the Send-Telegramm including the first bit (but not the second, beacuse ist will be send a cycle later)
        /// During sending the parallel Read updates also the RAM with the old value. So the first bit will be reset.
        /// Aftwerward the second bit will be send. It changes the ram and sends to the plc, with teh old Bit1-Value
        /// </summary>
        private bool _ReadPLCUpdateLock = false;
        private readonly ACMonitorObject _30162_LockReadPLCUpdateLock = new ACMonitorObject(30162);

        private readonly ACMonitorObject _30161_LockRAMinPLC = new ACMonitorObject(30161);
        private byte[] _RAMinPLC;
        internal byte[] RAMinPLC
        {
            get
            {
                return _RAMinPLC;
            }
            //set
            //{
            //    EnterCS();
            //    _RAMinPLC = value;
            //    LeaveCS();
            //}
        }

        EndianessEnum? _Endianess;
        private EndianessEnum Endianess
        {
            get
            {
                if (_Endianess.HasValue)
                    return _Endianess.Value;
                if (!this.Any())
                    _Endianess = EndianessEnum.MixedEndian;
                else
                {
                    var item = this.FirstOrDefault().Value.FirstOrDefault();
                    if (item == null)
                        _Endianess = EndianessEnum.MixedEndian;
                    else
                        _Endianess = item.ParentSubscription.Endianess;
                }
                return _Endianess.Value;
            }
        }

        public void UpdateRAMArea(ref byte[] update, int atIndex)
        {
            EnterLockRAMinPLC();
            if (_RAMinPLC == null)
                _RAMinPLC = new byte[this.RequestedSize];
            int ramEnd = update.Length + atIndex;
            if (_RAMinPLC.Length >= ramEnd)
            {
                int sizeToCopy = update.Length;
                for (int i = 0; i < sizeToCopy; i++)
                {
                    _RAMinPLC[atIndex + i] = update[i];
                }
            }
            ExitLockRAMinPLC();
        }


        private string _ReadErrorMessage = "";
        public string ReadErrorMessage
        {
            get
            {
                return _ReadErrorMessage;
            }
            set
            {
                bool changed = false;
                if (_ReadErrorMessage != value)
                    changed = true;
                _ReadErrorMessage = value;
                if (changed)
                    OnPropertyChanged("ReadErrorMessage");
            }
        }

        private string _WriteErrorMessage;
        public string WriteErrorMessage
        {
            get
            {
                return _WriteErrorMessage;
            }
            set
            {
                bool changed = false;
                if (_WriteErrorMessage != value)
                    changed = true;
                _WriteErrorMessage = value;
                if (changed)
                    OnPropertyChanged("WriteErrorMessage");
            }
        }

        #endregion

        #region Methods
        public void Add(ModbusItem s7Item)
        {
            ModbusDataBlockItems itemsList;
            if (!TryGetValue(s7Item.ItemStartByteAddr, out itemsList))
            {
                itemsList = new ModbusDataBlockItems();
                Add(s7Item.ItemStartByteAddr, itemsList);
            }
            itemsList.Add(s7Item);

            _RequestedSize = -1; // Recalc Requested Size
        }

        public void DeInit()
        {
            foreach (ModbusDataBlockItems itemList in ItemList)
            {
                itemList.DeInit();
            }
        }


        public void Reconnected()
        {
            foreach (ModbusDataBlockItems itemList in ItemList)
            {
                itemList.Reconnected();
            }
        }

        public void RefreshItems(ref bool[] readResult, ModbusDataBlockReadSegment readSegment)
        {
            if (readResult == null || readResult.Length <= 0)
                return;

            bool[] paddedBoolArr = readResult;
            if (readSegment.FirstItemBitNo.HasValue && readSegment.FirstItemBitNo.Value > 0
                && readSegment.LastItemBitNo.HasValue && readSegment.LastItemBitNo.Value < 7)
            {
                paddedBoolArr = new bool[readResult.Length + readSegment.FirstItemBitNo.Value + (7 - readSegment.LastItemBitNo.Value)];
                int i = 0;
                for (int j = 0; j < readSegment.FirstItemBitNo.Value; j++)
                {
                    paddedBoolArr[i] = false;
                    i++;
                }
                for (int j = 0; j < readResult.Length; j++)
                {
                    paddedBoolArr[i] = readResult[j];
                    i++;
                }
                for (int j = 0; j < (7 - readSegment.LastItemBitNo.Value); j++)
                {
                    paddedBoolArr[i] = false;
                    i++;
                }
            }
            else if (readSegment.FirstItemBitNo.HasValue && readSegment.FirstItemBitNo.Value > 0)
            {
                paddedBoolArr = new bool[readResult.Length + readSegment.FirstItemBitNo.Value];
                int i = 0;
                for (int j = 0; j < readSegment.FirstItemBitNo.Value; j++)
                {
                    paddedBoolArr[i] = false;
                    i++;
                }
                for (int j = 0; j < readResult.Length; j++)
                {
                    paddedBoolArr[i] = readResult[j];
                    i++;
                }
            }
            else if (readSegment.LastItemBitNo.HasValue && readSegment.LastItemBitNo.Value < 7)
            {
                paddedBoolArr = new bool[readResult.Length + (7 - readSegment.LastItemBitNo.Value)];
                int i = 0;
                for (int j = 0; j < readResult.Length; j++)
                {
                    paddedBoolArr[i] = readResult[j];
                    i++;
                }
                for (int j = 0; j < (7 - readSegment.LastItemBitNo.Value); j++)
                {
                    paddedBoolArr[i] = false;
                    i++;
                }
            }

            byte[] convResult = modbus.Types.Byte.Convert(paddedBoolArr);
            RefreshItems(ref convResult, readSegment.StartIndex);
        }

        public void RefreshItems(ref ushort[] readResult, int dbReadStartIndex = 0)
        {
            if (readResult == null || readResult.Length <= 0)
                return;
            byte[] convResult = modbus.Types.Word.ToByteArray(readResult, Endianess);
            RefreshItems(ref convResult, dbReadStartIndex);
        }

        public void RefreshItems(ref byte[] readResult, int dbReadStartIndex = 0)
        {

            using (ACMonitor.Lock(_30162_LockReadPLCUpdateLock))
            {
                if (!_ReadPLCUpdateLock)
                {
                    if (readResult == null)
                        return;
                    int dbReadEndIndex = dbReadStartIndex + readResult.Length - 1;
                    foreach (KeyValuePair<Int32, ModbusDataBlockItems> kvp in this)
                    {
                        if (kvp.Key < dbReadStartIndex)
                            continue;
                        else if (kvp.Key > dbReadEndIndex)
                            break;
                        int indexInResult = kvp.Key - dbReadStartIndex;
                        foreach (ModbusItem s7Item in kvp.Value)
                        {
                            s7Item.Refresh(this, ref readResult, indexInResult, dbReadStartIndex);
                        }
                    }
                    UpdateRAMArea(ref readResult, dbReadStartIndex);
                }
                //else
                //{
                //    bool isLocked = false;
                //}
            }
        }


        public bool AreItemsNeighboursDirect(ModbusItem item1, ModbusItem item2)
        {
            if (item1.ItemStartByteAddr == item2.ItemStartByteAddr)
                return true;
            if (item1.ItemStartByteAddr < item2.ItemStartByteAddr)
            {
                if ((item1.ItemEndByteAddr + 1) >= item2.ItemStartByteAddr)
                    return true;
            }
            else
            {
                if ((item2.ItemEndByteAddr + 1) >= item1.ItemStartByteAddr)
                    return true;
            }
            return false;
        }

        public bool AreItemsNeighboursInSegment(ModbusItem item1, ModbusItem item2)
        {
            if (item1.ItemStartByteAddr == item2.ItemStartByteAddr)
                return true;
            int firstIndex = 0;
            int secondIndex = 0;
            if (item1.ItemStartByteAddr < item2.ItemStartByteAddr)
            {
                firstIndex = item1.ItemEndByteAddr;
                secondIndex = item2.ItemEndByteAddr;
            }
            else
            {
                firstIndex = item2.ItemEndByteAddr;
                secondIndex = item1.ItemEndByteAddr;
            }

            bool checkNext = false;
            foreach (KeyValuePair<Int32, ModbusDataBlockItems> kvp in this)
            {
                if (checkNext == true)
                {
                    if (kvp.Key == secondIndex)
                        return true;
                    break;
                }
                if (kvp.Key == firstIndex)
                    checkNext = true;
            }
            return false;
        }


        /// <summary>
        /// Enters Critical Section e.g. if access to Childs-List which could be emptied from framework during access
        /// </summary>
        internal bool TryLockRAMinPLC()
        {
            int tries = 0;
            while (tries < 100)
            {
                if (ACMonitor.TryEnter(_30161_LockRAMinPLC, 10))
                    break;
                tries++;
            }
            return tries < 100;
        }

        internal void EnterLockRAMinPLC()
        {
            ACMonitor.Enter(_30161_LockRAMinPLC);
        }

        /// <summary>
        /// Leaves Critical Section
        /// </summary>
        internal void ExitLockRAMinPLC()
        {
            ACMonitor.Exit(_30161_LockRAMinPLC);
        }

        internal void SetLockReadUpdates()
        {

            using (ACMonitor.Lock(_30162_LockReadPLCUpdateLock))
            {
                _ReadPLCUpdateLock = true;
            }
        }

        /// <summary>
        /// Leaves Critical Section
        /// </summary>
        internal void ReleaseLockReadUpdates()
        {

            using (ACMonitor.Lock(_30162_LockReadPLCUpdateLock))
            {
                _ReadPLCUpdateLock = false;
            }
        }
#endregion

#region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
#endregion
    }
}
