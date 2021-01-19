using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DBSyncerUpdate.unit.test.Transfer.Operation.Model
{
    /// <summary>
    /// Represents database context and all related content
    /// </summary>
    public class OperationContext
    {

        public OperationContext(TransferJob job)
        {
            TransferJob = job;
        }


        #region OperationContext -> Input json data

        public int Order { get; set; }
        public string DataContextID { get; set; }

        [DataObjectField(false)]
        public string ContextProjectFile { get; set; }

        [DataObjectField(false)]
        public List<string> ContextFolders { get; set; }

        [DataObjectField(false)]
        public List<OperationFile> OperationFiles { get; set; }

        #endregion

        #region Operation properties - excluded from json serialization

        [JsonIgnore]
        public TransferJob TransferJob { get; set; }

        [JsonIgnore]
        public CSProjRenameModel CSProjRenameModel { get; set; }

        #endregion


        #region Overrides

        /// <summary>
        /// To string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            int minOperationFileVersion = 0;
            int maxOperationFileVersion = 0;
            if(OperationFiles != null && OperationFiles.Any())
            {
                minOperationFileVersion = OperationFiles.Min(c => c.Version);
                maxOperationFileVersion = OperationFiles.Max(c => c.Version);
            }
            return string.Format(@"DataContextID: {0}| Order:{1}| files:{2}-{3}", DataContextID, Order, minOperationFileVersion.ToString("0000"), maxOperationFileVersion.ToString("0000"));
        }

        #endregion
    }
}
