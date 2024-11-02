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
using gip.core.layoutengine;

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für LinkDialog.xaml
    /// </summary>
    public partial class LinkDialog : VBWindowDialog
    {
        public LinkDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += LinkDialog_Loaded;
        }
        public string Res = "Cancel";

        public string Link = null;
        private void OKButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            Link = TextBox1.Text;
            Res = "OK";
            Close();
        }

        private void TextBox1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && TextBox1.Text.Length > 0)
            {
                OKButton_Click(null, null);
            }
        }

        private void TextBox1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TextBox1.Text != null)
            {
                OKButton.IsEnabled = true;
            }
            else
            {
                OKButton.IsEnabled = false;
            }
        }

        private void LinkDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
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
