using System.Windows;

namespace gip.core.reporthandlerwpf.Flowdoc
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

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DictKeyProperty =
            DependencyProperty.Register("DictKey", typeof(string), typeof(InlinePropertyValueBase), new UIPropertyMetadata(null));
       
        /// <summary>
        /// Gets or sets the property name
        /// </summary>
        public virtual string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VBContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VBContentProperty =
            DependencyProperty.Register("VBContent", typeof(string), typeof(InlinePropertyValueBase), new UIPropertyMetadata(null));

        private string _aggregateGroup = null;
        /// <summary>
        /// Gets or sets the aggregate group
        /// </summary>
        public string AggregateGroup
        {
            get { return _aggregateGroup; }
            set { _aggregateGroup = value; }
        }
		
		public static readonly DependencyProperty CustomInt01Property = DependencyProperty.Register("CustomInt01", typeof(int), typeof(InlinePropertyValueBase), new PropertyMetadata(-1));
        public int CustomInt01
        {
            get { return (int)GetValue(CustomInt01Property); }
            set { SetValue(CustomInt01Property, value); }
        }

        public static readonly DependencyProperty CustomInt02Property = DependencyProperty.Register("CustomInt02", typeof(int), typeof(InlinePropertyValueBase), new PropertyMetadata(-1));
        public int CustomInt02
        {
            get { return (int)GetValue(CustomInt02Property); }
            set { SetValue(CustomInt02Property, value); }
        }

        public static readonly DependencyProperty CustomInt03Property = DependencyProperty.Register("CustomInt03", typeof(int), typeof(InlinePropertyValueBase), new PropertyMetadata(-1));
        public int CustomInt03
        {
            get { return (int)GetValue(CustomInt03Property); }
            set { SetValue(CustomInt03Property, value); }
        }

        public static readonly DependencyProperty XPosProperty = DependencyProperty.Register("XPos", typeof(int), typeof(InlinePropertyValueBase), new PropertyMetadata(0));
        public int XPos
        {
            get { return (int)GetValue(XPosProperty); }
            set { SetValue(XPosProperty, value); }
        }

        public static readonly DependencyProperty YPosProperty = DependencyProperty.Register("YPos", typeof(int), typeof(InlinePropertyValueBase), new PropertyMetadata(0));
        public int YPos
        {
            get { return (int)GetValue(YPosProperty); }
            set { SetValue(YPosProperty, value); }
        }
    }
}
