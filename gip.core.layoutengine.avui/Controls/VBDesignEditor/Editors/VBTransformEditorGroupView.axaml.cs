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
using gip.ext.design.avui;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.ComponentModel;
using gip.core.datamodel;
using System.Collections.ObjectModel;
using gip.ext.designer.avui.OutlineView;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a group view editor for transformations.
    /// </summary>
    public partial class VBTransformEditorGroupView : INotifyPropertyChanged //: ITypeEditorInitItem
	{
		/// <summary>
        /// Create a new VBTransformEditorGroupView instance.
		/// </summary>
		public VBTransformEditorGroupView()
		{
            InitializeComponent();
            DataContext = this;
            PART_TransformEditor.ParentGroupEditor = this;
            //TransformProperties.DataContext = null;
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
                if (property != null)
                {
                    TransformCollection.Clear();
                }

                property = value;
                if (property != null)
                {
                    if (property.ValueItem != null)
                    {
                        foreach (DesignItem item in property.ValueItem.Properties["Children"].CollectionElements)
                        {
                            TransformCollection.Add(new TransformOutlineNode(item));
                        }
                    }
                    else
                        TransformCollection.Clear();
                }

                RefreshView();
                RaisePropertyChanged("Property");
            }
        }


        protected DesignItem DesignObject
        {
            get
            {
                if (Property != null)
                    return Property.ValueItem;
                return null;
            }
        }

        private void RefreshView()
        {
            //RaisePropertyChanged("GlobalFunction");
            //RaisePropertyChanged("ACUrlCommand");
            //RaisePropertyChanged("Expression");
            //RaisePropertyChanged("ConversionBy");
        }

        ObservableCollection<TransformOutlineNode> _TransformCollection = new ObservableCollection<TransformOutlineNode>();
        public ObservableCollection<TransformOutlineNode> TransformCollection
        {
            get { return _TransformCollection; }
        }


        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            if (Property == null)
                return;
            Property.Reset();
            RefreshView();
        }

        private bool _InSelectionUpdate = false;
        void ComboTransform_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_InSelectionUpdate)
                return;

            TransformOutlineNode selectedOutlineNode = PART_TransformGroupList.SelectedItem as TransformOutlineNode;
            if (selectedOutlineNode != null)
                PART_TransformEditor.OutlineNode = selectedOutlineNode;
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

        private void PART_AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (Property == null)
                return;
            AddNewTransformObject(new RotateTransform());
        }

        private void PART_RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if ((Property == null) || (Property.ValueItem == null))
                return;
            if (PART_TransformGroupList.SelectedItem == null)
                return;
            TransformOutlineNode outlineNode = PART_TransformGroupList.SelectedItem as TransformOutlineNode;
            TransformCollection.Remove(outlineNode);
            Property.ValueItem.Properties["Children"].CollectionElements.Remove(outlineNode.DesignItem);
            RefreshView();
        }

        public void AddNewTransformObject(Transform newTransformObject)
        {
            if ((Property == null) || (Property.ValueItem == null) || (newTransformObject == null))
                return;
            DesignItem newTransformObjectItem = DesignObject.Services.Component.RegisterComponentForDesigner(newTransformObject);
            Property.ValueItem.Properties["Children"].CollectionElements.Add(newTransformObjectItem);
            TransformOutlineNode newOutlineNode = new TransformOutlineNode(newTransformObjectItem);
            if (newOutlineNode != null)
            {
                TransformCollection.Add(newOutlineNode);
                PART_TransformGroupList.SelectedItem = newOutlineNode;
            }
            RefreshView();
        }

        public void ReplaceWithNewTransformObject(TransformOutlineNode outlineNode, Transform newTransformObject)
        {
            if ((Property == null) || (Property.ValueItem == null) || (newTransformObject == null))
                return;
            Property.ValueItem.Properties["Children"].CollectionElements.Remove(outlineNode.DesignItem);
            DesignItem newTransformObjectItem = DesignObject.Services.Component.RegisterComponentForDesigner(newTransformObject);
            outlineNode.Replace(newTransformObjectItem);
            Property.ValueItem.Properties["Children"].CollectionElements.Add(newTransformObjectItem);
            RefreshView();
        }

        private void VBMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Property == null)
            {
                PART_AddItem.ContextMenu.IsOpen = false;
                return;
            }
            MenuItem itemClicked = sender as MenuItem;
            Transform newTransformObject = null;
            switch (itemClicked.Header as String)
            {
                case "RotateTransform":
                    newTransformObject = new RotateTransform();
                    break;
                case "ScaleTransform":
                    newTransformObject = new ScaleTransform();
                    break;
                case "SkewTransform":
                    newTransformObject = new SkewTransform();
                    break;
                case "TranslateTransform":
                    newTransformObject = new TranslateTransform();
                    break;
                case "MatrixTransform":
                    newTransformObject = new MatrixTransform();
                    break;
            }
            PART_AddItem.ContextMenu.IsOpen = false;
            AddNewTransformObject(newTransformObject);
        }
    }
}
