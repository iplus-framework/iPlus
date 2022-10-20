using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ACChildInfo : Attribute
    {
        /// <summary>
        /// </summary>
        public ACChildInfo(string instanceName, Type type)
        {
            InstanceName = instanceName;
            Type = type;
        }

        #region Properties
        public string InstanceName { get; set; }
        public Type Type { get; set; }
        #endregion
    }
}
