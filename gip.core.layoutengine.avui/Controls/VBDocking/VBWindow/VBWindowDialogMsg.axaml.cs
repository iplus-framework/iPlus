using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using gip.core.datamodel;
using System.Windows.Threading;
using gip.core.layoutengine.avui.Controls.VBDocking.VBWindow;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace gip.core.layoutengine.avui
{
    public class MsgTest
    {
        //        eMsgLevel MessageLevel { get; set; }

        public string Source { get; set; }

        public string MessageNo { get; set; }

        public string Message { get; set; }
    }
    /// <summary>
    /// Represents a message window dialog.
    /// </summary>
    public partial class VBWindowDialogMsg : VBWindowDialog
    {
        Global.MsgResult _MsgResult = Global.MsgResult.Cancel;

        Msg _Msg;
        eMsgButton _MsgButton;
        static int _ModalCounter = 0;

        public VBWindowDialogMsg() : base()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="VBWindowDialogMsg" /> class.</summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="msgButton">The MSG button.</param>
        /// <param name="caller">The caller.</param>
        public VBWindowDialogMsg(Msg msg, eMsgButton msgButton, AvaloniaObject caller) : base(caller)
        {
            InitializeComponent();
            _Msg = msg;
            _MsgButton = msgButton;

            if (!(_Msg is MsgWithDetails))
            {
                gridRoot.RowDefinitions[2].Height = new GridLength(0);
                this.Height = 500;
                this.Width = 750;
            }

            try
            {
                if (Database.Root != null)
                {
                    btnYes.Content = Database.Root.Environment.TranslateText(Database.Root, "_Yes");
                    btnNo.Content = Database.Root.Environment.TranslateText(Database.Root, "_No");
                    btnOK.Content = Database.Root.Environment.TranslateText(Database.Root, "_Ok");
                    btnCancel.Content = Database.Root.Environment.TranslateText(Database.Root, "_Cancel");
                }
            }
            catch (Exception e)
            {
                string msgEx = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msgEx += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBWindowDialogMsg", "VBWindowDialogMsg", msgEx);
            }
            this.DataContext = this;

            KeyDown += VBWindowDialogMsg_KeyDown;
        }

        private void VBWindowDialogMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.C)
                this.Clipboard.SetTextAsync(_Msg.Message);
        }

        internal override void DeInitVBControl(IACComponent bso = null)
        {
            _Msg = null;
            _Messages = null;
            KeyDown -= VBWindowDialogMsg_KeyDown;
            base.DeInitVBControl(bso);
        }


        ObservableCollection<Msg> _Messages = null;
        /// <summary>
        /// Gets the list of message details.
        /// </summary>
        public ObservableCollection<Msg> MsgDetails
        {
            get
            {
                if (!(_Msg is MsgWithDetails))
                    return null;
                if (_Messages == null)
                {
                    MsgWithDetails msgWithDetails = _Msg as MsgWithDetails;
                    _Messages = new ObservableCollection<Msg>(msgWithDetails.MsgDetails);
                }

                return _Messages;
            }
        }

        ObservableCollection<VBWindowDialogMsgViewModel> _MsgDetailsView;
        /// <summary>
        /// Gets the list of message details view.
        /// </summary>
        public ObservableCollection<VBWindowDialogMsgViewModel> MsgDetailsView
        {
            get
            {
                if(_MsgDetailsView == null && this.MsgDetails != null)
                    _MsgDetailsView = new ObservableCollection<VBWindowDialogMsgViewModel>(MsgDetails.Select(x =>
                    new VBWindowDialogMsgViewModel() { Message = x, ImageContent = MsgLevelContentControlGet(MsgLevelImageGet(x.MessageLevel)) }));
                return _MsgDetailsView;
            }
        }




        private void Button_Yes(object sender, RoutedEventArgs e)
        {
            _MsgResult = Global.MsgResult.Yes;
            Close();
        }

        private void Button_No(object sender, RoutedEventArgs e)
        {
            _MsgResult = Global.MsgResult.No;
            Close();
        }

        private void Button_OK(object sender, RoutedEventArgs e)
        {
            _MsgResult = Global.MsgResult.OK;
            Close();
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            _MsgResult = Global.MsgResult.Cancel;
            Close();
        }

        delegate Global.MsgResult ShowMsgBoxDelegate();
        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <returns>The message result.</returns>
        public Global.MsgResult ShowMessageBox()
        {
            txtMessage.Text = _Msg.Message;

            string imagePNG = MsgLevelImageGet(_Msg.MessageLevel);

            string title = "";
            if (Database.Root != null && Database.Root.Environment != null)
            {
                switch (_Msg.MessageLevel)
                {
                    case eMsgLevel.Info:
                        title = Database.Root.Environment.TranslateText(Database.Root, "Info {0}");
                        break;
                    case eMsgLevel.Warning:
                        title = Database.Root.Environment.TranslateText(Database.Root, "Warning {0}");
                        break;
                    case eMsgLevel.Failure:
                        title = Database.Root.Environment.TranslateText(Database.Root, "Failure {0}");
                        break;
                    case eMsgLevel.Error:
                        title = Database.Root.Environment.TranslateText(Database.Root, "Error {0}");
                        break;
                    case eMsgLevel.Exception:
                        title = Database.Root.Environment.TranslateText(Database.Root, "Exception {0}");
                        break;
                    case eMsgLevel.Question:
                        title = Database.Root.Environment.TranslateText(Database.Root, "Question {0}");
                        break;
                    default:
                        title = Database.Root.Environment.TranslateText(Database.Root, "Info {0}");
                        break;
                }
            }
            else
            {
                switch (_Msg.MessageLevel)
                {
                    case eMsgLevel.Info:
                        title = ("Info {0}");
                        break;
                    case eMsgLevel.Warning:
                        title = ("Warning {0}");
                        break;
                    case eMsgLevel.Failure:
                        title = ("Failure {0}");
                        break;
                    case eMsgLevel.Error:
                        title = ("Error {0}");
                        break;
                    case eMsgLevel.Exception:
                        title = ("Exception {0}");
                        break;
                    case eMsgLevel.Question:
                        title = ("Question {0}");
                        break;
                    default:
                        title = ("Info {0}");
                        break;
                }
            }


            
            imgIcon.Content = MsgLevelContentControlGet(MsgLevelImageGet(_Msg.MessageLevel));

            switch (_MsgButton)
            {
                case eMsgButton.OK:
                    stackPanel.Children.Remove(btnYes);
                    stackPanel.Children.Remove(btnNo);
                    stackPanel.Children.Remove(btnCancel);
                    break;
                case eMsgButton.OKCancel:
                    stackPanel.Children.Remove(btnYes);
                    stackPanel.Children.Remove(btnNo);
                    break;
                case eMsgButton.YesNo:
                    stackPanel.Children.Remove(btnOK);
                    stackPanel.Children.Remove(btnCancel);

                    break;
                case eMsgButton.YesNoCancel:
                    stackPanel.Children.Remove(btnOK);
                    break;
            }

            WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen;

            Title = string.Format(title, _Msg.ACIdentifier);
            RefreshTitle();

            //if (!CheckAccess())
            //{
            //    ShowMsgBoxDelegate showDel = ShowMessageBox;
            //    return (Global.MsgResult)Dispatcher.Invoke(showDel, DispatcherPriority.Normal);
            //    DispatcherOperation op = Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(delegate { ShowDialog(); }));
            //    op.Wait();
            //}
            try
            {
                _ModalCounter++;
                var owner = ((Database.Root?.RootPageWPF?.WPFApplication as Application)?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                ShowDialog(owner);
            }
            finally
            {
                _ModalCounter--;
            }

            return _MsgResult;
        }


        public static bool IsOneDialogOpenedModal
        {
            get
            {
                return _ModalCounter > 0;
            }
        }


        #region helper methods
        /// <summary>
        /// Gets the image for messagebox.
        /// </summary>
        /// <param name="level">The message level parameter</param>
        /// <returns>The name of image.</returns>
        public string MsgLevelImageGet(eMsgLevel level)
        {
            string imagePNG;
            if (ControlManager.WpfTheme == eWpfTheme.Aero)
            {
                switch (level)
                {
                    case eMsgLevel.Info:
                        imagePNG = "IconMsgInfoStyle";
                        break;
                    case eMsgLevel.Warning:
                        imagePNG = "IconMsgExclamationStyle";
                        break;
                    case eMsgLevel.Failure:
                    case eMsgLevel.Error:
                    case eMsgLevel.Exception:
                        imagePNG = "IconMsgStopStyle";
                        break;
                    case eMsgLevel.Question:
                        imagePNG = "IconMsgQuestionStyle";
                        break;
                    default:
                        imagePNG = "IconMsgInfoStyle";
                        break;
                }
            }
            else
            {
                switch (level)
                {
                    case eMsgLevel.Info:
                        imagePNG = "IconMsgInfoStyle";
                        break;
                    case eMsgLevel.Warning:
                        imagePNG = "IconMsgExclamationStyle";
                        break;
                    case eMsgLevel.Failure:
                    case eMsgLevel.Error:
                    case eMsgLevel.Exception:
                        imagePNG = "IconMsgStopStyle";
                        break;
                    case eMsgLevel.Question:
                        imagePNG = "IconMsgQuestionStyle";
                        break;
                    default:
                        imagePNG = "IconMsgInfoStyle";
                        break;
                }
            }
            return imagePNG;
        }

        /// <summary>
        /// Gets the messagebox content control.
        /// </summary>
        /// <param name="imagePNG">The imagePNG parameter.</param>
        /// <returns>The target content control.</returns>
        public ContentControl MsgLevelContentControlGet(string imagePNG)
        {
            ContentControl contentControl = new ContentControl();
            object theme = null;
            if (this.Resources.TryGetResource(imagePNG, null, out theme))
                contentControl.Styles.Add(theme as Style);
            else
            {
                foreach (ResourceDictionary dict in this.Resources.MergedDictionaries)
                {
                    if (dict.TryGetResource(imagePNG, null, out theme))
                    {
                        object resource = dict[imagePNG];
                        if (resource != null)
                        {
                            contentControl.Styles.Add(theme as Style);
                            break;
                        }
                    }
                }
            }
            return contentControl;
        }
        #endregion end
    }
}
