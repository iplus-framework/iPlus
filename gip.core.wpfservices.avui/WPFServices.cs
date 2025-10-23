// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.wpfservices.avui.Manager;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace gip.core.wpfservices.avui
{
    public class WPFServices : IWPFServices
    {
        VBDesignerService _VBDesignerService = new VBDesignerService();
        public virtual IVBDesignerService DesignerService { get { return _VBDesignerService; } }

        MediaControllerService _VBMediaControllerService = new MediaControllerService();
        public virtual IVBMediaControllerService VBMediaControllerService { get { return _VBMediaControllerService; } }

        WFLayoutCalculatorService _WFLayoutCalculatorService = new WFLayoutCalculatorService();
        public virtual IVBWFLayoutCalculatorService WFLayoutCalculatorService { get { return _WFLayoutCalculatorService; } }

        public void AddXamlNamespacesFromAssembly(Assembly classAssembly)
        {
            object[] xmlnsAttributes = classAssembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), true);
            object[] xmlnsPrefixAttributes = classAssembly.GetCustomAttributes(typeof(XmlnsPrefixAttribute), true);
            if (xmlnsAttributes != null && xmlnsPrefixAttributes != null && xmlnsAttributes.Any() && xmlnsPrefixAttributes.Any())
            {
                bool bAdded = false;
                foreach (XmlnsPrefixAttribute prefixAttr in xmlnsPrefixAttributes)
                {
                    var queryAttr = xmlnsAttributes.Where(c => ((XmlnsDefinitionAttribute)c).XmlNamespace == prefixAttr.XmlNamespace);
                    if (queryAttr.Any())
                    {
                        if (!bAdded)
                        {
                            ACxmlnsResolver.Assemblies.Add(classAssembly);
                            bAdded = true;
                        }
                        XmlnsDefinitionAttribute xmlnsDef = (XmlnsDefinitionAttribute)queryAttr.First();
                        if (!ACxmlnsResolver.NamespacesDict.ContainsKey(prefixAttr.Prefix))
                            ACxmlnsResolver.NamespacesDict.Add(prefixAttr.Prefix, new ACxmlnsInfo(classAssembly, prefixAttr.XmlNamespace, xmlnsDef.ClrNamespace, prefixAttr.Prefix));
                    }
                }
            }
        }

        public bool IsAvaloniaUI => true;

    }
}
