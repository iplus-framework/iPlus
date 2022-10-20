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
    /// Interaktionslogik für ObjectPropertiesDialog.xaml
    /// </summary>
    public partial class ObjectPropertiesDialog : VBWindowDialog
    {
        public ObjectPropertiesDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
        }
        public ObjectPropertiesDialog(int w, int h, string txt, DependencyObject caller) : base(caller)
        {
            Loaded += ObjectPropertiesDialog_Loaded;
            InitializeComponent();
            WBox.Value = w;
            HBox.Value = h;
            TxtBox.Text = txt;
        }

        private void OKButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }

        private void ObjectPropertiesDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
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
