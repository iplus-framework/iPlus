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
    /// Interaktionslogik für ShapeDialog.xaml
    /// </summary>
    public partial class ShapeDialog : VBWindowDialog
    {
        public ShapeDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += ShapeDialog_Loaded;
        }
        public string Res = "Cancel";

        public Shape Shape = null;
        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Res = "OK";
            Close();
        }

        private void ShapeDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
            TypeComboBox.Items.Add("Circle");
            TypeComboBox.Items.Add("Square");
            TypeComboBox.SelectedIndex = 0;
            if (TypeComboBox.SelectedIndex == 0)
            {
                Shape = new Ellipse();
                Shape.Height = 32;
                Shape.Width = 32;
                int int2 = Convert.ToInt32(BorderSizeTextBox.Value);
                Shape.StrokeThickness = int2;
                Shape.Stroke = Brushes.Black;
            }
            else if (TypeComboBox.SelectedIndex == 1)
            {
                Shape = new Rectangle();
                Shape.Height = 32;
                Shape.Width = 32;
                int int2 = Convert.ToInt32(BorderSizeTextBox.Value);
                Shape.StrokeThickness = int2;
                Shape.Stroke = Brushes.Black;
            }
            ScrollViewer1.Content = Shape;
        }

        private void TypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TypeComboBox.SelectedIndex == 0)
            {
                Shape = new Ellipse();
                int @int = Convert.ToInt32(SizeTextBox.Value);
                Shape.Height = @int;
                Shape.Width = @int;
                int int2 = Convert.ToInt32(BorderSizeTextBox.Value);
                Shape.StrokeThickness = int2;
                Shape.Stroke = Brushes.Black;
            }
            else if (TypeComboBox.SelectedIndex == 1)
            {
                Shape = new Rectangle();
                int @int = Convert.ToInt32(SizeTextBox.Value);
                Shape.Height = @int;
                Shape.Width = @int;
                int int2 = Convert.ToInt32(BorderSizeTextBox.Value);
                Shape.StrokeThickness = int2;
                Shape.Stroke = Brushes.Black;
            }
            ScrollViewer1.Content = Shape;
        }

        private void SizeTextBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int @int = Convert.ToInt32(SizeTextBox.Value);
                if (Shape != null)
                {
                    Shape.Height = @int;
                    Shape.Width = @int;
                }
            }
            catch (Exception ec)
            {
                SizeTextBox.Value = 32;

                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null && 
                                                                      gip.core.datamodel.Database.Root.InitState == gip.core.datamodel.ACInitState.Initialized)
                    gip.core.datamodel.Database.Root.Messages.LogException("Document.Editor.ShapeDialog", "SizeTextBox_ValueChanged", msg);
            }
        }

        private void BorderSizeTextBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int @int = Convert.ToInt32(BorderSizeTextBox.Value);
                Shape.StrokeThickness = @int;
            }
            catch (Exception ec)
            {
                BorderSizeTextBox.Value = 4;

                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null &&
                                                                      gip.core.datamodel.Database.Root.InitState == gip.core.datamodel.ACInitState.Initialized)
                    gip.core.datamodel.Database.Root.Messages.LogException("Document.Editor.ShapeDialog", "BorderSizeTextBox_ValueChanged", msg);
            }
        }
    }
}
