using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.ISOonTCP;

namespace gip.core.communication
{
    public class S7TCPItemsSendPackageSegment
    {
        public S7TCPItemsSendPackageSegment(int startIndex, byte[] segment)
        {
            _StartIndex = startIndex;
            _WriteSegment = segment;
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
    }

    public class S7TCPItemsSendPackage : List<S7TCPItems2SendEntry>
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

        public S7TCPSubscr ParentSubscription
        {
            get
            {
                if (Count <= 0)
                    return null;
                S7TCPItems2SendEntry item1 = this.First();
                return item1.Item.ParentSubscription;
            }
        }
        #endregion

        #region Methods
        private bool IsItemAddable(S7TCPItem item)
        {
            if (Count <= 0)
                return true;
            if ((ParentSubscription != item.ParentSubscription)
                || (DBNo != item.ItemDBNo))
                return false;
            return true;
        }

        public new void Add(S7TCPItems2SendEntry item)
        {
            if (!IsItemAddable(item.Item))
                return;
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

        public bool ExistsAPreviousEntry(S7TCPItem item)
        {
            return (this.Where(c => c.Item == item).Any());
        }

        public bool IsItemADirectNeighbour(S7TCPItem item)
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

        public IEnumerable<S7TCPItemsSendPackageSegment> BuildTransferArray(bool chronologicalValue)
        {
            if (ParentSubscription == null)
                return null;
            byte[] transferArray = new byte[DbLength];
            S7TCPDataBlock s7DataBlock = ParentSubscription.PLCRAMOfDataBlocks[DBNo];
            s7DataBlock.EnterLockRAMinPLC();
            try
            {
                foreach (S7TCPItems2SendEntry sendItemEntry in this)
                {
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
                    datamodel.Database.Root.Messages.LogException("S7TCPItemsSendPackage", "BuildTransferArray", msg);
                return null;
            }
            finally
            {
                s7DataBlock.ExitLockRAMinPLC();
            }

            List<S7TCPItemsSendPackageSegment> transferBlocks = new List<S7TCPItemsSendPackageSegment>();
            if (transferArray.Length <= PLC.PDUMaxDataSize)
            {
                transferBlocks.Add(new S7TCPItemsSendPackageSegment(this.DbStartIndex, transferArray));
            }
            else
            {
                int startIndex = this.DbStartIndex;
                int restLength = transferArray.Length;
                //S7TCPItemsSendPackageSegment currentSegment = new S7TCPItemsSendPackageSegment(startIndex,
                while (restLength > 0)
                {
                    if (restLength > PLC.PDUMaxDataSize)
                    {
                        byte[] segmentData = new byte[PLC.PDUMaxDataSize];
                        for (int i = 0; i < PLC.PDUMaxDataSize; i++)
                        {
                            segmentData[i] = transferArray[i + startIndex - this.DbStartIndex];
                        }
                        transferBlocks.Add(new S7TCPItemsSendPackageSegment(startIndex, segmentData));
                        restLength -= PLC.PDUMaxDataSize;
                        startIndex += PLC.PDUMaxDataSize;
                    }
                    else
                    {
                        byte[] segmentData = new byte[restLength];
                        for (int i = 0; i < restLength; i++)
                        {
                            segmentData[i] = transferArray[i + startIndex - this.DbStartIndex];
                        }
                        transferBlocks.Add(new S7TCPItemsSendPackageSegment(startIndex, segmentData));
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
