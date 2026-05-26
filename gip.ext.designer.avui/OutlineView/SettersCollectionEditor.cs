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
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Threading;

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
        private ISelectionService _selectionService;
        private bool _trackDesignerSelection;
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

        public bool TrackDesignerSelection { get; set; } = true;

        public void InitEditor(DesignItem designObject, DesignItemProperty collectionProperty)
        {
            Debug.Assert(designObject.View is Control);
            // TODO: Style erzeugen, falls nicht angelegt
            //DesignItemProperty styleProp = designObject.Properties.GetProperty(FrameworkElement.StyleProperty);
            //if ((styleProp.Value == null) || !(styleProp.Value is DesignItem))
            //    return;
            //DesignItemProperty settersProp = styleProp.Value.Properties.GetProperty("Setters");

            _trackDesignerSelection = TrackDesignerSelection && ShouldTrackDesignerSelection(collectionProperty);
            ApplyEditorContext(designObject, collectionProperty);
            AttachSelectionTracking(designObject);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            DetachSelectionTracking();
        }

        private void ApplyEditorContext(DesignItem designObject, DesignItemProperty collectionProperty)
        {
            _DesignObject = designObject;
            _SettersCollectionProp = collectionProperty;
            _componentService = designObject?.Services?.Component;

            _OutlineNodeCollection.Clear();

            if (_SettersCollectionProp?.CollectionElements != null)
            {
                foreach (DesignItem child in _SettersCollectionProp.CollectionElements)
                {
                    _OutlineNodeCollection.Add(new SetterOutlineNode(child, _DesignObject));
                }
            }

            if (PART_OutlineNodesContext != null)
                PART_OutlineNodesContext.DataContext = OutlineNodeCollection;

            if (PART_OutlineList != null)
                PART_OutlineList.SelectedItem = null;

            if (PART_PropertyGridView?.PropertyGrid != null)
            {
                if (_DesignObject != null)
                    PART_PropertyGridView.PropertyGrid.SelectedItems = new[] { _DesignObject };
                else
                    PART_PropertyGridView.PropertyGrid.SelectedItems = null;
            }
        }

        private static bool ShouldTrackDesignerSelection(DesignItemProperty collectionProperty)
        {
            if (collectionProperty == null)
                return false;

            // Only style-level Setters should follow global selection.
            if (!string.Equals(collectionProperty.Name, "Setters", StringComparison.Ordinal))
                return false;

            var ownerTypeName = collectionProperty.DesignItem?.ComponentType?.Name;
            return !string.IsNullOrWhiteSpace(ownerTypeName)
                && (ownerTypeName.IndexOf("Style", StringComparison.OrdinalIgnoreCase) >= 0
                    || ownerTypeName.IndexOf("Theme", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void AttachSelectionTracking(DesignItem designObject)
        {
            DetachSelectionTracking();

            if (!_trackDesignerSelection || designObject?.Services == null)
                return;

            _selectionService = designObject.Services.Selection;
            if (_selectionService != null)
                _selectionService.SelectionChanged += OnDesignerSelectionChanged;
        }

        private void DetachSelectionTracking()
        {
            if (_selectionService != null)
            {
                _selectionService.SelectionChanged -= OnDesignerSelectionChanged;
                _selectionService = null;
            }
        }

        private void OnDesignerSelectionChanged(object sender, DesignItemCollectionEventArgs e)
        {
            if (!_trackDesignerSelection || _selectionService == null)
                return;

            Dispatcher.UIThread.Invoke(() =>
            {
                var selected = _selectionService.SelectedItems?.FirstOrDefault();
                if (selected == null)
                {
                    ApplyEditorContext(null, null);
                    return;
                }

                var themeProperty = selected.Properties.GetProperty(Control.ThemeProperty);
                var settersProperty = themeProperty?.Value?.Properties?.HasProperty("Setters");
                ApplyEditorContext(selected, settersProperty);
            }, DispatcherPriority.Background);
        }

        private bool IsBehaviorActionCollection
        {
            get
            {
                return _SettersCollectionProp != null
                    && _SettersCollectionProp.Name != null
                    && _SettersCollectionProp.Name.EndsWith("Actions", StringComparison.Ordinal);
            }
        }

        ObservableCollection<SetterOutlineNode> _OutlineNodeCollection = new ObservableCollection<SetterOutlineNode>();
        public ObservableCollection<SetterOutlineNode> OutlineNodeCollection
        {
            get { return _OutlineNodeCollection; }
        }

        private void OnAddItemClicked(object sender, RoutedEventArgs e)
        {
            if (_SettersCollectionProp == null)
                return;

            if (PART_PropertyGridView == null || PART_PropertyGridView.PropertyGrid == null)
                return;

            var selectedNode = PART_PropertyGridView.PropertyGrid.SelectedNode;
            if (selectedNode != null)
            {
                if (selectedNode.IsDependencyProperty)
                {
                    if (OutlineNodeCollection.Any(c => string.Equals(c.SetterTargetPropertyName, selectedNode.Name, StringComparison.Ordinal)))
                        return;

                    DesignItem newSetterItem;
                    object valueOnInstance = selectedNode.FirstProperty != null
                        ? selectedNode.FirstProperty.NewClonedValueOnInstance
                        : null;

                    if (IsBehaviorActionCollection)
                    {
                        var newAction = new ChangePropertyAction();
                        newSetterItem = _DesignObject.Services.Component.RegisterComponentForDesigner(newAction);
                    }
                    else
                    {
                        Setter newSetter = new Setter();
                        newSetterItem = _DesignObject.Services.Component.RegisterComponentForDesigner(newSetter);
                    }

                    _SettersCollectionProp.CollectionElements.Add(newSetterItem);

                    if (IsBehaviorActionCollection)
                    {
                        var propertyNameProperty = newSetterItem.Properties.HasProperty("PropertyName");
                        if (propertyNameProperty != null)
                            propertyNameProperty.SetValue(selectedNode.Name);
                    }
                    else
                    {
                        var propertyProperty = newSetterItem.Properties.HasProperty("Property");
                        if (propertyProperty != null)
                        {
                            // Using the property name avoids serializing unresolved AvaloniaProperty
                            // instances (e.g. <StyledProperty />) in the XAML model.
                            var setterPropertyName = selectedNode.Name;
                            if (!string.IsNullOrWhiteSpace(setterPropertyName))
                                propertyProperty.SetValue(setterPropertyName);
                        }
                    }

                    var valueProperty = newSetterItem.Properties.HasProperty("Value");
                    if (valueProperty != null)
                        valueProperty.SetValue(valueOnInstance);

                    if (IsBehaviorActionCollection)
                    {
                        if (UseLegacyInverseFallbackForCurrentTrigger())
                            EnsureInverseBehaviorAction(selectedNode.Name, valueOnInstance);
                    }

                    SetterOutlineNode node = new SetterOutlineNode(newSetterItem, _DesignObject);
                    OutlineNodeCollection.Add(node);

                    // WPF-style trigger setters transfer the current local value into the setter and
                    // clear the source property. For behavior actions in Avalonia, keep the source
                    // property untouched and only configure the action payload.
                    if (!IsBehaviorActionCollection)
                    {
                        if (selectedNode.FirstProperty != null)
                            selectedNode.FirstProperty.Reset();
                        if (node.SetterTargetProperty != null)
                            node.SetterTargetProperty.SetValueOnInstance(valueOnInstance);
                    }
                }
            }
        }

        private void OnRemoveItemClicked(object sender, RoutedEventArgs e)
        {
            if (_SettersCollectionProp == null)
                return;

            SetterOutlineNode selectedNode = PART_OutlineList?.SelectedItem as SetterOutlineNode;
            if (selectedNode == null)
                return;

            if (IsBehaviorActionCollection)
            {
                if (UseLegacyInverseFallbackForCurrentTrigger())
                    RemoveInverseBehaviorAction(selectedNode.SetterTargetPropertyName);
            }

            OutlineNodeCollection.Remove(selectedNode);
            selectedNode.Reset();
            _SettersCollectionProp.CollectionElements.Remove(selectedNode.SetterItem);
        }

        private bool UseLegacyInverseFallbackForCurrentTrigger()
        {
            if (!IsBehaviorActionCollection || _SettersCollectionProp == null)
                return false;

            return TriggerRevertCompatibility.UseLegacyInverseFallback(_SettersCollectionProp.DesignItem);
        }

        private void EnsureInverseBehaviorAction(string propertyName, object revertValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return;

            if (!TryGetBehaviorTriggerContext(out DesignItem currentTriggerItem,
                                              out DesignItemProperty triggerCollectionProperty,
                                              out BindingBase triggerBinding,
                                              out object inverseTriggerValue))
                return;

            var sourceBindingProperty = currentTriggerItem.Properties.HasProperty("Binding");

            DesignItem inverseTrigger = FindInverseTrigger(triggerCollectionProperty,
                                                           currentTriggerItem,
                                                           triggerBinding,
                                                           sourceBindingProperty,
                                                           inverseTriggerValue);
            if (inverseTrigger == null)
            {
                inverseTrigger = _DesignObject.Services.Component.RegisterComponentForDesigner(new DataTrigger());
                triggerCollectionProperty.CollectionElements.Add(inverseTrigger);

                var inverseBindingProperty = inverseTrigger.Properties.HasProperty("Binding");
                if (inverseBindingProperty != null)
                {
                    var clonedBinding = CloneBindingDefinition(triggerBinding);
                    inverseBindingProperty.SetValue(clonedBinding);
                    ApplyBindingDesignMetadata(inverseBindingProperty, triggerBinding);
                }

                var inverseValueProperty = inverseTrigger.Properties.HasProperty("Value");
                if (inverseValueProperty != null)
                    inverseValueProperty.SetValue(inverseTriggerValue);
            }

            var inverseActionsProperty = inverseTrigger.Properties.HasProperty("Actions");
            if (inverseActionsProperty == null)
                return;

            DesignItem inverseAction = FindChangePropertyAction(inverseActionsProperty, propertyName);
            if (inverseAction == null)
            {
                inverseAction = _DesignObject.Services.Component.RegisterComponentForDesigner(new ChangePropertyAction());
                inverseActionsProperty.CollectionElements.Add(inverseAction);

                var inversePropertyNameProperty = inverseAction.Properties.HasProperty("PropertyName");
                if (inversePropertyNameProperty != null)
                    inversePropertyNameProperty.SetValue(propertyName);
            }

            var inverseValuePropertyOnAction = inverseAction.Properties.HasProperty("Value");
            if (inverseValuePropertyOnAction != null)
                inverseValuePropertyOnAction.SetValue(revertValue);
        }

        private void RemoveInverseBehaviorAction(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return;

            if (!TryGetBehaviorTriggerContext(out DesignItem currentTriggerItem,
                                              out DesignItemProperty triggerCollectionProperty,
                                              out BindingBase triggerBinding,
                                              out object inverseTriggerValue))
                return;

            var sourceBindingProperty = currentTriggerItem.Properties.HasProperty("Binding");

            DesignItem inverseTrigger = FindInverseTrigger(triggerCollectionProperty,
                                                           currentTriggerItem,
                                                           triggerBinding,
                                                           sourceBindingProperty,
                                                           inverseTriggerValue);
            if (inverseTrigger == null)
                return;

            var inverseActionsProperty = inverseTrigger.Properties.HasProperty("Actions");
            if (inverseActionsProperty == null)
                return;

            DesignItem inverseAction = FindChangePropertyAction(inverseActionsProperty, propertyName);
            if (inverseAction == null)
                return;

            inverseActionsProperty.CollectionElements.Remove(inverseAction);

            if (!inverseActionsProperty.CollectionElements.Any())
            {
                triggerCollectionProperty.CollectionElements.Remove(inverseTrigger);
            }
        }

        private bool TryGetBehaviorTriggerContext(out DesignItem currentTriggerItem,
                                                  out DesignItemProperty triggerCollectionProperty,
                                                  out BindingBase triggerBinding,
                                                  out object inverseTriggerValue)
        {
            currentTriggerItem = null;
            triggerCollectionProperty = null;
            triggerBinding = null;
            inverseTriggerValue = null;

            if (!IsBehaviorActionCollection || _SettersCollectionProp == null)
                return false;

            if (!string.Equals(_SettersCollectionProp.Name, "Actions", StringComparison.Ordinal))
                return false;

            currentTriggerItem = _SettersCollectionProp.DesignItem;
            if (currentTriggerItem == null || !IsDataTriggerBehavior(currentTriggerItem))
                return false;

            var bindingProperty = currentTriggerItem.Properties.HasProperty("Binding");
            triggerBinding = GetBindingDefinition(bindingProperty);
            if (!HasUsableTriggerBinding(triggerBinding, bindingProperty))
                return false;

            var valueProperty = currentTriggerItem.Properties.HasProperty("Value");
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

                var candidateBindingProperty = triggerItem.Properties.HasProperty("Binding");
                var candidateBinding = GetBindingDefinition(candidateBindingProperty);
                if (!AreEquivalentBindings(candidateBinding,
                                           candidateBindingProperty,
                                           triggerBinding,
                                           triggerBindingProperty))
                    continue;

                var candidateValueProperty = triggerItem.Properties.HasProperty("Value");
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

            // Preserve derived binding types (e.g., vb:VBBinding) when possible.
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
                    // Skip properties that cannot be copied.
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
                    // Fall back to reflection clone.
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
                    // Skip properties that cannot be copied.
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

        private static DesignItem FindChangePropertyAction(DesignItemProperty actionsProperty, string propertyName)
        {
            if (actionsProperty == null || actionsProperty.CollectionElements == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            foreach (DesignItem actionItem in actionsProperty.CollectionElements)
            {
                if (actionItem == null || actionItem.ComponentType == null)
                    continue;

                if (!typeof(ChangePropertyAction).IsAssignableFrom(actionItem.ComponentType)
                    && actionItem.ComponentType.Name.IndexOf("ChangePropertyAction", StringComparison.Ordinal) < 0)
                    continue;

                var actionPropertyName = actionItem.Properties.HasProperty("PropertyName");
                var actionPropertyNameValue = actionPropertyName != null ? actionPropertyName.ValueOnInstance as string : null;
                if (string.Equals(actionPropertyNameValue, propertyName, StringComparison.Ordinal))
                    return actionItem;
            }

            return null;
        }
    }
}
