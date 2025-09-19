using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Data;
using Avalonia.Styling;
using gip.ext.designer.avui.OutlineView;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using gip.ext.designer.avui.PropertyGrid;
using System.Linq;

namespace gip.ext.designer.avui.OutlineView
{
    [TemplatePart(Name = "PART_AddItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_RemoveItem", Type = typeof(Button))]
    [TemplatePart(Name = "PART_BindingList", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_BindingEditor", Type = typeof(ContentControl))]
    [CLSCompliant(false)]
    public class MultiBindingEditor : TemplatedControl, ITypeEditorInitItem // INotifyPropertyChanged
    {
        static MultiBindingEditor()
        {
            // In AvaloniaUI, we don't need to override DefaultStyleKeyProperty explicitly
            // The styling system will automatically look for styles targeting this type
        }

        protected DesignItem _DesignObjectBinding;
        protected IComponentService _componentService;
        public Button PART_ButtonAddItem { get; set; }
        public Button PART_ButtonRemoveItem { get; set; }
        public ItemsControl PART_BindingList { get; set; }
        public ContentControl PART_BindingEditor { get; set; }

        public MultiBindingEditor()
        {
            this.Loaded += MultiBindingEditor_Loaded;
            this.Unloaded += MultiBindingEditor_Unloaded;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            PART_ButtonAddItem = e.NameScope.Find("PART_AddItem") as Button;
            if (PART_ButtonAddItem != null)
                PART_ButtonAddItem.Click += OnAddItemClicked;

            PART_ButtonRemoveItem = e.NameScope.Find("PART_RemoveItem") as Button;
            if (PART_ButtonRemoveItem != null)
                PART_ButtonRemoveItem.Click += OnRemoveItemClicked;

            PART_BindingEditor = e.NameScope.Find("PART_BindingEditor") as ContentControl;

            PART_BindingList = e.NameScope.Find("PART_BindingList") as ListBox;
            if (PART_BindingList is ListBox listBox)
            {
                listBox.SelectionChanged += PART_BindingList_SelectionChanged;
            }

            base.OnApplyTemplate(e);
        }

        bool _Loaded = false;
        void MultiBindingEditor_Loaded(object? sender, RoutedEventArgs e)
        {
            if (_DesignObjectBinding != null)
                _DesignObjectBinding.Services.Tool.ToolEvents += OnToolEvents;
            if (_Loaded)
                return;

            if ((PART_BindingList != null) && (Wrapper != null))
            {
                if (Wrapper.BindingsCollection.Count > 0)
                {
                    if (PART_BindingList is ListBox listBox)
                    {
                        if (listBox.SelectedIndex < 0)
                            listBox.SelectedIndex = 0;
                        else
                            RefreshBindingEditor();
                    }
                }
            }
            _Loaded = true;
        }

        void MultiBindingEditor_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (_DesignObjectBinding != null)
                _DesignObjectBinding.Services.Tool.ToolEvents -= OnToolEvents;
        }

        void PART_BindingList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            RefreshBindingEditor();
        }

        private void RefreshBindingEditor()
        {
            if (PART_BindingEditor != null)
            {
                BindingEditorWrapperSingle selectedWrapper = null;
                if (PART_BindingList is ListBox listBox)
                {
                    selectedWrapper = listBox.SelectedItem as BindingEditorWrapperSingle;
                }

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
        public virtual Control ConverterEditor
        {
            get
            {
                if (_Wrapper == null)
                    return new TextBox();
                return _Wrapper.ConverterEditor;
            }
        }

        private void OnAddItemClicked(object? sender, RoutedEventArgs e)
        {
            if ((PART_BindingList == null) || (Wrapper == null))
                return;
            BindingEditorWrapperSingle newWrapper = Wrapper.AddNewBinding();
            if (newWrapper != null)
            {
                if (PART_BindingList is ListBox listBox)
                {
                    listBox.SelectedItem = newWrapper;
                }
            }
        }

        private void OnRemoveItemClicked(object? sender, RoutedEventArgs e)
        {
            if ((PART_BindingList == null) || (Wrapper == null))
                return;
            BindingEditorWrapperSingle selectedWrapper = null;
            if (PART_BindingList is ListBox listBox)
            {
                selectedWrapper = listBox.SelectedItem as BindingEditorWrapperSingle;
            }
            if (selectedWrapper == null)
                return;
            Wrapper.RemoveBinding(selectedWrapper);
        }

        void _Wrapper_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Description")
            {               
                //RaisePropertyChanged("TriggerInfoText");
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

        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void RaisePropertyChanged(string name)
        //{
        //    base.RaisePropertyChanged(name);
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(name));
        //    }
        //}

    }
}
