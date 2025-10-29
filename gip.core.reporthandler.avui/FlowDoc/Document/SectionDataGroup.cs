// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls.Documents;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Provides a section for grouped data
    /// </summary>
    /// <summary xml:lang="de">
    /// Bietet einen Abschnitt für gruppierte Daten.
    /// </summary>
    public class SectionDataGroup : Section
    {
        /// <summary>
        /// Gets or sets the data group name
        /// </summary>
        public string DataGroupName
        {
            get { return (string)GetValue(DataGroupProperty); }
            set { SetValue(DataGroupProperty, value); }
        }

        // Using a StyledProperty as the backing store for DataGroup. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<string> DataGroupProperty = 
            AvaloniaProperty.Register<SectionDataGroup, string>(nameof(DataGroupName), "");

        public virtual string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public static readonly AttachedProperty<string> StringFormatProperty = ReportDocument.StringFormatProperty.AddOwner<SectionDataGroup>();

        public virtual string CultureInfo
        {
            get { return (string)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }
        public static readonly AttachedProperty<string> CultureInfoProperty = ReportDocument.CultureInfoProperty.AddOwner<SectionDataGroup>();
    }
}
