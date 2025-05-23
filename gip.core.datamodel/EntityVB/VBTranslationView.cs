// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTranslationView'}de{'VBTranslationView'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "ACProjectName", "en{'Parent'}de{'Parent'}", "", "", true)]
    [ACPropertyEntity(2, "TableName", "en{'Table name'}de{'Tabellename'}", "", "", true)]
    [ACPropertyEntity(3, "MandatoryID", "en{'MandatoryID'}de{'MandatoryID'}", "", "", true)]
    [ACPropertyEntity(4, "MandatoryACIdentifier", "en{'MandatoryACIdentifier'}de{'MandatoryACIdentifier'}", "", "", true)]
    [ACPropertyEntity(5, "MandatoryACURLCached", "en{'MandatoryACURLCached'}de{'MandatoryACURLCached'}", "", "", true)]
    [ACPropertyEntity(6, "ID", "en{'ID'}de{'ID'}", "", "", true)]
    [ACPropertyEntity(7, "ACIdentifier", "en{'ACIdentifier'}de{'ACIdentifier'}", "", "", true)]
    [ACPropertyEntity(8, "TranslationValue", "en{'Translation'}de{'Übersetzung'}", "", "", true)]
    [ACPropertyEntity(498, Const.EntityUpdateName, Const.EntityTransUpdateName)]
    [ACPropertyEntity(499, Const.EntityUpdateDate, Const.EntityTransUpdateDate)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBTranslationView.ClassName, "en{'VBTranslationView'}de{'VBTranslationView'}", typeof(VBTranslationView), VBTranslationView.ClassName, "ACIdentifier", "ACIdentifier")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBTranslationView>) })]
    [NotMapped]
    public partial class VBTranslationView
    {

        public const string ClassName = "VBTranslationView";


        #region EditTranslation
        [NotMapped]
        private TranslationPair _SelectedEditTranslation;
        /// <summary>
        /// Selected property for TranslationPair
        /// </summary>
        /// <value>The selected Translation</value>
        [ACPropertySelected(9999, "EditTranslation", "en{'TODO: Translation'}de{'TODO: Translation'}")]
        [NotMapped]
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

        [NotMapped]
        private List<TranslationPair> _EditTranslationList;
        /// <summary>
        /// List property for TranslationPair
        /// </summary>
        /// <value>The Translation list</value>
        [ACPropertyList(9999, "EditTranslation")]
        [DataMemberAttribute()]
        [NotMapped]
        public List<TranslationPair> EditTranslationList
        {
            get
            {
                return _EditTranslationList;
            }
            set
            {
                _EditTranslationList = value;
            }
        }

        public void SetTranslationList(List<VBLanguage> vbLanguageList)
        {
            _EditTranslationList = LoadEditTranslationList(vbLanguageList, TranslationValue);
            OnPropertyChanged("EditTranslationList");
        }
        #endregion


        public override string ToString()
        {
            return string.Format(@"[{0}] {1}", TableName, ACIdentifier);
        }

        public static List<TranslationPair> LoadEditTranslationList(List<VBLanguage> vbLanguageList, string translationValue)
        {
            List<TranslationPair> list = new List<TranslationPair>();
            if (!string.IsNullOrEmpty(translationValue))
                foreach (var vBLanguage in vbLanguageList)
                {
                    if (translationValue.Contains(vBLanguage.VBLanguageCode + "{"))
                    {
                        TranslationPair translationPair = new TranslationPair()
                        {
                            LangCode = vBLanguage.VBLanguageCode,
                            Translation = Translator.GetTranslation("", translationValue, vBLanguage.VBLanguageCode)
                        };
                        list.Add(translationPair);
                    }
                }
            return list;
        }

    }
}
