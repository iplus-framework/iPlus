using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using gip.core.wpfservices.Manager;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace gip.core.wpfservices
{
    public class WPFServices : IWPFServices
    {
        VBDesignerService _VBDesignerService = new VBDesignerService();
        public IVBDesignerService DesignerService { get { return _VBDesignerService; } }

        WFLayoutCalculatorService _WFLayoutCalculatorService = new WFLayoutCalculatorService();
        public IVBWFLayoutCalculatorService WFLayoutCalculatorService { get { return _WFLayoutCalculatorService; } }

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
    }
}
