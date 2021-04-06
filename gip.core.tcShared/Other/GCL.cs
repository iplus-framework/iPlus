using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.tcShared
{
    public static class GCL
    {
        public const string cMainIdentifier = "MAIN";

        public const string cRootClass = "VB";
        public const string cRootACIdentifier = "_VB";

        public const string cADSInstancePrefix = "_";
        public const string cADSPathSeparator = ".";
        public const string cADSChildSeparator = cADSPathSeparator + cADSInstancePrefix;

        public const string cPathVB = cMainIdentifier + cADSPathSeparator + cRootACIdentifier;

        public const string cPathMemoryMetaObj = cPathVB + "._Metadata";

        public const string cPathMemory = cPathVB + "._Memory";

        public const string cEventCycleInfo = "._EventCycleInfo";
        public const string cMemReadCycle = "._MemReadCycle";
        public const string cEventsByte = "._EventsByte";
        public const string cEventsUInt = "._EventsUInt";
        public const string cEventsInt = "._EventsInt";
        public const string cEventsUDInt = "._EventsUDInt";
        public const string cEventsDInt = "._EventsDInt";
        public const string cEventsReal = "._EventsReal";
        public const string cEventsLReal = "._EventsLReal";
        public const string cEventsString = "._EventsString";
        public const string cEventsTime = "._EventsTime";
        public const string cEventsDT = "._EventsDT";
        public const string cACMethod = "._ACMethod";
        public const string cMethodParameter = "._Parameter";
        public const string cMethodResult = "._Result";

        public const string cRPC_InvokeSetterBool = "InvokeSetterBool";
        public const string cRPC_InvokeSetterByte = "InvokeSetterByte";
        public const string cRPC_InvokeSetterInt = "InvokeSetterInt";
        public const string cRPC_InvokeSetterDInt = "InvokeSetterDInt";
        public const string cRPC_InvokeSetterUInt = "InvokeSetterUInt";
        public const string cRPC_InvokeSetterUDInt = "InvokeSetterUDInt";
        public const string cRPC_InvokeSetterReal = "InvokeSetterReal";
        public const string cRPC_InvokeSetterLReal = "InvokeSetterLReal";
        public const string cRPC_InvokeSetterEnum = "InvokeSetterEnum";
        public const string cRPC_InvokeSetterTime = "InvokeSetterTime";
        public const string cRPC_InvokeSetterDT = "InvokeSetterDT";


        public const string cADSType_Bool = "BOOL";
        public const string cADSType_String = "STRING";
        public const string cADSType_DateTime = "DATE_AND_TIME";
        public const string cADSType_Byte = "BYTE";
        public const string cADSType_TimeSpan = "TIME";
        public const string cADSType_Short = "INT";
        public const string cADSType_Int = "DINT";
        public const string cADSType_UShort = "UINT";
        public const string cADSType_UInt = "UDINT";
        public const string cADSType_Double = "LREAL";
        public const string cADSType_Float = "REAL";

        public const string cMethodName_FB_init = "FB_init";
        public const string cMethodName_FB_reinit = "FB_reinit";
        public const string cMethodName_FB_exit = "FB_exit";
        public const string cMethodName_FB_postinit = "FB_postinit";
        public const string cMethodName_FB_childinit = "FB_childinit";
        public const string cMethodName_OnMainEnter = "OnMainEnter";
        public const string cMethodName_OnMainExit = "OnMainExit";
        public const string cMethodName_RunMain = "RunMain";
        public const string cMethodName_AddChild = "AddChild";
        public const string cMethodName_AddEdge = "AddEdge";

        public const string cSTCode_Call = "();";
        public const string cSTCode_CallSuper = "SUPER^.";
        public const string cST_Keyword_ST = "ST";
        public const string cST_Keyword_VAR = "VAR";
        public const string cST_Keyword_VAR_INPUT = "VAR_INPUT";
        public const string cST_Keyword_END_VAR = "END_VAR";
        public const string cST_Keyword_PUBLIC = "PUBLIC";
        public const string cST_Keyword_ENDIF = "END_IF";
        public const string cST_Keyword_Extends = "Extends";
        public const string cST_Keyword_Implements = "Implements";
        public const string cST_Keyword_IF = "IF";
        public const string cST_Keyword_THEN = "THEN";

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

        public const string Delimiter_DirSeperator = "_I_";
    }
}
