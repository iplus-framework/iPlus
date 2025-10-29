// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Contains a configurable report value is to be displayed on the report.
    /// </summary>
    /// <summary xml:lang="de">
    /// Enthält einen konfigurierbaren Berichtswert, der auf dem Bericht angezeigt werden soll.
    /// </summary>
    public class InlineTableCellConfigurationValue : InlineTableCellValue
    {

        public int ParameterNameIndex
        {
            get { return (int)GetValue(ParameterNameIndexProperty); }
            set { SetValue(ParameterNameIndexProperty, value); }
        }

        // Using a StyledProperty as the backing store for ParameterNameIndex. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<int> ParameterNameIndexProperty = 
            AvaloniaProperty.Register<InlineTableCellConfigurationValue, int>(nameof(ParameterNameIndex));

    }
}
