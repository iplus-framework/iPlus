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
    /// Interaktionslogik für DateDialog.xaml
    /// </summary>
    public partial class DateDialog : VBWindowDialog
    {
        public DateDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += DateDialog_Loaded;
        }
        public string Res = "Cancel";
        private void DateDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
            ListBox1.Items.Add(System.DateTime.Now.ToString("M/dd/yyyy"));
            ListBox1.Items.Add(System.DateTime.Now.ToString("M/dd/yy"));
            ListBox1.Items.Add(System.DateTime.Now.ToString("MM/dd/yy"));
            ListBox1.Items.Add(System.DateTime.Now.ToString("MM/dd/yyyy"));
            ListBox1.Items.Add(System.DateTime.Now.ToString("yy/MM/dd"));
            ListBox1.Items.Add(System.DateTime.Now.ToString("yyyy-MM-dd"));
            ListBox1.Items.Add(System.DateTime.Now.ToString("dd-MMM-yy"));
            ListBox1.Items.Add(System.DateTime.Now.ToString("dddd, MMMM dd, yyyy"));
            ListBox1.Items.Add(System.DateTime.Now.ToString("MMMM dd, yyyy"));
            ListBox1.Items.Add(System.DateTime.Now.ToString("dddd, dd MMMM, yyyy"));
            ListBox1.Items.Add(System.DateTime.Now.ToString("dd MMMM, yyyy"));

            ListBox1.SelectedIndex = 0;
            ListBox1.Focus();
        }

        private void OKButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            Res = "OK";
            Close();
        }
    }
}
