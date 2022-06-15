using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data;
using gip.core.datamodel;
using System.ComponentModel;
using System.Xml;

namespace gip.core.autocomponent
{
    public class WCFDataContractResolver : DataContractResolver
    {
        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
        {
            Type resolvedType = null;
            if (typeName == "XmlACEventArgs")
            {
                return TypeACEventArgs;
            }
            else if (typeName == "XmlACMethodEventArgs")
            {
                return TypeACMethodEventArgs;
            }
            else if (typeName == "XmlStringArray")
                return TypeStringArray;
            else
            {
                resolvedType = knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);
            }
            return resolvedType;
        }

        static Type _TypeACEventArgs = null;
        static Type TypeACEventArgs
        {
            get
            {
                if (_TypeACEventArgs == null)
                    _TypeACEventArgs = typeof(ACEventArgs);
                return _TypeACEventArgs;
            }
        }

        static Type _TypeACMethodEventArgs = null;
        static Type TypeACMethodEventArgs
        {
            get
            {
                if (_TypeACMethodEventArgs == null)
                    _TypeACMethodEventArgs = typeof(ACMethodEventArgs);
                return _TypeACMethodEventArgs;
            }
        }

        static Type _TypeStringArray = null;
        static Type TypeStringArray
        {
            get
            {
                if (_TypeStringArray == null)
                    _TypeStringArray = typeof(string[]);
                return _TypeStringArray;
            }
        }

        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out System.Xml.XmlDictionaryString typeName, out System.Xml.XmlDictionaryString typeNamespace)
        {
            bool result = false;
            if (type == TypeACEventArgs)
            {
                XmlDictionary dictionary = new XmlDictionary();
                typeName = dictionary.Add("XmlACEventArgs");
                typeNamespace = dictionary.Add("http://schemas.datacontract.org/2004/07/gip.core.datamodel");
                return true;
            }
            else if (type == TypeACMethodEventArgs)
            {
                XmlDictionary dictionary = new XmlDictionary();
                typeName = dictionary.Add("XmlACMethodEventArgs");
                typeNamespace = dictionary.Add("http://schemas.datacontract.org/2004/07/gip.core.datamodel");
                return true;
            }
            else if (type == TypeStringArray)
            {
                XmlDictionary dictionary = new XmlDictionary();
                typeName = dictionary.Add("XmlStringArray");
                typeNamespace = dictionary.Add("http://schemas.datacontract.org/2004/07/gip.core.datamodel");
                return true;
            }
            else
            {
                result = knownTypeResolver.TryResolveType(type, declaredType, null, out typeName, out typeNamespace);
                if (!result)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                        System.Diagnostics.Debugger.Break();
                    else
                        Database.Root.Messages.LogError("WCFDataContractResolver", "TryResolveType(10)", String.Format("Cant resolve type {0}, declaredType{1}", type.AssemblyQualifiedName, declaredType.AssemblyQualifiedName));
                }
            }
            return result;
        }
    }
}
