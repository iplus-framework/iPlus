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

namespace gip.tool.publish
{
    /// <summary>
    /// Interaction logic for PassToClear.xaml
    /// </summary>
    public partial class ClearPass : Window
    {
        public ClearPass()
        {
            InitializeComponent();
        }

        public string ShowPassClearDialog()
        {
            string result = "";

            ShowDialog();

            result = TBpassToClear.Text;

            return result;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
