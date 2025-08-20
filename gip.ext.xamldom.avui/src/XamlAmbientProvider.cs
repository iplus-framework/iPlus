// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

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
using System.Linq;

namespace gip.ext.xamldom.avui
{
    sealed class XamlAmbientProvider : IAmbientProvider, IServiceProvider
	{
		XamlDocument document;
		XamlObject containingObject;
		
		public XamlAmbientProvider(XamlObject containingObject)
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
            if (serviceType == typeof(IAmbientProvider) || serviceType == typeof(XamlAmbientProvider))
				return this;
			else
				return document.ServiceProvider.GetService(serviceType);
		}


        public IEnumerable<AmbientPropertyValue> GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, params XamlMember[] properties)
        {
            return DoGetAllAmbientValues(ceilingTypes, searchLiveStackOnly, types, properties).ToList();
        }

        public IEnumerable<object> GetAllAmbientValues(params XamlType[] types)
        {
            return GetAllAmbientValues(null, false, types);
        }

        public IEnumerable<AmbientPropertyValue> GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties)
        {
            return GetAllAmbientValues(ceilingTypes, false, null, properties);
        }

        public object GetFirstAmbientValue(params XamlType[] types)
        {
            foreach (var obj in GetAllAmbientValues(types))
				return obj;
			return null;
        }

        public AmbientPropertyValue GetFirstAmbientValue(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties)
        {
            foreach (var obj in GetAllAmbientValues(ceilingTypes, properties))
				return obj;
			return null;
        }


        List<AmbientPropertyValue> values = new List<AmbientPropertyValue> ();
		Stack<AmbientPropertyValue> live_stack = new Stack<AmbientPropertyValue> ();

		public void Push (AmbientPropertyValue v)
		{
			live_stack.Push (v);
			values.Add (v);
		}

		public void Pop ()
		{
			live_stack.Pop ();
		}

        IEnumerable<AmbientPropertyValue> DoGetAllAmbientValues (IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, params XamlMember [] properties)
		{
			if (searchLiveStackOnly) {
				if (live_stack.Count > 0) {
					// pop, call recursively, then push back.
					var p = live_stack.Pop ();
					if (p.RetrievedProperty != null && ceilingTypes != null && ceilingTypes.Contains (p.RetrievedProperty.Type))
						yield break;
					if (DoesAmbientPropertyApply (p, types, properties))
						yield return p;

					foreach (var i in GetAllAmbientValues (ceilingTypes, searchLiveStackOnly, types, properties))
						yield return i;

					live_stack.Push (p);
				}
			} else {
				// FIXME: does ceilingTypes matter?
				foreach (var p in values)
					if (DoesAmbientPropertyApply (p, types, properties))
						yield return p;
			}
		}
		
		bool DoesAmbientPropertyApply (AmbientPropertyValue p, IEnumerable<XamlType> types, params XamlMember [] properties)
		{
			if (types == null || !types.Any () || types.Any (xt => xt.UnderlyingType != null && xt.UnderlyingType.IsInstanceOfType (p.Value)))
				if (properties == null || !properties.Any () || properties.Contains (p.RetrievedProperty))
					return true;
			return false;
		}
		
    }
}
