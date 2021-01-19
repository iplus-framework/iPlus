// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IAccess.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System.Linq;
using System.Data.Objects;

namespace gip.core.datamodel
{
    /// <summary>The generic form of IAccess.</summary>
    /// <typeparam name="T">A Entity Framework class</typeparam>
    /// <seealso cref="gip.core.datamodel.IAccess" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IAccessT'}de{'IAccessT'}", Global.ACKinds.TACInterface)]
    public interface IAccessT<T> : IAccess where T : class
    {
        /// <summary>
        /// Result of the NavSearch-Query
        /// </summary>
        /// <value>The nav object list.</value>
        [ACPropertyInfo(9999)]
        IList<T> NavList { get; }


        /// <summary>Executes a Query according to the filter and sort entries in ACQueryDefinition without changing the NavObjectList. The result is returned directly.</summary>
        /// <param name="searchWord">The search word.</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>A IQueryableIQueryable<T></returns>
        IQueryable<T> OneTimeSearchT(string searchWord, MergeOption mergeOption = MergeOption.AppendOnly);


        /// <summary>Invokes the OneTimeSearchT()-Method an returns the first element in the result</summary>
        /// <param name="searchWord">The search word.</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>The first element oft type T in the result</returns>
        T OneTimeSearchFirstOrDefaultT(string searchWord, MergeOption mergeOption = MergeOption.AppendOnly);
    }

}
