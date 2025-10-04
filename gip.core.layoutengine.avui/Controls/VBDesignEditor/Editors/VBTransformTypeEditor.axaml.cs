// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using gip.ext.design.avui.PropertyGrid;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for tranformation of <see cref="Transform"/> type.
    /// </summary>
    [TypeEditor(typeof(Transform))]
    public partial class VBTransformTypeEditor : UserControl
    {
		public VBTransformTypeEditor()
		{
			InitializeComponent();
            UpdatePseudoClasses();
		}

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == DataContextProperty)
            {
                VBTransformTypeEditor_DataContextChanged(this, change);
            }
            else if (change.Property == IsTransformSetProperty)
            {
                UpdatePseudoClasses();
            }
        }

        private void UpdatePseudoClasses()
        {
            bool isSet = IsTransformSet;
            
            // Set pseudo-classes on this control for styling
            PseudoClasses.Set(":notset", !isSet);
            PseudoClasses.Set(":isset", isSet);
        }

        void VBTransformTypeEditor_DataContextChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            _Property = DataContext as IPropertyNode;
            if (_Property == null)
                IsTransformSet = false;
            else
            {
                if ((_Property.ValueItem == null)
                    || !(_Property.ValueItem.Component is Transform))
                    IsTransformSet = false;
                else
                    IsTransformSet = true;
            }
        }

		VBTransformEditorPopup TransformEditorPopup = new VBTransformEditorPopup();
        IPropertyNode _Property;

        public static readonly StyledProperty<bool> IsTransformSetProperty = AvaloniaProperty.Register<VBTransformTypeEditor, bool>(nameof(IsTransformSet), false);        
        public bool IsTransformSet
        {
            get { return GetValue(IsTransformSetProperty); }
            set { SetValue(IsTransformSetProperty, value); }
        }

        /// <summary>
        /// Handles the OnPointerReleased event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (_Property == null)
                return;
            TransformEditorPopup.VBTransformEditorView.Property = _Property;
			TransformEditorPopup.PlacementTarget = this;
			TransformEditorPopup.IsOpen = true;
            
            base.OnPointerReleased(e);
		}

        public VBTransformEditorView TransformEditorView
        {
            get
            {
                if ((TransformEditorPopup.VBTransformEditorView != null) && (TransformEditorPopup.VBTransformEditorView.Property != _Property))
                    TransformEditorPopup.VBTransformEditorView.Property = _Property;
                return TransformEditorPopup.VBTransformEditorView;
            }
        }
    }
}
