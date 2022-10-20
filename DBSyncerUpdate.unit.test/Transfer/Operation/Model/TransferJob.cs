using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DBSyncerUpdate.unit.test.Transfer.Operation.Model
{
    [DataObject]
    [Serializable]
    public class TransferJob
    {

        #region Settings Fields


        /// <summary>
        /// Build json TransferJob file on beginning
        /// VPN and SVN connection requiered
        /// </summary>
        [DataObjectField(false)]
        public bool IsBuildTransferJobJson { get; set; }

        /// <summary>
        /// If true changes are executed
        /// </summary>
        [DataObjectField(false)]
        public bool IsDoJob { get; set; }

        /// <summary>
        /// Is changes applied on visual studio solution 
        /// or when is false is case of Variobatch installation
        /// </summary>
        [DataObjectField(false)]
        public bool IsUpdateProjectFile { get; set; }

        [DataObjectField(false)]
        public bool IsClientUpdate { get; set; }

        #endregion

        public List<OperationContext> OperationContexts { get; set; }


        #region Calculated properties - used in process

        [JsonIgnore]
        public string SQLToExecute { get; set; }

        [JsonIgnore]
        public Dictionary<string, List<Exception>> Exceptions { get; set; }

        #endregion


        #region Methods

        public void RegisterException(string operationName, Exception exception)
        {
            if (Exceptions == null)
                Exceptions = new Dictionary<string, List<Exception>>();
            if (!Exceptions.ContainsKey(operationName))
                Exceptions.Add(operationName, new List<Exception>());
            Exceptions[operationName].Add(exception);
        }

        #endregion

        #region Override basics

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion

    }
}
