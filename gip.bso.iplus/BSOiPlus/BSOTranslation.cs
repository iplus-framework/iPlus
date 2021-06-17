using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace gip.bso.iplus
{

    /// <summary>
    /// Handle translations from one place
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Translation'}de{'Translation'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACPackage.ClassName)]
    public class BSOTranslation : ACBSO
    {

        #region c'tors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="acType"></param>
        /// <param name="content"></param>
        /// <param name="parentACObject"></param>
        /// <param name="parameter"></param>
        /// <param name="acIdentifier"></param>
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

        #region Properties

        #region Properties -> Filter

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

        #region Properties -> Autogenerate

        private bool _AutoGenerateByNavigation = true;
        /// <summary>
        /// AUtogenerate for all in the list
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "AutoGenerateByNavigation", "en{'Generate pair by navigation'}de{'Paar durch Navigation generieren'}")]
        public bool AutoGenerateByNavigation
        {
            get
            {
                return _AutoGenerateByNavigation;
            }
            set
            {
                if (_AutoGenerateByNavigation != value)
                {
                    _AutoGenerateByNavigation = value;
                    OnPropertyChanged("AutoGenerateByNavigation");
                }
            }
        }

        private bool _AutoGenerateForAll;
        /// <summary>
        /// AUtogenerate for all in the list
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "AutoGenerateForAll", "en{'Generate for all (not only current list)'}de{'Für alle generieren (nicht nur aktuelle Liste)'}")]
        public bool AutoGenerateForAll
        {
            get
            {
                return _AutoGenerateForAll;
            }
            set
            {
                if (_AutoGenerateForAll != value)
                {
                    _AutoGenerateForAll = value;
                    OnPropertyChanged("AutoGenerateForAll");
                }
            }
        }

        private string _AutoGeneratePrefix = "#";
        /// <summary>
        /// Doc  AutoGeneratePrefix
        /// </summary>
        /// <value>The selected </value>
        [ACPropertyInfo(999, "AutoGeneratePrefix", "en{'Prefix'}de{'Präfix'}")]
        public string AutoGeneratePrefix
        {
            get
            {
                return _AutoGeneratePrefix;
            }
            set
            {
                if (_AutoGeneratePrefix != value)
                {
                    _AutoGeneratePrefix = value;
                    OnPropertyChanged("AutoGeneratePrefix");
                }
            }
        }

        #region  Properties -> Autogenerate -> AutoGenerateOption

        public TranslationAutogenerateOption? SelectedAutoGenerateOptionEnumVal
        {
            get
            {
                if (SelectedAutoGenerateOption == null) return null;
                return (TranslationAutogenerateOption)((short)SelectedAutoGenerateOption.Value);
            }
        }


        /// <summary>
        /// Gibt eine Liste aller Enums zurück, damit die Gui
        /// damit arbeiten kann.
        /// </summary>
        public ACValueItemList GetAutoGenerateOptionList()
        {
            ACValueItemList aCValueItems = new ACValueItemList("AutoGenerateOptionList");
            aCValueItems.AddEntry((short)TranslationAutogenerateOption.GenerateEmptyTranslation, "en{'Generate empty translation'}de{'Leere Übersetzung generieren'}");
            aCValueItems.AddEntry((short)TranslationAutogenerateOption.GeneratePairFromEnglish, "en{'Copy from english'}de{'Kopie aus dem Englischen'}");
            aCValueItems.AddEntry((short)TranslationAutogenerateOption.GeneratePairUsingGoogleApi, "en{'Use google translator'}de{'Benutze den Google Übersetzer'}");
            return aCValueItems;
        }


        private ACValueItemList _AutoGenerateOptionList = null;
        [ACPropertyList(618, "AutoGenerateOption", "en{'Period'}de{'Periode'}")]
        public ACValueItemList AutoGenerateOptionList
        {
            get
            {
                if (_AutoGenerateOptionList == null)
                    _AutoGenerateOptionList = GetAutoGenerateOptionList();
                return _AutoGenerateOptionList;
            }
        }

        private ACValueItem _SelectedAutoGenerateOption;
        /// <summary>
        /// Selected property for VBTranslationView
        /// </summary>
        /// <value>The selected Translations</value>
        [ACPropertySelected(401, "AutoGenerateOption", "en{'Select task'}de{'Aufgabe auswählen'}")]
        public ACValueItem SelectedAutoGenerateOption
        {
            get
            {
                return _SelectedAutoGenerateOption;
            }
            set
            {
                if (_SelectedAutoGenerateOption != value)
                {
                    _SelectedAutoGenerateOption = value;
                    OnPropertyChanged("SelectedAutoGenerateOption");
                }
            }
        }

        #endregion

        #endregion

        #region Properties -> Messages

        /// <summary>
        /// The _ current MSG
        /// </summary>
        Msg _CurrentMsg;
        /// <summary>
        /// Gets or sets the current MSG.
        /// </summary>
        /// <value>The current MSG.</value>
        [ACPropertyCurrent(9999, "Message", "en{'Message'}de{'Meldung'}")]
        public Msg CurrentMsg
        {
            get
            {
                return _CurrentMsg;
            }
            set
            {
                _CurrentMsg = value;
                OnPropertyChanged("CurrentMsg");
            }
        }

        private ObservableCollection<Msg> msgList;
        /// <summary>
        /// Gets the MSG list.
        /// </summary>
        /// <value>The MSG list.</value>
        [ACPropertyList(9999, "Message", "en{'Messagelist'}de{'Meldungsliste'}")]
        public ObservableCollection<Msg> MsgList
        {
            get
            {
                if (msgList == null)
                    msgList = new ObservableCollection<Msg>();
                return msgList;
            }
        }

        #endregion

        #region Properties -> TranslationView

        private VBTranslationView _SelectedTranslationView;
        /// <summary>
        /// Selected property for VBTranslationView
        /// </summary>
        /// <value>The selected Translations</value>
        [ACPropertySelected(401, "TranslationView", "en{'Translation preview'}de{'Übersetzungsvorschau'}")]
        public VBTranslationView SelectedTranslationView
        {
            get
            {
                return _SelectedTranslationView;
            }
            set
            {
                if (_SelectedTranslationView != value)
                {
                    _SelectedTranslationView = value;
                    _TranslationPairList = LoadTranslationPairList();
                    OnPropertyChanged("SelectedTranslationView");
                    OnPropertyChanged("TranslationPairList");

                    if (_TranslationPairList != null)
                        SelectedTranslationPair = _TranslationPairList.FirstOrDefault();
                    else
                        SelectedTranslationPair = null;
                }
            }
        }

        private List<VBTranslationView> _TranslationViewList;
        /// <summary>
        /// List property for VBTranslationView
        /// </summary>
        /// <value>The Translations list</value>
        [ACPropertyList(402, "TranslationView")]
        public List<VBTranslationView> TranslationViewList
        {
            get
            {
                return _TranslationViewList;
            }
        }

        #endregion

        #region Properties -> TranslationView Selected index

        public int SelectedTranslationViewIndex
        {
            get
            {
                if (TranslationViewCount == 0 || SelectedTranslationView == null) return -1;
                return TranslationViewList.IndexOf(SelectedTranslationView);
            }
            set
            {
                if (TranslationViewCount > 0 && value > -1 && value < TranslationViewCount)
                {
                    SelectedTranslationView = TranslationViewList[value];
                }
            }
        }

        public int TranslationViewCount
        {
            get
            {
                if (TranslationViewList == null || !TranslationViewList.Any()) return 0;
                return TranslationViewList.Count();
            }
        }

        #endregion

        #region Properties -> Lang


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

        #region Properties -> TranslationPair
        private TranslationPair _SelectedTranslationPair;
        /// <summary>
        /// Selected property for TranslationPair
        /// </summary>
        /// <value>The selected EditTranslation</value>
        [ACPropertySelected(403, "TranslationPair", "en{'TODO: EditTranslation'}de{'TODO: EditTranslation'}")]
        public TranslationPair SelectedTranslationPair
        {
            get
            {
                return _SelectedTranslationPair;
            }
            set
            {
                if (_SelectedTranslationPair != value)
                {
                    _SelectedTranslationPair = value;
                    OnPropertyChanged("SelectedTranslationPair");
                }
            }
        }


        private List<TranslationPair> _TranslationPairList;
        /// <summary>
        /// List property for TranslationPair
        /// </summary>
        /// <value>The EditTranslation list</value>
        [ACPropertyList(404, "TranslationPair")]
        public List<TranslationPair> TranslationPairList
        {
            get
            {
                return _TranslationPairList;
            }
        }

        private List<TranslationPair> LoadTranslationPairList()
        {
            if (SelectedTranslationView == null) return null;
            if (AutoGenerateByNavigation
                && SelectedTranslationView.EditTranslationList != null
                && SelectedVBLanguage != null
                && SelectedAutoGenerateOption != null
                && !SelectedTranslationView.EditTranslationList.Any(c => c.LangCode == SelectedVBLanguage.VBLanguageCode))
            {
                TranslationPair translationPair = null;
                TranslationPair enPair = SelectedTranslationView.EditTranslationList.FirstOrDefault(c => c.LangCode == "en");
                switch (SelectedAutoGenerateOptionEnumVal.Value)
                {
                    case TranslationAutogenerateOption.GenerateEmptyTranslation:
                        translationPair = new TranslationPair() { LangCode = SelectedVBLanguage.VBLanguageCode, Translation = AutoGeneratePrefix };
                        break;
                    case TranslationAutogenerateOption.GeneratePairFromEnglish:
                        if (enPair != null)
                            translationPair = new TranslationPair() { LangCode = SelectedVBLanguage.VBLanguageCode, Translation = AutoGeneratePrefix + enPair.Translation };
                        break;
                    case TranslationAutogenerateOption.GeneratePairUsingGoogleApi:
                        if (enPair != null)
                            translationPair = new TranslationPair() { LangCode = SelectedVBLanguage.VBLanguageCode, Translation = AutoGeneratePrefix + GetTranslationPairFromGoogleApi(SelectedVBLanguage.VBLanguageCode, enPair.Translation) };
                        break;
                }
                if (translationPair != null)
                    SelectedTranslationView.EditTranslationList.Add(translationPair);
            }
            return SelectedTranslationView.EditTranslationList;
        }


        #endregion

        #region Properties -> TranslationPair SelectedIndex

        public int SelectedTranslationPairIndex
        {
            get
            {
                if (TranslationPairCount == 0 || SelectedTranslationPair == null) return -1;
                return TranslationPairList.IndexOf(SelectedTranslationPair);
            }
            set
            {
                if (TranslationPairCount > 0 && value > -1 && value < TranslationPairCount)
                {
                    SelectedTranslationPair = TranslationPairList[value];
                }
            }
        }

        public int TranslationPairCount
        {
            get
            {
                if (TranslationPairList == null || !TranslationPairList.Any()) return 0;
                return TranslationPairList.Count();
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Methods -> Common

        [ACMethodCommand("Translation", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void Save()
        {
            BackgroundWorker.RunWorkerAsync(TranslationAutogenerateOption.SaveTranslation);
            ShowDialog(this, DesignNameProgressBar);
        }

        [ACMethodInfo("Translation", "en{'Search'}de{'Suche'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void Search()
        {
            if (!IsEnabledSearch())
                return;
            BackgroundWorker.RunWorkerAsync(TranslationAutogenerateOption.FetchTranslation);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledSearch() => !string.IsNullOrEmpty(FilterClassACIdentifier) || !string.IsNullOrEmpty(FilterACIdentifier) || !string.IsNullOrEmpty(FilterTranslation);

        #endregion

        #region Methods -> Navigate

        #region Methods -> Navigate -> Navigate TranlationViewList

        [ACMethodCommand("Translation", "en{'Backward (Ctrl + <-)'}de{'Rückwärts (Ctrl + <-)'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void MoveBackward()
        {
            if (!IsEnabledMoveBackward())
                return;
            SelectedTranslationViewIndex--;
        }

        public bool IsEnabledMoveBackward() => TranslationViewCount > 0 && SelectedTranslationViewIndex > 0;


        [ACMethodCommand("Translation", "en{'Forward (Ctrl + ->)'}de{'Nach vorne (Ctrl + ->)'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void MoveForward()
        {
            if (!IsEnabledMoveForward())
                return;
            SelectedTranslationViewIndex++;
        }

        public bool IsEnabledMoveForward() => TranslationViewCount > 0 && (SelectedTranslationViewIndex + 1) < TranslationViewCount;




        #endregion

        #region Methods -> Navigate -> Navigate TranslationPairList

        [ACMethodCommand("TranslationPair", "en{'Up (Ctrl + Up)'}de{'Oben (Ctrl + Oben)'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void MoveUp()
        {
            if (!IsEnabledMoveUp())
                return;
            SelectedTranslationPairIndex--;
        }

        public bool IsEnabledMoveUp() => TranslationPairCount > 0 && SelectedTranslationPairIndex > 0;


        [ACMethodCommand("TranslationPair", "en{'Down (Ctrl + Down)'}de{'Unten (Ctrl + Unten)'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void MoveDown()
        {
            if (!IsEnabledMoveDown())
                return;
            SelectedTranslationPairIndex++;
        }

        public bool IsEnabledMoveDown() => TranslationPairCount > 0 && (SelectedTranslationPairIndex + 1) < TranslationPairCount;


        #endregion

        [ACMethodInfo("", "en{'Key event'}de{'Tastatur Ereignis'}", 9999, false)]
        public void OnKeyEvent(KeyEventArgs e)
        {
            bool ctrlkeyPresent = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            Console.WriteLine(e.Key);
            if (ctrlkeyPresent && e.Key == Key.Left)
                MoveBackward();
            if (ctrlkeyPresent && e.Key == Key.Right)
                MoveForward();
            if (ctrlkeyPresent && e.Key == Key.Up)
                MoveUp();
            if (ctrlkeyPresent && e.Key == Key.Down)
                MoveDown();

        }


        #endregion

        #region Methods -> GenerateTranslation

        [ACMethodInfo("GenerateTranslation", "en{'Execute'}de{'Ausführen'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void GenerateTranslation()
        {
            if (!IsEnabledGenerateTranslation())
                return;
            TranslationAutogenerateOption selectedAutoGenerateOption = SelectedAutoGenerateOptionEnumVal.Value;

            if (AutoGenerateForAll)
            {
                switch (selectedAutoGenerateOption)
                {
                    case TranslationAutogenerateOption.GenerateEmptyTranslation:
                        selectedAutoGenerateOption = TranslationAutogenerateOption.GenerateEmptyTranslationAll;
                        break;
                    case TranslationAutogenerateOption.GeneratePairFromEnglish:
                        selectedAutoGenerateOption = TranslationAutogenerateOption.GeneratePairFromEnglishAll;
                        break;
                    case TranslationAutogenerateOption.GeneratePairUsingGoogleApi:
                        selectedAutoGenerateOption = TranslationAutogenerateOption.GeneratePairUsingGoogleApiAll;
                        break;
                    case TranslationAutogenerateOption.RemoveAutoGenerated:
                        selectedAutoGenerateOption = TranslationAutogenerateOption.RemoveAutoGeneratedAll;
                        break;
                    default:
                        break;
                }
            }

            BackgroundWorker.RunWorkerAsync(selectedAutoGenerateOption);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledGenerateTranslation()
        {
            return
                 IsEnabledAutogeneratedListReady()
                 && IsEnabledAutogeneratedListReady()
                 && SelectedAutoGenerateOption != null;
        }

        [ACMethodInfo("GenerateTranslation", "en{'Remove generated translation (# prefix)'}de{'Generierte Übersetzung entfernen (# prefix)'}", (short)MISort.Search, false, false, true, Global.ACKinds.MSMethodPrePost)]
        public void RemoveGeneratedTranslation()
        {
            if (!IsEnabledRemoveGeneratedTranslation())
                return;
            TranslationAutogenerateOption removeOption = TranslationAutogenerateOption.RemoveAutoGenerated;

            if (AutoGenerateForAll)
                removeOption = TranslationAutogenerateOption.RemoveAutoGeneratedAll;

            BackgroundWorker.RunWorkerAsync(removeOption);
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledRemoveGeneratedTranslation()
        {
            return IsEnabledAutogeneratedListReady() && IsEnabledAutogeneratedListReady();
        }

        #region Methods -> GenerateTranslation -> Enable common

        public bool IsEnabledAutogeneratedListReady()
        {
            return
                AutoGenerateForAll
                || (
                    TranslationViewList != null
                    && TranslationViewList.Any()
                );
        }

        public bool IsEnabledAutogeneratedLanguageReady()
        {
            return SelectedVBLanguage != null && SelectedVBLanguage.VBLanguageCode != "en";
        }

        #endregion

        #endregion

        #region Methods -> Messages


        public void SendMessage(Msg msg)
        {
            MsgList.Add(msg);
            OnPropertyChanged("MsgList");
        }

        #endregion

        #region Methods -> TranslationPair

        [ACMethodInfo("TranslationPair", "en{'Add'}de{'Neu'}", 999)]
        public void AddTranslationPair()
        {
            TranslationPair translationPair = new TranslationPair();
            translationPair.LangCode = SelectedVBLanguage.VBLanguageCode;
            TranslationPairList.Add(translationPair);
            OnPropertyChanged("TranslationPairList");
            SelectedTranslationPair = translationPair;
        }

        public bool IsEnabledAddTranslationPair()
        {
            return
                SelectedVBLanguage != null
                && SelectedTranslationView != null
                && !TranslationPairList.Any(x => x.LangCode == SelectedVBLanguage.VBLanguageCode);
        }

        [ACMethodInfo("TranslationPair", "en{'Delete'}de{'Löschen'}", 999)]
        public void RemoveTranslationPair()
        {
            TranslationPairList.Remove(SelectedTranslationPair);
            SelectedTranslationPair = TranslationPairList.FirstOrDefault();
        }

        public bool IsEnabledRemoveTranslationPair()
        {
            return SelectedTranslationPair != null;
        }

        #endregion

        #region Methods ->  BackgroundWorker

        #region Methods ->  BackgroundWorker -> Methods
        /// <summary>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        public override void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            base.BgWorkerDoWork(sender, e);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            worker.ProgressInfo.OnlyTotalProgress = true;
            worker.ProgressInfo.TotalProgress.StartTime = DateTime.Now;
            TranslationAutogenerateOption command = (TranslationAutogenerateOption)e.Argument;
            switch (command)
            {
                case TranslationAutogenerateOption.FetchTranslation:
                    e.Result = DoFetchTranslation(worker, e, 0, 100);
                    break;
                case TranslationAutogenerateOption.SaveTranslation:
                    e.Result = DoSaveTranslation(worker, e, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.GenerateEmptyTranslation:
                    e.Result = DoGenerateEmptyTranslation(worker, e, SelectedVBLanguage.VBLanguageCode, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.GeneratePairFromEnglish:
                    e.Result = DoGeneratePairFromEnglish(worker, e, SelectedVBLanguage.VBLanguageCode, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.GeneratePairUsingGoogleApi:
                    e.Result = DoGeneratePairUsingGoogleApi(worker, e, SelectedVBLanguage.VBLanguageCode, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.RemoveAutoGenerated:
                    e.Result = DoRemoveAutoGenerated(worker, e, SelectedVBLanguage.VBLanguageCode, TranslationViewList, 0, 100);
                    break;
                case TranslationAutogenerateOption.GenerateEmptyTranslationAll:
                    e.Result = DoGenerateEmptyTranslationAll(worker, e, SelectedVBLanguage.VBLanguageCode);
                    break;
                case TranslationAutogenerateOption.GeneratePairFromEnglishAll:
                    e.Result = DoGeneratePairFromEnglishAll(worker, e, SelectedVBLanguage.VBLanguageCode);
                    break;
                case TranslationAutogenerateOption.GeneratePairUsingGoogleApiAll:
                    e.Result = DoGeneratePairUsingGoogleApiAll(worker, e, SelectedVBLanguage.VBLanguageCode);
                    break;
                case TranslationAutogenerateOption.RemoveAutoGeneratedAll:
                    e.Result = DoRemoveAutoGeneratedAll(worker, e, SelectedVBLanguage.VBLanguageCode);
                    break;
            }
        }

        public override void BgWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            base.BgWorkerCompleted(sender, e);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            worker.ProgressInfo.TotalProgress.EndTime = DateTime.Now;
            CloseWindow(this, DesignNameProgressBar);
            string command = worker.EventArgs.Argument.ToString();
            if (e.Cancelled)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Info, Message = string.Format(@"Operation {0} canceled by user!", command) });
            }
            if (e.Error != null)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format(@"Error by doing {0}! Message:{1}", command, e.Error.Message) });
            }
            else
            {
                List<VBTranslationView> list = e.Result as List<VBTranslationView>;
                _TranslationViewList = list;
                OnPropertyChanged("TranslationViewList");
                if (_TranslationViewList != null)
                    SelectedTranslationView = _TranslationViewList.FirstOrDefault();
                else
                    SelectedTranslationView = null;
            }
        }
        #endregion

        #region BackgroundWorker -> DoWork -> Select & Save
        private List<VBTranslationView> DoFetchTranslation(ACBackgroundWorker worker, DoWorkEventArgs e, int rangeFrom, int rangeTo)
        {
            int halfRange = (rangeFrom + rangeTo) / 2;
            worker.ProgressInfo.TotalProgress.ProgressText = "Start fetching data... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;
            List<VBTranslationView> list = (Database as iPlusV4_Entities)
                .udpTranslation(FilterOnlyACClassTables,
                FilterOnlyMDTables,
                FilterClassACIdentifier,
                FilterACIdentifier,
                FilterTranslation)
                .ToList();
            worker.ProgressInfo.TotalProgress.ProgressCurrent = halfRange;
            int itemsCount = list.Count();
            worker.ProgressInfo.TotalProgress.ProgressText = string.Format("Fetched {0} translations! prepare translation pairs...", itemsCount);
            if (list != null && list.Any())
                foreach (var translationItem in list)
                {
                    translationItem.SetTranslationList(VBLanguageList);

                    int itemIndex = list.IndexOf(translationItem);
                    int addToProgress = (itemIndex / itemsCount) * halfRange;
                    worker.ProgressInfo.TotalProgress.ProgressCurrent = halfRange + addToProgress;
                }
            worker.ProgressInfo.TotalProgress.ProgressText = "Fetching translation finished!";
            return list;
        }

        private List<VBTranslationView> DoSaveTranslation(ACBackgroundWorker worker, DoWorkEventArgs e, List<VBTranslationView> list, int rangeFrom, int rangeTo)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start saving translation data... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;
            int progressDiff = rangeTo - rangeFrom;
            int itemsCount = 0;
            if (list != null)
                itemsCount = list.Count();

            foreach (var translationItem in list)
            {
                string curentTransValue = string.Join("", translationItem.EditTranslationList.Select(c => c.GetTranslationTuple()));
                translationItem.TranslationValue = curentTransValue;
                System.Data.EntityKey entityKey = new System.Data.EntityKey((Database as iPlusV4_Entities).DefaultContainerName + "." + translationItem.TableName, translationItem.TableName + "ID", translationItem.ID);
                object item = (Database as iPlusV4_Entities).GetObjectByKey(entityKey);
                if (item is IACObjectEntityWithCheckTrans)
                {
                    (item as IACObjectEntityWithCheckTrans).ACCaptionTranslation = curentTransValue;
                }
                if (item is IMDTrans)
                {
                    (item as IMDTrans).MDNameTrans = curentTransValue;
                }

                int itemIndex = list.IndexOf(translationItem);
                int progressValue = (itemIndex / itemsCount) * progressDiff;
                worker.ProgressInfo.TotalProgress.ProgressCurrent = progressValue;
            }
            Database.ACSaveChanges();
            return list;
        }

        #endregion

        #region BackgroundWorker -> DoWork -> Generate List

        private List<VBTranslationView> DoGenerateEmptyTranslation(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode, List<VBTranslationView> list, int rangeFrom, int rangeTo)
        {

            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate empty translation... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;

            int half = (rangeFrom - rangeTo) / 2;
            int itemsCount = list.Count();
            foreach (var item in list)
            {
                if (!item.EditTranslationList.Any(x => x.LangCode == targetLanguageCode))
                {
                    TranslationPair translationPair = new TranslationPair() { LangCode = targetLanguageCode, Translation = AutoGeneratePrefix };
                    item.EditTranslationList.Add(translationPair);
                }

                int itemIndex = list.IndexOf(item);
                int progressValue = (itemIndex / itemsCount) * half;
                worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom + progressValue;
            }
            list = DoSaveTranslation(worker, e, list, rangeFrom + half, rangeTo);
            return list;
        }

        private List<VBTranslationView> DoGeneratePairFromEnglish(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode, List<VBTranslationView> list, int rangeFrom, int rangeTo)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate translation from english... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;

            int half = (rangeFrom - rangeTo) / 2;
            int itemsCount = list.Count();
            foreach (var item in list)
            {
                if (!item.EditTranslationList.Any(x => x.LangCode == targetLanguageCode))
                {
                    TranslationPair engPair = item.EditTranslationList.FirstOrDefault(c => c.LangCode == "en");
                    if (engPair != null)
                    {
                        TranslationPair translationPair = new TranslationPair() { LangCode = targetLanguageCode, Translation = AutoGeneratePrefix + engPair.Translation };
                        item.EditTranslationList.Add(translationPair);
                    }
                }


                int itemIndex = list.IndexOf(item);
                int progressValue = (itemIndex / itemsCount) * half;
                worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom + progressValue;
            }
            list = DoSaveTranslation(worker, e, list, rangeFrom + half, rangeTo);
            return list;
        }

        private List<VBTranslationView> DoGeneratePairUsingGoogleApi(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode, List<VBTranslationView> list, int rangeFrom, int rangeTo)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate translation from Google API... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;

            int half = (rangeFrom - rangeTo) / 2;
            int itemsCount = list.Count();
            foreach (var item in list)
            {
                if (!item.EditTranslationList.Any(x => x.LangCode == targetLanguageCode))
                {
                    TranslationPair engPair = item.EditTranslationList.FirstOrDefault(c => c.LangCode == "en");
                    if (engPair != null)
                    {
                        TranslationPair translationPair = GetTranslationPairFromGoogleApi(targetLanguageCode, engPair.Translation);
                        item.EditTranslationList.Add(translationPair);
                    }
                }

                int itemIndex = list.IndexOf(item);
                int progressValue = (itemIndex / itemsCount) * half;
                worker.ProgressInfo.TotalProgress.ProgressCurrent = rangeFrom + progressValue;
            }
            list = DoSaveTranslation(worker, e, list, rangeFrom + half, rangeTo);
            return list;
        }

        private List<VBTranslationView> DoRemoveAutoGenerated(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode, List<VBTranslationView> list, int rangeFrom, int rangeTo)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start remove generated translations... ";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = rangeFrom;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = rangeTo;


            int half = (rangeFrom - rangeTo) / 2;
            foreach (var item in list)
            {
                TranslationPair pair = item.EditTranslationList.FirstOrDefault(c => c.LangCode == targetLanguageCode);
                if (pair != null)
                    item.EditTranslationList.Remove(pair);
            }
            list = DoSaveTranslation(worker, e, list, rangeFrom + half, rangeTo);
            return list;
        }

        #endregion

        #region BackgroundWorker -> DoWork -> Generate All



        private List<VBTranslationView> DoGenerateEmptyTranslationAll(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate emtpy translations for all items! Fetch all items...";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = 100;

            List<VBTranslationView> allTranslations = GetAllTranslations();
            worker.ProgressInfo.TotalProgress.ProgressText = "All items fetched";
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 20;

            DoGenerateEmptyTranslation(worker, e, targetLanguageCode, allTranslations, 20, 80);
            return DoFetchTranslation(worker, e, 80, 100); // Return only preselected by filter
        }
        private List<VBTranslationView> DoGeneratePairFromEnglishAll(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate english copy of translations for all items! Fetch all items...";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = 100;

            List<VBTranslationView> allTranslations = GetAllTranslations();
            worker.ProgressInfo.TotalProgress.ProgressText = "All items fetched";
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 20;

            DoGeneratePairFromEnglish(worker, e, targetLanguageCode, allTranslations, 20, 80);
            return DoFetchTranslation(worker, e, 80, 100); // Return only preselected by filter
        }

        private List<VBTranslationView> DoGeneratePairUsingGoogleApiAll(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Start generate Google API translations for all items! Fetch all items ...";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = 100;

            List<VBTranslationView> allTranslations = GetAllTranslations();
            worker.ProgressInfo.TotalProgress.ProgressText = "All items fetched";
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 20;

            DoGeneratePairUsingGoogleApi(worker, e, targetLanguageCode, allTranslations, 20, 80);
            return DoFetchTranslation(worker, e, 80, 100); // Return only preselected by filter
        }

        private List<VBTranslationView> DoRemoveAutoGeneratedAll(ACBackgroundWorker worker, DoWorkEventArgs e, string targetLanguageCode)
        {
            worker.ProgressInfo.TotalProgress.ProgressText = "Remove autogenerated translations for all items! Fetch all items...";
            worker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            worker.ProgressInfo.TotalProgress.ProgressRangeTo = 100;

            List<VBTranslationView> allTranslations = GetAllTranslations();
            worker.ProgressInfo.TotalProgress.ProgressText = "All items fetched";
            worker.ProgressInfo.TotalProgress.ProgressCurrent = 20;

            DoRemoveAutoGenerated(worker, e, targetLanguageCode, allTranslations, 20, 80);
            return DoFetchTranslation(worker, e, 80, 100); // Return only preselected by filter
        }

        #endregion

        #region BackgroundWorker -> DoWork -> Common

        /// <summary>
        /// Fetch translation from google api
        /// </summary>
        /// <param name="targetLanguageCode"></param>
        /// <param name="translation"></param>
        /// <returns></returns>
        private TranslationPair GetTranslationPairFromGoogleApi(string targetLanguageCode, string translation)
        {
            string fakeTranslatedValue = AutoGeneratePrefix + "-" + targetLanguageCode + "-" + translation;
            return new TranslationPair() { LangCode = targetLanguageCode, Translation = fakeTranslatedValue };
        }

        private List<VBTranslationView> GetAllTranslations()
        {
            List<VBTranslationView> list = (Database as iPlusV4_Entities)
              .udpTranslation(FilterOnlyACClassTables,
              FilterOnlyMDTables,
              null,
              null,
              null)
              .ToList();

            if (list != null && list.Any())
                foreach (var translationItem in list)
                    translationItem.SetTranslationList(VBLanguageList);

            return list;
        }

        #endregion

        #endregion

        #endregion

        public enum TranslationAutogenerateOption
        {
            FetchTranslation,
            SaveTranslation,

            GenerateEmptyTranslation,
            GeneratePairFromEnglish,
            GeneratePairUsingGoogleApi,

            GenerateEmptyTranslationAll,
            GeneratePairFromEnglishAll,
            GeneratePairUsingGoogleApiAll,

            RemoveAutoGenerated,
            RemoveAutoGeneratedAll
        }
    }
}
