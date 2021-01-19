using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Dictionary for all Child-Workflow-Instances that are stored under the ACClassWF as key.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.Dictionary{gip.core.datamodel.ACClassWF, gip.core.datamodel.ACRef{gip.core.autocomponent.ACComponent}}" />
    public class WFDictionary : Dictionary<ACClassWF, ACRef<ACComponent>>
    {
        public ACComponent GetPWComponent(ACClassWF acClassWF)
        {
            ACRef<ACComponent> refComp;
            if (TryGetValue(acClassWF, out refComp))
                return refComp.ValueT;
            return null;
        }

        public void AddPWComponent(ACClassWF acClassWF, ACComponent pwComponent)
        {
            ACComponent exists = GetPWComponent(acClassWF);
            if (exists == null)
            {
                ACRef<ACComponent> refComp = new ACRef<ACComponent>(pwComponent, false);
                base.Add(acClassWF, refComp);
            }
        }

        public void DetachAll()
        {
            foreach (ACRef<ACComponent> refComp in this.Values)
            {
                refComp.Detach();
            }
        }
    }
}
