// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    /// <summary>Stores XAML for a user. It's used whne the user uses the snapshot-icon to store his last opened business objects. At the next logon his last snapshot will be restored.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'personalized layout'}de{'Personalisiertes Layout'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "VBUser", "en{'User'}de{'Benutzer'}", Const.ContextDatabase + "\\" + VBUser.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(2, "ACClassDesign", "en{'Design'}de{'Design'}", Const.ContextDatabase + "\\ACClassDesignList", "", true)]
    [ACPropertyEntity(9999, "XMLDesign", "en{'XMLDesign'}de{'de-XMLDesign'}")]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBUserACClassDesign.ClassName, "en{'Personalized layout'}de{'Personalisiertes Layout'}", typeof(VBUserACClassDesign), VBUserACClassDesign.ClassName, VBUser.ClassName + "\\VBUserName," + ACClassDesign.ClassName + "\\ACIdentifier", ACClassDesign.ClassName + "\\ACIdentifier")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBUserACClassDesign>) })]
    [NotMapped]
    public partial class VBUserACClassDesign : IACObjectEntity
    {
        public const string ClassName = "VBUserACClassDesign";

        #region New/Delete
        public static VBUserACClassDesign NewACObject(Database database, IACObject parentACObject)
        {
            VBUserACClassDesign entity = new VBUserACClassDesign();
            entity.VBUserACClassDesignID = Guid.NewGuid();
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            if (parentACObject is VBUser)
            {
                VBUser user = parentACObject as VBUser;
                entity.VBUserID = user.VBUserID;
                entity.VBUser = user;
            }
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
       }


        public static VBUserACClassDesign NewVBUserACClassDesign(Database database, VBUser vbUser, ACClassDesign acClassDesign)
        {
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            VBUserACClassDesign entity = VBUserACClassDesign.NewACObject(database, vbUser);
            if (acClassDesign != null)
            {
                entity.ACClassDesignID = acClassDesign.ACClassDesignID;
                entity.ACClassDesign = acClassDesign;
            }
            database.VBUserACClassDesign.Add(entity);
            return entity;
        }
        #endregion

        #region IACUrl Member

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                return VBUser.ACCaption + "->" + ACClassDesign.ACCaption;
            }
        }

        /// <summary>
        /// Returns VBUser
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to VBUser</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public override IACObject ParentACObject
        {
            get
            {
                return VBUser;
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
            base.EntityCheckAdded(user, context);
            if (VBUser == null)
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "Key",
                    Message = "Key",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "Key"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            return null;
        }


        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "ACClassDesign\\ACIdentifier";
            }
        }

        #endregion
    }
}
