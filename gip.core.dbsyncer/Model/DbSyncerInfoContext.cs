namespace gip.core.dbsyncer.model
{
    /// <summary>
    /// Mapped table DbSyncerInfoContext
    /// </summary>
    public class DbSyncerInfoContext
    {

        /// <summary>
        /// ID and primary key
        /// short unique name for context to
        /// </summary>
        public string DbSyncerInfoContextID { get; set; }

        /// <summary>
        /// Context (long) name
        /// </summary>
        public string Name {get;set;}

        /// <summary>
        /// Name of EF connection in .config file used for context
        /// </summary>
        public string ConnectionName {get;set;}

        /// <summary>
        /// Priority order of context
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Overriding ToString for object to provide relevant debug informations
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DbSyncerInfoContextID;
        }
    }
}
