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
    public class DataTriggerOutlineNode : TriggerOutlineNodeBase, IPropertyNode
    {
        //public static DataTriggerOutlineNode Create(DesignItem trigger, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(trigger, out node))
        //    {
        //        node = new DataTriggerOutlineNode(trigger, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as DataTriggerOutlineNode;
        //}

        //public static void Remove(DataTriggerOutlineNode node)
        //{
        //    outlineNodes.Remove(node.TriggerItem);
        //}

        public DataTriggerOutlineNode(DesignItem trigger, DesignItem designItem)
            : base(trigger, designItem)
        {
        }

        public DesignItemProperty BindingProperty
        {
            get
            {
                return TriggerItem.Properties["Binding"];
            }
        }

        public DesignItemProperty TriggerValue
        {
            get
            {
                return TriggerItem.Properties["Value"];
            }
        }

        public override bool IsSet
        {
            get
            {
                if (BindingProperty.IsSet && TriggerValue.IsSet)
                    return true;
                return false;
            }
        }

        public override string TriggerInfoText
        {
            get
            {
                string InfoText = "";
                if ((Editor != null) && (Editor is DataTriggerEditor))
                    InfoText = (Editor as DataTriggerEditor).TriggerInfoText;
                if (!String.IsNullOrEmpty(InfoText))
                {
                    return InfoText + " = " + TriggerValue.ValueOnInstance.ToString();
                }
                if (TriggerValue.ValueOnInstance != null)
                    return PropertyName + " = " + TriggerValue.ValueOnInstance.ToString();
                return PropertyName;
            }
        }

        public string PropertyName
        {
            get
            {
                if (BindingProperty.ValueOnInstance == null)
                    return "Binding";
                return BindingProperty.ValueOnInstance.ToString();
            }
        }

        public override FrameworkElement Editor
        {
            get
            {
                if (_Editor == null)
                {
                    _Editor = new DataTriggerEditor();
                    (_Editor as DataTriggerEditor).InitEditor(_DesignObject, this);
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
                if (result == DependencyProperty.UnsetValue) 
                    return null;
                if (TypeOfTriggerValue != null && result != null)
                {
                    if (TypeOfTriggerValue.IsEnum && (result is String))
                    {
                        try
                        {
                            result = Enum.Parse(TypeOfTriggerValue, result as String);
                        }
                        catch (Exception ec)
                        {
                            string msg = ec.Message;
                            if (ec.InnerException != null && ec.InnerException.Message != null)
                                msg += " Inner:" + ec.InnerException.Message;

                            if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null
                                                                           && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
                                core.datamodel.Database.Root.Messages.LogException("DataTriggerOutlineNode", "Value", msg);
                        }
                    }
                }
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
                    if (hasStringConverter)
                    {
                        try
                        {
                            return Value.ToString();
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
                // make sure we only catch specific exceptions
                // and/or show the error message to the user
                //try {
                Value = value;
                //} catch {
                //	OnValueOnInstanceChanged();
                //}

            }
        }

        protected Type TypeOfTriggerValue
        {
            get;
            set;
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
