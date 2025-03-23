using System;
using System.Data;

namespace gip.bso.iplus
{
    public class SQLScriptResult
    {
        public bool Success {  get; set; }  
        public DataTable DataTable { get; set; }    
        public TimeSpan Duration { get; set; }
        public Exception Exception { get; set; }
    }
}
