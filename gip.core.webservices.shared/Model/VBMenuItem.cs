using System;
using System.Runtime.Serialization;
using System.Reflection;

namespace gip.core.webservices
{
    [DataContract]
    public class VBMenuItem
    {
        [DataMember]
        public string Label { get; set; }

        [DataMember]
        public string PageClassName { get; set; }

        [IgnoreDataMember]
        private Type _DestPage;
        public Type DestPage
        {
            get
            {
                return _DestPage;
            }
            set
            {
                _DestPage = value;
            }
        }

        public bool ResolveDestPage(Assembly assembly, string pageNameSpace)
        {
            if (assembly == null || String.IsNullOrEmpty(pageNameSpace))
                return false;
            _DestPage = assembly.GetType(pageNameSpace + "." + PageClassName);
            return _DestPage != null;
        }
    }
}
