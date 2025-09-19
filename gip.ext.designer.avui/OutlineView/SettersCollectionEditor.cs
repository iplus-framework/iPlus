using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using gip.ext.designer.avui.OutlineView;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using gip.ext.designer.avui.PropertyGrid;
using System.Linq;
using Avalonia.Controls.Metadata;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace gip.ext.designer.avui.OutlineView
{
    [TemplatePart(Name = "PART_OutlineNodesContext", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_AddItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_RemoveItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_PropertyGridView", Type = typeof(PropertyGridView))]
    [TemplatePart(Name = "PART_OutlineList", Type = typeof(SelectingItemsControl))]
    public class SettersCollectionEditor : TemplatedControl, ITypeEditorInitCollection
    {
        private static readonly Dictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>();

        private DesignItem _DesignObject;
        private DesignItemProperty _SettersCollectionProp;
        private IComponentService _componentService;
        public Control PART_OutlineNodesContext { get; set; }
        public Button PART_ButtonAddItem { get; set; }
        public Button PART_ButtonRemoveItem { get; set; }
        public PropertyGridView PART_PropertyGridView { get; set; }
        public SelectingItemsControl PART_OutlineList { get; set; }

        public SettersCollectionEditor()
        {
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            PART_ButtonAddItem = e.NameScope.Find<Button>("PART_AddItem");
            if (PART_ButtonAddItem != null)
                PART_ButtonAddItem.Click += OnAddItemClicked;

            PART_ButtonRemoveItem = e.NameScope.Find<Button>("PART_RemoveItem");
            if (PART_ButtonRemoveItem != null)
                PART_ButtonRemoveItem.Click += OnRemoveItemClicked;

            PART_PropertyGridView = e.NameScope.Find<PropertyGridView>("PART_PropertyGridView");
            PART_OutlineList = e.NameScope.Find<SelectingItemsControl>("PART_OutlineList");
            PART_OutlineNodesContext = e.NameScope.Find<Control>("PART_OutlineNodesContext");
            
            if (PART_OutlineNodesContext != null)
                PART_OutlineNodesContext.DataContext = OutlineNodeCollection;
                
            if (PART_PropertyGridView != null && _DesignObject != null)
            {
                //PART_PropertyGridView.DataContext = null;
                List<DesignItem> designItemList = new List<DesignItem>();
                designItemList.Add(_DesignObject);
                PART_PropertyGridView.PropertyGrid.SelectedItems = designItemList;
            }
        }

        public static readonly StyledProperty<double> FirstColumnWidthProperty =
            AvaloniaProperty.Register<SettersCollectionEditor, double>(nameof(FirstColumnWidth), 120.0);

        public double FirstColumnWidth
        {
            get { return GetValue(FirstColumnWidthProperty); }
            set { SetValue(FirstColumnWidthProperty, value); }
        }

        public void InitEditor(DesignItem designObject, DesignItemProperty collectionProperty)
        {
            Debug.Assert(designObject.View is Control);
            // TODO: Style erzeugen, falls nicht angelegt
            //DesignItemProperty styleProp = designObject.Properties.GetProperty(FrameworkElement.StyleProperty);
            //if ((styleProp.Value == null) || !(styleProp.Value is DesignItem))
            //    return;
            //DesignItemProperty settersProp = styleProp.Value.Properties.GetProperty("Setters");

            _DesignObject = designObject;
            _SettersCollectionProp = collectionProperty;

            foreach (DesignItem child in _SettersCollectionProp.CollectionElements)
            {
                _OutlineNodeCollection.Add(new SetterOutlineNode(child, designObject));
            }
            _componentService = designObject.Services.Component;
        }

        ObservableCollection<SetterOutlineNode> _OutlineNodeCollection = new ObservableCollection<SetterOutlineNode>();
        public ObservableCollection<SetterOutlineNode> OutlineNodeCollection
        {
            get { return _OutlineNodeCollection; }
        }

        private void OnAddItemClicked(object sender, RoutedEventArgs e)
        {
            if (PART_PropertyGridView.PropertyGrid.SelectedNode != null)
            {
                if (PART_PropertyGridView.PropertyGrid.SelectedNode.IsDependencyProperty)
                {
                    if (OutlineNodeCollection.Where(c => (c.SetterTargetProperty != null) && (c.SetterTargetProperty.Name == PART_PropertyGridView.PropertyGrid.SelectedNode.Name)).Any())
                        return;
                    Setter newSetter = new Setter();
                    DesignItem newSetterItem = _DesignObject.Services.Component.RegisterComponentForDesigner(newSetter);
                    _SettersCollectionProp.CollectionElements.Add(newSetterItem);
                    newSetterItem.Properties["Property"].SetValue(PART_PropertyGridView.PropertyGrid.SelectedNode.FirstProperty.DependencyProperty);
                    object valueOnInstance = PART_PropertyGridView.PropertyGrid.SelectedNode.FirstProperty.NewClonedValueOnInstance;
                    newSetterItem.Properties["Value"].SetValue(valueOnInstance);
                    SetterOutlineNode node = new SetterOutlineNode(newSetterItem, _DesignObject);
                    OutlineNodeCollection.Add(node);
                    PART_PropertyGridView.PropertyGrid.SelectedNode.FirstProperty.Reset();
                    if (node.SetterTargetProperty != null)
                        node.SetterTargetProperty.SetValueOnInstance(valueOnInstance);
                }
            }
        }

        private void OnRemoveItemClicked(object sender, RoutedEventArgs e)
        {
            SetterOutlineNode selectedNode = PART_OutlineList?.SelectedItem as SetterOutlineNode;
            if (selectedNode == null)
                return;
            OutlineNodeCollection.Remove(selectedNode);
            selectedNode.Reset();
            _SettersCollectionProp.CollectionElements.Remove(selectedNode.SetterItem);
        }
    }
}
