// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACSerializer.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections;
using gip.core.datamodel;
using System.Reflection;

namespace gip.core.datamodel
{
    /// <summary>
    /// Serialisieren von beliebigen IACObjects
    /// </summary>
    public static class ACSerializer
    {
        #region Serialize
        /// <summary>
        /// Serializes the specified ac object.
        /// </summary>
        /// <param name="acObject">Dieses IACObject ist zu serialisieren</param>
        /// <param name="queryConfiguration">Definition welche Eigenschaften und Unter-ACObjects zu serialisieren sind</param>
        /// <returns>System.String.</returns>
        public static string Serialize(IACObject acObject, ACQueryDefinition queryConfiguration)
        {
            if (acObject == null)
                return "";

            // XML-Dokument erzeugen
            XElement xDoc = new XElement(acObject.GetType().Name);

            // Type-Attribut für die Haupt-IACObject
            xDoc.Add(new XAttribute("Type", acObject.GetType().FullName));

            // Url für das Haupt-IACObject
            xDoc.Add(new XAttribute("ACUrl", acObject.GetACUrl()));

            SerializeRecursive(xDoc, xDoc, acObject, queryConfiguration);

            return xDoc.ToString();
        }

        /// <summary>
        /// Serializes the recursive.
        /// </summary>
        /// <param name="xDoc">The x doc.</param>
        /// <param name="xNode">The x node.</param>
        /// <param name="acObject">The ac object.</param>
        /// <param name="queryConfiguration">The query configuration.</param>
        private static void SerializeRecursive(XElement xDoc, XElement xNode, IACObject acObject, ACQueryDefinition queryConfiguration)
        {
            foreach (var acColumn in queryConfiguration.ACColumns)
            {
                var value = acObject.ACUrlCommand(acColumn.PropertyName);
                string valueString = GetValueString(value);
                if (valueString != null)
                {
                    xNode.Add(new XElement(acColumn.PropertyName, valueString));
                }
                else
                {
                    IACType acTypeInfo = null;
                    object source = "";
                    string path = "";
                    Global.ControlModes rightControlMode = Global.ControlModes.Disabled;
                    if (acObject.ACUrlBinding(acColumn.PropertyName, ref acTypeInfo, ref source, ref path, ref rightControlMode))
                    {
                        ACClassProperty acClassProperty = acTypeInfo as ACClassProperty;
                        if (acClassProperty.IsEnumerable)
                        {
                            if (queryConfiguration.QueryType.ObjectType == acTypeInfo.ValueTypeACClass.ObjectType)
                            {
                                ACClass typeAsACClass = queryConfiguration.TypeAsACClass;
                                if (typeAsACClass != null)
                                {
                                    var query = typeAsACClass.Properties.Where(c => c.ACIdentifier == acColumn.PropertyName);
                                    if (query.Any())
                                    {
                                        var query1 = value as IEnumerable;

                                        XElement nodeElement = new XElement(acColumn.PropertyName);
                                        nodeElement.Add(new XAttribute("Type", query.First().ObjectFullType));

                                        xNode.Add(nodeElement);
                                        foreach (var valueItem in query1)
                                        {
                                            // XML-Dokument erzeugen
                                            XElement xNodeChild = new XElement(valueItem.GetType().Name);

                                            // Type-Attribut für die Haupt-IACObject
                                            xNodeChild.Add(new XAttribute("Type", valueItem.GetType().FullName));

                                            // Url für das Haupt-IACObject
                                            xNodeChild.Add(new XAttribute("ACUrl", ((IACObject)valueItem).GetACUrl()));

                                            SerializeRecursive(xDoc, xNodeChild, ((IACObject)valueItem), queryConfiguration);
                                            nodeElement.Add(xNodeChild);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                switch (acTypeInfo.ValueTypeACClass.ACKind)
                                {
                                    case Global.ACKinds.TACLRBaseTypes:
                                        {
                                            XElement nodeElement = new XElement(acColumn.PropertyName);
                                            xNode.Add(nodeElement);
                                            var query = value as IEnumerable;
                                            foreach (var valueItem in query)
                                            {
                                                nodeElement.Add(new XElement(Const.Value, GetValueString(valueItem)));
                                            }
                                        }
                                        break;
                                    case Global.ACKinds.TACSimpleClass:
                                        {
                                            ACClass typeAsACClass = queryConfiguration.TypeAsACClass;
                                            if (typeAsACClass != null)
                                            {
                                                var query = typeAsACClass.Properties.Where(c => c.ACIdentifier == acColumn.PropertyName);
                                                if (query.Any())
                                                {
                                                    ACClass valueTypeACClass = query.First().ValueTypeACClass;

                                                    XElement nodeElement = new XElement(acColumn.PropertyName);
                                                    nodeElement.Add(new XAttribute("Type", query.First().ObjectFullType));

                                                    xNode.Add(nodeElement);
                                                    var query1 = value as IEnumerable;
                                                    foreach (var valueItem in query1)
                                                    {
                                                        SerializeSimpleClass(xDoc, nodeElement, valueItem, valueTypeACClass);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    //    case Global.ACKinds.TACInterface:
                                }
                            }
                        }
                        else
                        {
                            switch (acTypeInfo.ValueTypeACClass.ACKind)
                            {
                                case Global.ACKinds.TACSimpleClass:
                                    {
                                        ACClass typeAsACClass = queryConfiguration.TypeAsACClass;
                                        if (typeAsACClass != null)
                                        {
                                            var query = typeAsACClass.Properties.Where(c => c.ACIdentifier == acColumn.PropertyName);
                                            if (query.Any())
                                            {
                                                ACClass valueTypeACClass = query.First().ValueTypeACClass;
                                                XElement nodeElement = new XElement(acColumn.PropertyName);
                                                xNode.Add(nodeElement);
                                                SerializeSimpleClass(xDoc, nodeElement, value, valueTypeACClass);
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Serializes the type of the simple.
        /// </summary>
        /// <param name="xDoc">The x doc.</param>
        /// <param name="xNode">The x node.</param>
        /// <param name="value">The value.</param>
        private static void SerializeSimpleType(XElement xDoc, XElement xNode, object value)
        {
            xNode.Add(new XElement(Const.Value, GetValueString(value)));
        }

        /// <summary>
        /// Serializes the simple class.
        /// </summary>
        /// <param name="xDoc">The x doc.</param>
        /// <param name="xNode">The x node.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueTypeACClass">The value type AC class.</param>
        private static void SerializeSimpleClass(XElement xDoc, XElement xNode, object value, ACClass valueTypeACClass)
        {
            XElement nodeElement = new XElement(valueTypeACClass.ACIdentifier);
            xNode.Add(nodeElement);

            Type t = value.GetType();
            List<ACColumnItem> acColumns = valueTypeACClass.GetColumns();

            foreach (var acColumn in acColumns)
            {
                PropertyInfo pi = t.GetProperty(acColumn.PropertyName);
                var columnValue = pi.GetValue(value, null);
                nodeElement.Add(new XElement(acColumn.PropertyName, GetValueString(columnValue)));
            }
        }
        #endregion

        #region Deserialize
        /// <summary>
        /// Deserializes the specified ac object.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <param name="acQueryDefinition">The ac query definition.</param>
        /// <param name="acXML">The ac XML.</param>
        /// <returns>IACObject.</returns>
        public static IACObject Deserialize(IACObject acObject, ACQueryDefinition acQueryDefinition, string acXML)
        {
            if (string.IsNullOrEmpty(acXML))
                return acObject;
            XElement xDoc = XElement.Parse(acXML);

            var typeValue = xDoc.Attribute("Type").Value;
            var acUrlValue = xDoc.Attribute("ACUrl").Value;

            //if (acObject.ACIdentifier != acUrlValue)
            //{
            //    return acObject;
            //}
            DeserializeRecursive(xDoc, xDoc, acObject, acQueryDefinition);

            return acObject;
        }

        /// <summary>
        /// Deserializes the recursive.
        /// </summary>
        /// <param name="xDoc">The x doc.</param>
        /// <param name="xNode">The x node.</param>
        /// <param name="acObject">The ac object.</param>
        /// <param name="queryConfiguration">The query configuration.</param>
        private static void DeserializeRecursive(XElement xDoc, XElement xNode, IACObject acObject, ACQueryDefinition queryConfiguration)
        {
            Type typeACObject = acObject.GetType();

            foreach (var acColumn in queryConfiguration.ACColumns)
            {
                PropertyInfo pi = typeACObject.GetProperty(acColumn.PropertyName);

                XElement xProperty = xNode.Element(pi.Name);
                if (xProperty == null) // Nicht im XML enthalten, dann nicht setzen
                    continue;

                bool isEnumerable = false;
                if (pi.PropertyType.IsGenericType)
                    isEnumerable = TypeAnalyser.IsEnumerable(pi.PropertyType.GetGenericTypeDefinition().FullName);
                if (!isEnumerable)
                {
                    SetValue(acObject, pi, xProperty.Value);
                }
                else
                {
                    IACType acTypeInfo = null;
                    object source = "";
                    string path = "";
                    Global.ControlModes rightControlMode = Global.ControlModes.Disabled;
                    if (acObject.ACUrlBinding(acColumn.PropertyName, ref acTypeInfo, ref source, ref path, ref rightControlMode))
                    {
                        if (queryConfiguration.QueryType.ObjectType == acTypeInfo.ValueTypeACClass.ObjectType)
                        {
                            IEnumerable childs = acObject.ACUrlCommand(acColumn.PropertyName,null) as IEnumerable;
                            foreach (XElement xmlChild in xProperty.Nodes().Where(c => c is XElement).Select(c => c as XElement))
                            {
                                var typeValue = xmlChild.Attribute("Type").Value;
                                var acUrlValue = xmlChild.Attribute("ACUrl").Value;

                                foreach (var child in childs)
                                {
                                    IACObject acObjectChild = child as IACObject;
                                    if (acObjectChild.ACIdentifier == acUrlValue)
                                    {
                                        DeserializeRecursive(xDoc, xmlChild, acObjectChild, queryConfiguration);
                                    }
                                }
                            }
                        }
                        else
                        {
                            switch (acTypeInfo.ValueTypeACClass.ACKind)
                            {
                                case Global.ACKinds.TACLRBaseTypes:
                                    {
                                        IList list = acObject.ACUrlCommand(acColumn.PropertyName) as IList;
                                        list.Clear();

                                        Type t = acTypeInfo.ValueTypeACClass.ObjectType;

                                        foreach (XElement xmlChild in xProperty.Nodes().Where(c => c is XElement).Select(c => c as XElement))
                                        {
                                            list.Add(GetValueObject(t, xmlChild.Value));
                                        }
                                    }
                                    break;
                                case Global.ACKinds.TACSimpleClass:
                                    {
                                        IList list = acObject.ACUrlCommand(acColumn.PropertyName) as IList;
                                        list.Clear();

                                        foreach (XElement xmlChild in xProperty.Nodes().Where(c => c is XElement).Select(c => c as XElement))
                                        {
                                            Type t = acTypeInfo.ValueTypeACClass.ObjectType;
                                            object childObject = Activator.CreateInstance(t);

                                            List<ACColumnItem> acColumns = acTypeInfo.ValueTypeACClass.GetColumns();

                                            foreach (var acColumn1 in acColumns)
                                            {
                                                XElement xProperty1 = xmlChild.Element(acColumn1.PropertyName);
                                                if (xProperty1 != null)
                                                {
                                                    PropertyInfo pi1 = t.GetProperty(acColumn1.PropertyName);
                                                    SetValue(childObject, pi1, xProperty1.Value);
                                                }
                                            }
                                            list.Add(childObject);
                                        }
                                    }
                                    break;
                                //    case Global.ACKinds.TACInterface:
                            }
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Gets the value string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        private static string GetValueString(object value)
        {
            if (value == null)
            {
                return "i:NULL";
            }
            Type objType = value.GetType();
            if (objType.IsPrimitive || objType.IsEnum)
            {
                return value.ToString();
            }
            else if (objType == typeof(string))
            {
                return (string)value;
            }
            else if (objType == typeof(Guid))
            {
                return value.ToString();
            }
            else if (objType == typeof(DateTime))
            {
                return ((DateTime)value).ToString("s");
            }
            else if (value is IACObject)
            {
                return (value as IACObject).GetACUrl();
            }
            else if (value is Type)
            {
                return value.ToString();
            }
            return null;
        }

        /// <summary>
        /// Gets the value object.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="xValue">The x value.</param>
        /// <returns>System.Object.</returns>
        private static object GetValueObject(Type t, string xValue)
        {
            object childObject = Activator.CreateInstance(t);

            if (xValue == "i:NULL")
            {
                return null;
            }


            switch (t.FullName)
            {
                case TypeAnalyser._TypeName_Boolean:
                    return Convert.ToBoolean(xValue);
                case TypeAnalyser._TypeName_Double:
                    return Convert.ToDouble(xValue);
                case TypeAnalyser._TypeName_Int16:
                    return Convert.ToInt16(xValue);
                case TypeAnalyser._TypeName_Int32:
                    return Convert.ToInt32(xValue);
                case TypeAnalyser._TypeName_Single:
                    return Convert.ToSingle(xValue);
                case TypeAnalyser._TypeName_String:
                    return xValue;
                case TypeAnalyser._TypeName_DateTime:
                    return DateTime.Parse(xValue);
                case "System.Type":
                    return Type.GetType(xValue);
                default:
                    return Convert.ChangeType(xValue, t);
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="objectParent">The object parent.</param>
        /// <param name="pi">The pi.</param>
        /// <param name="xValue">The x value.</param>
        private static void SetValue(object objectParent, PropertyInfo pi, string xValue)
        {
            if (xValue == "i:NULL")
            {
                if (pi.GetValue(objectParent, null) != null)
                {
                    pi.SetValue(objectParent, null, null);
                }
                return;
            }
            if (pi.PropertyType.IsPrimitive)
            {
                switch (pi.PropertyType.FullName)
                {
                    case TypeAnalyser._TypeName_Boolean:
                        {
                            var typeValue = Convert.ToBoolean(xValue);
                            if ((Boolean)pi.GetValue(objectParent, null) != typeValue)
                            {
                                pi.SetValue(objectParent, typeValue, null);
                            }
                        }
                        return;
                    case TypeAnalyser._TypeName_Double:
                        {
                            var typeValue = Convert.ToDouble(xValue);
                            if ((Double)pi.GetValue(objectParent, null) != typeValue)
                            {
                                pi.SetValue(objectParent, typeValue, null);
                            }
                        }
                        return;
                    case TypeAnalyser._TypeName_Int16:
                        {
                            var typeValue = Convert.ToInt16(xValue);
                            if ((Int16)pi.GetValue(objectParent, null) != typeValue)
                            {
                                pi.SetValue(objectParent, typeValue, null);
                            }
                        }
                        return;
                    case TypeAnalyser._TypeName_Int32:
                        {
                            var typeValue = Convert.ToInt32(xValue);
                            if ((Int32)pi.GetValue(objectParent, null) != typeValue)
                            {
                                pi.SetValue(objectParent, typeValue, null);
                            }
                        }
                        return;
                    case TypeAnalyser._TypeName_Single:
                        {
                            var typeValue = Convert.ToSingle(xValue);
                            if ((Single)pi.GetValue(objectParent, null) != typeValue)
                            {
                                pi.SetValue(objectParent, typeValue, null);
                            }
                        }
                        return;
                    default:
                        {
                            var typeValue = Convert.ChangeType(xValue, pi.PropertyType);
                            pi.SetValue(objectParent, typeValue, null);
                            return;
                        }
                }
            }
            else if (pi.PropertyType == typeof(string))
            {
                if ((string)pi.GetValue(objectParent, null) != xValue)
                {
                    pi.SetValue(objectParent, xValue, null);
                }
            }
            else if (pi.PropertyType == typeof(DateTime))
            {
                DateTime dateTime = DateTime.Parse(xValue);
                if ((DateTime)pi.GetValue(objectParent, null) != dateTime)
                {
                    pi.SetValue(objectParent, dateTime, null);
                }
            }
            else if (pi.PropertyType == typeof(Type))
            {
                var typeValue = Type.GetType(xValue);
                if ((Type)pi.GetValue(objectParent, null) != typeValue)
                {
                    pi.SetValue(objectParent, typeValue, null);
                }
            }
            else if (pi.PropertyType.IsEnum)
            {
                var enumValue = Enum.Parse(pi.PropertyType, xValue);
                if (pi.GetValue(objectParent, null) != enumValue)
                {
                    pi.SetValue(objectParent, enumValue, null);
                }
            }
            else if (pi.PropertyType.GetInterface("gip.core.datamodel.IACObject") != null)
            {
                //int i = 10;
                //if (xValue.StartsWith(Const.ContextDatabase + "\\"))
                //    xValue = xValue.Substring(9);

                //var entity = database.ACUrlCommand(xValue);
                //if (pi.GetValue(objectParent, null) != entity)
                //{
                //    pi.SetValue(objectParent, entity, null);
                //}
            }
        }
    }
}
