// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls.Documents;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Abstract class for fillable run values
    /// </summary>
    public abstract class InlineUIValueBase : InlineUIContainer, IInlinePropertyValue
    {
        public virtual string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public static readonly AttachedProperty<string> StringFormatProperty = ReportDocument.StringFormatProperty.AddOwner<InlineUIValueBase>();

        public virtual string CultureInfo
        {
            get { return (string)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }
        public static readonly AttachedProperty<string> CultureInfoProperty = ReportDocument.CultureInfoProperty.AddOwner<InlineUIValueBase>();

        public virtual int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }
        public static readonly AttachedProperty<int> MaxLengthProperty = ReportDocument.MaxLengthProperty.AddOwner<InlineUIValueBase>();

        public virtual int Truncate
        {
            get { return (int)GetValue(TruncateProperty); }
            set { SetValue(TruncateProperty, value); }
        }
        public static readonly AttachedProperty<int> TruncateProperty = ReportDocument.TruncateProperty.AddOwner<InlineUIValueBase>();

        public virtual double MaxWidth
        {
            get { return (double)GetValue(MaxWidthProperty); }
            set { SetValue(MaxWidthProperty, value); }
        }
        public static readonly StyledProperty<double> MaxWidthProperty = 
            AvaloniaProperty.Register<InlineUIValueBase, double>(nameof(MaxWidth), 0.0);

        public virtual double MaxHeight
        {
            get { return (double)GetValue(MaxHeightProperty); }
            set { SetValue(MaxHeightProperty, value); }
        }
        public static readonly StyledProperty<double> MaxHeightProperty = 
            AvaloniaProperty.Register<InlineUIValueBase, double>(nameof(MaxHeight), 0.0);

        public int FontWidth
        {
            get { return (int)GetValue(FontWidthProperty); }
            set { SetValue(FontWidthProperty, value); }
        }
        public static readonly StyledProperty<int> FontWidthProperty = 
            AvaloniaProperty.Register<InlineUIValueBase, int>(nameof(FontWidth), 0);

        public static readonly StyledProperty<int> XPosProperty = 
            AvaloniaProperty.Register<InlineUIValueBase, int>(nameof(XPos), 0);
        public int XPos
        {
            get { return (int)GetValue(XPosProperty); }
            set { SetValue(XPosProperty, value); }
        }

        public static readonly StyledProperty<int> YPosProperty = 
            AvaloniaProperty.Register<InlineUIValueBase, int>(nameof(YPos), 0);
        public int YPos
        {
            get { return (int)GetValue(YPosProperty); }
            set { SetValue(YPosProperty, value); }
        }

        /// <summary>
        /// Gets or sets the object value
        /// </summary>
        public virtual object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly StyledProperty<object> ValueProperty = 
            AvaloniaProperty.Register<InlineUIValueBase, object>(nameof(Value));

        /// <summary>
        /// Gets or sets the Key of ReportData-Dictionary
        /// </summary>
        public virtual string DictKey
        {
            get { return (string)GetValue(DictKeyProperty); }
            set { SetValue(DictKeyProperty, value); }
        }
        public static readonly StyledProperty<string> DictKeyProperty = 
            AvaloniaProperty.Register<InlineUIValueBase, string>(nameof(DictKey));

        /// <summary>
        /// Gets or sets the property name
        /// </summary>
        public virtual string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }
        public static readonly StyledProperty<string> VBContentProperty = 
            AvaloniaProperty.Register<InlineUIValueBase, string>(nameof(VBContent));

        private string _aggregateGroup = null;
        /// <summary>
        /// Gets or sets the aggregate group
        /// </summary>
        public string AggregateGroup
        {
            get { return _aggregateGroup; }
            set { _aggregateGroup = value; }
        }
    }
}
