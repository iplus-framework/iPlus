// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using gip.core.datamodel;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace gip.core.reporthandler.avui
{
    /// <summary>
    /// HTML preview browser for Scryber templates in the report editor.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBWebBrowser'}de{'VBWebBrowser'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBWebBrowser : UserControl
    {
        private readonly HtmlPanel _htmlPanel;

        public VBWebBrowser()
        {
            _htmlPanel = new HtmlPanel
            {
                Background = Brushes.Transparent
            };

            Content = _htmlPanel;
        }

        public static readonly StyledProperty<string> HtmlTextProperty =
            AvaloniaProperty.Register<VBWebBrowser, string>(nameof(HtmlText));

        public string HtmlText
        {
            get => GetValue(HtmlTextProperty);
            set => SetValue(HtmlTextProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == HtmlTextProperty)
            {
                _htmlPanel.Text = change.NewValue as string ?? string.Empty;
            }
        }
    }
}