using Avalonia.Controls;
using gip.core.autocomponent;

namespace gip.iplus.client.avui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private bool _ShutdownInProgress = false;
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (_ShutdownInProgress)
        {
            base.OnClosing(e);
            return;
        }
        _ShutdownInProgress = true;
        ACRoot.SRoot.ACDeInit();
        App._GlobalApp.ShutdownApplication();
    }
}