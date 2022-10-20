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
using System.Windows.Markup;

namespace gip.ext.designer.OutlineView
{
    [CLSCompliant(false)]
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
        #endregion


        public PropertyNode BindingGroupName 
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "BindingGroupName").First();
            }
        }

        public PropertyNode FallbackValue 
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "FallbackValue").First();
            }
        }

        public PropertyNode StringFormat 
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "StringFormat").First();
            }
        }

        public PropertyNode TargetNullValue 
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "TargetNullValue").First();
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
                string result = Path.ValueString + ElementName.ValueString + Source.ValueString;
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
        public FrameworkElement ConverterEditor
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
                return PropertiesOfBinding.Where(c => c.Name == "Converter").First();
            }
        }

        public PropertyNode ConverterParameter
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "ConverterParameter").First();
            }
        }

        public PropertyNode ElementName
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "ElementName").First();
            }
        }

        public PropertyNode IsAsync
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "IsAsync").First();
            }
        }

        public PropertyNode Mode
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "Mode").First();
            }
        }

        public PropertyNode NotifyOnSourceUpdated
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "NotifyOnSourceUpdated").First();
            }
        }

        public PropertyNode NotifyOnTargetUpdated
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "NotifyOnTargetUpdated").First();
            }
        }

        public PropertyNode NotifyOnValidationError
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "NotifyOnValidationError").First();
            }
        }

        public PropertyNode Path
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "Path").First();
            }
        }

        public PropertyNode RelativeSource
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "RelativeSource").First();
            }
        }

        public PropertyNode Source
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "Source").First();
            }
        }

        public PropertyNode UpdateSourceTrigger
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "UpdateSourceTrigger").First();
            }
        }

        public PropertyNode ValidatesOnDataErrors
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "ValidatesOnDataErrors").First();
            }
        }

        public PropertyNode ValidatesOnExceptions
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "ValidatesOnExceptions").First();
            }
        }

        //public Collection<ValidationRule> ValidationRules { get; }

        public PropertyNode XPath
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "XPath").First();
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
            MarkupExtension newBinding = CreateNewBinding();
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

        public virtual MarkupExtension CreateNewBinding()
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
        public FrameworkElement ConverterEditor
        {
            get
            {
                if (Converter == null)
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
                return PropertiesOfBinding.Where(c => c.Name == "Converter").First();
            }
        }

        public PropertyNode ConverterParameter
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "ConverterParameter").First();
            }
        }

        public PropertyNode Mode
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "Mode").First();
            }
        }

        public PropertyNode NotifyOnSourceUpdated
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "NotifyOnSourceUpdated").First();
            }
        }

        public PropertyNode NotifyOnTargetUpdated
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "NotifyOnTargetUpdated").First();
            }
        }

        public PropertyNode NotifyOnValidationError
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "NotifyOnValidationError").First();
            }
        }

        public PropertyNode UpdateSourceTrigger
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "UpdateSourceTrigger").First();
            }
        }

        public PropertyNode ValidatesOnDataErrors
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "ValidatesOnDataErrors").First();
            }
        }

        public PropertyNode ValidatesOnExceptions
        {
            get
            {
                return PropertiesOfBinding.Where(c => c.Name == "ValidatesOnExceptions").First();
            }
        }

    }
}
