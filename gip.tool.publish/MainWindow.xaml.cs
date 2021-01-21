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
using System.Xml;
using System.Runtime.Serialization;
using System.IO;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;

namespace gip.tool.publish
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            TryRestoreUserData();
            Manager = new PublishManager();
            cbiPlusVersion.DataContext = this;
            SelectedUserData = UserDataList.FirstOrDefault();
            pbProgress.DataContext = this;
        }

        #region Properties

        private UserData _SelectedUserData;
        public UserData SelectedUserData
        {
            get
            {
                return _SelectedUserData;
            }
            set
            {
                _SelectedUserData = value;
                mainTabControl.DataContext = _SelectedUserData;
                OnPropertyChanged("SelectedUserData");
            }
        }

        private List<UserData> _UserDataList;
        public List<UserData> UserDataList
        {
            get
            {
                return _UserDataList;
            }
            set
            {
                _UserDataList = value;
                OnPropertyChanged("UserDataList");
            }
        }

        private string _LastRevisionNo;
        public string LastRevisionNo
        {
            get
            {
                return _LastRevisionNo;
            }
            set
            {
                _LastRevisionNo = value;
                OnPropertyChanged("LastRevisionNo");
            }
        }

        public PublishManager Manager
        {
            get;
            set;
        }

        private ObservableCollection<string> _PublishOperationInfo;
        public ObservableCollection<string> PublishOperationInfo
        {
            get
            {
                if (_PublishOperationInfo == null)
                    _PublishOperationInfo = new ObservableCollection<string>();
                return _PublishOperationInfo;
            }
        }

        private bool _IsPublishFinished = true;
        public bool IsPublishFinished
        {
            get
            {
                return _IsPublishFinished;
            }
            set
            {
                _IsPublishFinished = value;
                OnPropertyChanged("IsPublishFinished");
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

        #endregion

        #region Methods

        private void SaveUserData()
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(List<UserData>));
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\userData.xml"))
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\userData.xml");
            using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\userData.xml", FileMode.OpenOrCreate))
            {
                try
                {
                    serializer.WriteObject(fs, UserDataList);
                }
                catch
                {
                    Console.WriteLine("User data serialization fail!!!");
                }
            }
        }

        private bool TryRestoreUserData()
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(List<UserData>));
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\userData.xml"))
            {
                if (UserDataList == null)
                    UserDataList = new List<UserData>();

                UserDataList.Add(new UserData() { UserDataName = "IPlus" });
                UserDataList.Add(new UserData() { UserDataName = "IPlusMES"});
                return false;
            }
            using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\userData.xml", FileMode.Open))
            {
                try
                {
                    UserDataList = serializer.ReadObject(fs) as List<UserData>;
                    return true;
                }
                catch
                {
                    if (UserDataList == null)
                        UserDataList = new List<UserData>();

                    UserDataList.Add(new UserData() { UserDataName = "IPlus" });
                    UserDataList.Add(new UserData() { UserDataName = "IPlusMES" });
                    return false;
                }
            }
        }

        private void btnStartNext_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBinFolder(SelectedUserData.BinFolderPath))
            {
                if (string.IsNullOrEmpty(SelectedUserData.SqlScriptFilePath) || !SelectedUserData.SqlScriptFilePath.EndsWith(".sql"))
                {
                    MessageBox.Show("Please select sql database script", "Sql script missing");
                    return;
                }

                if(!File.Exists(SelectedUserData.SqlScriptFilePath))
                {
                    MessageBox.Show("The database script file not exist " + SelectedUserData.SqlScriptFilePath, "Sql script missing");
                    return;
                }

                if (!CheckSqlScript())
                {
                    MessageBox.Show("The database script is not valid. Change the USE command to USE [-dbName-] in database script.", "Invalid script");
                    return;
                }
                mainTabControl.SelectedIndex = 1;
            }
            else
                MessageBox.Show("iPlus exe file not found in selected folder!!! Choose correct bin folder or rebuild application.", "Error");
        }

        private void btnFTPServerNext_Click(object sender, RoutedEventArgs e)
        {
            if (!Manager.TryConnectToFtpServer(SelectedUserData.FtpServerHost, SelectedUserData.FtpUserName, SelectedUserData.FtpPassword))
            {
                MessageBox.Show("Can't connect to FTP server!!! Please check credentials...", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            mainTabControl.SelectedIndex = 2;
            SaveUserData();
        }

        private void btnVCNext_Click(object sender, RoutedEventArgs e)
        {
            if (!Manager.TryGetLastRevisonNumber(SelectedUserData.VersionControlServer, SelectedUserData.VersionControlDeployFilePath, SelectedUserData.VersionControl))
            {
                MessageBox.Show("Can't connect to version control sever!!! Check version control server url.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SaveUserData();
            mainTabControl.SelectedIndex = 3;
        }

        private void btnVCBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog() { IsFolderPicker = false })
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    SelectedUserData.VersionControlDeployFilePath = dialog.FileName;
            }
        }

        private void btnChangeLogNext_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedUserData.ChangeLogMessage))
            {
                MessageBox.Show("Change log cannot be empty!!!", "Empty changelog", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            mainTabControl.SelectedIndex = 4;
        }

        private void btnChangeLogDENext_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedUserData.ChangeLogMessage))
            {
                MessageBox.Show("Change log cannot be empty!!!", "Empty changelog", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            mainTabControl.SelectedIndex = 5;
        }

        private void rulesOk_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBinFolder(SelectedUserData.BinFolderPath))
            {
                if(string.IsNullOrEmpty(SelectedUserData.SqlScriptFilePath) || !SelectedUserData.SqlScriptFilePath.EndsWith(".sql"))
                {
                    MessageBox.Show("Please select sql database script", "Sql script missing");
                    return;
                }

                SaveUserData();
                mainTabControl.SelectedIndex = 2;
            }
            else
                MessageBox.Show("iPlus exe file not found in selected folder!!! Choose correct bin folder or rebuild application.", "Error");
        }

        private void btnPublish_Click(object sender, RoutedEventArgs e)
        {
            IsPublishFinished = false;
            IsIndeterminate = true;

            mainTabControl.SelectedIndex = 5;

            string passToClear = new ClearPass().ShowPassClearDialog();

            PublishOperationInfo.Clear();
            Progress<string> progress = new Progress<string>();
            progress.ProgressChanged += progress_ProgressChanged;
            Manager.PublishApplication(SelectedUserData, progress, passToClear);

            SaveUserData();
        }

        void progress_ProgressChanged(object sender, string e)
        {
            PublishOperationInfo.Insert(0, e);
            if (e.Contains("10)"))
            {
                IsPublishFinished = true;
                IsIndeterminate = false;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedIndex = 0;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog() { IsFolderPicker = true })
            {
                if (SelectedUserData != null && !string.IsNullOrEmpty(SelectedUserData.BinFolderPath) && Directory.Exists(SelectedUserData.BinFolderPath))
                    dialog.InitialDirectory = SelectedUserData.BinFolderPath;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    SelectedUserData.BinFolderPath = dialog.FileName;
            }
        }

        private void btnBrowseSql_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog() { IsFolderPicker = false })
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    SelectedUserData.SqlScriptFilePath = dialog.FileName;
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool CheckBinFolder(string binFolderPath)
        {
            if (SelectedUserData == null)
                return false;

            var files = Directory.GetFiles(binFolderPath, "*.exe");
            if (SelectedUserData.UserDataName == "IPlusMES" && files.Any(c => c.Contains("gip.mes.client.exe")))
                return true;
            else if (SelectedUserData.UserDataName == "IPlus" && files.Any(c => c.Contains("gip.iplus.client.exe")))
                return true;
            return false;
        }

        private bool CheckSqlScript()
        {
            if (SelectedUserData == null || string.IsNullOrEmpty(SelectedUserData.SqlScriptFilePath))
                return false;

            using(StreamReader sr = new StreamReader(SelectedUserData.SqlScriptFilePath))
            {
                for (int i = 0; i < 5; i++)
                {
                    string line = sr.ReadLine();
                    if (line.Contains("-dbName-"))
                        return true;
                }
            }
            return false;
        }

        #endregion

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
