using System;
using System.Windows;
using lp = gip.tool.devLicenseProvider;
using System.ComponentModel;

namespace gip.tool.devLicense
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region c'tors

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #endregion

        #region Properties

        private lp.LicenseProvider _Provider;
        public lp.LicenseProvider Provider
        {
            get
            {
                if (_Provider == null)
                    _Provider = new gip.tool.devLicenseProvider.LicenseProvider();
                return _Provider;
            }
        }

        private lp.License _SelectedLicense;
        public lp.License SelectedLicense
        {
            get
            {
                return _SelectedLicense;
            }
            set
            {
                _SelectedLicense = value;
                OnPropertyChanged("SelectedLicense");
            }
        }

        #endregion

        #region Methods

        private void btnDevelLicense_Click(object sender, RoutedEventArgs e)
        {
            //new development licese
            DevelopmentLicense devel = new DevelopmentLicense(Provider);
            devel.ShowDialog();
        }

        private void btnDevelLicenseRewrite_Click(object sender, RoutedEventArgs e)
        {
            DevelopmentLicense devel = new DevelopmentLicense(SelectedLicense, Provider);
            devel.ShowDialog();
        }

        private void btnEndUserDev_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Provider.ClearDongle(Provider.GetAvailableDongles()));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedLicense != null)
            {
                if (MessageBox.Show("Are you sure, you want delete selected license?", "Delete license", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                    == MessageBoxResult.Yes)
                    Provider.DeleteLicense(SelectedLicense);
            }
        }

        private void miEditCustomers_Click(object sender, RoutedEventArgs e)
        {
            Customer customerDialog = new Customer(Provider);
            customerDialog.ShowDialog();
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
