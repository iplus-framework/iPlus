// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a group view editor for transformations.
    /// </summary>
    public partial class VBTransformEditorGroupView : UserControl
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

        /// <summary>
        /// Defines the Property styled property.
        /// </summary>
        public static readonly StyledProperty<IPropertyNode> PropertyProperty =
            AvaloniaProperty.Register<VBTransformEditorGroupView, IPropertyNode>(nameof(Property));

        /// <summary>
        /// Gets or sets the property node.
        /// </summary>
        public IPropertyNode Property
        {
            get => GetValue(PropertyProperty);
            set => SetValue(PropertyProperty, value);
        }

        static VBTransformEditorGroupView()
        {
            // Register property changed handler to update transform collection when Property changes
            PropertyProperty.Changed.AddClassHandler<VBTransformEditorGroupView>((sender, e) => sender.OnPropertyChanged(e.NewValue as IPropertyNode));
        }

        private void OnPropertyChanged(IPropertyNode newProperty)
        {
            if (newProperty != null)
            {
                TransformCollection.Clear();
            }

            if (newProperty != null)
            {
                if (newProperty.ValueItem != null)
                {
                    foreach (DesignItem item in newProperty.ValueItem.Properties["Children"].CollectionElements)
                    {
                        TransformCollection.Add(new TransformOutlineNode(item));
                    }
                }
                else
                    TransformCollection.Clear();
            }

            RefreshView();
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

        ObservableCollection<TransformOutlineNode> _TransformCollection = new ObservableCollection<TransformOutlineNode>();
        public ObservableCollection<TransformOutlineNode> TransformCollection
        {
            get { return _TransformCollection; }
        }

        private void RefreshView()
        {
            // Method to refresh the UI state after property changes
            // This can be extended with additional logic as needed
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
                PART_AddItem.ContextMenu.Close();
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
            PART_AddItem.ContextMenu.Close();
            AddNewTransformObject(newTransformObject);
        }
    }
}
