using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace gip.tool.installerAndUpdater
{
    public partial class MainWindow
    {
        private void InitUpdate()
        {
            CheckIfVBRunning();

            List<Tuple<RollbackType, string>> tempRollbackOptions = new List<Tuple<RollbackType, string>>();
            tempRollbackOptions.Add(new Tuple<RollbackType, string>(RollbackType.Disabled, TextResources.TextResource.ListRollbackOptionDisabled));
            tempRollbackOptions.Add(new Tuple<RollbackType, string>(RollbackType.EnabledManual, TextResources.TextResource.ListRollbackOptionEnabledManual));
            tempRollbackOptions.Add(new Tuple<RollbackType, string>(RollbackType.EnabledAutomatic, TextResources.TextResource.ListRollbackOptionEnabledAutomatic));
            RollbackOptionsList = tempRollbackOptions;
            SelectedRollbackOption = RollbackOptionsList[1];
            _closeState = CloseState.WithoutMessage;
        }

        #region Properties

        private string _UpdateiPlusVersion = "";
        private double _helpUpdateProgress = 0;

        private bool _IsNewUpdateAvailable;
        public bool IsNewUpdateAvailable
        {
            get
            {
                return _IsNewUpdateAvailable;
            }
            set
            {
                _IsNewUpdateAvailable = value;
                OnPropertyChanged("IsNewUpdateAvailable");
            }
        }

        private bool _IsUpdatePBIndeterminate = true;
        public bool IsUpdatePBIndeterminate
        {
            get
            {
                return _IsUpdatePBIndeterminate;
            }
            set
            {
                _IsUpdatePBIndeterminate = value;
                OnPropertyChanged("IsUpdatePBIndeterminate");
            }
        }

        private bool _IsCheckForUpdateFinished = false;
        public bool IsCheckForUpdateFinished
        {
            get
            {
                return _IsCheckForUpdateFinished;
            }
            set
            {
                _IsCheckForUpdateFinished = value;
                OnPropertyChanged("IsCheckForUpdateFinished");
            }
        }

        private string _ChangeLogMessage;
        public string ChangeLogMessage
        {
            get
            {
                return _ChangeLogMessage;
            }
            set
            {
                _ChangeLogMessage = value;
                OnPropertyChanged("ChangeLogMessage");
            }
        }

        private string _CheckForUpdateStatus = TextResources.TextResource.CheckForUpdateStatus1;
        public string CheckForUpdateStatus
        {
            get
            {
                return _CheckForUpdateStatus;
            }
            set
            {
                _CheckForUpdateStatus = value;
                OnPropertyChanged("CheckForUpdateStatus");
            }
        }

        private double _CurrentUpdateOperationProgress;
        public double CurrentUpdateOperationProgress
        {
            get 
            {
                return _CurrentUpdateOperationProgress; 
            }
            set 
            {
                _CurrentUpdateOperationProgress = value;
                OnPropertyChanged("CurrentUpdateOperationProgress"); 
            }
        }

        private int _CurrentUpdateOperation;
        public int CurrentUpdateOperation
        {
            get
            {
                return _CurrentUpdateOperation;
            }
            set
            {
                if (_CurrentUpdateOperation == 10)
                    return;
                _CurrentUpdateOperation = value;
                switch(_CurrentUpdateOperation)
                {
                    case 0:
                        CurrentUpdateOperationText = TextResources.TextResource.OperationPrepareUpdate;
                        break;
                    case 1:
                        CurrentUpdateOperationText = TextResources.TextResource.OperationDownloadUpdate;
                        break;
                    case 2:
                        CurrentUpdateOperationText = TextResources.TextResource.OperationUnpackUpdate;
                        break;
                    case 3:
                        CurrentUpdateOperationText = TextResources.TextResource.OperationCompleteUpdate;
                        break;
                    case 4:
                        CurrentUpdateOperationText = TextResources.TextResource.OperationFinishUpdate;
                        break;
                    case 9:
                        CurrentUpdateOperationText = "Canceling update...";
                        TotalUpdateProgress = 0;
                        CurrentUpdateOperationProgress = 0;
                        break;
                    case 10:
                        CurrentUpdateOperationText = "Update fail.";
                        TotalUpdateProgress = 0;
                        CurrentUpdateOperationProgress = 0;
                        break;
                }
                OnPropertyChanged("CurrentUpdateOperation");
            }
        }

        public string _CurrentUpdateOperationText;
        public string CurrentUpdateOperationText
        {
            get
            {
                return _CurrentUpdateOperationText;
            }
            set
            {
                _CurrentUpdateOperationText = value;
                OnPropertyChanged("CurrentUpdateOperationText");
            }
        }

        private double _TotalUpdateProgress;
        public double TotalUpdateProgress
        {
            get
            {
                return _TotalUpdateProgress;
            }
            set
            {
                _TotalUpdateProgress = value;
                OnPropertyChanged("TotalUpdateProgress");
            }
        }

        private Tuple<RollbackType,string> _SelectedRollbackOption;
        public Tuple<RollbackType, string> SelectedRollbackOption
        {
            get
            {
                return _SelectedRollbackOption;
            }
            set
            {
                _SelectedRollbackOption = value;
                switch(_SelectedRollbackOption.Item1)
                {
                    case installerAndUpdater.RollbackType.Disabled:
                        RollbackOptionDescription = TextResources.TextResource.RollbackOptionDescriptionDisabled;
                        UpdateRollbackInfo = TextResources.TextResource.TbUpdateRollbackInfo;
                        break;
                    case installerAndUpdater.RollbackType.EnabledManual:
                        RollbackOptionDescription = TextResources.TextResource.RollbackOptionDescriptionEnabledManual;
                        UpdateRollbackInfo = TextResources.TextResource.TbUpdateRollbackInfo;
                        break;
                    case installerAndUpdater.RollbackType.EnabledAutomatic:
                        RollbackOptionDescription = TextResources.TextResource.RollbackOptionDescriptionEnabledAutomatic;
                        UpdateRollbackInfo = TextResources.TextResource.TbUpdateRollbackInfoAuto;
                        if(RollbackConnectionInfo == null)
                            RollbackConnectionInfo = InstallerAndUpdaterManager.TryGetDBLoginFromAppConfig(InstallationFolder, _UpdateiPlusVersion);
                        if (RollbackConnectionInfo.CheckServerLocationAndBackupPath())
                            RollbackBackupDBFilePath = string.Format("{0}\\{1}.bak", RollbackConnectionInfo.ServerBackupPath, RollbackConnectionInfo.DatabaseName);
                        else
                            mainTabControlMaint.SelectedItem = TabItemUpdateRollbackDB;
                        break;
                }
                OnPropertyChanged("SelectedRollbackOption");
            }
        }

        private List<Tuple<RollbackType, string>> _RollbackOptionsList;
        public List<Tuple<RollbackType, string>> RollbackOptionsList
        {
            get
            {
                return _RollbackOptionsList;
            }
            set
            {
                _RollbackOptionsList = value;
                OnPropertyChanged("RollbackOptionsList");
            }
        }

        private string _RollbackOptionDescription;
        public string RollbackOptionDescription
        {
            get
            {
                return _RollbackOptionDescription;
            }
            set
            {
                _RollbackOptionDescription = value;
                OnPropertyChanged("RollbackOptionDescription");
            }
        }

        private SqlConnectionInfo _RollbackConnectionInfo;
        public SqlConnectionInfo RollbackConnectionInfo
        {
            get
            {
                return _RollbackConnectionInfo;
            }
            set
            {
                _RollbackConnectionInfo = value;
                OnPropertyChanged("RollbackConnectionInfo");
            }
        }

        private bool _IsRollbackOptionNextEnabled = true;
        public bool IsRollbackOptionNextEnabled
        {
            get
            {
                return _IsRollbackOptionNextEnabled;
            }
            set
            {
                _IsRollbackOptionNextEnabled = value;
                OnPropertyChanged("IsRollbackOptionNextEnabled");
            }
        }

        private string _UpdateRollbackInfo;
        public string UpdateRollbackInfo
        {
            get
            {
                return _UpdateRollbackInfo;
            }
            set
            {
                _UpdateRollbackInfo = value;
                OnPropertyChanged("UpdateRollbackInfo");
            }
        }

        private CancellationTokenSource _UpdateCancelTokenSource;
        internal ManualResetEvent _resetEvent;

        #endregion

        #region Methods

        private void CheckIfVBRunning()
        {
            string processName = string.Format("gip.{0}.client", _UpdateiPlusVersion).ToLower();
            var process = Process.GetProcessesByName(processName);
            if (process.Any())
            {
                //MsgBox.Show(string.Format("{0} is running, please close application before start updater.", _UpdateiPlusVersion), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                MsgBox.Show(string.Format(TextResources.TextResource.TbUpdateApplicationStillRunning, _UpdateiPlusVersion), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _closeState = CloseState.WithoutMessageFinish;
                Close();
            }
        }

        private void StartUpdateOperation()
        {
            string userInfo = InstallerAndUpdaterManager.RestoreCredentials(InstallationFolder);
            if (string.IsNullOrEmpty(userInfo))
            {
                mainTabControlMaint.SelectedItem = TabItemUpdateLogin;
                return;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(userInfo);
                XmlNode root = doc.SelectSingleNode("xml");
                string username = root.SelectSingleNode("user").InnerText;
                string password = root.SelectSingleNode("pass").InnerText;

                if (InstallerAndUpdaterManager.Login(username, password))
                {
                    mainTabControlMaint.SelectedItem = TabItemUpdate;
                    InstallerAndUpdaterManager.CheckForUpdate(_UpdateiPlusVersion, InstallationFolder);
                }
                else
                    mainTabControlMaint.SelectedItem = TabItemUpdateLogin;
            }
            catch
            {
                mainTabControlMaint.SelectedItem = TabItemUpdateLogin;
            }
        }

        //private void FillRollbackSqlConnInfo(string serverName, int authMode, string username, string pass)
        //{
        //    //tbSqlServerNameUpdate.Text = serverName;
        //    //cbAuthenticationUpdate.SelectedIndex = authMode;
        //    //tbSqlUserNameUpdate.Text = username;
        //    //pbSqlPasswordUpdate.Password = pass;
        //}

        //private void ReadRollbackSqlConnInfo(SqlConnectionInfo rollbackConnectionInfo)
        //{
        //    //rollbackConnectionInfo.ServerName = tbSqlServerNameUpdate.Text;
        //    //rollbackConnectionInfo.AuthMode = cbAuthenticationUpdate.SelectedIndex;
        //    //rollbackConnectionInfo.Username = tbSqlUserNameUpdate.Text;
        //    //rollbackConnectionInfo.Password = pbSqlPasswordUpdate.Password;
        //}

        private async void UpdateApp()
        {
            _UpdateCancelTokenSource = new CancellationTokenSource();
            ReportCurrentOperation(0);
            InstallerAndUpdaterManager.PrepareRollbackFolder(InstallationFolder);
            if(SelectedRollbackOption.Item1 == RollbackType.EnabledAutomatic)
                InstallerAndUpdaterManager.BackupDatabase(RollbackConnectionInfo, RollbackBackupDBFilePath);
            Progress<double> progressUpdate = new Progress<double>(ReportUpdateProgress);
            await InstallerAndUpdaterManager.DownloadVersion(InstallationFolder, _UpdateiPlusVersion, new Progress<double>(), _UpdateCancelTokenSource.Token);
            await InstallerAndUpdaterManager.DownloadApp(InstallationFolder, _UpdateiPlusVersion, progressUpdate, _UpdateCancelTokenSource.Token);
            ReportCurrentOperation(3);
            InstallerAndUpdaterManager.SetNewVersion(InstallationFolder);
            InstallerAndUpdaterManager.CopyUserFiles(InstallationFolder, _UpdateiPlusVersion);
            if (_UpdateCancelTokenSource.Token.IsCancellationRequested)
            {
                if (_resetEvent != null)
                    _resetEvent.Set();
                return;
            }
            InstallerAndUpdaterManager.DeleteRollbackOldVersion(InstallationFolder);
            if (SelectedRollbackOption.Item1 == installerAndUpdater.RollbackType.Disabled)
                InstallerAndUpdaterManager.DeleteRollbackVersion(InstallationFolder);

            ReportCurrentOperation(4);
        }

        public void ReportUpdateProgress(double progress)
        {
            CurrentUpdateOperationProgress = progress;
            CalculateTotalUpdateProgress(progress, 0.5);
            if (CurrentUpdateOperationProgress == 100 && CurrentUpdateOperation != 2)
            {
                CurrentUpdateOperationProgress = 0;
                _helpUpdateProgress = 50;
            }
        }

        private void CalculateTotalUpdateProgress(double progress, double currentUpdateOperation)
        {
            TotalUpdateProgress = _helpUpdateProgress + Math.Round((double)currentUpdateOperation * progress, 2);
        }

        private void btnBackLoginUpdate_Click(object sender, RoutedEventArgs e)
        {
            mainTabControlMaint.SelectedItem = TabItemUpdate;
        }

        private void btnLoginUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (InstallerAndUpdaterManager.Login(tbEmailUpdate.Text.Trim(), pbPasswordUpdate.Password.Trim()))
            {
                InstallerAndUpdaterManager.SaveCredentials(InstallationFolder, _UpdateiPlusVersion);
                mainTabControlMaint.SelectedItem = TabItemUpdate;
                InstallerAndUpdaterManager.CheckForUpdate(_UpdateiPlusVersion, InstallationFolder);
            }
        }

        private void btnCancelLoginUpdate_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnUpdateNext_Click(object sender, RoutedEventArgs e)
        {
            if (RollbackConnectionInfo == null && SelectedRollbackOption.Item1 == RollbackType.EnabledAutomatic)
            {
                RollbackConnectionInfo = InstallerAndUpdaterManager.TryGetDBLoginFromAppConfig(InstallationFolder, _UpdateiPlusVersion);
            }
            mainTabControlMaint.SelectedItem = TabItemUpdateRollbackOptions;
            if (RollbackConnectionInfo != null)
            {
                if (RollbackConnectionInfo.CheckServerLocationAndBackupPath())
                    RollbackBackupDBFilePath = string.Format("{0}\\{1}.bak", RollbackConnectionInfo.ServerBackupPath, RollbackConnectionInfo.DatabaseName);
                else
                    mainTabControlMaint.SelectedItem = TabItemUpdateRollbackDB;
            }
        }

        private void btnUpdateRollbackDB_Click(object sender, RoutedEventArgs e)
        {
            mainTabControlMaint.SelectedItem = TabItemUpdateRollbackOptions;
            RollbackConnectionInfo.AuthMode = cbAuthenticationUpdateRollback.SelectedIndex;
            if (RollbackConnectionInfo.CheckServerLocationAndBackupPath())
                RollbackBackupDBFilePath = string.Format("{0}\\{1}.bak", RollbackConnectionInfo.ServerBackupPath, RollbackConnectionInfo.DatabaseName);
            else
                mainTabControlMaint.SelectedItem = TabItemUpdateRollbackDB;
        }

        private void btnUpdateRollbackDBSkip_Click(object sender, RoutedEventArgs e)
        {
            var rollbackAuto = RollbackOptionsList.FirstOrDefault(c => c.Item1 == installerAndUpdater.RollbackType.EnabledAutomatic);
            RollbackOptionsList.Remove(rollbackAuto);
            SelectedRollbackOption = RollbackOptionsList.LastOrDefault();
            mainTabControlMaint.SelectedItem = TabItemUpdateRollbackOptions;
        }

        private void btnUpdateRollbackNext_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRollbackOption.Item1 == RollbackType.EnabledAutomatic)
            {
                if (RollbackBackupDBFilePath == null || !RollbackBackupDBFilePath.EndsWith(".bak"))
                {
                    MsgBox.Show(String.Format(TextResources.TextResource.MsgRollbackWrongBackupFileType), TextResources.TextResource.MsgHeaderWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Task.Run(() => InstallerAndUpdaterManager.TryConnectToDatabase(RollbackConnectionInfo, OnTryConnectToDBCallback));
                _OperationDialog = new OperationDialog(TextResources.TextResource.MsgConnecting);
                _OperationDialog.ShowDialog();
                IsRollbackOptionNextEnabled = false;
            }
            else
            {
                var result = MsgBox.Show(TextResources.TextResource.MsgUpdateRollbackOptionText, TextResources.TextResource.MsgUpdateRollbackOptionHeader, MessageBoxButton.YesNo, 
                                             MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                    mainTabControlMaint.SelectedItem = TabItemUpdateChangeLog;
            }
        }

        private void btnBrowseUpdateRollbackPath_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog() { IsFolderPicker = false })
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    RollbackBackupDBFilePath = dialog.FileName;
            }
        }

        public void OnTryConnectToDBCallback(SqlConfigurationState result)
        {
            Dispatcher.Invoke(() => _OperationDialog.Close());
            Dispatcher.Invoke(() => _OperationDialog = null);
            switch(result)
            {
                case SqlConfigurationState.AllOk:
                    Dispatcher.Invoke(() => (mainTabControlMaint.SelectedItem = TabItemUpdateChangeLog));
                    break;

                case SqlConfigurationState.NoConnection:
                    MsgBox.Show(TextResources.TextResource.MsgSqlErrorNoConnection, "MSSQL", MessageBoxButton.OK, MessageBoxImage.Warning);

                    break;

                case SqlConfigurationState.UserNotSysAdmin:
                    MsgBox.Show(TextResources.TextResource.MsgSqlErrorUserNotSysAdmin, "MSSQL", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
            IsRollbackOptionNextEnabled = true;
        }

        private void btnUpdateRollbackExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnUpdateInstallUpdate_Click(object sender, RoutedEventArgs e)
        {
            mainTabControlMaint.SelectedItem = TabItemUpdateProgress;
            _closeState = CloseState.WithMessage;
            Task.Run(() => UpdateApp());
        }

        private void btnUpdateChangeLogExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnUpdateChangeLogCancel_Click(object sender, RoutedEventArgs e)
        {
            mainTabControlMaint.SelectedItem = TabItemUpdateRollbackOptions;
        }

        private void btnStartVB_Click(object sender, RoutedEventArgs e)
        {
            string ver = _UpdateiPlusVersion == InstallerAndUpdaterManager.IPlusMES ? "mes" : _UpdateiPlusVersion;
            string pathToApp = string.Format("{0}\\gip.{1}.client.exe",InstallationFolder, ver);
            string param = "-controlLoad=True";
            Process vb = new Process() { StartInfo = new ProcessStartInfo(pathToApp, param) { WorkingDirectory = InstallationFolder } };
            vb.Start();
            _closeState = CloseState.WithoutMessageFinish;
            Close();
        }

        private void btnUpdateExit_Click(object sender, RoutedEventArgs e)
        {
            _closeState = CloseState.WithoutMessageFinish;
            Close();
        }

        private void btnUpdateProgressCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
