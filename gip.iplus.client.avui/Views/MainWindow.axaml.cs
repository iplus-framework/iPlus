using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using gip.core.autocomponent;

namespace gip.iplus.client.avui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        //// Refresh restore bounds from previous window opening
        //try
        //{
        //    Rect restoreBounds = Properties.Settings.Default.WindowPosition;
        //    this.Left = restoreBounds.Left;
        //    this.Top = restoreBounds.Top;
        //    this.Width = restoreBounds.Width;
        //    this.Height = restoreBounds.Height;
        //    this.sldZoom.Value = Properties.Settings.Default.Zoom;
        //}
        //catch (Exception ec)
        //{
        //    string msg = ec.Message;
        //    if (ec.InnerException != null && ec.InnerException.Message != null)
        //        msg += " Inner:" + ec.InnerException.Message;

        //    this.Root().Messages.LogException("VarioiplusMasterpage", "VarioiplusMasterpage", msg);
        //}
        base.OnInitialized();
    }

    private bool _ShutdownInProgress = false;
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        //// Save restore bounds for the next time this window is opened
        //Properties.Settings.Default.WindowPosition = this.RestoreBounds;
        //Properties.Settings.Default.Zoom = (int)this.sldZoom.Value;
        //Properties.Settings.Default.Save();

        if (_ShutdownInProgress)
        {
            base.OnClosing(e);
            return;
        }
        _ShutdownInProgress = true;
        e.Cancel = true;

        // Deinit on a background thread to avoid blocking the UI,
        // then shut down the app on the UI thread once deinit completes.
        _ = Task.Run(async () =>
        {
            try
            {
                await ACRoot.SRoot.ACDeInit().ConfigureAwait(false);
            }
            catch
            {
                // Swallow exceptions during shutdown to ensure the app still closes.
            }
            finally
            {
                _ = Dispatcher.UIThread.InvokeAsync(() => App._GlobalApp.ShutdownApplication());
            }
        });
    }

}