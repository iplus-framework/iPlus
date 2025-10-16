using Avalonia.Controls;
using Avalonia.Input;
using gip.core.datamodel;
using System.ComponentModel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control for scrollbars.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement für Scrollbalken
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBScrollViewer'}de{'VBScrollViewer'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBScrollViewer : ScrollViewer
    {


        private bool _IsEnabledScrollOnLeftCtrlKeyDown = true;
        [Category("VBControl")]
        public bool IsEnabledScrollOnLeftCtrlKeyDown
        {
            get
            {
                return _IsEnabledScrollOnLeftCtrlKeyDown;
            }
            set
            {
                _IsEnabledScrollOnLeftCtrlKeyDown = value;
            }
        }

        /// <summary>
        /// Handles the OnMouseLeftButtonDown event.
        /// </summary>
        /// <param name="e">The MouseButtonEvent arguments.</param>
        //protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        //{
        //    bool handled = e.Handled;
        //    base.OnMouseLeftButtonDown(e);
        //    // Bug in WPF!!!!
        //    e.Handled = handled;
        //}

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            if (!IsEnabledScrollOnLeftCtrlKeyDown && e.KeyModifiers == KeyModifiers.Control)
                return;
            base.OnPointerWheelChanged(e);
        }
    }
}
