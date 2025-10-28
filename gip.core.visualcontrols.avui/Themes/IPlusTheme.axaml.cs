using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace gip.core.visualcontrols.avui
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
        }
    }
}