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
// <copyright file="ACQuery.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Collections;
using gip.core.datamodel;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACQuery
    /// </summary>
    public static class ACQuery
    {

        // Define method infos
        private static readonly MethodInfo _DynQueryMethod = typeof(ACQuery).GetMethod("SearchWithDynQuery", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo _EntityQueryMethod = typeof(ACQuery).GetMethod("SearchWithEntitySQL", BindingFlags.Static | BindingFlags.NonPublic);

        #region public Methods
        /// <summary>
        /// ACs the select.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <param name="queryDefinition">The query definition.</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>IEnumerable.</returns>
        public static IQueryable ACSelect(this IACObject acObject, ACQueryDefinition queryDefinition, MergeOption mergeOption = MergeOption.AppendOnly)
        {
            return acObject.ACSelect(queryDefinition, queryDefinition.ChildACUrl, mergeOption);
        }

        public static IQueryable<T> ACSelect<T>(this IACObject acObject, ACQueryDefinition queryDefinition, MergeOption mergeOption = MergeOption.AppendOnly) where T : class
        {
            return acObject.ACSelect<T>(queryDefinition, queryDefinition.ChildACUrl, mergeOption);
        }

        /// <summary>
        /// ACs the select.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <param name="queryDefinition">The query definition.</param>
        /// <param name="childACUrl">The child AC URL.</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>IEnumerable.</returns>
        public static IQueryable ACSelect(this IACObject acObject, ACQueryDefinition queryDefinition, string childACUrl, MergeOption mergeOption = MergeOption.AppendOnly)
        {
            Type typeParent = acObject.GetType();
            try
            {
                if (acObject is IACEntityObjectContext)
                {
                    PropertyInfo pi = typeParent.GetProperty(childACUrl);
                    if (pi != null && ((!UseDynLINQ && !string.IsNullOrEmpty(queryDefinition.EntitySQL)) || !string.IsNullOrEmpty(queryDefinition.EntitySQL_FromEdit)))
                    {
                        MethodInfo miEntitySQL = _EntityQueryMethod.MakeGenericMethod(new Type[] { queryDefinition.QueryType.ObjectType });

                        var childProp = acObject.ACUrlCommand(childACUrl);
                        if (childProp == null)
                            return null;
                        return miEntitySQL.Invoke(null, new object[] { childProp, acObject, queryDefinition, childACUrl, mergeOption }) as IQueryable;
                    }
                    else
                    {
                        MethodInfo miDynQuery = _DynQueryMethod.MakeGenericMethod(new Type[] { queryDefinition.QueryType.ObjectType });
                        if (pi != null)
                        {
                            //if (typeof(ObjectQuery).IsAssignableFrom(pi.PropertyType))
                            //    return miEntitySQL.Invoke(null, new object[] { acObject, queryDefinition, childACUrl, mergeOption }) as IQueryable;
                            //else
                            return miDynQuery.Invoke(null, new object[] { pi.GetValue(acObject, null), queryDefinition, mergeOption }) as IQueryable;
                        }
                        else
                        {
                            var childProp = acObject.ACUrlCommand(childACUrl);
                            if (childProp == null)
                                return null;
                            return miDynQuery.Invoke(null, new object[] { childProp, queryDefinition, mergeOption }) as IQueryable;
                        }
                    }
                }
                else
                {
                    MethodInfo miDynQuery = _DynQueryMethod.MakeGenericMethod(new Type[] { queryDefinition.QueryType.ObjectType });
                    var childProp = typeParent.InvokeMember(childACUrl, Global.bfGetProp, null, acObject, null);
                    if (childProp == null)
                        return null;
                    return miDynQuery.Invoke(null, new object[] { childProp, queryDefinition, mergeOption }) as IQueryable;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACQuery", "ACSelect", msg);

                return null;
            }
        }

        public static IQueryable<T> ACSelect<T>(this IACObject acObject, ACQueryDefinition queryDefinition, string childACUrl, MergeOption mergeOption = MergeOption.AppendOnly) where T : class
        {
            Type typeParent = acObject.GetType();

            try
            {
                if (acObject is DbContext)
                {
                    PropertyInfo pi = typeParent.GetProperty(childACUrl);
                    if (pi != null && (!UseDynLINQ || !string.IsNullOrEmpty(queryDefinition.EntitySQL_FromEdit)))
                    {
                        DbSet<T> childProp = acObject.ACUrlCommand(childACUrl) as DbSet<T>;
                        if (childProp == null)
                            return null;
                        return SearchWithEntitySQL<T>(childProp, acObject, queryDefinition, childACUrl, mergeOption);
                    }
                    else
                    {
                        IEnumerable<T> childProp = acObject.ACUrlCommand(childACUrl) as IEnumerable<T>;
                        if (childProp == null)
                            return null;
                        queryDefinition.QueryContext = acObject;
                        return SearchWithDynQuery<T>(childProp, queryDefinition, mergeOption);
                    }
                }
                else
                {
                    IEnumerable<T> childProp = null;
                    PropertyInfo pi = typeParent.GetProperty(childACUrl);
                    if (pi == null)
                        childProp = acObject.ACUrlCommand(childACUrl) as IEnumerable<T>;
                    else
                        childProp = typeParent.InvokeMember(childACUrl, Global.bfGetProp, null, acObject, null) as IEnumerable<T>;
                    if (childProp == null)
                        return null;
                    return SearchWithDynQuery<T>(childProp, queryDefinition, mergeOption);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACQuery", "ACSelect<T>", msg);

                return null;
            }
        }


        public static IQueryable ACSelect(this IEnumerable list, ACQueryDefinition queryDefinition)
        {
            return _DynQueryMethod.MakeGenericMethod(new Type[] { queryDefinition.QueryType.ObjectType }).Invoke(null, new object[] { list, queryDefinition, MergeOption.AppendOnly }) as IQueryable;
        }

        public static IQueryable<T> ACSelect<T>(this IEnumerable<T> list, ACQueryDefinition queryDefinition, MergeOption mergeOption = MergeOption.AppendOnly) where T : class
        {
            return SearchWithDynQuery<T>(list, queryDefinition, mergeOption);
        }
        #endregion

        #region private Methods
        private static bool? _UseDynLINQ;
        private static bool UseDynLINQ
        {
            get
            {
                if (_UseDynLINQ.HasValue)
                    return _UseDynLINQ.Value;
                _UseDynLINQ = false;
                _UseDynLINQ = Database.Root?.Environment?.UseDynLINQ;
                return _UseDynLINQ.Value;
            }
        }

        /// <summary>
        /// Searches the data Q.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="queryDefinition">The query definition.</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>IQueryable{``0}.</returns>
        private static IQueryable<T> SearchWithDynQuery<T>(IEnumerable<T> source, ACQueryDefinition queryDefinition, MergeOption mergeOption)
        {
            var sourceQ = source.AsQueryable();
            IQueryable<T> resultQuery = null;

            //queryDefinition.QueryContext = context as IACObject;
            List<ObjectParameter> parameterList = queryDefinition.FilterParameters;


            if (string.IsNullOrEmpty(queryDefinition.LINQPredicateWhere) || parameterList == null || parameterList.Count <= 0)
            {
                if (String.IsNullOrEmpty(queryDefinition.LINQPredicateOrderBy))
                {
                    if (queryDefinition.TakeCount > 0)
                        resultQuery = sourceQ.Select(c => c).Take<T>(queryDefinition.TakeCount);
                    else
                        resultQuery = sourceQ.Select(c => c);
                }
                else
                {
                    if (queryDefinition.TakeCount > 0)
                        resultQuery = sourceQ.Select(c => c).OrderBy(queryDefinition.LINQPredicateOrderBy).Take<T>(queryDefinition.TakeCount);
                    else
                        resultQuery = sourceQ.Select(c => c).OrderBy(queryDefinition.LINQPredicateOrderBy);
                }
            }
            else
            {
                object[] filterArray = parameterList.Select(c => c.Value).ToArray();
                if (String.IsNullOrEmpty(queryDefinition.LINQPredicateOrderBy))
                {
                    if (queryDefinition.TakeCount > 0)
                        resultQuery = sourceQ.Where(queryDefinition.LINQPredicateWhere, filterArray).Take<T>(queryDefinition.TakeCount);
                    else
                        resultQuery = sourceQ.Where(queryDefinition.LINQPredicateWhere, filterArray);
                }
                else
                {
                    if (queryDefinition.TakeCount > 0)
                        resultQuery = sourceQ.Where(queryDefinition.LINQPredicateWhere, filterArray).OrderBy(queryDefinition.LINQPredicateOrderBy).Take<T>(queryDefinition.TakeCount);
                    else
                        resultQuery = sourceQ.Where(queryDefinition.LINQPredicateWhere, filterArray).OrderBy(queryDefinition.LINQPredicateOrderBy);
                }
            }

            // Hot to get a IQueryable from a Collection-Property: https://github.com/dotnet/efcore/issues/12893
            // This is not possible in EF7 - maybe in future versions https://github.com/dotnet/efcore/issues/16491
#if !EFCR
            ObjectQuery objectQuery = resultQuery as ObjectQuery;
            if ((objectQuery != null) && (mergeOption != MergeOption.AppendOnly))
                objectQuery.MergeOption = mergeOption;
#endif
            return resultQuery;
        }

        private static IQueryable<TEntity> SearchWithEntitySQL<TEntity>(DbSet<TEntity> source, IACObject context, ACQueryDefinition queryDefinition, string childACUrl, MergeOption mergeOption) where TEntity : class
        {
            IQueryable<TEntity> dynQuery;
            queryDefinition.QueryContext = context;
            if (ACQueryDefinition.C_SQLNamedParams)
            {
                dynQuery = source.FromSqlRaw<TEntity>(queryDefinition.EntitySQL, queryDefinition.SQLParameters.Select(c => new Microsoft.Data.SqlClient.SqlParameter(c.Name, c.Value)).ToArray());
            }
            else
            {
                dynQuery = source.FromSqlRaw<TEntity>(queryDefinition.EntitySQL, queryDefinition.SQLParameters.Select(c => c.Value).ToArray());
            }
            return dynQuery;
        }
#endregion
    }
}
