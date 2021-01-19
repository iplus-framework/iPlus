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
    /// Interaktionslogik für FindDialog.xaml
    /// </summary>
    public partial class FindDialog : VBWindowDialog
    {
        public FindDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
        }
        public string Res = "Cancel";

        public FindDialog(FlowDocument document, DependencyObject caller) : base(caller)
        {
            Loaded += FindDialog_Loaded;
            // This call is required by the designer.
            InitializeComponent();
            // Add any initialization after the InitializeComponent() call.

        }

        private void OKButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            Res = "OK";
            Close();
        }

        private void FindDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
            TextBox1.Focus();
        }

        private void TextBox1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (OKButton.IsEnabled)
                {
                    OKButton_Click(null, null);
                }
            }
        }

        private void TextBox1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TextBox1.Text.Length > 0)
            {
                OKButton.IsEnabled = true;
            }
            else
            {
                OKButton.IsEnabled = false;
            }
        }

    }
}
