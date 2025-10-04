// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using gip.ext.design.avui.PropertyGrid;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for converter of a following types: <see cref="IValueConverter"/>, <see cref="IMultiValueConverter"/>
    /// </summary>
    [TypeEditor(typeof(IMultiValueConverter))]
    [TypeEditor(typeof(IValueConverter))]
    public partial class VBConverterTypeEditor : UserControl
    {
		public VBConverterTypeEditor()
		{
			InitializeComponent();
            UpdatePseudoClasses();
		}

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == DataContextProperty)
            {
                VBConverterTypeEditor_DataContextChanged(this, change);
            }
            else if (change.Property == IsConverterSetProperty)
            {
                UpdatePseudoClasses();
            }
        }

        private void UpdatePseudoClasses()
        {
            bool isSet = IsConverterSet;
            
            // Set pseudo-classes on this control for styling
            PseudoClasses.Set(":notset", !isSet);
            PseudoClasses.Set(":isset", isSet);
        }

        void VBConverterTypeEditor_DataContextChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            _Property = DataContext as IPropertyNode;
            if (_Property == null)
                IsConverterSet = false;
            else
            {
                if ((_Property.ValueItem == null) 
                    || !(_Property.ValueItem.Component is IValueConverter || _Property.ValueItem.Component is IMultiValueConverter))
                    IsConverterSet = false;
                else
                    IsConverterSet = true;
            }
        }

		VBConverterEditorPopup converterEditorPopup = new VBConverterEditorPopup();
        IPropertyNode _Property;

        public static readonly StyledProperty<bool> IsConverterSetProperty =
            AvaloniaProperty.Register<VBConverterTypeEditor, bool>(nameof(IsConverterSet), false);
        
        public bool IsConverterSet
        {
            get { return GetValue(IsConverterSetProperty); }
            set { SetValue(IsConverterSetProperty, value); }
        }

        /// <summary>
        /// Handles the OnPointerReleased event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (_Property == null)
                return;
            converterEditorPopup.VBConverterEditorView.Property = _Property;
            converterEditorPopup.PlacementTarget = this;
            converterEditorPopup.IsOpen = true;
            
            base.OnPointerReleased(e);
        }

        public VBConverterEditorView ConverterEditorView
        {
            get
            {
                if ((converterEditorPopup.VBConverterEditorView != null) && (converterEditorPopup.VBConverterEditorView.Property != _Property))
                    converterEditorPopup.VBConverterEditorView.Property = _Property;
                return converterEditorPopup.VBConverterEditorView;
            }
        }
    }
}
