using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public class CoreConfiguration : ConfigurationSection
    {
        /// <summary>
        /// If DefaultPrecision is negative, then the DefaultValue of 6 digits will be used
        /// </summary>
        [ConfigurationProperty("DefaultPrecision", DefaultValue = (short)6, IsRequired = false)]
        public short DefaultPrecision
        {
            get
            {
                return (short)this["DefaultPrecision"];
            }
            set
            {
                this["DefaultPrecision"] = value;
            }
        }


        [ConfigurationProperty("UseSimpleMonitor", DefaultValue = true, IsRequired = false)]
        public bool UseSimpleMonitor
        {
            get
            {
                return (bool)this["UseSimpleMonitor"];
            }
            set
            {
                this["UseSimpleMonitor"] = value;
            }
        }


        [ConfigurationProperty("ValidateLockHierarchies", DefaultValue = true, IsRequired = false)]
        public bool ValidateLockHierarchies
        {
            get
            {
                return (bool)this["ValidateLockHierarchies"];
            }
            set
            {
                this["ValidateLockHierarchies"] = value;
            }
        }


        [ConfigurationProperty("PoolWFNodes", DefaultValue = true, IsRequired = false)]
        public bool PoolWFNodes
        {
            get
            {
                return (bool)this["PoolWFNodes"];
            }
            set
            {
                this["PoolWFNodes"] = value;
            }
        }

        [ConfigurationProperty("UseWeekRefForPooling", DefaultValue = false, IsRequired = false)]
        public bool UseWeekRefForPooling
        {
            get
            {
                return (bool)this["UseWeekRefForPooling"];
            }
            set
            {
                this["UseWeekRefForPooling"] = value;
            }
        }

        [ConfigurationProperty("ConsistencyCheckWF", DefaultValue = true, IsRequired = false)]
        public bool ConsistencyCheckWF
        {
            get
            {
                return (bool)this["ConsistencyCheckWF"];
            }
            set
            {
                this["ConsistencyCheckWF"] = value;
            }
        }

        [ConfigurationProperty("RoutingDefaultAlternatives", DefaultValue = 3, IsRequired = false)]
        public int RoutingDefaultAlternatives
        {
            get
            {
                return (int)this[nameof(RoutingDefaultAlternatives)];
            }
            set
            {
                this[nameof(RoutingDefaultAlternatives)] = value;
            }
        }
    }
}
