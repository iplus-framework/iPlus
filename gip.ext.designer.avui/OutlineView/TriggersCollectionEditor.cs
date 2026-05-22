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
using Avalonia.Data;
using Avalonia.Xaml.Interactions.Core;

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
        protected enum TriggerKind
        {
            Property,
            Data,
            Event,
            Multi,
            MultiData
        }

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
            switch (DetermineTriggerKind(child))
            {
                case TriggerKind.Property:
                    return new PropertyTriggerOutlineNode(child, designObject);
                case TriggerKind.Data:
                    return new DataTriggerOutlineNode(child, designObject);
                case TriggerKind.Event:
                    return new EventTriggerOutlineNode(child, designObject);
                case TriggerKind.Multi:
                    return new MultiTriggerOutlineNode(child, designObject);
                case TriggerKind.MultiData:
                    return new MultiDataTriggerOutlineNode(child, designObject);
                default:
                    return null;
            }
        }

        protected virtual TriggerKind DetermineTriggerKind(DesignItem child)
        {
            if (child == null || child.ComponentType == null)
                return TriggerKind.Data;

            if (typeof(EventTriggerBehavior).IsAssignableFrom(child.ComponentType))
                return TriggerKind.Event;

            if (typeof(MultiDataTriggerBehavior).IsAssignableFrom(child.ComponentType))
                return IsPropertyBasedMultiTrigger(child) ? TriggerKind.Multi : TriggerKind.MultiData;

            if (typeof(DataTriggerBehavior).IsAssignableFrom(child.ComponentType))
                return IsPropertyBasedDataTrigger(child) ? TriggerKind.Property : TriggerKind.Data;

            var typeName = child.ComponentType.Name;
            if (typeName.Contains("EventTrigger"))
                return TriggerKind.Event;
            if (typeName.Contains("MultiDataTrigger"))
                return TriggerKind.MultiData;
            if (typeName.Contains("MultiTrigger"))
                return TriggerKind.Multi;
            if (typeName.Contains("DataTrigger"))
                return TriggerKind.Data;
            if (typeName.Contains("Trigger"))
                return TriggerKind.Property;

            return TriggerKind.Data;
        }

        protected virtual object CreateNewTrigger(TriggerKind kind)
        {
            switch (kind)
            {
                case TriggerKind.Property:
                case TriggerKind.Data:
                    return new DataTrigger();
                case TriggerKind.Event:
                    return new EventTrigger();
                case TriggerKind.Multi:
                case TriggerKind.MultiData:
                    return new MultiDataTrigger();
                default:
                    return new DataTrigger();
            }
        }

        protected virtual bool IsPropertyBasedDataTrigger(DesignItem child)
        {
            var bindingProp = GetProperty(child, "Binding");
            var binding = bindingProp != null ? bindingProp.ValueOnInstance as Binding : null;
            return binding != null
                && binding.RelativeSource != null
                && binding.RelativeSource.Mode == RelativeSourceMode.Self
                && !string.IsNullOrWhiteSpace(binding.Path);
        }

        protected virtual bool IsPropertyBasedMultiTrigger(DesignItem child)
        {
            var conditionsProp = GetProperty(child, "Conditions");
            if (conditionsProp == null || conditionsProp.CollectionElements == null)
                return false;

            foreach (var condition in conditionsProp.CollectionElements)
            {
                var propertyProp = GetProperty(condition, "Property");
                if (propertyProp != null && propertyProp.ValueOnInstance != null)
                    return true;
            }

            return false;
        }

        protected DesignItemProperty GetProperty(DesignItem item, params string[] names)
        {
            if (item == null || item.Properties == null || names == null)
                return null;

            foreach (string name in names)
            {
                var property = item.Properties.HasProperty(name);
                if (property != null)
                    return property;
            }

            return null;
        }

        ObservableCollection<TriggerOutlineNodeBase> _OutlineNodeCollection = new ObservableCollection<TriggerOutlineNodeBase>();
        public ObservableCollection<TriggerOutlineNodeBase> OutlineNodeCollection
        {
            get { return _OutlineNodeCollection; }
        }

        private void OnAddPropertyTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(TriggerKind.Property);
        }

        private void OnAddDataTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(TriggerKind.Data);
        }

        private void OnAddEventTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(TriggerKind.Event);
        }

        private void OnAddMultiTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(TriggerKind.Multi);
        }

        private void OnAddMultiDataTriggerClicked(object sender, RoutedEventArgs e)
        {
            AddTrigger(TriggerKind.MultiData);
        }

        private void AddTrigger(TriggerKind kind)
        {
            var newTrigger = CreateNewTrigger(kind);
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
