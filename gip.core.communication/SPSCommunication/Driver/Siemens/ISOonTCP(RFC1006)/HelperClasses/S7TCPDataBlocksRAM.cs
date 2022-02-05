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
    public class S7TCPDataBlocksRAM : SortedDictionary<Int32, S7TCPDataBlock>, INotifyPropertyChanged
    {
        #region Properties
        public IEnumerable<S7TCPDataBlock> DataBlocks
        {
            get
            {
                return this.Select(c => c.Value);
            }
        }

        public string ReadErrorMessage
        {
            get
            {
                string _ReadErrorMessage = "";
                foreach (S7TCPDataBlock datablock in DataBlocks)
                {
                    if (!String.IsNullOrEmpty(datablock.ReadErrorMessage))
                        _ReadErrorMessage += datablock.ReadErrorMessage + "\n";
                }
                return _ReadErrorMessage;
            }
        }

        public string WriteErrorMessage
        {
            get
            {
                string _WriteErrorMessage = "";
                foreach (S7TCPDataBlock datablock in DataBlocks)
                {
                    if (!String.IsNullOrEmpty(datablock.WriteErrorMessage))
                        _WriteErrorMessage += datablock.WriteErrorMessage + "\n";
                }
                return _WriteErrorMessage;
            }
        }

        #endregion

        #region Methods
        public void Add(S7TCPItem s7Item)
        {
            S7TCPDataBlock DataBlockItemsInfo;
            if (!TryGetValue(s7Item.ItemDBNo, out DataBlockItemsInfo))
            {
                DataBlockItemsInfo = new S7TCPDataBlock(s7Item.ItemDBNo);
                DataBlockItemsInfo.PropertyChanged += S7TCPDataBlock_PropertyChanged;
                Add(s7Item.ItemDBNo, DataBlockItemsInfo);
            }

            DataBlockItemsInfo.Add(s7Item);
        }

        public void DeInit()
        {
            foreach (S7TCPDataBlock dataBlock in DataBlocks)
            {
                dataBlock.PropertyChanged -= S7TCPDataBlock_PropertyChanged;
                dataBlock.DeInit();
            }
        }

        public bool AreItemsNeighboursDirect(S7TCPItem item1, S7TCPItem item2)
        {
            if ((item1.ParentSubscription != item2.ParentSubscription)
                || (item1.ItemDBNo != item2.ItemDBNo))
                return false;
            return this[item1.ItemDBNo].AreItemsNeighboursDirect(item1, item2);
        }

        public bool AreItemsNeighboursInSegment(S7TCPItem item1, S7TCPItem item2)
        {
            if ((item1.ParentSubscription != item2.ParentSubscription)
                || (item1.ItemDBNo != item2.ItemDBNo))
                return false;
            return this[item1.ItemDBNo].AreItemsNeighboursInSegment(item1, item2);
        }
        #endregion

        #region Events
        void S7TCPDataBlock_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "ReadErrorMessage") || (e.PropertyName == "WriteErrorMessage"))
                OnPropertyChanged(e.PropertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}
