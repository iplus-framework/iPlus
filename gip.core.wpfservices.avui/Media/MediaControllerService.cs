// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace gip.core.wpfservices.avui
{
    public class MediaControllerService : IVBMediaControllerService
    {
        private ConcurrentDictionary<IACComponent, IVBMediaControllerProxy> _MediaControllerProxies = new ConcurrentDictionary<IACComponent, IVBMediaControllerProxy>();

        public IVBMediaControllerProxy GetMediaControllerProxy(IACComponent component)
        {
            IVBMediaControllerProxy proxy = null;
            if (!_MediaControllerProxies.TryGetValue(component, out proxy))
            {
                proxy = new MediaControllerProxy(component);

                if (proxy != null)
                    _MediaControllerProxies.TryAdd(component, proxy);
            }
            return proxy;
        }

        public void RemoveMediaControllerProxy(IACComponent component)
        {
            IVBMediaControllerProxy proxy = null;
            if (_MediaControllerProxies.ContainsKey(component))
                _MediaControllerProxies.Remove(component, out proxy);

        }

        public IEnumerable<string> GetWindowsPrinters()
        {
            System.Drawing.Printing.PrinterSettings.StringCollection windowsPrinters = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
            List<string> windowsPrintersList = new List<string>();
            foreach (string printer in windowsPrinters)
            {
                windowsPrintersList.Add(printer);
            }
            return windowsPrintersList;
        }
    }
}
