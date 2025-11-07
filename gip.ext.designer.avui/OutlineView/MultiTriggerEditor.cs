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
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia;

namespace gip.ext.designer.avui.OutlineView
{
    [TemplatePart(Name = "PART_AddItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_RemoveItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ConditionList", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_SetterEditor", Type = typeof(SettersCollectionEditor))]
    [TemplatePart(Name = "PART_ConditionEditor", Type = typeof(ConditionEditor))]
    [TemplatePart(Name = "PART_EnterActionsEditor", Type = typeof(ActionCollectionEditor))]
    [TemplatePart(Name = "PART_ExitActionsEditor", Type = typeof(ActionCollectionEditor))]
    [CLSCompliant(false)]
    public class MultiTriggerEditor : TemplatedControl
    {
        static MultiTriggerEditor()
        {
            // Register default style key for Avalonia
        }

        public static readonly StyledProperty<ObservableCollection<ConditionWrapper>> ConditionWrapperCollectionProperty =
            AvaloniaProperty.Register<MultiTriggerEditor, ObservableCollection<ConditionWrapper>>(
                nameof(ConditionWrapperCollection), 
                new ObservableCollection<ConditionWrapper>());

        public ObservableCollection<ConditionWrapper> ConditionWrapperCollection
        {
            get => GetValue(ConditionWrapperCollectionProperty);
            set => SetValue(ConditionWrapperCollectionProperty, value);
        }

        private static readonly Dictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>();

        protected DesignItem _DesignObject;
        protected MultiTriggerNodeBase _NodeMultiTrigger;
        protected IComponentService _componentService;

        public Button PART_ButtonAddItem { get; set; }
        public Button PART_ButtonRemoveItem { get; set; }
        public ListBox PART_ConditionList { get; set; }
        public ContentControl PART_ConditionEditor { get; set; }
        public SettersCollectionEditor PART_SetterEditor { get; set; }
        public ActionCollectionEditor PART_EnterActionsEditor { get; set; }
        public ActionCollectionEditor PART_ExitActionsEditor { get; set; }

        public MultiTriggerEditor() : base()
        {
            ConditionWrapperCollection = new ObservableCollection<ConditionWrapper>();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            // Unsubscribe from previous template parts if any
            if (PART_ButtonAddItem != null)
                PART_ButtonAddItem.Click -= OnAddItemClicked;
            if (PART_ButtonRemoveItem != null)
                PART_ButtonRemoveItem.Click -= OnRemoveItemClicked;
            if (PART_ConditionList != null)
                PART_ConditionList.SelectionChanged -= PART_ConditionList_SelectionChanged;

            PART_ButtonAddItem = e.NameScope.Find("PART_AddItem") as Button;
            if (PART_ButtonAddItem != null)
                PART_ButtonAddItem.Click += OnAddItemClicked;

            PART_ButtonRemoveItem = e.NameScope.Find("PART_RemoveItem") as Button;
            if (PART_ButtonRemoveItem != null)
                PART_ButtonRemoveItem.Click += OnRemoveItemClicked;

            PART_ConditionEditor = e.NameScope.Find("PART_ConditionEditor") as ContentControl;

            PART_ConditionList = e.NameScope.Find("PART_ConditionList") as ListBox;
            if (PART_ConditionList != null)
            {
                PART_ConditionList.SelectionChanged += PART_ConditionList_SelectionChanged;
            }

            PART_SetterEditor = e.NameScope.Find("PART_SetterEditor") as SettersCollectionEditor;
            if (PART_SetterEditor != null && _DesignObject != null && _NodeMultiTrigger?.TriggerItem?.Properties != null)
            {
                var settersProperty = _NodeMultiTrigger.TriggerItem.Properties.GetProperty("Setters");
                if (settersProperty != null)
                {
                    PART_SetterEditor.InitEditor(_DesignObject, settersProperty);
                }
            }

            PART_EnterActionsEditor = e.NameScope.Find("PART_EnterActionsEditor") as ActionCollectionEditor;
            if (PART_EnterActionsEditor != null && _DesignObject != null && _NodeMultiTrigger?.TriggerItem?.Properties != null)
            {
                var enterActionsProperty = _NodeMultiTrigger.TriggerItem.Properties.GetProperty("EnterActions");
                if (enterActionsProperty != null)
                {
                    PART_EnterActionsEditor.InitEditor(_DesignObject, enterActionsProperty);
                }
            }

            PART_ExitActionsEditor = e.NameScope.Find("PART_ExitActionsEditor") as ActionCollectionEditor;
            if (PART_ExitActionsEditor != null && _DesignObject != null && _NodeMultiTrigger?.TriggerItem?.Properties != null)
            {
                var exitActionsProperty = _NodeMultiTrigger.TriggerItem.Properties.GetProperty("ExitActions");
                if (exitActionsProperty != null)
                {
                    PART_ExitActionsEditor.InitEditor(_DesignObject, exitActionsProperty);
                }
            }
        }

        private bool _Loaded = false;
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (_Loaded)
                return;
            if (PART_ConditionList != null)
            {
                if (ConditionWrapperCollection.Count > 0)
                {
                    if (PART_ConditionList.SelectedIndex < 0)
                        PART_ConditionList.SelectedIndex = 0;
                    else
                        RefreshConditionEditor();
                }
            }
            _Loaded = true;
        }

        public void InitEditor(DesignItem designObject, MultiTriggerNodeBase MultiTrigger)
        {
            Debug.Assert(designObject.View is Control);

            _DesignObject = designObject;
            _NodeMultiTrigger = MultiTrigger;
            _componentService = designObject.Services.Component;
            DataContext = this;

            ConditionWrapperCollection.Clear();
            if (_NodeMultiTrigger?.ConditionsProperty?.CollectionElements != null)
            {
                foreach (DesignItem child in _NodeMultiTrigger.ConditionsProperty.CollectionElements)
                {
                    ConditionWrapperCollection.Add(OnCreateConditionWrapper(child));
                }
            }

            if (_NodeMultiTrigger != null)
                _NodeMultiTrigger.PropertyChanged += _NodeMultiTrigger_PropertyChanged;
        }

        void _NodeMultiTrigger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Handle property changes if needed
        }

        protected virtual ConditionWrapper OnCreateConditionWrapper(DesignItem designObjectCondition)
        {
            return new ConditionWrapper(designObjectCondition, _NodeMultiTrigger);
        }

        void PART_ConditionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshConditionEditor();
        }

        private void RefreshConditionEditor()
        {
            if (PART_ConditionEditor != null)
            {
                ConditionWrapper selectedWrapper = PART_ConditionList?.SelectedItem as ConditionWrapper;
                if (selectedWrapper != null)
                {
                    ConditionEditor editor = PART_ConditionEditor.Content as ConditionEditor;
                    if (editor == null)
                    {
                        editor = CreateConditionEditor();
                        if (editor != null)
                        {
                            editor.Wrapper = selectedWrapper;
                            PART_ConditionEditor.Content = editor;
                        }
                    }
                    else
                    {
                        editor.Wrapper = selectedWrapper;
                    }
                }
                else
                {
                    PART_ConditionEditor.Content = null;
                }
            }
        }

        protected virtual ConditionEditor CreateConditionEditor()
        {
            return new ConditionEditor();
        }

        private void OnAddItemClicked(object sender, RoutedEventArgs e)
        {
            if ((PART_ConditionList == null) || (_NodeMultiTrigger == null))
                return;

            try
            {
                // Note: Condition class needs to be defined or replaced with appropriate trigger condition type
                // For now, using a placeholder - this will need to be adapted for StyledElementTrigger
                var newCondition = new object(); // Replace with actual condition type
                DesignItem newConditionItem = _DesignObject.Services.Component.RegisterComponentForDesigner(newCondition);
                _NodeMultiTrigger.ConditionsProperty.CollectionElements.Add(newConditionItem);
                ConditionWrapper newWrapper = OnCreateConditionWrapper(newConditionItem);
                ConditionWrapperCollection.Add(newWrapper);
                if (newWrapper != null)
                    PART_ConditionList.SelectedItem = newWrapper;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding condition: {ex.Message}");
            }
        }

        private void OnRemoveItemClicked(object sender, RoutedEventArgs e)
        {
            if ((PART_ConditionList == null) || (_NodeMultiTrigger == null))
                return;

            ConditionWrapper selectedNode = PART_ConditionList.SelectedItem as ConditionWrapper;
            if (selectedNode == null)
                return;
                
            try
            {
                ConditionWrapperCollection.Remove(selectedNode);
                _NodeMultiTrigger.ConditionsProperty.CollectionElements.Remove(selectedNode.DesignObjectCondition);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing condition: {ex.Message}");
            }
        }
    }
}
