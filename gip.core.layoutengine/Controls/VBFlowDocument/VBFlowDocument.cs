using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace gip.core.layoutengine
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBFlowDocument'}de{'VBFlowDocument'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBFlowDocument : FlowDocument
    {

        #region Properties

        #region Properties - > CodePage

        /// <summary>
        /// Represents the dependency property for WidthCaption.
        /// </summary>
        public static readonly DependencyProperty CodePageProperty
            = DependencyProperty.Register("CodePage", typeof(int), typeof(VBFlowDocument), new PropertyMetadata(1250));
        /// <summary>
        /// Gets or sets the width of caption.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Breite der Beschriftung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public int CodePage
        {
            get { return (int)GetValue(CodePageProperty); }
            set { SetValue(CodePageProperty, value); }
        }

        #endregion

        #endregion
    }
}
