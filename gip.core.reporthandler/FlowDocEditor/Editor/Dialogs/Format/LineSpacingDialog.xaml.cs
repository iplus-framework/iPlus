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
    /// Interaktionslogik für LineSpacingDialog.xaml
    /// </summary>
    public partial class LineSpacingDialog : VBWindowDialog
    {
        public LineSpacingDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += LineSpacingDialog_Loaded;
        }
        public string Res = "Cancel";

        public double number;
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

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Res = "OK";
            Close();
        }

        private void LineSpacingDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
            TextBox1.Value = number;
            TextBox1.Focus();
        }

        private void TextBox1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                number = TextBox1.Value;
            }
            catch (Exception ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null &&
                                                                      gip.core.datamodel.Database.Root.InitState == gip.core.datamodel.ACInitState.Initialized)
                    gip.core.datamodel.Database.Root.Messages.LogException("Document.Editor.LineSpacingDialog", "TextBox1_ValueChanged", msg);
            }
        }
    }
}
