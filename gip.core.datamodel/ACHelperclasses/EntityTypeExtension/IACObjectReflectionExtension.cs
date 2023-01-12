// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 12-14-2012
// ***********************************************************************
// <copyright file="IACObjectReflectionExtension.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using System.ComponentModel;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class IACObjectReflectionExtension
    /// </summary>
    public static class IACObjectReflectionExtension
    {
        /// <summary>
        /// Gets the type of the AC.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>IACType.</returns>
        public static IACType GetACType(this Type objectType)
        {
            return Database.GlobalDatabase.GetACType(objectType);
        }

        /// <summary>
        /// Reflects the type of the AC.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <returns>IACType.</returns>
        public static IACType ReflectACType(this IACObject reflectedObject)
        {
            return Database.GlobalDatabase.GetACType(reflectedObject.GetType());
        }

        /// <summary>
        /// Reflects the get AC type info.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <param name="acName">Name of the ac.</param>
        /// <returns>IACType.</returns>
        public static IACType ReflectGetACTypeInfo(this IACObject reflectedObject, string acName)
        {
            ACClass typeAsACClass = reflectedObject.ACType as ACClass;
            return typeAsACClass != null ? typeAsACClass.Properties.Where(c => c.ACIdentifier == acName).FirstOrDefault() : null;
        }

        /// <summary>
        /// Reflects the get AC URL.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <param name="rootACObject">The root AC object.</param>
        /// <returns>System.String.</returns>
        public static string ReflectGetACUrl(this IACObject reflectedObject, IACObject rootACObject)
        {
            if (rootACObject != null && rootACObject == reflectedObject.ParentACObject)
            {
                return ReflectGetACIdentifier(reflectedObject);
            }
            if (reflectedObject.ParentACObject != null)
            {
                return reflectedObject.ParentACObject.GetACUrl(rootACObject) + "\\" + ReflectGetACIdentifier(reflectedObject);
            }
            return Const.ContextDatabase + "\\" + ReflectGetACIdentifier(reflectedObject);
        }

        /// <summary>
        /// Reflects the get AC identifier.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <returns>System.String.</returns>
        public static string ReflectGetACIdentifier(this IACObject reflectedObject)
        {
            // aagincic COMMENT: @      in case when root enviroment is not started - for example testing with only DatabaseApp instance, 
            //                          then while ACType == null mehtod fail
            if (reflectedObject.ACType == null) return null;
            Type AssemblyType = reflectedObject.ACType.ObjectType;
            string acName = AssemblyType.Name;

            PropertyInfo pi = AssemblyType.GetProperty("KeyACIdentifier");
            if (pi != null)
            {
                string dataIdentifier = pi.GetValue(AssemblyType, null) as string;
                if (!string.IsNullOrEmpty(dataIdentifier))
                {
                    string[] dataIdentifiers = dataIdentifier.Split(',');
                    acName += "(";
                    bool first = true;
                    foreach (var identifier in dataIdentifiers)
                    {
                        if (!first)
                            acName += ",";
                        object result = reflectedObject.ACUrlCommand(identifier);
                        if (result != null)
                            acName += result.ToString();
                        first = false;
                    }
                    acName += ")";
                }
            }

            return acName;
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="reflectedObject">object where ACURLCommand should be applied</param>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public static object ReflectACUrlCommand(this IACObject reflectedObject, string acUrl, params Object[] acParameter)
        {
            if (string.IsNullOrEmpty(acUrl))
                return null;

            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    {
                        if (Database.Root.ACType == null)
                            return null;
                        return Database.Root.ACType.ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                    }
                case ACUrlHelper.UrlKeys.Child:
                    {

                        IACType acTypeInfo = reflectedObject.ReflectGetACTypeInfo(acUrlHelper.ACUrlPart);
                        if (acTypeInfo != null)
                        {
                            switch (acTypeInfo.ACKind)
                            {
                                case Global.ACKinds.PSProperty:
                                    {
                                        PropertyInfo pi = reflectedObject.GetType().GetProperty(acUrlHelper.ACUrlPart);
                                        if (pi == null)
                                            return null;
                                        if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                        {
                                            if (acParameter == null || !acParameter.Any())
                                            {
                                                return pi.GetValue(reflectedObject, null);
                                            }
                                            else
                                            {
                                                pi.SetValue(reflectedObject, acParameter[0], null);
                                                return null;
                                            }
                                        }
                                        else
                                        {
                                            var nextObject = pi.GetValue(reflectedObject, null);
                                            if (nextObject == null)
                                                return null;
                                            if (nextObject is IACObject)
                                            {
                                                return ((IACObject)nextObject).ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                                            }
                                            else
                                            {
                                                return reflectedObject.ReflectACUrlCommandAssembly(nextObject, acTypeInfo, acUrlHelper.NextACUrl, acParameter);
                                            }
                                        }
                                    }
                                case Global.ACKinds.PSPropertyExt:
                                    {
                                        if (!(reflectedObject is IACEntityProperty))
                                            return null;
                                        IACEntityProperty entityProperty = reflectedObject as IACEntityProperty;
                                        if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                        {
                                            if (acParameter == null || !acParameter.Any())
                                            {
                                                return entityProperty[acUrlHelper.ACUrlPart];
                                            }
                                            else
                                            {
                                                entityProperty[acUrlHelper.ACUrlPart] = acParameter[0];
                                                return null;
                                            }
                                        }
                                        else
                                        {
                                            object valueObject = entityProperty[acUrlHelper.ACUrlPart];
                                            if (valueObject == null)
                                                return null;
                                            if (valueObject is IACObject)
                                            {
                                                return ((IACObject)valueObject).ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                                            }
                                            else
                                            {
                                                return reflectedObject.ReflectACUrlCommandAssembly(valueObject, acTypeInfo, acUrlHelper.NextACUrl, acParameter);
                                            }
                                        }
                                    }
                                default:
                                    return null;
                            }
                        }
                        else
                        {
                            if (reflectedObject is IACObjectEntity)
                            {
                                string[] acParameterCopy;
                                if ((acParameter != null) && (acParameter.Any()))
                                {
                                    acParameterCopy = new string[acParameter.Count() + 1];
                                    acParameterCopy[0] = ACUrlHelper.ExtractInstanceName(acUrlHelper.ACUrlPart);
                                    for (int i = 0; i < acParameter.Count(); i++)
                                    {
                                        string value = acParameter[i] as string;
                                        acParameterCopy[i + 1] = value;
                                    }
                                }
                                else
                                {
                                    acParameterCopy = new string[1] { ACUrlHelper.ExtractInstanceName(acUrlHelper.ACUrlPart) };
                                }

                                var result = ((IACObjectEntity)reflectedObject).GetChildEntityObject(ACUrlHelper.ExtractTypeName(acUrlHelper.ACUrlPart), acParameterCopy);
                                if (result == null)
                                    return null;
                                if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                {
                                    return result;
                                }
                                return ((IACObject)result).ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                            }
                            else if (reflectedObject is ACGenericObject)
                            {
                                var query = (reflectedObject as ACGenericObject).ACObjectChilds.Where(c => c.ACIdentifier == acUrlHelper.ACUrlPart);
                                if (!query.Any())
                                    return null;
                                var result = query.First();
                                if (result == null)
                                    return null;
                                if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                {
                                    return result;
                                }
                                return ((IACObject)result).ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                case ACUrlHelper.UrlKeys.InvokeMethod:
                    {
                        switch (acUrlHelper.UrlType)
                        {
                            case ACUrlHelper.UrlTypes.QueryType:
                                return null;
                            default:
                                return ReflectExecuteACMethod(reflectedObject, acUrlHelper.ACUrlPart, acParameter);
                        }
                    }

                case ACUrlHelper.UrlKeys.Parent:
                    return reflectedObject.ParentACObject.ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                case ACUrlHelper.UrlKeys.Start:
                    {
                        if (!(reflectedObject is VBEntityObject) || !string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                            return null;

                        VBEntityObject entityObject = reflectedObject as VBEntityObject;

                        try
                        {
                            string acName = ACUrlHelper.ExtractTypeName(acUrlHelper.ACUrlPart);
                            object[] filterValues = ACUrlHelper.GetFilterValues(acUrlHelper.ACUrlPart);

                            Type objectType = null;
                            foreach (var context in ACObjectContextManager.NamespacesOfUsedContexts)
                            {
                                string typeName = context + "." + acName;
                                if (reflectedObject.GetType().FullName.StartsWith(context))
                                    objectType = TypeAnalyser.GetTypeInAssembly(typeName);
                                if (objectType != null)
                                    break;
                            }
                            if (objectType == null)
                                return null;

                            MethodInfo miNewACObject = objectType.GetMethod(Const.MN_NewACObject, Global.bfInvokeMethodStatic);
                            if (miNewACObject == null)
                                return null;
                            // Erzeugen einer neuen Datenbankentität. ParentACObject ist immer null

                            IACObject newACObject = null;
                            if (miNewACObject.GetParameters().Count() == 2)
                                newACObject = miNewACObject.Invoke(objectType, new object[] { entityObject.GetObjectContext(), reflectedObject }) as IACObject;
                            else if (miNewACObject.GetParameters().Count() == 3)
                                newACObject = miNewACObject.Invoke(objectType, new object[] { entityObject.GetObjectContext(), reflectedObject, "" }) as IACObject;
                            return newACObject;
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (Database.Root != null && Database.Root.Messages != null)
                                Database.Root.Messages.LogException("IACObjectReflectionExtension", "ReflectACUrlCommand", msg);

                            return null;
                        }
                    }
                case ACUrlHelper.UrlKeys.Stop:
                    {
                        if (reflectedObject is IACObjectEntity)
                            return (reflectedObject as IACObjectEntity).DeleteACObject((reflectedObject as VBEntityObject).GetObjectContext<Database>(), false);
                        return null;
                    }
                case ACUrlHelper.UrlKeys.TranslationText:
                    {
                        if (reflectedObject is IACObjectEntityWithCheckTrans)
                            return (reflectedObject as IACObjectEntityWithCheckTrans).GetTranslation(Translator.VBLanguageCode);
                        else if (reflectedObject is IACType)
                            return (reflectedObject as IACType).GetTranslation(Translator.VBLanguageCode);
                        return "No translationtext";
                    }
                default:
                    return null; // TODO: Fehlerbehandlung
            }
        }

        /// <summary>
        /// Reflects the is enabled AC URL command.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="acParameter">The ac parameter.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public static bool ReflectIsEnabledACUrlCommand(this IACObject reflectedObject, string acUrl, params Object[] acParameter)
        {
            if (string.IsNullOrEmpty(acUrl))
                return false;
            try
            {
                ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
                switch (acUrlHelper.UrlKey)
                {
                    case ACUrlHelper.UrlKeys.Root:
                        if (reflectedObject is IACInteractiveObject)
                            return reflectedObject.Root().IsEnabledACUrlCommand(acUrl.Substring(1), acParameter);
                        return false;
                    case ACUrlHelper.UrlKeys.Parent:
                        if (reflectedObject is IACComponent)
                            return ((IACComponent)reflectedObject).ParentACComponent.IsEnabledACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                        return false;
                    case ACUrlHelper.UrlKeys.InvokeMethod:
                        return ReflectIsEnabledExecuteACMethod(reflectedObject, acUrlHelper.ACUrlPart, acParameter);
                    case ACUrlHelper.UrlKeys.Stop:
                        {
                            if (reflectedObject is IACObjectEntity)
                                return (reflectedObject as IACObjectEntity).IsEnabledDeleteACObject((reflectedObject as VBEntityObject).GetObjectContext<Database>()) == null;
                            return false;
                        }
                    default:
                        return false; // TODO: Fehlerbehandlung
                }
            }
            catch (Exception ex)
            {
                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("IACObjectReflectionExtension", "ReflectIsEnabledACUrlCommand", ex);
                return false;
            }
        }

        /// <summary>
        /// Reflects the AC URL command assembly.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="entityACTypeInfo">The entity AC type info.</param>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="acParameter">The ac parameter.</param>
        /// <returns>System.Object.</returns>
        public static object ReflectACUrlCommandAssembly(this IACObject reflectedObject, object entity, IACType entityACTypeInfo, string acUrl, Object[] acParameter)
        {
            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Child:
                    {
                        Type t = entity.GetType();
                        PropertyInfo valueObject = t.GetProperty(acUrlHelper.ACUrlPart);
                        if (valueObject == null)
                            return null;
                        if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                        {
                            if (acParameter == null || !acParameter.Any())
                            {
                                return valueObject.GetValue(entity, null);
                            }
                            else
                            {
                                valueObject.SetValue(entity, acParameter[0], null);
                                return null;
                            }
                        }
                        else
                        {
                            if (valueObject is IACObject)
                            {
                                return ((IACObject)valueObject).ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                            }
                            else
                            {
                                IACType acTypeInfo = entityACTypeInfo.GetMember(acUrlHelper.ACUrlPart);
                                if (acTypeInfo == null)
                                    return null;
                                return reflectedObject.ReflectACUrlCommandAssembly(valueObject, acTypeInfo, acUrlHelper.NextACUrl, acParameter);
                            }
                        }
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reflects the execute AC method.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <param name="acMethodName">Name of the ac method.</param>
        /// <param name="acParameter">The ac parameter.</param>
        /// <returns>System.Object.</returns>
        public static object ReflectExecuteACMethod(this IACObject reflectedObject, string acMethodName, Object[] acParameter)
        {
            string acMethodName1;
            int pos = acMethodName.IndexOf('!');
            if (pos == 0)
                acMethodName1 = acMethodName.Substring(1);
            else
                acMethodName1 = acMethodName;

            ACClass ClassACType = reflectedObject.ACType as ACClass;
            if (ClassACType == null)
                return null;

            ACClassMethod acClassMethod = ClassACType.GetMethod(acMethodName1);

            // Es sind grundsätzlich nur Kommandos erlaubt, die
            // vorher registriert wurden
            if (acClassMethod == null)
            {
                return null;
            }
            switch (acClassMethod.ACKind)
            {
                case Global.ACKinds.MSMethod:
                case Global.ACKinds.MSMethodClient:
                case Global.ACKinds.MSMethodPrePost:
                    try
                    {
                        MethodInfo mi = reflectedObject.GetType().GetMethod(acMethodName1);
                        if (acClassMethod.ACKind == Global.ACKinds.MSMethodClient)
                        {
                            object[] acParamWithThis = new object[acParameter.Count() + 1];
                            acParamWithThis[0] = reflectedObject;
                            for (int i = 0; i < acParameter.Count(); i++)
                            {
                                acParamWithThis[i + 1] = acParameter[i];
                            }
                            acParameter = acParamWithThis;
                        }
                        return mi.Invoke(reflectedObject, acParameter);
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("IACObjectReflectionExtension", "ReflectExecuteACMethod", msg);
                        return null;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reflects the is enabled execute AC method.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <param name="acMethodName">Name of the ac method.</param>
        /// <param name="acParameter">The ac parameter.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public static bool ReflectIsEnabledExecuteACMethod(this IACObject reflectedObject, string acMethodName, Object[] acParameter)
        {
            string acMethodName1;
            int pos = acMethodName.IndexOf('!');
            if (pos == 0)
                acMethodName1 = acMethodName.Substring(1);
            else
                acMethodName1 = acMethodName;

            ACClass ClassACType = reflectedObject.ACType as ACClass;
            if (ClassACType == null)
                return false;

            ACClassMethod acClassMethod = ClassACType.GetMethod(acMethodName1);

            // Es sind grundsätzlich nur Kommandos erlaubt, die
            // vorher registriert wurden
            if (acClassMethod == null)
            {
                return false;
            }

            // Falls AutoEnabled, dann gibt es keine IsEnabled-Methode und das 
            // Komando ist immer verfügbar
            if (acClassMethod.IsAutoenabled)
            {
                return true;
            }
            // Abfragen eines Werts
            // Mögliche Fehlerquellen:
            // 1. Entsprechende IsEnabled-Methode ist nicht vorhanden
            // 2. Entsprechende IsEnabled-Methode ist nicht public deklariert
            try
            {
                MethodInfo mi = reflectedObject.GetType().GetMethod("IsEnabled" + acMethodName1);
                if (mi == null)
                    return false;
                return (bool)mi.Invoke(reflectedObject, acParameter);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("IACObjectReflectionExtension", "ReflectIsEnabledExecuteACMethod", msg);
                return false;
            }
        }

        /// <summary>
        /// Reflects the AC URL binding.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="acTypeInfo">The ac type info.</param>
        /// <param name="source">The source.</param>
        /// <param name="path">The path.</param>
        /// <param name="rightControlMode">The right control mode.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public static bool ReflectACUrlBinding(this IACObject reflectedObject, string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            source = reflectedObject; // ParentACObject;
            path = "";
            if (string.IsNullOrEmpty(acUrl))
                return true;

            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    return false;
                case ACUrlHelper.UrlKeys.Child:
                    {
                        string[] parts = acUrl.Split('\\');
                        foreach (var part in parts)
                        {
                            if (acTypeInfo == null)
                                acTypeInfo = reflectedObject.ACType;

                            if (path == Const.Value && reflectedObject is IACContainer)
                            {
                                var acEntityProperty = reflectedObject as IACContainer;
                                var query = acEntityProperty.ValueTypeACClass.Properties.Where(c => c.ACIdentifier == part);
                                if (query.Any())
                                {
                                    acTypeInfo = query.First();
                                    path += "." + part;
                                    //path += "[" + part + "]";
                                    rightControlMode = Global.ControlModes.Enabled;
                                }
                            }
                            else if (path == "ConfigValue" && reflectedObject is ACClassProperty)
                            {
                                var acEntityProperty = reflectedObject as ACClassProperty;
                                var query = acEntityProperty.ConfigACClass.Properties.Where(c => c.ACIdentifier == part);
                                if (query.Any())
                                {
                                    acTypeInfo = query.First();
                                    path += "." + part;
                                    //path += "[" + part + "]";
                                    rightControlMode = Global.ControlModes.Enabled;
                                }
                            }
                            else
                            {
                                IACType childACTypeInfo = acTypeInfo.GetMember(part);
                                if (childACTypeInfo == null)
                                {
                                    if (reflectedObject is IACObjectEntity)
                                    {
                                        string[] filterValues = ACUrlHelper.GetFilterValues(acUrlHelper.ACUrlPart);
                                        if (filterValues != null)
                                        {
                                            var result = ((IACObjectEntity)reflectedObject).GetChildEntityObject(ACUrlHelper.ExtractTypeName(acUrlHelper.ACUrlPart), filterValues);
                                            if (result == null)
                                                return false;
                                            if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                            {
                                                source = result;
                                                path = "";
                                                rightControlMode = Global.ControlModes.Enabled;
                                                IACObject sourceACObject = source as IACObject;
                                                if (sourceACObject != null)
                                                {
                                                    acTypeInfo = sourceACObject.ACType;
                                                    return true;
                                                }
                                                else
                                                {
                                                    acTypeInfo = null;
                                                    return false;
                                                }
                                            }
                                            return ((IACObject)result).ACUrlBinding(acUrlHelper.NextACUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                                        }
                                        else
                                            return false;
                                    }
                                    else if (reflectedObject is ACGenericObject)
                                    {
                                        var query = (reflectedObject as ACGenericObject).ACObjectChilds.Where(c => c.ACIdentifier == acUrlHelper.ACUrlPart);
                                        if (!query.Any())
                                            return false;
                                        var result = query.First();
                                        if (result == null)
                                            return false;
                                        if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                        {
                                            source = result;
                                            path = "";
                                            rightControlMode = Global.ControlModes.Enabled;
                                            IACObject sourceACObject = source as IACObject;
                                            if (sourceACObject != null)
                                            {
                                                acTypeInfo = sourceACObject.ACType;
                                                return true;
                                            }
                                            else
                                            {
                                                acTypeInfo = null;
                                                return false;
                                            }
                                        }
                                        return ((IACObject)result).ACUrlBinding(acUrlHelper.NextACUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                                    }
                                    return false;
                                }
                                else
                                {
                                    acTypeInfo = childACTypeInfo;
                                    path += acTypeInfo.GetACPath(string.IsNullOrEmpty(path));
                                }
                            }
                        }
                        ACClassProperty cp = acTypeInfo as ACClassProperty;
                        rightControlMode = cp.Safe_ACClass.RightManager.GetControlMode(cp);
                        if (path == Const.Value && acUrl == Const.Value && parts.Count() == 1 && reflectedObject is IACContainer)
                            acTypeInfo = (reflectedObject as IACContainer).ValueTypeACClass;
                        return true;
                    }
                case ACUrlHelper.UrlKeys.Parent:
                    return false;
                default:
                    return false; // TODO: Fehlerbehandlung
            }
        }

        /// <summary>
        /// Reflects the is enabled delete.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <returns>Msg.</returns>
        public static MsgWithDetails ReflectIsEnabledDelete(this IACObject reflectedObject)
        {
            MsgWithDetails msg = null;
            Type entityType = reflectedObject.GetType();
            ACClass acType = reflectedObject.ACType as ACClass;
            IEnumerable<ACClassProperty> relations = null;

            using (ACMonitor.Lock(acType.Database.QueryLock_1X000))
            {
                relations = acType.ACClassProperty_ACClass
                    .Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.RelationPoint && c.DeleteActionIndex == (Int16)Global.DeleteAction.None)
                    .ToArray();
            }
            if (relations != null && relations.Any())
            {
                foreach (var relation in relations)
                {
                    PropertyInfo pi = entityType.GetProperty(relation.ACIdentifier);
                    int count = 0;

                    using (ACMonitor.Lock(acType.Database.QueryLock_1X000))
                    {
                        var enumerable = pi.GetValue(reflectedObject, null) as IEnumerable;
                        Type t = enumerable.GetType();
                        pi = t.GetProperty("Count");
                        count = (int)pi.GetValue(enumerable, null);
                    }

                    if (count > 0)
                    {
                        ACClass typeAsACClass = reflectedObject.ACType as ACClass;
                        if (typeAsACClass != null)
                        {
                            ACClassProperty acClassProperty = typeAsACClass.GetProperty(relation.ACIdentifier);

                            if (acClassProperty != null &&
                                (acClassProperty.DeleteActionIndex == (Int16)Global.DeleteAction.Cascade ||
                                acClassProperty.DeleteActionIndex == (Int16)Global.DeleteAction.CascadeManual)
                                )
                                continue;
                            if (msg == null)
                            {
                                msg = ReflectNewMsgWithDetails(reflectedObject);
                            }

                            msg.AddDetailMessage(reflectedObject.NewMessage(eMsgLevel.Error,
                                Database.Root.Environment.TranslateMessage(Database.Root, "Error00008", count.ToString(), relation.ValueTypeACClass.ACCaption),
                                relation.ValueTypeACClass.ACIdentifier, reflectedObject.GetACUrl() + "\\" + relation.ACIdentifier));
                        }
                    }
                }
            }
            return msg;
        }

        /// <summary>
        /// Reflects the get AC content list.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <returns>IEnumerable{IACObject}.</returns>
        public static IEnumerable<IACObject> ReflectGetACContentList(this IACObject reflectedObject)
        {
            ACClass typeAsACClass = reflectedObject.ACType as ACClass;
            if (typeAsACClass == null)
                return null;
            var query = typeAsACClass.Properties.Where(c => c.IsContent);
            if (!query.Any())
                return null;
            List<IACObject> acContentList = new List<IACObject>();
            foreach (var acTypeInfo in query)
            {
                IACObject acObject = reflectedObject.ACUrlCommand(acTypeInfo.ACIdentifier) as IACObject;
                if (acObject != null)
                {
                    acContentList.Add(acObject);
                }
            }

            return acContentList;
        }

        /// <summary>
        /// Reflects the get menu.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <param name="acElement">The ac element.</param>
        /// <returns>ACMenuItemList.</returns>
        public static ACMenuItemList ReflectGetMenu(this IACObject reflectedObject, IACInteractiveObject acElement)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();

            ACClass classACType = reflectedObject.ACType as ACClass;
            if (classACType != null)
            {
                var methods = classACType.Methods.Where(c => c.IsInteraction).OrderBy(c => c.SortIndex).ThenBy(c => c.ACCaption);

                foreach (var method in methods)
                {
                    if (ReflectIsEnabledACUrlCommand(reflectedObject, "!" + method.ACIdentifier, null))
                    {
                        Global.ControlModes controlMode = classACType.RightManager.GetControlMode(method);
                        if (controlMode == Global.ControlModes.Enabled)
                        {
                            acMenuItemList = ACMenuItem.CreateParentACMenuItem(method, acMenuItemList);
                            ACMenuItem parentItem = acMenuItemList.FirstOrDefault(c => c.ACUrl == method.ContextMenuCategoryIndex.ToString());

                            string parentItemACUrl = null;
                            if (parentItem != null)
                                parentItemACUrl = parentItem.ACUrl;

                            ACMenuItem item = new ACMenuItem(method.ACCaption, "!" + method.ACIdentifier, method.SortIndex, null, parentItemACUrl, false, acElement);

                            item.IconACUrl = method.GetIconACUrl();
                            acMenuItemList.Add(item);
                        }
                    }
                }
            }
            return acMenuItemList;
        }

        /// <summary>
        /// Reflects the new MSG with details.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <returns>MsgWithDetails.</returns>
        public static MsgWithDetails ReflectNewMsgWithDetails(this IACObject reflectedObject)
        {
            string message1 = Database.Root.Environment.TranslateMessage(Database.Root, "Error00010", reflectedObject.ACType.ACCaption, reflectedObject.ACCaption);

            MsgWithDetails msg = new MsgWithDetails
            {
                Source = Database.ClassName,
                MessageLevel = eMsgLevel.Error,
                ACIdentifier = reflectedObject.ACType.ACIdentifier,
                Message = message1
            };

            msg.AddACCommandMsg(reflectedObject.GetACUrl(), null);

            return msg;
        }

        /// <summary>
        /// News the message.
        /// </summary>
        /// <param name="reflectedObject">The reflected object.</param>
        /// <param name="messageLevel">The message level.</param>
        /// <param name="message">The message.</param>
        /// <param name="acNameIdentifier">The ac name identifier.</param>
        /// <param name="acUrl">The ac URL.</param>
        /// <returns>Msg.</returns>
        public static MsgWithDetails NewMessage(this IACObject reflectedObject, eMsgLevel messageLevel, string message, string acNameIdentifier, string acUrl)
        {
            MsgWithDetails msg = new MsgWithDetails { Source = Const.ContextDatabase, MessageLevel = messageLevel, ACIdentifier = acNameIdentifier, Message = message };
            msg.AddACCommandMsg(acUrl, null);

            return msg;
        }

        public static void DefaultValuesACObject(this IACObject acObject)
        {
            if (acObject == null)
                return;
            ACClass entitySchema = acObject.ACType as ACClass;
            if (entitySchema == null)
                return;

            Type entityType = acObject.GetType();
            foreach (ACClassProperty acClassProperty in entitySchema.PropertiesCached)
            {
                //if (acClassProperty.AssemblyQualifiedName == "Item")
                //    continue;

                // Falls kein Defaultwert gesetzt und Feldd nullable, dann belasse Feld null
                if (acClassProperty.IsNullable && (acClassProperty.XMLValue == null))
                    continue;
                PropertyInfo pi = entityType.GetProperty(acClassProperty.ACIdentifier);
                if (pi == null)
                {
                    Type typeOfProperty = acClassProperty.ObjectType;
                    if (typeof(Guid).IsAssignableFrom(typeOfProperty))
                        continue;
                    try
                    {
                        bool isString = typeof(String).IsAssignableFrom(typeOfProperty);
                        // Objekte, die nicht Strings sind, können per Default nicht gesetzt werden (z.B. Relationship-Attribute)
                        if (!typeOfProperty.IsValueType && !isString)
                            continue;
                        // Falls Default-Wert gesetzt
                        if (acClassProperty.XMLValue != null)
                        {
                            acObject.ACUrlCommand(acClassProperty.ACIdentifier, ACConvert.ChangeType(acClassProperty.XMLValue, typeOfProperty, true, Database.GlobalDatabase));
                        }
                        // Falls kein Default-Wert gesetzt und Feld nicht nullbar ist und keine Eingabepflicht besteht,
                        // dann darf das Feld automatisch bestückt werden
                        else if (!acClassProperty.IsNullable && !acClassProperty.MinLength.HasValue)
                        {
                            // Falls Mindestwert vorgegeben, dann setzte diesen als Default
                            if (acClassProperty.MinValue.HasValue)
                                acObject.ACUrlCommand(acClassProperty.ACIdentifier, ACConvert.ChangeType(acClassProperty.MinValue.Value, typeOfProperty, true, Database.GlobalDatabase));
                            // Sonst ist 0 Default wert
                            else
                            {
                                if (typeof(DateTime).IsAssignableFrom(typeOfProperty))
                                    acObject.ACUrlCommand(acClassProperty.ACIdentifier, DateTime.Now);
                                else
                                {
                                    if (isString)
                                        acObject.ACUrlCommand(acClassProperty.ACIdentifier, "");
                                    else
                                        acObject.ACUrlCommand(acClassProperty.ACIdentifier, ACConvert.ChangeType((int)0, typeOfProperty, true, Database.GlobalDatabase));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("IACObjectReflectionExtension", "DefaultValuesACObject", msg);

                        // DO NOTHING
                        // Kann vorkommen, wenn die Daten in der ACValueType nicht sauber sind. 
                        // TODO: Erst mal ignorieren, später vielleicht mal ne Korrektur einbauen
                    }

                }
                else
                {
                    if (!pi.CanWrite)
                        continue;
                    if (typeof(Guid).IsAssignableFrom(pi.PropertyType))
                        continue;

                    try
                    {
                        bool isString = typeof(String).IsAssignableFrom(pi.PropertyType);
                        // Objekte, die nicht Strings sind, können per Default nicht gesetzt werden (z.B. Relationship-Attribute)
                        if (!pi.PropertyType.IsValueType && !isString)
                            continue;
                        // Falls Default-Wert gesetzt
                        if (acClassProperty.XMLValue != null)
                        {
                            if (pi.PropertyType.IsEnum)
                                pi.SetValue(acObject, Enum.Parse(pi.PropertyType, acClassProperty.XMLValue), null);
                            else
                                pi.SetValue(acObject, ACConvert.ChangeType(acClassProperty.XMLValue, pi.PropertyType, true, Database.GlobalDatabase), null);
                        }
                        // Falls kein Default-Wert gesetzt und Feld nicht nullbar ist und keine Eingabepflicht besteht,
                        // dann darf das Feld automatisch bestückt werden
                        else if (!acClassProperty.IsNullable && !acClassProperty.MinLength.HasValue)
                        {
                            if (pi.PropertyType.IsEnum)
                                continue;
                            // Falls Mindestwert vorgegeben, dann setzte diesen als Default
                            if (acClassProperty.MinValue.HasValue)
                            {
                                pi.SetValue(acObject, ACConvert.ChangeType(acClassProperty.MinValue.Value, pi.PropertyType, true, Database.GlobalDatabase), null);
                            }
                            // Sonst ist 0 Default wert
                            else
                            {
                                if (typeof(DateTime).IsAssignableFrom(pi.PropertyType))
                                    pi.SetValue(acObject, DateTime.Now, null);
                                else
                                {
                                    if (isString)
                                        pi.SetValue(acObject, "", null);
                                    else
                                        pi.SetValue(acObject, ACConvert.ChangeType((int)0, pi.PropertyType, true, Database.GlobalDatabase), null);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("IACObjectReflectionExtension", "DefaultValuesACObject(10)", msg);

                        // DO NOTHING
                        // Kann vorkommen, wenn die Daten in der ACValueType nicht sauber sind. 
                        // TODO: Erst mal ignorieren, später vielleicht mal ne Korrektur einbauen
                    }
                }
            }
        }

        /// <summary>
        /// Prüft eine Entität gegenüber der Datenbankinformationen aus ACValueType und 
        /// ACValueType
        /// </summary>
        /// <param name="acObject"></param>
        /// <param name="ose"></param>
        /// <param name="changeLogList"></param>
        /// <param name="database"></param>
        public static IList<Msg> CheckACObject(this IACObject acObject, EntityEntry ose, List<Tuple<ACChangeLog, int>> changeLogList, Database database = null)
        {
            if (acObject == null)
                return null;
            // No vaildation for Task-Classes to avoid deadlock beacuse of Reflection and increases performance
            if (acObject is ACClassTask || acObject is ACClassTaskValue || acObject is ACClassTaskValuePos)
                return null;
            ACClass entitySchema = acObject.ACType as ACClass;
            if (entitySchema == null)
                return null;
            if (database == null)
                database = Database.GlobalDatabase;

            IList<Msg> resultList = null;

            Type typeIEnumerable = typeof(IEnumerable);
            Type typeString = typeof(String);
            Type typeGuid = typeof(Guid);
            //IEnumerable<string> modifiedProps = ose.GetModifiedProperties();
            IEnumerable<string> modifiedProps = database.Entry(ose).Properties.Where(c => c.IsModified).Select(c => c.Metadata.Name);
            Type entityType = acObject.GetType();
            foreach (ACClassProperty acClassProperty in entitySchema.Properties)
            {
                //if (   acClassProperty.AssemblyQualifiedName == "Item" 
                //    || (acObject is IACConfig && acClassProperty.ACIdentifier == Const.Value))
                if (acObject is IACConfig && acClassProperty.ACIdentifier == Const.Value)
                {
                    if (modifiedProps.Contains(Const.EntityXMLConfig) || (!modifiedProps.Any() && ose.State == EntityState.Added))
                        ProcessChangeLog(acObject, entitySchema, acClassProperty, modifiedProps, ose, changeLogList);
                    continue;
                }
                PropertyInfo pi = entityType.GetProperty(acClassProperty.ACIdentifier);
                Type typeOfProperty = acClassProperty.ObjectType;

                if (pi != null)
                {
                    typeOfProperty = pi.PropertyType;
                    // Nur Properties, die geschrieben werden können überprüfen (get + set), weil sonst zusätzliche Properties (Relationen und Abfragen unnötig geprüft werden)
                    if (!pi.CanRead || !pi.CanWrite)
                        continue;

                    // Keine Enumerationen/Relationship-Attribute überprüfbar
                    if (typeIEnumerable.IsAssignableFrom(typeOfProperty) && !typeString.IsAssignableFrom(typeOfProperty))
                        continue;

                    if (!string.IsNullOrEmpty(acClassProperty.ACSource))
                    {
                        string propNameWithID = acClassProperty.ACIdentifier;
                        int uPos = acClassProperty.ACIdentifier.LastIndexOf("_");
                        if (uPos > 0)
                        {
                            if (uPos + 1 >= acClassProperty.ACIdentifier.Length)
                                continue;
                            propNameWithID = acClassProperty.ACIdentifier.Substring(uPos + 1);
                            if (String.IsNullOrEmpty(propNameWithID) || propNameWithID.Length <= 1)
                                continue;
                        }

                        propNameWithID += "ID";
                        PropertyInfo pi2 = entityType.GetProperty(propNameWithID);
                        if (pi2 == null)
                        {
                            continue;
                        }
                        else
                        {
                            typeOfProperty = pi2.PropertyType;
                            if (!pi2.CanRead || !pi2.CanWrite)
                                continue;
                            pi = pi2;
                        }
                    }

                    //acClassProperty.AutoRefresh();

                    ProcessChangeLog(acObject, entitySchema, acClassProperty, modifiedProps, ose, changeLogList);
                }

                object valueToCheck = null;
                try
                {
                    if (pi != null)
                        valueToCheck = pi.GetValue(acObject, null);
                    else
                        valueToCheck = acObject.ACUrlCommand(acClassProperty.ACIdentifier);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("IACObjectReflectionExtension", "CheckACObject", msg);
                }

                Global.ControlModesInfo checkInfo = CheckPropertyMinMax(acClassProperty, valueToCheck, acObject, typeOfProperty, database);
                if (checkInfo.Message != null)
                {
                    if (resultList == null)
                        resultList = new List<Msg>();
                    checkInfo.Message.Source = acObject.GetACUrl();
                    resultList.Add(checkInfo.Message);
                }
            }
            return resultList;
        }

        public static Global.ControlModesInfo CheckPropertyMinMax(ACClassProperty acClassProperty, object valueToCheck, IACObject parentObject, Type typeOfProperty = null, Database databaseForMessageText = null)
        {
            return CheckPropertyMinMax(acClassProperty, valueToCheck, parentObject,
                                        typeOfProperty == null ? acClassProperty.ObjectFullType : typeOfProperty,
                                        acClassProperty.IsNullable, acClassProperty.MinLength, acClassProperty.MaxLength,
                                        acClassProperty.MinValue, acClassProperty.MaxValue, databaseForMessageText);
        }


        public static Global.ControlModesInfo CheckPropertyMinMax(ACClassProperty acClassProperty, object valueToCheck, IACObject parentObject, Type typeOfProperty,
                                                                    bool IsNullable, Nullable<int> MinLength, Nullable<int> MaxLength,
                                                                    Nullable<Double> MinValue, Nullable<Double> MaxValue,
                                                                    Database databaseForMessageText = null)
        {
            if (typeOfProperty == null)
                return Global.ControlModesInfo.Disabled;

            Type typeIComparable = typeof(IComparable);
            Type typeIConvertible = typeof(IConvertible);
            Type typeIFormattable = typeof(IFormattable);
            bool isString = typeof(String).IsAssignableFrom(typeOfProperty);

            bool bNotEditableEntityKey = false;
            if (parentObject != null)
            {
                VBEntityObject entityObject = parentObject as VBEntityObject;
                if (entityObject != null && entityObject.EntityState != EntityState.Added && entityObject.EntityState != EntityState.Detached)
                {
                    ACClass saveClass = acClassProperty.Safe_ACClass;
                    if (saveClass != null && !String.IsNullOrEmpty(saveClass.ACFilterColumns))
                    {
                        if (saveClass.ACFilterColumns.Contains(acClassProperty.ACIdentifier))
                        {
                            bNotEditableEntityKey = true;
                        }
                    }
                }
            }

            Global.ControlModesInfo newMode = Global.ControlModesInfo.Enabled;
            if (valueToCheck == null)
            {
                newMode = Global.ControlModesInfo.Enabled;
                newMode.IsNull = true;
                // Falls Feld nicht nullbar aber Wert null ist, dann Meldung
                if (!IsNullable)
                {
                    newMode.Mode = Global.ControlModes.EnabledRequired;
                    if (databaseForMessageText != null)
                        newMode.Message = new Msg
                        {
                            ACIdentifier = acClassProperty.ACIdentifier,
                            Message = Database.Root.Environment.TranslateMessage(databaseForMessageText, "Error50000", acClassProperty.ACIdentifier, acClassProperty.ACCaption),
                            MessageLevel = eMsgLevel.Error
                        };
                }
                else if (MinLength.HasValue)
                {
                    newMode.Mode = Global.ControlModes.EnabledRequired;
                    if (databaseForMessageText != null)
                        newMode.Message = new Msg
                        {
                            ACIdentifier = acClassProperty.ACIdentifier,
                            Message = Database.Root.Environment.TranslateMessage(databaseForMessageText, "Error50001", acClassProperty.ACIdentifier, acClassProperty.ACCaption, MinLength.Value),
                            MessageLevel = eMsgLevel.Error
                        };
                }
                return newMode;
            }

            // Falls ID
            // oder Objekte, die nicht Strings sind, können nicht geprüft werden (z.B. Relationship-Attribute)
            if (typeof(Guid).IsAssignableFrom(typeOfProperty)
                || (!typeOfProperty.IsValueType && !isString)
                || (!(typeIComparable.IsAssignableFrom(typeOfProperty) && typeIConvertible.IsAssignableFrom(typeOfProperty))))
            {
                return Global.ControlModesInfo.Enabled;
            }

            // Falls Eingabepflicht
            if (MinLength.HasValue)
            {
                // Feld Nullbar
                if (IsNullable)
                {
                    // Falls wert null aber Eingabepflicht: Meldung
                    if (valueToCheck == null)
                    {
                        newMode.Mode = Global.ControlModes.EnabledRequired;
                        if (databaseForMessageText != null)
                            newMode.Message = new Msg
                            {
                                ACIdentifier = acClassProperty.ACIdentifier,
                                Message = Database.Root.Environment.TranslateMessage(databaseForMessageText, "Error50001", acClassProperty.ACIdentifier, acClassProperty.ACCaption, MinLength.Value),
                                MessageLevel = eMsgLevel.Error
                            };
                        return newMode;
                    }
                    // Überprüfe Mindest Länge falls String
                    else if (isString)
                    {
                        string valueString = valueToCheck as String;
                        if (valueString.Length < MinLength)
                        {
                            newMode.Mode = Global.ControlModes.EnabledRequired;
                            if (databaseForMessageText != null)
                                newMode.Message = new Msg
                                {
                                    ACIdentifier = acClassProperty.ACIdentifier,
                                    Message = Database.Root.Environment.TranslateMessage(databaseForMessageText, "Error50002", acClassProperty.ACIdentifier, acClassProperty.ACCaption, MinLength.Value),
                                    MessageLevel = eMsgLevel.Error
                                };
                            return newMode;
                        }
                        else if (bNotEditableEntityKey)
                        {
                            newMode.Mode = Global.ControlModes.Disabled;
                            return newMode;
                        }
                    }
                }
                // Wert ist nicht null und Feld nicht nullbar, Überprüfe Mindest Länge falls String
                else
                {
                    if (isString)
                    {
                        string valueString = valueToCheck as String;
                        if (valueString.Length < MinLength)
                        {
                            newMode.Mode = Global.ControlModes.EnabledRequired;
                            if (databaseForMessageText != null)
                                newMode.Message = new Msg
                                {
                                    ACIdentifier = acClassProperty.ACIdentifier,
                                    Message = Database.Root.Environment.TranslateMessage(databaseForMessageText, "Error50002", acClassProperty.ACIdentifier, acClassProperty.ACCaption, MinLength.Value),
                                    MessageLevel = eMsgLevel.Error
                                };
                            return newMode;
                        }
                        else if (bNotEditableEntityKey)
                        {
                            newMode.Mode = Global.ControlModes.Disabled;
                            return newMode;
                        }
                    }
                }
            }
            // Sonst keine Eingabepflicht
            //else
            //{
            //    // Wert ist nicht null
            //    if (!property.IsNullable)
            //    {
            //    }
            //    // Ob Wert gesetzt ist oder nicht spielt keine Rolle
            //    else
            //    {
            //    }
            //}

            if (valueToCheck == null)
                return newMode;

            if (isString)
            {
                // Falls Maximal-Länge gesetzt
                if (MaxLength.HasValue)
                {
                    string valueString = valueToCheck as String;
                    if (valueString.Length > MaxLength)
                    {
                        newMode = Global.ControlModesInfo.EnabledWrong;
                        if (databaseForMessageText != null)
                            newMode.Message = new Msg
                            {
                                ACIdentifier = acClassProperty.ACIdentifier,
                                Message = Database.Root.Environment.TranslateMessage(databaseForMessageText, "Error50003", acClassProperty.ACIdentifier, acClassProperty.ACCaption, acClassProperty.MaxLength.Value),
                                MessageLevel = eMsgLevel.Error
                            };
                    }
                    else if (bNotEditableEntityKey)
                    {
                        newMode = Global.ControlModesInfo.Disabled;
                    }
                }
                // Falls Datenbankfeldlänge überschritten
                if (acClassProperty.DataTypeLength > 0)
                {
                    string valueString = valueToCheck as String;
                    if (valueString.Length > acClassProperty.DataTypeLength)
                    {
                        newMode = Global.ControlModesInfo.EnabledWrong;
                        if (databaseForMessageText != null)
                            newMode.Message = new Msg
                            {
                                ACIdentifier = acClassProperty.ACIdentifier,
                                Message = Database.Root.Environment.TranslateMessage(databaseForMessageText, "Error50003", acClassProperty.ACIdentifier, acClassProperty.ACCaption, acClassProperty.DataTypeLength),
                                MessageLevel = eMsgLevel.Error
                            };
                    }
                    else if (bNotEditableEntityKey && !String.IsNullOrEmpty(valueString))
                    {
                        newMode = Global.ControlModesInfo.Disabled;
                    }
                }
            }
            else
            {
                // UInt64, UInt32, UInt16, Byte, Int64, Int32, Int16, SByte, Single, Double, Decimal
                if (typeIFormattable.IsAssignableFrom(typeOfProperty))
                {
                    try
                    {
                        Type typeToConvert = typeOfProperty;
                        if (typeOfProperty.IsEnum)
                        {
                            typeToConvert = Enum.GetUnderlyingType(typeOfProperty);
                        }
                        if (MinValue.HasValue && ((valueToCheck as IComparable).CompareTo(System.Convert.ChangeType(MinValue, typeToConvert)) < 0))
                        {
                            newMode = Global.ControlModesInfo.EnabledWrong;
                            if (databaseForMessageText != null)
                                newMode.Message = new Msg
                                {
                                    ACIdentifier = acClassProperty.ACIdentifier,
                                    Message = Database.Root.Environment.TranslateMessage(databaseForMessageText, "Error50004", acClassProperty.ACIdentifier, acClassProperty.ACCaption, MinValue.Value),
                                    MessageLevel = eMsgLevel.Error
                                };
                        }
                        if (MaxValue.HasValue && ((valueToCheck as IComparable).CompareTo(System.Convert.ChangeType(MaxValue, typeToConvert)) > 0))
                        {
                            newMode = Global.ControlModesInfo.EnabledWrong;
                            if (databaseForMessageText != null)
                                newMode.Message = new Msg
                                {
                                    ACIdentifier = acClassProperty.ACIdentifier,
                                    Message = Database.Root.Environment.TranslateMessage(databaseForMessageText, "Error50005", acClassProperty.ACIdentifier, acClassProperty.ACCaption, MaxValue.Value),
                                    MessageLevel = eMsgLevel.Error
                                };
                        }
                        else if (newMode.Mode != Global.ControlModes.EnabledWrong && bNotEditableEntityKey)
                        {
                            Int64 valueAsInt64 = 0;
                            try
                            {
                                valueAsInt64 = Convert.ToInt64(valueToCheck);
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (Database.Root != null && Database.Root.Messages != null)
                                    Database.Root.Messages.LogException("IACObjectReflectionExtension", "CheckPropertyMinMax", msg);
                            }
                            if (valueAsInt64 != 0)
                                newMode = Global.ControlModesInfo.Disabled;
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("IACObjectReflectionExtension", "CheckPropertyMinMax(10)", msg);
                    }
                }
            }
            return newMode;
        }

        internal static void ProcessChangeLog(IACObject acObject, ACClass entitySchema, ACClassProperty acClassProperty, IEnumerable<string> modifiedProps, EntityEntry ose,
                                             List<Tuple<ACChangeLog, int>> changeLogs)
        {
            if (acClassProperty.ChangeLogMax.HasValue || entitySchema.ChangeLogMax.HasValue)
            {

                if (modifiedProps.Contains(acClassProperty.ACIdentifier) ||
                   (acClassProperty.ACIdentifier == Const.Value && modifiedProps.Contains(Const.EntityXMLConfig)) ||
                   (!modifiedProps.Any() && acClassProperty.ACIdentifier == Const.Value && (ose.State == EntityState.Added || ose.State == EntityState.Deleted)))
                {
                    int changeLogMax = acClassProperty.ChangeLogMax.HasValue ? acClassProperty.ChangeLogMax.Value : entitySchema.ChangeLogMax.Value;

                    VBEntityObject eObj = acObject as VBEntityObject;
                    if (eObj != null && eObj.EntityKey != null && eObj.EntityKey.EntityKeyValues != null)
                    {
                        Guid entityKey = (Guid)eObj.EntityKey.EntityKeyValues[0].Value;
                        ACChangeLog aCChangeLog = ACChangeLog.NewACObject();
                        aCChangeLog.ACClassID = entitySchema.ACClassID;
                        aCChangeLog.ACClassPropertyID = acClassProperty.ACClassPropertyID;
                        aCChangeLog.EntityKey = entityKey;
                        aCChangeLog.ChangeDate = DateTime.Now;

                        if (ose.State == EntityState.Deleted)
                        {
                            aCChangeLog.Deleted = true;
                            ACChangeLogInfo info = new ACChangeLogInfo() { Info = acObject.ToString(), ACUrl = acObject.GetACUrl() };
                            aCChangeLog.XMLValue = info.XMLValue;
                        }
                        else
                        {
                            aCChangeLog.Deleted = false;
                            object changedValue = acObject.GetValue(acClassProperty.ACIdentifier);
                            aCChangeLog.XMLValue = changedValue != null ? ACConvert.ObjectToXML(changedValue, true) : "";
                        }
                        changeLogs.Add(new Tuple<ACChangeLog, int>(aCChangeLog, changeLogMax));
                    }
                }
            }
        }

        public static IEnumerable<IACConfig> GetConfigByKeyACUrl(this IACObject reflectedObject, string keyACUrl)
        {
            ACClass acClass = reflectedObject as ACClass;
            if (acClass == null)
                acClass = reflectedObject.ACType as ACClass;
            if (acClass == null)
                return new IACConfig[] { };
            return acClass.ConfigurationEntries.Where(c => c.KeyACUrl == keyACUrl).ToArray();
        }

    }
}
