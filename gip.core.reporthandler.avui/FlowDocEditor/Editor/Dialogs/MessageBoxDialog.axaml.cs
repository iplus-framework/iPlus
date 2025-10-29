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

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für MessageBoxDialog.xaml
    /// </summary>
    public partial class MessageBoxDialog : Window
    {
        public MessageBoxDialog()
        {
            InitializeComponent();
        }
        public string Result = "Cancel";
        public MessageBoxDialog(string text, string title, string buttons, int icon)
        {
            // This call is required by the designer.
            InitializeComponent();
            // Add any initialization after the InitializeComponent() call.
            MessageBoxText.Text = text;
            this.Title = title;
            if (buttons == "YesNo")
            {
                OKButton.Visibility = Visibility.Collapsed;
                YesButton.Visibility = Visibility.Visible;
                NoButton.Visibility = Visibility.Visible;
                YesButton.IsDefault = true;
            }
            else if (buttons == "YesNoCancel")
            {
                OKButton.Visibility = Visibility.Collapsed;
                YesButton.Visibility = Visibility.Visible;
                NoButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;
                YesButton.IsDefault = true;
            }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Result = "OK";
            DialogResult = true;
            Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = "Yes";
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = "No";
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = "Cancel";
            DialogResult = true;
            Close();
        }
    }
}
