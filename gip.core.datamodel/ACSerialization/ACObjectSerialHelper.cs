// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    public static class ACObjectSerialHelper
    {
        #region Legacy-Support
        public const string C_LegacyRootProjectName = "ACProject(Variobatch)";
        public const string C_LegacyRootClassName = "ACClass(Variobatch)";
        public const string C_RootProjectName = "ACProject(" + Const.ACRootProjectName + ")";
        public const string C_RootClassName = "ACClass(" + Const.ACRootClassName + ")";
        public static string ReplaceLegacyNames(string val)
        {
            if (!String.IsNullOrEmpty(val))
            {
                val = val.Replace(C_LegacyRootProjectName, C_RootProjectName);
                val = val.Replace(C_LegacyRootClassName, C_RootClassName);
            }
            return val;
        }
        #endregion

        #region Type definition and Factory

        public static IACObject FactoryACObject(ACFSItem acFSParentItem, string acUrl)
        {
            if (acUrl.StartsWith(Const.ContextDatabase + "\\"))
                acUrl = acUrl.Substring(9);
            acUrl = ReplaceLegacyNames(acUrl);
            IACObject acObject = null;
            try
            {
                acObject = acFSParentItem.Container.ACUrlCommandCached(acUrl) as IACObject;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACObjectHelper", "FactoryACObject", msg);
            }

            if (acObject == null)
            {
                List<string> urlParts = new List<string>();
                string tmp = "";
                //bool inBraces = false;
                int inBracesCount = 0;
                foreach (char element in acUrl)
                {
                    if (element == '(')
                        inBracesCount++;
                    if (element == '\\' && inBracesCount == 0)
                    {
                        urlParts.Add(tmp);
                        tmp = "";
                    }
                    else
                    {
                        tmp += element;
                    }

                    if (element == ')')
                        inBracesCount--;
                }
                urlParts.Add(tmp);

                if (acFSParentItem.ACObject != null)
                {
                    acObject = acFSParentItem.ACObject.ACUrlCommand(acUrl) as IACObject;
                    if (acObject == null)
                    {
                        acObject = acFSParentItem.ACObject.ACUrlCommand(ACUrlHelper.Delimiter_Start + urlParts.Last()) as IACObject;
                        if (acObject is VBEntityObject 
                            && (acObject as VBEntityObject).EntityState == Microsoft.EntityFrameworkCore.EntityState.Detached
                            && acFSParentItem.Container != null && acFSParentItem.Container.DB != null)
                        {
                            acFSParentItem.Container.DB.Add(acObject);
                        }
                    }
                }
                else
                {
                    #region URL parts
                    if (acObject == null && acUrl.Contains("\\"))
                    {
                        IACObject tmpIACObject = null;
                        string acUrlExists = "";
                        IACObject tmpParentIACObject = null;
                        string delimiter = "";
                        for (int i = 0; i < urlParts.Count(); i++)
                        {
                            acUrlExists += delimiter + urlParts[i];
                            if (acUrlExists == urlParts[0])
                            {
                                delimiter = "\\";
                                tmpIACObject = acFSParentItem.Container.ACUrlCommandCached(urlParts[i]) as IACObject;
                            }
                            else
                            {
                                tmpIACObject = tmpParentIACObject.ACUrlCommand(urlParts[i]) as IACObject;
                            }

                            if (tmpIACObject == null)
                            {
                                ACFSItem tmpItemWithSameURL = acFSParentItem.Container.Stack.FirstOrDefault(x => x.ACObjectACUrl != null && x.ACObjectACUrl == Const.ContextDatabase + "\\" + acUrlExists);
                                if (tmpItemWithSameURL != null)
                                    tmpIACObject = tmpItemWithSameURL.ACObject;
                            }

                            if (urlParts[i] == urlParts.Last())
                            {
                                if (tmpIACObject == null)
                                {
                                    tmpIACObject = tmpParentIACObject.ACUrlCommand(ACUrlHelper.Delimiter_Start + urlParts.Last()) as IACObject;
                                    if (tmpIACObject is VBEntityObject
                                        && (tmpIACObject as VBEntityObject).EntityState == Microsoft.EntityFrameworkCore.EntityState.Detached
                                        && acFSParentItem.Container != null && acFSParentItem.Container.DB != null)
                                    {
                                        acFSParentItem.Container.DB.Add(tmpIACObject);
                                    }
                                }

                            }
                            if (tmpIACObject == null)
                            {
                                tmpIACObject = acFSParentItem.Container.DB.ACUrlCommand(urlParts[i]) as IACObject;
                            }
                            if (tmpIACObject == null) 
                                break;
                            tmpParentIACObject = tmpIACObject;
                        }
                        acObject = tmpIACObject;
                    }
                    #endregion
                }

                if (acObject == null)
                {
                    IACObject tmpNewObject = acFSParentItem.Container.DB.ACUrlCommand(acUrl) as IACObject;
                    if (tmpNewObject != null) // && (tmpNewObject is ACProject || tmpNewObject is ACClass))
                        acObject = tmpNewObject;
                    if (acObject != null && !acFSParentItem.Container.CachedIACObjects.Keys.Contains(acUrl))
                        acFSParentItem.Container.CachedIACObjects.Add(acUrl, acObject);
                }
            }


            return acObject;
        }

        static Type _TypeGuid1 = typeof(Guid);
        static Type _TypeGuid2 = typeof(Guid?);
        static Type _TypeString = typeof(string);
        static Type _TypeIACObject = typeof(IACObject); // Type that can resolve ACUrl
        static Type _TypeContext = typeof(IACEntityObjectContext);

        public static bool PropertyForIgnore(PropertyInfo pi)
        {
            return
                !pi.CanWrite
                || pi.PropertyType == _TypeGuid1
                || pi.PropertyType == _TypeGuid2
                || !(pi.PropertyType.IsValueType || pi.PropertyType == _TypeString || (_TypeIACObject.IsAssignableFrom(pi.PropertyType) && !_TypeContext.IsAssignableFrom(pi.PropertyType)))
                || (pi.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() != null
                    && pi.GetCustomAttribute<ACPropertyBase>()?.SortIndex >= 10000);
        }

#endregion

#region Set property

        public static List<ACObjectSerialPropertyContainerModel> GetPropertyList(IACObject acObject, bool? onlyKeyMembers)
        {
            List<ACObjectSerialPropertyContainerModel> list = new List<ACObjectSerialPropertyContainerModel>();
            Type typeOfObject = acObject.GetType();
            PropertyInfo[] properties = typeOfObject.GetProperties().ToArray();

            properties = properties.Where(pi => !ACObjectSerialHelper.PropertyForIgnore(pi)).ToArray();

            PropertyInfo keyMemberPropertyInfo = typeOfObject.GetProperty(nameof(ACClass.KeyACIdentifier));
            string dataIdentifierStr = keyMemberPropertyInfo?.GetValue(acObject.GetType(), null) as string;
            string[] dataIdentifiers = dataIdentifierStr?.Split(',');

            if (onlyKeyMembers != null && keyMemberPropertyInfo != null && dataIdentifiers != null)
            {
                // Setup key members
                List<string> keyMembers = new List<string>();
                foreach (string dataIdentifier in dataIdentifiers)
                {
                    if (dataIdentifier.Contains("\\"))
                        keyMembers.Add(dataIdentifier.Substring(0, dataIdentifier.IndexOf("\\")));
                    else
                        keyMembers.Add(dataIdentifier);
                }

                if (onlyKeyMembers.Value)
                    properties = properties.Where(x => keyMembers.Contains(x.Name)).ToArray();
                else
                    properties = properties.Where(x => !keyMembers.Contains(x.Name)).ToArray();
            }

            foreach (PropertyInfo pi in properties)
            {
                ACObjectSerialPropertyHandlingTypesEnum acObjectPropertyType = GetACObjectPropertyHandlingType(pi, acObject);
                if (!list.Exists(x => x.PropertyType == acObjectPropertyType))
                {
                    list.Add(new ACObjectSerialPropertyContainerModel()
                    {
                        PropertyType = acObjectPropertyType,
                        Properties = new List<PropertyInfo>()
                    });
                }
                list.FirstOrDefault(x => x.PropertyType == acObjectPropertyType).Properties.Add(pi);
            }
            return list.OrderBy(x => x.PropertyType).ToList();
        }

        public static ACObjectSerialPropertyHandlingTypesEnum GetACObjectPropertyHandlingType(PropertyInfo pi, IACObject acObject)
        {
            ACObjectSerialPropertyHandlingTypesEnum acObjectPropertyType = ACObjectSerialPropertyHandlingTypesEnum.Primitive;
            if (pi.PropertyType == typeof(string))
            {
                acObjectPropertyType = ACObjectSerialPropertyHandlingTypesEnum.String;
            }
            else if (pi.PropertyType == typeof(DateTime))
            {
                acObjectPropertyType = ACObjectSerialPropertyHandlingTypesEnum.DateTime;
            }
            else if (pi.PropertyType == typeof(System.Byte[]) && acObject is ACClassDesign)
            {
                acObjectPropertyType = ACObjectSerialPropertyHandlingTypesEnum.ACClassDesignByte;
            }
            else if (_TypeIACObject.IsAssignableFrom(pi.PropertyType))
            {
                acObjectPropertyType = ACObjectSerialPropertyHandlingTypesEnum.IACObject;
            }
            return acObjectPropertyType;
        }

        /// <summary>
        /// Set property from XML
        /// </summary>
        /// <param name="db"></param>
        /// <param name="resource"></param>
        /// <param name="container"></param>
        /// <param name="acObject"></param>
        /// <param name="pi"></param>
        /// <param name="acObjectPropertyType"></param>
        /// <param name="xValue"></param>
        /// <returns></returns>
        public static ACFSItemChanges SetProperty(IACEntityObjectContext db, IResources resource, ACFSItemContainer container, IACObject acObject, PropertyInfo pi, ACObjectSerialPropertyHandlingTypesEnum acObjectPropertyType, string xValue)
        {
            ACFSItemChanges changesItem = null;
            if (xValue == null || xValue == "i:NULL")
            {
                if (pi.GetValue(acObject, null) != null)
                {
                    changesItem = new ACFSItemChanges(pi.Name, pi.GetValue(acObject, null), null);
                    pi.SetValue(acObject, null, null);
                }
            }
            else
            {
                switch (acObjectPropertyType)
                {
                    case ACObjectSerialPropertyHandlingTypesEnum.Primitive:
                        Type tmpType = pi.PropertyType;
                        if (Nullable.GetUnderlyingType(tmpType) != null)
                        {
                            tmpType = Nullable.GetUnderlyingType(tmpType);
                        }
                        var typeValue = Convert.ChangeType(xValue, tmpType);
                        changesItem = new ACFSItemChanges(pi.Name, pi.GetValue(acObject, null), typeValue);
                        pi.SetValue(acObject, typeValue, null);
                        break;
                    case ACObjectSerialPropertyHandlingTypesEnum.String:
                        string old = "";
                        if ((string)pi.GetValue(acObject, null) != xValue)
                        {
                            old = (string)pi.GetValue(acObject, null);
                            if (old != null)
                            {
                                old = old.Replace("\r\n", "\n").Trim();
                            }
                            if (xValue != null)
                                xValue = xValue.Trim();
                            if (old != xValue)
                            {
                                pi.SetValue(acObject, xValue, null);
                            }
                        }
                        changesItem = new ACFSItemChanges(pi.Name, old, xValue);
                        break;
                    case ACObjectSerialPropertyHandlingTypesEnum.DateTime:
                        DateTime newDateValue = DateTime.Parse(xValue);
                        DateTime oldDateValue = (DateTime)pi.GetValue(acObject, null);
                        if (oldDateValue != newDateValue)
                        {
                            pi.SetValue(acObject, newDateValue, null);
                        }
                        changesItem = new ACFSItemChanges(pi.Name, oldDateValue, newDateValue);
                        break;
                    case ACObjectSerialPropertyHandlingTypesEnum.ACClassDesignByte:
                        ACClassDesign acClassDesign = acObject as ACClassDesign;
                        int indexProject = xValue.IndexOf("\\" + ACProject.ClassName);
                        var paths = xValue.Substring(0, indexProject).Split('\\');
                        xValue = xValue.Remove(0, indexProject + 1).Insert(0, System.IO.Path.GetTempPath() + paths.LastOrDefault() + "\\");
                        byte[] newValue = resource.ReadBinary(xValue);
                        if (newValue != null && acClassDesign != null)
                        {
                            changesItem = new ACFSItemChanges("DesignBinary", string.Format("binary[{0}]", acClassDesign.DesignBinary != null ? acClassDesign.DesignBinary.Length : 0), string.Format("binary[{0}]", newValue.Length));
                            acClassDesign.DesignBinary = newValue;
                        }
                        break;
                    case ACObjectSerialPropertyHandlingTypesEnum.IACObject:
                        if (xValue.StartsWith(Const.ContextDatabase + "\\"))
                            xValue = xValue.Substring(9);

                        var entity = container.ACUrlCommandCached(xValue);
                        if (entity == null)
                        {
                            var testStr = container.Stack.Where(x => x.ACObject != null).Select(x => x.ACObjectACUrl).ToList();
                            if (container.Stack.Any(x => x.ACObjectACUrl == (Const.ContextDatabase + "\\" + xValue)))
                                entity = container.Stack.FirstOrDefault(x => x.ACObjectACUrl == (Const.ContextDatabase + "\\" + xValue)).ACObject;
                        }
                        if (entity == null)
                        {
                            entity = db.ACUrlCommand(xValue) as IACObject;
                        }
                        if (entity == null)
                        {
                            IACObject vOld = pi.GetValue(acObject, null) as IACObject;
                            string oldValue = "i:NULL";
                            if (vOld != null)
                                oldValue = vOld.GetACUrl();

                            ACDummyValues dummyValues = new ACDummyValues(container.DB);
                            changesItem = dummyValues.SetDummyValue(container.DB, pi, acObject);
                            if (changesItem == null)
                            {
                                if (pi.PropertyType.Name.StartsWith("MD") && xValue.IndexOf('\\') == -1)
                                {
                                    string acNameInstance = ACUrlHelper.ExtractInstanceName(xValue);
                                    entity = container.DB.ACUrlCommand("#" + pi.PropertyType.Name, new object[] { acNameInstance }) as IACObject;
                                    changesItem = new ACFSItemChanges(pi.Name, (pi.GetValue(acObject, null) as IACObject).GetACUrl(), (entity as IACObject).GetACUrl());
                                    pi.SetValue(acObject, entity, null);
                                }
                            }
                        }
                        else
                        {
                            string oldACUrl = "";
                            string newACUrl = "";
                            if(acObject != null)
                            {
                                IACObject oldIACObject = pi.GetValue(acObject, null) as IACObject;
                                if (oldIACObject != null)
                                    oldACUrl = (pi.GetValue(acObject, null) as IACObject).GetACUrl();
                            }
                            if(entity != null)
                            {
                                IACObject newIACObject = entity as IACObject;
                                if (newIACObject != null)
                                    newACUrl = (entity as IACObject).GetACUrl();
                            }
                            changesItem = new ACFSItemChanges(pi.Name, oldACUrl, newACUrl);
                            if (pi.GetValue(acObject, null) != entity)
                            {
                                pi.SetValue(acObject, entity, null);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            return changesItem;
        }

        public static void SetIACObjectProperties(IResources resource, IACEntityObjectContext db, ACFSItemContainer container, ACFSItem acFsItem, bool? setupKeyACIdentifier, List<Msg> msgList)
        {
            ACFSItemChanges itemChange = null;
            List<ACObjectSerialPropertyContainerModel> acObjectPropertyModelList = ACObjectSerialHelper.GetPropertyList(acFsItem.ACObject, setupKeyACIdentifier);
            foreach (ACObjectSerialPropertyContainerModel acObjectPropertyModel in acObjectPropertyModelList)
            {
                foreach (PropertyInfo pi in acObjectPropertyModel.Properties)
                {
                    KeyValuePair<bool, string> propertyValue = acFsItem.ReadPropertyValue(pi, acObjectPropertyModel.PropertyType);
                    if (!propertyValue.Key) continue; // Continue for not defined values in XML (but in property list)
                    string xValue = propertyValue.Value;
                    try
                    {
                        itemChange = ACObjectSerialHelper.SetProperty(db, resource, container, acFsItem.ACObject, pi, acObjectPropertyModel.PropertyType, xValue);
                        if (itemChange != null)
                            acFsItem.AddItemChange(itemChange);
                        //else
                        //    System.Diagnostics.Debugger.Break();
                    }
                    catch (Exception ec)
                    {
                        Msg msg = new Msg()
                        {
                            MessageLevel = eMsgLevel.Error,
                            Message = string.Format("Unable set property {0} for {1}! Exception: {2}", pi.Name, acFsItem.ACObjectACUrl, ec.Message)
                        };

                        if (msgList != null)
                            msgList.Add(msg);
                        throw (new Exception(msg.Message, ec));
                    }
                }
            }
        }

#endregion

    }
}
