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
    public class ModbusDataBlockItems : List<ModbusItem>
    {
        public void DeInit()
        {
            foreach (ModbusItem s7Item in this)
            {
                s7Item.DeInit();
            }
        }

        public int MaxLength
        {
            get
            {
                if (!this.Any())
                    return 0;
                return this.Max(c => c.ItemLength);
            }
        }

        public short? FirstItemBitNo
        {
            get
            {
                if (!this.Any())
                    return null;
                return this.Min(c => c.ItemBitNo);
                //if (this.Where(c => c.ItemTableType == TableType.Input || c.ItemTableType == TableType.Output).Any())
            }
        }

        public short? LastItemBitNo
        {
            get
            {
                if (!this.Any())
                    return null;
                return this.Max(c => c.ItemBitNo);
                //if (this.Where(c => c.ItemTableType == TableType.Input || c.ItemTableType == TableType.Output).Any())
            }
        }

        public void Reconnected()
        {
            foreach (ModbusItem s7Item in this)
            {
                s7Item.Reconnected();
            }
        }
    }
}
