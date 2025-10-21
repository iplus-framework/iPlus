using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBFlowDocument'}de{'VBFlowDocument'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBFlowDocument : Control  // Avalonia doesn't have FlowDocument, using Control as base
    {

        #region Properties

        #region Properties - > CodePage

        private int _codePage = 1250;

        public static readonly DirectProperty<VBFlowDocument, int> CodePageProperty =
            AvaloniaProperty.RegisterDirect<VBFlowDocument, int>(
                nameof(CodePage),
                o => o.CodePage,
                (o, v) => o.CodePage = v);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public int CodePage
        {
            get { return _codePage; }
            set { SetAndRaise(CodePageProperty, ref _codePage, value); }
        }

        #endregion

        #region Properties Custom

        private string _custom01;

        public static readonly DirectProperty<VBFlowDocument, string> Custom01Property =
            AvaloniaProperty.RegisterDirect<VBFlowDocument, string>(
                nameof(Custom01),
                o => o.Custom01,
                (o, v) => o.Custom01 = v);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string Custom01
        {
            get { return _custom01; }
            set { SetAndRaise(Custom01Property, ref _custom01, value); }
        }

        private string _custom02;

        public static readonly DirectProperty<VBFlowDocument, string> Custom02Property =
            AvaloniaProperty.RegisterDirect<VBFlowDocument, string>(
                nameof(Custom02),
                o => o.Custom02,
                (o, v) => o.Custom02 = v);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string Custom02
        {
            get { return _custom02; }
            set { SetAndRaise(Custom02Property, ref _custom02, value); }
        }

        private int _customInt01 = -1;

        public static readonly DirectProperty<VBFlowDocument, int> CustomInt01Property =
            AvaloniaProperty.RegisterDirect<VBFlowDocument, int>(
                nameof(CustomInt01),
                o => o.CustomInt01,
                (o, v) => o.CustomInt01 = v);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public int CustomInt01
        {
            get { return _customInt01; }
            set { SetAndRaise(CustomInt01Property, ref _customInt01, value); }
        }

        private int _customInt02 = -1;

        public static readonly DirectProperty<VBFlowDocument, int> CustomInt02Property =
            AvaloniaProperty.RegisterDirect<VBFlowDocument, int>(
                nameof(CustomInt02),
                o => o.CustomInt02,
                (o, v) => o.CustomInt02 = v);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public int CustomInt02
        {
            get { return _customInt02; }
            set { SetAndRaise(CustomInt02Property, ref _customInt02, value); }
        }

        private int _customInt03 = -1;

        public static readonly DirectProperty<VBFlowDocument, int> CustomInt03Property =
            AvaloniaProperty.RegisterDirect<VBFlowDocument, int>(
                nameof(CustomInt03),
                o => o.CustomInt03,
                (o, v) => o.CustomInt03 = v);

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public int CustomInt03
        {
            get { return _customInt03; }
            set { SetAndRaise(CustomInt03Property, ref _customInt03, value); }
        }
        #endregion

        #endregion
    }
}
