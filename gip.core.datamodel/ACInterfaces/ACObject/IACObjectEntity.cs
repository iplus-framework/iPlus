// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACObjectEntity.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

namespace gip.core.datamodel
{
    /// <summary>Interface for Entity-Framework classes</summary>
    /// <seealso cref="gip.core.datamodel.IACObject" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACObjectEntity'}de{'IACObjectEntity'}", Global.ACKinds.TACInterface)]
    public interface IACObjectEntity : IACObject
    {
        /// <summary>Returns the ACUrl for this runtime instance.</summary>
        /// <param name="rootACObject">If null, then a absolute ACUrl will be returned. Else a relative url to the passed object.</param>
        /// <returns>ACUrl as string</returns>
        string GetACUrlComponent(IACObject rootACObject = null);


        /// <summary>Deletes this entity-object from the database</summary>
        /// <param name="database">Entity-Framework databasecontext</param>
        /// <param name="withCheck">If set to true, a validation happens before deleting this EF-object. If Validation fails message ís returned.</param>
        /// <param name="softDelete">If set to true a delete-Flag is set in the dabase-table instead of a physical deletion. If  a delete-Flag doesn't exit in the table the record will be deleted.</param>
        /// <returns>If a validation or deletion failed a message is returned. NULL if sucessful.</returns>
        MsgWithDetails DeleteACObject(IACEntityObjectContext database, bool withCheck, bool softDelete = false);


        /// <summary>Check if entity-object can be deleted from the database</summary>
        /// <param name="database">Entity-Framework databasecontext</param>
        /// <returns>If deletion is not allowed or the validation failed a message is returned. NULL if sucessful.</returns>
        MsgWithDetails IsEnabledDeleteACObject(IACEntityObjectContext database);


        /// <summary>
        /// Returns a related EF-Object which is in a Child-Relationship to this.
        /// </summary>
        /// <param name="className">Classname of the Table/EF-Object</param>
        /// <param name="filterValues">Search-Parameters</param>
        /// <returns>A Entity-Object as IACObject</returns>
        IACObject GetChildEntityObject(string className, params string[] filterValues);


        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context);


        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for changed entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        IList<Msg> EntityCheckModified(string user, IACEntityObjectContext context);
    }

}
