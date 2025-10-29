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
using System.Collections;
using System.Data;
using System.Diagnostics;
//using System.Windows.Controls.DataVisualization;
//using System.Windows.Controls.DataVisualization.Charting;
using gip.core.layoutengine.avui;

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für ChartDialog.xaml
    /// </summary>
    public partial class ChartDialog : VBWindowDialog
    {
        public ChartDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += ChartDialog_Loaded;
        }
        public string Res = "Cancel";
        //private ColumnSeries Series;

        private Dictionary<string, int> Items = new Dictionary<string, int>();
        #region "Loaded"

        private void ChartDialog_Loaded(object sender, RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
            Items.Add("Item1", 1);
            Items.Add("Item2", 2);
            Items.Add("Item3", 3);
            //LoadColumnData(PreviewChart.Series[0]);
            //LoadPieData(PreviewChart.Series[1]);
            //PreviewChart.Series.Remove(PieSeries);
            ItemsListBox.ItemsSource = Items;
        }

        private void ColumnSeries_Loaded(object sender, RoutedEventArgs e)
        {
            //LoadColumnData(ColumnSeries);
        }

        private void PieSeries_Loaded(object sender, RoutedEventArgs e)
        {
            //LoadPieData(PieSeries);
        }

        #endregion

        #region "Chart Editor"

        //private void LoadColumnData(ISeries series)
        //{
        //    ((ColumnSeries)series).ItemsSource = Items;
        //}

        //private void LoadPieData(ISeries series)
        //{
        //    ((PieSeries)series).ItemsSource = Items;
        //}

        //private void UpdatePreview()
        //{
        //    if (ChartTypeComboBox.SelectedIndex == 0)
        //    {
        //        PreviewChart.Series.Remove(PieSeries);
        //        PreviewChart.Series.Add(ColumnSeries);
        //        LoadColumnData(ColumnSeries);
        //    }
        //    else if (ChartTypeComboBox.SelectedIndex == 1)
        //    {
        //        PreviewChart.Series.Remove(ColumnSeries);
        //        PreviewChart.Series.Add(PieSeries);
        //        LoadPieData(PieSeries);
        //    }
        //}

        #region "Chart"

        private void ChartTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                //UpdatePreview();
            }
        }

        private void ChartTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                //PreviewChart.Title = ChartTitleTextBox.Text;
            }
        }

        private void ForegroundColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                //SolidColorBrush co = new SolidColorBrush();
                //co.Color = ForegroundColorGallery.SelectedColor.Value;
                //PreviewChart.Foreground = co;
            }
        }

        private void BackgroundColorGallery_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                //SolidColorBrush co = new SolidColorBrush();
                //co.Color = BackgroundColorGallery.SelectedColor.Value;
                //PreviewChart.Background = co;
            }
        }

        private void ChartHight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                //PreviewChart.Height = ChartHight.Value;
            }
        }

        private void ChartWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
            {
                //PreviewChart.Width = ChartWidth.Value;
            }
        }

        #endregion

        #region "Series"

        private void SeriesTitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsLoaded)
            {
                //ISeries s = PreviewChart.Series[0];
                //if (s is ColumnSeries)
                //{
                //    ColumnSeries series = s as ColumnSeries;
                //    series.Title = SeriesTitleTextBox.Text;
                //}
                //else if (s is PieSeries)
                //{
                //    PieSeries series = s as PieSeries;
                //    series.Title = SeriesTitleTextBox.Text;
                //}
            }
        }

        #endregion

        #region "Items"

        private void ItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsListBox.SelectedItem != null)
            {
                RemoveItemButton.IsEnabled = true;
                ItemTitleTextBox.Visibility = Visibility.Visible;
                ItemValueBox.Visibility = Visibility.Visible;
                KeyValuePair<string, int> i = (KeyValuePair<string, int>)ItemsListBox.SelectedItem;
                ItemTitleTextBox.Text = i.Key;
                IsEditing = false;
                ItemValueBox.Value = i.Value;
                IsEditing = true;
            }
            else
            {
                RemoveItemButton.IsEnabled = false;
                ItemTitleTextBox.Visibility = Visibility.Collapsed;
                ItemValueBox.Visibility = Visibility.Collapsed;
            }
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            Items.Add("Item" + Convert.ToString(Items.Count + 1), 1);
            ItemsListBox.ItemsSource = null;
            ItemsListBox.ItemsSource = Items;
            //ColumnSeries.ItemsSource = null;
            //LoadColumnData(ColumnSeries);
            //PieSeries.ItemsSource = null;
            //LoadPieData(PieSeries);
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            Items.Remove(((KeyValuePair<string, int>)ItemsListBox.SelectedItem).Key);
            ItemsListBox.ItemsSource = null;
            ItemsListBox.ItemsSource = Items;
            //ColumnSeries.ItemsSource = null;
            //LoadColumnData(ColumnSeries);
            //PieSeries.ItemsSource = null;
            //LoadPieData(PieSeries);
        }

        private void ItemTitleTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //KeyValuePair<string, int> i = (KeyValuePair<string, int>)ItemsListBox.SelectedItem;
                //dynamic newi = new KeyValuePair<string, int>(ItemTitleTextBox.Text, i.Value);
                //Dictionary<string, int> c = new Dictionary<string, int>();
                //foreach (KeyValuePair<string, int> it in Items)
                //{
                //    if (object.ReferenceEquals(it.Key, i.Key))
                //    {
                //        c.Add(newi.Key, newi.Value);
                //    }
                //    else
                //    {
                //        c.Add(it.Key, it.Value);
                //    }
                //}
                //Items = c;
                //ItemsListBox.ItemsSource = null;
                //ItemsListBox.ItemsSource = Items;
                //ColumnSeries.ItemsSource = null;
                //LoadColumnData(ColumnSeries);
                //PieSeries.ItemsSource = null;
                //LoadPieData(PieSeries);
                //e.Handled = true;
            }
        }

        private bool IsEditing = true;
        private void ItemValueBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && IsEditing)
            {
                //KeyValuePair<string, int> i = (KeyValuePair<string, int>)ItemsListBox.SelectedItem;
                //Dictionary<string, int> c = new Dictionary<string, int>();
                //foreach (KeyValuePair<string, int> it in Items)
                //{
                //    if (object.ReferenceEquals(it.Key, i.Key) && it.Value == i.Value)
                //    {
                //        c.Add(i.Key, System.Convert.ToInt32(ItemValueBox.Value));
                //    }
                //    else
                //    {
                //        c.Add(it.Key, it.Value);
                //    }
                //}
                //Items = c;
                //ItemsListBox.ItemsSource = null;
                //ItemsListBox.ItemsSource = Items;
                //ColumnSeries.ItemsSource = null;
                //LoadColumnData(ColumnSeries);
                //PieSeries.ItemsSource = null;
                //LoadPieData(PieSeries);
            }
        }

        #endregion

        #endregion

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Res = "OK";
            Close();
        }

    }
}
