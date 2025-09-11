// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace gip.ext.xamldom.avui
{
    /// <summary>
    /// A service provider that provides the IProvideValueTarget and IXamlTypeResolver services.
    /// No other services (e.g. from the document's service provider) are offered.
    /// </summary>
    public class XamlObjectServiceProvider : IServiceProvider, IProvideValueTarget, IUriContext
    // Avalonia not supported interfaces: , IXamlNameResolver, IAmbientProvider, IXamlSchemaContextProvider
    {
        /// <summary>
        /// Creates a new XamlObjectServiceProvider instance.
        /// </summary>
        public XamlObjectServiceProvider(XamlObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            XamlObject = obj;
            Resolver = new XamlTypeResolverProvider(obj);
            //SchemaContext = new XamlSchemaContextProvider(obj);
            //AmbientProvider = new XamlAmbientProvider(obj);
        }

        /// <summary>
        /// Gets the XamlObject that owns this service provider (e.g. the XamlObject that represents a markup extension).
        /// </summary>
        public XamlObject XamlObject { get; private set; }
        internal XamlTypeResolverProvider Resolver { get; private set; }
        //internal XamlSchemaContextProvider SchemaContext { get; private set; }
        //internal XamlAmbientProvider AmbientProvider { get; private set; }


        #region IServiceProvider Members

        /// <summary>
        /// Retrieves the service of the specified type.
        /// </summary>
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IProvideValueTarget))
            {
                return this;
            }
            if (   serviceType == typeof(IXamlTypeResolver)
                || serviceType == typeof(XamlTypeResolverProvider))
            {
                return Resolver;
            }
            if (serviceType == typeof(IUriContext))
            {
                return this;
            }
#if AVALONIA_SUPPORTS_FROM_WPF
            if (serviceType == typeof(IXamlSchemaContextProvider))
                return SchemaContext;
            if (serviceType == typeof(IAmbientProvider))
                return this;
                //return AmbientProvider;
            if (serviceType == typeof(IXamlNameResolver))
			{
				return this;
			}
#endif
            return null;
        }

        #endregion

        #region IProvideValueTarget Members

        /// <summary>
        /// Gets the target object (the DependencyObject instance on which a property should be set)
        /// </summary>
        public virtual object TargetObject
        {
            get
            {
                var parentProperty = XamlObject.ParentProperty;

                if (parentProperty == null)
                {
                    return null;
                }

                if (parentProperty.IsCollection)
                {
                    return parentProperty.ValueOnInstance;
                }

                return parentProperty.ParentObject.Instance;
            }
        }

        /// <summary>
        /// Gets the target dependency property.
        /// </summary>
        public virtual object TargetProperty
        {
            get
            {
                var parentProperty = XamlObject.ParentProperty;

                if (parentProperty == null)
                {
                    return null;
                }

                // Parent Property is a DependencyProperty
                if (parentProperty.IsDependencyProperty)
                {
                    //if (XamlObject.ParentProperty.DependencyProperty != null)
                    return parentProperty.DependencyProperty;
                }
                // Parent Property is not a DependencyProperty  e.g. Collection of MultiBinding
                else
                {
                    //if (XamlObject.ParentProperty.ValueOnInstance != null)
                    return parentProperty.ValueOnInstance;
                }

                //return null; 
            }
        }

        #endregion

        #region IUriContext implementation

        /// <inheritdoc/>		
        public virtual Uri BaseUri
        {
            get
            {
                return new Uri("pack://application:,,,/");
            }
            set
            {

            }
        }

        #endregion

        #region IXamlSchemaContextProvider Members

#if AVALONIA_SUPPORTS_FROM_WPF
        private XamlSchemaContext iCsharpXamlSchemaContext;

        //Maybe we new our own XamlSchemaContext?
        //private class ICsharpXamlSchemaContext : XamlSchemaContext
        //{
        //    public override XamlType GetXamlType(Type type)
        //    {
        //        return base.GetXamlType(type);
        //    }
        //}

        /// <inheritdoc/>
        public XamlSchemaContext SchemaContext
        {
            get
            {
                return iCsharpXamlSchemaContext = iCsharpXamlSchemaContext ?? System.Windows.Markup.XamlReader.GetWpfSchemaContext(); // new XamlSchemaContext();
            }
        }
#endif

        #endregion

        #region IAmbientProvider Members

#if AVALONIA_SUPPORTS_FROM_WPF
        /// <inheritdoc/>
        public AmbientPropertyValue GetFirstAmbientValue(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties)
        {
            return GetAllAmbientValues(ceilingTypes, properties).FirstOrDefault();
        }

        /// <inheritdoc/>
        public object GetFirstAmbientValue(params XamlType[] types)
        {
            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<AmbientPropertyValue> GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties)
        {
            var obj = this.XamlObject.ParentObject;

            while (obj != null)
            {
                if (ceilingTypes.Any(x => obj.SystemXamlTypeForProperty.CanAssignTo(x)))
                {
                    foreach (var pr in obj.Properties)
                    {
                        if (properties.Any(x => x.Name == pr.PropertyName))
                        {
                            yield return new AmbientPropertyValue(pr.SystemXamlMemberForProperty, pr.ValueOnInstance);
                        }
                    }
                }

                obj = obj.ParentObject;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<object> GetAllAmbientValues(params XamlType[] types)
        {
            return new List<object>();
        }

        /// <inheritdoc/>
        public IEnumerable<AmbientPropertyValue> GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, params XamlMember[] properties)
        {
            return new List<AmbientPropertyValue>();
        }
#endif

        #endregion

        #region IXamlNameResolver

        /// <inheritdoc/>
        public object Resolve(string name)
        {
            INameScope ns = null;
            var xamlObj = this.XamlObject;
            while (xamlObj != null)
            {
                ns = NameScopeHelper.GetNameScopeFromObject(xamlObj);

                if (ns != null)
                {
                    var obj = ns.Find(name);
                    if (obj != null)
                        return obj;
                }

                xamlObj = xamlObj.ParentObject;
            }

            return null;
        }

        /// <inheritdoc/>
        public object Resolve(string name, out bool isFullyInitialized)
        {
            var ret = Resolve(name);
            isFullyInitialized = ret != null;
            return ret;
        }

        /// <inheritdoc/>
        public object GetFixupToken(IEnumerable<string> names)
        {
            return null;
        }

        /// <inheritdoc/>
        public object GetFixupToken(IEnumerable<string> names, bool canAssignDirectly)
        {
            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope()
        {
            return null;
        }

        /// <inheritdoc/>
        public bool IsFixupTokenAvailable
        {
            get { return false; }
        }

#pragma warning disable 0067 // Required by interface implementation, disable Warning CS0067: The event is never used
        /// <inheritdoc/>
        public event EventHandler OnNameScopeInitializationComplete;
#pragma warning restore 0067

        #endregion
    }


    public class TriggerDummyServiceProvider : XamlObjectServiceProvider
    {
        public TriggerDummyServiceProvider(XamlObject obj, XamlPropertyInfo propertyInfo)
            : base(obj)
        {
            _PropertyInfo = propertyInfo;
        }

        private XamlPropertyInfo _PropertyInfo;
        public override object TargetObject
        {
            get
            {
                return XamlObject.Instance;
            }
        }

        public override object TargetProperty
        {
            get
            {
                return _PropertyInfo.DependencyProperty;
            }
        }
    }
}
