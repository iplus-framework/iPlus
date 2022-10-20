// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using System.Xaml;

namespace gip.ext.xamldom
{
    sealed class XamlSchemaContextProvider : IXamlSchemaContextProvider, IServiceProvider
	{
		XamlDocument document;
		XamlObject containingObject;
		
		public XamlSchemaContextProvider(XamlObject containingObject)
		{
			if (containingObject == null)
				throw new ArgumentNullException("containingObject");
			this.document = containingObject.OwnerDocument;
			this.containingObject = containingObject;
		}

		XmlElement ContainingElement{
			get { return containingObject.XmlElement; }
		}
		
	
		public object GetService(Type serviceType)
		{
            if (serviceType == typeof(IXamlSchemaContextProvider) || serviceType == typeof(XamlSchemaContextProvider))
				return this;
			else
				return document.ServiceProvider.GetService(serviceType);
		}

        private XamlSchemaContext _SchemaContext;
        public XamlSchemaContext SchemaContext
        {
            get 
            {
                if (_SchemaContext == null)
                    _SchemaContext = new XamlSchemaContext(XamlTypeFinder.CreateWpfTypeFinder().LoadedAssemblies);
                return _SchemaContext;
            }
        }
    }
}
