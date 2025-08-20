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
    [TemplatePart(Name = "PART_SelectorTriggerable", Type = typeof(Selector))]
    [TemplatePart(Name = "PART_TriggerValueEditor", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_SetterEditor", Type = typeof(SettersCollectionEditor))]
    [TemplatePart(Name = "PART_EnterActionsEditor", Type = typeof(ActionCollectionEditor))]
    [TemplatePart(Name = "PART_ExitActionsEditor", Type = typeof(ActionCollectionEditor))]
    [CLSCompliant(false)]
    public class PropertyTriggerEditor : Control
    {
        static PropertyTriggerEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyTriggerEditor), new FrameworkPropertyMetadata(typeof(PropertyTriggerEditor)));
        }


        private DesignItem _DesignObject;
        private PropertyTriggerOutlineNode _NodePropertyTrigger;
        private IComponentService _componentService;

        public Selector PART_SelectorTriggerable { get; set; }
        public ContentControl PART_TriggerValueEditor { get; set; }
        public SettersCollectionEditor PART_SetterEditor { get; set; }
        public ActionCollectionEditor PART_EnterActionsEditor { get; set; }
        public ActionCollectionEditor PART_ExitActionsEditor { get; set; }

        public PropertyTriggerEditor()
        {
            this.Loaded += new RoutedEventHandler(PropertyTriggerEditor_Loaded);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_SelectorTriggerable = Template.FindName("PART_SelectorTriggerable", this) as Selector;
            if (PART_SelectorTriggerable != null)
            {
                PART_SelectorTriggerable.SelectionChanged += new SelectionChangedEventHandler(PART_SelectorTriggerable_SelectionChanged);
            }
            PART_TriggerValueEditor = Template.FindName("PART_TriggerValueEditor", this) as ContentControl;
            PART_SetterEditor = Template.FindName("PART_SetterEditor", this) as SettersCollectionEditor;
            if (PART_SetterEditor != null)
            {
                PART_SetterEditor.InitEditor(_DesignObject, _NodePropertyTrigger.TriggerItem.Properties["Setters"]);
            }

            PART_EnterActionsEditor = Template.FindName("PART_EnterActionsEditor", this) as ActionCollectionEditor;
            if (PART_EnterActionsEditor != null)
            {
                PART_EnterActionsEditor.InitEditor(_DesignObject, _NodePropertyTrigger.TriggerItem.Properties["EnterActions"]);
            }

            PART_ExitActionsEditor = Template.FindName("PART_ExitActionsEditor", this) as ActionCollectionEditor;
            if (PART_ExitActionsEditor != null)
            {
                PART_ExitActionsEditor.InitEditor(_DesignObject, _NodePropertyTrigger.TriggerItem.Properties["ExitActions"]);
            }
        }

        private bool _Loaded = false;
        void PropertyTriggerEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (_Loaded)
                return;
            //if (!IsTriggerEditable && PART_TriggerValueEditor != null)
            PART_TriggerValueEditor.Content = TriggerValueEditor;
            _Loaded = true;
        }

        public void InitEditor(DesignItem designObject, PropertyTriggerOutlineNode propertyTrigger)
        {
            Debug.Assert(designObject.View is FrameworkElement);

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
            _NodePropertyTrigger.PropertyChanged += new PropertyChangedEventHandler(_NodePropertyTrigger_PropertyChanged);

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
                return TriggerableProperties.First();
            }
            set
            {
                if (value != null)
                {
                    _NodePropertyTrigger.TriggerTargetPropertyName = value.Name;
                }
            }
        }

        public static readonly DependencyProperty IsTriggerEditableProperty
            = DependencyProperty.Register("IsTriggerEditable", typeof(bool), typeof(PropertyTriggerEditor), new PropertyMetadata(false));
        public bool IsTriggerEditable
        {
            get { return (bool)GetValue(IsTriggerEditableProperty); }
            set { SetValue(IsTriggerEditableProperty, value); }
        }


        public static readonly DependencyProperty AreTriggerValuesValidProperty
            = DependencyProperty.Register("AreTriggerValuesValid", typeof(bool), typeof(PropertyTriggerEditor), new PropertyMetadata(false));
        public bool AreTriggerValuesValid
        {
            get { return (bool)GetValue(AreTriggerValuesValidProperty); }
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
        public FrameworkElement TriggerValueEditor
        {
            get
            {
                _LockValueEditorRefresh = true;
                if (PART_SelectorTriggerable != null && SelectedTriggerableProperty != null)
                {
                    //PropertyNode selectedNode = PART_SelectorTriggerable.SelectedItem as PropertyNode;
                    FrameworkElement editor = SelectedTriggerableProperty.Editor;
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
