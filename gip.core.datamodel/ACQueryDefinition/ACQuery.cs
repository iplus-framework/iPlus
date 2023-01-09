// ***********************************************************************
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
            MethodInfo miDynQuery = _DynQueryMethod.MakeGenericMethod(new Type[] { queryDefinition.QueryType.ObjectType });
            MethodInfo miEntitySQL = _EntityQueryMethod.MakeGenericMethod(new Type[] { queryDefinition.QueryType.ObjectType });
            Type typeParent = acObject.GetType();

            try
            {
                if (acObject is IACEntityObjectContext)
                {
                    PropertyInfo pi = typeParent.GetProperty(childACUrl);
                    if (pi != null)
                    {
#if !EFCR
                        if (typeof(ObjectQuery).IsAssignableFrom(pi.PropertyType))
                            return miEntitySQL.Invoke(null, new object[] { acObject, queryDefinition, childACUrl, mergeOption }) as IQueryable;
                        else
                            return miDynQuery.Invoke(null, new object[] { pi.GetValue(acObject, null), queryDefinition, mergeOption }) as IQueryable;
#endif
                        throw new NotImplementedException();
                    }
                    else
                    {
                        var childProp = acObject.ACUrlCommand(childACUrl);
                        if (childProp == null)
                            return null;
                        return miDynQuery.Invoke(null, new object[] { childProp, queryDefinition, mergeOption }) as IQueryable;
                    }
                }
                else
                {
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
                    if (pi != null)
                    {
                        return SearchWithEntitySQL<T>(acObject, queryDefinition, childACUrl, mergeOption);
                    }
                    else
                    {
                        IEnumerable<T> childProp = acObject.ACUrlCommand(childACUrl) as IEnumerable<T>;
                        if (childProp == null)
                            return null;
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

#if !EFCR
            ObjectQuery objectQuery = resultQuery as ObjectQuery;
            if ((objectQuery != null) && (mergeOption != MergeOption.AppendOnly))
                objectQuery.MergeOption = mergeOption;
#endif
            return resultQuery;
        }

        private static IQueryable<T> SearchWithEntitySQL<T>(IACObject context, ACQueryDefinition queryDefinition, string childACUrl, MergeOption mergeOption)
        {
            //if () / if merge ako ne postoje sve 3 opcije tu postaviti upit
            queryDefinition.QueryContext = context;

            ObjectQuery<T> dynQuery = new ObjectQuery<T>(queryDefinition.EntitySQL, context as DbContext, mergeOption);
            List<ObjectParameter> parameterList = queryDefinition.FilterParameters;

            int parameterCount = 0;
            if (parameterList != null && parameterList.Any())
            {
                foreach (ObjectParameter parameter in parameterList)
                {
                    dynQuery.Parameters.Add(parameter);
                    parameterCount++;
                }
            }

            // Take Count moved to ACAccess
            //if (queryDefinition.TakeCount > 0)
            //    dynQuery.Parameters.Add(new ObjectParameter("p" + parameterCount.ToString(), queryDefinition.TakeCount));

            throw new NotImplementedException();
#if !EFCR
            return dynQuery;
#endif
        }
#endregion
    }
}
