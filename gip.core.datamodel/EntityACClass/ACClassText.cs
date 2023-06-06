// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACClassText.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    /// <summary>ACClassText is a translation table for texts. ACClassText, like ACClassMethod and ACClassProperty, belong to an ACClass.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Text'}de{'Text'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, Const.ACIdentifierPrefix, "en{'Text-ID'}de{'Text-ID'}", "", "", true)]
    [ACPropertyEntity(2, "ACCaptionTranslation", "en{'Translation'}de{'Übersetzung'}","", "", true)]
    [ACPropertyEntity(3, "ACIdentifierKey", "en{'Key'}de{'Schlüssel'}","", "", true)]
    [ACPropertyEntity(4, ACClass.ClassName, "en{'Class'}de{'Klasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(5, "IsSystem", "en{'System'}de{'System'}","", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassText.ClassName, "en{'Text'}de{'Text'}", typeof(ACClassText), ACClassText.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassText>) })]
    [NotMapped]
    public partial class ACClassText : IACObjectEntityWithCheckTrans, ICloneable, IACClassEntity
    {
        public const string ClassName = "ACClassText";

        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <returns>ACClassText.</returns>
        public static ACClassText NewACObject(Database database, IACObject parentACObject)
        {
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            ACClassText entity = new ACClassText();
            entity.ACClassTextID = Guid.NewGuid();
            if (parentACObject is ACClass)
            {
                entity.ACClass = parentACObject as ACClass;
            }
            entity.BranchNo = 0;
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        #endregion

        #region IACUrl Member

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

        /// <summary>
        /// Returns ACClass, where this text belongs to
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>Reference to ACClass</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public override IACObject ParentACObject
        {
            get
            {
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return ACClass;
                }
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
            if (string.IsNullOrEmpty(ACIdentifier))
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = Const.ACIdentifierPrefix,
                    Message = "ACIdentifier is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", Const.ACIdentifierPrefix), 
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
                return Const.ACIdentifierPrefix;
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(4, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                return Translator.GetTranslation(ACIdentifier, ACCaptionTranslation);
            }
            set
            {
                this.OnACCaptionChanging(value);
                // this.ReportPropertyChanging(Const.ACCaptionPrefix);
                ACCaptionTranslation = Translator.SetTranslation(ACCaptionTranslation, value);
                //this.ReportPropertyChanged(Const.ACCaptionPrefix);
                OnPropertyChanged(Const.ACCaptionPrefix);
                this.OnACCaptionChanged();
            }
        }
        partial void OnACCaptionChanging(string value);
        partial void OnACCaptionChanged();


        /// <summary>
        /// Method for getting the translated text from ACCaptionTranslation
        /// </summary>
        /// <param name="VBLanguageCode">I18N-code</param>
        /// <returns>Translated text</returns>
        public string GetTranslation(string VBLanguageCode)
        {
            return Translator.GetTranslation(ACIdentifier, ACCaptionTranslation, VBLanguageCode);
        }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>The database.</value>
        [NotMapped]
        public Database Database
        {
            get
            {
                return Context as Database;
            }
        }

        #endregion

        #region Clone

        public object Clone()
        {
            ACClassText clonedObject = new ACClassText();
            clonedObject.ACClassTextID = this.ACClassTextID;
            clonedObject.ACClassID = this.ACClassID;
            clonedObject.ACIdentifier = this.ACIdentifier;
            clonedObject.ACIdentifierKey = this.ACIdentifierKey;
            clonedObject.ACCaptionTranslation = this.ACCaptionTranslation;
            clonedObject.BranchNo = this.BranchNo;
            return clonedObject;
        }


        #endregion
    }
}
