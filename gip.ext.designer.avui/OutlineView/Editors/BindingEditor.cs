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
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace gip.ext.designer.avui.OutlineView
{
    [CLSCompliant(false)]
    public class BindingEditor : TemplatedControl, INotifyPropertyChanged, ITypeEditorInitItem
    {
        static BindingEditor()
        {
            // In AvaloniaUI, DefaultStyleKey is handled differently
        }

        protected DesignItem _DesignObjectBinding;
        protected IComponentService _componentService;

        public BindingEditor()
        {
            this.Loaded += BindingEditor_Loaded;
            this.Unloaded += BindingEditor_Unloaded;
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
        }

        void BindingEditor_Loaded(object? sender, RoutedEventArgs e)
        {
            if (_DesignObjectBinding != null)
                _DesignObjectBinding.Services.Tool.ToolEvents += OnToolEvents;
        }

        void BindingEditor_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (_DesignObjectBinding != null)
                _DesignObjectBinding.Services.Tool.ToolEvents -= OnToolEvents;
        }

        protected virtual void CreateWrapper()
        {
            if (_DesignObjectBinding.Component is Avalonia.Data.Binding)
                _Wrapper = new BindingEditorWrapperSingle(_DesignObjectBinding, null);
        }

        public void InitEditor(DesignItem designObject, TriggerOutlineNodeBase parentTriggerNode)
        {
            LoadItemsCollection(designObject);
            _ParentTriggerNode = parentTriggerNode;
        }

        public void LoadItemsCollection(DesignItem designObject)
        {
            if (_DesignObjectBinding == designObject)
                return;

            _DesignObjectBinding = designObject;

            _componentService = _DesignObjectBinding.Services.Component;
            CreateWrapper();
            if (_Wrapper != null)
            {
                DataContext = _Wrapper;
                _Wrapper.PropertyChanged += _Wrapper_PropertyChanged;
            }
            else
                DataContext = this;
        }

        protected virtual void OnToolEvents(object sender, ToolEventArgs e)
        {
        }

        protected BindingEditorWrapperSingle? _Wrapper;
        public BindingEditorWrapperSingle? Wrapper
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
                    _DesignObjectBinding = _Wrapper.DesignObjectBinding;
                    DataContext = _Wrapper;
                    _Wrapper.PropertyChanged += _Wrapper_PropertyChanged;
                }
            }
        }

        protected TriggerOutlineNodeBase? _ParentTriggerNode = null;
        public TriggerOutlineNodeBase? ParentTriggerNode
        {
            get
            {
                return _ParentTriggerNode;
            }
        }


        //private bool _LockValueEditorRefresh = false;
        public virtual Control ConverterEditor
        {
            get
            {
                if (_Wrapper == null)
                    return new TextBox();
                return _Wrapper.ConverterEditor;
            }
        }

        void _Wrapper_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Description")
            {
                RaisePropertyChanged("TriggerInfoText");
            }
        }

        public virtual string TriggerInfoText
        {
            get
            {
                if (Wrapper != null)
                    return Wrapper.Description;
                return "";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
