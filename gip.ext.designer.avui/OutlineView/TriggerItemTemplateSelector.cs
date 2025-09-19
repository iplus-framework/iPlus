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
using Avalonia;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls;
using Avalonia.Controls.Templates;


namespace gip.ext.designer.avui.OutlineView
{
    public class TriggerItemTemplateSelector : IDataTemplate
    {
        DataTemplate dtmpltPropertyTrigger;
        DataTemplate dtmpltDataTrigger;
        DataTemplate dtmpltEventTrigger;
        DataTemplate dtmpltMultiTrigger;
        DataTemplate dtmpltMultiDataTrigger;

        public Control Build(object? item)
        {
            if (item == null)
                return null;

            DataTemplate template = SelectTemplate(item);
            return template.Build(item);
        }

        public bool Match(object data)
        {
            return data is PropertyTriggerOutlineNode ||
                   data is DataTriggerOutlineNode ||
                   data is EventTriggerOutlineNode ||
                   data is MultiTriggerOutlineNode ||
                   data is MultiDataTriggerOutlineNode;
        }

        private DataTemplate? SelectTemplate(object item)
        {
            // Try to get resource from Application if not already loaded
            if (dtmpltPropertyTrigger == null)
            {
                dtmpltPropertyTrigger = TryFindResource("dtmpltPropertyTrigger");
            }
            if (dtmpltDataTrigger == null)
            {
                dtmpltDataTrigger = TryFindResource("dtmpltDataTrigger");
            }
            if (dtmpltEventTrigger == null)
            {
                dtmpltEventTrigger = TryFindResource("dtmpltEventTrigger");
            }
            if (dtmpltMultiTrigger == null)
            {
                dtmpltMultiTrigger = TryFindResource("dtmpltMultiTrigger");
            }
            if (dtmpltMultiDataTrigger == null)
            {
                dtmpltMultiDataTrigger = TryFindResource("dtmpltMultiDataTrigger");
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

            return null;
        }

        private DataTemplate TryFindResource(string resourceKey)
        {
            if (Application.Current.TryFindResource(resourceKey, out var resource) == true)
            {
                return resource as DataTemplate;
            }
            return null;
        }
    }
}
