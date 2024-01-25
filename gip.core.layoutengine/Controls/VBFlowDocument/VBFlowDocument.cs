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

        public static readonly DependencyProperty CodePageProperty
            = DependencyProperty.Register("CodePage", typeof(int), typeof(VBFlowDocument), new PropertyMetadata(1250));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public int CodePage
        {
            get { return (int)GetValue(CodePageProperty); }
            set { SetValue(CodePageProperty, value); }
        }

        #endregion

        #region Properties - > CodePage

        public static readonly DependencyProperty Custom01Property
            = DependencyProperty.Register("Custom01Property", typeof(string), typeof(VBFlowDocument), new PropertyMetadata());

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string Custom01
        {
            get { return (string)GetValue(Custom01Property); }
            set { SetValue(Custom01Property, value); }
        }

        #endregion

        #endregion
    }
}
