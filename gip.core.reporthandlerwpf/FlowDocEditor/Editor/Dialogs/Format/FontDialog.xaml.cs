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
using gip.core.layoutengine;

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für FontDialog.xaml
    /// </summary>
    public partial class FontDialog : VBWindowDialog
    {
        public FontDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += FontDialog_Loaded;
        }
        public string Res = "Cancel";

        public FontFamily font = new FontFamily();
        private void OKButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            font = FontListBox.SelectedItem as FontFamily;
            Res = "OK";
            Close();
        }

        private void FontListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FontListBox.SelectedItem != null)
            {
                OKButton.IsEnabled = true;
            }
            else
            {
                OKButton.IsEnabled = false;
            }
        }

        private void FontDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
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
