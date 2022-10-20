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
    public class ActionOutlineNode : OutlineNodeBase, IPropertyNode
    {
        //public static ActionOutlineNode Create(DesignItem setter, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(setter, out node))
        //    {
        //        node = new ActionOutlineNode(setter, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as ActionOutlineNode;
        //}

        //public static void Remove(ActionOutlineNode node)
        //{
        //    outlineNodes.Remove(node.SetterItem);
        //}


        public ActionOutlineNode(DesignItem actionItem, DesignItem triggerItem)
            : base(actionItem)
        {
            _DesignObject = triggerItem;
            _PropertyNodeChildren = new ObservableCollection<PropertyNode>();
            _PropertyNodeMoreChildren = new ObservableCollection<PropertyNode>();
            Load();
        }

        public void Load()
        {
            //if (_Properties != null)
            //{
            //    // detach events from old properties
            //    foreach (var property in _Properties)
            //    {
            //        property.ValueChanged -= new EventHandler(property_ValueChanged);
            //        property.ValueOnInstanceChanged -= new EventHandler(property_ValueOnInstanceChanged);
            //    }
            //}

            //_Properties = null;

            //foreach (var property in Properties)
            //{
            //    property.ValueChanged += new EventHandler(property_ValueChanged);
            //    property.ValueOnInstanceChanged += new EventHandler(property_ValueOnInstanceChanged);
            //}

            //try
            //{
            //    hasStringConverter =
            //        FirstProperty.TypeConverter.CanConvertFrom(typeof(string)) &&
            //        FirstProperty.TypeConverter.CanConvertTo(typeof(string));
            //    OnValueChanged();
            //}
            //catch (Exception)
            //{
            //}
        }

        protected static Dictionary<DesignItem, ActionOutlineNode> outlineNodes = new Dictionary<DesignItem, ActionOutlineNode>();

        public static ActionOutlineNode Create(DesignItem designItem)
        {
            ActionOutlineNode node;
            if (!outlineNodes.TryGetValue(designItem, out node))
            {
                node = new ActionOutlineNode(designItem,null);
                outlineNodes[designItem] = node;
            }
            return node;
        }

        protected override OutlineNodeBase OnCreateChildrenNode(DesignItem child)
        {
            return ActionOutlineNode.Create(child);
        }



        private DesignItem _DesignObject;

        public DesignItem ActionItem
        {
            get
            {
                return this.DesignItem;
            }
        }

        public object Description
        {
            get
            {
                return ActionItem.ComponentType.Name + "(" + ActionItem.Name + ")";
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
                return null;
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
                //if (SetterValue == null)
                    return null;
                //if (IsAmbiguous) 
                    //return null;
                //var result = SetterValue.ValueOnInstance;
                //if (result == DependencyProperty.UnsetValue) 
                //    return null;
                //return result;
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
                    //if (SetterTargetProperty == null)
                    //    return null;
                    //if (hasStringConverter)
                    //{
                    //    try
                    //    {
                    //        return SetterTargetProperty.TypeConverter.ConvertToInvariantString(Value);
                    //    }
                    //    catch (Exception /*e*/)
                    //    {
                    //        return "(" + Value.GetType().Name + ")";
                    //    }
                    //}
                    return "(" + Value.GetType().Name + ")";
                }
                return "(" + ValueItem.ComponentType.Name + ")";
            }
            set
            {
                //if (SetterTargetProperty == null)
                    return;

                // make sure we only catch specific exceptions
                // and/or show the error message to the user
                //try {
                //Value = SetterTargetProperty.TypeConverter.ConvertFromInvariantString(value);
                //} catch {
                //	OnValueOnInstanceChanged();
                //}

            }
        }

        bool hasStringConverter = false;
        public bool IsEnabled
        {
            get { return ValueItem == null && hasStringConverter; }
        }

        public bool IsSet
        {
            get
            {
                //if (SetterProperty.IsSet && SetterValue.IsSet)
                //    return true;
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
                return ActionItem;
            }

        }

        public bool IsAmbiguous
        {
            get
            {
                //foreach (var p in Properties)
                //{
                //    if (!object.Equals(p.ValueOnInstance, FirstProperty.ValueOnInstance))
                //    {
                //        return true;
                //    }
                //}
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
            //if (value == Unset)
            //{
            //    if (SetterValue != null)
            //        SetterValue.Reset();
            //}
            //else
            //{
            //    if (SetterValue != null)
            //    {
            //        SetterValue.SetValue(value);
            //        if (SetterTargetProperty != null)
            //        {
            //            SetterTargetProperty.SetValueOnInstance(value);
            //        }
            //    }
            //}
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
