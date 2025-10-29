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
using System.IO;
using System.Windows.Markup;

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für SaveFileDialog.xaml
    /// </summary>
    public partial class SaveFileDialog : Window
    {
        public SaveFileDialog()
        {
            InitializeComponent();
            Loaded += SaveFileDialog_Loaded;
            Closing += SaveFileDialog_Closing;
        }
        public string Res = null;
        public void SetFileInfo(string name, RichTextBox RTB)
        {
            Label1.Content = "Do you want to save " + name + "?";
            RichTextBox1.AppendText("");
        }

        #region "Buttons"

        private void YesButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            Res = "Yes";
            Close();
        }

        private void NoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Res = "No";
            Close();
        }

        private void CancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Res = null;
            Close();
        }

        #endregion

        private void SaveFileDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.SaveDialog_IsMax = true;
            }
            else
            {
                Properties.Settings.Default.SaveDialog_IsMax = false;
            }
        }

        private void SaveFileDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (Properties.Settings.Default.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
            try
            {
                FileStream fs = File.OpenRead(System.IO.Path.GetTempPath() + "\\TVPre.xaml");
                TextRange tr = new TextRange(RichTextBox1.Document.ContentStart, RichTextBox1.Document.ContentEnd);
                FlowDocument content = XamlReader.Load(fs) as FlowDocument;
                RichTextBox1.Document = content;
                fs.Close();
                if (Properties.Settings.Default.SaveDialog_IsMax)
                {
                    this.WindowState = WindowState.Maximized;
                }
                else
                {
                    this.WindowState = WindowState.Normal;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
