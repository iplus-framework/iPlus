using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using AvaloniaEdit.Editing;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using StatusBar.Avalonia;
using StatusBar.Avalonia.Controls;

namespace gip.core.layoutengine.avui
{
    public class VBTextArea : TextArea
    {
        public VBTextArea() : base()
        {
            Caret.PositionChanged += Caret_PositionChanged;
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            if (statusBarItemCaretLineInfo != null)
                statusBarItemCaretLineInfo.Text = String.Format("L {0}", Caret.Location.Line);
            if (statusBarItemCaretColInfo != null)
                statusBarItemCaretColInfo.Text = String.Format("C {0}", Caret.Location.Column);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            statusBar = e.NameScope.Find<StatusBar>("PART_StatusBar");
            statusBarItemCaretLineInfo = e.NameScope.Find<TextBlock>("PART_StatusBarItemCaretLineInfo");
            statusBarItemCaretColInfo = e.NameScope.Find<TextBlock>("PART_StatusBarItemCaretColInfo");
        }

        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBTextArea, Global.ControlModes>(nameof(ControlMode));

        public Global.ControlModes ControlMode
        {
            get
            {
                return GetValue(ControlModeProperty);
            }
            set
            {
                SetValue(ControlModeProperty, value);
            }
        }

        public StatusBar statusBar;

        public TextBlock statusBarItemCaretLineInfo;

        public TextBlock statusBarItemCaretColInfo;

        public void DeInitVBControl()
        {
            Caret.PositionChanged -= Caret_PositionChanged;
            statusBar = null;
            statusBarItemCaretColInfo = null;
            statusBarItemCaretLineInfo = null;
        }

    }
}
