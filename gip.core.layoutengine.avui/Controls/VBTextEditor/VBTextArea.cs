using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Markup;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows.Controls.Primitives;

namespace gip.core.layoutengine.avui
{
    public class VBTextArea : TextArea
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TextAreaStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTextEditor/Themes/TextAreaStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TextAreaStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTextEditor/Themes/TextAreaStyleAero.xaml" },
        };
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBTextArea()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTextArea), new FrameworkPropertyMetadata(typeof(VBTextArea)));
        }

        bool _themeApplied = false;
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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        public override void OnApplyTemplate()
        {
            if (this.Style == null)
            {
                if (ControlManager.WpfTheme == eWpfTheme.Aero)
                    Style = ControlManager.GetStyleOfTheme(StyleInfoList);
            }
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);

            statusBar = (StatusBar)Template.FindName("PART_StatusBar", this);
            statusBarItemCaretLineInfo = (TextBlock)Template.FindName("PART_StatusBarItemCaretLineInfo", this);
            statusBarItemCaretColInfo = (TextBlock)Template.FindName("PART_StatusBarItemCaretColInfo", this);
        }

        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBTextArea));

        public Global.ControlModes ControlMode
        {
            get
            {
                return (Global.ControlModes)GetValue(ControlModeProperty);
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
