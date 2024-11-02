// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace gip.tool.installerAndUpdater
{
    /// <summary>
    /// Interaction logic for MsgBox.xaml
    /// </summary>
    public partial class MsgBox : Window, INotifyPropertyChanged
    {
        public MsgBox()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private string _Header = "";
        public string Header
        {
            get
            {
                return _Header;
            }
            set
            {
                _Header = value;
                OnPropertyChanged("Header");
            }
        }

        private string _Text;
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
                OnPropertyChanged("Text");
            }
        }

        private MessageBoxButton _MessageBoxBtn = MessageBoxButton.OK;
        public MessageBoxButton MessageBoxBtn
        {
            get
            {
                return _MessageBoxBtn;
            }
            set
            {
                _MessageBoxBtn = value;
                OnPropertyChanged("MessageBoxBtn");
            }
        }

        private MessageBoxImage _MessageBoxImage = MessageBoxImage.Warning;
        public MessageBoxImage MessageBoxImage
        {
            get
            {
                return _MessageBoxImage;
            }
            set
            {
                _MessageBoxImage = value;
                switch(_MessageBoxImage)
                {
                    case System.Windows.MessageBoxImage.Question:
                        MsgBoxImg.Source = new BitmapImage(new Uri("/gip.tool.installerAndUpdater;component/Images/question.png", UriKind.Relative));
                        break;

                    case System.Windows.MessageBoxImage.Warning:
                        MsgBoxImg.Source = new BitmapImage(new Uri("/gip.tool.installerAndUpdater;component/Images/warning.png", UriKind.Relative));
                        break;
                }


                OnPropertyChanged("MessageBoxImage");
            }
        }

        private MessageBoxResult _MessageBoxResult;

        public MessageBoxResult ShowMessageBox(string text, string header, MessageBoxButton msgBoxButton, MessageBoxImage msgBoxImage, MessageBoxResult defaultResult)
        {
            Text = text;
            Header = header;
            MessageBoxBtn = msgBoxButton;
            Application.Current.Dispatcher.Invoke(() => MessageBoxImage = msgBoxImage);
            _MessageBoxResult = defaultResult;
            Application.Current.Dispatcher.Invoke(() => ShowDialog());
            return _MessageBoxResult;
        }

        public static MessageBoxResult Show(string text, string header, MessageBoxButton msgBoxButton)
        {
            MsgBox msgBox = null;
            Application.Current.Dispatcher.Invoke(() => msgBox = new MsgBox());
            return msgBox.ShowMessageBox(text, header, msgBoxButton, 0, 0);
        }

        public static MessageBoxResult Show(string text, string header, MessageBoxButton msgBoxButton, MessageBoxImage msgBoxImage)
        {
            MsgBox msgBox = null;
            Application.Current.Dispatcher.Invoke(() => msgBox = new MsgBox());
            return msgBox.ShowMessageBox(text, header, msgBoxButton, msgBoxImage, 0);
        }

        public static MessageBoxResult Show(string text, string header, MessageBoxButton msgBoxButton, MessageBoxImage msgBoxImage, MessageBoxResult defaultResult)
        {
            MsgBox msgBox = null;
            Application.Current.Dispatcher.Invoke(() => msgBox = new MsgBox());
            Application.Current.Dispatcher.Invoke(() => msgBox.SelectButtonWithDefaultResult(defaultResult));
            return msgBox.ShowMessageBox(text, header, msgBoxButton, msgBoxImage, defaultResult);
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            _MessageBoxResult = MessageBoxResult.Yes;
            Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            _MessageBoxResult = MessageBoxResult.No;
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            _MessageBoxResult = MessageBoxResult.OK;
            Close();
        }

        private void SelectButtonWithDefaultResult(MessageBoxResult defaultResult)
        {
            switch (defaultResult)
            {
                case MessageBoxResult.OK:
                    btnOk.Focus();
                    break;
                case MessageBoxResult.Yes:
                    btnYes.Focus();
                    break;
                case MessageBoxResult.No:
                    btnNo.Focus();
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if(e.Key == Key.Enter && _MessageBoxResult != MessageBoxResult.None)
            {
                Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
