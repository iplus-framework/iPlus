using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.modbus;

namespace gip.core.communication
{
    public class ModbusDataBlocksRAM : SortedDictionary<Int32, ModbusDataBlock>, INotifyPropertyChanged
    {
        #region Properties
        public IEnumerable<ModbusDataBlock> DataBlocks
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
                foreach (ModbusDataBlock datablock in DataBlocks)
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
                foreach (ModbusDataBlock datablock in DataBlocks)
                {
                    if (!String.IsNullOrEmpty(datablock.WriteErrorMessage))
                        _WriteErrorMessage += datablock.WriteErrorMessage + "\n";
                }
                return _WriteErrorMessage;
            }
        }

        #endregion

        #region Methods
        public void Add(ModbusItem s7Item)
        {
            ModbusDataBlock DataBlockItemsInfo;
            if (!TryGetValue(s7Item.ItemDBNo, out DataBlockItemsInfo))
            {
                DataBlockItemsInfo = new ModbusDataBlock(s7Item.ItemDBNo);
                DataBlockItemsInfo.PropertyChanged += ModbusDataBlock_PropertyChanged;
                Add(s7Item.ItemDBNo, DataBlockItemsInfo);
            }

            DataBlockItemsInfo.Add(s7Item);
        }

        public void DeInit()
        {
            foreach (ModbusDataBlock dataBlock in DataBlocks)
            {
                dataBlock.PropertyChanged -= ModbusDataBlock_PropertyChanged;
                dataBlock.DeInit();
            }
        }

        public bool AreItemsNeighboursDirect(ModbusItem item1, ModbusItem item2)
        {
            if ((item1.ParentSubscription != item2.ParentSubscription)
                || (item1.ItemDBNo != item2.ItemDBNo))
                return false;
            return this[item1.ItemDBNo].AreItemsNeighboursDirect(item1, item2);
        }

        public bool AreItemsNeighboursInSegment(ModbusItem item1, ModbusItem item2)
        {
            if ((item1.ParentSubscription != item2.ParentSubscription)
                || (item1.ItemDBNo != item2.ItemDBNo))
                return false;
            return this[item1.ItemDBNo].AreItemsNeighboursInSegment(item1, item2);
        }
        #endregion

        #region Events
        void ModbusDataBlock_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
