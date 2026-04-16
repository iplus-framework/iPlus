using System;
using System.Diagnostics;
using System.IO;
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
    #if LOAD_AVALONIAEDIT_STYLES
            Add(new StyleInclude(new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml")));
            Add(new StyleInclude(new Uri("avares://gip.core.layoutengine.avui/Controls/VBTextEditor/Themes/TextEditorStyle.axaml")));
            Add(new StyleInclude(new Uri("avares://gip.core.layoutengine.avui/Controls/VBTextEditor/Themes/TextAreaStyle.axaml")));
#endif
        }
    }
}