// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

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
using gip.ext.design.PropertyGrid;
using System.Windows.Controls.Primitives;
using gip.ext.design;

namespace gip.core.layoutengine.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for tranformation of <see cref="Transform"/> type.
    /// </summary>
    [TypeEditor(typeof(Transform))]
    public partial class VBTransformTypeEditor
	{
		public VBTransformTypeEditor()
		{
			InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(VBTransformTypeEditor_DataContextChanged);
		}

        void VBTransformTypeEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        public static readonly DependencyProperty IsTransformSetProperty
            = DependencyProperty.Register("IsTransformSet", typeof(bool), typeof(VBTransformTypeEditor), new PropertyMetadata(false));
        public bool IsTransformSet
        {
            get { return (bool)GetValue(IsTransformSetProperty); }
            set { SetValue(IsTransformSetProperty, value); }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
        }


		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
            if (_Property == null)
                return;
            TransformEditorPopup.VBTransformEditorView.Property = _Property;
			TransformEditorPopup.PlacementTarget = this;
			TransformEditorPopup.IsOpen = true;
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
