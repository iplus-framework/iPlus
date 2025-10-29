// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Abstract class for fillable run values
    /// </summary>
    public abstract class InlinePropertyValueBase : InlineValueBase, IInlinePropertyValue, IAggregateValue
    {
        /// <summary>
        /// Gets or sets the Key of ReportData-Dictionary
        /// </summary>
        public virtual string DictKey
        {
            get { return (string)GetValue(DictKeyProperty); }
            set { SetValue(DictKeyProperty, value); }
        }

        // Using a StyledProperty as the backing store for DictKey. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<string> DictKeyProperty = 
            AvaloniaProperty.Register<InlinePropertyValueBase, string>(nameof(DictKey));

        /// <summary>
        /// Gets or sets the property name
        /// </summary>
        public virtual string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        // Using a StyledProperty as the backing store for VBContent. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<string> VBContentProperty = 
            AvaloniaProperty.Register<InlinePropertyValueBase, string>(nameof(VBContent));

        private string _aggregateGroup = null;
        /// <summary>
        /// Gets or sets the aggregate group
        /// </summary>
        public string AggregateGroup
        {
            get { return _aggregateGroup; }
            set { _aggregateGroup = value; }
        }

        public static readonly StyledProperty<int> CustomInt01Property = 
            AvaloniaProperty.Register<InlinePropertyValueBase, int>(nameof(CustomInt01), -1);
        public int CustomInt01
        {
            get { return (int)GetValue(CustomInt01Property); }
            set { SetValue(CustomInt01Property, value); }
        }

        public static readonly StyledProperty<int> CustomInt02Property = 
            AvaloniaProperty.Register<InlinePropertyValueBase, int>(nameof(CustomInt02), -1);
        public int CustomInt02
        {
            get { return (int)GetValue(CustomInt02Property); }
            set { SetValue(CustomInt02Property, value); }
        }

        public static readonly StyledProperty<int> CustomInt03Property = 
            AvaloniaProperty.Register<InlinePropertyValueBase, int>(nameof(CustomInt03), -1);
        public int CustomInt03
        {
            get { return (int)GetValue(CustomInt03Property); }
            set { SetValue(CustomInt03Property, value); }
        }

        public static readonly StyledProperty<int> XPosProperty = 
            AvaloniaProperty.Register<InlinePropertyValueBase, int>(nameof(XPos), 0);
        public int XPos
        {
            get { return (int)GetValue(XPosProperty); }
            set { SetValue(XPosProperty, value); }
        }

        public static readonly StyledProperty<int> YPosProperty = 
            AvaloniaProperty.Register<InlinePropertyValueBase, int>(nameof(YPos), 0);
        public int YPos
        {
            get { return (int)GetValue(YPosProperty); }
            set { SetValue(YPosProperty, value); }
        }
    }
}
