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
using System.Windows.Markup;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace gip.ext.designer.avui.OutlineView
{
    public abstract class BindingEditorWrapperBase : INotifyPropertyChanged
    {
        protected DesignItem _DesignObjectBinding;

        public BindingEditorWrapperBase(DesignItem designObjectBinding)
        {
            _DesignObjectBinding = designObjectBinding;
            LoadProperties();
        }

        public DesignItem DesignObjectBinding
        {
            get
            {
                return _DesignObjectBinding;
            }
        }

        public virtual string Description
        {
            get
            {
                return "BindingBase";
            }
        }

        #region Wrap Properties of Binding
        protected virtual PropertyNode CreatePropertyNode()
        {
            return new PropertyNode();
        }

        protected void LoadProperties()
        {
            foreach (var md in GetDescriptors())
            {
                AddNode(md);
            }
        }

        IList<MemberDescriptor> GetDescriptors()
        {
            SortedList<string, MemberDescriptor> list = new SortedList<string, MemberDescriptor>();

            if (_DesignObjectBinding != null)
            {
                foreach (MemberDescriptor d in TypeHelper.GetAvailableProperties(_DesignObjectBinding.Component, true))
                {
                    list.Add(d.Name, d);
                }
            }
            return list.Values;
        }


        void AddNode(MemberDescriptor md)
        {
            DesignItemProperty[] designProperties = new DesignItemProperty[] { _DesignObjectBinding.Properties[md.Name] };
            PropertyNode node = CreatePropertyNode();
            node.Load(designProperties);
            if (node.Editor != null)
                node.Editor.DataContext = node;
            PropertiesOfBinding.Add(node);
            node.PropertyChanged += new PropertyChangedEventHandler(node_PropertyChanged);
        }

        protected virtual void node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        ObservableCollection<PropertyNode> _PropertiesOfBinding = new ObservableCollection<PropertyNode>();
        public ObservableCollection<PropertyNode> PropertiesOfBinding
        {
            get { return _PropertiesOfBinding; }
        }

        protected PropertyNode FindPropertyNode(string propertyName)
        {
            return PropertiesOfBinding.FirstOrDefault(c => c.Name == propertyName);
        }

        protected string FindPropertyValueString(string propertyName)
        {
            PropertyNode node = FindPropertyNode(propertyName);
            return node != null ? node.ValueString : String.Empty;
        }
        #endregion


        public PropertyNode BindingGroupName 
        {
            get
            {
                return FindPropertyNode("BindingGroupName");
            }
        }

        public PropertyNode FallbackValue 
        {
            get
            {
                return FindPropertyNode("FallbackValue");
            }
        }

        public PropertyNode StringFormat 
        {
            get
            {
                return FindPropertyNode("StringFormat");
            }
        }

        public PropertyNode TargetNullValue 
        {
            get
            {
                return FindPropertyNode("TargetNullValue");
            }
        }

        public PropertyNode ConverterCulture
        {
            get
            {
                return FindPropertyNode("ConverterCulture");
            }
        }

        public PropertyNode Priority
        {
            get
            {
                return FindPropertyNode("Priority");
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


    [CLSCompliant(false)]
    public class BindingEditorWrapperSingle : BindingEditorWrapperBase
    {
        public BindingEditorWrapperSingle(DesignItem designObjectBinding, BindingEditorWrapperMulti parentMultiWrapper) 
            : base(designObjectBinding)
        {
            _ParentMultiWrapper = parentMultiWrapper;
        }

        protected BindingEditorWrapperMulti _ParentMultiWrapper;
        public BindingEditorWrapperMulti ParentMultiWrapper
        {
            get
            {
                return _ParentMultiWrapper;
            }
        }

        public override string Description
        {
            get
            {
                string result = FindPropertyValueString("Path") + FindPropertyValueString("ElementName") + FindPropertyValueString("Source");
                if (String.IsNullOrEmpty(result))
                    result = "---";
                return result;
            }
        }

        protected override void node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "Value") && ((sender == Path) || (sender == ElementName) || (sender == Source)))
            {
                RaisePropertyChanged("Description");
            }
        }

        //private Control _DummyEditor = new Control() { Height = 2 };
        public Control ConverterEditor
        {
            get
            {
                if (DesignObjectBinding.Parent == null)
                    return null;
                if (DesignObjectBinding.Parent.Component is Collection<BindingBase> || DesignObjectBinding.Parent.Component is MultiBinding)
                {
                    return null;
                }
                else if (Converter == null)
                    return null;
                else if (Converter.Editor == null)
                    return null;
                else
                {
                    if (Converter.Editor.DataContext != Converter)
                        Converter.Editor.DataContext = Converter;
                    return Converter.Editor;
                }
            }
        }

        public PropertyNode Converter
        {
            get
            {
                return FindPropertyNode("Converter");
            }
        }

        public PropertyNode ConverterParameter
        {
            get
            {
                return FindPropertyNode("ConverterParameter");
            }
        }

        public PropertyNode ElementName
        {
            get
            {
                return FindPropertyNode("ElementName");
            }
        }

        public PropertyNode IsAsync
        {
            get
            {
                return FindPropertyNode("IsAsync");
            }
        }

        public PropertyNode Mode
        {
            get
            {
                return FindPropertyNode("Mode");
            }
        }

        public PropertyNode NotifyOnSourceUpdated
        {
            get
            {
                return FindPropertyNode("NotifyOnSourceUpdated");
            }
        }

        public PropertyNode NotifyOnTargetUpdated
        {
            get
            {
                return FindPropertyNode("NotifyOnTargetUpdated");
            }
        }

        public PropertyNode NotifyOnValidationError
        {
            get
            {
                return FindPropertyNode("NotifyOnValidationError");
            }
        }

        public PropertyNode Path
        {
            get
            {
                return FindPropertyNode("Path");
            }
        }

        public PropertyNode RelativeSource
        {
            get
            {
                return FindPropertyNode("RelativeSource");
            }
        }

        public PropertyNode Source
        {
            get
            {
                return FindPropertyNode("Source");
            }
        }

        public PropertyNode UpdateSourceTrigger
        {
            get
            {
                return FindPropertyNode("UpdateSourceTrigger");
            }
        }

        public PropertyNode Delay
        {
            get
            {
                return FindPropertyNode("Delay");
            }
        }

        public PropertyNode ValidatesOnDataErrors
        {
            get
            {
                return FindPropertyNode("ValidatesOnDataErrors");
            }
        }

        public PropertyNode ValidatesOnExceptions
        {
            get
            {
                return FindPropertyNode("ValidatesOnExceptions");
            }
        }

        //public Collection<ValidationRule> ValidationRules { get; }

        public PropertyNode XPath
        {
            get
            {
                return FindPropertyNode("XPath");
            }
        }

        public PropertyNode TypeResolver
        {
            get
            {
                return FindPropertyNode("TypeResolver");
            }
        }

    }

    [CLSCompliant(false)]
    public class BindingEditorWrapperMulti : BindingEditorWrapperBase
    {
        public BindingEditorWrapperMulti(DesignItem designObjectBinding) 
            : base(designObjectBinding)
        {
            LoadBindingCollection();
        }

        protected virtual BindingEditorWrapperSingle CreateBindingWrapper(DesignItem bindingChild)
        {
            if (bindingChild.Component is Binding)
                return new BindingEditorWrapperSingle(bindingChild,this);
            return null;
        }

        protected DesignItemProperty BindingCollectionProp
        {
            get
            {
                return _DesignObjectBinding.Properties["Bindings"];
            }
        }

        protected void LoadBindingCollection()
        {
            foreach (DesignItem child in BindingCollectionProp.CollectionElements)
            {
                BindingEditorWrapperSingle wrapper = CreateBindingWrapper(child);
                if (wrapper != null)
                {
                    BindingsCollection.Add(wrapper);
                    wrapper.PropertyChanged += new PropertyChangedEventHandler(wrapper_PropertyChanged);
                }
            }
        }

        ObservableCollection<BindingEditorWrapperSingle> _BindingsCollection = new ObservableCollection<BindingEditorWrapperSingle>();
        public ObservableCollection<BindingEditorWrapperSingle> BindingsCollection
        {
            get { return _BindingsCollection; }
        }

        public virtual BindingEditorWrapperSingle AddNewBinding()
        {
            Binding newBinding = CreateNewBinding();
            DesignItem newBindingItem = _DesignObjectBinding.Services.Component.RegisterComponentForDesigner(newBinding);
            BindingCollectionProp.CollectionElements.Add(newBindingItem);
            BindingEditorWrapperSingle wrapper = CreateBindingWrapper(newBindingItem);
            if (wrapper != null)
            {
                BindingsCollection.Add(wrapper);
                wrapper.PropertyChanged += new PropertyChangedEventHandler(wrapper_PropertyChanged);
            }
            return wrapper;
        }

        public virtual Binding CreateNewBinding()
        {
            return new Binding();
        }

        public virtual void RemoveBinding(BindingEditorWrapperSingle childWrapper)
        {
            if (!BindingsCollection.Remove(childWrapper))
                return;
            childWrapper.PropertyChanged -= wrapper_PropertyChanged;
            BindingCollectionProp.CollectionElements.Remove(childWrapper.DesignObjectBinding);
        }

        void wrapper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Description")
            {
                RaisePropertyChanged(e.PropertyName);
            }
        }


        public override string Description
        {
            get
            {
                string result = "";
                foreach (BindingEditorWrapperSingle editor in BindingsCollection)
                {
                    result += editor.Description + ", ";
                }
                if (String.IsNullOrEmpty(result))
                    return base.Description;
                return result;
            }
        }

        private Control _DummyEditor = new Control() { Height = 2 };
        public Control ConverterEditor
        {
            get
            {
                if (Converter == null)
                    return null;
                if (Converter.Editor == null)
                    return null;
                else
                {
                    if (Converter.Editor.DataContext != Converter)
                        Converter.Editor.DataContext = Converter;
                    return Converter.Editor;
                }
            }
        }

        public PropertyNode Converter
        {
            get
            {
                return FindPropertyNode("Converter");
            }
        }

        public PropertyNode ConverterParameter
        {
            get
            {
                return FindPropertyNode("ConverterParameter");
            }
        }

        public PropertyNode Mode
        {
            get
            {
                return FindPropertyNode("Mode");
            }
        }

        public PropertyNode RelativeSource
        {
            get
            {
                return FindPropertyNode("RelativeSource");
            }
        }

        public PropertyNode NotifyOnSourceUpdated
        {
            get
            {
                return FindPropertyNode("NotifyOnSourceUpdated");
            }
        }

        public PropertyNode NotifyOnTargetUpdated
        {
            get
            {
                return FindPropertyNode("NotifyOnTargetUpdated");
            }
        }

        public PropertyNode NotifyOnValidationError
        {
            get
            {
                return FindPropertyNode("NotifyOnValidationError");
            }
        }

        public PropertyNode UpdateSourceTrigger
        {
            get
            {
                return FindPropertyNode("UpdateSourceTrigger");
            }
        }

        public PropertyNode ValidatesOnDataErrors
        {
            get
            {
                return FindPropertyNode("ValidatesOnDataErrors");
            }
        }

        public PropertyNode ValidatesOnExceptions
        {
            get
            {
                return FindPropertyNode("ValidatesOnExceptions");
            }
        }

    }
}
