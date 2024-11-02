// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACxmlnsResolver.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Xml.Schema;
using gip.core.datamodel;
using System.Reflection;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACxmlnsInfo
    /// </summary>
    public class ACxmlnsInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACxmlnsInfo"/> class.
        /// </summary>
        /// <param name="xmlNameSpace">The XML name space.</param>
        /// <param name="clrNamespace">The CLR namespace.</param>
        /// <param name="prefix">The prefix.</param>
        public ACxmlnsInfo(string xmlNameSpace, string clrNamespace="", string prefix="") 
            : this (null, xmlNameSpace, clrNamespace, prefix)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACxmlnsInfo"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="xmlNameSpace">The XML name space.</param>
        /// <param name="clrNamespace">The CLR namespace.</param>
        /// <param name="prefix">The prefix.</param>
        public ACxmlnsInfo(Assembly assembly, string xmlNameSpace, string clrNamespace="", string prefix="")
        {
            _Assembly = assembly;
            _XMLNameSpace = xmlNameSpace;
            _ClrNamespace = clrNamespace;
            _Prefix = prefix;
        }

        /// <summary>
        /// The _ assembly
        /// </summary>
        private Assembly _Assembly;
        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <value>The assembly.</value>
        public Assembly Assembly 
        {
            get
            {
                return _Assembly;
            }
        }

        /// <summary>
        /// The _ XML name space
        /// </summary>
        private string _XMLNameSpace = "";
        /// <summary>
        /// Gets the XML name space.
        /// </summary>
        /// <value>The XML name space.</value>
        public string XMLNameSpace
        {
            get
            {
                return _XMLNameSpace;
            }
        }

        /// <summary>
        /// The _ CLR namespace
        /// </summary>
        private string _ClrNamespace = "";
        /// <summary>
        /// Gets the CLR namespace.
        /// </summary>
        /// <value>The CLR namespace.</value>
        public string ClrNamespace
        {
            get
            {
                return _ClrNamespace;
            }
        }

        /// <summary>
        /// The _ prefix
        /// </summary>
        private string _Prefix = "";
        /// <summary>
        /// Gets the prefix.
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix 
        {
            get
            {
                return _Prefix;
            }
        }
    }

    /// <summary>
    /// Class ACxmlnsResolver
    /// </summary>
    public static class ACxmlnsResolver 
    {
        /// <summary>
        /// The _ namespace list
        /// </summary>
        static List<string> _NamespaceList = null;
        /// <summary>
        /// Gets the namespace list.
        /// </summary>
        /// <value>The namespace list.</value>
        public static List<string> NamespaceList
        {
            get
            {
                if (_NamespaceList == null)
                {
                    _NamespaceList = new List<string>();
                    foreach (KeyValuePair<string, ACxmlnsInfo> kvp in NamespacesDict)
                    {
                        string key = kvp.Key.Trim();
                        string entry = "";
                        if (String.IsNullOrEmpty(key))
                            entry = "xmlns=\"" + kvp.Value.XMLNameSpace + "\"";
                        else
                            entry = "xmlns:" + key + "=\"" + kvp.Value.XMLNameSpace + "\"";
                        _NamespaceList.Add(entry);
                    }
                }
                return _NamespaceList;
            }
        }

        /// <summary>
        /// Gets the XML namespaces.
        /// </summary>
        /// <value>The XML namespaces.</value>
        public static String XMLNamespaces
        {
            get
            {
                string xmlNameSpace = "";
                foreach (string nameSpace in ACxmlnsResolver.NamespaceList)
                {
                    xmlNameSpace += " " + nameSpace;
                }
                return xmlNameSpace;
            }
        }

        /// <summary>
        /// The _ namespace dict
        /// </summary>
        private static Dictionary<string, ACxmlnsInfo> _NamespaceDict = null;
        /// <summary>
        /// Gets the namespaces dict.
        /// </summary>
        /// <value>The namespaces dict.</value>
        public static Dictionary<string, ACxmlnsInfo> NamespacesDict
        {
            get
            {
                if (_NamespaceDict == null)
                {
                    _NamespaceDict = new Dictionary<string, ACxmlnsInfo>();
                    _NamespaceDict.Add(" ", new ACxmlnsInfo("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"));
                    _NamespaceDict.Add("x", new ACxmlnsInfo("http://schemas.microsoft.com/winfx/2006/xaml", "http://schemas.microsoft.com/winfx/2006/xaml"));
                }
                return _NamespaceDict;
            }
        }

        /// <summary>
        /// The assemblies
        /// </summary>
        public static readonly ICollection<Assembly> Assemblies = new List<Assembly>();


        /// <summary>
        /// The _ XML schema
        /// </summary>
        private static XmlSchema _XmlSchema = null;
        /// <summary>
        /// Gets the XML schema.
        /// </summary>
        /// <value>The XML schema.</value>
        public static XmlSchema XmlSchema
        {
            get
            {
                if (_XmlSchema == null)
                {
                    _XmlSchema = new XmlSchema();
                    foreach (KeyValuePair<string, ACxmlnsInfo> kvp in NamespacesDict)
                    {
                        string key = kvp.Key.Trim();
                        _XmlSchema.Namespaces.Add(key, kvp.Value.XMLNameSpace);
                    }
                }
                return _XmlSchema;
            }
        }

        /// <summary>
        /// The _x namespace WPF
        /// </summary>
        private static XNamespace _xNamespaceWPF = null;
        /// <summary>
        /// Gets the x namespace WPF.
        /// </summary>
        /// <value>The x namespace WPF.</value>
        public static XNamespace xNamespaceWPF
        {
            get
            {
                if (_xNamespaceWPF != null)
                    return _xNamespaceWPF;
                KeyValuePair<string, ACxmlnsInfo> nsX = ACxmlnsResolver.GetNamespaceInfo("http://schemas.microsoft.com/winfx/2006/xaml");
                if (nsX.Value == null)
                    return "";
                _xNamespaceWPF = nsX.Value.XMLNameSpace;
                return _xNamespaceWPF;
            }
        }

        /// <summary>
        /// Gets the namespace info.
        /// </summary>
        /// <param name="typeOfUIElement">The type of UI element.</param>
        /// <returns>KeyValuePair{System.StringACxmlnsInfo}.</returns>
        static public KeyValuePair<string, ACxmlnsInfo> GetNamespaceInfo(Type typeOfUIElement)
        {
            if (typeOfUIElement == null)
                return new KeyValuePair<string, ACxmlnsInfo>();
            return GetNamespaceInfo(typeOfUIElement.Namespace);
        }

        /// <summary>
        /// Gets the namespace info.
        /// </summary>
        /// <param name="clrNamespaceName">Name of the CLR namespace.</param>
        /// <returns>KeyValuePair{System.StringACxmlnsInfo}.</returns>
        static public KeyValuePair<string, ACxmlnsInfo> GetNamespaceInfo(string clrNamespaceName)
        {
            var query = NamespacesDict.Where(c => c.Value.ClrNamespace == clrNamespaceName);
            if (query == null || !query.Any())
                query = NamespacesDict.Where(c => c.Value.ClrNamespace.IndexOf(clrNamespaceName) >= 0);
            if (query == null || !query.Any())
                query = NamespacesDict.Where(c => c.Key == " "); // .NET-Element: http://schemas.microsoft.com/winfx/2006/xaml/presentation
            if (query == null || !query.Any())
                return new KeyValuePair<string, ACxmlnsInfo>();
            return query.First();
        }

    }
}
