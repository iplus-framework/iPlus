namespace DBSyncerUpdate.unit.test.DBSyncerOldModel
{
    /// <summary>
    /// 
    /// </summary>
    public class DbSyncerInfoContext
    {
        public string DbSyncerInfoContextID { get; set; }
        public string Name {get;set;}
        public string ConnectionName {get;set;}
        public int Order { get; set; }

        public override string ToString()
        {
            return DbSyncerInfoContextID;
        }
    }
}
