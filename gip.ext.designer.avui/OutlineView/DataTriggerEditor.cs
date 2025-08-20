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
using System.Windows.Markup;


namespace gip.ext.designer.avui.OutlineView
{
    [TemplatePart(Name = "PART_TriggerValueEditor", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_BindingEditor", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_SetterEditor", Type = typeof(SettersCollectionEditor))]
    [TemplatePart(Name = "PART_SetBinding", Type = typeof(Button))]
    [TemplatePart(Name = "PART_SetMultiBinding", Type = typeof(Button))]
    [TemplatePart(Name = "PART_EnterActionsEditor", Type = typeof(ActionCollectionEditor))]
    [TemplatePart(Name = "PART_ExitActionsEditor", Type = typeof(ActionCollectionEditor))]
    [CLSCompliant(false)]
    public class DataTriggerEditor : Control
    {
        static DataTriggerEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataTriggerEditor), new FrameworkPropertyMetadata(typeof(DataTriggerEditor)));
        }


        protected DesignItem _DesignObject;
        protected DataTriggerOutlineNode _NodeDataTrigger;
        protected IComponentService _componentService;

        public ContentControl PART_TriggerValueEditor { get; set; }
        public ContentControl PART_BindingEditor { get; set; }
        public SettersCollectionEditor PART_SetterEditor { get; set; }
        public Button PART_SetBinding { get; set; }
        public Button PART_SetMultiBinding { get; set; }
        public ActionCollectionEditor PART_EnterActionsEditor { get; set; }
        public ActionCollectionEditor PART_ExitActionsEditor { get; set; }

        public DataTriggerEditor()
        {
            this.Loaded += new RoutedEventHandler(DataTriggerEditor_Loaded);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PART_TriggerValueEditor = Template.FindName("PART_TriggerValueEditor", this) as ContentControl;
            PART_BindingEditor = Template.FindName("PART_BindingEditor", this) as ContentControl;

            PART_SetBinding = Template.FindName("PART_SetBinding", this) as Button;
            if (PART_SetBinding != null)
                PART_SetBinding.Click += new RoutedEventHandler(OnSetBindingClicked);

            PART_SetMultiBinding = Template.FindName("PART_SetMultiBinding", this) as Button;
            if (PART_SetMultiBinding != null)
                PART_SetMultiBinding.Click += new RoutedEventHandler(OnSetMultiBindingClicked);

            PART_SetterEditor = Template.FindName("PART_SetterEditor", this) as SettersCollectionEditor;
            if (PART_SetterEditor != null)
            {
                PART_SetterEditor.InitEditor(_DesignObject, _NodeDataTrigger.TriggerItem.Properties["Setters"]);
            }

            PART_EnterActionsEditor = Template.FindName("PART_EnterActionsEditor", this) as ActionCollectionEditor;
            if (PART_EnterActionsEditor != null)
            {
                PART_EnterActionsEditor.InitEditor(_DesignObject, _NodeDataTrigger.TriggerItem.Properties["EnterActions"]);
            }

            PART_ExitActionsEditor = Template.FindName("PART_ExitActionsEditor", this) as ActionCollectionEditor;
            if (PART_ExitActionsEditor != null)
            {
                PART_ExitActionsEditor.InitEditor(_DesignObject, _NodeDataTrigger.TriggerItem.Properties["ExitActions"]);
            }
        }

        private bool _Loaded = false;
        void DataTriggerEditor_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!IsTriggerEditable && PART_TriggerValueEditor != null)
            if (_Loaded)
                return;
            PART_TriggerValueEditor.Content = TriggerValueEditor;
            UpdatePARTBindingEditor(TriggerBindingEditor);
            _Loaded = true;
        }

        public void InitEditor(DesignItem designObject, DataTriggerOutlineNode propertyTrigger)
        {
            Debug.Assert(designObject.View is FrameworkElement);

            _DesignObject = designObject;
            _NodeDataTrigger = propertyTrigger;
            _componentService = designObject.Services.Component;
            DataContext = this;

            if (_NodeDataTrigger.IsEnabled)
                IsTriggerEditable = true;
            else
                IsTriggerEditable = false;

            if (_NodeDataTrigger.IsSet)
                AreTriggerValuesValid = true;
            else
                AreTriggerValuesValid = false;
            _NodeDataTrigger.PropertyChanged += new PropertyChangedEventHandler(_NodeDataTrigger_PropertyChanged);
        }

        void _NodeDataTrigger_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSet")
            {
                if (_NodeDataTrigger.IsSet)
                    AreTriggerValuesValid = true;
                else
                    AreTriggerValuesValid = false;
            }
            else if (e.PropertyName == "IsEnabled")
            {
                if (_NodeDataTrigger.IsEnabled)
                    IsTriggerEditable = true;
                else
                    IsTriggerEditable = false;
            }
            else if (e.PropertyName == "TriggerInfoText")
            {
                if (PART_TriggerValueEditor != null)
                    PART_TriggerValueEditor.Content = TriggerValueEditor;
            }
        }


        public static readonly DependencyProperty IsTriggerEditableProperty
            = DependencyProperty.Register("IsTriggerEditable", typeof(bool), typeof(DataTriggerEditor), new PropertyMetadata(false));
        public bool IsTriggerEditable
        {
            get { return (bool)GetValue(IsTriggerEditableProperty); }
            set { SetValue(IsTriggerEditableProperty, value); }
        }

        public static readonly DependencyProperty AreTriggerValuesValidProperty
            = DependencyProperty.Register("AreTriggerValuesValid", typeof(bool), typeof(DataTriggerEditor), new PropertyMetadata(false));
        public bool AreTriggerValuesValid
        {
            get { return (bool)GetValue(AreTriggerValuesValidProperty); }
            set { SetValue(AreTriggerValuesValidProperty, value); }
        }



        public virtual FrameworkElement TriggerBindingEditor
        {
            get
            {
                if (_NodeDataTrigger == null)
                    return null;
                if (_NodeDataTrigger.BindingProperty.ValueOnInstance != null)
                {
                    if (_NodeDataTrigger.BindingProperty.ValueOnInstance is MultiBinding)
                    {
                        MultiBindingEditor editor = new MultiBindingEditor();
                        editor.InitEditor(_NodeDataTrigger.BindingProperty.Value, _NodeDataTrigger);
                        return editor;
                    }
                    else if (_NodeDataTrigger.BindingProperty.ValueOnInstance is Binding)
                    {
                        BindingEditor editor = new BindingEditor();
                        editor.InitEditor(_NodeDataTrigger.BindingProperty.Value, _NodeDataTrigger);
                        return editor;
                    }
                }
                return null;
            }
        }

        protected bool _LockValueEditorRefresh = false;
        public virtual FrameworkElement TriggerValueEditor
        {
            get
            {
                _LockValueEditorRefresh = true;

                // TODO: Provide Value um Type heruaszufinden => Editor-Generieren für Type
                //if (_NodeDataTrigger.BindingProperty.ValueOnInstance is MultiBinding)
                //{
                //}
                //else if (_NodeDataTrigger.BindingProperty.ValueOnInstance is Binding)
                //{
                //}

                FrameworkElement editor = EditorManager.CreateEditor(typeof(string));
                editor.DataContext = _NodeDataTrigger;
                return editor;
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
            if ((_NodeDataTrigger == null) || (PART_BindingEditor == null))
                return;
            if (!_NodeDataTrigger.IsEnabled)
                return;
            if (_NodeDataTrigger.BindingProperty.ValueOnInstance != null)
            {
                UpdatePARTBindingEditor(null);
                _NodeDataTrigger.BindingProperty.Reset();
            }

            MarkupExtension newBinding = CreateNewBinding();
            //DesignItem newBindingItem = _DesignObject.Services.Component.RegisterComponentForDesigner(newBinding);
            _NodeDataTrigger.BindingProperty.SetValue(newBinding);
            UpdatePARTBindingEditor(TriggerBindingEditor);
        }

        protected virtual void OnSetMultiBindingClicked(object sender, RoutedEventArgs e)
        {
            if ((_NodeDataTrigger == null) || (PART_BindingEditor == null))
                return;
            if (!_NodeDataTrigger.IsEnabled)
                return;
            if (_NodeDataTrigger.BindingProperty.ValueOnInstance != null)
            {
                UpdatePARTBindingEditor(null);
                _NodeDataTrigger.BindingProperty.Reset();
            }
            MarkupExtension newBinding = CreateNewMultiBinding();
            //DesignItem newBindingItem = _DesignObject.Services.Component.RegisterComponentForDesigner(newBinding);
            _NodeDataTrigger.BindingProperty.SetValue(newBinding);
            UpdatePARTBindingEditor(TriggerBindingEditor);
        }

        void _Editor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TriggerInfoText")
            {
                if (_NodeDataTrigger != null)
                    _NodeDataTrigger.RaisePropertyChanged("TriggerInfoText");
            }
        }

        public string TriggerInfoText
        {
            get
            {
                if ((PART_BindingEditor != null) && (PART_BindingEditor.Content != null))
                {
                    if (PART_BindingEditor.Content is BindingEditor)
                        return (PART_BindingEditor.Content as BindingEditor).TriggerInfoText;
                    else if (PART_BindingEditor.Content is MultiBindingEditor)
                        return (PART_BindingEditor.Content as MultiBindingEditor).TriggerInfoText;
                }
                return "";
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
            if (_NodeDataTrigger != null)
                _NodeDataTrigger.RaisePropertyChanged("TriggerInfoText");
        }
    }
}
