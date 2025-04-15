// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACCrypt.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Schema;

namespace gip.core.datamodel
{
    public class XmlHelper
    {
        public static string XmlSchemaUrl = "http://www.w3.org/2001/XMLSchema.dtd";

        public static XmlElement CreateNodeIn(XmlDocument document, string nodeName, string nameSpace, XmlElement rootNode)
        {
            var result = document.CreateElement(nodeName, nameSpace);
            rootNode.AppendChild(result);
            return result;
        }
        public static XmlElement CreateNodeWithTextIn(XmlDocument document, string nodeName, string text, string nameSpace, XmlElement rootNode)
        {
            var newNode = CreateNodeIn(document, nodeName, nameSpace, rootNode);
            newNode.InnerText = text;
            return newNode;
        }

        public static string DateTimeToCanonicalRepresentation(DateTime ahora)
        {
            return ahora.Year.ToString("0000") + "-" + ahora.Month.ToString("00") +
                   "-" + ahora.Day.ToString("00") +
                   "T" + ahora.Hour.ToString("00") + ":" +
                   ahora.Minute.ToString("00") + ":" + ahora.Second.ToString("00") +
                   "Z";
        }
        public static string NowInCanonicalRepresentation()
        {
            var now = DateTime.Now.ToUniversalTime();
            return DateTimeToCanonicalRepresentation(now);
        }

        public static XmlElement FindNodeWithAttributeValueIn(XmlNodeList nodeList, string attributeName, string value)
        {
            if (nodeList.Count == 0) return null;
            foreach (XmlNode node in nodeList)
            {
                var nodeWithSameId = FindNodeWithAttributeValueIn(node, attributeName, value);
                if (nodeWithSameId != null) return nodeWithSameId;
            }
            return null;
        }

        private static XmlElement FindNodeWithAttributeValueIn(XmlNode node, string attributeName, string value)
        {
            var attributeValueInNode = GetAttributeValueInNodeOrNull(node, attributeName);
            if ((attributeValueInNode != null) && (attributeValueInNode.Equals(value))) return (XmlElement)node;
            return FindNodeWithAttributeValueIn(node.ChildNodes, attributeName, value);
        }

        private static string GetAttributeValueInNodeOrNull(XmlNode node, string attributeName)
        {
            var xmlAttributeCollection = node.Attributes;
            if (xmlAttributeCollection != null)
            {
                var attribute = xmlAttributeCollection[attributeName];
                if (attribute != null) return attribute.Value;
            }
            return null;
        }

        public static XmlElement DescendantWith(XmlElement rootNode, Predicate<XmlElement> conditionToComply)
        {
            foreach (var childNode in rootNode.ChildNodes)
            {
                if (!(childNode is XmlElement)) continue;
                var xmlElement = (XmlElement)childNode;
                if (conditionToComply(xmlElement))
                {
                    return xmlElement;
                }
            }
            return null;
        }

        public static List<XmlElement> DescendantsWith(XmlElement rootNode, Predicate<XmlElement> conditionToComply)
        {
            var results = new List<XmlElement>();
            foreach (var childNode in rootNode.ChildNodes)
            {
                if (!(childNode is XmlElement)) continue;
                var xmlElement = (XmlElement)childNode;
                if (conditionToComply(xmlElement))
                {
                    results.Add(xmlElement);
                }
            }
            return results;
        }

        public static List<XmlElement> FindNodesIn(XmlElement rootNode, string path)
        {
            var results = new List<XmlElement>();
            if (string.IsNullOrEmpty(path)) return results;
            if (!path.Contains("/")) return DescendantsWith(rootNode, n => path.Equals(n.LocalName));
            var token = path.Split('/')[0];
            var descendantsWithNameCorrect = DescendantsWith(rootNode, n => token.Equals(n.LocalName));
            if (descendantsWithNameCorrect.Count == 0) return results;
            foreach (var xmlElement in descendantsWithNameCorrect)
            {
                results.AddRange(FindNodesIn(xmlElement, path.Substring(token.Length + 1)));
            }
            return results;
        }

        public static XmlDocument ReadXmlFromUri(string uri)
        {
            var results = new XmlDocument();
            var reader = new XmlTextReader(uri);
            results.Load(reader);
            return results;
        }

        public static string AttributeOf(XmlElement node, string attributeName)
        {
            var xmlAttribute = node.Attributes[attributeName];
            return xmlAttribute == null ? null : xmlAttribute.Value;
        }
    }
}
