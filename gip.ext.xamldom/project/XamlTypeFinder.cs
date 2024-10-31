// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Markup;

namespace gip.ext.xamldom
{
    /// <summary>
    /// Allows finding types in a set of assemblies.
    /// </summary>
    public class XamlTypeFinder : ICloneable
    {
        sealed class AssemblyNamespaceMapping : IEquatable<AssemblyNamespaceMapping>
        {
            internal readonly Assembly Assembly;
            internal readonly string Namespace;

            internal AssemblyNamespaceMapping(Assembly assembly, string @namespace)
            {
                this.Assembly = assembly;
                this.Namespace = @namespace;
            }

            public override int GetHashCode()
            {
                return Assembly.GetHashCode() ^ Namespace.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as AssemblyNamespaceMapping);
            }

            public bool Equals(AssemblyNamespaceMapping other)
            {
                return other != null && other.Assembly == this.Assembly && other.Namespace == this.Namespace;
            }
        }

        sealed class XamlNamespace
        {
            internal readonly string XmlNamespace;

            internal XamlNamespace(string xmlNamespace)
            {
                this.XmlNamespace = xmlNamespace;
            }

            internal List<AssemblyNamespaceMapping> ClrNamespaces = new List<AssemblyNamespaceMapping>();

            internal XamlNamespace Clone()
            {
                XamlNamespace copy = new XamlNamespace(this.XmlNamespace);
                // AssemblyNamespaceMapping is immutable
                copy.ClrNamespaces.AddRange(this.ClrNamespaces);
                return copy;
            }
        }

        Dictionary<string, XamlNamespace> namespaces = new Dictionary<string, XamlNamespace>();
        Dictionary<AssemblyNamespaceMapping, string> reverseDict = new Dictionary<AssemblyNamespaceMapping, string>();
        Dictionary<AssemblyNamespaceMapping, string> reverseDictPrefix = new Dictionary<AssemblyNamespaceMapping, string>();

        /// <summary>
        /// Gets a type referenced in XAML.
        /// </summary>
        /// <param name="xmlNamespace">The XML namespace to use to look up the type.
        /// This can be a registered namespace or a 'clr-namespace' value.</param>
        /// <param name="localName">The local name of the type to find.</param>
        /// <returns>
        /// The requested type, or null if it could not be found.
        /// </returns>
        public Type GetType(string xmlNamespace, string localName)
        {
            if (xmlNamespace == null)
                throw new ArgumentNullException("xmlNamespace");
            if (localName == null)
                throw new ArgumentNullException("localName");
            XamlNamespace ns;
            if (!namespaces.TryGetValue(xmlNamespace, out ns))
            {
                if (xmlNamespace.StartsWith("clr-namespace:", StringComparison.Ordinal))
                {
                    ns = namespaces[xmlNamespace] = ParseNamespace(xmlNamespace);
                }
                else
                {
                    foreach (KeyValuePair<string, XamlNamespace> kvp in namespaces)
                    {
                        Type result = GetTypeInNamespaceMapping(kvp.Value, localName, true);
                        if (result != null)
                            return result;
                    }
                    return null;
                }
            }
            if (ns != null)
            {
                return GetTypeInNamespaceMapping(ns, localName);
            }
            return null;
        }

        private Type GetTypeInNamespaceMapping(XamlNamespace ns, string localName, bool onlyWPF = false)
        {
            foreach (AssemblyNamespaceMapping mapping in ns.ClrNamespaces)
            {
                if (onlyWPF)
                {
                    // WindowsBase, PresentationCore, PresentationFramework
                    if ((typeof(MarkupExtension).Assembly != mapping.Assembly)
                        && (typeof(IAddChild).Assembly != mapping.Assembly)
                        && (typeof(XamlReader).Assembly != mapping.Assembly))
                        continue;
                }

                try
                {
                    Type type = mapping.Assembly.GetType(mapping.Namespace + "." + localName);
                    if (type != null)
                        return type;
                }
                catch (Exception)
                {
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="localName"></param>
        /// <returns></returns>
        public Type GetTypeOverPrefix(string prefix, string localName)
        {
            if (String.IsNullOrEmpty(prefix) || String.IsNullOrEmpty(localName))
                return null;
            foreach (KeyValuePair<AssemblyNamespaceMapping, string> kvp in reverseDictPrefix)
            {
                if (kvp.Value == prefix)
                {
                    Type type = kvp.Key.Assembly.GetType(kvp.Key.Namespace + "." + localName);
                    if (type != null)
                        return type;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlNamespace"></param>
        /// <param name="localName"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public Type GetType(string xmlNamespace, string localName, string prefix)
        {
            if (xmlNamespace == null)
                throw new ArgumentNullException("xmlNamespace");
            if (localName == null)
                throw new ArgumentNullException("localName");
            XamlNamespace ns;
            if (!namespaces.TryGetValue(xmlNamespace, out ns))
            {
                if (xmlNamespace.StartsWith("clr-namespace:", StringComparison.Ordinal))
                {
                    ns = namespaces[xmlNamespace] = ParseNamespace(xmlNamespace);
                }
                else
                {
                    return null;
                }
            }
            foreach (AssemblyNamespaceMapping mapping in ns.ClrNamespaces)
            {
                Type type = mapping.Assembly.GetType(mapping.Namespace + "." + localName);
                if (type != null)
                {
                    if (!String.IsNullOrEmpty(prefix))
                    {
                        string hasPrefix;
                        if (!reverseDictPrefix.TryGetValue(mapping, out hasPrefix))
                        {
                            reverseDictPrefix.Add(mapping, prefix);
                        }
                    }
                    return type;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the XML namespace that can be used for the specified assembly/namespace combination.
        /// </summary>
        public string GetXmlNamespaceFor(Assembly assembly, string @namespace, out string prefix)
        {
            AssemblyNamespaceMapping mapping = new AssemblyNamespaceMapping(assembly, @namespace);
            string xmlNamespace;
            if (reverseDict.TryGetValue(mapping, out xmlNamespace))
            {
                reverseDictPrefix.TryGetValue(mapping, out prefix);
                return xmlNamespace;
            }
            else
            {
                prefix = "";
                return "clr-namespace:" + mapping.Namespace + ";assembly=" + mapping.Assembly.GetName().Name;
            }
        }

        XamlNamespace ParseNamespace(string xmlNamespace)
        {
            string name = xmlNamespace;
            Debug.Assert(name.StartsWith("clr-namespace:", StringComparison.Ordinal));
            name = name.Substring("clr-namespace:".Length);
            string namespaceName, assembly;
            int pos = name.IndexOf(';');
            if (pos < 0)
            {
                namespaceName = name;
                assembly = "";
            }
            else
            {
                namespaceName = name.Substring(0, pos);
                name = name.Substring(pos + 1).Trim();
                if (!name.StartsWith("assembly=", StringComparison.Ordinal))
                {
                    throw new XamlLoadException("Expected: 'assembly='");
                }
                assembly = name.Substring("assembly=".Length);
            }
            XamlNamespace ns = new XamlNamespace(xmlNamespace);
            Assembly asm = LoadAssembly(assembly);
            if (asm != null)
            {
                AddMappingToNamespace(ns, new AssemblyNamespaceMapping(asm, namespaceName));
            }
            return ns;
        }

        void AddMappingToNamespace(XamlNamespace ns, AssemblyNamespaceMapping mapping)
        {
            ns.ClrNamespaces.Add(mapping);

            string xmlNamespace;
            if (reverseDict.TryGetValue(mapping, out xmlNamespace))
            {
                if (xmlNamespace == XamlConstants.PresentationNamespace)
                {
                    return;
                }
            }
            reverseDict[mapping] = ns.XmlNamespace;
        }

        /// <summary>
        /// Registers XAML namespaces defined in the <paramref name="assembly"/> for lookup.
        /// </summary>
        public void RegisterAssembly(Assembly assembly, bool withPrefixes = false)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            foreach (XmlnsDefinitionAttribute xmlnsDef in assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), true))
            {
                XamlNamespace ns;
                if (!namespaces.TryGetValue(xmlnsDef.XmlNamespace, out ns))
                {
                    ns = namespaces[xmlnsDef.XmlNamespace] = new XamlNamespace(xmlnsDef.XmlNamespace);
                }
                if (string.IsNullOrEmpty(xmlnsDef.AssemblyName))
                {
                    AssemblyNamespaceMapping mapping = new AssemblyNamespaceMapping(assembly, xmlnsDef.ClrNamespace);
                    AddMappingToNamespace(ns, mapping);
                    if (withPrefixes)
                    {
                        foreach (XmlnsPrefixAttribute xmlnsPrefix in assembly.GetCustomAttributes(typeof(XmlnsPrefixAttribute), true))
                        {
                            if (xmlnsPrefix.XmlNamespace == xmlnsDef.XmlNamespace)
                            {
                                if (!String.IsNullOrEmpty(xmlnsPrefix.Prefix))
                                {
                                    string hasPrefix;
                                    if (!reverseDictPrefix.TryGetValue(mapping, out hasPrefix))
                                    {
                                        reverseDictPrefix.Add(mapping, xmlnsPrefix.Prefix);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Assembly asm = LoadAssembly(xmlnsDef.AssemblyName);
                    if (asm != null)
                    {
                        AssemblyNamespaceMapping mapping = new AssemblyNamespaceMapping(asm, xmlnsDef.ClrNamespace);
                        AddMappingToNamespace(ns, mapping);
                        if (withPrefixes)
                        {
                            foreach (XmlnsPrefixAttribute xmlnsPrefix in asm.GetCustomAttributes(typeof(XmlnsPrefixAttribute), true))
                            {
                                if (xmlnsPrefix.XmlNamespace == xmlnsDef.XmlNamespace)
                                {
                                    if (!String.IsNullOrEmpty(xmlnsPrefix.Prefix))
                                    {
                                        string hasPrefix;
                                        if (!reverseDictPrefix.TryGetValue(mapping, out hasPrefix))
                                        {
                                            reverseDictPrefix.Add(mapping, xmlnsPrefix.Prefix);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers XAML namespaces defined in the <paramref name="assembly"/> for lookup.
        /// </summary>
        public void RegisterAssembly(Assembly assembly, string @namespace, string prefix)
        {
            RegisterAssembly(assembly);
            AssemblyNamespaceMapping mapping = new AssemblyNamespaceMapping(assembly, @namespace);

            if (!String.IsNullOrEmpty(prefix))
            {
                string hasPrefix;
                if (!reverseDictPrefix.TryGetValue(mapping, out hasPrefix))
                {
                    reverseDictPrefix.Add(mapping, prefix);
                }
            }
        }

        /// <summary>
        /// Load the assembly with the specified name.
        /// You can override this method to implement custom assembly lookup.
        /// </summary>
        public virtual Assembly LoadAssembly(string name)
        {
            return Assembly.Load(name);
        }

        public IEnumerable<Assembly> LoadedAssemblies
        {
            get
            {
                List<Assembly> assemblyList = new List<Assembly>();
                foreach (KeyValuePair<AssemblyNamespaceMapping, string> kvp in reverseDict)
                {
                    if (!assemblyList.Contains(kvp.Key.Assembly))
                        assemblyList.Add(kvp.Key.Assembly);
                }
                return assemblyList;
            }
        }

        /// <summary>
        /// Clones this XamlTypeFinder.
        /// </summary>
        public virtual XamlTypeFinder Clone()
        {
            XamlTypeFinder copy = new XamlTypeFinder();
            copy.ImportFrom(this);
            return copy;
        }

        /// <summary>
        /// Import information from another XamlTypeFinder.
        /// Use this if you override Clone().
        /// </summary>
        protected void ImportFrom(XamlTypeFinder source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            foreach (KeyValuePair<string, XamlNamespace> pair in source.namespaces)
            {
                this.namespaces.Add(pair.Key, pair.Value.Clone());
            }
            foreach (KeyValuePair<AssemblyNamespaceMapping, string> pair in source.reverseDict)
            {
                this.reverseDict.Add(pair.Key, pair.Value);
            }
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        /// <summary>
        /// Creates a new XamlTypeFinder where the WPF namespaces are registered.
        /// </summary>
        public static XamlTypeFinder CreateWpfTypeFinder()
        {
            //return WpfTypeFinder.Instance.Clone();
            return WpfTypeFinder.Instance;
        }

        static class WpfTypeFinder
        {
            internal static readonly XamlTypeFinder Instance;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline",
                                                             Justification = "We're using an explicit constructor to get it's lazy-loading semantics.")]
            static WpfTypeFinder()
            {
                Instance = new XamlTypeFinder();
                Instance.RegisterAssembly(typeof(MarkupExtension).Assembly); // WindowsBase
                Instance.RegisterAssembly(typeof(IAddChild).Assembly); // PresentationCore
                Instance.RegisterAssembly(typeof(XamlReader).Assembly); // PresentationFramework
            }
        }
    }
}
