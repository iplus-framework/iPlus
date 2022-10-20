using System.Runtime.CompilerServices;
using gip.tool.devLicenseProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace gip.tool.devLicense
{
    /// <summary>
    /// Interaction logic for Customer.xaml
    /// </summary>
    public partial class Customer : Window, INotifyPropertyChanged
    {
        #region c'tors

        public Customer(gip.tool.devLicenseProvider.LicenseProvider provider)
        {
            InitializeComponent();
            Provider = provider;
            this.DataContext = this;
            gridContent.Visibility = System.Windows.Visibility.Visible;
            borderNewCustomer.Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion

        #region Properties

        public gip.tool.devLicenseProvider.LicenseProvider Provider
        {
            get;
            set;
        }

        private gip.tool.devLicenseProvider.Customer _SelectedCustomer;
        public gip.tool.devLicenseProvider.Customer SelectedCustomer
        {
            get
            {
                return _SelectedCustomer;
            }
            set
            {
                _SelectedCustomer = value;
                OnPropertyChanged("SelectedCustomer");
            }
        }

        #endregion

        #region Methods

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCustomer == null)
                return;

            using(var ctx = new GIPLicenseEntities())
            {
                var cust = ctx.Customer.FirstOrDefault(c => c.CustomerID == SelectedCustomer.CustomerID);
                cust.Address = SelectedCustomer.Address;
                ctx.SaveChanges();
            }
        }

        private void btnNewCustomer_Click(object sender, RoutedEventArgs e)
        {
            gridContent.Visibility = System.Windows.Visibility.Collapsed;
            borderNewCustomer.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnCreateNewCustomer_Click(object sender, RoutedEventArgs e)
        {
            string message = Provider.CreateNewCustomer(tbCustomerName.Text.Trim(), tbCustomerAddress.Text.Trim());
            if (message != null)
            {
                MessageBox.Show(message, "Warning");
                return;
            }
            gridContent.Visibility = System.Windows.Visibility.Visible;
            borderNewCustomer.Visibility = System.Windows.Visibility.Hidden;
        }

        private void miDelCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(string.Format("Are you sure you want delete customer {0}?", SelectedCustomer.CustomerName), "Are you sure", MessageBoxButton.YesNo, MessageBoxImage.Question) 
                == MessageBoxResult.Yes)
            {
                Provider.DeleteCustomer(SelectedCustomer);
            }
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
