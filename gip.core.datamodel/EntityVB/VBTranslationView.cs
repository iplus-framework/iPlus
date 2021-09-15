using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        [DataMemberAttribute()]
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
            _EditTranslationList = Translator.LoadEditTranslationList(vbLanguageList, TranslationValue);
            OnPropertyChanged("EditTranslationList");
        }
        #endregion


        public override string ToString()
        {
            return string.Format(@"[{0}] {1}", TableName, ACIdentifier);
        }
    }
}
