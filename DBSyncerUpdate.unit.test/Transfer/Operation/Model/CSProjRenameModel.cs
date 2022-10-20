namespace DBSyncerUpdate.unit.test.Transfer.Operation.Model
{
    /// <summary>
    /// Holder for processed .csproj file content / per database context
    /// </summary>
    public class CSProjRenameModel
    {
        public string OldContent { get; set; }
        public string NewContent { get; set; }
    }
}
