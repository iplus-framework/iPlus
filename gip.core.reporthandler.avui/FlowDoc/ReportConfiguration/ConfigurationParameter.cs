// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using System;


namespace gip.core.reporthandler.avui.Flowdoc
{
    public class ConfigurationParameter : StyledElement
    {
        public ConfigurationParameter()
        {

        }

        public string ParameterName
        {
            get { return (string)GetValue(ParameterNameProperty); }
            set { SetValue(ParameterNameProperty, value); }
        }

        // Using a StyledProperty as the backing store for ParameterName. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<string> ParameterNameProperty = 
            AvaloniaProperty.Register<ConfigurationParameter, string>(nameof(ParameterName));
    }
}
