using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Configuration transfer
    /// </summary>
    public class ACConfigTransferCommand
    {

        #region ctor's

        public ACConfigTransferCommand()
        {

        }

        #endregion

        #region Internal properties


        private IACConfigStore sourceConfigStore;
        private IACConfigStore targetConfigStore;

        public IACConfigStore SourceConfigStore
        {
            get
            {
                return sourceConfigStore;
            }
            set
            {
                sourceConfigStore = value;
            }
        }
        public IACConfigStore TargetConfigStore
        {
            get
            {
                return targetConfigStore;
            }
            set
            {
                targetConfigStore = value;
            }
        }

        public List<ACClassMethod> SourceACClassMethods { get; set; }
        public List<ACClassMethod> TargetACClassMethods { get; set; }

        public List<ACConfigTransferMethodModel> MethodSelection { get; set; }

        public List<ACConfigTransferTaskModel> TransferTask
        {
            get;
            private set;
        }

        #endregion

        #region TransferSelection

        public void Search()
        {
            List<ACClassMethod> intersectMethods = SourceACClassMethods.Where(x => TargetACClassMethods.Select(p => p.ACClassMethodID).Contains(x.ACClassMethodID)).ToList();
            if (intersectMethods != null && intersectMethods.Any())
            {
                MethodSelection = intersectMethods.Select(x => new ACConfigTransferMethodModel() { Method = x, Selected = true }).ToList();
                TransferTask =
                    SourceConfigStore
                    .ConfigurationEntries
                    // .Where(cnf => MethodSelection.Where(ts => ts.Value).Any(mth => mth.Key.UrlBelongsTo(cnf.LocalConfigACUrl)))
                    .Select(x =>
                        new ACConfigTransferTaskModel()
                        {
                            Selected = true,
                            PreConfigACUrl = x.PreConfigACUrl,
                            LocalConfigACUrl = x.LocalConfigACUrl,
                            ExistOnTarget =
                TargetConfigStore.ConfigurationEntries.Any(y =>
                        (
                            y.PreConfigACUrl == null || ((y.PreConfigACUrl ?? "") == (x.PreConfigACUrl ?? ""))
                        )
                        && y.LocalConfigACUrl == x.LocalConfigACUrl)
                        }).ToList();

                foreach (var item in TransferTask)
                {
                    ACClassMethod mth = MethodSelection.Where(ts => ts.Selected && ts.Method.UrlBelongsTo(item.LocalConfigACUrl)).Select(x => x.Method).FirstOrDefault();
                    if (mth != null)
                    {
                        item.ACClassMethodID = mth.ACClassMethodID;
                    }
                }
                //TransferTask.RemoveAll(x => x.ACClassMethodID == null);
            }
        }


        public void Process()
        {
            List<ACConfigTransferTaskModel> selected = TransferTask.Where(x => x.Selected).ToList();
            foreach (ACConfigTransferTaskModel transferTask in selected)
            {
                IACConfig newConfig = TargetConfigStore.ConfigurationEntries.Where(x =>
                    ((x.PreConfigACUrl ?? "") == (transferTask.PreConfigACUrl ?? "")) &&
                    x.LocalConfigACUrl == transferTask.LocalConfigACUrl
                 ).FirstOrDefault();
                if (newConfig == null)
                {
                    newConfig = TargetConfigStore.NewACConfig();
                    //TargetConfigStore.ConfigurationEntries.Add(newConfig);
                }

                IACConfig oldConfig = SourceConfigStore.ConfigurationEntries.Where(x =>
                    ((x.PreConfigACUrl ?? "") == (transferTask.PreConfigACUrl ?? "")) &&
                    x.LocalConfigACUrl == transferTask.LocalConfigACUrl
                 ).FirstOrDefault();
                newConfig.PreConfigACUrl = oldConfig.PreConfigACUrl;
                newConfig.LocalConfigACUrl = oldConfig.LocalConfigACUrl;
                newConfig.SetValueTypeACClass(oldConfig.ValueTypeACClass);
                newConfig.Comment = oldConfig.Comment;
                newConfig.Expression = oldConfig.Expression;
                //newConfig.Value = oldConfig.Value; // doing with next line - this use same object as source - changes in soruce makes changes in target
                newConfig.XMLConfig = oldConfig.XMLConfig;
            }
        }
        #endregion

    }
}
