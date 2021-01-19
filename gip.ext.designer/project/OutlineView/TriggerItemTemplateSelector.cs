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
    public class TriggerItemTemplateSelector : DataTemplateSelector
    {
        DataTemplate dtmpltPropertyTrigger;
        DataTemplate dtmpltDataTrigger;
        DataTemplate dtmpltEventTrigger;
        DataTemplate dtmpltMultiTrigger;
        DataTemplate dtmpltMultiDataTrigger;
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (dtmpltPropertyTrigger == null)
            {
                FrameworkElement element = container as FrameworkElement;
                if (element != null)
                {
                    dtmpltPropertyTrigger = element.FindResource("dtmpltPropertyTrigger") as DataTemplate;
                }
            }
            if (dtmpltDataTrigger == null)
            {
                FrameworkElement element = container as FrameworkElement;
                if (element != null)
                {
                    dtmpltDataTrigger = element.FindResource("dtmpltDataTrigger") as DataTemplate;
                }
            }
            if (dtmpltEventTrigger == null)
            {
                FrameworkElement element = container as FrameworkElement;
                if (element != null)
                {
                    dtmpltEventTrigger = element.FindResource("dtmpltEventTrigger") as DataTemplate;
                }
            }
            if (dtmpltMultiTrigger == null)
            {
                FrameworkElement element = container as FrameworkElement;
                if (element != null)
                {
                    dtmpltMultiTrigger = element.FindResource("dtmpltMultiTrigger") as DataTemplate;
                }
            }
            if (dtmpltMultiDataTrigger == null)
            {
                FrameworkElement element = container as FrameworkElement;
                if (element != null)
                {
                    dtmpltMultiDataTrigger = element.FindResource("dtmpltMultiDataTrigger") as DataTemplate;
                }
            }

            if (item != null)
            {
                if (item is PropertyTriggerOutlineNode)
                    return dtmpltPropertyTrigger;
                else if (item is DataTriggerOutlineNode)
                    return dtmpltDataTrigger;
                else if (item is EventTriggerOutlineNode)
                    return dtmpltEventTrigger;
                else if (item is MultiTriggerOutlineNode)
                    return dtmpltMultiTrigger;
                else if (item is MultiDataTriggerOutlineNode)
                    return dtmpltMultiDataTrigger;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
