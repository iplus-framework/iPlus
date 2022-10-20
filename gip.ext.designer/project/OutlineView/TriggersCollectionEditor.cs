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
    [TemplatePart(Name = "PART_NewPropertyTrigger", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NewDataTrigger", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NewEventTrigger", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NewMultiTrigger", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NewMultiDataTrigger", Type = typeof(Button))]
    [TemplatePart(Name = "PART_RemoveItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_OutlineList", Type = typeof(Selector))]
    [CLSCompliant(false)]
    public class TriggersCollectionEditor : Control, ITypeEditorInitCollection
    {
        static TriggersCollectionEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TriggersCollectionEditor), new FrameworkPropertyMetadata(typeof(TriggersCollectionEditor)));
        }

        private static readonly Dictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>();

        private DesignItem _DesignObject;
        private DesignItemProperty _TriggerCollectionProp;
        private IComponentService _componentService;
        public  FrameworkElement PART_OutlineNodesContext { get; set; }
        public Button PART_NewPropertyTrigger { get; set; }
        public Button PART_NewDataTrigger { get; set; }
        public Button PART_NewEventTrigger { get; set; }
        public Button PART_NewMultiTrigger { get; set; }
        public Button PART_NewMultiDataTrigger { get; set; }
        public Button PART_ButtonRemoveItem { get; set; }
        public Selector PART_OutlineList { get; set; }

        public TriggersCollectionEditor()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_NewPropertyTrigger = Template.FindName("PART_NewPropertyTrigger", this) as Button;
            if (PART_NewPropertyTrigger != null)
                PART_NewPropertyTrigger.Click += new RoutedEventHandler(OnAddPropertyTriggerClicked);

            PART_NewDataTrigger = Template.FindName("PART_NewDataTrigger", this) as Button;
            if (PART_NewDataTrigger != null)
                PART_NewDataTrigger.Click += new RoutedEventHandler(OnAddDataTriggerClicked);

            PART_NewEventTrigger = Template.FindName("PART_NewEventTrigger", this) as Button;
            if (PART_NewEventTrigger != null)
                PART_NewEventTrigger.Click += new RoutedEventHandler(OnAddEventTriggerClicked);

            PART_NewMultiTrigger = Template.FindName("PART_NewMultiTrigger", this) as Button;
            if (PART_NewMultiTrigger != null)
                PART_NewMultiTrigger.Click += new RoutedEventHandler(OnAddMultiTriggerClicked);

            PART_NewMultiDataTrigger = Template.FindName("PART_NewMultiDataTrigger", this) as Button;
            if (PART_NewMultiDataTrigger != null)
                PART_NewMultiDataTrigger.Click += new RoutedEventHandler(OnAddMultiDataTriggerClicked);

            PART_ButtonRemoveItem = Template.FindName("PART_RemoveItem", this) as Button;
            if (PART_ButtonRemoveItem != null)
                PART_ButtonRemoveItem.Click += new RoutedEventHandler(OnRemoveItemClicked);

            PART_OutlineList = Template.FindName("PART_OutlineList", this) as Selector;
            PART_OutlineNodesContext = Template.FindName("PART_OutlineNodesContext", this) as FrameworkElement;
            if (PART_OutlineNodesContext != null)
                PART_OutlineNodesContext.DataContext = OutlineNodeCollection;
        }


        public void InitEditor(DesignItem designObject, DesignItemProperty collectionProperty)
        {
            Debug.Assert(designObject.View is FrameworkElement);

            _DesignObject = designObject;
            _TriggerCollectionProp = collectionProperty;

            foreach (DesignItem child in _TriggerCollectionProp.CollectionElements)
            {
                TriggerOutlineNodeBase node = CreateOutlineNode(child, designObject);
                if (node != null)
                    _OutlineNodeCollection.Add(node);
            }
            _componentService = designObject.Services.Component;
        }

        protected virtual TriggerOutlineNodeBase CreateOutlineNode(DesignItem child, DesignItem designObject)
        {
            if (typeof(MultiDataTrigger).IsAssignableFrom(child.ComponentType))
                return new MultiDataTriggerOutlineNode(child, designObject);
            else if (typeof(MultiTrigger).IsAssignableFrom(child.ComponentType))
                return new MultiTriggerOutlineNode(child, designObject);
            else if (typeof(DataTrigger).IsAssignableFrom(child.ComponentType))
                return new DataTriggerOutlineNode(child, designObject);
            else if (typeof(EventTrigger).IsAssignableFrom(child.ComponentType))
                return new EventTriggerOutlineNode(child, designObject);
            else if (typeof(Trigger).IsAssignableFrom(child.ComponentType))
                return new PropertyTriggerOutlineNode(child, designObject);
            return null;
        }

        ObservableCollection<TriggerOutlineNodeBase> _OutlineNodeCollection = new ObservableCollection<TriggerOutlineNodeBase>();
        public ObservableCollection<TriggerOutlineNodeBase> OutlineNodeCollection
        {
            get { return _OutlineNodeCollection; }
        }

        private void OnAddPropertyTriggerClicked(object sender, RoutedEventArgs e)
        {
            Trigger newTrigger = new Trigger();
            DesignItem newTriggerItem = _DesignObject.Services.Component.RegisterComponentForDesigner(newTrigger);
            _TriggerCollectionProp.CollectionElements.Add(newTriggerItem);
            //newTriggerItem.Properties["Property"].SetValue(PART_PropertyGridView.PropertyGrid.SelectedNode.FirstProperty.DependencyProperty);
            //object valueOnInstance = PART_PropertyGridView.PropertyGrid.SelectedNode.FirstProperty.ValueOnInstance;
            //newTriggerItem.Properties["Value"].SetValue(valueOnInstance);
            TriggerOutlineNodeBase node = CreateOutlineNode(newTriggerItem, _DesignObject);
            if (node != null)
            {
                OutlineNodeCollection.Add(node);
                if (PART_OutlineList != null)
                    PART_OutlineList.SelectedItem = node;
            }
        }

        private void OnAddDataTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(new DataTrigger());
        }

        private void OnAddEventTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(new EventTrigger());
        }

        private void OnAddMultiTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(new MultiTrigger());
        }

        private void OnAddMultiDataTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(new MultiDataTrigger());
        }

        private void AddTrigger(TriggerBase newTrigger)
        {
            DesignItem newTriggerItem = _DesignObject.Services.Component.RegisterComponentForDesigner(newTrigger);
            _TriggerCollectionProp.CollectionElements.Add(newTriggerItem);
            TriggerOutlineNodeBase node = CreateOutlineNode(newTriggerItem, _DesignObject);
            if (node != null)
            {
                OutlineNodeCollection.Add(node);
                if (PART_OutlineList != null)
                    PART_OutlineList.SelectedItem = node;
            }
        }

        private void OnRemoveItemClicked(object sender, RoutedEventArgs e)
        {
            TriggerOutlineNodeBase selectedNode = PART_OutlineList.SelectedItem as TriggerOutlineNodeBase;
            if (selectedNode == null)
                return;
            OutlineNodeCollection.Remove(selectedNode);
            selectedNode.Reset();
            _TriggerCollectionProp.CollectionElements.Remove(selectedNode.TriggerItem);
        }
    }
}
