using System;
using System.ComponentModel;
using System.Collections;
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
using System.Windows.Markup;


namespace gip.ext.designer.avui.OutlineView
{
    [TemplatePart(Name = "PART_SetBinding", Type = typeof(Button))]
    [TemplatePart(Name = "PART_SetMultiBinding", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ResetBinding", Type = typeof(Button))]
    [TemplatePart(Name = "PART_BindingEditor", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_SelectorTriggerable", Type = typeof(Selector))]
    [TemplatePart(Name = "PART_TriggerValueEditor", Type = typeof(ContentControl))]
    [CLSCompliant(false)]
    public class ConditionEditor : Control, INotifyPropertyChanged, ITypeEditorInitItem
    {
        static ConditionEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ConditionEditor), new FrameworkPropertyMetadata(typeof(ConditionEditor)));
        }

        public Button PART_SetBinding { get; set; }
        public Button PART_SetMultiBinding { get; set; }
        public Button PART_ResetBinding { get; set; }
        public ContentControl PART_BindingEditor { get; set; }
        public Selector PART_SelectorTriggerable { get; set; }
        public ContentControl PART_TriggerValueEditor { get; set; }

        public ConditionEditor()
        {
            this.Loaded += new RoutedEventHandler(ConditionEditor_Loaded);
            this.Unloaded += new RoutedEventHandler(ConditionEditor_Unloaded);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_BindingEditor = Template.FindName("PART_BindingEditor", this) as ContentControl;

            PART_SetBinding = Template.FindName("PART_SetBinding", this) as Button;
            if (PART_SetBinding != null)
                PART_SetBinding.Click += new RoutedEventHandler(OnSetBindingClicked);

            PART_SetMultiBinding = Template.FindName("PART_SetMultiBinding", this) as Button;
            if (PART_SetMultiBinding != null)
                PART_SetMultiBinding.Click += new RoutedEventHandler(OnSetMultiBindingClicked);

            PART_ResetBinding = Template.FindName("PART_ResetBinding", this) as Button;
            if (PART_ResetBinding != null)
                PART_ResetBinding.Click += new RoutedEventHandler(OnResetBindingClicked);
        
            PART_SelectorTriggerable = Template.FindName("PART_SelectorTriggerable", this) as Selector;
            if (PART_SelectorTriggerable != null)
            {
                PART_SelectorTriggerable.DataContext = this;
                PART_SelectorTriggerable.SelectionChanged += new SelectionChangedEventHandler(PART_SelectorTriggerable_SelectionChanged);
            }

            PART_TriggerValueEditor = Template.FindName("PART_TriggerValueEditor", this) as ContentControl;
        }

        private bool _Loaded = false;
        void ConditionEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_Loaded)
            {
                RefreshView();
            }
            _Loaded = true;
        }

        void ConditionEditor_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        protected virtual void CreateWrapper()
        {
            if (_DesignObjectCondition == null)
                return;
            if (_DesignObjectCondition.Component is Condition)
                _Wrapper = new ConditionWrapper(_DesignObjectCondition, _ParentTriggerNode);
        }

        public void InitEditor(DesignItem designObject, MultiTriggerNodeBase parentTriggerNode)
        {
            LoadItemsCollection(designObject);
            _ParentTriggerNode = parentTriggerNode;
        }

        public void LoadItemsCollection(DesignItem designObject)
        {
            if (_DesignObjectCondition == designObject)
                return;

            _DesignObjectCondition = designObject;

            CreateWrapper();
            if (_Wrapper != null)
            {
                DataContext = _Wrapper;
                _Wrapper.PropertyChanged += _Wrapper_PropertyChanged;
                if (ParentTriggerNode != null)
                    IsMultiDataTrigger = ParentTriggerNode is MultiDataTriggerOutlineNode ? true : false;
            }
            else
                DataContext = this;
            LoadTriggerableProperties();
        }

        protected virtual void OnToolEvents(object sender, ToolEventArgs e)
        {
        }

        protected ConditionWrapper _Wrapper;
        public ConditionWrapper Wrapper
        {
            get
            {
                return _Wrapper;
            }
            set
            {
                if (_Wrapper != null)
                    _Wrapper.PropertyChanged -= _Wrapper_PropertyChanged;
                _Wrapper = value;
                if (_Wrapper != null)
                {
                    DataContext = _Wrapper;
                    _Wrapper.PropertyChanged += _Wrapper_PropertyChanged;
                    if (ParentTriggerNode != null)
                        IsMultiDataTrigger = ParentTriggerNode is MultiDataTriggerOutlineNode ? true : false;
                    RefreshView();
                }
            }
        }

        protected void RefreshView()
        {
            if (_TriggerableProperties.Count <= 0)
                LoadTriggerableProperties();
            UpdatePARTBindingEditor(TriggerBindingEditor);
            if (PART_TriggerValueEditor != null)
                PART_TriggerValueEditor.Content = TriggerValueEditor;
            RaisePropertyChanged("SelectedTriggerableProperty");
        }

        protected DesignItem _DesignObjectCondition;
        public DesignItem DesignObjectCondition
        {
            get
            {
                if (Wrapper == null)
                    return _DesignObjectCondition;
                return Wrapper.DesignObjectCondition;
            }
        }

        protected MultiTriggerNodeBase _ParentTriggerNode;
        public MultiTriggerNodeBase ParentTriggerNode
        {
            get
            {
                if (Wrapper == null)
                    return _ParentTriggerNode;
                return Wrapper.ParentTriggerNode;
            }
        }

        public static readonly DependencyProperty IsMultiDataTriggerProperty
            = DependencyProperty.Register("IsMultiDataTrigger", typeof(bool), typeof(ConditionEditor), new PropertyMetadata(false));
        public bool IsMultiDataTrigger
        {
            get { return (bool)GetValue(IsMultiDataTriggerProperty); }
            set { SetValue(IsMultiDataTriggerProperty, value); }
        }

        public static readonly DependencyProperty IsTemplateTriggerProperty
            = DependencyProperty.Register("IsTemplateTrigger", typeof(bool), typeof(ConditionEditor), new PropertyMetadata(false));
        public bool IsTemplateTrigger
        {
            get { return (bool)GetValue(IsTemplateTriggerProperty); }
            set { SetValue(IsTemplateTriggerProperty, value); }
        }


        public virtual FrameworkElement BindingEditor
        {
            get
            {
                if (_Wrapper == null)
                    return new TextBox();
                return _Wrapper.BindingEditor;
            }
        }

        void _Wrapper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Description")
            {
                RaisePropertyChanged("TriggerInfoText");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public virtual MarkupExtension CreateNewBinding()
        {
            return new Binding();
        }

        public virtual MarkupExtension CreateNewMultiBinding()
        {
            return new MultiBinding();
        }

        protected virtual void OnSetBindingClicked(object sender, RoutedEventArgs e)
        {
            if ((Wrapper == null) || (PART_BindingEditor == null))
                return;
            if (Wrapper.Binding.Value != null)
            {
                UpdatePARTBindingEditor(null);
                Wrapper.Binding.Reset();
            }

            MarkupExtension newBinding = CreateNewBinding();
            //DesignItem newBindingItem = _DesignObject.Services.Component.RegisterComponentForDesigner(newBinding);
            Wrapper.Binding.Value = newBinding;
            UpdatePARTBindingEditor(TriggerBindingEditor);
        }

        protected virtual void OnSetMultiBindingClicked(object sender, RoutedEventArgs e)
        {
            if ((Wrapper == null) || (PART_BindingEditor == null))
                return;
            if (Wrapper.Binding.Value != null)
            {
                UpdatePARTBindingEditor(null);
                Wrapper.Binding.Reset();
            }
            MarkupExtension newBinding = CreateNewMultiBinding();
            //DesignItem newBindingItem = _DesignObject.Services.Component.RegisterComponentForDesigner(newBinding);
            Wrapper.Binding.Value = newBinding;
            UpdatePARTBindingEditor(TriggerBindingEditor);
        }

        protected virtual void OnResetBindingClicked(object sender, RoutedEventArgs e)
        {
            if ((Wrapper == null) || (PART_BindingEditor == null))
                return;
            if (Wrapper.Binding.Value != null)
            {
                UpdatePARTBindingEditor(null);
                Wrapper.Binding.Reset();
            }
        }

        public virtual FrameworkElement TriggerBindingEditor
        {
            get
            {
                if (Wrapper == null)
                    return null;
                if (Wrapper.Binding.Value != null)
                {
                    if (Wrapper.Binding.Value is MultiBinding)
                    {
                        MultiBindingEditor editor = new MultiBindingEditor();
                        editor.LoadItemsCollection(Wrapper.Binding.ValueItem);
                        return editor;
                    }
                    else if (Wrapper.Binding.Value is Binding)
                    {
                        BindingEditor editor = new BindingEditor();
                        editor.LoadItemsCollection(Wrapper.Binding.ValueItem);
                        return editor;
                    }
                }
                return null;
            }
        }

        private void UpdatePARTBindingEditor(FrameworkElement editor)
        {
            if (PART_BindingEditor == null)
                return;
            if (PART_BindingEditor.Content != null)
            {
                if (PART_BindingEditor.Content is INotifyPropertyChanged)
                    (PART_BindingEditor.Content as INotifyPropertyChanged).PropertyChanged -= _Editor_PropertyChanged;
            }
            PART_BindingEditor.Content = editor;
            if (PART_BindingEditor.Content != null)
            {
                if (PART_BindingEditor.Content is INotifyPropertyChanged)
                    (PART_BindingEditor.Content as INotifyPropertyChanged).PropertyChanged += _Editor_PropertyChanged;
            }
            if (_ParentTriggerNode != null)
                _ParentTriggerNode.RaisePropertyChanged("TriggerInfoText");
        }

        void _Editor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TriggerInfoText")
            {
                if (_ParentTriggerNode != null)
                    _ParentTriggerNode.RaisePropertyChanged("TriggerInfoText");
                if (PART_TriggerValueEditor != null)
                    PART_TriggerValueEditor.Content = TriggerValueEditor;
            }
        }

        public string TriggerInfoText
        {
            get
            {
                if (Wrapper != null)
                    return Wrapper.Description;
                else if ((PART_BindingEditor != null) && (PART_BindingEditor.Content != null))
                {
                    if (PART_BindingEditor.Content is BindingEditor)
                        return (PART_BindingEditor.Content as BindingEditor).TriggerInfoText;
                    else if (PART_BindingEditor.Content is MultiBindingEditor)
                        return (PART_BindingEditor.Content as MultiBindingEditor).TriggerInfoText;
                }
                return "";
            }
        }

        void PART_SelectorTriggerable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PART_TriggerValueEditor != null)
            {
                PART_TriggerValueEditor.Content = TriggerValueEditor;
            }
        }



        protected virtual PropertyNode CreatePropertyNode()
        {
            return new PropertyNode();
        }

        protected DesignItem ParentUIItem
        {
            get
            {
                if (ParentTriggerNode == null)
                    return null;
                DesignItem uiItem = ParentTriggerNode.DesignItem;
                if (typeof(UIElement).IsAssignableFrom(uiItem.ComponentType))
                    return uiItem;
                while (uiItem != null)
                {
                    uiItem = uiItem.Parent;
                    if (uiItem == null)
                        return null;
                    else if (typeof(UIElement).IsAssignableFrom(uiItem.ComponentType))
                        return uiItem;
                }
                return null;
            }
        }

        IList<MemberDescriptor> GetDescriptors()
        {
            SortedList<string, MemberDescriptor> list = new SortedList<string, MemberDescriptor>();

            if (ParentUIItem != null)
            {
                foreach (MemberDescriptor d in TypeHelper.GetAvailableProperties(ParentUIItem.Component, true))
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
            DesignItemProperty[] designProperties = new DesignItemProperty[] { ParentUIItem.Properties[md.Name] };
            PropertyNode node = CreatePropertyNode();
            node.Load(designProperties);
            if (!node.IsDependencyProperty)
                return;
            _TriggerableProperties.Add(node);
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
                if (TriggerableProperties.Count <= 0)
                    return null;
                //if (!_LockValueEditorRefresh && !IsTriggerEditable && PART_TriggerValueEditor != null)
                if (!_LockValueEditorRefresh && PART_TriggerValueEditor != null)
                    PART_TriggerValueEditor.Content = TriggerValueEditor;
                if ((Wrapper != null) && (Wrapper.Property.Value != null))
                {
                    var query = TriggerableProperties.Where(c => c.Name == Wrapper.PropertyName);
                    if (query.Any())
                        return query.First();
                }
                return TriggerableProperties.First();
            }
            set
            {
                if ((value != null) && (Wrapper != null))
                {
                    Wrapper.PropertyName = value.Name;
                }
            }
        }

        protected bool _LockValueEditorRefresh = false;
        public virtual FrameworkElement TriggerValueEditor
        {
            get
            {
                _LockValueEditorRefresh = true;
                if (ParentTriggerNode is MultiDataTriggerOutlineNode)
                {
                    if (Wrapper != null)
                    {
                        return Wrapper.Value.Editor;
                    }
                }
                else
                {
                    if (PART_SelectorTriggerable != null && SelectedTriggerableProperty != null)
                    {
                        //PropertyNode selectedNode = PART_SelectorTriggerable.SelectedItem as PropertyNode;
                        FrameworkElement editor = SelectedTriggerableProperty.Editor;
                        if (editor != null)
                            editor.DataContext = Wrapper.Value;
                        _LockValueEditorRefresh = false;
                        return editor;
                    }
                }
                _LockValueEditorRefresh = false;
                return new ContentControl();
            }
        }

    }
}
