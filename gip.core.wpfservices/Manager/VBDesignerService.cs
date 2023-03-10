using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using gip.core.datamodel;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Markup;
using gip.ext.design;
using gip.core.layoutengine;
using gip.ext.designer.Controls;
using gip.core.manager;
using static gip.core.manager.VBDesigner;
using System.Windows.Media;
using System.ComponentModel.Design;
using System.Collections.Concurrent;

namespace gip.core.wpfservices
{
    public class VBDesignerService : IVBDesignerService
    {
        public VBDesignerService() 
        {
        }

        private ConcurrentDictionary<IACComponent, IVBComponentDesignManagerProxy> _DesignManagerProxies = new ConcurrentDictionary<IACComponent, IVBComponentDesignManagerProxy>();

        public IVBComponentDesignManagerProxy GetDesignMangerProxy(IACComponent component)
        {
            IVBComponentDesignManagerProxy proxy = null;
            if (!_DesignManagerProxies.TryGetValue(component, out proxy))
            {
                if (component is VBDesignerWorkflowMethod)
                    proxy = new VBDesignerWorkflowMethodProxy(component);
                else if (component is VBDesignerWorkflow)
                    proxy = new VBDesignerWorkflowProxy(component);
                else if (component is VBDesignerXAML)
                    proxy = new VBDesignerXAMLProxy(component);
                if (proxy != null)
                    _DesignManagerProxies.TryAdd(component, proxy);
            }
            return proxy;
        }

        public void RemoveDesignMangerProxy(IACComponent component)
        {
            IVBComponentDesignManagerProxy proxy = null;
            if (_DesignManagerProxies.ContainsKey(component))
                _DesignManagerProxies.Remove(component, out proxy);
        }

        #region VBPresenterMethod
        public Msg GetPresenterElements(out List<string> result, string xaml)
        {
            result = new List<string>();

            if (string.IsNullOrEmpty(xaml))
                return null;

            try
            {
                Canvas root = XamlReader.Parse(xaml) as Canvas;
                if (root != null)
                    GetElements(root, result);
            }
            catch (Exception e)
            {
                return new Msg(eMsgLevel.Exception, e.Message);
            }
            result = result.Where(c => !string.IsNullOrEmpty(c)).ToList();
            return null;
        }

        private void GetElements(FrameworkElement item, List<string> results)
        {
            if (item == null)
                return;

            results.Add(item.Name);

            ContentControl contentControl = item as ContentControl;
            if (contentControl != null && contentControl.Content != null)
            {
                GetElements(contentControl.Content as FrameworkElement, results);
            }
            else
            {
                Canvas canvas = item as Canvas;
                if (canvas != null && canvas.Children.Count > 0)
                {
                    foreach (var child in canvas.Children)
                    {
                        GetElements(child as FrameworkElement, results);
                    }
                }
            }
        }
        #endregion

    }
}
