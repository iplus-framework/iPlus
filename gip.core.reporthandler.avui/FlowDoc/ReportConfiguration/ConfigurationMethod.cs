// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using System.Linq;


namespace gip.core.reporthandler.avui.Flowdoc
{
    public class ConfigurationMethod : ItemsControl
    {
        public ConfigurationMethod()
        {
            
        }

        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        // Using a StyledProperty as the backing store for VBContent. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<string> VBContentProperty = 
            AvaloniaProperty.Register<ConfigurationMethod, string>(nameof(VBContent));


        public string MethodName
        {
            get
            {
                return VBContent.Split('\\').Last().Split('(').Last().Split(')').First();
            }
        }

    }
}
