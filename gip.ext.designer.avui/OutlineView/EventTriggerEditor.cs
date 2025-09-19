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
    [TemplatePart(Name = "PART_EnterActionsEditor", Type = typeof(ActionCollectionEditor))]
    [TemplatePart(Name = "PART_ExitActionsEditor", Type = typeof(ActionCollectionEditor))]
    [CLSCompliant(false)]
    public class EventTriggerEditor : TemplatedControl
    {
        private static readonly Dictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>();

        private DesignItem _DesignObject;
        private EventTriggerOutlineNode _NodeEventTrigger;
        private IComponentService _componentService;

        public ComboBox PART_SelectorTriggerable { get; set; }
        public ActionCollectionEditor PART_EnterActionsEditor { get; set; }
        public ActionCollectionEditor PART_ExitActionsEditor { get; set; }

        public EventTriggerEditor()
        {
            this.AttachedToVisualTree += EventTriggerEditor_AttachedToVisualTree;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            PART_SelectorTriggerable = e.NameScope.Find<ComboBox>("PART_SelectorTriggerable");
            if (PART_SelectorTriggerable != null)
            {
                PART_SelectorTriggerable.SelectionChanged += PART_SelectorTriggerable_SelectionChanged;
            }

            PART_EnterActionsEditor = e.NameScope.Find<ActionCollectionEditor>("PART_EnterActionsEditor");
            if (PART_EnterActionsEditor != null && _DesignObject != null && _NodeEventTrigger != null)
            {
                PART_EnterActionsEditor.InitEditor(_DesignObject, _NodeEventTrigger.TriggerItem.Properties["EnterActions"]);
            }

            PART_ExitActionsEditor = e.NameScope.Find<ActionCollectionEditor>("PART_ExitActionsEditor");
            if (PART_ExitActionsEditor != null && _DesignObject != null && _NodeEventTrigger != null)
            {
                PART_ExitActionsEditor.InitEditor(_DesignObject, _NodeEventTrigger.TriggerItem.Properties["ExitActions"]);
            }
        }

        private bool _Loaded = false;
        void EventTriggerEditor_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            if (_Loaded)
                return;
            _Loaded = true;
        }

        public void InitEditor(DesignItem designObject, EventTriggerOutlineNode eventTrigger)
        {
            Debug.Assert(designObject.View is Control);

            _DesignObject = designObject;
            _NodeEventTrigger = eventTrigger;
            _componentService = designObject.Services.Component;
            DataContext = this;

            if (_NodeEventTrigger.IsEnabled)
                IsTriggerEditable = true;
            else
                IsTriggerEditable = false;

            if (_NodeEventTrigger.IsSet)
                AreTriggerValuesValid = true;
            else
                AreTriggerValuesValid = false;
            _NodeEventTrigger.PropertyChanged += new PropertyChangedEventHandler(_NodeEventTrigger_PropertyChanged);

            LoadTriggerableEvents();
        }

        void _NodeEventTrigger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSet")
            {
                if (_NodeEventTrigger.IsSet)
                    AreTriggerValuesValid = true;
                else
                    AreTriggerValuesValid = false;
            }
            else if (e.PropertyName == "IsEnabled")
            {
                if (_NodeEventTrigger.IsEnabled)
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
                foreach (MemberDescriptor d in TypeHelper.GetAvailableEvents(_DesignObject.ComponentType))
                {
                    list.Add(d.Name, d);
                }
            }
            return list.Values;
        }

        protected void LoadTriggerableEvents()
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
            if (!node.IsEvent)
                return;
            TriggerableEvents.Add(node);
        }

        ObservableCollection<PropertyNode> _TriggerableEvents = new ObservableCollection<PropertyNode>();
        public ObservableCollection<PropertyNode> TriggerableEvents
        {
            get { return _TriggerableEvents; }
        }

        public PropertyNode SelectedTriggerableEvent
        {
            get
            {
                if (_NodeEventTrigger.RoutedEventProperty != null)
                {
                    var query = TriggerableEvents.Where(c => c.Name == _NodeEventTrigger.RoutedEventName);
                    if (query.Any())
                        return query.First();
                }
                if (TriggerableEvents.Count <= 0)
                    return null;
                return TriggerableEvents.First();
            }
            set
            {
                if (value != null)
                {
                    _NodeEventTrigger.RoutedEventName = value.Name;
                }
            }
        }

        public static readonly StyledProperty<bool> IsTriggerEditableProperty =
            AvaloniaProperty.Register<EventTriggerEditor, bool>(nameof(IsTriggerEditable), false);
        public bool IsTriggerEditable
        {
            get { return GetValue(IsTriggerEditableProperty); }
            set { SetValue(IsTriggerEditableProperty, value); }
        }

        public static readonly StyledProperty<bool> AreTriggerValuesValidProperty =
            AvaloniaProperty.Register<EventTriggerEditor, bool>(nameof(AreTriggerValuesValid), false);
        public bool AreTriggerValuesValid
        {
            get { return GetValue(AreTriggerValuesValidProperty); }
            set { SetValue(AreTriggerValuesValidProperty, value); }
        }

        void PART_SelectorTriggerable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
