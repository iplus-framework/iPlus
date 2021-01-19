// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.ext.design;
using System.Collections.ObjectModel;
using System.Collections;
using gip.ext.designer;
using gip.ext.xamldom;
using gip.ext.design.PropertyGrid;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Markup;

namespace gip.ext.designer.OutlineView
{
    public class SetterOutlineNode : OutlineNodeBase, IPropertyNode
    {
        //public static SetterOutlineNode Create(DesignItem setter, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(setter, out node))
        //    {
        //        node = new SetterOutlineNode(setter, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as SetterOutlineNode;
        //}

        //public static void Remove(SetterOutlineNode node)
        //{
        //    outlineNodes.Remove(node.SetterItem);
        //}


        public SetterOutlineNode(DesignItem setter, DesignItem designItem)
            : base(setter)
        {
            _DesignObject = designItem;
            _PropertyNodeChildren = new ObservableCollection<PropertyNode>();
            _PropertyNodeMoreChildren = new ObservableCollection<PropertyNode>();

            if (SetterTargetPropertyInfo != null)
                Editor = EditorManager.CreateEditor(SetterTargetPropertyInfo.PropertyType);
            if (Editor == null)
                Editor = EditorManager.CreateEditor(SetterItem.Properties["Value"]);

            Load();
        }

        //protected static Dictionary<DesignItem, SetterOutlineNode> outlineNodes = new Dictionary<DesignItem, SetterOutlineNode>();

        public static SetterOutlineNode Create(DesignItem designItem)
        {
            return null;
        }

        protected override OutlineNodeBase OnCreateChildrenNode(DesignItem child)
        {
            return SetterOutlineNode.Create(child);
        }


        public void Load()
        {
            if (_Properties != null)
            {
                // detach events from old properties
                foreach (var property in _Properties)
                {
                    property.ValueChanged -= new EventHandler(property_ValueChanged);
                    property.ValueOnInstanceChanged -= new EventHandler(property_ValueOnInstanceChanged);
                }
            }

            _Properties = null;

            foreach (var property in Properties)
            {
                property.ValueChanged += new EventHandler(property_ValueChanged);
                property.ValueOnInstanceChanged += new EventHandler(property_ValueOnInstanceChanged);
            }

            try
            {
                hasStringConverter =
                    FirstProperty.TypeConverter.CanConvertFrom(typeof(string)) &&
                    FirstProperty.TypeConverter.CanConvertTo(typeof(string));
                OnValueChanged();
            }
            catch (Exception ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null
                                                               && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                    core.datamodel.Database.Root.Messages.LogException("SetterOutlineNode", "Load", msg);
            }
        }


        private DesignItem _DesignObject;

        public DesignItem SetterItem
        {
            get
            {
                return this.DesignItem;
            }
        }

        public DesignItemProperty SetterProperty
        {
            get
            {
                return SetterItem.Properties["Property"];
            }
        }

        public DesignItemProperty SetterValue
        {
            get
            {
                return SetterItem.Properties["Value"];
            }
        }

        private PropertyDescriptor _TargetPropertyInfo;
        public PropertyDescriptor SetterTargetPropertyInfo
        {
            get
            {
                if (_TargetPropertyInfo != null)
                    return _TargetPropertyInfo;
                if (!String.IsNullOrEmpty(SetterTargetPropertyName))
                {
                    if (_DesignObject.View != null)
                    {
                        try
                        {
                            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(_DesignObject.View);
                            _TargetPropertyInfo = propertyDescriptors[SetterTargetPropertyName];
                        }
                        catch (Exception ec)
                        {
                            string msg = ec.Message;
                            if (ec.InnerException != null && ec.InnerException.Message != null)
                                msg += " Inner:" + ec.InnerException.Message;

                            if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null
                                                                           && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                                core.datamodel.Database.Root.Messages.LogException("SetterOutlineNode", "SetterTargetPropertyInfo", msg);
                        }
                    }
                }
                return _TargetPropertyInfo;
            }
        }

        public DesignItemProperty SetterTargetProperty
        {
            get
            {
                if (String.IsNullOrEmpty(SetterTargetPropertyName))
                    return null;
                return _DesignObject.Properties[SetterTargetPropertyName];
            }
        }

        public string SetterTargetPropertyName
        {
            get
            {
                if (SetterProperty.ValueOnInstance == null)
                    return null;
                return SetterProperty.ValueOnInstance.ToString();
            }
        }

        public object Description
        {
            get
            {
                IPropertyDescriptionService s = DesignItem.Services.GetService<IPropertyDescriptionService>();
                if (s != null)
                {
                    return s.GetDescription(SetterTargetProperty);
                }
                return null;
            }
        }


        public FrameworkElement Editor { get; private set; }

        private ReadOnlyCollection<DesignItemProperty> _Properties;
        public ReadOnlyCollection<DesignItemProperty> Properties
        {
            get 
            {
                if (_Properties == null)
                {
                    List<DesignItemProperty> list = new List<DesignItemProperty>();
                    if (SetterTargetProperty != null)
                        list.Add(SetterTargetProperty);
                    _Properties = new ReadOnlyCollection<DesignItemProperty>(list);
                }
                return _Properties;
            }
        }

        public bool IsEvent
        {
            get 
            {
                return false;
            }
        }

        public bool IsDependencyProperty
        {
            get 
            {
                if (SetterTargetPropertyInfo == null)
                    return false;
                MethodInfo getMethod = SetterTargetPropertyInfo.PropertyType.GetMethod("Get" + SetterTargetPropertyName, BindingFlags.Public | BindingFlags.Static);
                MethodInfo setMethod = SetterTargetPropertyInfo.PropertyType.GetMethod("Set" + SetterTargetPropertyName, BindingFlags.Public | BindingFlags.Static);
                if (getMethod != null && setMethod != null)
                {
                    FieldInfo field = SetterTargetPropertyInfo.PropertyType.GetField(SetterTargetPropertyName + "Property", BindingFlags.Public | BindingFlags.Static);
                    if (field != null && field.FieldType == typeof(DependencyProperty))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public DesignContext Context
        {
            get 
            {
                return this.DesignItem.Context;
            }
        }

        public ServiceContainer Services
        {
            get 
            {
                return this.DesignItem.Services;
            }
        }

        public DesignItemProperty FirstProperty
        {
            get 
            {
                return SetterTargetProperty;
            }
        }

        public new PropertyNode Parent
        {
            get 
            {
                return null;
            }
        }

        public int Level
        {
            get { return 0; }
        }

        public Category Category
        {
            get; set;
        }

        ObservableCollection<PropertyNode> _PropertyNodeChildren;
        /// <summary>
        /// Gets the list of child nodes.
        /// </summary>
        ObservableCollection<PropertyNode> IPropertyNode.Children 
        {
            get
            {
                return _PropertyNodeChildren;
            }
        }

        ObservableCollection<PropertyNode> _PropertyNodeMoreChildren;
        /// <summary>
        /// Gets the list of advanced child nodes (not visible by default).
        /// </summary>
        ObservableCollection<PropertyNode> IPropertyNode.MoreChildren 
        {
            get
            {
                return _PropertyNodeMoreChildren;
            }
        }

        public bool HasChildren
        {
            get
            {
                return (this as IPropertyNode).Children.Count > 0 || (this as IPropertyNode).MoreChildren.Count > 0;
            }
        }

        public object Value
        {
            get
            {
                if (SetterValue == null)
                    return null;
                //if (IsAmbiguous) 
                    //return null;
                var result = SetterValue.ValueOnInstance;
                if (result == DependencyProperty.UnsetValue) 
                    return null;
                return result;
            }
            set
            {
                SetValueCore(value);
            }
        }

        public string ValueString
        {
            get
            {
                if (ValueItem == null || ValueItem.Component is MarkupExtension)
                {
                    if (Value == null) 
                        return null;
                    if (SetterTargetProperty == null)
                        return null;
                    if (hasStringConverter)
                    {
                        try
                        {
                            return SetterTargetProperty.TypeConverter.ConvertToInvariantString(Value);
                        }
                        catch (Exception /*e*/)
                        {
                            return "(" + Value.GetType().Name + ")";
                        }
                    }
                    return "(" + Value.GetType().Name + ")";
                }
                return "(" + ValueItem.ComponentType.Name + ")";
            }
            set
            {
                if (SetterTargetProperty == null)
                    return;

                // make sure we only catch specific exceptions
                // and/or show the error message to the user
                //try {
                Value = SetterTargetProperty.TypeConverter.ConvertFromInvariantString(value);
                //} catch {
                //	OnValueOnInstanceChanged();
                //}

            }
        }

        bool hasStringConverter;
        public bool IsEnabled
        {
            get { return ValueItem == null && hasStringConverter; }
        }

        public bool IsSet
        {
            get
            {
                if (SetterProperty.IsSet && SetterValue.IsSet)
                    return true;
                return false;
            }
        }

        public Brush NameForeground
        {
			get {
				if (ValueItem != null) {
					object component = ValueItem.Component;
					if (component is BindingBase)
						return Brushes.DarkGoldenrod;
					if (component is StaticResourceExtension || component is DynamicResourceExtension)
						return Brushes.DarkGreen;
				}
				return SystemColors.WindowTextBrush;
			}
        }

        public DesignItem ValueItem
        {
            get
            {
                return SetterValue.Value;
            }

        }

        public bool IsAmbiguous
        {
            get
            {
                foreach (var p in Properties)
                {
                    if (!object.Equals(p.ValueOnInstance, FirstProperty.ValueOnInstance))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        bool isVisible;
        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                isVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }

        public bool CanReset
        {
            get { return IsSet; }
        }

        static object Unset = new object();
        public void Reset()
        {
            SetValueCore(Unset);
        }

        public void CreateBindings()
        {
        }

        public void CreateMultiBindings()
        {
        }

        bool raiseEvents = true;
        void SetValueCore(object value)
        {
            raiseEvents = false;
            if (value == Unset)
            {
                if (SetterValue != null)
                    SetterValue.Reset();
            }
            else
            {
                if (SetterValue != null)
                {
                    SetterValue.SetValue(value);
                    if (SetterTargetProperty != null)
                    {
                        SetterTargetProperty.SetValueOnInstance(value);
                    }
                }
            }
            raiseEvents = true;
            OnValueChanged();
        }

        void OnValueChanged()
        {
            RaisePropertyChanged("IsSet");
            RaisePropertyChanged("Value");
            RaisePropertyChanged("ValueString");
            RaisePropertyChanged("IsAmbiguous");
            RaisePropertyChanged("FontWeight");
            RaisePropertyChanged("IsEnabled");
            RaisePropertyChanged("NameForeground");
        }

        void OnValueOnInstanceChanged()
        {
            RaisePropertyChanged("Value");
            RaisePropertyChanged("ValueString");
        }

        void property_ValueOnInstanceChanged(object sender, EventArgs e)
        {
            if (raiseEvents) OnValueOnInstanceChanged();
        }

        void property_ValueChanged(object sender, EventArgs e)
        {
            if (raiseEvents) OnValueChanged();
        }
    }
}
