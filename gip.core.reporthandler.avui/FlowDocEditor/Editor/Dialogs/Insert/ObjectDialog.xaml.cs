// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
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
using gip.core.layoutengine.avui;

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für ObjectDialog.xaml
    /// </summary>
    public partial class ObjectDialog : VBWindowDialog
    {
        public ObjectDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += ObjectDialog_Loaded;
        }
        public string Res = null;
        public int OW = 96;
        public int OH = 24;

        public string OT = "Text";
        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Res = null;
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (O_Button.IsChecked.Value)
            {
                Res = "button";
            }
            else if (O_RadioButton.IsChecked.Value)
            {
                Res = "radiobutton";
            }
            else if (O_CheckBox.IsChecked.Value)
            {
                Res = "checkbox";
            }
            else if (O_TextBlock.IsChecked.Value)
            {
                Res = "textblock";
            }
            Close();
        }

        private void Button1_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            ObjectPropertiesDialog op = new ObjectPropertiesDialog(OW, OH, OT, this);
            op.Owner = this;
            op.ShowDialog();
            OW = Convert.ToInt32(op.WBox.Value);
            OH = Convert.ToInt32(op.HBox.Value);
            OT = op.TxtBox.Text;
        }

        private void ObjectDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
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
