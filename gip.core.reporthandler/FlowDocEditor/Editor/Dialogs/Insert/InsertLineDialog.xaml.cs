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
    /// Interaktionslogik für InsertLineDialog.xaml
    /// </summary>
    public partial class InsertLineDialog : VBWindowDialog
    {
        public InsertLineDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += InsertLineDialog_Loaded;
        }
        public string Res = "Cancel";

        public int h;
        //Private Sub TextBox1_TextChanged(ByVal sender As Object, ByVal e As System.Windows.Controls.TextChangedEventArgs) Handles TextBox1.TextChanged
        //    Try
        //        h = Convert.ToInt32(TextBox1.Text)
        //    Catch ex As Exception
        //        TextBox1.Clear()
        //    End Try
        //    If TextBox1.Text.Length > 0 Then
        //        OKButton.IsEnabled = True
        //    Else
        //        OKButton.IsEnabled = False
        //    End If
        //End Sub

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            h = Convert.ToInt32(TextBox1.Value);
            Res = "OK";
            Close();
        }

        private void InsertLineDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
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
    }
}
