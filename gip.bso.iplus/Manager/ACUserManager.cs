// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACUserManager.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.bso.iplus
{
    public class ACUserManager : ACManagerBase
    {
        public ACUserManager(IACEntityObjectContext db, IRoot root)
            : base(db, root)
        {
        }

        #region Manager->Modify->VBUser
        public Database Database
        {
            get
            {
                // Für "normale" Anwendungen
                return this.ObjectContext as Database;
            }
        }

        /// <summary>
        /// Loads the user.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <returns>VBUser.</returns>
        public VBUser LoadUser(Guid userID)
        {
            return Database.VBUser.Where(c => c.VBUserID == userID).AutoMergeOption().FirstOrDefault();
        }

        /// <summary>
        /// News the user.
        /// </summary>
        /// <returns>VBUser.</returns>
        public VBUser NewUser()
        {
            string secondaryKey = ACRoot.SRoot.NoManager.GetNewNo(Database, typeof(VBUser), VBUser.NoColumnName, VBUser.FormatNewNo, null);
            VBUser user = VBUser.NewACObject(Database, null, secondaryKey);
            Database.VBUser.Add(user);
            _ = VBUserInstance.NewACObject(Database, user);
            return user;
        }
        #endregion

        #region Manager->Search
        /// <summary>
        /// Gets the name of the user by.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>VBUser.</returns>
        public VBUser GetUserByName(string userName)
        {
            return Database.VBUser.Where(c => c.VBUserName == userName).FirstOrDefault();
        }

        #endregion
    }
}
