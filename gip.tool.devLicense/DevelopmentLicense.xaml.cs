using System.Runtime.CompilerServices;
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
using System.Windows.Shapes;
using lp = gip.tool.devLicenseProvider;
using System.ComponentModel;
using System.Management;
using System.Security.Cryptography;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace gip.tool.devLicense
{
    /// <summary>
    /// Interaction logic for DevelopmentLicence.xaml
    /// </summary>
    public partial class DevelopmentLicense : Window, INotifyPropertyChanged
    {
        #region c'tors

        public DevelopmentLicense(lp.LicenseProvider provider)
        {
            InitializeComponent();
            DataContext = this;
            CurrentLicense = new lp.License();
            CurrentLicense.LicenseID = Guid.NewGuid();
            Provider = provider;
            IsNewLicense = true;
        }

        public DevelopmentLicense(lp.License licence, gip.tool.devLicenseProvider.LicenseProvider provider)
        {
            InitializeComponent();
            DataContext = this;
            CurrentLicense = licence;
            Provider = provider;
            //CustomerName = string.Format("Customer: {0}", CurrentLicense.Customer.CustomerName);
            tabControlMain.SelectedIndex = 1;
        }

        #endregion

        #region Properties

        private bool _IsNewLicense = false;
        public bool IsNewLicense
        {
            get
            {
                return _IsNewLicense;
            }
            set
            {
                _IsNewLicense = value;
                OnPropertyChanged("IsNewLicense");
            }
        }

        private lp.License _CurrentLicense;
        public lp.License CurrentLicense
        {
            get
            {
                return _CurrentLicense;
            }
            set
            {
                _CurrentLicense = value;
                OnPropertyChanged("CurrentLicense");
            }
        }

        public lp.LicenseProvider Provider
        {
            get;
            set;
        }

        private lp.Customer _CurrentCustomer;
        public lp.Customer CurrentCustomer
        {
            get
            {
                return _CurrentCustomer;
            }
            set
            {
                _CurrentCustomer = value;
                OnPropertyChanged("CurrentCustomer");
            }
        }

        private string _LicenseFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public string LicenseFilePath
        {
            get
            {
                return _LicenseFilePath;
            }
            set
            {
                _LicenseFilePath = value;
                OnPropertyChanged("LicenceFilePath");
            }
        }

        private string _ProjectNo;
        public string ProjectNo
        {
            get
            {
                return _ProjectNo;
            }
            set
            {
                _ProjectNo = value;
                OnPropertyChanged("ProjectNo");
            }
        }

        public List<lp.DongleInfo> AvailableDongles
        {
            get
            {
                return Provider.GetAvailableDongles();
            }
        }

        private string _RemoteLoginUserID;
        public string RemoteLoginUserID
        {
            get
            {
                return _RemoteLoginUserID;
            }
            set
            {
                _RemoteLoginUserID = value;
                OnPropertyChanged("RemoteLoginUserID");
            }
        }

        private string _RemoteLoginKey;
        public string RemoteLoginKey
        {
            get
            {
                return _RemoteLoginKey;
            }
            set
            {
                _RemoteLoginKey = value;
                OnPropertyChanged("RemoteLoginKey");
            }
        }

        #endregion

        #region Methods

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            tabControlMain.SelectedIndex = 1;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog() { IsFolderPicker = true })
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    LicenseFilePath = dialog.FileName;
            }
            this.Focus();
        }

        private void ClearDongle()
        {
            MessageBox.Show(Provider.ClearDongle(AvailableDongles), "Info");
        }

        private void btnWriteDevLicense_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLicense != null && CurrentLicense.Customer != null)
            {
                if (Provider.GenerateLicenseFile(CurrentLicense.Customer, LicenseFilePath, CurrentLicense.ProjectNo, CurrentLicense))
                {
                    MessageBox.Show("Licnese created!!!");
                    Close();
                }
                else
                    MessageBox.Show("License creation fail!!!");
            }
            else
            {
                if (Provider.GenerateLicenseFile(CurrentCustomer, LicenseFilePath, ProjectNo))
                {
                    MessageBox.Show("Licnese created!!!");
                    Close();
                }
                else
                    MessageBox.Show("License creation fail!!!");
            }
        }

        private void btnGenerateRLKey_Click(object sender, RoutedEventArgs e)
        {
            GenerateRemoteLicenceKey();
        }

        private void GenerateRemoteLicenceKey()
        {
            if (string.IsNullOrEmpty(RemoteLoginUserID) || CurrentLicense == null || string.IsNullOrEmpty(CurrentLicense.RemotePrivateKey))
                return;

            string tempKey = Provider.GenerateRemoteLoginKey(RemoteLoginUserID, CurrentLicense.RemotePrivateKey);
            if (string.IsNullOrEmpty(tempKey))
                return;
            RemoteLoginKey = tempKey;
        }

        private void btnCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(RemoteLoginKey))
                return;
            Clipboard.SetText(RemoteLoginKey);
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
