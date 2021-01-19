using System;

namespace gip.core.dbsyncer.model
{
    public class DBSyncerVersion
    {
        /// <summary>
        /// Version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Time when update is realized
        /// </summary>
        public DateTime UpdateDate { get; set; }
    }
}
