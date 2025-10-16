using Avalonia.Controls.Primitives;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBPasswordBox'}de{'VBPasswordBox'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBPasswordBox : VBTextBox
    {
        public VBPasswordBox()
            : base()
        {
            this.PasswordChar = '•';
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            this.PasswordChar = '•';
            base.OnApplyTemplate(e);
        }
    }
}
