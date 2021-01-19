using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.tcShared
{
    public static class GCL
    {
        public const string cPathVB = "MAIN._VB";

        public const string cPathMemoryMetaObj = cPathVB + "._Metadata";

        public const string cPathMemory = cPathVB + "._Memory";

        public const short cACUrlLEN = 200;

        // Maximum Properties for Metadata
        public const short cMetaPropMAX = 100;

        // Maximum Instances for Metadata
        public const int cMetaObjMAX = 10000;

        // Maximum reserved Memory for one ACComponent
        public const short cCompMemSizeMAX = 1000;

        // Min PropertyChanges 
        public const short cMemoryEventsMinMAX = 100;

        // Mid PropertyChanges 
        public const short cMemoryEventsMidMAX = 1000;

        // MaxPropertyChanges 
        public const short cMemoryEventsMAX = 2000;

        // Maximum reserved Memory
        public const int cMemorySizeMAX = 5000000;

        public const uint cMemoryEventCycleMAX = 4000000000;
    }
}
