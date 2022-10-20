using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{

    public delegate void TransferComplete(IACConfigStore targetConfigStore);
    /// <summary>
    /// BSO for add elements
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBBSOConfigTransfer'}de{'VBBSOConfigTransfer'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, true)]
    public class VBBSOConfigTransfer : ACBSO
    {
        #region c´tors


        public VBBSOConfigTransfer(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _VarioConfigManager = ConfigManagerIPlus.ACRefToServiceInstance(this);
            if (_VarioConfigManager == null)
                throw new Exception("ConfigManagerIPlus not found!");
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            WindowOpened = false;
            if (_VarioConfigManager != null)
                ConfigManagerIPlus.DetachACRefFromServiceInstance(this, _VarioConfigManager);
            _VarioConfigManager = null;
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region events

        public event TransferComplete OnTransferComplete;

        #endregion

        #region Managers

        protected ACRef<ConfigManagerIPlus> _VarioConfigManager = null;
        public ConfigManagerIPlus VarioConfigManager
        {
            get
            {
                if (_VarioConfigManager == null)
                    return null;
                return _VarioConfigManager.ValueT;
            }
        }

        #endregion

        #region properties
        public bool WindowOpened { get; set; }

        #endregion

        #region Config Transfer

        private ACConfigTransferCommand _ConfigTransferCommand;
        public ACConfigTransferCommand ConfigTransferCommand
        {
            get
            {
                if (_ConfigTransferCommand == null)
                    _ConfigTransferCommand = new ACConfigTransferCommand();
                return _ConfigTransferCommand;
            }
        }
        #endregion

        #region Stores properties caption

        [ACPropertyInfo(1, "ConfigStoreSourceACIdentifier", "en{'ConfigStoreSourceACIdentifier'}de{'ConfigStoreSourceACIdentifier'}")]
        public string ConfigStoreSourceACIdentifier
        {
            get
            {
                if (ConfigTransferCommand == null || ConfigTransferCommand.SourceConfigStore == null) return null;
                return ConfigTransferCommand.SourceConfigStore.ACIdentifier;
            }
        }

        [ACPropertyInfo(2, "ConfigStoreTargetACIdentifier", "en{'ConfigStoreTargetACIdentifier'}de{'ConfigStoreTargetACIdentifier'}")]
        public string ConfigStoreTargetACIdentifier
        {
            get
            {
                if (ConfigTransferCommand == null || ConfigTransferCommand.TargetConfigStore == null) return null;
                return ConfigTransferCommand.TargetConfigStore.ACIdentifier;
            }
        }


        public void RefreshStorePreview()
        {
            OnPropertyChanged("ConfigStoreSourceACIdentifier");
            OnPropertyChanged("ConfigStoreTargetACIdentifier");
        }

        public void RefreshLists()
        {
            OnPropertyChanged("TransferMethodList");
            OnPropertyChanged("TransferTaskList");
        }
        #endregion

        #region Config Transfer Selection

        #region TransferMethod
        private ACConfigTransferMethodModel _SelectedTransferMethod;
        /// <summary>
        /// Selected property for Tuple&lt;ACClassMethod, bool&gt;
        /// </summary>
        /// <value>The selected TransferMethod</value>
        [ACPropertySelected(9999, "TransferMethod", "en{'TODO: TransferMethod'}de{'TODO: TransferMethod'}")]
        public ACConfigTransferMethodModel SelectedTransferMethod
        {
            get
            {
                return _SelectedTransferMethod;
            }
            set
            {
                if (_SelectedTransferMethod != value)
                {
                    _SelectedTransferMethod = value;
                    OnPropertyChanged("SelectedTransferMethod");
                    //OnPropertyChanged("TransferTaskList");
                }
            }
        }


        private List<ACConfigTransferMethodModel> _TransferMethodList;
        /// <summary>
        /// List property for Tuple&lt;ACClassMethod, bool&gt;
        /// </summary>
        /// <value>The TransferMethod list</value>
        [ACPropertyList(9999, "TransferMethod")]
        public List<ACConfigTransferMethodModel> TransferMethodList
        {
            get
            {
                if (_TransferMethodList == null)
                {
                    _TransferMethodList = LoadTransferMethodList();
                    if (_TransferMethodList != null)
                        SelectedTransferMethod = _TransferMethodList.FirstOrDefault();
                    else
                        SelectedTransferMethod = null;
                }
                return _TransferMethodList;
            }
        }

        private List<ACConfigTransferMethodModel> LoadTransferMethodList()
        {
            if (ConfigTransferCommand == null || ConfigTransferCommand.MethodSelection == null) return null;
            return ConfigTransferCommand.MethodSelection;
        }
        #endregion

        #region TransferTask
        private ACConfigTransferTaskModel _SelectedTransferTask;
        /// <summary>
        /// Selected property for ACClassConfigTransferTask
        /// </summary>
        /// <value>The selected TransferTask</value>
        [ACPropertySelected(9999, "TransferTask", "en{'TODO: TransferTask'}de{'TODO: TransferTask'}")]
        public ACConfigTransferTaskModel SelectedTransferTask
        {
            get
            {
                return _SelectedTransferTask;
            }
            set
            {
                if (_SelectedTransferTask != value)
                {
                    _SelectedTransferTask = value;
                    OnPropertyChanged("SelectedTransferTask");
                }
            }
        }


        //private List<ACConfigTransferTaskModel> _TransferTaskList;
        /// <summary>
        /// List property for ACClassConfigTransferTask
        /// </summary>
        /// <value>The TransferTask list</value>
        [ACPropertyList(9999, "TransferTask")]
        public List<ACConfigTransferTaskModel> TransferTaskList
        {
            get
            {
                if (ConfigTransferCommand.TransferTask == null) return null;
                return ConfigTransferCommand.TransferTask.ToList();
            }
        }

        #endregion

        #endregion

        #region Methods

        [ACMethodInfo("BatchCreateAutomaticallyCreateDlgOk", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Okay)]
        public void Cancel()
        {
            CloseWindow(this, "Mainlayout");
        }

        [ACMethodInfo("BatchCreateAutomaticallyCreateDlgOk", "en{'Copy'}de{'Kopieren'}", (short)MISort.Okay)]
        public void Process()
        {
            if (IsEnabledProcess())
            {
                ConfigTransferCommand.Process();
            }
            if (OnTransferComplete != null)
                OnTransferComplete(ConfigTransferCommand.TargetConfigStore);
            CloseWindow(this, "Mainlayout");
        }

        private bool IsEnabledProcess()
        {
            return ConfigTransferCommand != null && ConfigTransferCommand.MethodSelection != null && ConfigTransferCommand.MethodSelection.Any(x => x.Selected)
                && ConfigTransferCommand.TransferTask != null && ConfigTransferCommand.TransferTask.Any(x => x.Selected);
        }


        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Cancel":
                    Cancel();
                    return true;
                case "Process":
                    Process();
                    return true;
                case Const.IsEnabledPrefix + "Process":
                    result = IsEnabledProcess();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #endregion


        
    }
}
