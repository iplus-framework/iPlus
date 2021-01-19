using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class DeserializedSQLDataModel
    {

        public IACEntityObjectContext InnerDatabase { get; set; }
        public IACEntityObjectContext OuterDatabase { get; set; }
        public ACQueryDefinition ACQueryDefinition { get; set; }

        public List<IACObject> DeserializedSQLData { get; set; }

    }
}
