// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACAssembly.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    /// <summary>
    /// Table that stores information about the assemblies, that were registered to the iPlus-Type-System.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Assembly'}de{'Assembly'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "AssemblyName", "en{'Assembly name'}de{'Assembly Name'}","", "", true)]
    [ACPropertyEntity(2, "LastReflectionDate", "en{'Last reflection'}de{'Letzte Reflektion'}","", "", true)]
    [ACPropertyEntity(3, "AssemblyDate", "en{'Date of assembly'}de{'Datum der dll'}","", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACAssembly.ClassName, "en{'Text'}de{'Text'}", typeof(ACAssembly), ACAssembly.ClassName, "AssemblyName", "AssemblyName")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACAssembly>) })]
    [NotMapped]
    public partial class ACAssembly : IACObjectEntity
    {
        public const string ClassName = "ACAssembly";

        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <returns>ACAssembly.</returns>
        public static ACAssembly NewACObject(Database database, IACObject parentACObject)
        {
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            ACAssembly entity = new ACAssembly();
            entity.Context = database;
            entity.ACAssemblyID = Guid.NewGuid();
            return entity;
        }

        #endregion

        #region IACObject Member

        /// <summary>
        /// Gets the AC URL.
        /// </summary>
        /// <value>The AC URL.</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public string ACUrl
        {
            get
            {
                return GetACUrl();
            }
        }

        #endregion

        #region IACObjectEntity Members
        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "AssemblyName";
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(4, "", "en{'Description'}de{'ACCaption'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                return AssemblyName;
            }
        }

        #endregion
    }
}
