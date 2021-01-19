// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.ComponentModel;
using System.Xaml;

namespace gip.ext.xamldom
{
    /// <summary>
    /// A service provider that provides the IProvideValueTarget and IXamlTypeResolver services.
    /// No other services (e.g. from the document's service provider) are offered.
    /// </summary>
    public class XamlObjectServiceProvider : IServiceProvider, IProvideValueTarget
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
            SchemaContext = new XamlSchemaContextProvider(obj);
            AmbientProvider = new XamlAmbientProvider(obj);
        }

        /// <summary>
        /// Gets the XamlObject that owns this service provider (e.g. the XamlObject that represents a markup extension).
        /// </summary>
        public XamlObject XamlObject { get; private set; }
        internal XamlTypeResolverProvider Resolver { get; private set; }
        internal XamlSchemaContextProvider SchemaContext { get; private set; }
        internal XamlAmbientProvider AmbientProvider { get; private set; }


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
            if (serviceType == typeof(IXamlTypeResolver))
            {
                return Resolver;
            }
            if (serviceType == typeof(IXamlSchemaContextProvider))
                return SchemaContext;
            if (serviceType == typeof(IAmbientProvider))
                return AmbientProvider;
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
                return XamlObject.ParentProperty.ParentObject.Instance;
            }
        }

        /// <summary>
        /// Gets the target dependency property.
        /// </summary>
        public virtual object TargetProperty
        {
            get
            {
                // Parent Property is a DependencyProperty
                if (XamlObject.ParentProperty.IsDependencyProperty)
                {
                    //if (XamlObject.ParentProperty.DependencyProperty != null)
                    return XamlObject.ParentProperty.DependencyProperty;
                }
                // Parent Property is not a DependencyProperty  e.g. Collection of MultiBinding
                else
                {
                    //if (XamlObject.ParentProperty.ValueOnInstance != null)
                    return XamlObject.ParentProperty.ValueOnInstance;
                }

                //return null; 
            }
        }

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
