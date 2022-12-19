// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACPackage.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACPackage
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Package'}de{'Packet'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "ACPackageName", "en{'Packagename'}de{'Paketname'}","", "", true)]
    [ACPropertyEntity(2, "Comment", "en{'Comment'}de{'Bemerkung'}","", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACPackage.ClassName, "en{'Package'}de{'Packet'}", typeof(ACPackage), ACPackage.ClassName, "ACPackageName", "ACPackageName")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACPackage>) })]
    [NotMapped]
    public partial class ACPackage : IACObjectEntity
    {
        public const string ClassName = "ACPackage";

        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <returns>ACPackage.</returns>
        public static ACPackage NewACObject(Database database, IACObject parentACObject)
        {
            ACPackage entity = new ACPackage();
            entity.ACPackageID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            entity.Comment = "";
            entity.BranchNo = 0;
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        /// <summary>
        /// Gets the AC package.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acPackageName">Name of the ac package.</param>
        /// <returns>ACPackage.</returns>
        public static ACPackage GetACPackage(Database database, string acPackageName)
        {
            if (InsertedPackages == null)
                InsertedPackages = database.ACPackage.ToList();

            //var package = database.ACPackage.FirstOrDefault(c => c.ACPackageName == acPackageName);
            //if (package != null)
            //    return package;

            ACPackage package = InsertedPackages.FirstOrDefault(c => c.ACPackageName == acPackageName);
            if (package != null)
                return package;

            ACPackage acPackage = ACPackage.NewACObject(database, null);
            acPackage.ACPackageName = acPackageName;
            database.ACPackage.Add(acPackage);
            InsertedPackages.Add(acPackage);
            return acPackage;
        }

        [NotMapped]
        private static List<ACPackage> _InsertedPackages;
        [NotMapped]
        public static List<ACPackage> InsertedPackages
        {
            get
            {
                return _InsertedPackages;
            }
            private set
            {
                _InsertedPackages = value;
            }
        }

        #endregion

        #region IACUrl Member

        public override string ToString()
        {
            return ACCaption;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                return ACPackageName;
            }
        }

        #endregion

        #region IACObjectEntity Members
        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context)
        {
            if (string.IsNullOrEmpty(ACPackageName))
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "ACPackageName",
                    Message = "ACPackageName is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "ACPackageName"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            base.EntityCheckAdded(user, context);
            return null;
        }

        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for changed entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckModified(string user, IACEntityObjectContext context)
        {
            base.EntityCheckModified(user, context);
            return null;
        }


        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "ACPackageName";
            }
        }

        #endregion

        #region Helper

        [NotMapped]
        public bool IsLicensed
        {
            get
            {
                if (Database.Root == null || !Database.Root.Initialized)
                    return true;
                return Database.Root.Environment.License.IsPackageLicensed(this);
            }
        }

        public static List<string> GetAllNonVBPackages(Database db)
        {
            List<string> tempList = null;
            tempList = db.ACPackage.Select(c => c.ACPackageName).Where(x => x != Const.PackName_VarioLicense).Except(VBPackages.Select(c => c.Item1)).ToList();
            return tempList;
        }

        [NotMapped]
        private static List<Tuple<string,short>> _VBPackages;
        [NotMapped]
        public static List<Tuple<string, short>> VBPackages
        {
            get
            {
                if(_VBPackages == null)
                {
                    _VBPackages = new List<Tuple<string, short>>();
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioAutomation,0));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioCompany,1));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioDevelopment,2));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioFacility,3));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioLogistics,4));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioManufacturing,5));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioMaterial, 6));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioPurchase,7));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioSales,8));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioScheduling,9));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioSystem,10));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_VarioTest,11));
                    _VBPackages.Add(new Tuple<string,short>(Const.PackName_System, 12));
                }
                return _VBPackages;
            }
        }

        #endregion

    }
}
