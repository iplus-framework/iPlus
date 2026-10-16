using Android.App;
using Avalonia;
using Avalonia.Android;
using Android.Runtime;

namespace gip.iplus.client.avui.Android
{
    [Application]
    public class Application : AvaloniaAndroidApplication<App>
    {
        static Application()
        {
            // Android has no ConnectionStrings.config: register app-specific EF contexts so
            // the SQL Tunnel login derives their connection strings from the default container.
            gip.iplus.client.avui.LoginView.AdditionalConnectionContainerNames.Add(
                "iPlusMESV5_Entities"); // gip.mes.datamodel.DatabaseApp.C_DefaultContainerName
        }

        protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }


        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            return base.CustomizeAppBuilder(builder).WithInterFont();
                //  .AfterSetup(_ =>
                //  {
                //      Pages.EmbedSample.Implementation = new EmbedSampleAndroid();
                //  });
        }
    }
}
