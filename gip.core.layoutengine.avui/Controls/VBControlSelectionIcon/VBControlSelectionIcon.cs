using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.ComponentModel;
using Avalonia.Controls.Metadata;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia;

namespace gip.core.layoutengine.avui
{

    [TemplatePart(Name = "PART_Lense_Fill", Type = typeof(Control))]
    public class VBControlSelectionIcon : Button
    {
        public VBControlSelectionIcon() : base()
        {
            // Animation is now handled declaratively in XAML
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            object partObj = (object)e.NameScope.Find("PART_Lense_Fill");
            if ((partObj != null) && (partObj is Shape))
            {
                _PART_Lense_Fill = ((Shape)partObj);
            }
        }

        public static readonly StyledProperty<bool> ControlSelectionActiveProperty = 
            AvaloniaProperty.Register<VBControlSelectionIcon, bool>(nameof(ControlSelectionActive));
        
        public bool ControlSelectionActive
        {
            get { return GetValue(ControlSelectionActiveProperty); }
            set { SetValue(ControlSelectionActiveProperty, value); }
        }

        public static readonly StyledProperty<short> IconTypeProperty = 
            AvaloniaProperty.Register<VBControlSelectionIcon, short>(nameof(IconType));
        
        [Category("VBControl")]
        public short IconType
        {
            get { return GetValue(IconTypeProperty); }
            set { SetValue(IconTypeProperty, value); }
        }

        private Shape _PART_Lense_Fill;
        public Shape PART_Lense_Fill
        {
            get
            {
                return _PART_Lense_Fill;
            }
        }

        void VBControlSelectionIcon_Click(object sender, RoutedEventArgs e)
        {
            SwitchControlSelectionState();
        }

        public void SwitchControlSelectionState()
        {
            ControlSelectionActive = !ControlSelectionActive;
            // Animation is now handled declaratively in XAML via TemplateBinding
        }
    }
}
