using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    public static class VBResourceFinder
    {
        public static IEnumerable<object> FindResources(IACInteractiveObject acContext)
        {
            if (!(acContext is FrameworkElement))
                return null;
            
            List<object> resources = new List<object>();
            FrameworkElement fwElement = acContext as FrameworkElement;

            // Generelle Einstellungen für WPF-Objekte
            VBResourceKey key = new VBResourceKey() { ACUrlWPF = acContext.ACType.GetACUrl() };
            object resource = fwElement.TryFindResource(key.GetHashCode());
            if (resource != null)
                resources.Add(resource);

            // Generelle Einstellungen für die gebundene Property
            IACObject acObject = acContext.ACContentList.First();
            if ((acObject != null) && (acObject.ACType is ACClassProperty))
            {
                ACClassProperty acClassProperty = acObject.ACType as ACClassProperty;
                foreach (ACClassProperty baseProperty in acClassProperty.OverriddenProperties)
                {
                    key = new VBResourceKey() { ACUrlProperty = baseProperty.GetACUrl() };
                    resource = fwElement.TryFindResource(key.GetHashCode());
                    if (resource != null)
                        resources.Add(resource);

                    // Kombination aus Property mit WPF-Objekt
                    key = new VBResourceKey() { ACUrlWPF = acContext.ACType.GetACUrl(), ACUrlProperty = baseProperty.GetACUrl() };
                    resource = fwElement.TryFindResource(key.GetHashCode());
                    if (resource != null)
                        resources.Add(resource);
                }
            }

            return resources;
        }
    }

}
