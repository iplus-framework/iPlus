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
        private readonly HashSet<DesignItem> _pendingLegacyInverseTriggers = new HashSet<DesignItem>();
        private readonly HashSet<DesignItem> _legacyInverseCreationInProgress = new HashSet<DesignItem>();
        private bool _suppressLegacyInversePropertyChanged;
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

            if (_componentService != null)
                _componentService.PropertyChanged -= ComponentService_PropertyChanged;

            _DesignObject = designObject;
            _TriggerCollectionProp = collectionProperty;
            _pendingLegacyInverseTriggers.Clear();
            _legacyInverseCreationInProgress.Clear();
            _suppressLegacyInversePropertyChanged = false;

            foreach (DesignItem child in _TriggerCollectionProp.CollectionElements)
            {
                TriggerOutlineNodeBase node = CreateOutlineNode(child, designObject);
                if (node != null)
                    _OutlineNodeCollection.Add(node);

                if (TriggerRevertCompatibility.UseLegacyInverseFallback(child))
                    QueueLegacyInverseForDeferredCreation(child);
            }

            _componentService = designObject.Services.Component;
            if (_componentService != null)
                _componentService.PropertyChanged += ComponentService_PropertyChanged;
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
            if (binding == null || string.IsNullOrWhiteSpace(binding.Path) || binding.RelativeSource == null)
                return false;

            if (binding.RelativeSource.Mode == RelativeSourceMode.Self)
                return true;

            return binding.RelativeSource.Mode == RelativeSourceMode.FindAncestor
                && binding.RelativeSource.Tree == TreeType.Logical
                && binding.RelativeSource.AncestorLevel == 1;
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

            if (TriggerRevertCompatibility.UseLegacyInverseFallback(newTriggerItem))
                QueueLegacyInverseForDeferredCreation(newTriggerItem);

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

            _pendingLegacyInverseTriggers.Remove(selectedNode.TriggerItem);
            _legacyInverseCreationInProgress.Remove(selectedNode.TriggerItem);

            if (TriggerRevertCompatibility.UseLegacyInverseFallback(selectedNode.TriggerItem))
                RemoveInverseBehaviorAction(selectedNode.TriggerItem);

            OutlineNodeCollection.Remove(selectedNode);
            selectedNode.Reset();
            _TriggerCollectionProp.CollectionElements.Remove(selectedNode.TriggerItem);
        }

        private void ComponentService_PropertyChanged(object sender, DesignItemPropertyChangedEventArgs e)
        {
            if (e?.ItemProperty == null)
                return;

            if (_suppressLegacyInversePropertyChanged)
                return;

            if (_pendingLegacyInverseTriggers.Count == 0)
                return;

            var pendingTriggerItem = ResolvePendingTriggerFromChangedItem(e.Item);
            if (pendingTriggerItem != null)
            {
                TryCreatePendingLegacyInverse(pendingTriggerItem);
                return;
            }

            if (!IsPendingTriggerRelevantProperty(e.ItemProperty.Name))
                return;

            foreach (var triggerItem in _pendingLegacyInverseTriggers.ToList())
                TryCreatePendingLegacyInverse(triggerItem);
        }

        private DesignItem ResolvePendingTriggerFromChangedItem(DesignItem changedItem)
        {
            var current = changedItem;
            while (current != null)
            {
                if (_pendingLegacyInverseTriggers.Contains(current))
                    return current;

                var parentProperty = current.ParentProperty;
                current = parentProperty != null ? parentProperty.DesignItem : null;
            }

            return null;
        }

        private static bool IsPendingTriggerRelevantProperty(string propertyName)
        {
            return string.Equals(propertyName, "Binding", StringComparison.Ordinal)
                || string.Equals(propertyName, "Bindings", StringComparison.Ordinal)
                || string.Equals(propertyName, "Value", StringComparison.Ordinal)
                || string.Equals(propertyName, "PropertyName", StringComparison.Ordinal)
                || string.Equals(propertyName, "Path", StringComparison.Ordinal)
                || string.Equals(propertyName, "VBContent", StringComparison.Ordinal)
                || string.Equals(propertyName, "RelativeSource", StringComparison.Ordinal);
        }

        private void QueueLegacyInverseForDeferredCreation(DesignItem triggerItem)
        {
            if (triggerItem == null)
                return;

            _pendingLegacyInverseTriggers.Add(triggerItem);
            TryCreatePendingLegacyInverse(triggerItem);
        }

        private void TryCreatePendingLegacyInverse(DesignItem triggerItem)
        {
            if (triggerItem == null)
                return;

            if (_legacyInverseCreationInProgress.Contains(triggerItem))
                return;

            _legacyInverseCreationInProgress.Add(triggerItem);
            _suppressLegacyInversePropertyChanged = true;

            try
            {
                if (EnsureInverseBehaviorAction(triggerItem))
                    _pendingLegacyInverseTriggers.Remove(triggerItem);
            }
            finally
            {
                _legacyInverseCreationInProgress.Remove(triggerItem);
                _suppressLegacyInversePropertyChanged = _legacyInverseCreationInProgress.Count > 0;
            }
        }

        private bool EnsureInverseBehaviorAction(DesignItem currentTriggerItem)
        {
            if (!TryGetBehaviorTriggerContext(currentTriggerItem,
                                              out DesignItemProperty triggerCollectionProperty,
                                              out BindingBase triggerBinding,
                                              out object inverseTriggerValue))
                return false;

            var sourceBindingProperty = GetProperty(currentTriggerItem, "Binding");

            DesignItem inverseTrigger = FindInverseTrigger(triggerCollectionProperty,
                                                           currentTriggerItem,
                                                           triggerBinding,
                                                           sourceBindingProperty,
                                                           inverseTriggerValue);
            if (inverseTrigger != null)
                return true;

            inverseTrigger = _DesignObject.Services.Component.RegisterComponentForDesigner(new DataTrigger());
            triggerCollectionProperty.CollectionElements.Add(inverseTrigger);

            var inverseBindingProperty = GetProperty(inverseTrigger, "Binding");
            if (inverseBindingProperty != null)
            {
                var clonedBinding = CloneBindingDefinition(triggerBinding);
                inverseBindingProperty.SetValue(clonedBinding);
                ApplyBindingDesignMetadata(inverseBindingProperty, triggerBinding);
            }

            var inverseValueProperty = GetProperty(inverseTrigger, "Value");
            if (inverseValueProperty != null)
                inverseValueProperty.SetValue(inverseTriggerValue);

            TriggerOutlineNodeBase inverseNode = CreateOutlineNode(inverseTrigger, _DesignObject);
            if (inverseNode != null)
                OutlineNodeCollection.Add(inverseNode);

            return true;
        }

        private void RemoveInverseBehaviorAction(DesignItem currentTriggerItem)
        {
            if (!TryGetBehaviorTriggerContext(currentTriggerItem,
                                              out DesignItemProperty triggerCollectionProperty,
                                              out BindingBase triggerBinding,
                                              out object inverseTriggerValue))
                return;

            var sourceBindingProperty = GetProperty(currentTriggerItem, "Binding");

            DesignItem inverseTrigger = FindInverseTrigger(triggerCollectionProperty,
                                                           currentTriggerItem,
                                                           triggerBinding,
                                                           sourceBindingProperty,
                                                           inverseTriggerValue);
            if (inverseTrigger == null)
                return;

            var inverseNode = OutlineNodeCollection.FirstOrDefault(c => ReferenceEquals(c.TriggerItem, inverseTrigger));
            if (inverseNode != null)
            {
                OutlineNodeCollection.Remove(inverseNode);
                inverseNode.Reset();
            }

            triggerCollectionProperty.CollectionElements.Remove(inverseTrigger);
        }

        private bool TryGetBehaviorTriggerContext(DesignItem currentTriggerItem,
                                                  out DesignItemProperty triggerCollectionProperty,
                                                  out BindingBase triggerBinding,
                                                  out object inverseTriggerValue)
        {
            triggerCollectionProperty = null;
            triggerBinding = null;
            inverseTriggerValue = null;

            if (!IsDataTriggerBehavior(currentTriggerItem))
                return false;

            var bindingProperty = GetProperty(currentTriggerItem, "Binding");
            triggerBinding = GetBindingDefinition(bindingProperty);
            if (!HasUsableTriggerBinding(triggerBinding, bindingProperty))
                return false;

            var valueProperty = GetProperty(currentTriggerItem, "Value");
            if (valueProperty == null || !TryGetInverseBooleanValue(valueProperty.ValueOnInstance, out inverseTriggerValue))
                return false;

            triggerCollectionProperty = currentTriggerItem.ParentProperty;
            return triggerCollectionProperty != null && triggerCollectionProperty.CollectionElements != null;
        }

        private static bool IsDataTriggerBehavior(DesignItem triggerItem)
        {
            if (triggerItem == null || triggerItem.ComponentType == null)
                return false;

            return typeof(DataTriggerBehavior).IsAssignableFrom(triggerItem.ComponentType)
                || triggerItem.ComponentType.Name.IndexOf("DataTrigger", StringComparison.Ordinal) >= 0;
        }

        private static bool IsPropertyBasedTriggerBinding(Binding binding)
        {
            if (binding == null || string.IsNullOrWhiteSpace(binding.Path) || binding.RelativeSource == null)
                return false;

            if (binding.RelativeSource.Mode == RelativeSourceMode.Self)
                return true;

            return binding.RelativeSource.Mode == RelativeSourceMode.FindAncestor
                && binding.RelativeSource.Tree == TreeType.Logical
                && binding.RelativeSource.AncestorLevel == 1;
        }

        private static bool TryGetInverseBooleanValue(object value, out object inverseValue)
        {
            inverseValue = null;

            if (value is bool boolValue)
            {
                inverseValue = !boolValue;
                return true;
            }

            if (value is string stringValue && bool.TryParse(stringValue, out bool parsed))
            {
                inverseValue = (!parsed).ToString();
                return true;
            }

            return false;
        }

        private DesignItem FindInverseTrigger(DesignItemProperty triggerCollectionProperty,
                                              DesignItem currentTriggerItem,
                                              BindingBase triggerBinding,
                                              DesignItemProperty triggerBindingProperty,
                                              object inverseTriggerValue)
        {
            if (triggerCollectionProperty == null || triggerCollectionProperty.CollectionElements == null)
                return null;

            foreach (DesignItem triggerItem in triggerCollectionProperty.CollectionElements)
            {
                if (triggerItem == null || triggerItem == currentTriggerItem)
                    continue;

                if (!IsDataTriggerBehavior(triggerItem))
                    continue;

                var candidateBindingProperty = GetProperty(triggerItem, "Binding");
                var candidateBinding = GetBindingDefinition(candidateBindingProperty);
                if (!AreEquivalentBindings(candidateBinding,
                                           candidateBindingProperty,
                                           triggerBinding,
                                           triggerBindingProperty))
                    continue;

                var candidateValueProperty = GetProperty(triggerItem, "Value");
                if (candidateValueProperty == null || !AreEquivalentTriggerValues(candidateValueProperty.ValueOnInstance, inverseTriggerValue))
                    continue;

                return triggerItem;
            }

            return null;
        }

        private static bool AreEquivalentBindings(BindingBase left,
                                                  DesignItemProperty leftProperty,
                                                  BindingBase right,
                                                  DesignItemProperty rightProperty)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left == null || right == null)
                return false;

            var leftKey = GetBindingIdentityKey(left);
            var rightKey = GetBindingIdentityKey(right);
            if (!string.IsNullOrWhiteSpace(leftKey) && !string.IsNullOrWhiteSpace(rightKey))
                return string.Equals(leftKey, rightKey, StringComparison.Ordinal);

            var leftDesignKey = GetBindingDesignIdentityKey(leftProperty);
            var rightDesignKey = GetBindingDesignIdentityKey(rightProperty);
            if (!string.IsNullOrWhiteSpace(leftDesignKey) && !string.IsNullOrWhiteSpace(rightDesignKey))
                return string.Equals(leftDesignKey, rightDesignKey, StringComparison.Ordinal);

            return false;
        }

        private static BindingBase GetBindingDefinition(DesignItemProperty bindingProperty)
        {
            if (bindingProperty == null)
                return null;

            var designBinding = bindingProperty.Value?.Component as BindingBase;
            var instanceBinding = bindingProperty.ValueOnInstance as BindingBase;

            if (designBinding == null)
                return instanceBinding;
            if (instanceBinding == null)
                return designBinding;

            // Keep custom binding payload (e.g. VBContent) when design-model projection is incomplete.
            if (HasMoreCompleteBindingIdentity(instanceBinding, designBinding))
                return instanceBinding;

            return designBinding;
        }

        private static bool HasUsableTriggerBinding(BindingBase binding, DesignItemProperty bindingProperty)
        {
            if (!string.IsNullOrWhiteSpace(GetBindingIdentityKey(binding)))
                return true;

            return !string.IsNullOrWhiteSpace(GetBindingDesignIdentityKey(bindingProperty));
        }

        private static string GetBindingDesignIdentityKey(DesignItemProperty bindingProperty)
        {
            return GetBindingDesignIdentityKey(bindingProperty?.Value);
        }

        private static string GetBindingDesignIdentityKey(DesignItem bindingItem)
        {
            if (bindingItem == null)
                return null;

            if (IsMultiBindingDesignItem(bindingItem))
            {
                var bindingsProperty = bindingItem.Properties?.HasProperty("Bindings");
                var childKeys = new List<string>();

                var childBindings = bindingsProperty?.CollectionElements;
                if (childBindings != null)
                {
                    foreach (var childBinding in childBindings)
                    {
                        var childKey = GetBindingDesignIdentityKey(childBinding);
                        if (!string.IsNullOrWhiteSpace(childKey))
                            childKeys.Add(childKey);
                    }
                }

                if (!childKeys.Any())
                    return null;

                return "M:" + string.Join(";", childKeys);
            }

            var identityPath = GetDesignItemStringProperty(bindingItem, "Path");
            if (string.IsNullOrWhiteSpace(identityPath))
                identityPath = GetDesignItemStringProperty(bindingItem, "VBContent");

            if (string.IsNullOrWhiteSpace(identityPath))
                return null;

            var relativeSourceIdentity = GetRelativeSourceDesignIdentity(bindingItem);
            return $"B:{identityPath}|{relativeSourceIdentity}";
        }

        private static string GetRelativeSourceDesignIdentity(DesignItem bindingItem)
        {
            var relativeSourceItem = bindingItem?.Properties?.HasProperty("RelativeSource")?.Value;
            if (relativeSourceItem == null)
                return "none";

            var mode = relativeSourceItem.Properties?.HasProperty("Mode")?.ValueOnInstance;
            var tree = relativeSourceItem.Properties?.HasProperty("Tree")?.ValueOnInstance;
            var ancestorLevel = relativeSourceItem.Properties?.HasProperty("AncestorLevel")?.ValueOnInstance;
            var ancestorType = relativeSourceItem.Properties?.HasProperty("AncestorType")?.ValueOnInstance;

            return $"{mode}|{tree}|{ancestorLevel}|{ancestorType}";
        }

        private static string GetDesignItemStringProperty(DesignItem designItem, string propertyName)
        {
            if (designItem == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            var property = designItem.Properties?.HasProperty(propertyName);
            if (property == null)
                return null;

            var value = property.ValueOnInstance as string;
            if (!string.IsNullOrWhiteSpace(value))
                return value;

            var textValue = property.TextValue;
            if (!string.IsNullOrWhiteSpace(textValue))
                return textValue;

            return null;
        }

        private static bool IsMultiBindingDesignItem(DesignItem bindingItem)
        {
            if (bindingItem == null || bindingItem.ComponentType == null)
                return false;

            return typeof(MultiBinding).IsAssignableFrom(bindingItem.ComponentType)
                || bindingItem.ComponentType.Name.IndexOf("MultiBinding", StringComparison.Ordinal) >= 0;
        }

        private static string GetBindingIdentityKey(BindingBase binding)
        {
            if (binding is Binding singleBinding)
            {
                var identityPath = GetBindingIdentityPath(singleBinding);
                if (string.IsNullOrWhiteSpace(identityPath))
                    return null;

                return $"B:{identityPath}|{GetRelativeSourceIdentity(singleBinding.RelativeSource)}";
            }

            if (binding is MultiBinding multiBinding)
            {
                var childKeys = new List<string>();
                foreach (var childBinding in multiBinding.Bindings)
                {
                    if (!(childBinding is BindingBase childBindingBase))
                        continue;

                    var childKey = GetBindingIdentityKey(childBindingBase);
                    if (!string.IsNullOrWhiteSpace(childKey))
                        childKeys.Add(childKey);
                }

                if (!childKeys.Any())
                    return null;

                return "M:" + string.Join(";", childKeys);
            }

            return null;
        }

        private static string GetRelativeSourceIdentity(RelativeSource relativeSource)
        {
            if (relativeSource == null)
                return "none";

            return $"{relativeSource.Mode}|{relativeSource.Tree}|{relativeSource.AncestorLevel}|{relativeSource.AncestorType?.FullName}";
        }

        private static string GetBindingIdentityPath(Binding binding)
        {
            if (binding == null)
                return null;

            if (!string.IsNullOrWhiteSpace(binding.Path))
                return binding.Path;

            var vbContent = GetBindingDescriptorString(binding, "VBContent");
            if (!string.IsNullOrWhiteSpace(vbContent))
                return vbContent;

            return null;
        }

        private static bool HasMoreCompleteBindingIdentity(BindingBase candidate, BindingBase baseline)
        {
            if (candidate == null)
                return false;
            if (baseline == null)
                return true;

            var candidateIdentity = GetBindingIdentityKey(candidate);
            var baselineIdentity = GetBindingIdentityKey(baseline);
            if (string.IsNullOrWhiteSpace(baselineIdentity) && !string.IsNullOrWhiteSpace(candidateIdentity))
                return true;

            if (candidate is MultiBinding candidateMultiBinding && baseline is MultiBinding baselineMultiBinding)
                return candidateMultiBinding.Bindings.Count > baselineMultiBinding.Bindings.Count;

            if (!(candidate is Binding candidateBinding) || !(baseline is Binding baselineBinding))
                return false;

            var candidateVbContent = GetBindingDescriptorString(candidateBinding, "VBContent");
            var baselineVbContent = GetBindingDescriptorString(baselineBinding, "VBContent");
            return string.IsNullOrWhiteSpace(baselineVbContent)
                && !string.IsNullOrWhiteSpace(candidateVbContent);
        }

        private static string GetBindingDescriptorString(Binding binding, string propertyName)
        {
            if (binding == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            var descriptor = TypeDescriptor.GetProperties(binding)[propertyName];
            if (descriptor == null)
                return null;

            var value = descriptor.GetValue(binding) as string;
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static void ApplyBindingDesignMetadata(DesignItemProperty bindingProperty, BindingBase sourceBinding)
        {
            if (bindingProperty == null || sourceBinding == null)
                return;

            if (!(sourceBinding is Binding singleBinding))
                return;

            var bindingItem = bindingProperty.Value;
            var bindingProps = bindingItem?.Properties;
            if (bindingProps == null)
                return;

            var pathProperty = bindingProps.HasProperty("Path");
            if (pathProperty != null && !string.IsNullOrWhiteSpace(singleBinding.Path))
                pathProperty.SetValue(singleBinding.Path);

            var vbContentProperty = bindingProps.HasProperty("VBContent");
            var vbContent = GetBindingDescriptorString(singleBinding, "VBContent");
            if (vbContentProperty != null && !string.IsNullOrWhiteSpace(vbContent))
                vbContentProperty.SetValue(vbContent);

            var sourceRelativeSource = singleBinding.RelativeSource;
            var relativeSourceProperty = bindingProps.HasProperty("RelativeSource");
            if (relativeSourceProperty != null && sourceRelativeSource != null)
            {
                relativeSourceProperty.SetValue(new RelativeSource());

                var relativeSourceItem = relativeSourceProperty.Value;
                var relativeSourceProps = relativeSourceItem?.Properties;
                var modeProperty = relativeSourceProps?.HasProperty("Mode");
                var ancestorLevelProperty = relativeSourceProps?.HasProperty("AncestorLevel");
                var treeProperty = relativeSourceProps?.HasProperty("Tree");

                if (modeProperty != null)
                    modeProperty.SetValue(sourceRelativeSource.Mode);
                if (ancestorLevelProperty != null)
                    ancestorLevelProperty.SetValue(sourceRelativeSource.AncestorLevel);
                if (treeProperty != null)
                    treeProperty.SetValue(sourceRelativeSource.Tree);
            }
        }

        private static bool AreEquivalentRelativeSources(RelativeSource left, RelativeSource right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left == null || right == null)
                return false;

            return left.Mode == right.Mode
                && left.Tree == right.Tree
                && left.AncestorLevel == right.AncestorLevel
                && left.AncestorType == right.AncestorType;
        }

        private static bool AreEquivalentTriggerValues(object left, object right)
        {
            if (Equals(left, right))
                return true;

            if (TryGetBooleanValue(left, out bool leftBool) && TryGetBooleanValue(right, out bool rightBool))
                return leftBool == rightBool;

            return false;
        }

        private static bool TryGetBooleanValue(object value, out bool boolValue)
        {
            if (value is bool directBool)
            {
                boolValue = directBool;
                return true;
            }

            if (value is string stringValue && bool.TryParse(stringValue, out bool parsedBool))
            {
                boolValue = parsedBool;
                return true;
            }

            boolValue = false;
            return false;
        }

        private static BindingBase CloneBindingDefinition(BindingBase source)
        {
            if (source is Binding singleBinding)
                return CloneBinding(singleBinding);

            if (source is MultiBinding multiBinding)
                return CloneMultiBinding(multiBinding);

            return source;
        }

        private static Binding CloneBinding(Binding source)
        {
            if (source == null)
                return null;

            var clonedObject = CloneObject(source);
            if (clonedObject is Binding clonedBinding)
            {
                CopyCustomBindingProperties(source, clonedBinding);
                if (clonedBinding.RelativeSource == null && source.RelativeSource != null)
                    clonedBinding.RelativeSource = CloneRelativeSource(source.RelativeSource);
                return clonedBinding;
            }

            var fallbackBinding = new Binding
            {
                Path = source.Path,
                RelativeSource = CloneRelativeSource(source.RelativeSource)
            };

            CopyCustomBindingProperties(source, fallbackBinding);
            return fallbackBinding;
        }

        private static MultiBinding CloneMultiBinding(MultiBinding source)
        {
            if (source == null)
                return null;

            var clonedObject = CloneObject(source);
            var clonedMultiBinding = clonedObject as MultiBinding ?? new MultiBinding();

            clonedMultiBinding.Bindings.Clear();
            foreach (var childBinding in source.Bindings)
            {
                if (childBinding is BindingBase childBindingBase)
                {
                    var clonedChildBinding = CloneBindingDefinition(childBindingBase);
                    if (clonedChildBinding != null)
                        clonedMultiBinding.Bindings.Add(clonedChildBinding);
                    continue;
                }

                clonedMultiBinding.Bindings.Add(childBinding);
            }

            return clonedMultiBinding;
        }

        private static void CopyCustomBindingProperties(Binding source, Binding target)
        {
            if (source == null || target == null)
                return;

            var targetProperties = TypeDescriptor.GetProperties(target);
            foreach (PropertyDescriptor sourceProperty in TypeDescriptor.GetProperties(source))
            {
                if (sourceProperty.IsReadOnly)
                    continue;

                if (string.Equals(sourceProperty.Name, nameof(Binding.Path), StringComparison.Ordinal)
                    || string.Equals(sourceProperty.Name, nameof(Binding.RelativeSource), StringComparison.Ordinal))
                    continue;

                var targetProperty = targetProperties[sourceProperty.Name];
                if (targetProperty == null || targetProperty.IsReadOnly)
                    continue;

                try
                {
                    var value = sourceProperty.GetValue(source);
                    targetProperty.SetValue(target, value);
                }
                catch
                {
                }
            }
        }

        private static object CloneObject(object source)
        {
            if (source == null)
                return null;

            if (source is string || source.GetType().IsValueType)
                return source;

            if (source is ICloneable cloneable)
            {
                try
                {
                    return cloneable.Clone();
                }
                catch
                {
                }
            }

            Type sourceType = source.GetType();
            var defaultCtor = sourceType.GetConstructor(Type.EmptyTypes);
            if (defaultCtor == null)
                return source;

            object clone;
            try
            {
                clone = defaultCtor.Invoke(null);
            }
            catch
            {
                return source;
            }

            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(source))
            {
                if (pd.IsReadOnly)
                    continue;

                try
                {
                    var value = pd.GetValue(source);
                    pd.SetValue(clone, value);
                }
                catch
                {
                }
            }

            return clone;
        }

        private static RelativeSource CloneRelativeSource(RelativeSource source)
        {
            if (source == null)
                return null;

            return new RelativeSource
            {
                Mode = source.Mode,
                Tree = source.Tree,
                AncestorLevel = source.AncestorLevel,
                AncestorType = source.AncestorType
            };
        }
    }
}
