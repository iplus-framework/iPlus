using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace gip.core.datamodel
{
    /// <summary>Table for declaring languages. For each language, translations in ACClassMessage and ACClassText can be maintained.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, Const.VBLanguage, Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true, "", "BSOLanguage")]
    [ACPropertyEntity(9999, "VBNameTrans", Const.EntityNameTrans,  "", "", true, MinLength = 1)]
    [ACPropertyEntity(5, "VBKey", "en{'Key'}de{'Schlüssel'}",  "", "", true, MinLength = 1)]
    [ACPropertyEntity(1, "VBLanguageCode", "en{'Language Code'}de{'Sprachcode'}","", "", true)]
    [ACPropertyEntity(3, "IsTranslation", "en{'Translation'}de{'Übersetzung'}","", "", true)]
    [ACPropertyEntity(4, "SortIndex", Const.EntitySortSequence,"", "", true)]
    [ACPropertyEntity(5, "IsDefault", "en{'Default'}de{'Standard'}","", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBLanguage.ClassName, "en{'Language'}de{'Sprache'}", typeof(VBLanguage), VBLanguage.ClassName, "VBNameTrans", "SortIndex")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBLanguage>) })]
    public partial class VBLanguage
    {
        public const string ClassName = "VBLanguage";

        #region New/Delete
        public static VBLanguage NewACObject(Database database, IACObject parentACObject)
        {
            VBLanguage entity = new VBLanguage();
            entity.VBLanguageID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            entity.IsDefault = false;
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        public static VBLanguage DefaultVBLanguage(Database database)
        {
            try
            {
                VBLanguage state = database.VBLanguage.Where(c => c.IsDefault).FirstOrDefault();
                if (state == null)
                    state = database.VBLanguage.Where(c => c.VBLanguageCode == "en").FirstOrDefault();
                return state;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("VBLanguage", "DefaultVBLanguage", msg);

                return null;
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
        [ACPropertyInfo(9999)]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                return VBLanguageName;
            }
        }

        #endregion

        #region IACObjectEntity Members

        static public string KeyACIdentifier
        {
            get
            {
                return "VBKey";
            }
        }
        #endregion

        #region AdditionalProperties
        [ACPropertyInfo(1, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public String VBLanguageName
        {
            get
            {
                return Translator.GetTranslation(VBNameTrans);
            }
            set
            {
                VBNameTrans = Translator.SetTranslation(VBNameTrans, value);
                OnPropertyChanged("VBLanguageName");
            }
        }
        #endregion

        #region IEntityProperty Members

        bool bRefreshConfig = false;
        protected override void OnPropertyChanging<T>(T newValue, string propertyName, bool afterChange)
        {
            if (propertyName == nameof(XMLConfig))
            {
                string xmlConfig = newValue as string;
                if (afterChange)
                {
                    if (bRefreshConfig)
                        ACProperties.Refresh();
                }
                else
                {
                    bRefreshConfig = false;
                    if (!(String.IsNullOrEmpty(xmlConfig) && String.IsNullOrEmpty(XMLConfig)) && xmlConfig != XMLConfig)
                        bRefreshConfig = true;
                }
            }
            base.OnPropertyChanging(newValue, propertyName, afterChange);
        }

        #endregion

    }
}




