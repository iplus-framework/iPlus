using gip.core.datamodel;
using System.Collections.Generic;

namespace gip.core.autocomponent
{
    public class ValidateConfigStoreModel
    {
        public bool IsValid { get; set; }

        public List<IACConfigStore> NotValidConfigStores{get;set;}
    }
}
