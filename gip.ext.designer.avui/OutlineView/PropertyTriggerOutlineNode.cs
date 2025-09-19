// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.ext.design.avui;
using System.Collections.ObjectModel;
using System.Collections;
using gip.ext.designer.avui;
using gip.ext.xamldom.avui;
using gip.ext.design.avui.PropertyGrid;
using System.Reflection;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace gip.ext.designer.avui.OutlineView
{
    [CLSCompliant(false)]
    public class PropertyTriggerOutlineNode : TriggerOutlineNodeBase, IPropertyNode
    {
        //public static PropertyTriggerOutlineNode Create(DesignItem trigger, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(trigger, out node))
        //    {
        //        node = new PropertyTriggerOutlineNode(trigger, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as PropertyTriggerOutlineNode;
        //}

        //public static void Remove(PropertyTriggerOutlineNode node)
        //{
        //    outlineNodes.Remove(node.TriggerItem);
        //}

        public PropertyTriggerOutlineNode(DesignItem trigger, DesignItem designItem)
            : base(trigger, designItem)
        {
        }

        public DesignItemProperty TriggerProperty
        {
            get
            {
                return TriggerItem.Properties["Property"];
            }
        }

        public DesignItemProperty TriggerValue
        {
            get
            {
                return TriggerItem.Properties["Value"];
            }
        }

        private PropertyDescriptor _TargetPropertyInfo;
        public PropertyDescriptor TriggerTargetPropertyInfo
        {
            get
            {
                if (_TargetPropertyInfo != null)
                    return _TargetPropertyInfo;
                if (!String.IsNullOrEmpty(TriggerTargetPropertyName))
                {
                    if (_DesignObject.View != null)
                    {
                        try
                        {
                            PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(_DesignObject.View);
                            _TargetPropertyInfo = propertyDescriptors[TriggerTargetPropertyName];
                        }
                        catch (Exception ec)
                        {
                            string msg = ec.Message;
                            if (ec.InnerException != null && ec.InnerException.Message != null)
                                msg += " Inner:" + ec.InnerException.Message;

                            if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null 
                                                                           && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                                core.datamodel.Database.Root.Messages.LogException("PropertyTriggerOutlineNode", "TriggerTargetPropertyInfo", msg);
                        }
                    }
                }
                return _TargetPropertyInfo;
            }
        }

        public DesignItemProperty TriggerTargetProperty
        {
            get
            {
                if (String.IsNullOrEmpty(TriggerTargetPropertyName))
                    return null;
                return _DesignObject.Properties[TriggerTargetPropertyName];
            }
        }

        public override string TriggerInfoText
        {
            get
            {
                if (TriggerTargetProperty != null)
                {
                    if (TriggerValue.ValueOnInstance != null)
                        return TriggerTargetProperty.Name + " = " + TriggerValue.ValueOnInstance.ToString();
                    return TriggerTargetProperty.Name;
                }
                return "Property-Trigger";
            }
        }

        public string TriggerTargetPropertyName
        {
            get
            {
                if (TriggerProperty.ValueOnInstance == null)
                    return "";
                return TriggerProperty.ValueOnInstance.ToString();
            }
            set
            {
                if (!IsEnabled)
                    return;
                TriggerProperty.SetValue(value);
                RaisePropertyChanged("TriggerTargetPropertyName");
                RaisePropertyChanged("TriggerTargetPropertyInfo");
                RaisePropertyChanged("TriggerTargetProperty");
                OnValueChanged();
            }
        }

        public override bool IsSet 
        {
            get
            {
                if (TriggerProperty.IsSet && TriggerValue.IsSet)
                    return true;
                return false;
            }
        }

        public override Control Editor 
        {
            get
            {
                if (_Editor == null)
                {
                    _Editor = new PropertyTriggerEditor();
                    (_Editor as PropertyTriggerEditor).InitEditor(_DesignObject, this);
                }
                return _Editor;
            }

            protected set
            {
                _Editor = value;
            }
        }


        public override object Description
        {
            get
            {
                IPropertyDescriptionService s = DesignItem.Services.GetService<IPropertyDescriptionService>();
                if (s != null)
                {
                    return s.GetDescription(TriggerTargetProperty);
                }
                return null;
            }
        }


        public override ReadOnlyCollection<DesignItemProperty> Properties
        {
            get 
            {
                if (_Properties == null)
                {
                    List<DesignItemProperty> list = new List<DesignItemProperty>();
                    if (TriggerTargetProperty != null)
                        list.Add(TriggerTargetProperty);
                    _Properties = new ReadOnlyCollection<DesignItemProperty>(list);
                }
                return _Properties;
            }
        }

        public override bool IsDependencyProperty
        {
            get 
            {
                if (TriggerTargetPropertyInfo == null)
                    return false;
                MethodInfo getMethod = TriggerTargetPropertyInfo.PropertyType.GetMethod("Get" + TriggerTargetPropertyName, BindingFlags.Public | BindingFlags.Static);
                MethodInfo setMethod = TriggerTargetPropertyInfo.PropertyType.GetMethod("Set" + TriggerTargetPropertyName, BindingFlags.Public | BindingFlags.Static);
                if (getMethod != null && setMethod != null)
                {
                    FieldInfo field = TriggerTargetPropertyInfo.PropertyType.GetField(TriggerTargetPropertyName + "Property", BindingFlags.Public | BindingFlags.Static);
                    if (field != null && field.FieldType == typeof(AvaloniaProperty))
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        public override DesignItemProperty FirstProperty
        {
            get 
            {
                return TriggerValue;
            }
        }


        public override object Value
        {
            get
            {
                if (TriggerValue == null)
                    return null;
                //if (IsAmbiguous) 
                    //return null;
                var result = TriggerValue.ValueOnInstance;
                if (result == AvaloniaProperty.UnsetValue) 
                    return null;
                return result;
            }
            set
            {
                SetValueCore(value);
            }
        }

        public override string ValueString
        {
            get
            {
                if (ValueItem == null || ValueItem.Component is MarkupExtension)
                {
                    if (Value == null) 
                        return null;
                    if (TriggerTargetProperty == null)
                        return null;
                    if (hasStringConverter)
                    {
                        try
                        {
                            return TriggerTargetProperty.TypeConverter.ConvertToInvariantString(Value);
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
                if (TriggerTargetProperty == null)
                    return;

                // make sure we only catch specific exceptions
                // and/or show the error message to the user
                //try {
                Value = TriggerTargetProperty.TypeConverter.ConvertFromInvariantString(value);
                //} catch {
                //	OnValueOnInstanceChanged();
                //}

            }
        }


        public override DesignItem ValueItem
        {
            get
            {
                return TriggerValue.Value;
            }

        }

        protected override void SetValueCore(object value)
        {
            raiseEvents = false;
            if (value == Unset)
            {
                if (TriggerValue != null)
                    TriggerValue.Reset();
            }
            else
            {
                if (TriggerValue != null)
                {
                    TriggerValue.SetValue(value);
                }
            }
            raiseEvents = true;
            OnValueChanged();
        }

    }
}
