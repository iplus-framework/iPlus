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
    /// Interaktionslogik für VideoDialog.xaml
    /// </summary>
    public partial class VideoDialog : VBWindowDialog
    {
        public VideoDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }
        public string Res = "Cancel";
        public int w;

        public int h;
        private void Window_Loaded(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Res = "OK";
            Close();
        }

        private void TextBox1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                w = Convert.ToInt32(TextBox1.Value);
            }
            catch (Exception ec)
            {
                TextBox1.Value = 1;

                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null &&
                                                                      gip.core.datamodel.Database.Root.InitState == gip.core.datamodel.ACInitState.Initialized)
                    gip.core.datamodel.Database.Root.Messages.LogException("Document.Editor.VideoDialog", "TextBox1_ValueChanged", msg);
            }
            try
            {
                h = Convert.ToInt32(TextBox2.Text);
            }
            catch (Exception ec)
            {
                TextBox2.Value = 1;

                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null &&
                                                                      gip.core.datamodel.Database.Root.InitState == gip.core.datamodel.ACInitState.Initialized)
                    gip.core.datamodel.Database.Root.Messages.LogException("Document.Editor.VideoDialog", "TextBox1_ValueChanged(10)", msg);
            }
            if (TextBox1.Value > 0 && TextBox2.Value > 0)
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
