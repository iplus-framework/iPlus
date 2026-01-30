using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using DialogHostAvalonia;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui;

public partial class VBMessageBox : UserControl
{
    #region c'tors

    public VBMessageBox()
    {
        InitializeComponent();
    }
    public VBMessageBox(Msg msg, Global.MsgResult defaultResult, eMsgButton msgButton = eMsgButton.OK)
    {
        InitializeComponent();
        _Message = msg;
        _DefaultResult = defaultResult;
        InitalizeVBControl(msgButton);
        DataContext = this;
    }

    #endregion

    #region Properties

    private Msg _Message;

    public Msg Message
    {
        get => _Message;
    }
    public List<VBMessageBoxModel> MsgDetails
    {
        get;
        set;
    }

    private Action _CloseAction;

    private Global.MsgResult _Result = Global.MsgResult.None;
    private Global.MsgResult _DefaultResult = Global.MsgResult.None;

    #endregion

    #region Methods

    #region Methods => Button Click Events

    private void btnYes_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _Result = Global.MsgResult.Yes;
        _CloseAction?.Invoke();
    }

    private void btnNo_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _Result = Global.MsgResult.No;
        _CloseAction?.Invoke();
    }

    private void btnOK_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _Result = Global.MsgResult.OK;
        _CloseAction?.Invoke();
    }

    private void btnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _Result = Global.MsgResult.Cancel;
        _CloseAction?.Invoke();
    }

    #endregion

    private void InitalizeVBControl(eMsgButton msgButton)
    {
        switch (msgButton)
        {
            case eMsgButton.OK:
                btnOK.IsVisible = true;
                break;
            case eMsgButton.OKCancel:
                btnOK.IsVisible = true;
                btnCancel.IsVisible = true;
                break;
            case eMsgButton.YesNo:
                btnYes.IsVisible = true;
                btnNo.IsVisible = true;
                break;
            case eMsgButton.YesNoCancel:
                btnYes.IsVisible = true;
                btnNo.IsVisible = true;
                btnCancel.IsVisible = true;
                break;
            case eMsgButton.Cancel:
                btnCancel.IsVisible = true;
                break;
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

        MsgWithDetails msgWithDetails = _Message as MsgWithDetails;

        if (msgWithDetails != null && msgWithDetails.MsgDetails.Any())
        {
            List<VBMessageBoxModel> details = new List<VBMessageBoxModel>();

            foreach (var msg in msgWithDetails.MsgDetails)
            { 
                details.Add(new VBMessageBoxModel { Message = msg, ImageContent = GetIconContent(msg.MessageLevel)});
            }

            MsgDetails = details;
            lstDetails.IsVisible = true;
        }

        if (_Message != null && _Message.Message != null)
            imgIcon.Content = GetIconContent(_Message.MessageLevel);
    }

    public ContentControl GetIconContent(eMsgLevel msgLevel)
    {
        switch (msgLevel)
        {
            case eMsgLevel.Info:
                return GetMsgLevelContent("IconMsgInfoStyle");
            case eMsgLevel.Warning:
                return GetMsgLevelContent("IconMsgExclamationStyle");
            case eMsgLevel.Failure:
            case eMsgLevel.Error:
            case eMsgLevel.Exception:
                return GetMsgLevelContent("IconMsgStopStyle");
            case eMsgLevel.Question:
                return GetMsgLevelContent("IconMsgQuestionStyle");
            default:
                return GetMsgLevelContent("IconMsgInfoStyle");
        }
    }

    public ContentControl GetMsgLevelContent(string themeKey)
    {
        ContentControl contentControl = new ContentControl();
        object theme = null;
        if (this.Resources.TryGetResource(themeKey, null, out theme))
        {
            contentControl.Theme = theme as ControlTheme;
        }
        else
        {
            foreach (ResourceDictionary dict in this.Resources.MergedDictionaries)
            {
                if (dict.TryGetResource(themeKey, null, out theme))
                {
                    object resource = dict[themeKey];
                    if (resource != null)
                    {
                        contentControl.Theme = theme as ControlTheme;
                    }
                }
            }
        }
        return contentControl;
    }

    public async Task<Global.MsgResult> ShowMessageAsync()
    {
        if (Application.Current != null &&
            Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return await ShowMessageWindow(desktop.MainWindow);
        }

        if (Application.Current != null &&
            Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime lifetime)
        {
            return await ShowMessageSingleView(lifetime.MainView as ContentControl);
        }

        return Global.MsgResult.None;
    }

    private async Task<Global.MsgResult> ShowMessageWindow(Window mainWindow)
    {
        Window dialog = new Window();
        dialog.ExtendClientAreaTitleBarHeightHint = -1;
        dialog.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
        dialog.ExtendClientAreaToDecorationsHint = true;

        dialog.SizeToContent = SizeToContent.WidthAndHeight;
        dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        dialog.PointerPressed += (s, e) => dialog.BeginMoveDrag(e);

        dialog.MaxWidth = mainWindow.Width / 2;
        dialog.Content = this;

        dialog.Opened += (s, e) =>
        {
            var column = lstDetails.Columns.LastOrDefault();
            if (column != null)
            {
                column.Width = new DataGridLength(dialog.DesiredSize.Width - 60);
            }

            SetDefaultResult(_DefaultResult);
        };

        var tcs = new TaskCompletionSource<Global.MsgResult>();

        this.SetCloseAction(() =>
        {
            tcs.TrySetResult(this.GetButtonResult());
            dialog.Close();
        });

        await dialog.ShowDialog(mainWindow);
        return await tcs.Task;
    }   

    private Task<Global.MsgResult> ShowMessageSingleView(ContentControl mainView)
    {
        DialogHostStyles style = null;
        if (!mainView.Styles.OfType<DialogHostStyles>().Any())
        {
            style = [];
            mainView.Styles.Add(style);
        }

        var column = lstDetails.Columns.LastOrDefault();
        if (column != null)
        {
            column.Width = new DataGridLength(mainView.DesiredSize.Width - 60);
        }

        var parentContent = mainView.Content;
        var dh = new DialogHost
        {
            Identifier = "MsBoxIdentifier" + Guid.NewGuid()
        };

        mainView.Content = null;
        dh.Content = parentContent;
        
        mainView.Content = dh;

        var tcs = new TaskCompletionSource<Global.MsgResult>();
        this.SetCloseAction(() =>
        {
            Global.MsgResult r = this.GetButtonResult();

            if (dh.CurrentSession != null && dh.CurrentSession.IsEnded == false)
            {
                DialogHost.Close(dh.Identifier);
            }

            mainView.Content = null;
            dh.Content = null;
            mainView.Content = parentContent;
            if (style != null)
            {
                mainView.Styles.Remove(style);
            }
            tcs.TrySetResult(r);
        });
        DialogHost.Show(this, dh.Identifier);
        return tcs.Task;
    }

    private Global.MsgResult GetButtonResult()
    {
        return _Result;
    }

    private void SetDefaultResult(Global.MsgResult value)
    {
       switch(value)
        {
            case Global.MsgResult.OK:
                if (btnOK.IsVisible)
                    btnOK.Focus();
                break;
            case Global.MsgResult.Cancel:
                if (btnCancel.IsVisible)
                    btnCancel.Focus();
                break;
            case Global.MsgResult.Yes:
                if (btnYes.IsVisible)
                    btnYes.Focus();
                break;
            case Global.MsgResult.No:
                if (btnNo.IsVisible)
                    btnNo.Focus();
                break;
        }
    }

    private void SetCloseAction(Action value)
    {
        _CloseAction = value;
    }


    #endregion
}