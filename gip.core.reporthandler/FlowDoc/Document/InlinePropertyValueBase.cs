using System.Windows;

namespace gip.core.reporthandler.Flowdoc
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
    }
}
