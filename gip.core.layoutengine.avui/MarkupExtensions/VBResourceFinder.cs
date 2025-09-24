using Avalonia;
using Avalonia.Controls;
using gip.core.datamodel;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    public static class VBResourceFinder
    {
        public static IEnumerable<object> FindResources(IACInteractiveObject acContext)
        {
            if (!(acContext is StyledElement))
                return null;
            
            List<object> resources = new List<object>();
            StyledElement fwElement = acContext as StyledElement;

            // Generelle Einstellungen für WPF-Objekte
            VBResourceKey key = new VBResourceKey() { ACUrlWPF = acContext.ACType.GetACUrl() };
            object resource = null;
            if (fwElement.TryFindResource(key.GetHashCode(), out resource))
                resources.Add(resource);

            // Generelle Einstellungen für die gebundene Property
            IACObject acObject = acContext.ACContentList.First();
            if ((acObject != null) && (acObject.ACType is ACClassProperty))
            {
                ACClassProperty acClassProperty = acObject.ACType as ACClassProperty;
                foreach (ACClassProperty baseProperty in acClassProperty.OverriddenProperties)
                {
                    key = new VBResourceKey() { ACUrlProperty = baseProperty.GetACUrl() };
                    if (fwElement.TryFindResource(key.GetHashCode(), out resource))
                        resources.Add(resource);

                    // Kombination aus Property mit WPF-Objekt
                    key = new VBResourceKey() { ACUrlWPF = acContext.ACType.GetACUrl(), ACUrlProperty = baseProperty.GetACUrl() };
                    if (fwElement.TryFindResource(key.GetHashCode(), out resource))
                        resources.Add(resource);
                }
            }

            return resources;
        }
    }

}
