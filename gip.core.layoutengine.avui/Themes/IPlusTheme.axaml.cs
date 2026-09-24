using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Includes the aero theme in an application.
    /// </summary>
    public sealed partial class IPlusTheme
        : Styles
    {
        public IPlusTheme()
        {
            AvaloniaXamlLoader.Load(this);

            // OxyPlot theme: assembly name differs between Avalonia fork (OxyPlot.Avalonia)
            // and NuGet package (OxyPlot.Avalonia12). Resolve at runtime.
            // Note: OxyPlot.PlotModel lives in OxyPlot.Core (assembly "OxyPlot") which has no theme -
            // we need the Avalonia wrapper assembly.
            string oxyPlotAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetName().Name)
                .FirstOrDefault(n => n != null && n.StartsWith("OxyPlot.Avalonia"));
            if (!string.IsNullOrEmpty(oxyPlotAssembly))
            {
                var oxyPlotStyles = new StyleInclude(new Uri("avares://" + oxyPlotAssembly))
                {
                    Source = new Uri("avares://" + oxyPlotAssembly + "/Themes/Default.axaml")
                };
                Add(oxyPlotStyles);
            }
        }
    }
}