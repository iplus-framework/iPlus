// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-08-2012
// ***********************************************************************
// <copyright file="IACType.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Base interface for the iPlus-Typesystem. It helps to read the Metadata or get informations about the type of a instance that is a derivation of IACObject.
    /// Classes that primarily implements IACType are: ACClass, ACClassProperty, ACClassMethod, ACClassDesign
    /// </summary>
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACType", "en{'Type'}de{'Typ'}", typeof(IACType), "ACType", "ACCaption,ACIdentifier", "ACIdentifier")]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACType'}de{'IACType'}", Global.ACKinds.TACInterface)]
    public interface IACType : IACObject
    {
        #region Type-Informations

        /// <summary>
        /// Primary Key of a Entity in the Database/Table
        /// (Uniqued Identifier of a type in the iPlus-Framework)
        /// </summary>
        Guid ACTypeID { get; }

        /// <summary>
        /// Returns Name/Description of this type including all translation-tuples
        /// </summary>
        String ACCaptionTranslation { get; set; }

        /// <summary>
        /// Group (More properties and methods have the same group id if they belongs logically to each other)
        /// </summary>
        [ACPropertyInfo(9999)]
        string ACGroup { get; }

        /// <summary>
        /// Sort order in a Parent-/Child-Relationship
        /// </summary>
        [ACPropertyInfo(9999)]
        Int16 SortIndex { get; }

        /// <summary>
        /// Comment
        /// </summary>
        [ACPropertyInfo(9999)]
        string Comment { get; }

        /// <summary>
        /// Gets the Data-Type of a Property or the Return-Type of a Method
        /// </summary>
        [ACPropertyInfo(9999)]
        ACClass ValueTypeACClass { get; }

        /// <summary>
        /// Category
        /// </summary>
        [ACPropertyInfo(9999)]
        Global.ACKinds ACKind { get; }

        /// <summary>
        /// Returns the .NET-Type (If Property is a generic it returns the inner type)
        /// </summary>
        Type ObjectType { get; }

        /// <summary>
        /// Returns the .NET-Type (If Property is a generic it returns the outer+inner type)
        /// </summary>
        Type ObjectFullType { get; }

        /// <summary>
        /// Returns the .NET-Type  of the parent object in a composition tree
        /// </summary>
        Type ObjectTypeParent { get; }

        /// <summary>
        /// Is this type under rightmanagment
        /// </summary>
        Boolean IsRightmanagement { get; }

        #endregion


        #region Class and Hierarchy

        /// <summary>
        /// Returns first Member with this name in complete class hierarchy.
        /// 1. Searches for Child-Classes in Composition-Tree
        /// 2. Searches for Properties
        /// 3. Searches for Methods
        /// THREAD-SAFE while using QueryLock_1X000
        /// </summary>
        /// <param name="acIdentifier"></param>
        /// <param name="forceRefreshFromDB"></param>
        /// <returns></returns>
        IACType GetMember(string acIdentifier, bool forceRefreshFromDB = false);

        /// <summary>
        /// Returns all Properties of a Entity-Object which are visible for presentation
        /// THREAD-SAFE while using QueryLock_1X000
        /// </summary>
        /// <param name="maxColumns">The max columns.</param>
        /// <param name="acColumns">The ac columns.</param>
        /// <returns>List{ACColumnItem}.</returns>
        List<ACColumnItem> GetColumns(int maxColumns = 9999, string acColumns = null);
        #endregion


        #region Design

        /// <summary>
        /// Returns the first Design which matches the identifier over complete class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="acIdentifier"></param>
        /// <param name="forceRefreshFromDB"></param>
        /// <returns></returns>
        ACClassDesign GetDesign(string acIdentifier, bool forceRefreshFromDB = false);

        /// <summary>
        /// Returns the first Design which matches the criteria
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="acObject"></param>
        /// <param name="acUsage"></param>
        /// <param name="acKind"></param>
        /// <param name="vbDesignName"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        ACClassDesign GetDesign(IACObject acObject, Global.ACUsages acUsage, Global.ACKinds acKind, string vbDesignName = "", MsgWithDetails msg = null);

        /// <summary>
        /// Returns all Designs over complete class hierarchy including overridden Designs
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>ACClassDesign List</value>
        IEnumerable<ACClassDesign> Designs { get; }

        #endregion


        #region Configuration

        /// <summary>
        /// Gets the class config list.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <returns>IEnumerable{IACConfig}.</returns>
        IEnumerable<IACConfig> GetConfigListOfType(IACObjectEntity acObject = null);
        
        #endregion


        #region Meta informations

        /// <summary>
        /// Gets the AC path.
        /// </summary>
        /// <param name="first">if set to <c>true</c> [first].</param>
        /// <returns>String.</returns>
        String GetACPath(bool first);

        /// <summary>
        /// Returns the Signature of a dynamic parameterized Method or constructor
        /// THREAD-SAFE
        /// </summary>
        /// <param name="acUrl"></param>
        /// <param name="attachToObject"></param>
        /// <returns></returns>
        ACMethod ACUrlACTypeSignature(string acUrl, IACObject attachToObject = null);

        /// <summary>
        /// Resturns the dynamic signature of a Class-Constructor or as Method
        /// </summary>
        /// <returns></returns>
        ACMethod TypeACSignature();


        /// <summary>
        /// Returns the translated Name/Description of this type for a given  I18N-languagecode
        /// </summary>
        /// <param name="VBLanguageCode">The md language code.</param>
        /// <returns>System.String.</returns>
        string GetTranslation(string VBLanguageCode);

        #endregion
    }
}
