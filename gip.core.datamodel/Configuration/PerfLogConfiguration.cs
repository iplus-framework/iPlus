using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    /// Entity-Framework-Configuration
    /// </summary>
    public class PerfLogConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("Active", DefaultValue = false, IsRequired = true)]
        public bool Active
        {
            get
            {
                return (bool)this["Active"];
            }
            set
            {
                this["Active"] = value;
            }
        }

        [ConfigurationProperty("MaxPerfEntries", DefaultValue = (short)20, IsRequired = false)]
        public short MaxPerfEntries
        {
            get
            {
                return (short)this["MaxPerfEntries"];
            }
            set
            {
                this["MaxPerfEntries"] = value;
            }
        }

        [ConfigurationProperty("LogACState", DefaultValue = false, IsRequired = false)]
        public bool LogACState
        {
            get
            {
                return (bool)this["LogACState"];
            }
            set
            {
                this["LogACState"] = value;
            }
        }
    }
}
