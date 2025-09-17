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
    [TemplatePart(Name = "PART_BindingList", Type = typeof(Selector))]
    [TemplatePart(Name = "PART_BindingEditor", Type = typeof(ContentControl))]
    [CLSCompliant(false)]
    public class MultiBindingEditor : Control, INotifyPropertyChanged, ITypeEditorInitItem
    {
        static MultiBindingEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiBindingEditor), new FrameworkPropertyMetadata(typeof(MultiBindingEditor)));
        }

        protected DesignItem _DesignObjectBinding;
        protected IComponentService _componentService;
        public Button PART_ButtonAddItem { get; set; }
        public Button PART_ButtonRemoveItem { get; set; }
        public Selector PART_BindingList { get; set; }
        public ContentControl PART_BindingEditor { get; set; }

        public MultiBindingEditor()
        {
            this.Loaded += new RoutedEventHandler(MultiBindingEditor_Loaded);
            this.Unloaded += new RoutedEventHandler(MultiBindingEditor_Unloaded);
        }

        public override void OnApplyTemplate()
        {
            PART_ButtonAddItem = Template.FindName("PART_AddItem", this) as Button;
            if (PART_ButtonAddItem != null)
                PART_ButtonAddItem.Click += new RoutedEventHandler(OnAddItemClicked);

            PART_ButtonRemoveItem = Template.FindName("PART_RemoveItem", this) as Button;
            if (PART_ButtonRemoveItem != null)
                PART_ButtonRemoveItem.Click += new RoutedEventHandler(OnRemoveItemClicked);

            PART_BindingEditor = Template.FindName("PART_BindingEditor", this) as ContentControl;

            PART_BindingList = Template.FindName("PART_BindingList", this) as Selector;
            if (PART_BindingList != null)
            {
                PART_BindingList.SelectionChanged += new SelectionChangedEventHandler(PART_BindingList_SelectionChanged);
            }

            base.OnApplyTemplate();
        }

        bool _Loaded = false;
        void MultiBindingEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (_DesignObjectBinding != null)
                _DesignObjectBinding.Services.Tool.ToolEvents += OnToolEvents;
            if (_Loaded)
                return;

            if ((PART_BindingList != null) && (Wrapper != null))
            {
                if (Wrapper.BindingsCollection.Count > 0)
                {
                    if (PART_BindingList.SelectedIndex < 0)
                        PART_BindingList.SelectedIndex = 0;
                    else
                        RefreshBindingEditor();
                }
            }
            _Loaded = true;
        }

        void MultiBindingEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_DesignObjectBinding != null)
                _DesignObjectBinding.Services.Tool.ToolEvents -= OnToolEvents;
        }

        void PART_BindingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshBindingEditor();
        }

        private void RefreshBindingEditor()
        {
            if (PART_BindingEditor != null)
            {
                BindingEditorWrapperSingle selectedWrapper = PART_BindingList.SelectedItem as BindingEditorWrapperSingle;
                if (selectedWrapper != null)
                {
                    BindingEditor editor = PART_BindingEditor.Content as BindingEditor;
                    if (editor == null)
                    {
                        editor = CreateBindingEditor();
                        editor.Wrapper = selectedWrapper;
                        PART_BindingEditor.Content = editor;
                    }
                    else
                    {
                        editor.Wrapper = selectedWrapper;
                    }
                }
                else
                {
                    PART_BindingEditor.Content = null;
                }
            }
        }

        protected virtual void CreateWrapper()
        {
            if (_DesignObjectBinding.Component is MultiBinding)
                _Wrapper = new BindingEditorWrapperMulti(_DesignObjectBinding);
        }

        protected virtual BindingEditor CreateBindingEditor()
        {
            return new BindingEditor();
        }

        public void InitEditor(DesignItem designObject, TriggerOutlineNodeBase parentTriggerNode)
        {
            LoadItemsCollection(designObject);
            _ParentTriggerNode = parentTriggerNode;
        }

        public void LoadItemsCollection(DesignItem designObject)
        {
            Debug.Assert(designObject.Component is MultiBinding);
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

        protected BindingEditorWrapperMulti _Wrapper;
        public BindingEditorWrapperMulti Wrapper
        {
            get
            {
                return _Wrapper;
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

        private void OnAddItemClicked(object sender, RoutedEventArgs e)
        {
            if ((PART_BindingList == null) || (Wrapper == null))
                return;
            BindingEditorWrapperSingle newWrapper = Wrapper.AddNewBinding();
            if (newWrapper != null)
            {
                PART_BindingList.SelectedItem = newWrapper;
            }
        }

        private void OnRemoveItemClicked(object sender, RoutedEventArgs e)
        {
            if ((PART_BindingList == null) || (Wrapper == null))
                return;
            BindingEditorWrapperSingle selectedWrapper = PART_BindingList.SelectedItem as BindingEditorWrapperSingle;
            if (selectedWrapper == null)
                return;
            Wrapper.RemoveBinding(selectedWrapper);
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
