using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.ControlScriptSync
{
    /// <summary>
    /// Database storage model - store information about control script update
    /// </summary>
    public class ControlScriptSyncInfo
    {
        public int ControlScriptSyncInfoID { get; set; }
        /// <summary>
        /// Version defintion - time stamp of update  creation
        /// </summary>
        public DateTime VersionTime {get;set;}
        /// <summary>
        /// Time when update is applyed on specific location (installation)
        /// </summary>
        public DateTime UpdateTime {get;set;}
        /// <summary>
        /// Update Autor - when is specified in update zip file name 
        /// </summary>
        public string UpdateAuthor { get; set; }
    }
}
