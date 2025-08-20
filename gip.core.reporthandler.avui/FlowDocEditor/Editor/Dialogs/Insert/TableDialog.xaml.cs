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
    /// Interaktionslogik für TableDialog.xaml
    /// </summary>
    public partial class TableDialog : VBWindowDialog
    {
        public TableDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += TableDialog_Loaded;
        }
        public string Res = "Cancel";
        public SolidColorBrush BackgroundColor = Brushes.Transparent;
        public SolidColorBrush CellBackgroundColor = Brushes.Transparent;
        public SolidColorBrush BorderColor = Brushes.Transparent;

        public SolidColorBrush CellBorderColor = Brushes.Black;
        private void UpdatePreview()
        {
            Table t = new Table();
            int @int = Convert.ToInt32(RowsTextBox.Value);
            int int2 = Convert.ToInt32(CellsTextBox.Value);
            while (!(@int == 0))
            {
                TableRowGroup trg = new TableRowGroup();
                TableRow tr = new TableRow();
                while (!(int2 == 0))
                {
                    TableCell tc = new TableCell();
                    tc.Background = CellBackgroundColor;
                    tc.BorderBrush = CellBorderColor;
                    tc.BorderThickness = new Thickness(1, 1, 1, 1);
                    tr.Cells.Add(tc);
                    int2 -= 1;
                }
                int2 = Convert.ToInt32(CellsTextBox.Value);
                trg.Rows.Add(tr);
                t.RowGroups.Add(trg);
                @int -= 1;
            }
            t.Background = BackgroundColor;
            t.BorderBrush = BorderColor;
            t.BorderThickness = new Thickness(1, 1, 1, 1);
            FlowDocument flowdoc = new FlowDocument();
            flowdoc.Blocks.Add(t);
            PreviewBox.Document = flowdoc;
        }

        private void TableDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
            UpdatePreview();
            RowsTextBox.Focus();
        }

        private void RowsTextBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                UpdatePreview();
            }
        }

        private void BorderColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            BorderColor = new SolidColorBrush(BorderColorGallery.SelectedColor.Value);
            UpdatePreview();
        }

        private void CellBorderColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            CellBorderColor = new SolidColorBrush(CellBorderColorGallery.SelectedColor.Value);
            UpdatePreview();
        }

        private void BackgroundColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            BackgroundColor = new SolidColorBrush(BackgroundColorGallery.SelectedColor.Value);
            UpdatePreview();
        }

        private void CellBackgroundColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            CellBackgroundColor = new SolidColorBrush(CellBackgroundColorGallery.SelectedColor.Value);
            UpdatePreview();
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Res = "OK";
            Close();
        }
    }
}
