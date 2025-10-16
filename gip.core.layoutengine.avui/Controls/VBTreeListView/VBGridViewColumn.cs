using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the column that displays data.
    /// </summary>
    public class VBGridViewColumn : GridViewColumn
    {
        static VBGridViewColumn()
        {
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner(typeof(VBGridViewColumn), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        }

        /// <summary>
        /// Creates a new instance of VBGridViewColumn.
        /// </summary>
        public VBGridViewColumn() : base()
        {

        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBGridViewColumn));

        /// <summary>
        /// Gets or sets the VBContent.
        /// </summary>
        [Category("VBControl")]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for StringFormat.
        /// </summary>
        public static readonly DependencyProperty StringFormatProperty;
        /// <summary>
        /// Gets or sets the string format for the control.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt das Stringformat für das Steuerelement.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        private bool _IsSortEnabled = false;
        /// <summary>
        /// Enables or disables sort by this column.
        /// </summary>
        [Category("VBControl")]
        public bool IsSortEnabled
        {
            get => _IsSortEnabled;
            set
            {
                _IsSortEnabled = value;
            }
        }
    }
}
