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
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using System.Collections;
using gip.core.datamodel;
using gip.core.layoutengine;
using gip.core.autocomponent;
using gip.core.datamodel.Licensing;
using System.Threading.Tasks;

namespace gip.iplus.client
{
    /// <summary>
    /// Interaktionslogik für StartupProgress.xaml
    /// </summary>
    public partial class Login : Window
    {
        protected object _WaitOnOkClick = new object();
        int _CountAttempts = 0;

        //public VarioiplusLogin(MsgWithDetails infoMessage)
        public Login()
        {

            InitializeComponent();
            this.DataContext = this;

            // Loaded-Eventhandler registrieren
            this.Loaded += new RoutedEventHandler(Login_Loaded);
            // Unloaded-EventHandler registrieren
            this.Unloaded += new RoutedEventHandler(Login_Unloaded);
        }

        #region Eventhandler

        /// <summary>
        /// Ereignishandler der ausgelöst wird, wenn die Form VarioiplusLogin komplett geladen wurde.
        /// </summary>
        /// <param name="sender">Die Klasse VarioiplusLogin als Objekt.</param>
        /// <param name="e">Die gerouteten Ereignisdaten.</param>
        /// <remarks></remarks>
        private async void Login_Loaded(object sender, RoutedEventArgs e)
        {
            Monitor.Enter(_WaitOnOkClick);
            // eine IAsyncResult-Instanz für die Initialisierung
            // der Anwendung erzeugen
            Task result = null;
            // die Initialisierung der Anwendung starten
            if ((App.Current != null) && (App.Current.ApplicationInitialize != null))
            {
                //result = App.Current.ApplicationInitialize.BeginInvoke(this, initCompleted, null);
                result = Task.Run(() => App.Current.ApplicationInitialize(this));

                // als behandelt markieren  
                e.Handled = true;
            }

            await result;
            await Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Invoker)delegate { Close(); });


            // dieser anonyme Delegat wird aufgerufen, wenn die
            // Initialisierung abgeschlossen wurde
            //AsyncCallback initCompleted = delegate(IAsyncResult ar)
           // {
                //if ((App.Current != null) && (App.Current.ApplicationInitialize != null))
                //    App.Current.ApplicationInitialize.EndInvoke(result);

                // Sicherstellen das Close auf dem UI Thread ausgeführt wird.
                // Deshalb wird auf den anwendungsweiten Delegaten Invoker gecastet.
                //Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Invoker)delegate { Close(); });
            //};

        }

        /// <summary>
        /// Ereignishandler der ausgelöst wird, wenn die Form VarioiplusLogin entladen wurde.
        /// </summary>
        /// <param name="sender">Die Klasse VarioiplusLogin als Objekt.</param>
        /// <param name="e">Die gerouteten Ereignisdaten.</param>
        /// <remarks></remarks>
        private void Login_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this._contentLoaded)
            {
                // EventHandler löschen
                this.Loaded -= new RoutedEventHandler(Login_Loaded);
                this.Unloaded -= new RoutedEventHandler(Login_Unloaded);
            }

            // prüfen ob der Ladevorgang abgebrochen wurde
            if (e != null)
            {
                // hier konnen evtl. Dispose-Aufrufe von nicht mehr benötigten
                // Objekten ausgeführt werden.

                // kein Abbruch, Ereignis als behandelt markieren
                e.Handled = true;
            }
        }
        #endregion


        #region Progress-Area
        public MsgWithDetails InfoMessage
        {
            get
            {
                return Messages.GlobalMsg;
            }
        }

        ObservableCollection<Msg> _Messages = null;
        public ObservableCollection<Msg> MsgDetails
        {
            get
            {
                if (_Messages == null)
                {
                    if (InfoMessage.MsgDetails != null)
                    {
                        if (InfoMessage.MsgDetails is ObservableCollection<Msg>)
                        {
                            (InfoMessage.MsgDetails as ObservableCollection<Msg>).CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_Messages_CollectionChanged);
                        }
                    }
                    _Messages = new ObservableCollection<Msg>();
                }

                return _Messages;
            }
        }

        void _Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e != null)
            {
                if (!this.listboxInfo.CheckAccess())
                {
                    this.listboxInfo.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action<object, System.Collections.Specialized.NotifyCollectionChangedEventArgs>(_Messages_CollectionChanged), sender, e);
                    return;
                }
                AddItems(e.NewItems);

            }
        }

        private void AddItems(IList newItems)
        {
            if (_Messages != null)
            {
                if (newItems != null)
                {
                    foreach (Msg msg in newItems)
                    {
                        _Messages.Add(msg);
                    }
                }

                if (listboxInfo.Items.Count > 0)
                {
                    listboxInfo.SelectedIndex = listboxInfo.Items.Count - 1;
                    listboxInfo.ScrollIntoView(listboxInfo.SelectedItem);
                }
            }
        }
        #endregion

        #region Login-Area
        /// <summary>
        /// 1. Call from App
        /// </summary>
        /// <param name="display"></param>
        /// <param name="defaultUser"></param>
        /// <param name="defaultPassword"></param>
        public void DisplayLogin(bool display, string defaultUser, string defaultPassword, eWpfTheme wpfTheme, String errorMsg)
        {
            if (!this.ProgressGrid.CheckAccess())
            {
                this.ProgressGrid.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action<bool, string, string, eWpfTheme, String>(DisplayLogin), display, defaultUser, defaultPassword, wpfTheme, errorMsg);
                return;
            }

            if (display)
            {
                CheckRestoreSameScreen.IsChecked = !ControlManager.RestoreWindowsOnSameScreen;
                CheckTouchScreen.IsChecked = ControlManager.TouchScreenMode;
                _CountAttempts++;
                if (_CountAttempts <= 1)
                {
                    _User = defaultUser;
                    TextboxUser.Text = _User;
                    TextboxKey.Text = "";
#if DEBUG
                    _Password = defaultPassword;
                    TextboxPassword.Password = _Password;
#endif
                    selTheme.ItemsSource = System.Enum.GetValues(typeof(eWpfTheme));
                    selTheme.SelectedValue = wpfTheme;
                    //selTheme.SelectedIndex = 0;
                }
                else
                {
                    Msg msgLogon = Messages.GlobalMsg.MsgDetails.Where(c => c.Message == "DB-Connection failed!").FirstOrDefault();
                    Msg userMsg;
                    if (msgLogon != null)
                    {
                        userMsg = new Msg() { Message = "Cannot connect to database. Check your connection string or rights connecting the database!", MessageLevel = eMsgLevel.Info };
                    }
                    else
                    {
                        userMsg = new Msg() { Message = String.Format("User {0} doesn't exist or wrong password!", this.User), MessageLevel = eMsgLevel.Info };
                    }
                    if (!String.IsNullOrEmpty(errorMsg))
                        userMsg.Message += " // " + errorMsg;
                    //VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(userMsg, eMsgButton.OK, null);
                    //vbMessagebox.ShowMessageBox();
                    MessageBox.Show(userMsg.Message, "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    _Password = "";
                    TextboxPassword.Password = _Password;
                }

                _CtrlPressed = false;
                _F1Pressed = false;

                ProgressGrid.Visibility = System.Windows.Visibility.Collapsed;
                LoginGrid.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Monitor.Enter(_WaitOnOkClick);
                LoginGrid.Visibility = System.Windows.Visibility.Collapsed;
                ProgressGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// 2. Call from App and waits on User-OK-click
        /// </summary>
        /// <param name="enteredUser"></param>
        /// <param name="enteredPassword"></param>
        /// <param name="shiftPressed"></param>
        /// <param name="showAllMenus"></param>
        public void GetLoginResult(ref string enteredUser, ref string enteredPassword, ref bool shiftPressed, ref bool f1Pressed)
        {
            Monitor.Enter(_WaitOnOkClick);
            enteredUser = _User;
            enteredPassword = _Password;
            shiftPressed = _CtrlPressed;
            f1Pressed = _F1Pressed;
            Monitor.Exit(_WaitOnOkClick);
        }

        /// <summary>
        /// 3. User Clicks OK => Signal on GetLoginResult
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_Keyword))
            {
                if (_Keyword == TextboxKey.Text)
                    return;
                if (!License.VerifyRemoteLogin(TextboxKey.Text, _Keyword, Database.GlobalDatabase))
                    return;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                _CtrlPressed = true;

            if (IsLoginWithControlLoad)
                _CtrlPressed = true;

            if (Keyboard.IsKeyDown(Key.F1))
                _F1Pressed = true;

            _WpfTheme = (eWpfTheme)System.Enum.Parse(typeof(eWpfTheme), selTheme.SelectedValue.ToString());
            _User = TextboxUser.Text;
            _Password = TextboxPassword.Password;
            _Keyword = TextboxKey.Text;
            label3.Visibility = System.Windows.Visibility.Hidden;
            TextboxKey.Visibility = System.Windows.Visibility.Hidden;
            ControlManager.RestoreWindowsOnSameScreen = CheckRestoreSameScreen.IsChecked.HasValue ? !CheckRestoreSameScreen.IsChecked.Value : true;
            ControlManager.TouchScreenMode = CheckTouchScreen.IsChecked.HasValue ? CheckTouchScreen.IsChecked.Value : false;

            Monitor.Exit(_WaitOnOkClick);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            // VarioiplusLogin schliessen
            this.Close();

            // Unload-EreignisHandler mit Ereignis=null aufrufen
            // um Abbruch zu signalisieren
            this.Login_Unloaded(this, null);

            Monitor.Exit(_WaitOnOkClick);
        }

        private string _Keyword = "";

        private string _User;
        public string User
        {
            get
            {
                return _User;
            }
        }

        private string _Password;
        public string Password
        {
            get
            {
                return _Password;
            }
        }



        private bool _CtrlPressed;
        public bool CtrlPressed
        {
            get
            {
                return _CtrlPressed;
            }
        }

        private bool _F1Pressed;
        public bool F1Pressed
        {
            get
            {
                return _F1Pressed;
            }
        }

        private eWpfTheme _WpfTheme;
        public eWpfTheme WpfTheme
        {
            get
            {
                return _WpfTheme;
            }
        }

        #endregion

        private void ButtonLogin_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (labelTouchScreen.Visibility == Visibility.Visible)
                {
                    label3.Visibility = Visibility.Visible;
                    TextboxKey.Visibility = Visibility.Visible;
                    labelTouchScreen.Visibility = Visibility.Collapsed;
                    CheckTouchScreen.Visibility = Visibility.Collapsed;
                }
                else
                {
                    label3.Visibility = Visibility.Collapsed;
                    TextboxKey.Visibility = Visibility.Collapsed;
                    labelTouchScreen.Visibility = Visibility.Visible;
                    CheckTouchScreen.Visibility = Visibility.Visible;
                }

                _Keyword = License.GenerateRemoteLoginCode();
                TextboxKey.Text = _Keyword;
                e.Handled = true;

                return;
            }
        }

        private bool _IsLoginWithControlLoad = false;
        internal bool IsLoginWithControlLoad
        {
            get
            {
                return _IsLoginWithControlLoad;
            }
            set
            {
                _IsLoginWithControlLoad = value;
            }
        }
    }
}
