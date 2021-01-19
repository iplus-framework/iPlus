using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using gip.ext.designer.OutlineView;
using gip.ext.design;
using gip.ext.design.PropertyGrid;
using gip.ext.designer.PropertyGrid;
using System.Linq;

namespace gip.ext.designer.OutlineView
{
    [TemplatePart(Name = "PART_OutlineNodesContext", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_AddItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_RemoveItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_PropertyGridView", Type = typeof(PropertyGridView))]
    [TemplatePart(Name = "PART_OutlineList", Type = typeof(Selector))]
    public class SettersCollectionEditor : Control, ITypeEditorInitCollection
    {
        static SettersCollectionEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SettersCollectionEditor), new FrameworkPropertyMetadata(typeof(SettersCollectionEditor)));
        }

        private static readonly Dictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>();

        private DesignItem _DesignObject;
        private DesignItemProperty _SettersCollectionProp;
        private IComponentService _componentService;
        public  FrameworkElement PART_OutlineNodesContext { get; set; }
        public Button PART_ButtonAddItem { get; set; }
        public Button PART_ButtonRemoveItem { get; set; }
        public PropertyGridView PART_PropertyGridView { get; set;}
        public Selector PART_OutlineList { get; set; }

        public SettersCollectionEditor()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_ButtonAddItem = Template.FindName("PART_AddItem", this) as Button;
            if (PART_ButtonAddItem != null)
                PART_ButtonAddItem.Click += new RoutedEventHandler(OnAddItemClicked);

            PART_ButtonRemoveItem = Template.FindName("PART_RemoveItem", this) as Button;
            if (PART_ButtonRemoveItem != null)
                PART_ButtonRemoveItem.Click += new RoutedEventHandler(OnRemoveItemClicked);

            PART_PropertyGridView = Template.FindName("PART_PropertyGridView", this) as PropertyGridView;
            PART_OutlineList = Template.FindName("PART_OutlineList", this) as Selector;
            PART_OutlineNodesContext = Template.FindName("PART_OutlineNodesContext", this) as FrameworkElement;
            PART_OutlineNodesContext.DataContext = OutlineNodeCollection;
            if (PART_PropertyGridView != null && _DesignObject != null)
            {
                //PART_PropertyGridView.DataContext = null;
                List<DesignItem> designItemList = new List<DesignItem>();
                designItemList.Add(_DesignObject);
                PART_PropertyGridView.PropertyGrid.SelectedItems = designItemList;
            }
        }

        public static readonly DependencyProperty FirstColumnWidthProperty =
            DependencyProperty.Register("FirstColumnWidth", typeof(double), typeof(SettersCollectionEditor),
            new PropertyMetadata(120.0));

        public double FirstColumnWidth
        {
            get { return (double)GetValue(FirstColumnWidthProperty); }
            set { SetValue(FirstColumnWidthProperty, value); }
        }


        public void InitEditor(DesignItem designObject, DesignItemProperty collectionProperty)
        {
            Debug.Assert(designObject.View is FrameworkElement);
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
            SetterOutlineNode selectedNode = PART_OutlineList.SelectedItem as SetterOutlineNode;
            if (selectedNode == null)
                return;
            OutlineNodeCollection.Remove(selectedNode);
            selectedNode.Reset();
            _SettersCollectionProp.CollectionElements.Remove(selectedNode.SetterItem);
        }
    }
}
