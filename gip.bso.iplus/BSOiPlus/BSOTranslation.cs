using gip.core.autocomponent;
using gip.core.datamodel;
using System.Collections.Generic;
using System.Linq;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'Translation'}de{'Translation'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACPackage.ClassName)]
    public class BSOTranslation : ACBSO
    {
        #region c'tors

        public BSOTranslation(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {

            return base.ACInit(startChildMode);
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Filter Data

        private bool? _FilterOnlyACClassTables;
        [ACPropertyInfo(405, "FilterOnlyACClassTables", "en{'Only ACClass tables'}de{'Nur ACClass Tabellen'}")]
        public bool? FilterOnlyACClassTables
        {
            get
            {
                return _FilterOnlyACClassTables;
            }
            set
            {
                if (_FilterOnlyACClassTables != value)
                {
                    _FilterOnlyACClassTables = value;
                    OnPropertyChanged("FilterOnlyACClassTables");
                }
            }
        }

        private bool? _FilterOnlyMDTables;
        [ACPropertyInfo(405, "FilterOnlyMDTables", "en{'Only MD Tables'}de{'Nur MD Tabellen'}")]
        public bool? FilterOnlyMDTables
        {
            get
            {
                return _FilterOnlyMDTables;
            }
            set
            {
                if (_FilterOnlyMDTables != value)
                {
                    _FilterOnlyMDTables = value;
                    OnPropertyChanged("FilterOnlyMDTables");
                }
            }
        }


        private string _FilterClassACIdentifier;
        [ACPropertyInfo(407, "FilterClassACIdentifier", "en{'Class ACIdentifier'}de{'Class ACIdentifier'}")]
        public string FilterClassACIdentifier
        {
            get
            {
                return _FilterClassACIdentifier;
            }
            set
            {
                if (_FilterClassACIdentifier != value)
                {
                    _FilterClassACIdentifier = value;
                    OnPropertyChanged("FilterClassACIdentifier");
                }
            }
        }

        private string _FilterACIdentifier;
        [ACPropertyInfo(410, "FilterACIdentifier", "en{'ACIdentifier'}de{'ACIdentifier'}")]
        public string FilterACIdentifier
        {
            get
            {
                return _FilterACIdentifier;
            }
            set
            {
                if (_FilterACIdentifier != value)
                {
                    _FilterACIdentifier = value;
                    OnPropertyChanged("FilterACIdentifier");
                }
            }
        }

        private string _FilterTranslation;
        [ACPropertyInfo(411, "FilterTranslation", "en{'Translation'}de{'Translation'}")]
        public string FilterTranslation
        {
            get
            {
                return _FilterTranslation;
            }
            set
            {
                if (_FilterTranslation != value)
                {
                    _FilterTranslation = value;
                    OnPropertyChanged("FilterTranslation");
                }
            }
        }

        #endregion

        #region Translation

        private VBTranslationView _SelectedTranslation;
        /// <summary>
        /// Selected property for VBTranslationView
        /// </summary>
        /// <value>The selected Translations</value>
        [ACPropertySelected(401, "Translation", "en{'TODO: Translation'}de{'TODO: Translation'}")]
        public VBTranslationView SelectedTranslation
        {
            get
            {
                return _SelectedTranslation;
            }
            set
            {
                if (_SelectedTranslation != value)
                {
                    _SelectedTranslation = value;
                    OnPropertyChanged("SelectedTranslation");

                    _EditTranslationList = LoadEditTranslationList();
                    OnPropertyChanged("EditTranslationList");
                }
            }
        }


        private List<VBTranslationView> _TranslationList;
        /// <summary>
        /// List property for VBTranslationView
        /// </summary>
        /// <value>The Translations list</value>
        [ACPropertyList(402, "Translation")]
        public List<VBTranslationView> TranslationList
        {
            get
            {
                return _TranslationList;
            }
        }

        private List<VBTranslationView> LoadTranslationList()
        {
            return (Database as iPlusV4_Entities)
                .udpTranslation(FilterOnlyACClassTables,
                FilterOnlyMDTables,
                FilterClassACIdentifier,
                FilterACIdentifier,
                FilterTranslation)
                .ToList();
        }
        #endregion

        #region command
        [ACMethodInfo("Translation", "en{'Search'}de{'Suche'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void Search()
        {
            if (!IsEnabledSearch()) return;
            _TranslationList = LoadTranslationList();
            OnPropertyChanged("TranslationList");
        }

        public bool IsEnabledSearch()
        {
            return !string.IsNullOrEmpty(FilterClassACIdentifier)
                || !string.IsNullOrEmpty(FilterACIdentifier)
                || !string.IsNullOrEmpty(FilterTranslation);
        }

        #endregion

        #region Lang


        #region VBLanguage
        private VBLanguage _SelectedVBLanguage;
        /// <summary>
        /// Selected property for VBLanguage
        /// </summary>
        /// <value>The selected VBLanguage</value>
        [ACPropertySelected(9999, "VBLanguage", Const.VBLanguage)]
        public VBLanguage SelectedVBLanguage
        {
            get
            {
                return _SelectedVBLanguage;
            }
            set
            {
                if (_SelectedVBLanguage != value)
                {
                    _SelectedVBLanguage = value;
                    OnPropertyChanged("SelectedVBLanguage");
                }
            }
        }


        private List<VBLanguage> _VBLanguageList;
        /// <summary>
        /// List property for VBLanguage
        /// </summary>
        /// <value>The VBLanguage list</value>
        [ACPropertyList(9999, "VBLanguage")]
        public List<VBLanguage> VBLanguageList
        {
            get
            {
                if (_VBLanguageList == null)
                    _VBLanguageList = LoadVBLanguageList();
                return _VBLanguageList;
            }
        }

        private List<VBLanguage> LoadVBLanguageList()
        {
            List<VBLanguage> vBLanguages = new List<VBLanguage>();
            using (Database db = new core.datamodel.Database())
            {
                vBLanguages = db.VBLanguage.Where(c => c.IsTranslation).OrderBy(c => c.SortIndex).ToList();
            }
            return vBLanguages;
        }
        #endregion


        #endregion

        #region EditTranslation
        private TranslationPair _SelectedEditTranslation;
        /// <summary>
        /// Selected property for TranslationPair
        /// </summary>
        /// <value>The selected EditTranslation</value>
        [ACPropertySelected(403, "EditTranslation", "en{'TODO: EditTranslation'}de{'TODO: EditTranslation'}")]
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
        /// <value>The EditTranslation list</value>
        [ACPropertyList(404, "EditTranslation")]
        public List<TranslationPair> EditTranslationList
        {
            get
            {
                return _EditTranslationList;
            }
        }

        private List<TranslationPair> LoadEditTranslationList()
        {
            if (SelectedTranslation == null) return null;

            List<TranslationPair> list = new List<TranslationPair>();
            foreach (var vBLanguage in VBLanguageList)
            {
                if (SelectedTranslation.TranslationValue.Contains(vBLanguage.VBLanguageCode + "{"))
                {
                    TranslationPair translationPair = new TranslationPair()
                    {
                        LangCode = vBLanguage.VBLanguageCode,
                        Translation = Translator.GetTranslation("", SelectedTranslation.TranslationValue, vBLanguage.VBLanguageCode)
                    };
                    list.Add(translationPair);
                }
            }
            return list;
        }


        [ACMethodInfo("EditTranslation", "en{'Save'}de{'Speichern'}", 999)]
        public void Save()
        {
            if (!IsEnabledSave()) return;
            // setup previewed object
            string curentTransValue = string.Join("", EditTranslationList.Select(c => c.GetTranslationTuple()));
            SelectedTranslation.TranslationValue = curentTransValue;
            OnPropertyChanged("TranslationList");
            System.Data.EntityKey entityKey = new System.Data.EntityKey((Database as iPlusV4_Entities).DefaultContainerName + "." + SelectedTranslation.TableName, SelectedTranslation.TableName + "ID", SelectedTranslation.ID);
            object item = (Database as iPlusV4_Entities).GetObjectByKey(entityKey);


            if (item is IACObjectEntityWithCheckTrans)
            {
                (item as IACObjectEntityWithCheckTrans).ACCaptionTranslation = curentTransValue;
            }
            if (item is IMDTrans)
            {
                (item as IMDTrans).MDNameTrans = curentTransValue;
            }
            Database.ACSaveChanges();
        }

        public bool IsEnabledSave()
        {
            return SelectedTranslation != null;
        }

        [ACMethodInfo("AddLanguage", "en{'Add language'}de{'Sprache hinzufügen'}", 999)]
        public void AddLanguage()
        {
            TranslationPair translationPair = new TranslationPair();
            translationPair.LangCode = SelectedVBLanguage.VBLanguageCode;
            EditTranslationList.Add(translationPair);
            OnPropertyChanged("EditTranslationList");
            SelectedEditTranslation = translationPair;
        }

        public bool IsEnabledAddLanguage()
        {
            return
                SelectedVBLanguage != null
                && EditTranslationList != null
                && EditTranslationList.Any()
                && !EditTranslationList.Any(x => x.LangCode == SelectedVBLanguage.VBLanguageCode);
        }

        [ACMethodInfo("RemoveLanguage", "en{'Remove language'}de{'Sprache entfernen'}", 999)]
        public void RemoveLanguage()
        {
            TranslationList.Remove(SelectedTranslation);
            SelectedTranslation = TranslationList.FirstOrDefault();
        }

        public bool IsEnabledRemoveLanguage()
        {
            return SelectedEditTranslation != null;
        }

        #endregion

    }
}
