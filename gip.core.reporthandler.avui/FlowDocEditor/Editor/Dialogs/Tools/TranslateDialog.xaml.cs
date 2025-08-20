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
//using MS.WindowsAPICodePack.Internal.CoreHelpers;
using gip.core.datamodel;
using gip.core.layoutengine.avui;

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für TranslateDialog.xaml
    /// </summary>
    public partial class TranslateDialog : VBWindowDialog
    {
        public TranslateDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
        }
        private string intContent;

        public bool res = false;
        public TranslateDialog(string s, DependencyObject caller) : base(caller)
        {
            Loaded += TranslateDialog_Loaded;
            InitializeComponent();
            intContent = s;
        }

        private void TranslateButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                //if (My.Computer.Network.IsAvailable)
                //{
                //    Microsoft.DetectedLanguage froml = new Microsoft.DetectedLanguage();
                //    Microsoft.Language tol = new Microsoft.Language();
                //    froml.Code = FromBox.Items[FromBox.SelectedIndex].ToString();
                //    tol.Code = ToBox.Items[ToBox.SelectedIndex].ToString();
                //    Semagsoft.Translator.TranslatorHelper translator = new Semagsoft.Translator.TranslatorHelper();
                //    string transres = translator.Translate(intContent, froml, tol);
                //    TranslatedText.Content = transres;
                //    OKButton.IsEnabled = true;
                //}
                //else
                //{
                //    MessageBoxDialog m = new MessageBoxDialog("No Internet Found", "Error", null, null);
                //    m.MessageImage.Source = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/error32.png", UriKind.Relative));
                //    m.Owner = this;
                //    m.ShowDialog();
                //}
            }
            catch (Exception ex)
            {
                VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = ex.Message, MessageLevel = eMsgLevel.Error }, eMsgButton.OK, this);
                vbMessagebox.ShowMessageBox();

                //MessageBoxDialog m = new MessageBoxDialog(ex.Message, ex.Message, null, 0);
                //m.MessageImage.Source = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/error32.png", UriKind.Relative));
                //m.Owner = this;
                //m.ShowDialog();
            }
        }

        private void OKButton_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            res = true;
            Close();
        }

        private void TranslateDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
        }

    }
}
