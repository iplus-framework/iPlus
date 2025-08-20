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
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Markup;

namespace gip.ext.designer.avui.OutlineView
{
    [CLSCompliant(false)]
    public class EventTriggerOutlineNode : TriggerOutlineNodeBase, IPropertyNode
    {
        //public static EventTriggerOutlineNode Create(DesignItem trigger, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(trigger, out node))
        //    {
        //        node = new EventTriggerOutlineNode(trigger, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as EventTriggerOutlineNode;
        //}

        //public static void Remove(EventTriggerOutlineNode node)
        //{
        //    outlineNodes.Remove(node.TriggerItem);
        //}


        public EventTriggerOutlineNode(DesignItem trigger, DesignItem designItem)
            : base(trigger, designItem)
        {
        }

        public DesignItemProperty RoutedEventProperty
        {
            get
            {
                return TriggerItem.Properties["RoutedEvent"];
            }
        }

        public string RoutedEventName
        {
            get
            {
                if (RoutedEventProperty.ValueOnInstance == null)
                    return "";
                return RoutedEventProperty.ValueOnInstance.ToString();
            }
            set
            {
                if (!IsEnabled)
                    return;
                RoutedEventProperty.SetValue(value);
                RaisePropertyChanged("RoutedEventName");
                RaisePropertyChanged("RoutedEventProperty");
                OnValueChanged();
            }
        }


        public override bool IsSet
        {
            get
            {
                if (RoutedEventProperty.IsSet)
                    return true;
                return false;
            }
        }


        public override string TriggerInfoText
        {
            get
            {
                return PropertyName;
            }
        }

        public string PropertyName
        {
            get
            {
                if (RoutedEventProperty.ValueOnInstance == null)
                    return null;
                return RoutedEventProperty.ValueOnInstance.ToString();
            }
        }

        public override object Description
        {
            get
            {
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
                    _Properties = new ReadOnlyCollection<DesignItemProperty>(list);
                }
                return _Properties;
            }
        }

        public override bool IsDependencyProperty
        {
            get 
            {
                return false;
            }
        }


        public override DesignItemProperty FirstProperty
        {
            get 
            {
                return RoutedEventProperty;
            }
        }


        public override object Value
        {
            get
            {
                if (RoutedEventProperty == null)
                    return null;
                //if (IsAmbiguous) 
                    //return null;
                var result = RoutedEventProperty.ValueOnInstance;
                if (result == DependencyProperty.UnsetValue) 
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
                    if (RoutedEventProperty == null)
                        return null;
                    if (hasStringConverter)
                    {
                        try
                        {
                            return RoutedEventProperty.TypeConverter.ConvertToInvariantString(Value);
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
                if (RoutedEventProperty == null)
                    return;

                // make sure we only catch specific exceptions
                // and/or show the error message to the user
                //try {
                Value = RoutedEventProperty.TypeConverter.ConvertFromInvariantString(value);
                //} catch {
                //	OnValueOnInstanceChanged();
                //}

            }
        }


        public override DesignItem ValueItem
        {
            get
            {
                return null;
            }

        }

        protected override void SetValueCore(object value)
        {
            raiseEvents = false;
            if (value == Unset)
            {
                if (RoutedEventProperty != null)
                    RoutedEventProperty.Reset();
            }
            else
            {
                if (RoutedEventProperty != null)
                {
                    RoutedEventProperty.SetValue(value);
                }
            }
            raiseEvents = true;
            OnValueChanged();
        }

        public override FrameworkElement Editor
        {
            get
            {
                if (_Editor == null)
                {
                    _Editor = new EventTriggerEditor();
                    (_Editor as EventTriggerEditor).InitEditor(_DesignObject, this);
                }
                return _Editor;
            }

            protected set
            {
                _Editor = value;
            }
        }

    }
}
