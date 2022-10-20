using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class ACObjectSerialPropertyContainerModel
    {
        public ACObjectSerialPropertyHandlingTypesEnum PropertyType { get; set; }
        public List<PropertyInfo> Properties { get; set; }
    }
}
