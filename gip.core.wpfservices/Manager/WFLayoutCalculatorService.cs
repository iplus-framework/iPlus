using gip.core.datamodel;
using gip.core.manager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.wpfservices.Manager
{
    public class WFLayoutCalculatorService : IVBWFLayoutCalculatorService
    {
        public WFLayoutCalculatorService()
        {
        }

        private ConcurrentDictionary<IACComponent, IVBWFLayoutCalculatorProxy> _WFLayoutCalculatorProxies = new ConcurrentDictionary<IACComponent, IVBWFLayoutCalculatorProxy>();

        public IVBWFLayoutCalculatorProxy GetWFLayoutCalculatorProxy(IACComponent component)
        {
            IVBWFLayoutCalculatorProxy proxy = null;
            if (!_WFLayoutCalculatorProxies.TryGetValue(component, out proxy))
            {
                proxy = new WFLayoutCalculatorProxy(component);

                if (proxy != null)
                    _WFLayoutCalculatorProxies.TryAdd(component, proxy);
            }
            return proxy;
        }

        public void RemoveWFLayoutCalculatorProxy(IACComponent component)
        {
            IVBWFLayoutCalculatorProxy proxy = null;
            if (_WFLayoutCalculatorProxies.ContainsKey(component))
                _WFLayoutCalculatorProxies.Remove(component, out proxy);

        }
    }
}
