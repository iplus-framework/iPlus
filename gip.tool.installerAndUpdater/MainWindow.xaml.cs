using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml;
using System.Reflection;

namespace gip.tool.installerAndUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);
            mainTabControl.DataContext = this;
            mainTabControlMaint.DataContext = this;
            _installerAndUpdaterManager = new InstallerAndUpdaterManager(this);
            ApplicationMode = DetermineAppInstallOrUpdate();
            if (ApplicationMode == AppMode.Install)
                InitInstallation();
            else if(ApplicationMode == AppMode.Update)
                InitUpdate();
        }

        #region Common

        private InstallerAndUpdaterManager _installerAndUpdaterManager;
        public InstallerAndUpdaterManager InstallerAndUpdaterManager
        {
            get
            {
                return _installerAndUpdaterManager;
            }
        }

        private AppMode _ApplicationMode;
        public AppMode ApplicationMode
        {
            get
            {
                return _ApplicationMode;
            }
            set
            {
                _ApplicationMode = value;
                OnPropertyChanged("ApplicationMode");
            }
        }

        CloseState _closeState = CloseState.WithMessage;

        OperationDialog _OperationDialog;

        private bool _IsCanceled = false;
        public bool IsCanceled
        {
            get
            {
                return _IsCanceled;
            }
            set
            {
                _IsCanceled = value;
                OnPropertyChanged("IsCanceled");
            }
        }

        private SqlConnectionInfo _CurrentConnectionInfo;
        public SqlConnectionInfo CurrentConnectionInfo
        {
            get
            {
                return _CurrentConnectionInfo;
            }
            set
            {
                _CurrentConnectionInfo = value;
                OnPropertyChanged("CurrentConnectionInfo");
            }
        }

        private string _CompletedInstallationText = TextResources.TextResource.TbFinishInfo;
        public string CompletedInstallationText
        {
            get
            {
                return _CompletedInstallationText;
            }
            set
            {
                _CompletedInstallationText = value;
                OnPropertyChanged("CompletedInstallationText");
            }
        }

        private AppMode DetermineAppInstallOrUpdate()
        {
            string installFolder;
            if (InstallerAndUpdaterManager.ReadGipInstall(out installFolder, out _UpdateiPlusVersion))
            {
                InstallationFolder = installFolder;
                if (string.IsNullOrEmpty(InstallationFolder) ||  !Directory.Exists(InstallationFolder))
                    return AppMode.Install;
                return AppMode.Update;
            }
            return AppMode.Install;
        }

        public void ReportCurrentOperation(int currentOperation)
        {
            if (ApplicationMode == AppMode.Update)
                CurrentUpdateOperation = currentOperation;

            else if (ApplicationMode == AppMode.Rollback)
                CurrentRollbackOperation = currentOperation;

            else if (ApplicationMode == AppMode.Install)
                CurrentOperation = currentOperation;
        }

        public void CloseAfterDelete()
        {
            _cancelTokenSource = null;
            _closeState = CloseState.WithoutMessageFinish;
            Dispatcher.Invoke(() => Close());
        }

        public void ReportError(string msg)
        {
            MsgBox.Show(msg, "Error", MessageBoxButton.OK);
            _closeState = CloseState.WithoutMessageFinish;
            Dispatcher.Invoke(() => Close());
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (ApplicationMode == AppMode.Install)
            {
                if (_closeState == CloseState.WithoutMessageFinish)
                    base.OnClosing(e);
                else
                {
                    var result = MessageBoxResult.Yes;
                    if (_closeState == CloseState.WithMessage)
                        result = MsgBox.Show(TextResources.TextResource.MsgCancel, TextResources.TextResource.MsgCancelHeader, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                    {
                        IsCanceled = true;
                        switch (mainTabControl.SelectedIndex)
                        {
                            case 3:
                                CurrentOperation = 10;
                                IsIndeterminateCurrentOperationProgress = true;
                                _cancelTokenSource.Cancel();
                                _resetEvent = new ManualResetEvent(false);
                                ThreadPool.QueueUserWorkItem((object state) => InstallerAndUpdaterManager.DeleteFolder(InstallationFolder));
                                e.Cancel = true;
                                break;
                            case 4:
                                SqlServerCheckInfo = TextResources.TextResource.OperationCancel;
                                SqlBrowserServiceState = installerAndUpdater.SqlBrowserServiceState.ServiceRunning;
                                IsNextButtonEnabled = false;
                                IsCancelButtonEnabled = false;
                                ThreadPool.QueueUserWorkItem((object state) => InstallerAndUpdaterManager.DeleteFolder(InstallationFolder));
                                e.Cancel = true;
                                break;
                            case 6:
                                _cancelTokenSource.Cancel();
                                ThreadPool.QueueUserWorkItem((object state) => InstallerAndUpdaterManager.DeleteFolder(InstallationFolder));
                                e.Cancel = true;
                                break;
                        }
                        base.OnClosing(e);
                    }
                    else
                        e.Cancel = true;
                }
            }
            else if(_ApplicationMode == AppMode.Update)
            {
                if (_closeState == CloseState.WithoutMessageFinish )
                    base.OnClosing(e);
                else
                {
                    var result = MessageBoxResult.Yes;
                    if (_closeState == CloseState.WithMessage)
                        result = MsgBox.Show(TextResources.TextResource.MsgCancelUpdate, TextResources.TextResource.MsgCancelUpdateHeader, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (mainTabControlMaint.SelectedItem == TabItemUpdateProgress && CurrentUpdateOperation != 3)
                        {
                            ReportCurrentOperation(9);
                            _UpdateCancelTokenSource.Cancel(false);
                            _resetEvent = new ManualResetEvent(false);
                            ThreadPool.QueueUserWorkItem((object state) => InstallerAndUpdaterManager.DeleteUpdate(InstallationFolder));
                            e.Cancel = true;
                        }
                        else
                            base.OnClosing(e);
                    }
                    else
                        e.Cancel = true;
                }
            }
            else
            {
                base.OnClosing(e);
            }
        }

        private void btnAppMaintNext_Click(object sender, RoutedEventArgs e)
        {
            if (rbUpdate.IsChecked.HasValue && rbUpdate.IsChecked.Value)
                StartUpdateOperation();

            else if (rbRollback.IsChecked.HasValue && rbRollback.IsChecked.Value)
                InitRollback();

            else if (rbUninstall.IsChecked.HasValue && rbUninstall.IsChecked.Value)
                InitUninstall();
        }

        private void btnAppMaintExit_Click(object sender, RoutedEventArgs e)
        {
            _closeState = CloseState.WithoutMessage;
            Close();
        }

        private void tbUninstall_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            rbUninstall.IsChecked = true;
        }

        private void tbRollback_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            rbRollback.IsChecked = true;
        }

        private void tbUpdate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            rbUpdate.IsChecked = true;
        }

        void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.FullName.StartsWith("ManagedInjector"))
            {
                _closeState = CloseState.WithoutMessage;
                Close();
            }
        }

        #endregion

        #region Installation

        private void InitInstallation()
        {
            IsSerachCompleted = System.Windows.Visibility.Hidden;
            IsIndeterminate = true;
            FillProgressLevel();
        }

        #region License agreement

        private void btnAgree_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedIndex = 1;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region SqlServer Installation

        private void btnNextSQLServerInfo_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedIndex = 2;
        }

        private void btnCancelSQLServerInfo_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Hyperlink_RequestNavigate_SqlServer(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(TextResources.TextResource.SqlServerDownloadPage));
        }

        #endregion

        #region Login

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (InstallerAndUpdaterManager.Login(tbEmail.Text, pbPassword.Password))
                mainTabControl.SelectedIndex = 3;
            else
            {
                MsgBox.Show(TextResources.TextResource.MsgLoginWrongCred, TextResources.TextResource.MsgLoginWrongCredHeader, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                pbPassword.Clear();
            }
        }

        private void btnCancelLogin_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnBackLogin_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedIndex = 1;
        }

        #endregion

        #region Install

        private CancellationTokenSource _cancelTokenSource;

        string _InstallationFolder = @"C:\Program Files\gipSoft\iPlus";
        public string InstallationFolder
        {
            get
            {
                return _InstallationFolder;
            }
            set
            {
                _InstallationFolder = value;
                OnPropertyChanged("InstallationFolder");
            }
        }

        private string _CurrentOperationText;
        public string CurrentOperationText
        {
            get
            {
                return _CurrentOperationText;   
            }
            set
            {
                _CurrentOperationText = value;
                OnPropertyChanged("CurrentOperationText");
            }
        }

        private int _CurrentOperation;
        public int CurrentOperation
        {
            get
            {
                return _CurrentOperation;
            }
            set
            {
                if (_CurrentOperation == 10)
                    return;
                _CurrentOperation = value;
                switch (_CurrentOperation)
                {
                    case 0:
                        CurrentOperationText = TextResources.TextResource.OperationCreateFolder;
                        break;
                    case 1:
                        CurrentOperationText = TextResources.TextResource.OperationDownloadApp;
                        _helpProgress = 1;
                        break;
                    case 2:
                        CurrentOperationText = TextResources.TextResource.OperationUnpackApp;
                        _helpProgress = 30;
                        break;
                    case 3:
                        CurrentOperationText = TextResources.TextResource.OperationDownloadDB;
                        _helpProgress = 65;
                        break;
                    case 4:
                        CurrentOperationText = TextResources.TextResource.OperationUnpackDB;
                        _helpProgress = 85;
                        break;
                    case 5:
                        CurrentOperationText = TextResources.TextResource.OperationSetShortcut;
                        _helpProgress = 95;
                        break;
                    case 6:
                        CurrentOperationText = TextResources.TextResource.OperationCompleted;
                        _helpProgress = 100;
                        break;
                    case 10:
                        CurrentOperationText = TextResources.TextResource.OperationCancel;
                        break;
                }
                OnPropertyChanged("CurrentOperation");
            }
        }

        private int _helpProgress = 0;
        private Dictionary<int, double> _progressLevel;

        private bool _IsInstallationFinished = false;
        public bool IsInstallationFinished
        {
            get
            {
                return _IsInstallationFinished;
            }
            set
            {
                _IsInstallationFinished = value;
                OnPropertyChanged("IsInstallationFinished");
            }
        }

        public double _CurrentOperationProgress;
        public double CurrentOperationProgress
        {
            get
            {
                return _CurrentOperationProgress;
            }
            set
            {
                _CurrentOperationProgress = value;
                OnPropertyChanged("CurrentOperationProgress");
            }
        }

        public double _TotalProgress;
        public double TotalProgress
        {
            get
            {
                return _TotalProgress;
            }
            set
            {
                _TotalProgress = value;
                OnPropertyChanged("TotalProgress");
            }
        }

        private bool _IsIndeterminateCurrentOperationProgress = false;
        public bool IsIndeterminateCurrentOperationProgress
        {
            get
            {
                return _IsIndeterminateCurrentOperationProgress;
            }
            set
            {
                _IsIndeterminateCurrentOperationProgress = value;
                OnPropertyChanged("IsIndeterminateCurrentOperationProgress");
            }
        }

        private void FillProgressLevel()
        {
            _progressLevel = new Dictionary<int, double>();
            _progressLevel.Add(0, 0.01);
            _progressLevel.Add(1, 0.29);
            _progressLevel.Add(2, 0.35);
            _progressLevel.Add(3, 0.20);
            _progressLevel.Add(4, 0.10);
            _progressLevel.Add(5, 0.5);
            _progressLevel.Add(6, 0.0);
            _progressLevel.Add(10, 0);
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedIndex = 4;
            Progress<double> progress = new Progress<double>(ReportCurrentOperationProgress);
            _cancelTokenSource = new CancellationTokenSource();
            InstallerAndUpdaterManager.InstallVB(InstallationFolder, ChbDesktopShortcut.IsChecked.Value, cbiPlusVersion.Text, progress, _cancelTokenSource.Token);
        }

        public void ReportCurrentOperationProgress(double progress)
        {
            CurrentOperationProgress = progress;
            CalculateTotalProgress(progress);
            if (CurrentOperationProgress == 100)
                CurrentOperationProgress = 0;
        }

        public void CalculateTotalProgress(double progress)
        {
            TotalProgress = _helpProgress + Math.Round((double)_progressLevel[CurrentOperation] * progress, 2);
        }

        private void btnCancelInstall_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnBackInstall_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedIndex = 2;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using(var dialog = new CommonOpenFileDialog(){IsFolderPicker=true})
            {
                if (Directory.Exists(InstallationFolder))
                    dialog.InitialDirectory = InstallationFolder;
                else if (Directory.Exists("C:\\Program Files"))
                    dialog.InitialDirectory = "C:\\Program Files";
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    InstallationFolder = dialog.FileName;
            }
        }

        private void btnNextInstallProgress_Click(object sender, RoutedEventArgs e)
        {
            SqlBrowserServiceState = InstallerAndUpdaterManager.CheckSqlBrowserService();
            mainTabControl.SelectedIndex = 5;

            if (SqlBrowserServiceState == SqlBrowserServiceState.ServiceRunning)
                Task.Run(async () => await InstallerAndUpdaterManager.FindSqlServersAsync());
            else
                IsNextButtonEnabled = true;
        }

        private void btnCancelInstallProgress_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Sql server check

        private SqlBrowserServiceState _SqlBrowserServiceState;
        [DefaultValue(SqlBrowserServiceState.ServiceNotExist)]
        public SqlBrowserServiceState SqlBrowserServiceState
        {
            get
            {
                return _SqlBrowserServiceState;
            }
            set
            {
                _SqlBrowserServiceState = value;
                OnPropertyChanged("SqlBrowserServiceState");
            }
        }

        private Visibility _IsSearchCompleted;
        public Visibility IsSerachCompleted
        {
            get
            {
                return _IsSearchCompleted;
            }
            set
            {
                _IsSearchCompleted = value;
                OnPropertyChanged("IsSerachCompleted");
            }
        }

        private bool _IsNextButtonEnabled;
        public bool IsNextButtonEnabled
        {
            get
            {
                return _IsNextButtonEnabled;
            }
            set
            {
                _IsNextButtonEnabled = value;
                OnPropertyChanged("IsNextButtonEnabled");
            }
        }

        private bool _IsCancelButtonEnabled = true;
        public bool IsCancelButtonEnabled
        {
            get 
            {
                return _IsCancelButtonEnabled;
            }
            set
            {
                _IsCancelButtonEnabled = value;
                OnPropertyChanged("IsCancelButtonEnabled");
            }
        }

        private bool _IsIndeterminate;
        public bool IsIndeterminate
        {
            get
            {
                return _IsIndeterminate;
            }
            set
            {
                _IsIndeterminate = value;
                OnPropertyChanged("IsIndeterminate");
            }
        }

        private string _SqlServerCheckInfo = TextResources.TextResource.TbSqlCheckSearching;
        public string SqlServerCheckInfo
        {
            get
            {
                return _SqlServerCheckInfo;
            }
            set
            {
                _SqlServerCheckInfo = value;
                OnPropertyChanged("SqlServerCheckInfo");
            }
        }

        public void SqlServerSearchingCompleted(List<string> sqlServerList)
        {
            SqlServerList = sqlServerList;
            if (SqlServerList.Any())
                SelectedSqlServer = SqlServerList.FirstOrDefault();

            IsSerachCompleted = System.Windows.Visibility.Visible;
            IsNextButtonEnabled = true;
            IsIndeterminate = false;
        }

        private void btnRefreshSql_Click(object sender, RoutedEventArgs e)
        {
            SqlBrowserServiceState = InstallerAndUpdaterManager.CheckSqlBrowserService();
            if (SqlBrowserServiceState == installerAndUpdater.SqlBrowserServiceState.ServiceRunning)
            {
                IsNextButtonEnabled = false;
                Task<List<string>> task = Task.Run(async () => await InstallerAndUpdaterManager.FindSqlServersAsync());
            }
        }

        private void btnNextSql_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedIndex = 6;
        }

        private void btnCancelSql1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region DatabaseConfig

        private ObservableCollection<string> _SqlProgressInfo;
        public ObservableCollection<string> SqlProgressInfo
        {
            get
            {
                if (_SqlProgressInfo == null)
                    _SqlProgressInfo = new ObservableCollection<string>();
                return _SqlProgressInfo;
            }
        }

        public void ReportProgressMessage(double progress)
        {
            //switch(progress)
            //{
                if(progress == 0.0)
                    SqlProgressInfo.Add(TextResources.TextResource.DBProgress0);
                    //break;
                else if(progress == 20.0)
                    SqlProgressInfo.Add(TextResources.TextResource.DBProgress10);
                    //break;
                else if(progress == 30.0)
                    SqlProgressInfo.Add(TextResources.TextResource.DBProgress30);
                    //break;
                else if(progress == 70.0)
                    SqlProgressInfo.Add(TextResources.TextResource.DBProgress70);
                    //break;
                else if(progress == 75.0)
                    SqlProgressInfo.Add(TextResources.TextResource.DBProgress75);
                    //break;
                else if (progress == 100)
                {
                    SqlProgressInfo.Add(TextResources.TextResource.DBProgress100);
                    IsEnabledbtnNextSqlProgress_Click = true;
                }
                else if (progress == -1)
                {
                    SqlProgressInfo.Add("Please insert your database and configure app.config manually...");
                    IsEnabledbtnNextSqlProgress_Click = true;
                }
                    //break;
            }
        //}

        private void btnNextSqlProgress_Click(object sender, RoutedEventArgs e)
        {
            _closeState = CloseState.WithoutMessageFinish;
            mainTabControl.SelectedIndex = 8;
        }

        private bool _IsEnabledbtnNextSqlProgress_Click = false;
        public bool IsEnabledbtnNextSqlProgress_Click
        {
            get
            {
                return _IsEnabledbtnNextSqlProgress_Click;
            }
            set
            {
                _IsEnabledbtnNextSqlProgress_Click = value;
                OnPropertyChanged("IsEnabledbtnNextSqlProgress_Click");
            }
        }

        private void btnCancelSqlProgress_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private string _SelectedSqlServer;
        public string SelectedSqlServer
        {
            get
            {
                return _SelectedSqlServer;
            }
            set
            {
                _SelectedSqlServer = value;
                OnPropertyChanged("SelectedSqlServer");
            }
        }

        private List<string> _SqlServerList;
        public List<string> SqlServerList
        {
            get
            {
                return _SqlServerList;
            }
            set
            {
                _SqlServerList = value;
                OnPropertyChanged("SqlServerList");
            }
        }

        private void btnConfigureDB_Click(object sender, RoutedEventArgs e)
        {
            string serverName = cbServerName.Text;
            int authMode = cbAuthentication.SelectedIndex;
            string userName = tbSqlUserName.Text.Trim().ToString();
            string password = pbSqlPassword.Password.ToString();

            if (string.IsNullOrEmpty(serverName))
            {
                MsgBox.Show(TextResources.TextResource.MsgServerNameEmpty, TextResources.TextResource.MsgHeaderWarning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(authMode == 1 && (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)))
            {
                MsgBox.Show(TextResources.TextResource.MsgDBUserOrPassEmpty, TextResources.TextResource.MsgHeaderWarning, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return;
            }

            Task<SqlConnectionInfo> task =
                Task.Run(async () => await
                    InstallerAndUpdaterManager.CheckSqlServerUserRoleAsync(new SqlConnectionInfo() { ServerName = serverName, AuthMode = authMode, Username = userName, Password = password }));

            if (task.Result.ConnectionState == SqlConfigurationState.AllOk)
            {
                mainTabControl.SelectedIndex = 7;
                string version = cbiPlusVersion.Text.ToString();
                if (!string.IsNullOrEmpty(_UpdateiPlusVersion))
                    version = _UpdateiPlusVersion;

                var progress = new Progress<double>(ReportProgress);
                _cancelTokenSource = new CancellationTokenSource();
                Task.Run(async () => await InstallerAndUpdaterManager.InstallationSqlConfigureAsync(task.Result, progress, InstallationFolder, version, _cancelTokenSource.Token));
            }
            else if (task.Result.ConnectionState == SqlConfigurationState.NoConnection)
                MsgBox.Show(TextResources.TextResource.MsgSqlErrorNoConnection, TextResources.TextResource.MsgErrorHeader, MessageBoxButton.OK, MessageBoxImage.Warning);

            else if (task.Result.ConnectionState == SqlConfigurationState.UserNotSysAdmin)
                MsgBox.Show(TextResources.TextResource.MsgSqlErrorUserNotSysAdmin, TextResources.TextResource.MsgErrorHeader, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void btnCancelDB_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSkipDB_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedIndex = 8;
        }

        public void ReportProgress(double value)
        {
            pbStatus.Value = value;
            ReportProgressMessage(value);
        }

        #endregion

        #region Completed installation

        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            _closeState = CloseState.WithoutMessageFinish;
            if (Directory.Exists(InstallationFolder))
                AddInstallFlagToUserAppData();
            Close();
        }

        internal void AddInstallFlagToUserAppData()
        {
            string gipInstallPath = System.Environment.ExpandEnvironmentVariables("%LocalAppData%\\gipInstall");
            string gipFileInstallPath = gipInstallPath+"\\install.gip";
            if (!Directory.Exists(gipInstallPath))
                Directory.CreateDirectory(gipInstallPath);

            string xml = string.Format("<xml><installationFolder>{0}</installationFolder><iplusVer>{1}</iplusVer></xml>", InstallationFolder, cbiPlusVersion.Text);
            File.WriteAllText(gipFileInstallPath, xml);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (Directory.Exists(InstallationFolder))
                AddInstallFlagToUserAppData();
            Process.Start(new ProcessStartInfo(TextResources.TextResource.iPlusDocumentationPage));
            e.Handled = true;
            _closeState = CloseState.WithoutMessageFinish;
            if (Directory.Exists(InstallationFolder))
                AddInstallFlagToUserAppData();
            Close();
        }

        #endregion

        #endregion

        #region Rollback

        public void InitRollback()
        {
            ApplicationMode = AppMode.Rollback;
            string installFolder = "";
            InstallerAndUpdaterManager.ReadGipInstall(out installFolder, out _UpdateiPlusVersion);
            InstallationFolder = installFolder;
            if (string.IsNullOrEmpty(InstallationFolder))
                return;

            RollbackType = InstallerAndUpdaterManager.DetermineRollbackType(InstallationFolder);
            
            switch(RollbackType)
            {
                case installerAndUpdater.RollbackType.Disabled:
                    StartRollbackDisabled();
                    break;
                
                case installerAndUpdater.RollbackType.EnabledManual:
                    StartRollbackEnabledManual();
                    break;
            }
        }

        private RollbackType _RollbackType;
        public RollbackType RollbackType
        {
            get
            {
                return _RollbackType;
            }
            set
            {
                _RollbackType = value;
                OnPropertyChanged("RollbackType");
            }
        }

        private gip.tool.publish.Version _SelectedRollbackVersion;
        public gip.tool.publish.Version SelectedRollbackVersion
        {
            get
            {
                return _SelectedRollbackVersion;
            }
            set
            {
                _SelectedRollbackVersion = value;
                OnPropertyChanged("SelectedRollbackVersion");
            }
        }

        private List<gip.tool.publish.Version> _RollbackVersionList;
        public List<gip.tool.publish.Version> RollbackVersionList
        {
            get
            {
                return _RollbackVersionList;
            }
            set
            {
                _RollbackVersionList = value;
                OnPropertyChanged("RollbackVersionList");
            }
        }

        public bool _IsIndeterminateRollbackProgress = true;
        public bool IsIndeterminateRollbackProgress
        {
            get
            {
                return _IsIndeterminateRollbackProgress;
            }
            set
            {
                _IsIndeterminateRollbackProgress = value;
                OnPropertyChanged("IsIndeterminateRollbackProgress");
            }
        }

        private int _CurrentRollbackOperation;
        public int CurrentRollbackOperation
        {
            get
            {
                return _CurrentRollbackOperation;
            }
            set
            {
                if (_CurrentRollbackOperation == 10)
                    return;
                _CurrentRollbackOperation = value;
                switch(_CurrentRollbackOperation)
                {
                    case 1:
                        Dispatcher.Invoke(() => RollbackOperationTextList.Add(TextResources.TextResource.OperationRollbackStart));
                        break;
                    case 2:
                        Dispatcher.Invoke(() => RollbackOperationTextList.Add(TextResources.TextResource.OperationRollbackUnpackDatabase));
                        break;
                    case 3:
                        Dispatcher.Invoke(() => RollbackOperationTextList.Add(TextResources.TextResource.OperationRollbackRestoreDatabase));
                        break;
                    case 4:
                        Dispatcher.Invoke(() => RollbackOperationTextList.Add(TextResources.TextResource.OperationRollbackUninstallCurrentVersion));
                        break;
                    case 5:
                        Dispatcher.Invoke(() => RollbackOperationTextList.Add(TextResources.TextResource.OperationRollbackRestorePrevAppVersion));
                        break;
                    case 6:
                        Dispatcher.Invoke(() => RollbackOperationTextList.Add(TextResources.TextResource.OperationRollbackCompleted));
                        IsIndeterminateRollbackProgress = false;
                        break;
                    case 10:
                        Dispatcher.Invoke(() => RollbackOperationTextList.Add("Rollback fail."));
                        break;
                }
                OnPropertyChanged("CurrentRollbackOperation");
            }
        }

        private ObservableCollection<string> _RollbackOperationTextList;
        public ObservableCollection<string> RollbackOperationTextList
        {
            get
            {
                if (_RollbackOperationTextList == null)
                    _RollbackOperationTextList = new ObservableCollection<string>();
                return _RollbackOperationTextList;
            }
        }

        private string _RollbackAppVersion;
        public string RollbackAppVersion
        {
            get
            {
                return _RollbackAppVersion;
            }
            set
            {
                _RollbackAppVersion = value;
                OnPropertyChanged("RollbackAppVersion");
            }
        }

        private string _RollbackBackupDBFilePath;
        public string RollbackBackupDBFilePath
        {
            get
            {
                return _RollbackBackupDBFilePath;
            }
            set
            {
                _RollbackBackupDBFilePath = value;
                OnPropertyChanged("RollbackBackupDBFilePath");
            }
        }
            
        public void StartRollbackDisabled()
        {
            cbiPlusVersion.Text = _UpdateiPlusVersion;
            string userInfo = InstallerAndUpdaterManager.RestoreCredentials(InstallationFolder);
            if (string.IsNullOrEmpty(userInfo))
            {
                mainTabControlMaint.SelectedItem = TabItemRollbackDisabledLogin;
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
                    mainTabControlMaint.SelectedItem = TabItemRollbackDisabled;
                    CancellationTokenSource source = new CancellationTokenSource();
                    Task.Run(() => LoadAppVersions(source));
                    _OperationDialog = new OperationDialog(TextResources.TextResource.MsgLoading);
                    _OperationDialog.ShowDialog();
                }
                else
                    mainTabControlMaint.SelectedItem = TabItemRollbackDisabledLogin;
            }
            catch
            {
                mainTabControlMaint.SelectedItem = TabItemRollbackDisabledLogin;
            }
        }

        public async void LoadAppVersions(CancellationTokenSource cancelToken)
        {
            await InstallerAndUpdaterManager.CheckUpdateVersion(InstallationFolder, _UpdateiPlusVersion);
            RollbackVersionList = InstallerAndUpdaterManager.AppVersionList.Where(c => c.PublishDateTime < InstallerAndUpdaterManager.AppVersion.PublishDateTime).ToList();
            if (RollbackVersionList != null && RollbackVersionList.Any())
            {
                SelectedRollbackVersion = RollbackVersionList.LastOrDefault();
                RollbackDisabledAvailable = RollbackAvailablity.Yes;
            }
            else
                RollbackDisabledAvailable = RollbackAvailablity.No;
            Dispatcher.Invoke(() => _OperationDialog.Close());
            Dispatcher.Invoke(() => _OperationDialog = null);
        }

        private RollbackAvailablity _RollbackDisabledAvailable = RollbackAvailablity.Search;
        public RollbackAvailablity RollbackDisabledAvailable
        {
            get
            {
                return _RollbackDisabledAvailable;
            }
            set
            {
                _RollbackDisabledAvailable = value;
                OnPropertyChanged("RollbackDisabledAvailable");
            }
        }

        public void StartRollbackEnabledManual()
        {
            CurrentConnectionInfo = InstallerAndUpdaterManager.TryGetDBLoginFromAppConfig(InstallationFolder, _UpdateiPlusVersion);
            RollbackAppVersion = TextResources.TextResource.TbRollbackAppVersion + " " + InstallerAndUpdaterManager.AppVersion.SvnRevision;
            mainTabControlMaint.SelectedItem = TabItemRollbackManual;
        }

        private void btnRollbackDB_Click(object sender, RoutedEventArgs e)
        {
            if (MsgBox.Show(TextResources.TextResource.MsgRollbackManualText, TextResources.TextResource.MsgRollbackManualHeader, MessageBoxButton.YesNo, MessageBoxImage.Warning) 
                == MessageBoxResult.Yes)
            {
                Task.Run(async () => await InstallerAndUpdaterManager.TryConnectToDatabase(CurrentConnectionInfo, OnCheckRollbackDBLogin));
                _OperationDialog = new OperationDialog(TextResources.TextResource.MsgConnecting);
                _OperationDialog.ShowDialog();
            }
        }

        private void btnRollbackDBExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnCheckRollbackDBLogin(SqlConfigurationState state)
        {
            Dispatcher.Invoke(() => _OperationDialog.Close());
            Dispatcher.Invoke(() => _OperationDialog = null);
            switch(state)
            {
                case SqlConfigurationState.AllOk:
                    Dispatcher.Invoke(() => mainTabControlMaint.SelectedItem = TabItemRollbackProgress);
                    _cancelTokenSource = new CancellationTokenSource();
                    InstallerAndUpdaterManager.RollbackApp(InstallationFolder, _UpdateiPlusVersion, RollbackType, _cancelTokenSource.Token, CurrentConnectionInfo, 
                                                           ReportCurrentOperation, RollbackBackupDBFilePath);
                    break;
                case SqlConfigurationState.NoConnection:
                    MsgBox.Show(TextResources.TextResource.MsgSqlErrorNoConnection, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                case SqlConfigurationState.UserNotSysAdmin:
                    MsgBox.Show(TextResources.TextResource.MsgSqlErrorUserNotSysAdmin, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
        }

        private void btnRollbackManual_Click(object sender, RoutedEventArgs e)
        {
            if (MsgBox.Show(TextResources.TextResource.MsgRollbackManualText, TextResources.TextResource.MsgRollbackManualHeader, MessageBoxButton.YesNo, MessageBoxImage.Warning) 
                == MessageBoxResult.Yes)
            {
                mainTabControlMaint.SelectedItem = TabItemRollbackProgress;
                _cancelTokenSource = new CancellationTokenSource();
                InstallerAndUpdaterManager.RollbackApp(InstallationFolder, _UpdateiPlusVersion, RollbackType, _cancelTokenSource.Token, CurrentConnectionInfo, ReportCurrentOperation);
            }
        }

        private void btnRollbackManualNext_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(RollbackBackupDBFilePath))
            {
                MsgBox.Show(TextResources.TextResource.MsgRollbackManualMissingFileText, TextResources.TextResource.MsgRollbackManualMissingFileHeader, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if (!RollbackBackupDBFilePath.EndsWith(".bak"))
            {
                MsgBox.Show(TextResources.TextResource.MsgRollbackManualWrongFileTypeText, TextResources.TextResource.MsgRollbackManualWrongFileTypeHeader, MessageBoxButton.OK, 
                                MessageBoxImage.Warning);
                return;
            }

            mainTabControlMaint.SelectedItem = TabItemRollbackDB;
        }

        private void btnRollbackDisabled_Click(object sender, RoutedEventArgs e)
        {
            if (MsgBox.Show(TextResources.TextResource.MsgRollbackDisabledNext, TextResources.TextResource.MsgHeaderWarning, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                InitInstallation();
                ApplicationMode = AppMode.Install;
                InstallerAndUpdaterManager.SaveAppVersion(InstallationFolder, SelectedRollbackVersion);
                mainTabControl.SelectedIndex = 4;
                Progress<double> progress = new Progress<double>(ReportCurrentOperationProgress);
                _cancelTokenSource = new CancellationTokenSource();
                InstallerAndUpdaterManager.InstallVB(InstallationFolder, ChbDesktopShortcut.IsChecked.Value, _UpdateiPlusVersion, progress, _cancelTokenSource.Token, SelectedRollbackVersion);
            }
        }

        private void btnRollbackDisabledExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnRollbackProgressFinish_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnRollbackDBFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                if (Directory.Exists(RollbackBackupDBFilePath))
                    dialog.InitialDirectory = RollbackBackupDBFilePath;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    RollbackBackupDBFilePath = dialog.FileName;
            }
        }

        private void btnLoginRollback_Click(object sender, RoutedEventArgs e)
        {
            if (InstallerAndUpdaterManager.Login(tbEmailRollback.Text.Trim(), pbPasswordRollback.Password.Trim()))
            {
                InstallerAndUpdaterManager.SaveCredentials(InstallationFolder, _UpdateiPlusVersion);
                mainTabControlMaint.SelectedItem = TabItemRollbackDisabled;
                CancellationTokenSource source = new CancellationTokenSource();
                Task.Run(() => LoadAppVersions(source));
                _OperationDialog = new OperationDialog(TextResources.TextResource.MsgLoading);
                _OperationDialog.ShowDialog();
            }
            else
            {
                MsgBox.Show(TextResources.TextResource.MsgLoginWrongCred, TextResources.TextResource.MsgLoginWrongCredHeader, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                pbPasswordRollback.Clear();
            }
        }

        private void btnCancelLoginRollback_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Uninstall

        private void InitUninstall()
        {
            UninstallInfo = string.Format(TextResources.TextResource.TbUninstallRemove, _UpdateiPlusVersion);
            UninstallFromInfo = string.Format(TextResources.TextResource.TbUninstallFollowingFolder, _UpdateiPlusVersion);
            mainTabControlMaint.SelectedItem = TabItemUnistall;
        }

        private string _UninstallInfo;
        public string UninstallInfo
        {
            get
            {
                return _UninstallInfo;
            }
            set
            {
                _UninstallInfo = value;
                OnPropertyChanged("UninstallInfo");
            }
        }

        private string _UninstallFromInfo;
        public string UninstallFromInfo
        {
            get
            {
                return _UninstallFromInfo;
            }
            set
            {
                _UninstallFromInfo = value;
                OnPropertyChanged("UninstallFromInfo");
            }
        }

        private double _UninstallProgress;
        public double UninstallProgress
        {
            get
            {
                return _UninstallProgress;
            }
            set
            {
                _UninstallProgress = value;
                OnPropertyChanged("UninstallProgress");
            }
        }

        private bool _IsUninstallCompleted = false;
        public bool IsUninstallCompleted
        {
            get
            {
                return _IsUninstallCompleted;
            }
            set
            {
                _IsUninstallCompleted = value;
                OnPropertyChanged("IsUninstallCompleted");
            }
        }

        private void btnUninstall_Click(object sender, RoutedEventArgs e)
        {
            if (MsgBox.Show(TextResources.TextResource.MsgUninstallText, TextResources.TextResource.MsgUninstallHeader, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                mainTabControlMaint.SelectedItem = TabItemUnistallProgress;
                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += progress_ProgressChanged;
                ThreadPool.QueueUserWorkItem((object state) => InstallerAndUpdaterManager.UninstallApp(InstallationFolder, true, _UpdateiPlusVersion, progress));
            }
        }

        private void btnUninstallCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void progress_ProgressChanged(object sender, double e)
        {
            if (e == -1)
                IsUninstallCompleted = true;
            else
                UninstallProgress = e;
        }

        private void btnUninstallFinish_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion
    }
}
