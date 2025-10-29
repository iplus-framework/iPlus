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
    /// Interaktionslogik für SpellCheckDialog.xaml
    /// </summary>
    public partial class SpellCheckDialog : VBWindowDialog
    {
        public SpellCheckDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += SpellCheckDialog_Loaded;
        }
        public string Res = "Cancel";
        private void SpellCheckDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
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

        private void WordListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter & WordListBox.SelectedItem != null)
            {
                OKButton_Click(null, null);
            }
        }

        private void WordListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (WordListBox.SelectedItem != null)
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
