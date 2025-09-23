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
using Avalonia.Metadata;


namespace gip.ext.designer.avui.OutlineView
{
    public class TriggerItemTemplateSelector : IDataTemplate
    {
        [Content]
        public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

        public Control Build(object param)
        {
            var key = param?.ToString(); // Our Keys in the dictionary are strings, so we call .ToString() to get the key to look up
            if (key is null) // If the key is null, we throw an ArgumentNullException
            {
                throw new ArgumentNullException(nameof(param));
            }
            return AvailableTemplates[key].Build(param);
        }

        public bool Match(object data)
        {
            // Our Keys in the dictionary are strings, so we call .ToString() to get the key to look up
            var key = data?.ToString();

            return !string.IsNullOrEmpty(key)           // and the key must not be null or empty
                    && AvailableTemplates.ContainsKey(key); // and the key must be found in our Dictionary
        }
    }
}
