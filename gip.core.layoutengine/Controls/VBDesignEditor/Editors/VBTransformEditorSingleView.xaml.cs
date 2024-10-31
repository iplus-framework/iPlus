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
using gip.ext.design;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.ComponentModel;
using gip.core.datamodel;
using gip.ext.designer.OutlineView;

namespace gip.core.layoutengine.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for signle view transformation.
    /// </summary>
    public partial class VBTransformEditorSingleView : INotifyPropertyChanged //: ITypeEditorInitItem
	{
		/// <summary>
        /// Create a new VBTransformEditorSingleView instance.
		/// </summary>
		public VBTransformEditorSingleView()
		{
            InitializeComponent();
            DataContext = this;
            //TransformProperties.DataContext = Property;
        }


        IPropertyNode property;
        public IPropertyNode Property
        {
            get
            {
                return property;
            }
            set
            {
                property = value;
                RefreshView();
                RaisePropertyChanged("Property");
            }
        }

        TransformOutlineNode _OutlineNode;
        public TransformOutlineNode OutlineNode
        {
            get
            {
                return _OutlineNode;
            }
            set
            {
                _OutlineNode = value;
                RefreshView();
                RaisePropertyChanged("OutlineNode");
            }
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

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            if (Property != null)
            {
                Property.Reset();
                RefreshView();
            }            
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
