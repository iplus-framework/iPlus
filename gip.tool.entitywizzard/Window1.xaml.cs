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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Windows.Forms;
using gip.tool.entitywizzard.Properties;
using System.Data.Linq;

namespace EntityWizzard
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            if (Settings.Default.FileOpened !=null && Settings.Default.FileOpened.Count > 0)
             {
                 RemoveDuplicates();
                 var arrayOfOpenedFiles = Settings.Default.FileOpened;
                 //var list = arrayOfOpenedFiles.Cast<string>().ToList();
                 //list = list.Distinct().ToList();
                    
                 foreach(var item in arrayOfOpenedFiles)
                 {
                     System.Windows.Controls.Button btn = new System.Windows.Controls.Button();
                     btn.Content = item.ToString();
                     btn.Click += btn_Click;
                     btnList.Items.Add(btn);
                 }
             }
        }

        private void RemoveDuplicates()
        {
            var arrayOfOpenedFiles = Settings.Default.FileOpened;
            var list = arrayOfOpenedFiles.Cast<string>().ToList();
            list = list.Distinct().ToList();
            arrayOfOpenedFiles = new System.Collections.Specialized.StringCollection();
            arrayOfOpenedFiles.AddRange(list.ToArray());
            Settings.Default.FileOpened = arrayOfOpenedFiles;
            Settings.Default.Save();
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = (((System.Windows.Controls.Button)sender).Content).ToString();
            EntityParser entityParser = new EntityParser();
            entityParser.UpdateMapping(fileName);
            Close();
        }

        private void updateEntityModell_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "ADO.NET Entity Data Model Designer (*.edmx)|*.edmx";

            //dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\gip.core.datamodel\\iPlusV4.edmx";
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] dirParts = baseDirectory.Split('\\');
            baseDirectory = "";
            for (int i = 0; i < dirParts.Count() - 4; i++)
            {
                baseDirectory += dirParts[i] + "\\";
            }
            baseDirectory += "gip.core.datamodel\\";

            dlg.FileName = "iPlusV4.edmx";
            dlg.InitialDirectory = baseDirectory;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && !String.IsNullOrEmpty(dlg.FileName))
            {
                EntityParser entityParser = new EntityParser();
                entityParser.UpdateMapping(dlg.FileName);
                if (Settings.Default.FileOpened == null)
                {
                    Settings.Default.FileOpened = new System.Collections.Specialized.StringCollection();
                }
                if(!Settings.Default.FileOpened.Contains(dlg.FileName))
                Settings.Default.FileOpened.Add(dlg.FileName);
                if(Settings.Default.FileOpened.Count > 5)
                Settings.Default.FileOpened.RemoveAt(0);
                Settings.Default.Save();
                Close();
            }
        }
    }
}
