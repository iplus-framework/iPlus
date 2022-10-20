using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für LoadFileDialog.xaml
    /// </summary>
    public partial class LoadFileDialog : Window
    {
        public LoadFileDialog()
        {
            InitializeComponent();
            Loaded += LoadFileDialog_Loaded;
            Closing += LoadFileDialog_Closing;
        }
        public bool i = false;
        private void LoadFileDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (i == false)
            {
                e.Cancel = true;
            }
        }

        private void LoadFileDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
        }
    }
}
