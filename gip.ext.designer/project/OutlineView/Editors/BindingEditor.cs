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
using gip.ext.designer.OutlineView;
using gip.ext.design;
using gip.ext.design.PropertyGrid;
using gip.ext.designer.PropertyGrid;
using System.Linq;

namespace gip.ext.designer.OutlineView
{
    [CLSCompliant(false)]
    public class BindingEditor : Control, INotifyPropertyChanged, ITypeEditorInitItem
    {
        static BindingEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BindingEditor), new FrameworkPropertyMetadata(typeof(BindingEditor)));
        }

        protected DesignItem _DesignObjectBinding;
        protected IComponentService _componentService;

        public BindingEditor()
        {
            this.Loaded += new RoutedEventHandler(BindingEditor_Loaded);
            this.Unloaded += new RoutedEventHandler(BindingEditor_Unloaded);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        void BindingEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (_DesignObjectBinding != null)
                _DesignObjectBinding.Services.Tool.ToolEvents += OnToolEvents;
        }

        void BindingEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_DesignObjectBinding != null)
                _DesignObjectBinding.Services.Tool.ToolEvents -= OnToolEvents;
        }

        protected virtual void CreateWrapper()
        {
            if (_DesignObjectBinding.Component is Binding)
                _Wrapper = new BindingEditorWrapperSingle(_DesignObjectBinding, null);
        }

        public void InitEditor(DesignItem designObject, TriggerOutlineNodeBase parentTriggerNode)
        {
            InitEditor(designObject);
            _ParentTriggerNode = parentTriggerNode;
        }

        public void InitEditor(DesignItem designObject)
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

        protected BindingEditorWrapperSingle _Wrapper;
        public BindingEditorWrapperSingle Wrapper
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

        protected TriggerOutlineNodeBase _ParentTriggerNode = null;
        public TriggerOutlineNodeBase ParentTriggerNode
        {
            get
            {
                return _ParentTriggerNode;
            }
        }


        //private bool _LockValueEditorRefresh = false;
        public virtual FrameworkElement ConverterEditor
        {
            get
            {
                if (_Wrapper == null)
                    return new TextBox();
                return _Wrapper.ConverterEditor;
            }
        }

        void _Wrapper_PropertyChanged(object sender, PropertyChangedEventArgs e)
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
