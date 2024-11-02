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
    /// Interaktionslogik für TimeDialog.xaml
    /// </summary>
    public partial class TimeDialog : VBWindowDialog
    {
        public TimeDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += TimeDialog_Loaded;
        }
        public string Res = "Cancel";

        public string Time = null;
        private void OKButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            Res = "OK";
            Close();
        }

        private void RadioButton12_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AMPMCheckBox != null)
            {
                AMPMCheckBox.IsEnabled = true;
            }
        }

        private void RadioButton12_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            AMPMCheckBox.IsEnabled = false;
        }

        private void TimeDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
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
