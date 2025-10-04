// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using gip.ext.design.avui.PropertyGrid;
using System;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for bidning of follwoing types: <see cref="Binding"/>, <see cref="MultiBinding"/>, <see cref="VBBinding"/> and <see cref="VBBindingExt"/>
    /// </summary>
    [TypeEditor(typeof(MultiBinding))]
    [TypeEditor(typeof(Binding))]
    [TypeEditor(typeof(VBBindingExt))]
    [TypeEditor(typeof(VBBinding))]
    public partial class VBBindingTypeEditor : UserControl
	{
		public VBBindingTypeEditor()
		{
			InitializeComponent();
            DataContextChanged += VBBindingTypeEditor_DataContextChanged;
            
            // Initialize pseudo-classes
            UpdatePseudoClasses();
		}

        void VBBindingTypeEditor_DataContextChanged(object sender, EventArgs e)
        {
            _Property = DataContext as IPropertyNode;
            if (_Property == null)
                IsBindingSet = false;
            else
            {
                if ((_Property.ValueItem == null)
                    || !(_Property.ValueItem.Component is Binding || _Property.ValueItem.Component is MultiBinding || _Property.ValueItem.Component is VBBindingExt))
                    IsBindingSet = false;
                else
                    IsBindingSet = true;
            }

            // Update pseudo-classes for styling based on IsSet state
            UpdatePseudoClasses();
        }

        private void UpdatePseudoClasses()
        {
            bool isSet = _Property?.IsSet ?? false;
            
            // Set pseudo-classes on this control for styling
            PseudoClasses.Set(":notset", !isSet);
            PseudoClasses.Set(":isset", isSet);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == IsBindingSetProperty)
            {
                UpdatePseudoClasses();
            }
        }

		VBBindingEditorPopup converterEditorPopup = new VBBindingEditorPopup();
        IPropertyNode _Property;

        public static readonly StyledProperty<bool> IsBindingSetProperty =
            AvaloniaProperty.Register<VBBindingTypeEditor, bool>(nameof(IsBindingSet), false);

        public bool IsBindingSet
        {
            get { return GetValue(IsBindingSetProperty); }
            set { SetValue(IsBindingSetProperty, value); }
        }

        /// <summary>
        /// Handles the OnPointerPressed event for right button.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                gip.ext.designer.avui.PropertyGrid.PropertyContextMenu contextMenu = new gip.ext.designer.avui.PropertyGrid.PropertyContextMenu();
                contextMenu.DataContext = _Property;
                contextMenu.Placement = PlacementMode.Bottom;
                contextMenu.HorizontalOffset = -30;
                contextMenu.PlacementTarget = this;
                // In Avalonia, use Open method to display the context menu
                contextMenu.Open(this);
                e.Handled = true;
            }
            else
            {
                base.OnPointerPressed(e);
            }
        }

		protected override void OnPointerReleased(PointerReleasedEventArgs e)
		{
            if (_Property == null)
                return;
            converterEditorPopup.Property = _Property;
			converterEditorPopup.PlacementTarget = this;
			converterEditorPopup.IsOpen = true;
		}
    }
}
