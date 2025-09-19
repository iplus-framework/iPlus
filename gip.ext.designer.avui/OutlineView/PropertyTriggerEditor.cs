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
using Avalonia;
using Avalonia.Interactivity;

namespace gip.ext.designer.avui.OutlineView
{
    [TemplatePart(Name = "PART_SelectorTriggerable", Type = typeof(ComboBox))]
    [TemplatePart(Name = "PART_TriggerValueEditor", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_SetterEditor", Type = typeof(SettersCollectionEditor))]
    [TemplatePart(Name = "PART_EnterActionsEditor", Type = typeof(ActionCollectionEditor))]
    [TemplatePart(Name = "PART_ExitActionsEditor", Type = typeof(ActionCollectionEditor))]
    [CLSCompliant(false)]
    public class PropertyTriggerEditor : TemplatedControl
    {
        static PropertyTriggerEditor()
        {
            // In AvaloniaUI, we don't need DefaultStyleKeyProperty.OverrideMetadata
            // Avalonia automatically resolves styles by type
        }

        private DesignItem _DesignObject;
        private PropertyTriggerOutlineNode _NodePropertyTrigger;
        private IComponentService _componentService;

        public ComboBox PART_SelectorTriggerable { get; set; }
        public ContentControl PART_TriggerValueEditor { get; set; }
        public SettersCollectionEditor PART_SetterEditor { get; set; }
        public ActionCollectionEditor PART_EnterActionsEditor { get; set; }
        public ActionCollectionEditor PART_ExitActionsEditor { get; set; }

        public PropertyTriggerEditor()
        {
            this.AttachedToVisualTree += PropertyTriggerEditor_Loaded;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            PART_SelectorTriggerable = e.NameScope.Find<ComboBox>("PART_SelectorTriggerable");
            if (PART_SelectorTriggerable != null)
            {
                PART_SelectorTriggerable.SelectionChanged += PART_SelectorTriggerable_SelectionChanged;
            }
            PART_TriggerValueEditor = e.NameScope.Find<ContentControl>("PART_TriggerValueEditor");
            PART_SetterEditor = e.NameScope.Find<SettersCollectionEditor>("PART_SetterEditor");
            if (PART_SetterEditor != null)
            {
                PART_SetterEditor.InitEditor(_DesignObject, _NodePropertyTrigger.TriggerItem.Properties["Setters"]);
            }

            PART_EnterActionsEditor = e.NameScope.Find<ActionCollectionEditor>("PART_EnterActionsEditor");
            if (PART_EnterActionsEditor != null)
            {
                PART_EnterActionsEditor.InitEditor(_DesignObject, _NodePropertyTrigger.TriggerItem.Properties["EnterActions"]);
            }

            PART_ExitActionsEditor = e.NameScope.Find<ActionCollectionEditor>("PART_ExitActionsEditor");
            if (PART_ExitActionsEditor != null)
            {
                PART_ExitActionsEditor.InitEditor(_DesignObject, _NodePropertyTrigger.TriggerItem.Properties["ExitActions"]);
            }
        }

        private bool _Loaded = false;
        void PropertyTriggerEditor_Loaded(object sender, VisualTreeAttachmentEventArgs e)
        {
            if (_Loaded)
                return;
            //if (!IsTriggerEditable && PART_TriggerValueEditor != null)
            if (PART_TriggerValueEditor != null)
                PART_TriggerValueEditor.Content = TriggerValueEditor;
            _Loaded = true;
        }

        public void InitEditor(DesignItem designObject, PropertyTriggerOutlineNode propertyTrigger)
        {
            Debug.Assert(designObject.View is Control);

            _DesignObject = designObject;
            _NodePropertyTrigger = propertyTrigger;
            _componentService = designObject.Services.Component;
            DataContext = this;
            if (_NodePropertyTrigger.IsEnabled)
                IsTriggerEditable = true;
            else
                IsTriggerEditable = false;

            if (_NodePropertyTrigger.IsSet)
                AreTriggerValuesValid = true;
            else
                AreTriggerValuesValid = false;
            _NodePropertyTrigger.PropertyChanged += _NodePropertyTrigger_PropertyChanged;

            LoadTriggerableProperties();
        }

        void _NodePropertyTrigger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSet")
            {
                if (_NodePropertyTrigger.IsSet)
                    AreTriggerValuesValid = true;
                else
                    AreTriggerValuesValid = false;
            }
            else if (e.PropertyName == "IsEnabled")
            {
                if (_NodePropertyTrigger.IsEnabled)
                    IsTriggerEditable = true;
                else
                    IsTriggerEditable = false;
            }
        }

        protected virtual PropertyNode CreatePropertyNode()
        {
            return new PropertyNode();
        }

        IList<MemberDescriptor> GetDescriptors()
        {
            SortedList<string, MemberDescriptor> list = new SortedList<string, MemberDescriptor>();

            if (_DesignObject != null)
            {
                foreach (MemberDescriptor d in TypeHelper.GetAvailableProperties(_DesignObject.Component, true))
                {
                    list.Add(d.Name, d);
                }
            }
            return list.Values;
        }

        protected void LoadTriggerableProperties()
        {
            foreach (var md in GetDescriptors())
            {
                AddNode(md);
            }
        }

        void AddNode(MemberDescriptor md)
        {
            DesignItemProperty[] designProperties = new DesignItemProperty[] {_DesignObject.Properties[md.Name]};
            PropertyNode node = CreatePropertyNode();
            node.Load(designProperties);
            if (!node.IsDependencyProperty)
                return;
            TriggerableProperties.Add(node);
        }

        ObservableCollection<PropertyNode> _TriggerableProperties = new ObservableCollection<PropertyNode>();
        public ObservableCollection<PropertyNode> TriggerableProperties
        {
            get { return _TriggerableProperties; }
        }

        public PropertyNode SelectedTriggerableProperty
        {
            get
            {
                if (!_LockValueEditorRefresh && !IsTriggerEditable && PART_TriggerValueEditor != null)
                    PART_TriggerValueEditor.Content = TriggerValueEditor;
                if (_NodePropertyTrigger.TriggerTargetProperty != null)
                {
                    var query = TriggerableProperties.Where(c => c.Name == _NodePropertyTrigger.TriggerTargetPropertyName);
                    if (query.Any())
                        return query.First();
                }
                if (TriggerableProperties.Count > 0)
                    return TriggerableProperties.First();
                return null;
            }
            set
            {
                if (value != null)
                {
                    _NodePropertyTrigger.TriggerTargetPropertyName = value.Name;
                }
            }
        }

        public static readonly StyledProperty<bool> IsTriggerEditableProperty
            = AvaloniaProperty.Register<PropertyTriggerEditor, bool>(nameof(IsTriggerEditable), false);
        public bool IsTriggerEditable
        {
            get { return GetValue(IsTriggerEditableProperty); }
            set { SetValue(IsTriggerEditableProperty, value); }
        }

        public static readonly StyledProperty<bool> AreTriggerValuesValidProperty
            = AvaloniaProperty.Register<PropertyTriggerEditor, bool>(nameof(AreTriggerValuesValid), false);
        public bool AreTriggerValuesValid
        {
            get { return GetValue(AreTriggerValuesValidProperty); }
            set { SetValue(AreTriggerValuesValidProperty, value); }
        }

        void PART_SelectorTriggerable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PART_TriggerValueEditor != null)
            {
                PART_TriggerValueEditor.Content = TriggerValueEditor;
            }
        }

        private bool _LockValueEditorRefresh = false;
        public Control TriggerValueEditor
        {
            get
            {
                _LockValueEditorRefresh = true;
                if (PART_SelectorTriggerable != null && SelectedTriggerableProperty != null)
                {
                    //PropertyNode selectedNode = PART_SelectorTriggerable.SelectedItem as PropertyNode;
                    Control editor = SelectedTriggerableProperty.Editor;
                    if (editor != null)
                        editor.DataContext = _NodePropertyTrigger;
                    _LockValueEditorRefresh = false;
                    return editor;
                }
                _LockValueEditorRefresh = false;
                return new ContentControl();
            }
        }
    }
}
