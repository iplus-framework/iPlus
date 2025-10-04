// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using System.Collections.Generic;
using System.ComponentModel;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for signle view transformation.
    /// </summary>
    public partial class VBTransformEditorSingleView : UserControl
	{
		/// <summary>
        /// Create a new VBTransformEditorSingleView instance.
		/// </summary>
		public VBTransformEditorSingleView()
		{
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Defines the Property styled property.
        /// </summary>
        public static readonly StyledProperty<IPropertyNode> PropertyProperty =
            AvaloniaProperty.Register<VBTransformEditorSingleView, IPropertyNode>(nameof(Property));

        /// <summary>
        /// Gets or sets the property node.
        /// </summary>
        public IPropertyNode Property
        {
            get => GetValue(PropertyProperty);
            set => SetValue(PropertyProperty, value);
        }

        /// <summary>
        /// Defines the OutlineNode styled property.
        /// </summary>
        public static readonly StyledProperty<TransformOutlineNode> OutlineNodeProperty =
            AvaloniaProperty.Register<VBTransformEditorSingleView, TransformOutlineNode>(nameof(OutlineNode));

        /// <summary>
        /// Gets or sets the outline node.
        /// </summary>
        public TransformOutlineNode OutlineNode
        {
            get => GetValue(OutlineNodeProperty);
            set => SetValue(OutlineNodeProperty, value);
        }

        static VBTransformEditorSingleView()
        {
            // Register property changed handlers to call RefreshView
            PropertyProperty.Changed.AddClassHandler<VBTransformEditorSingleView>((sender, e) => sender.RefreshView());
            OutlineNodeProperty.Changed.AddClassHandler<VBTransformEditorSingleView>((sender, e) => sender.RefreshView());
        }

        public VBTransformEditorGroupView ParentGroupEditor
        {
            get;
            set;
        }

        protected DesignItem DesignObject
        {
            get
            {
                if (Property != null)
                    return Property.ValueItem;
                else if (OutlineNode != null)
                    return OutlineNode.DesignItem;
                return null;
            }
        }


        private void RefreshView()
        {
            if (DesignObject != null)
            {
                if ((DesignObject.Component is RotateTransform) && (ComboTransform.SelectedIndex != 0))
                    ComboTransform.SelectedIndex = 0;
                else if ((DesignObject.Component is ScaleTransform) && (ComboTransform.SelectedIndex != 1))
                    ComboTransform.SelectedIndex = 1;
                else if ((DesignObject.Component is SkewTransform) && (ComboTransform.SelectedIndex != 2))
                    ComboTransform.SelectedIndex = 2;
                else if ((DesignObject.Component is TranslateTransform) && (ComboTransform.SelectedIndex != 3))
                    ComboTransform.SelectedIndex = 3;
                else if ((DesignObject.Component is MatrixTransform) && (ComboTransform.SelectedIndex != 4))
                    ComboTransform.SelectedIndex = 4;
            }
            else
                ComboTransform.SelectedIndex = -1;


            List<DesignItem> selection = new List<DesignItem>();
            if (DesignObject != null)
                selection.Add(DesignObject);
            TransformProperties.PropertyGrid.AttachedPropFilter = true;
            TransformProperties.PropertyGrid.SelectedItems = selection;

            //RaisePropertyChanged("GlobalFunction");
            //RaisePropertyChanged("ACUrlCommand");
            //RaisePropertyChanged("Expression");
            //RaisePropertyChanged("ConversionBy");
        }

        List<DesignItem> _Selection = new List<DesignItem>();

        private bool _InSelectionUpdate = false;
        void ComboTransform_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_InSelectionUpdate)
                return;

            if ((Property != null) && (Property.Value != null))
            {
                switch (ComboTransform.SelectedIndex)
                {
                    case 0:
                        if (!(Property.Value is RotateTransform))
                        {
                            Property.Reset();
                            Property.Value = new RotateTransform();
                        }
                        break;
                    case 1:
                        if (!(Property.Value is ScaleTransform))
                        {
                            Property.Reset();
                            Property.Value = new ScaleTransform();
                        }
                        break;
                    case 2:
                        if (!(Property.Value is SkewTransform))
                        {
                            Property.Reset();
                            Property.Value = new SkewTransform();
                        }
                        break;
                    case 3:
                        if (!(Property.Value is TranslateTransform))
                        {
                            Property.Reset();
                            Property.Value = new TranslateTransform();
                        }
                        break;
                    case 4:
                        if (!(Property.Value is MatrixTransform))
                        {
                            Property.Reset();
                            Property.Value = new MatrixTransform();
                        }
                        break;
                    default:
                        Property.Reset();
                        break;
                }
            }
            else if ((OutlineNode != null) && (ParentGroupEditor != null))
            {
                switch (ComboTransform.SelectedIndex)
                {
                    case 0:
                        if (!(OutlineNode.DesignItem.Component is RotateTransform))
                        {
                            ParentGroupEditor.ReplaceWithNewTransformObject(OutlineNode, new RotateTransform());
                        }
                        break;
                    case 1:
                        if (!(OutlineNode.DesignItem.Component is ScaleTransform))
                        {
                            ParentGroupEditor.ReplaceWithNewTransformObject(OutlineNode, new ScaleTransform());
                        }
                        break;
                    case 2:
                        if (!(OutlineNode.DesignItem.Component is SkewTransform))
                        {
                            ParentGroupEditor.ReplaceWithNewTransformObject(OutlineNode, new SkewTransform());
                        }
                        break;
                    case 3:
                        if (!(OutlineNode.DesignItem.Component is TranslateTransform))
                        {
                            ParentGroupEditor.ReplaceWithNewTransformObject(OutlineNode, new TranslateTransform());
                        }
                        break;
                    case 4:
                        if (!(OutlineNode.DesignItem.Component is MatrixTransform))
                        {
                            ParentGroupEditor.ReplaceWithNewTransformObject(OutlineNode, new MatrixTransform());
                        }
                        break;
                }
            }
            RefreshView();
        }

    }
}
