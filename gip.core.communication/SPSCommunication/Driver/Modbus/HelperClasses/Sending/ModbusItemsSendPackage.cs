using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.modbus;

namespace gip.core.communication
{
    public class ModbusItemsSendPackageSegment
    {
        public ModbusItemsSendPackageSegment(int startIndex, byte[] segment, ModbusItems2SendEntry firstBooleanItem = null, ModbusItems2SendEntry lastBooleanItem = null)
        {
            _StartIndex = startIndex;
            _WriteSegment = segment;
            _FirstBooleanItem = firstBooleanItem;
            _LastBooleanItem = lastBooleanItem;
        }

        private int _StartIndex = 0;
        public int StartIndex
        {
            get
            {
                return _StartIndex;
            }
        }

        internal byte[] _WriteSegment;
        public byte[] WriteSegment
        {
            get
            {
                return _WriteSegment;
            }
            set
            {
                _WriteSegment = value;
            }
        }

        private ModbusItems2SendEntry _FirstBooleanItem;
        public ModbusItems2SendEntry FirstBooleanItem
        {
            get
            {
                return _FirstBooleanItem;
            }
            set
            {
                _FirstBooleanItem = value;
            }
        }

        private ModbusItems2SendEntry _LastBooleanItem;
        public ModbusItems2SendEntry LastBooleanItem
        {
            get
            {
                return _LastBooleanItem;
            }
            set
            {
                _LastBooleanItem = value;
            }
        }
    }

    public class ModbusItemsSendPackage : List<ModbusItems2SendEntry>
    {
        #region Properties
        private int _DbStartIndex = -1;
        public int DbStartIndex
        {
            get
            {
                return _DbStartIndex;
            }
        }

        private int _DbEndIndex = -1;
        public int DbEndIndex
        {
            get
            {
                return _DbEndIndex;
            }
        }


        public int DbLength
        {
            get
            {
                return DbEndIndex - DbStartIndex + 1;
            }
        }

        private int _DBNo = -10;
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

        public ModbusSubscr ParentSubscription
        {
            get
            {
                if (Count <= 0)
                    return null;
                ModbusItems2SendEntry item1 = this.First();
                return item1.Item.ParentSubscription;
            }
        }

        private byte? _SlaveUnitID;
        public byte SlaveUnitID
        {
            get
            {
                if (!_SlaveUnitID.HasValue)
                    return 0;
                return _SlaveUnitID.Value;
            }
        }
        #endregion

        #region Methods
        private bool IsItemAddable(ModbusItem item)
        {
            if (Count <= 0)
                return true;
            if ((ParentSubscription != item.ParentSubscription)
                || (DBNo != item.ItemDBNo))
                return false;
            return true;
        }

        public new void Add(ModbusItems2SendEntry item)
        {
            if (!IsItemAddable(item.Item))
                return;
            if (!_SlaveUnitID.HasValue && item.Item != null && item.Item.ParentSubscription != null)
                _SlaveUnitID = item.Item.ParentSubscription.ModbusSlaveUnitId;
            base.Add(item);
            if (_DBNo <= -10)
                _DBNo = item.Item.ItemDBNo;
            if (_DbStartIndex <= -1)
            {
                _DbStartIndex = item.Item.ItemStartByteAddr;
                _DbEndIndex = item.Item.ItemEndByteAddr;
                return;
            }

            if (item.Item.ItemStartByteAddr < _DbStartIndex)
                _DbStartIndex = item.Item.ItemStartByteAddr;
            if (item.Item.ItemEndByteAddr > _DbEndIndex)
                _DbEndIndex = item.Item.ItemEndByteAddr;
        }

        public bool ExistsAPreviousEntry(ModbusItem item)
        {
            return (this.Where(c => c.Item == item).Any());
        }

        public bool IsItemADirectNeighbour(ModbusItem item)
        {
            if (Count <= 0)
                return true;
            if (!IsItemAddable(item))
                return false;

            if ((item.ItemEndByteAddr + 1) < DbStartIndex)
                return false;
            else if (((item.ItemEndByteAddr + 1) == DbStartIndex)
                    || (item.ItemStartByteAddr == DbStartIndex)
                    || (item.ItemStartByteAddr <= DbEndIndex)
                    || (item.ItemStartByteAddr == (DbEndIndex + 1)))
                return true;
            return false;
        }

        public IEnumerable<ModbusItemsSendPackageSegment> BuildTransferArray(bool chronologicalValue)
        {
            if (ParentSubscription == null)
                return null;
            ModbusItems2SendEntry firstBooleanItem = null;
            ModbusItems2SendEntry lastBooleanItem = null;
            byte[] transferArray = new byte[DbLength];
            ModbusDataBlock s7DataBlock = ParentSubscription.PLCRAMOfDataBlocks[DBNo];
            s7DataBlock.EnterLockRAMinPLC();
            try
            {
                foreach (ModbusItems2SendEntry sendItemEntry in this)
                {
                    if (sendItemEntry.Item.ItemTableType == modbus.TableType.Output)
                    {
                        if (firstBooleanItem == null)
                        {
                            firstBooleanItem = sendItemEntry;
                            lastBooleanItem = sendItemEntry;
                        }
                        else
                        {
                            if (   (sendItemEntry.Item.ItemStartByteAddr < firstBooleanItem.Item.ItemStartByteAddr)
                                || (sendItemEntry.Item.ItemStartByteAddr == firstBooleanItem.Item.ItemStartByteAddr && sendItemEntry.Item.ItemBitNo < firstBooleanItem.Item.ItemBitNo))
                                firstBooleanItem = sendItemEntry;
                            if ((sendItemEntry.Item.ItemStartByteAddr > firstBooleanItem.Item.ItemStartByteAddr)
                                || (sendItemEntry.Item.ItemStartByteAddr == firstBooleanItem.Item.ItemStartByteAddr && sendItemEntry.Item.ItemBitNo > firstBooleanItem.Item.ItemBitNo))
                                lastBooleanItem = sendItemEntry;
                        }
                    }
                    sendItemEntry.UpdateDataBlockRAM(chronologicalValue, s7DataBlock);
                }
                for (int i = 0; i < DbLength; i++)
                {
                    transferArray[i] = s7DataBlock.RAMinPLC[DbStartIndex + i];
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ModbusItemsSendPackage", "BuildTransferArray", msg);
                return null;
            }
            finally
            {
                s7DataBlock.ExitLockRAMinPLC();
            }

            List<ModbusItemsSendPackageSegment> transferBlocks = new List<ModbusItemsSendPackageSegment>();
            if (transferArray.Length <= ModbusDataBlockReadSegment.PDUMaxDataSize)
            {
                transferBlocks.Add(new ModbusItemsSendPackageSegment(this.DbStartIndex, transferArray, firstBooleanItem, lastBooleanItem));
            }
            else
            {
                int startIndex = this.DbStartIndex;
                int restLength = transferArray.Length;
                //ModbusItemsSendPackageSegment currentSegment = new ModbusItemsSendPackageSegment(startIndex,
                while (restLength > 0)
                {
                    if (restLength > ModbusDataBlockReadSegment.PDUMaxDataSize)
                    {
                        byte[] segmentData = new byte[ModbusDataBlockReadSegment.PDUMaxDataSize];
                        for (int i = 0; i < ModbusDataBlockReadSegment.PDUMaxDataSize; i++)
                        {
                            segmentData[i] = transferArray[i + startIndex - this.DbStartIndex];
                        }
                        transferBlocks.Add(new ModbusItemsSendPackageSegment(startIndex, segmentData, null, lastBooleanItem));
                        restLength -= ModbusDataBlockReadSegment.PDUMaxDataSize;
                        startIndex += ModbusDataBlockReadSegment.PDUMaxDataSize;
                    }
                    else
                    {
                        byte[] segmentData = new byte[restLength];
                        for (int i = 0; i < restLength; i++)
                        {
                            segmentData[i] = transferArray[i + startIndex - this.DbStartIndex];
                        }
                        transferBlocks.Add(new ModbusItemsSendPackageSegment(startIndex, segmentData, firstBooleanItem));
                        startIndex += restLength;
                        restLength = 0;
                        break;
                    }
                }
            }

            return transferBlocks;
        }

        public void MarkItemsAsWritten()
        {
            this.ForEach(c => c.Item.MarkItemAsWritten());
        }
        #endregion
    }
}
