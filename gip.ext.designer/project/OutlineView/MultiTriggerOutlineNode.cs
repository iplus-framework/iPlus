// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

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
    [CLSCompliant(false)]
    public abstract class MultiTriggerNodeBase : TriggerOutlineNodeBase
    {
        public MultiTriggerNodeBase(DesignItem trigger, DesignItem designItem)
            : base(trigger, designItem)
        {
        }

        public override FrameworkElement Editor
        {
            get
            {
                if (_Editor == null)
                {
                    _Editor = new MultiTriggerEditor();
                    (_Editor as MultiTriggerEditor).InitEditor(_DesignObject, this);
                }
                return _Editor;
            }

            protected set
            {
                _Editor = value;
            }
        }

        public DesignItemProperty ConditionsProperty
        {
            get
            {
                return TriggerItem.Properties["Conditions"];
            }
        }

    }

    [CLSCompliant(false)]
    public class MultiTriggerOutlineNode : MultiTriggerNodeBase, IPropertyNode
    {
        //public static MultiTriggerOutlineNode Create(DesignItem trigger, DesignItem designItem)
        //{
        //    OutlineNode node;
        //    if (!outlineNodes.TryGetValue(trigger, out node))
        //    {
        //        node = new MultiTriggerOutlineNode(trigger, designItem);
        //        outlineNodes[designItem] = node;
        //    }
        //    return node as MultiTriggerOutlineNode;
        //}

        //public static void Remove(MultiTriggerOutlineNode node)
        //{
        //    outlineNodes.Remove(node.TriggerItem);
        //}

        public MultiTriggerOutlineNode(DesignItem trigger, DesignItem designItem)
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
                    propName = "MultiTrigger";

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
