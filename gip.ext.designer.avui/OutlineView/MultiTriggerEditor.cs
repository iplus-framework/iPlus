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
using gip.ext.designer.avui.OutlineView;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using gip.ext.designer.avui.PropertyGrid;
using System.Linq;

namespace gip.ext.designer.avui.OutlineView
{
    [TemplatePart(Name = "PART_AddItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_RemoveItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ConditionList", Type = typeof(Selector))]
    [TemplatePart(Name = "PART_SetterEditor", Type = typeof(SettersCollectionEditor))]
    [TemplatePart(Name = "PART_ConditionEditor", Type = typeof(ConditionEditor))]
    [TemplatePart(Name = "PART_EnterActionsEditor", Type = typeof(ActionCollectionEditor))]
    [TemplatePart(Name = "PART_ExitActionsEditor", Type = typeof(ActionCollectionEditor))]
    [CLSCompliant(false)]
    public class MultiTriggerEditor : Control
    {
        static MultiTriggerEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiTriggerEditor), new FrameworkPropertyMetadata(typeof(MultiTriggerEditor)));
        }

        private static readonly Dictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>();

        protected DesignItem _DesignObject;
        protected MultiTriggerNodeBase _NodeMultiTrigger;
        protected IComponentService _componentService;

        public Button PART_ButtonAddItem { get; set; }
        public Button PART_ButtonRemoveItem { get; set; }
        public Selector PART_ConditionList { get; set; }
        public ContentControl PART_ConditionEditor { get; set; }
        public SettersCollectionEditor PART_SetterEditor { get; set; }
        public ActionCollectionEditor PART_EnterActionsEditor { get; set; }
        public ActionCollectionEditor PART_ExitActionsEditor { get; set; }

        public MultiTriggerEditor()
        {
            this.Loaded += new RoutedEventHandler(MultiTriggerEditor_Loaded);
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

            PART_ConditionEditor = Template.FindName("PART_ConditionEditor", this) as ContentControl;

            PART_ConditionList = Template.FindName("PART_ConditionList", this) as Selector;
            if (PART_ConditionList != null)
            {
                PART_ConditionList.SelectionChanged += new SelectionChangedEventHandler(PART_ConditionList_SelectionChanged);
            }

            PART_SetterEditor = Template.FindName("PART_SetterEditor", this) as SettersCollectionEditor;
            if (PART_SetterEditor != null)
            {
                PART_SetterEditor.InitEditor(_DesignObject, _NodeMultiTrigger.TriggerItem.Properties["Setters"]);
            }

            PART_EnterActionsEditor = Template.FindName("PART_EnterActionsEditor", this) as ActionCollectionEditor;
            if (PART_EnterActionsEditor != null)
            {
                PART_EnterActionsEditor.InitEditor(_DesignObject, _NodeMultiTrigger.TriggerItem.Properties["EnterActions"]);
            }

            PART_ExitActionsEditor = Template.FindName("PART_ExitActionsEditor", this) as ActionCollectionEditor;
            if (PART_ExitActionsEditor != null)
            {
                PART_ExitActionsEditor.InitEditor(_DesignObject, _NodeMultiTrigger.TriggerItem.Properties["ExitActions"]);
            }
        }

        private bool _Loaded = false;
        void MultiTriggerEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (_Loaded)
                return;
            if (PART_ConditionList != null)
            {
                if (_ConditionWrapperCollection.Count > 0)
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
            Debug.Assert(designObject.View is FrameworkElement);

            _DesignObject = designObject;
            _NodeMultiTrigger = MultiTrigger;
            _componentService = designObject.Services.Component;
            DataContext = this;

            foreach (DesignItem child in _NodeMultiTrigger.ConditionsProperty.CollectionElements)
            {
                _ConditionWrapperCollection.Add(OnCreateConditionWrapper(child));
            }

            _NodeMultiTrigger.PropertyChanged += new PropertyChangedEventHandler(_NodeMultiTrigger_PropertyChanged);

        }

        ObservableCollection<ConditionWrapper> _ConditionWrapperCollection = new ObservableCollection<ConditionWrapper>();
        public ObservableCollection<ConditionWrapper> ConditionWrapperCollection
        {
            get { return _ConditionWrapperCollection; }
        }

        void _NodeMultiTrigger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
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
                ConditionWrapper selectedWrapper = PART_ConditionList.SelectedItem as ConditionWrapper;
                if (selectedWrapper != null)
                {
                    ConditionEditor editor = PART_ConditionEditor.Content as ConditionEditor;
                    if (editor == null)
                    {
                        editor = CreateConditionEditor();
                        editor.Wrapper = selectedWrapper;
                        PART_ConditionEditor.Content = editor;
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

            Condition newCondition = new Condition();
            DesignItem newConditionItem = _DesignObject.Services.Component.RegisterComponentForDesigner(newCondition);
            _NodeMultiTrigger.ConditionsProperty.CollectionElements.Add(newConditionItem);
            ConditionWrapper newWrapper = OnCreateConditionWrapper(newConditionItem);
            ConditionWrapperCollection.Add(newWrapper);
            if (newWrapper != null)
                PART_ConditionList.SelectedItem = newWrapper;
        }

        private void OnRemoveItemClicked(object sender, RoutedEventArgs e)
        {
            if ((PART_ConditionList == null) || (_NodeMultiTrigger == null))
                return;

            ConditionWrapper selectedNode = PART_ConditionList.SelectedItem as ConditionWrapper;
            if (selectedNode == null)
                return;
            ConditionWrapperCollection.Remove(selectedNode);
            _NodeMultiTrigger.ConditionsProperty.CollectionElements.Remove(selectedNode.DesignObjectCondition);
        }

    }
}
