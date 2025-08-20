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
using gip.ext.design.avui.PropertyGrid;
using System.Windows.Controls.Primitives;
using gip.ext.design.avui;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    //[TypeEditor(typeof(ConverterObject))]
    //[TypeEditor(typeof(ConverterObjectMulti))]
    //[TypeEditor(typeof(ConverterBoolean))]
    //[TypeEditor(typeof(ConverterBooleanMulti))]
    //[TypeEditor(typeof(ConverterByte))]
    //[TypeEditor(typeof(ConverterByteMulti))]
    //[TypeEditor(typeof(ConverterChar))]
    //[TypeEditor(typeof(ConverterCharMulti))]
    //[TypeEditor(typeof(ConverterDateTime))]
    //[TypeEditor(typeof(ConverterDateTimeMulti))]
    //[TypeEditor(typeof(ConverterTimeSpan))]
    //[TypeEditor(typeof(ConverterTimeSpanMulti))]
    //[TypeEditor(typeof(ConverterDecimal))]
    //[TypeEditor(typeof(ConverterDecimalMulti))]
    //[TypeEditor(typeof(ConverterDouble))]
    //[TypeEditor(typeof(ConverterDoubleMulti))]
    //[TypeEditor(typeof(ConverterInt16))]
    //[TypeEditor(typeof(ConverterInt16Multi))]
    //[TypeEditor(typeof(ConverterInt32))]
    //[TypeEditor(typeof(ConverterInt32Multi))]
    //[TypeEditor(typeof(ConverterInt64))]
    //[TypeEditor(typeof(ConverterInt64Multi))]
    //[TypeEditor(typeof(ConverterSByte))]
    //[TypeEditor(typeof(ConverterSByteMulti))]
    //[TypeEditor(typeof(ConverterSingle))]
    //[TypeEditor(typeof(ConverterSingleMulti))]
    //[TypeEditor(typeof(ConverterString))]
    //[TypeEditor(typeof(ConverterStringMulti))]
    //[TypeEditor(typeof(ConverterUInt16))]
    //[TypeEditor(typeof(ConverterUInt16Multi))]
    //[TypeEditor(typeof(ConverterUInt32))]
    //[TypeEditor(typeof(ConverterUInt32Multi))]
    //[TypeEditor(typeof(ConverterUInt64))]
    //[TypeEditor(typeof(ConverterUInt64Multi))]
    //[TypeEditor(typeof(ConverterBrushSingle))]
    //[TypeEditor(typeof(ConverterBrushMulti))]
    //[TypeEditor(typeof(ConverterFontFamilySingle))]
    //[TypeEditor(typeof(ConverterFontFamilyMulti))]
    //[TypeEditor(typeof(ConverterFontStyleSingle))]
    //[TypeEditor(typeof(ConverterFontStyleMulti))]
    //[TypeEditor(typeof(ConverterFontWeightSingle))]
    //[TypeEditor(typeof(ConverterFontWeightMulti))]
    //[TypeEditor(typeof(ConverterHorizontalAlignmentSingle))]
    //[TypeEditor(typeof(ConverterHorizontalAlignmentMulti))]
    //[TypeEditor(typeof(ConverterThicknessSingle))]
    //[TypeEditor(typeof(ConverterThicknessMulti))]
    //[TypeEditor(typeof(ConverterVerticalAlignmentSingle))]
    //[TypeEditor(typeof(ConverterVerticalAlignmentMulti))]
    //[TypeEditor(typeof(ConverterVisibilitySingle))]
    //[TypeEditor(typeof(ConverterVisibilityMulti))]

    /// <summary>
    /// Represents a editor for converter of a following types: <see cref="IValueConverter"/>, <see cref="IMultiValueConverter"/>
    /// </summary>
    [TypeEditor(typeof(IMultiValueConverter))]
    [TypeEditor(typeof(IValueConverter))]
    public partial class VBConverterTypeEditor
	{
		public VBConverterTypeEditor()
		{
			InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(VBConverterTypeEditor_DataContextChanged);
		}

        void VBConverterTypeEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        public static readonly DependencyProperty IsConverterSetProperty
            = DependencyProperty.Register("IsConverterSet", typeof(bool), typeof(VBConverterTypeEditor), new PropertyMetadata(false));
        public bool IsConverterSet
        {
            get { return (bool)GetValue(IsConverterSetProperty); }
            set { SetValue(IsConverterSetProperty, value); }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
        }


		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
            if (_Property == null)
                return;
            converterEditorPopup.VBConverterEditorView.Property = _Property;
			converterEditorPopup.PlacementTarget = this;
			converterEditorPopup.IsOpen = true;
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
