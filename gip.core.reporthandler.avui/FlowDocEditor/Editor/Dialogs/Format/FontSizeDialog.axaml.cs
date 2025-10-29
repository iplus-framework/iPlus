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
    /// Interaktionslogik für FontSizeDialog.xaml
    /// </summary>
    public partial class FontSizeDialog : VBWindowDialog
    {
        public FontSizeDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += FontSizeDialog_Loaded;
        }
        public string Res = "Cancel";

        public double Number = new double();
        private void SizeBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OKButton_Click(null, null);
            }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Number = Convert.ToDouble(SizeBox.Text);
            Res = "OK";
            Close();
        }

        private void FontSizeDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
            SizeBox.Focus();
        }
    }
}
