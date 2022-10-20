// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACKnownTypes.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACKnownTypes
    /// </summary>
    public static class ACKnownTypes
    {
        /// <summary>
        /// Initializes static members of the <see cref="ACKnownTypes"/> class.
        /// </summary>
        static ACKnownTypes()
        {
            RegisterUnKnownType(typeof(Boolean));
            RegisterUnKnownType(typeof(Byte));
            RegisterUnKnownType(typeof(SByte));
            RegisterUnKnownType(typeof(Int16));
            RegisterUnKnownType(typeof(Int32));
            RegisterUnKnownType(typeof(Int64));
            RegisterUnKnownType(typeof(UInt16));
            RegisterUnKnownType(typeof(UInt32));
            RegisterUnKnownType(typeof(UInt64));
            RegisterUnKnownType(typeof(Single));
            RegisterUnKnownType(typeof(Double));
            RegisterUnKnownType(typeof(Decimal));
            RegisterUnKnownType(typeof(String));
            RegisterUnKnownType(typeof(Guid));
            RegisterUnKnownType(typeof(TimeSpan));
            RegisterUnKnownType(typeof(DateTime));
            RegisterUnKnownType(typeof(Nullable<Boolean>));
            RegisterUnKnownType(typeof(Nullable<Byte>));
            RegisterUnKnownType(typeof(Nullable<SByte>));
            RegisterUnKnownType(typeof(Nullable<Int16>));
            RegisterUnKnownType(typeof(Nullable<Int32>));
            RegisterUnKnownType(typeof(Nullable<Int64>));
            RegisterUnKnownType(typeof(Nullable<UInt16>));
            RegisterUnKnownType(typeof(Nullable<UInt32>));
            RegisterUnKnownType(typeof(Nullable<UInt64>));
            RegisterUnKnownType(typeof(Nullable<Single>));
            RegisterUnKnownType(typeof(Nullable<Double>));
            RegisterUnKnownType(typeof(Nullable<Decimal>));
            RegisterUnKnownType(typeof(eMsgLevel));
            RegisterUnKnownType(typeof(eMsgButton));
            RegisterKnownMessageType(typeof(object[]));
        }

        /// <summary>
        /// The _ known message types
        /// </summary>
        private static List<Type> _KnownMessageTypes = new List<Type>();
        /// <summary>
        /// Methode, bei der alle Typen registriert werden, die als Message-Struktur vorliegen,
        /// aber nicht als Property oder Parameter im Methoden übergeben werden
        /// </summary>
        /// <param name="t">The t.</param>
        public static void RegisterKnownMessageType(Type t)
        {
            if (t == null)
                return;
            try
            {
                if (!_KnownMessageTypes.Where(c => c.FullName == t.FullName).Any())
                {
                    _KnownMessageTypes.Add(t);
                    _AllTypes = null;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACKnownTypes", "RegisterKnownMessageType", msg);
            }
        }

        /// <summary>
        /// The _ un known types
        /// </summary>
        private static List<Type> _UnKnownTypes = new List<Type>();
        /// <summary>
        /// Methode, bei der alle Typen registriert werden, die später als
        /// Property oder Parameter in Methoden serialisiert werden müssen
        /// </summary>
        /// <param name="t">The t.</param>
        public static void RegisterUnKnownType(Type t)
        {
            if (t == null)
                return;
            try
            {
                if (!_UnKnownTypes.Where(c => c.FullName == t.FullName).Any())
                {
                    _UnKnownTypes.Add(t);
                    _AllTypes = null;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACKnownTypes", "RegisterUnKnownType", msg);
            }
        }

        /// <summary>
        /// Liste von Typen, die als Property oder Parameter in Methoden serialisiert werden
        /// </summary>
        /// <value>The un known types.</value>
        public static List<Type> UnKnownTypes
        {
            get
            {
                return _UnKnownTypes;
            }
        }

        /// <summary>
        /// The _ all types
        /// </summary>
        private static Type[] _AllTypes = null;
        /// <summary>
        /// Gets the type of the known.
        /// </summary>
        /// <returns>Type[][].</returns>
        public static Type[] GetKnownType()
        {
            if (_AllTypes == null)
            {
                List<Type> allTypes = new List<Type>();
                allTypes.AddRange(_KnownMessageTypes);
                allTypes.AddRange(_UnKnownTypes);
                _AllTypes = allTypes.ToArray();
            }
            return _AllTypes;
        }

        /// <summary>
        /// Determines whether [is known type] [the specified t].
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns><c>true</c> if [is known type] [the specified t]; otherwise, <c>false</c>.</returns>
        public static bool IsKnownType(Type t)
        {
            if (t == null)
                return false;
            Type[] knownTypes = ACKnownTypes.GetKnownType();
            try
            {
                return knownTypes.Where(c => c.FullName == t.FullName).Any();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACKnownTypes", "IsKnownType", msg);
            }
            return false;
        }

        /// <summary>
        /// Determines whether [is type broadcastable] [the specified t].
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns><c>true</c> if [is type broadcastable] [the specified t]; otherwise, <c>false</c>.</returns>
        public static bool IsTypeBroadcastable(Type t)
        {
            if (t == null)
                return false;
            try
            {
                if (_UnKnownTypes.Where(c => c.FullName == t.FullName).Any())
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACKnownTypes", "IsTypeBroadcastable", msg);
            }
            return false;
        }
    }
}
