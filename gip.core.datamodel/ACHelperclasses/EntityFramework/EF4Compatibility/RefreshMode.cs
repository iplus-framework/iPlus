using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
        public enum RefreshMode
        {
            //
            // Summary:
            //     Property changes made to objects in the object context are not replaced with
            //     values from the data source. On the next call to System.Data.Objects.ObjectContext.SaveChanges,
            //     these changes are sent to the data source.
            ClientWins = 2,
            //
            // Summary:
            //     Property changes made to objects in the object context are replaced with values
            //     from the data source.
            StoreWins = 1
        }
}
