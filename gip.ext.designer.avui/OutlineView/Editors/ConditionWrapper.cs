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
    [CLSCompliant(false)]
    public class ConditionWrapper : INotifyPropertyChanged
    {
        protected DesignItem _DesignObjectCondition;

        public ConditionWrapper(DesignItem designObjectCondition, MultiTriggerNodeBase parentTriggerNode)
        {
            _DesignObjectCondition = designObjectCondition;
            _ParentTriggerNode = parentTriggerNode;
            LoadProperties();
        }

        public DesignItem DesignObjectCondition
        {
            get
            {
                return _DesignObjectCondition;
            }
        }

        protected MultiTriggerNodeBase _ParentTriggerNode;
        public MultiTriggerNodeBase ParentTriggerNode
        {
            get
            {
                return _ParentTriggerNode;
            }
        }


        public virtual string Description
        {
            get
            {
                string result = Value.ValueString;
                if (String.IsNullOrEmpty(result))
                    result = "Condition";
                return result;
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

            if (_DesignObjectCondition != null)
            {
                foreach (MemberDescriptor d in TypeHelper.GetAvailableProperties(_DesignObjectCondition.Component, true))
                {
                    list.Add(d.Name, d);
                }
            }
            return list.Values;
        }


        void AddNode(MemberDescriptor md)
        {
            DesignItemProperty[] designProperties = new DesignItemProperty[] { _DesignObjectCondition.Properties[md.Name] };
            PropertyNode node = CreatePropertyNode();
            node.Load(designProperties);
            if (node.Editor != null)
                node.Editor.DataContext = node;
            PropertiesOfCondition.Add(node);
            node.PropertyChanged += new PropertyChangedEventHandler(node_PropertyChanged);
        }

        protected virtual void node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if ((e.PropertyName == "Value") && ((sender == Path) || (sender == ElementName) || (sender == Source)))
            //{
            //    RaisePropertyChanged("Description");
            //}
        }

        ObservableCollection<PropertyNode> _PropertiesOfCondition = new ObservableCollection<PropertyNode>();
        public ObservableCollection<PropertyNode> PropertiesOfCondition
        {
            get { return _PropertiesOfCondition; }
        }
        #endregion


        public PropertyNode SourceName 
        {
            get
            {
                return PropertiesOfCondition.Where(c => c.Name == "SourceName").First();
            }
        }

        public PropertyNode Property 
        {
            get
            {
                return PropertiesOfCondition.Where(c => c.Name == "Property").First();
            }
        }

        public string PropertyName
        {
            get
            {
                return Property.ValueString;
            }
            set
            {
                Property.Value = value;
            }
        }

        public PropertyNode Binding 
        {
            get
            {
                return PropertiesOfCondition.Where(c => c.Name == "Binding").First();
            }
        }

        public PropertyNode Value 
        {
            get
            {
                return PropertiesOfCondition.Where(c => c.Name == "Value").First();
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


        private Control _DummyEditor = new Control() { Height = 2 };
        public FrameworkElement BindingEditor
        {
            get
            {
                if (DesignObjectCondition.Parent == null)
                    return null;
                if (Binding == null)
                    return null;
                else
                {
                    if (Binding.Editor.DataContext != Binding)
                        Binding.Editor.DataContext = Binding;
                    return Binding.Editor;
                }
            }
        }

    }

}
