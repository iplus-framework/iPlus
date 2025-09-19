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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Styling;

namespace gip.ext.designer.avui.OutlineView
{
    [TemplatePart(Name = "PART_OutlineNodesContext", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_NewPropertyTrigger", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NewDataTrigger", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NewEventTrigger", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NewMultiTrigger", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NewMultiDataTrigger", Type = typeof(Button))]
    [TemplatePart(Name = "PART_RemoveItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_OutlineList", Type = typeof(SelectingItemsControl))]
    [CLSCompliant(false)]
    public class TriggersCollectionEditor : TemplatedControl, ITypeEditorInitCollection
    {
        private static readonly Dictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>();

        private DesignItem _DesignObject;
        private DesignItemProperty _TriggerCollectionProp;
        private IComponentService _componentService;
        public Control PART_OutlineNodesContext { get; set; }
        public Button PART_NewPropertyTrigger { get; set; }
        public Button PART_NewDataTrigger { get; set; }
        public Button PART_NewEventTrigger { get; set; }
        public Button PART_NewMultiTrigger { get; set; }
        public Button PART_NewMultiDataTrigger { get; set; }
        public Button PART_ButtonRemoveItem { get; set; }
        public SelectingItemsControl PART_OutlineList { get; set; }

        public TriggersCollectionEditor()
        {
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            PART_NewPropertyTrigger = e.NameScope.Find<Button>("PART_NewPropertyTrigger");
            if (PART_NewPropertyTrigger != null)
                PART_NewPropertyTrigger.Click += OnAddPropertyTriggerClicked;

            PART_NewDataTrigger = e.NameScope.Find<Button>("PART_NewDataTrigger");
            if (PART_NewDataTrigger != null)
                PART_NewDataTrigger.Click += OnAddDataTriggerClicked;

            PART_NewEventTrigger = e.NameScope.Find<Button>("PART_NewEventTrigger");
            if (PART_NewEventTrigger != null)
                PART_NewEventTrigger.Click += OnAddEventTriggerClicked;

            PART_NewMultiTrigger = e.NameScope.Find<Button>("PART_NewMultiTrigger");
            if (PART_NewMultiTrigger != null)
                PART_NewMultiTrigger.Click += OnAddMultiTriggerClicked;

            PART_NewMultiDataTrigger = e.NameScope.Find<Button>("PART_NewMultiDataTrigger");
            if (PART_NewMultiDataTrigger != null)
                PART_NewMultiDataTrigger.Click += OnAddMultiDataTriggerClicked;

            PART_ButtonRemoveItem = e.NameScope.Find<Button>("PART_RemoveItem");
            if (PART_ButtonRemoveItem != null)
                PART_ButtonRemoveItem.Click += OnRemoveItemClicked;

            PART_OutlineList = e.NameScope.Find<SelectingItemsControl>("PART_OutlineList");
            PART_OutlineNodesContext = e.NameScope.Find<Control>("PART_OutlineNodesContext");
            if (PART_OutlineNodesContext != null)
                PART_OutlineNodesContext.DataContext = OutlineNodeCollection;
        }

        public void InitEditor(DesignItem designObject, DesignItemProperty collectionProperty)
        {
            Debug.Assert(designObject.View is Control);

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
            // Note: These trigger types are placeholders for the future BehaviorCollection implementation
            // In AvaloniaUI, we'll use Xaml Behaviors instead of WPF triggers
            if (child.ComponentType.Name.Contains("MultiDataTrigger"))
                return new MultiDataTriggerOutlineNode(child, designObject);
            else if (child.ComponentType.Name.Contains("MultiTrigger"))
                return new MultiTriggerOutlineNode(child, designObject);
            else if (child.ComponentType.Name.Contains("DataTrigger"))
                return new DataTriggerOutlineNode(child, designObject);
            else if (child.ComponentType.Name.Contains("EventTrigger"))
                return new EventTriggerOutlineNode(child, designObject);
            else if (child.ComponentType.Name.Contains("Trigger"))
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
            // TODO: Replace with appropriate Xaml Behavior when implementing BehaviorCollection
            // For now, keeping the structure for future adaptation
            var newTrigger = CreateNewTriggerPlaceholder("PropertyTrigger");
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

        private void OnAddDataTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(CreateNewTriggerPlaceholder("DataTrigger"));
        }

        private void OnAddEventTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(CreateNewTriggerPlaceholder("EventTrigger"));
        }

        private void OnAddMultiTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(CreateNewTriggerPlaceholder("MultiTrigger"));
        }

        private void OnAddMultiDataTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(CreateNewTriggerPlaceholder("MultiDataTrigger"));
        }

        // Placeholder method for creating trigger objects
        // This will be replaced with appropriate Xaml Behavior objects in the future
        private object CreateNewTriggerPlaceholder(string triggerType)
        {
            // Return a placeholder object that can be registered with the designer
            // In the final implementation, this will create appropriate Xaml Behavior objects
            return new TriggerPlaceholder { TriggerType = triggerType };
        }

        private void AddTrigger(object newTrigger)
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

    // Placeholder class for trigger objects until Xaml Behaviors are implemented
    public class TriggerPlaceholder
    {
        public string TriggerType { get; set; }
    }
}
