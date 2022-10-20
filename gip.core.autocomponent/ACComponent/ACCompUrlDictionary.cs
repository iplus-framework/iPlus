using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public class ACCompUrlDictionary : Dictionary<string, ACRef<ACComponent>>
    {
        private object _Lock = new object();

        public void AddComponent(ACComponent acComponent)
        {
            if (acComponent == null)
                return;
            lock (_Lock)
            {
                if(!this.ContainsKey(acComponent.ACUrl))
                {
                    Add(acComponent.ACUrl, new ACRef<ACComponent>(acComponent, false));
                }
            }
        }

        public void RemoveComponent(ACComponent acComponent)
        {
            if (acComponent == null)
                return;
            lock (_Lock)
            {
                if(this.ContainsKey(acComponent.ACUrl))
                {
                    Remove(acComponent.ACUrl);
                }
            }
        }
    }
}
