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
using Avalonia.Data;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Markup.Xaml.MarkupExtensions;

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
    public class DataTriggerEditor : TemplatedControl
    {
        static DataTriggerEditor()
        {
            // In Avalonia, we set the default style key like this
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
            this.AttachedToVisualTree += DataTriggerEditor_AttachedToVisualTree;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            PART_TriggerValueEditor = e.NameScope.Find("PART_TriggerValueEditor") as ContentControl;
            PART_BindingEditor = e.NameScope.Find("PART_BindingEditor") as ContentControl;

            PART_SetBinding = e.NameScope.Find("PART_SetBinding") as Button;
            if (PART_SetBinding != null)
                PART_SetBinding.Click += OnSetBindingClicked;

            PART_SetMultiBinding = e.NameScope.Find("PART_SetMultiBinding") as Button;
            if (PART_SetMultiBinding != null)
                PART_SetMultiBinding.Click += OnSetMultiBindingClicked;

            PART_SetterEditor = e.NameScope.Find("PART_SetterEditor") as SettersCollectionEditor;
            if (PART_SetterEditor != null && _DesignObject != null && _NodeDataTrigger != null)
            {
                PART_SetterEditor.InitEditor(_DesignObject, _NodeDataTrigger.TriggerItem.Properties["Setters"]);
            }

            PART_EnterActionsEditor = e.NameScope.Find("PART_EnterActionsEditor") as ActionCollectionEditor;
            if (PART_EnterActionsEditor != null && _DesignObject != null && _NodeDataTrigger != null)
            {
                PART_EnterActionsEditor.InitEditor(_DesignObject, _NodeDataTrigger.TriggerItem.Properties["EnterActions"]);
            }

            PART_ExitActionsEditor = e.NameScope.Find("PART_ExitActionsEditor") as ActionCollectionEditor;
            if (PART_ExitActionsEditor != null && _DesignObject != null && _NodeDataTrigger != null)
            {
                PART_ExitActionsEditor.InitEditor(_DesignObject, _NodeDataTrigger.TriggerItem.Properties["ExitActions"]);
            }
        }

        private bool _Loaded = false;
        void DataTriggerEditor_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            if (_Loaded)
                return;
            if (PART_TriggerValueEditor != null)
                PART_TriggerValueEditor.Content = TriggerValueEditor;
            UpdatePARTBindingEditor(TriggerBindingEditor);
            _Loaded = true;
        }

        public void InitEditor(DesignItem designObject, DataTriggerOutlineNode propertyTrigger)
        {
            Debug.Assert(designObject.View is Control);

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
            _NodeDataTrigger.PropertyChanged += _NodeDataTrigger_PropertyChanged;
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

        public static readonly StyledProperty<bool> IsTriggerEditableProperty =
            AvaloniaProperty.Register<DataTriggerEditor, bool>(nameof(IsTriggerEditable), false);
        
        public bool IsTriggerEditable
        {
            get { return GetValue(IsTriggerEditableProperty); }
            set { SetValue(IsTriggerEditableProperty, value); }
        }

        public static readonly StyledProperty<bool> AreTriggerValuesValidProperty =
            AvaloniaProperty.Register<DataTriggerEditor, bool>(nameof(AreTriggerValuesValid), false);
        
        public bool AreTriggerValuesValid
        {
            get { return GetValue(AreTriggerValuesValidProperty); }
            set { SetValue(AreTriggerValuesValidProperty, value); }
        }

        public virtual Control TriggerBindingEditor
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
        public virtual Control TriggerValueEditor
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

                Control editor = EditorManager.CreateEditor(typeof(string));
                editor.DataContext = _NodeDataTrigger;
                return editor;
            }
        }

        public virtual Binding CreateNewBinding()
        {
            return new Binding();
        }

        public virtual MultiBinding CreateNewMultiBinding()
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

            Binding newBinding = CreateNewBinding();
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
            MultiBinding newBinding = CreateNewMultiBinding();
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

        private void UpdatePARTBindingEditor(Control editor)
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
