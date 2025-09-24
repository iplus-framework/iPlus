using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    public static class AsyncMessageBox
    {
        // Shows a message box from a separate worker thread.
        public static async void BeginMessageBoxAsync(string strMessage, string strCaption, ButtonEnum enmButton, Icon enmImage)
        {
            ShowMessageBoxDelegate caller = new ShowMessageBoxDelegate(ShowMessageBox);
            var workTask = Task.Run(() => caller.Invoke(strMessage, strCaption, enmButton, enmImage));
            await workTask;
            //caller.BeginInvoke(strMessage, strCaption, enmButton, enmImage, null, null);
        }

        // Shows a message box from a separate worker thread. The specified asynchronous
        // result object allows the caller to monitor whether the message box has been 
        // closed. This is useful for showing only one message box at a time.
        public static void BeginMessageBoxAsync(
            string strMessage,
            string strCaption,
            ButtonEnum enmButton,
            Icon enmImage,
            ref IAsyncResult asyncResult)
        {
            BeginMessageBoxAsync(strMessage, strCaption, enmButton, enmImage, ref asyncResult, null);
        }

        // Shows a message box from a separate worker thread. The specified asynchronous
        // result object allows the caller to monitor whether the message box has been 
        // closed. This is useful for showing only one message box at a time.
        // Also specifies a callback method when the message box is closed.
        public static void BeginMessageBoxAsync(
            string strMessage,
            string strCaption,
            ButtonEnum enmButton,
            Icon enmImage,
            ref IAsyncResult asyncResult,
            AsyncCallback callBack)
        {
            if ((asyncResult == null) || asyncResult.IsCompleted)
            {
                ShowMessageBoxDelegate caller = new ShowMessageBoxDelegate(ShowMessageBox);
                asyncResult = caller.BeginInvoke(strMessage, strCaption, enmButton, enmImage, callBack, null);
            }
        }

        private delegate IMsBox<ButtonResult> ShowMessageBoxDelegate(string strMessage, string strCaption, ButtonEnum enmButton, Icon enmImage);

        // Method invoked on a separate thread that shows the message box.
        private static IMsBox<ButtonResult> ShowMessageBox(string strMessage, string strCaption, ButtonEnum enmButton, Icon enmImage)
        {
            return MessageBoxManager.GetMessageBoxStandard(strCaption, strMessage, enmButton, enmImage);
        }

        //public static MessageBoxResult EndMessageBoxAsync(IAsyncResult result)
        //{
        //    // Retrieve the delegate.
        //    AsyncResult asyncResult = (AsyncResult)result;
        //    ShowMessageBoxDelegate caller = (ShowMessageBoxDelegate)asyncResult.AsyncDelegate;

        //    return caller.EndInvoke(asyncResult);
        //}
    }
}
