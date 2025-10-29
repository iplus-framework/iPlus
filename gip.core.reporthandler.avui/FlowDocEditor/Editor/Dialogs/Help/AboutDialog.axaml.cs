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
using System.Net;
using System.ComponentModel;
using System.Threading;
using System.IO;
using gip.core.layoutengine.avui;

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : VBWindowDialog
    {
        public AboutDialog(DependencyObject caller) : base(caller)
        {
            InitializeComponent();
            Loaded += AboutDialog_Loaded;
        }
        private BitmapImage licenseicon = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Help/license16.png", UriKind.Relative));

        private BitmapImage backicon = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Help/back16.png", UriKind.Relative));
        #region "Reuseable Code"

        public bool PathExists(string path, int timeout)
	    {
            return false;
	        //bool exists = true;
	        //Thread t = new Thread(() => CheckPathFunction(path));
	        //t.Start();
	        //bool completed = t.Join(timeout);
	        //if (!completed) {
		       // exists = false;
		       // t.Abort();
	        //}
	        //return exists;
	    }

        public bool CheckPathFunction(string path)
        {
            return File.Exists(path);
        }

        #endregion

        #region "Loaded"

        private void AboutDialog_Loaded(Object sender, RoutedEventArgs e)
        {
            //TextBox1.Text = My.Resources.License;
            //AppNameLabel.Content = My.Application.Info.ProductName + " " + My.Application.Info.Version.Major.ToString;
            //VersionLabel.Content = "Version: " + My.Application.Info.Version.Major.ToString + "." + My.Application.Info.Version.Minor.ToString;
            //CopyLabel.Content = My.Application.Info.Copyright.ToString + " By Semagsoft";
            //if (My.Computer.Info.OSVersion >= "6.0")
            //{
            //    if (My.Settings.Options_EnableGlass)
            //    {
            //        AppHelper.ExtendGlassFrame(this, new Thickness(-1, -1, -1, -1));
            //    }
            //}
        }

        #endregion

        #region "License"

        private void LicenseButton_Click(Object sender, RoutedEventArgs e)
        {
            if (TextBox1.IsVisible)
            {
                TextBox1.Visibility = Visibility.Hidden;
                UpdateButton.Visibility = Visibility.Visible;
                LicenseButton.Icon = licenseicon;
                LicenseButton.Header = "License";
            }
            else
            {
                TextBox1.Visibility = Visibility.Visible;
                UpdateButton.Visibility = Visibility.Collapsed;
                LicenseButton.Icon = backicon;
                LicenseButton.Header = "Back";
            }
        }

        #endregion

        #region "Check For Update"

        private BackgroundWorker withEventsField_CheckForUpdateWorker = new BackgroundWorker();
        private BackgroundWorker CheckForUpdateWorker
        {
            get { return withEventsField_CheckForUpdateWorker; }
            set
            {
                if (withEventsField_CheckForUpdateWorker != null)
                {
                    withEventsField_CheckForUpdateWorker.DoWork -= CheckForUpdateWorker_DoWork;
                    withEventsField_CheckForUpdateWorker.RunWorkerCompleted -= CheckForUpdateWorker_RunWorkerCompleted;
                }
                withEventsField_CheckForUpdateWorker = value;
                if (withEventsField_CheckForUpdateWorker != null)
                {
                    withEventsField_CheckForUpdateWorker.DoWork += CheckForUpdateWorker_DoWork;
                    withEventsField_CheckForUpdateWorker.RunWorkerCompleted += CheckForUpdateWorker_RunWorkerCompleted;
                }
            }

        }
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            //if (My.Computer.Network.IsAvailable)
            //{
            //    AppLogo.Visibility = Visibility.Hidden;
            //    LicenseButton.Visibility = Visibility.Hidden;
            //    OKButton.Visibility = Visibility.Hidden;
            //    UpdateButton.Visibility = Visibility.Hidden;
            //    UpdateBox.Visibility = Visibility.Visible;
            //    CheckForUpdateWorker.RunWorkerAsync();
            //}
            //else
            //{
            //    MessageBoxDialog m = new MessageBoxDialog("Internet not found.", "Error", null, null);
            //    m.MessageImage.Source = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/error32.png", UriKind.Relative));
            //    m.Owner = this;
            //    m.ShowDialog();
            //}
        }

        private void CheckForUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = true;
            //try
            //{
            //    if (PathExists("http://documenteditor.net/updatechecker.ini", 5000))
            //    {
            //        WebClient fileReader = new WebClient();
            //        My.Computer.FileCreateDirectory(My.Computer.FileSpecialDirectories.Temp + "\\Semagsoft\\DocumentEditor\\");
            //        string filename = My.Computer.FileSpecialDirectories.Temp + "\\Semagsoft\\DocumentEditor\\updatechecker.ini";
            //        if (File.Exists(filename))
            //        {
            //            File.Delete(filename);
            //        }
            //        fileReader.DownloadFile(new Uri("http://documenteditor.net/updatechecker.ini"), filename);
            //        e.Result = true;
            //    }
            //    else
            //    {
            //        e.Result = false;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, ex.ToString, MessageBoxButton.OK, MessageBoxImage.Error);
            //    e.Result = false;
            //}
        }

        private void CheckForUpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //if (e.Result == true)
            //{
            //    TextReader textreader = File.OpenText(My.Computer.FileSpecialDirectories.Temp + "\\Semagsoft\\DocumentEditor\\updatechecker.ini");
            //    string version = textreader.ReadLine;
            //    int versionyear = Convert.ToInt16(version.Substring(0, 4));
            //    int versionnumber = Convert.ToInt16(version.Substring(5));
            //    if (versionyear >= My.Application.Info.Version.Major && versionnumber > My.Application.Info.Version.Minor)
            //    {
            //        Collection whatsnew = new Collection();
            //        string line = null;
            //        do
            //        {
            //            line = textreader.ReadLine;
            //            if (line != null)
            //            {
            //                whatsnew.Add(line.ToString);
            //            }
            //        } while (!(line == null));
            //        textreader.Close();
            //        UpdateText.Text = "An update(" + version + ") was found, do you want to apply it?";
            //        ProgressBox.Visibility = Visibility.Collapsed;
            //        ApplyUpdateButton.Visibility = Visibility.Visible;
            //        CancelUpdateButton.Visibility = Visibility.Visible;
            //        WhatsNewTextBox.Clear();
            //        foreach (string s in whatsnew)
            //        {
            //            WhatsNewTextBox.AppendText(s + Constants.vbNewLine);
            //        }
            //        WhatsNewTextBlock.Visibility = Visibility.Visible;
            //        WhatsNewTextBox.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        MessageBoxDialog m = new MessageBoxDialog("Document.Editor is already up to date", "Up To Date", null, null);
            //        m.MessageImage.Source = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/info32.png", UriKind.Relative));
            //        m.Owner = this;
            //        m.ShowDialog();
            //        UpdateBox.Visibility = Visibility.Hidden;
            //        AppLogo.Visibility = Visibility.Visible;
            //        LicenseButton.Visibility = Visibility.Visible;
            //        OKButton.Visibility = Visibility.Visible;
            //        UpdateButton.Visibility = Visibility.Visible;
            //    }
            //}
        }

        #endregion

        #region "Update"

        //private WebClient withEventsField_webmanager = new WebClient();
        //public WebClient webmanager
        //{
        //    get { return withEventsField_webmanager; }
        //    set
        //    {
        //        if (withEventsField_webmanager != null)
        //        {
        //            withEventsField_webmanager.DownloadProgressChanged -= webmanager_DownloadProgressChanged;
        //            withEventsField_webmanager.DownloadFileCompleted -= webmanager_DownloadFileCompleted;
        //        }
        //        withEventsField_webmanager = value;
        //        if (withEventsField_webmanager != null)
        //        {
        //            withEventsField_webmanager.DownloadProgressChanged += webmanager_DownloadProgressChanged;
        //            withEventsField_webmanager.DownloadFileCompleted += webmanager_DownloadFileCompleted;
        //        }
        //    }

        //}
        private void ApplyUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            //UpdateProgressbar.IsIndeterminate = false;
            //UpdateProgressbar.Minimum = 0;
            //UpdateProgressbar.Maximum = 100;
            //UpdateProgressbar.Value = 0;
            //ProgressBox.Visibility = Visibility.Visible;
            //if (File.Exists(My.Computer.FileSpecialDirectories.Temp + "\\Semagsoft\\DocumentEditor\\setup.exe"))
            //{
            //    File.Delete(My.Computer.FileSpecialDirectories.Temp + "\\Semagsoft\\DocumentEditor\\setup.exe");
            //}
            //webmanager.DownloadFileAsync(new Uri("http://documenteditor.net/download/Installer"), My.Computer.FileSpecialDirectories.Temp + "\\Semagsoft\\DocumentEditor\\setup.exe");
            //UpdateText.Text = "Downloading Update, Please Wait...";
            //ApplyUpdateButton.Visibility = Visibility.Collapsed;
        }

        private void CancelUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            //while (webmanager.IsBusy)
            //{
            //    webmanager.CancelAsync();
            //}

            //if (File.Exists(My.Computer.FileSpecialDirectories.Temp + "\\Semagsoft\\DocumentEditor\\setup.exe"))
            //{
            //    File.Delete(My.Computer.FileSpecialDirectories.Temp + "\\Semagsoft\\DocumentEditor\\setup.exe");
            //}
            //AppLogo.Visibility = Visibility.Visible;
            //LicenseButton.Visibility = Visibility.Visible;
            //OKButton.Visibility = Visibility.Visible;
            //UpdateButton.Visibility = Visibility.Visible;
            //UpdateBox.Visibility = Visibility.Collapsed;
            //UpdateText.Text = "Checking for updates...";
            //UpdateProgressbar.IsIndeterminate = true;
            //FilesizeTextBlock.Visibility = Visibility.Collapsed;
            //ApplyUpdateButton.Visibility = Visibility.Collapsed;
            //CancelUpdateButton.Visibility = Visibility.Collapsed;
            //WhatsNewTextBlock.Visibility = Visibility.Collapsed;
            //WhatsNewTextBox.Visibility = Visibility.Collapsed;
        }

        public void webmanager_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //UpdateProgressbar.Value = e.ProgressPercentage;
            //FilesizeTextBlock.Visibility = Visibility.Visible;
            //int downloadedfilesize = e.BytesReceived / 1024;
            //int totalfilesize = e.TotalBytesToReceive / 1024;
            //FilesizeTextBlock.Text = downloadedfilesize.ToString + " KB" + "/" + totalfilesize.ToString + " KB";
        }

        public void webmanager_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //if (!e.Cancelled)
            //{
            //    try
            //    {
            //        Process.Start(My.Computer.FileSpecialDirectories.Temp + "\\Semagsoft\\DocumentEditor\\setup.exe", "/D=" + My.Application.Info.DirectoryPath);
            //        My.Application.Shutdown();
            //    }
            //    catch (Exception ex)
            //    {
            //        if (ex.Message.EndsWith("canceled by the user"))
            //        {
            //            MessageBoxDialog m = new MessageBoxDialog("The update has been canceled!", "Update Canceled", null, null);
            //            m.MessageImage.Source = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/info32.png", UriKind.Relative));
            //            m.Owner = this;
            //            m.ShowDialog();
            //            CancelUpdateButton_Click(null, null);
            //        }
            //        else
            //        {
            //            MessageBoxDialog m = new MessageBoxDialog("Error running update installer", "Error", null, null);
            //            m.MessageImage.Source = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/error32.png", UriKind.Relative));
            //            m.Owner = this;
            //            m.ShowDialog();
            //        }
            //    }
            //}
        }

        #endregion


    }
}
