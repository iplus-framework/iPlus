// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACClassMessage.cs" company="gip mbh, Oftersheim, Germany">
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
    /// <summary>ACClassMessage is a translation table for messages. ACClassMessages, like ACClassMethod and ACClassProperty, belong to an ACClass.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Message'}de{'Meldung'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, Const.ACIdentifierPrefix, "en{'Message-ID'}de{'Meldungs-ID'}", "", "", true)]
    [ACPropertyEntity(2, "ACCaptionTranslation", "en{'Translation'}de{'Übersetzung'}","", "", true)]
    [ACPropertyEntity(3, "ACIdentifierKey", "en{'Key'}de{'Schlüssel'}","", "", true)]
    [ACPropertyEntity(4, ACClass.ClassName, "en{'Class'}de{'Klasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(5, "IsSystem", "en{'System'}de{'System'}","", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassMessage.ClassName, "en{'Text'}de{'Text'}", typeof(ACClassMessage), ACClassMessage.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassMessage>) })]
    [NotMapped]
    public partial class ACClassMessage : IACObjectEntityWithCheckTrans, ICloneable, IACClassEntity
    {
        public const string ClassName = "ACClassMessage";

        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <returns>ACClassMessage.</returns>
        public static ACClassMessage NewACObject(Database database, IACObject parentACObject)
        {
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            ACClassMessage entity = new ACClassMessage();
            entity.Database = database;
            entity.ACClassMessageID = Guid.NewGuid();
            entity.BranchNo = 0;
            if (parentACObject is ACClass)
            {
                entity.ACClass = parentACObject as ACClass;
            }

            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        /// <summary>
        /// Gets the unique name identifier.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="isSystemMessage">if set to <c>true</c> [is system message].</param>
        /// <returns>System.String.</returns>
        public static string GetUniqueNameIdentifier(Database database, string prefix, bool isSystemMessage)
        {
            string acIdentifierMin = prefix + (isSystemMessage ? "00001" : "50000");
            string acIdentifierMax = prefix + (isSystemMessage ? "49999" : "99999");

            var query = database.ACClassMessage.Where(c => string.Compare(c.ACIdentifier, acIdentifierMin) >= 0 && string.Compare(c.ACIdentifier, acIdentifierMax) <= 0).Select(c => c.ACIdentifier);
            if (query.Any())
            {
#if !EFCR
                string maxName = query.Max();
                int index;
                if (!Int32.TryParse(maxName.Substring(maxName.Length - 5), out index))
                    index = 0;
                index++;
                return string.Format("{0}{1:D5}", prefix, index);
#endif
            }
            return acIdentifierMin;
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
        /// Returns ACClass, where this message belongs to
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
        public Database Database
        {
            get;
            set;
        }

        public void OnObjectMaterialized(Database db)
        {
            if (Database == null)
                Database = db;
        }

        #endregion

        #region Clone

        public object Clone()
        {
            ACClassMessage clonedObject = new ACClassMessage();
            clonedObject.ACClassMessageID = this.ACClassMessageID;
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
