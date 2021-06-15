﻿using System;
using System.Collections.Generic;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTranslationView'}de{'VBTranslationView'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "ACProjectName", "en{'Parent'}de{'Parent'}", "", "", true)]
    [ACPropertyEntity(2, "TableName", "en{'Table name'}de{'Tabellename'}", "", "", true)]
    [ACPropertyEntity(3, "MandatoryID", "en{'MandatoryID'}de{'MandatoryID'}", "", "", true)]
    [ACPropertyEntity(3, "MandatoryACIdentifier", "en{'MandatoryACIdentifier'}de{'MandatoryACIdentifier'}", "", "", true)]
    [ACPropertyEntity(3, "ID", "en{'ID'}de{'ID'}", "", "", true)]
    [ACPropertyEntity(4, "ACIdentifier", "en{'ACIdentifier'}de{'ACIdentifier'}", "", "", true)]
    [ACPropertyEntity(5, "TranslationValue", "en{'Translation'}de{'Übersetzung'}", "", "", true)]
    [ACPropertyEntity(498, Const.EntityUpdateName, Const.EntityTransUpdateName)]
    [ACPropertyEntity(499, Const.EntityUpdateDate, Const.EntityTransUpdateDate)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBTranslationView.ClassName, "en{'VBTranslationView'}de{'VBTranslationView'}", typeof(VBTranslationView), VBTranslationView.ClassName, "ACIdentifier", "ACIdentifier")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBTranslationView>) })]
    public partial class VBTranslationView
    {

        public const string ClassName = "VBTranslationView";


        #region EditTranslation
        private TranslationPair _SelectedEditTranslation;
        /// <summary>
        /// Selected property for TranslationPair
        /// </summary>
        /// <value>The selected Translation</value>
        [ACPropertySelected(9999, "EditTranslation", "en{'TODO: Translation'}de{'TODO: Translation'}")]
        public TranslationPair SelectedEditTranslation
        {
            get
            {
                return _SelectedEditTranslation;
            }
            set
            {
                if (_SelectedEditTranslation != value)
                {
                    _SelectedEditTranslation = value;
                    OnPropertyChanged("SelectedEditTranslation");
                }
            }
        }


        private List<TranslationPair> _EditTranslationList;
        /// <summary>
        /// List property for TranslationPair
        /// </summary>
        /// <value>The Translation list</value>
        [ACPropertyList(9999, "EditTranslation")]
        public List<TranslationPair> EditTranslationList
        {
            get
            {
                return _EditTranslationList;
            }
        }
       
        private List<TranslationPair> LoadEditTranslationList(List<VBLanguage> vbLanguageList)
        {
            List<TranslationPair> list = new List<TranslationPair>();
            foreach (var vBLanguage in vbLanguageList)
            {
                if (TranslationValue.Contains(vBLanguage.VBLanguageCode + "{"))
                {
                    TranslationPair translationPair = new TranslationPair()
                    {
                        LangCode = vBLanguage.VBLanguageCode,
                        Translation = Translator.GetTranslation("", TranslationValue, vBLanguage.VBLanguageCode)
                    };
                    list.Add(translationPair);
                }
            }
            return list;
        }

        public void SetTranslationList(List<VBLanguage> vbLanguageList)
        {
            _EditTranslationList  = LoadEditTranslationList(vbLanguageList);
            OnPropertyChanged("EditTranslationList");
        }
        #endregion


        public override string ToString()
        {
            return string.Format(@"[{0}] {1}", TableName, ACIdentifier);
        }
    }
}
