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
using System.Diagnostics;

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für StartupDialog.xaml
    /// </summary>
    public partial class StartupDialog : Window
    {
        public StartupDialog()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }
        private void Window_Loaded(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (Properties.Settings.Default.Options_EnableGlass)
            //    {
            //       AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
            if (Properties.Settings.Default.Options_ShowStartupDialog)
            {
                ShowOnStartupCheckBox.IsChecked = true;
            }
            else
            {
                ShowOnStartupCheckBox.IsChecked = false;
            }
        }

        private void OnlineHelpButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("http://documenteditor.net/documentation/");
        }

        private void GetPluginsButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("http://documenteditor.net/plugins");
        }

        private void WebsiteButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start("http://documenteditor.net");
        }

        private void CloseButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (ShowOnStartupCheckBox.IsChecked.Value)
            {
                Properties.Settings.Default.Options_ShowStartupDialog = true;
            }
            else
            {
                Properties.Settings.Default.Options_ShowStartupDialog = false;
            }
            Close();
        }
    }
}
