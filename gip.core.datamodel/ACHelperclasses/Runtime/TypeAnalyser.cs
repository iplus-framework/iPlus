// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="TypeAnalyser.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class TypeAnalyser
    /// </summary>
    public static class TypeAnalyser
    {
        #region Properties
        /// <summary>
        /// The _ types in assembly
        /// </summary>
        private static ConcurrentDictionary<string, TypeResolver> _TypesInAssembly = new ConcurrentDictionary<string, TypeResolver>();
        /// <summary>
        /// The _ loaded assemblies
        /// </summary>
        private static ConcurrentDictionary<string, Assembly> _LoadedAssemblies = new ConcurrentDictionary<string, Assembly>();

        private static readonly object _Lock = new object();

        public static readonly string _TypeName_EntityCollection = typeof(ICollection<>).FullName;
        public static readonly string _TypeName_IEnumerable = typeof(System.Collections.Generic.IEnumerable<>).FullName;
        public static readonly string _TypeName_GenericList = typeof(System.Collections.Generic.List<>).FullName;
        public static readonly string _TypeName_IQueryable = typeof(System.Linq.IQueryable<>).FullName;
        public static readonly string _TypeName_BindingList = typeof(System.ComponentModel.BindingList<>).FullName;
        //public static readonly string _TypeName_ObjectSet = typeof(ObjectSet<>).FullName;
        public static readonly string _TypeName_ObjectSet = typeof(DbSet<>).FullName;


        public const string _TypeName_Boolean = "System.Boolean";
        public const string _TypeName_Byte = "System.Byte";
        public const string _TypeName_SByte = "System.SByte";
        public const string _TypeName_Char = "System.Char";
        public const string _TypeName_Int16 = "System.Int16";
        public const string _TypeName_UInt16 = "System.UInt16";
        public const string _TypeName_Single = "System.Single";
        public const string _TypeName_Int32 = "System.Int32";
        public const string _TypeName_UInt32 = "System.UInt32";
        public const string _TypeName_Double = "System.Double";
        public const string _TypeName_Int64 = "System.Int64";
        public const string _TypeName_UInt64 = "System.UInt64";
        public const string _TypeName_Decimal = "System.Decimal";
        public const string _TypeName_String = "System.String";
        public const string _TypeName_DateTime = "System.DateTime";
        public const string _TypeName_TimeSpan = "System.TimeSpan";
        public const string _TypeName_Guid = "System.Guid";

        public static bool IsEnumerable(string fullNameOfGenericType)
        {
            return fullNameOfGenericType == _TypeName_EntityCollection
                || fullNameOfGenericType == _TypeName_IEnumerable
                || fullNameOfGenericType == _TypeName_GenericList
                || fullNameOfGenericType == _TypeName_IQueryable
                || fullNameOfGenericType == _TypeName_BindingList;
        }

        public static bool IsNumericType(Type typeOfProp)
        {
            if (typeOfProp == null)
                return false;
            return (typeOfProp.FullName == TypeAnalyser._TypeName_Byte)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_Int16)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_Int32)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_Int64)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_UInt16)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_UInt32)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_UInt64)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_Double)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_Single);
        }
#endregion


#region Methods

#region Type-Resolving
        /// <summary>
        /// Gets the type in assembly.
        /// </summary>
        /// <param name="longTypeName">Long name of the type.</param>
        /// <returns>Type.</returns>
        public static Type GetTypeInAssembly(string longTypeName)
        {
            string typeName = "";
            string nestedTypeName = "";
            string nameSpace = "";
            if (string.IsNullOrEmpty(longTypeName))
                return null;

            var type = Type.GetType(longTypeName);
            if (type != null)
                return type;

            if (longTypeName == "System.Linq.IQueryable`1")
                return typeof(IQueryable<>);

            TypeResolver typeResolver = null;

            // 1. Suche ob Typ schonmal abgefragt worden ist
            _TypesInAssembly.TryGetValue(longTypeName, out typeResolver);
            if (typeResolver != null)
                return typeResolver.Type;

            // 2. Falls nicht gefunden, leite mögliche Assemblynamen aus dem namespace ab
            string assemblyName = "";
            string assemblyQlfyName = longTypeName;
            if (!GetNSpaceAndTypeName(assemblyQlfyName, ref assemblyName, ref longTypeName, ref typeName, ref nameSpace, ref nestedTypeName))
                return null;

            if (!String.IsNullOrEmpty(assemblyName))
            {
                try
                {
                    Assembly classAssembly;
                    if (_LoadedAssemblies.TryGetValue(assemblyName, out classAssembly))
                    {
                        lock (_Lock)
                        {
                            string path = assemblyName + ".dll";
                            if (!File.Exists(path))
                                return null;
                            classAssembly = Assembly.LoadFrom(path);
                        }
                        if (classAssembly != null)
                            _LoadedAssemblies.TryAdd(assemblyName, classAssembly);
                    }

                    if (classAssembly != null)
                    {
                        typeResolver = new TypeResolver(longTypeName, typeName, nestedTypeName, classAssembly);
                        Type foundType = typeResolver.Type;
                        if (foundType != null)
                        {
                            if (longTypeName != typeResolver.Type.FullName)
                                return null;
                            _TypesInAssembly.TryAdd(longTypeName, typeResolver);
                            return foundType;
                        }
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("TypeAnalyser", "GetTypeInAssembly", msg);

                    return null;
                }
            }

            // 3. Durchsuche in bereits durchsuten Assemblies
            foreach (Assembly loadedAssembly in _LoadedAssemblies.Values)
            {
                typeResolver = new TypeResolver(longTypeName, typeName, nestedTypeName, loadedAssembly);
                Type foundType = typeResolver.Type;
                if (foundType != null)
                {
                    if (longTypeName != typeResolver.Type.FullName)
                        continue;
                    _TypesInAssembly.TryAdd(longTypeName, typeResolver);
                    return foundType;
                }
            }


            // 3. Suche ob es eine Assembly gibt, die ähnlich heisst wie der namespacename 
            // Falls gefunden, dann suche den Typ in dieser Assembly
            string[] assemblyNameParts = nameSpace.Split('.');
            int size = assemblyNameParts.Count();
            string[] probablyAsmblNames = new string[size]; // vermutlicher Assemblyname
            string lastAssemblyName = "";
            int pos = 0;
            foreach (string namePart in assemblyNameParts)
            {
                if (pos > 0)
                    lastAssemblyName += "." + namePart;
                else
                    lastAssemblyName = namePart;
                pos++;
                probablyAsmblNames[size - pos] = lastAssemblyName;
            }

            foreach (string probablyName in probablyAsmblNames)
            {
                try
                {
                    Assembly classAssembly;
                    if (!_LoadedAssemblies.TryGetValue(probablyName, out classAssembly))
                        continue;

                    typeResolver = new TypeResolver(longTypeName, typeName, nestedTypeName, classAssembly);
                    Type foundType = typeResolver.Type;
                    if (foundType != null)
                    {
                        if (longTypeName != typeResolver.Type.FullName)
                            continue;
                        _TypesInAssembly.TryAdd(longTypeName, typeResolver);
                        return foundType;
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("TypeAnalyser", "GetTypeInAssembly(10)", msg);

                    continue;
                }
            }

            // 4. Falls Typ noch nicht gefunden, dann suche die entsprechende DLL im Verzeichnis und lade sie in den Speicher
            foreach (string probablyName in probablyAsmblNames)
            {
                try
                {
                    Assembly classAssembly = null;
                    lock (_Lock)
                    {
                        string path = probablyName + ".dll";
                        if (!File.Exists(path))
                            continue;
                        classAssembly = Assembly.LoadFrom(path);
                        if (classAssembly != null)
                            _LoadedAssemblies.TryAdd(probablyName, classAssembly);
                    }

                    typeResolver = new TypeResolver(longTypeName, typeName, nestedTypeName, classAssembly);
                    Type foundType = typeResolver.Type;
                    if (foundType != null)
                    {
                        if (longTypeName != typeResolver.Type.FullName)
                            continue;
                        _TypesInAssembly.TryAdd(longTypeName, typeResolver);
                        return foundType;
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("TypeAnalyser", "GetTypeInAssembly(20)", msg);

                    continue;
                }
            }

            // 5. Falls Typ noch immer nicht gefunden, dann durchsuche alle im Speicher geladenen dll's nach diesem Typ
            // aktualisiere Liste
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly loadedAssembly in loadedAssemblies)
            {
                string[] fullName = loadedAssembly.FullName.Split(',');
                typeResolver = new TypeResolver(longTypeName, typeName, nestedTypeName, loadedAssembly);
                Type foundType = typeResolver.Type;
                if (foundType != null)
                {
                    if (longTypeName != typeResolver.Type.FullName)
                        continue;

                    if (!_LoadedAssemblies.ContainsKey(fullName[0]))
                        _LoadedAssemblies.TryAdd(fullName[0], loadedAssembly);
                    _TypesInAssembly.TryAdd(longTypeName, typeResolver);
                    return foundType;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the name of the type by short type.
        /// THREAD-SAFE: Locks with QueryLock_1X000
        /// </summary>
        /// <param name="shortTypeName">Short name of the type.</param>
        /// <param name="database">The database.</param>
        /// <returns>Type.</returns>
        public static Type GetTypeByShortTypeName(string shortTypeName, Database database)
        {
            switch (shortTypeName)
            {
                case "bool":
                    return typeof(Boolean);
                case "double":
                    return typeof(Double);
                case "float":
                    return typeof(Single);
                case "short":
                    return typeof(Int16);
                case "int":
                    return typeof(Int32);
                case "long":
                    return typeof(Int64);
                case "ushort":
                    return typeof(UInt16);
                case "uint":
                    return typeof(UInt32);
                case "ulong":
                    return typeof(UInt64);
                case "byte":
                    return typeof(Byte);
                case "string":
                    return typeof(String);
            }
            Type valueType = TypeAnalyser.GetTypeInAssembly(shortTypeName);
            if (valueType != null)
                return valueType;
            try
            {
                valueType = Type.GetType(shortTypeName);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("TypeAnalyser", "GetTypeByShortTypeName", msg);
            }
            if (valueType != null)
                return valueType;
            if (shortTypeName.Contains('.'))
            {
                shortTypeName = shortTypeName.Substring(shortTypeName.LastIndexOf('.') + 1);
            }

            ACClass acClass = database.GetACType(shortTypeName);
            if (acClass != null)
                valueType = acClass.ObjectType;
            if (valueType != null)
                return valueType;
            return typeof(object);
        }


        public static bool GetNSpaceAndTypeName(string assemblyQualifiedName, ref string assemblyName, ref string longTypeName, ref string typeName, ref string nameSpace, ref string nestedTypeName)
        {
            if (String.IsNullOrEmpty(assemblyQualifiedName))
                return false;
            string[] parts = assemblyQualifiedName.Split(',');
            if (parts == null || !parts.Any())
                return false;
            if (parts.Count() == 1)
            {
                assemblyName = "";
                longTypeName = assemblyQualifiedName;
            }
            else
            {
                assemblyName = parts[1].Trim();
                longTypeName = parts[0].Trim();
            }
            return GetNSpaceAndTypeName(longTypeName, ref typeName, ref nameSpace, ref nestedTypeName);
        }

        /// <summary>
        /// Gets the name of the assembly and type.
        /// </summary>
        /// <param name="longTypeName">Long name of the type.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="nestedTypeName">Name of the nested type.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public static bool GetNSpaceAndTypeName(string longTypeName, ref string typeName, ref string nameSpace, ref string nestedTypeName)
        {
            if (longTypeName.Length <= 0)
                return false;
            int nLastPos = longTypeName.LastIndexOf('.');
            if ((nLastPos <= 0) || (nLastPos >= (longTypeName.Length - 1)))
                return false;

            typeName = longTypeName.Substring((nLastPos + 1));
            // Wegen Enums, die einen Punkt daziwschen haben z.B. Global.ACStates tauchen als Global+ACStates auf
            if (typeName.Contains("+"))
            {
                string[] enumArray = typeName.Split('+');
                typeName = enumArray[0];
                nestedTypeName = enumArray[1];
            }
            nameSpace = longTypeName.Substring(0, nLastPos);
            return true;
        }


        public static IRoot Root(this object object1)
        {
            try
            {
                return gip.core.datamodel.Database.Root;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("TypeAnalyser", "GetTypeInAssembly(10)", msg);
            }
            return null;
        }
#endregion

        
#region Property-Access through Reflection
        public static object GetValue(this object obj, string acUrlOrPropPath)
        {
            if (obj == null || String.IsNullOrEmpty(acUrlOrPropPath))
                return obj;

            IACObject acObject = obj as IACObject;
            if (acObject != null && acUrlOrPropPath.IndexOf('.') < 0)
                obj = acObject.ACUrlCommand(acUrlOrPropPath);
            // Sonst per Reflection
            else
            {
                PropertyInfo pi = null;
                obj = TypeAnalyser.GetPropertyPathValue(obj, acUrlOrPropPath, out pi);
            }
            return obj;
        }

        public static object GetPropertyPathValue(object value, string path, out PropertyInfo pi, char[] pathSeparator = null)
        {
            pi = null;
            if (value == null)
                return null;
            Type typeOfValue = value.GetType();

            if (pathSeparator == null)
                pathSeparator = new char[] { '.', '\\' };
            foreach (string propertyName in path.Split(pathSeparator))
            {
                pi = typeOfValue.GetProperty(propertyName);
                if (pi == null)
                    return null;
                value = pi.GetValue(value, null);
                if (value == null)
                    return null;
                typeOfValue = pi.PropertyType;
                if (typeOfValue == null)
                    return null;
            }
            return value;
        }

        public static PropertyInfo GetPropertyPathInfo(Type typeOfValue, string path, char[] pathSeparator = null)
        {
            PropertyInfo pi = null;
            if (typeOfValue == null)
                return pi;

            if (pathSeparator == null)
                pathSeparator = new char[] { '.', '\\' };
            foreach (string propertyName in path.Split(pathSeparator))
            {
                pi = typeOfValue.GetProperty(propertyName);
                if (pi == null)
                    return null;
                typeOfValue = pi.PropertyType;
                if (typeOfValue == null)
                    return null;
            }
            return pi;
        }
#endregion

#endregion


#if DIAGNOSE2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe public static IntPtr AddressOf(object t)
        {
            // https://github.com/dotnet/coreclr/blob/a6729dab35fde5d50fb12b06aeb77d3e1f3be872/src/System.Private.CoreLib/src/System/TypedReference.cs
            // public ref struct TypedReference
            //      private readonly ByReference<byte> _value; [1]
            //      private readonly RuntimeTypeHandle _typeHandle; [2]

            // https://github.com/dotnet/coreclr/blob/14d6b0258852dd7354bc349d7566dc5337905ac9/src/System.Private.CoreLib/shared/System/ByReference.cs
            // ref struct ByReference<T>
            //  private readonly IntPtr _value; [3]

            TypedReference tr = __makeref(t);
            return **(IntPtr**)(&tr); // Dereferenzierung muss ZWEIMAL erfolgen: 1 mal landet man auf [1] ByReference, danach auf [3] welches aufdie Daten im Memory verweist
        }

        private static object _GCRegionLock = new object();
        private static object _GCToken = null;
        public static void TryStartNoGCRegion(long totalSize, object token)
        {
            lock (_GCRegionLock)
            {
                if (_GCToken != null)
                    return;
                try
                {
                    if (GCSettings.LatencyMode != GCLatencyMode.NoGCRegion)
                    {
                        if (GC.TryStartNoGCRegion(totalSize))
                            _GCToken = token;
                    }
                }
                catch
                {
                }
            }
        }

        public static void EndNoGCRegion(object token)
        {
            lock (_GCRegionLock)
            {
                if (_GCToken == null || _GCToken != token)
                    return;
                try
                {
                    if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
                        GC.EndNoGCRegion();
                    _GCToken = null;
                }
                catch
                {
                }
            }
        }
#endif
    }
}
