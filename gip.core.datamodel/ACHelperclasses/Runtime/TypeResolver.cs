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
using System.Data.Objects.DataClasses;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class TypeResolver
    /// </summary>
    public class TypeResolver
    {
        public TypeResolver(string longTypeName, string typeName, string nestedTypeName, Assembly assembly)
        {
            _LongTypeName = longTypeName;
            _TypeName = typeName;
            _NestedTypeName = nestedTypeName;
            _Assembly = assembly;
        }

        #region Properties
        private readonly object _Lock = new object();

        private string _LongTypeName;
        public string LongTypeName { get  { return _LongTypeName; } }

        private string _TypeName;
        public string TypeName { get { return _TypeName; } }

        private string _NestedTypeName;
        public string NestedTypeName { get { return _NestedTypeName; } }

        private Assembly _Assembly;
        public Assembly Assembly { get { return _Assembly; } }

        private Type _Type = null;
        public Type Type
        {
            get
            {
                lock (_Lock)
                {
                    if (_Type != null)
                        return _Type;

                    try
                    {
                        Type type = this.Assembly.GetType(TypeName);
                        // If generic Type e.g. "ACPointAsyncRMIWrap`1" then Type must be searched by Query
                        if (type == null)
                            type = this.Assembly.GetTypes().Where(c => c.Name == TypeName).FirstOrDefault();
                        if (type == null)
                            return null;
                        if (String.IsNullOrEmpty(NestedTypeName))
                        {
                            _Type = type;
                            return _Type;
                        }
                        else
                        {
                            _Type = type.GetNestedType(NestedTypeName);
                            // If generic Type e.g. "ACPointAsyncRMIWrap`1" then Type must be searched by Query
                            if (_Type == null)
                                _Type = type.GetNestedTypes().Where(c => c.Name == NestedTypeName).FirstOrDefault();
                            return _Type;
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("TypeResolver", "Type(0)", String.Format("Cant find type {0}, {1}, {2}, {3}, Msg: {4}" , LongTypeName, TypeName, NestedTypeName, Assembly.FullName, msg));

                        return null;
                    }
                }
            }
        }

        #endregion

    }
}
