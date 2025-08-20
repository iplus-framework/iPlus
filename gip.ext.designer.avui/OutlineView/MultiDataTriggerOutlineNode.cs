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
    public class MultiDataTriggerOutlineNode : MultiTriggerNodeBase, IPropertyNode
    {
        //public static MultiDataTriggerOutlineNode Create(DesignItem trigger, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(trigger, out node))
        //    {
        //        node = new MultiDataTriggerOutlineNode(trigger, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as MultiDataTriggerOutlineNode;
        //}

        //public static void Remove(MultiDataTriggerOutlineNode node)
        //{
        //    outlineNodes.Remove(node.TriggerItem);
        //}


        public MultiDataTriggerOutlineNode(DesignItem trigger, DesignItem designItem)
            : base(trigger, designItem)
        {
        }

        public override string TriggerInfoText
        {
            get
            {
                return PropertyName;
            }
        }

        public override bool IsSet
        {
            get
            {
                return true;
            }
        }

        public string PropertyName
        {
            get
            {
                string propName = "";
                DesignItemProperty conditions = TriggerItem.Properties["Conditions"];
                if (conditions.ValueOnInstance != null)
                {
                    foreach (DesignItem condition in conditions.CollectionElements)
                    {
                        if (condition.Properties["Property"].ValueOnInstance != null)
                        {
                            propName += condition.Properties["Property"].ValueOnInstance.ToString() + ", ";
                        }
                    }
                }
                if (String.IsNullOrEmpty(propName))
                    propName = "MultiDataTrigger";

                return propName;
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
                return null;
            }
        }


        public override object Value
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public override string ValueString
        {
            get
            {
                return "";
            }
            set
            {
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
            OnValueChanged();
        }

    }
}
