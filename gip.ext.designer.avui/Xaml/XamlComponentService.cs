﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Markup;
using System.Collections;
using gip.ext.xamldom.avui;
using gip.ext.design.avui;
using System.Linq;

namespace gip.ext.designer.avui.Xaml
{
	sealed class XamlComponentService : IComponentService
	{
        public event EventHandler<DesignItemPropertyChangedEventArgs> PropertyChanged;

        #region IdentityEqualityComparer
        sealed class IdentityEqualityComparer : IEqualityComparer<object>
		{
			internal static readonly IdentityEqualityComparer Instance = new IdentityEqualityComparer();
			
			int IEqualityComparer<object>.GetHashCode(object obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}
			
			bool IEqualityComparer<object>.Equals(object x, object y)
			{
				return x == y;
			}
		}
		#endregion
		
		readonly XamlDesignContext _context;
		
		public XamlComponentService(XamlDesignContext context)
		{
			this._context = context;
		}

        public event EventHandler<DesignItemEventArgs> ComponentRegisteredAndAddedToContainer;

        public event EventHandler<DesignItemEventArgs> ComponentRegistered;

        public event EventHandler<DesignItemEventArgs> ComponentRemoved;

        // TODO: this must not be a dictionary because there's no way to unregister components
        // however, this isn't critical because our design items will stay alive for the lifetime of the
        // designer anyway if we don't limit the Undo stack.
        Dictionary<object, XamlDesignItem> _sites = new Dictionary<object, XamlDesignItem>(IdentityEqualityComparer.Instance);

        public DesignItem GetDesignItem(object component)
        {
            return GetDesignItem(component, false);
        }

        public DesignItem GetDesignItem(object component, bool findByView)
        {
            if (component == null)
                throw new ArgumentNullException("component");
            XamlDesignItem site;
            _sites.TryGetValue(component, out site);

            if (findByView)
            {
                site = site ?? _sites.Values.FirstOrDefault(x => Equals(x.View, component));
            }
            return site;
        }

        public void SetDefaultPropertyValues(DesignItem designItem)
        {
            var values = Metadata.GetDefaultPropertyValues(designItem.ComponentType);
            if (values != null)
            {
                foreach (var value in values)
                {
                    designItem.Properties[value.Key].SetValue(value.Value);
                }
            }
        }

        public DesignItem RegisterComponentForDesigner(object component)
        {
            return RegisterComponentForDesigner(null, component);
        }

        public DesignItem RegisterComponentForDesigner(DesignItem parent, object component)
        {
            if (component == null)
            {
                component = new NullExtension();
            }
            else if (component is Type)
            {
                component = new TypeExtension((Type)component);
            }

            XamlObject parentXamlObject = null;
            if (parent != null)
                parentXamlObject = ((XamlDesignItem)parent).XamlObject;

            XamlDesignItem item = new XamlDesignItem(_context.Document.CreateObject(parentXamlObject, component), _context);
            _context.Services.ExtensionManager.ApplyDesignItemInitializers(item);

            if (!(component is string))
                _sites.Add(component, item);
            if (ComponentRegistered != null)
            {
                ComponentRegistered(this, new DesignItemEventArgs(item));
            }
            return item;
        }

        public DesignItem RegisterComponentForDesignerRecursiveUsingXaml(object component)
        {
            string componentXaml = XamlWriter.Save(component);
            var xamlObject = XamlParser.ParseSnippet(((XamlDesignItem)_context.RootItem).XamlObject, componentXaml, ((XamlDesignContext)_context.RootItem.Context).ParserSettings);
            return RegisterXamlComponentRecursive(xamlObject);
        }

        /// <summary>
        /// registers components from an existing XAML tree
        /// </summary>
        internal void RaiseComponentRegisteredAndAddedToContainer(DesignItem obj)
        {
            if (ComponentRegisteredAndAddedToContainer != null)
            {
                ComponentRegisteredAndAddedToContainer(this, new DesignItemEventArgs(obj));
            }
        }


        /// <summary>
        /// registers components from an existing XAML tree
        /// </summary>
        public XamlDesignItem RegisterXamlComponentRecursive(XamlObject obj)
        {
            if (obj == null) return null;

            foreach (XamlProperty prop in obj.Properties)
            {
                RegisterXamlComponentRecursive(prop.PropertyValue as XamlObject);
                foreach (XamlPropertyValue val in prop.CollectionElements)
                {
                    RegisterXamlComponentRecursive(val as XamlObject);
                }
            }

            XamlDesignItem site = new XamlDesignItem(obj, _context);
            _context.Services.ExtensionManager.ApplyDesignItemInitializers(site);

            _sites.Add(site.Component, site);
            if (ComponentRegistered != null)
            {
                ComponentRegistered(this, new DesignItemEventArgs(site));
            }

            if (_context.RootItem != null && !string.IsNullOrEmpty(site.Name))
            {
                var nameScope = NameScopeHelper.GetNameScopeFromObject(((XamlDesignItem)_context.RootItem).XamlObject);

                if (nameScope != null)
                {
                    // The object will be a part of the RootItem namescope, remove local namescope if set
                    NameScopeHelper.ClearNameScopeProperty(obj.Instance);

                    string newName = site.Name;
                    if (nameScope.FindName(newName) != null)
                    {
                        int copyIndex = newName.LastIndexOf("_Copy", StringComparison.Ordinal);
                        if (copyIndex < 0)
                        {
                            newName += "_Copy";
                        }
                        else if (!newName.EndsWith("_Copy", StringComparison.Ordinal))
                        {
                            string copyEnd = newName.Substring(copyIndex + "_Copy".Length);
                            int copyEndValue;
                            if (Int32.TryParse(copyEnd, out copyEndValue))
                                newName = newName.Remove(copyIndex + "_Copy".Length);
                            else
                                newName += "_Copy";
                        }

                        int i = 1;
                        string newNameTemplate = newName;
                        while (nameScope.FindName(newName) != null)
                        {
                            newName = newNameTemplate + i++;
                        }

                        site.Name = newName;
                    }

                    nameScope.RegisterName(newName, obj.Instance);
                }
            }
            return site;
        }


        /// <summary>
        /// raises the Property changed Events
        /// </summary>
        internal void RaisePropertyChanged(XamlModelProperty property, object oldValue, object newValue)
        {
            var ev = this.PropertyChanged;
            if (ev != null)
            {
                ev(this, new DesignItemPropertyChangedEventArgs(property.DesignItem, property, oldValue, newValue));
            }
        }

        /// <summary>
        /// raises the RaiseComponentRemoved Event
        /// </summary>
        internal void RaiseComponentRemoved(DesignItem item)
        {
            var ev = this.ComponentRemoved;
            if (ev != null)
            {
                ev(this, new DesignItemEventArgs(item));
            }
        }
	}


        public IEnumerable<DesignItem> DesignItems
        {
            get 
            {
                return _sites.Values;
            }
        }
    }
}
