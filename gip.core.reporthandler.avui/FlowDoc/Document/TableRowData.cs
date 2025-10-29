// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Represents the row data for table in reports.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt die Zeilendaten für die Tabelle in Berichten dar.
    /// </summary>
    public class TableRowData : TableRowDataBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TableRowData()
        {
        }

        public bool Configuration
        {
            get { return (bool)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        // Using a StyledProperty as the backing store for Configuration. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<bool> ConfigurationProperty = 
            AvaloniaProperty.Register<TableRowData, bool>(nameof(Configuration), false);
    }
}
