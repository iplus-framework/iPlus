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
using gip.core.datamodel;
using gip.core.layoutengine.avui;

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für GoToDialog.xaml
    /// </summary>
    public partial class GoToDialog : VBWindowDialog
    {
        public GoToDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += GoToDialog_Loaded;
        }
        public string Res = "Cancel";

        public int line = 1;
        private void OKButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            //line = Convert.ToInt16(LineNumberBox.Value)
            Res = "OK";
            Close();
        }

        private void GoToDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
            LineNumberBox.Focus();
            LineNumberBox_ValueChanged(null, null);
        }

        private void LineNumberBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && OKButton.IsEnabled)
            {
                OKButton_Click(null, null);
            }
        }

        private void LineNumberBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                line = Convert.ToInt32(LineNumberBox.Value);
                // - 1

            }
            catch (Exception ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("Document.Editor.GoToDialog", "LineNumberBox_ValueChanged", msg);
            }
        }
    }
}
