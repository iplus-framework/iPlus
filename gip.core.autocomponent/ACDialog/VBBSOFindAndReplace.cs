using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Collections.ObjectModel;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Search/Replace'}de{'Suchen/Ersetzen'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    [ACClassConstructorInfo(
        new object[]
        { 
            new object[] {"FindAndReplaceHandler", Global.ParamOption.Required, typeof(IVBFindAndReplace)},
            //new ACValueInfo("FindAndReplaceHandler",typeof(IVBFindAndReplace), Global.ParamOption.Required),
        }
    )]
    public class VBBSOFindAndReplace : ACBSO
    {
        public VBBSOFindAndReplace(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            if ((parameter != null) && (parameter.Any()))
                _findAndReplaceHandler = parameter[0].Value as IVBFindAndReplace;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._CurrentEntity = null;
            this._CurrentFindText = null;
            this._CurrentReplaceText = null;
            this._EntityList = null;
            this._findAndReplaceHandler = null;
            this._FindTextInput = null;
            this._FindTextList = null;
            this._MessageText = null;
            this._ReplaceTextInput = null;
            this._ReplaceTextList = null;
            this._SelectedEntity = null;
            return base.ACDeInit(deleteACClassTask);
        }

        #region Properties
        IVBFindAndReplace _findAndReplaceHandler;
        [ACPropertyInfo(9999, "", "en{'FindAndReplaceHandler'}de{'FindAndReplaceHandler'}")]
        public IVBFindAndReplace FindAndReplaceHandler
        {
            get
            {
                return _findAndReplaceHandler;
            }
        }


        #region FindText-Property
        ObservableCollection<ACValueItem> _FindTextList = new ObservableCollection<ACValueItem>();
        [ACPropertyList(9999, "FindText", "en{'Search for'}de{'Suchen nach'}")]
        public ObservableCollection<ACValueItem> FindTextList
        {
            get
            {
                return _FindTextList;
            }
        }

        ACValueItem _CurrentFindText = null;
        [ACPropertySelected(9999, "FindText", "en{'Search for'}de{'Suchen nach'}")]
        public ACValueItem CurrentFindText
        {
            get
            {
                return _CurrentFindText;
            }
            set
            {
                _CurrentFindText = value;
                if (_CurrentFindText != null)
                {
                    if ((_CurrentFindText.Value as String) != FindTextInput)
                        FindTextInput = _CurrentFindText.Value as String;
                }
                OnPropertyChanged("CurrentFindText");
            }
        }

        private string _FindTextInput = "";
        [ACPropertyInfo(9999, "", "en{'Search for'}de{'Suchen nach'}")]
        public string FindTextInput
        {
            get
            {
                return _FindTextInput;
                //if (CurrentFindText != null)
                //return CurrentFindText.Value as String;
                //return " ";
            }
            set
            {
                _FindTextInput = value;
                OnPropertyChanged("FindTextInput");
            }
        }
        #endregion

        #region ReplaceText-Property
        ObservableCollection<ACValueItem> _ReplaceTextList = new ObservableCollection<ACValueItem>();
        [ACPropertyList(9999, "ReplaceText", "en{'Replace with'}de{'Ersetzen durch'}")]
        public ObservableCollection<ACValueItem> ReplaceTextList
        {
            get
            {
                return _ReplaceTextList;
            }
        }

        ACValueItem _CurrentReplaceText = null;
        [ACPropertySelected(9999, "ReplaceText", "en{'Replace with'}de{'Ersetzen durch'}")]
        public ACValueItem CurrentReplaceText
        {
            get
            {
                return _CurrentReplaceText;
            }
            set
            {
                _CurrentReplaceText = value;
                if (_CurrentReplaceText != null)
                {
                    if ((_CurrentReplaceText.Value as String) != ReplaceTextInput)
                        ReplaceTextInput = _CurrentReplaceText.Value as String;
                }
                OnPropertyChanged("CurrentReplaceText");
            }
        }

        private string _ReplaceTextInput = "";
        [ACPropertyInfo(9999, "", "en{'Replace with'}de{'Ersetzen durch'}")]
        public string ReplaceTextInput
        {
            get
            {
                //if (CurrentReplaceText != null)
                //return CurrentReplaceText.Value as String;
                return _ReplaceTextInput;
            }
            set
            {
                _ReplaceTextInput = value;
                OnPropertyChanged("ReplaceTextInput");
            }
        }

        #endregion

        #region Options-Properties
        bool _OptionCaseSensitive = false;
        [ACPropertyInfo(9999, "", "en{'Case senstive'}de{'Groß-/Kleinschreibung beachten'}")]
        public bool OptionCaseSensitive
        {
            get
            {
                if (FindAndReplaceHandler != null)
                    return FindAndReplaceHandler.OptionCaseSensitive;
                return _OptionCaseSensitive;
            }
            set
            {
                if (FindAndReplaceHandler != null)
                    FindAndReplaceHandler.OptionCaseSensitive = value;
                else
                    _OptionCaseSensitive = value;
            }
        }

        bool _OptionWholeWord = false;
        [ACPropertyInfo(9999, "", "en{'Find whole word only'}de{'Nur ganzes Wort suchen'}")]
        public bool OptionWholeWord
        {
            get
            {
                if (FindAndReplaceHandler != null)
                    return FindAndReplaceHandler.OptionFindCompleteWord;
                return _OptionWholeWord;
            }
            set
            {
                if (FindAndReplaceHandler != null)
                    FindAndReplaceHandler.OptionFindCompleteWord = value;
                else
                    _OptionWholeWord = value;
            }
        }

        bool _OptionIsWildcard = false;
        [ACPropertyInfo(9999, "", "en{'Wildcard'}de{'Platzhaltersuche'}")]
        public bool OptionIsWildcard
        {
            get
            {
                if (FindAndReplaceHandler != null)
                    return FindAndReplaceHandler.OptionIsWildcard;
                return _OptionIsWildcard;
            }
            set
            {
                if (FindAndReplaceHandler != null)
                    FindAndReplaceHandler.OptionIsWildcard = value;
                else
                    _OptionIsWildcard = value;
            }
        }

        bool _OptionIsRegularExpr = false;
        [ACPropertyInfo(9999, "", "en{'Regular expressions'}de{'Reguläre Ausdrücke'}")]
        public bool OptionIsRegularExpr
        {
            get
            {
                if (FindAndReplaceHandler != null)
                    return FindAndReplaceHandler.OptionIsRegularExpr;
                return _OptionIsRegularExpr;
            }
            set
            {
                if (FindAndReplaceHandler != null)
                    FindAndReplaceHandler.OptionIsRegularExpr = value;
                else
                    _OptionIsRegularExpr = value;
            }
        }
        #endregion

        #region MessageText
        private string _MessageText;
        [ACPropertyInfo(9999, "", "en{'Message'}de{'Meldung'}")]
        public string MessageText
        {
            get
            {
                return _MessageText;
            }
            set
            {
                _MessageText = value;
                OnPropertyChanged("MessageText");
            }
        }
        #endregion

        IACObjectEntityWithCheckTrans _SelectedEntity = null;
        [ACPropertySelected(9999, "DBSearch")]
        public IACObjectEntityWithCheckTrans SelectedEntity
        {
            get
            {
                return _SelectedEntity;
            }
            set
            {
                _SelectedEntity = value;
                IVBFindAndReplaceDBSearch parentFind = ParentACComponent as IVBFindAndReplaceDBSearch;
                if (parentFind != null)
                {
                    parentFind.FARSearchItemSelected(this, _SelectedEntity);
                }
                OnPropertyChanged("SelectedEntity");
            }
        }

        IEnumerable<IACObjectEntityWithCheckTrans> _EntityList = null;
        [ACPropertyList(9999, "DBSearch")]
        public IEnumerable<IACObjectEntityWithCheckTrans> EntityList
        {
            get
            {
                return _EntityList;
            }
        }


        IACObjectEntityWithCheckTrans _CurrentEntity = null;
        [ACPropertyCurrent(9999, "DBSearch")]
        public IACObjectEntityWithCheckTrans CurrentEntity
        {
            get
            {
                return _CurrentEntity;
            }
            set
            {
                _CurrentEntity = value;
                OnPropertyChanged("CurrentEntity");
            }
        }

        #endregion

        #region Methods
        [ACMethodInfo("", "en{'SetComboFindText'}de{'SetComboFindText'}", 9999, false)]
        public void UpdateFindTextFromSelection()
        {
            if (FindAndReplaceHandler != null)
                FindTextInput = FindAndReplaceHandler.GetTextInSelection();
        }

        private void PopulateFindAndReplaceHandler()
        {
            if (FindAndReplaceHandler == null)
                return;
            FindAndReplaceHandler.wordToFind = FindTextInput;
            FindAndReplaceHandler.wordReplaceWith = ReplaceTextInput;
            if (FindTextInput.Length > 0)
            {
                //bool found = false;
                var query = FindTextList.Where(c => (c.Value != null) && (c.Value as String) == FindTextInput);
                if (!query.Any())
                {
                    ACValueItem newValueItem = new ACValueItem(FindTextInput, Value = FindTextInput, null);
                    FindTextList.Add(newValueItem);
                    OnPropertyChanged("FindTextList");
                    CurrentFindText = newValueItem;
                }
            }

            if (ReplaceTextInput.Length > 0)
            {
                //bool found = false;
                var query = ReplaceTextList.Where(c => (c.Value != null) && (c.Value as String) == ReplaceTextInput);
                if (!query.Any())
                {
                    ACValueItem newValueItem = new ACValueItem(ReplaceTextInput, ReplaceTextInput, null);
                    ReplaceTextList.Add(newValueItem);
                    OnPropertyChanged("ReplaceTextList");
                    CurrentReplaceText = newValueItem;
                }
            }
        }

        [ACMethodInfo("", "en{'Find next'}de{'Weitersuchen'}", 9999, false)]
        public void Search()
        {
            PopulateFindAndReplaceHandler();
            FindAndReplaceResult result = FindAndReplaceHandler.FindNext();
            DisplayResultMessage(result);
        }

        public bool IsEnabledSearch()
        {
            return !String.IsNullOrEmpty(FindTextInput);
        }


        [ACMethodInfo("", "en{'Bookmark'}de{'Lesezeichen'}", 9999, false)]
        public void Bookmark()
        {
            PopulateFindAndReplaceHandler();
        }

        public bool IsEnabledBookmark()
        {
            return !String.IsNullOrEmpty(FindTextInput);
        }


        [ACMethodInfo("", "en{'Replace next'}de{'Ersetzen'}", 9999, false)]
        public void Replace()
        {
            PopulateFindAndReplaceHandler();
            if (FindAndReplaceHandler != null)
            {
                FindAndReplaceResult result = FindAndReplaceHandler.Replace();
                DisplayResultMessage(result);
            }
        }

        public bool IsEnabledReplace()
        {
            return !String.IsNullOrEmpty(FindTextInput) && !String.IsNullOrEmpty(ReplaceTextInput);
        }


        [ACMethodInfo("", "en{'Replace all'}de{'Alle ersetzen'}", 9999, false)]
        public void ReplaceAll()
        {
            PopulateFindAndReplaceHandler();
            if (FindAndReplaceHandler != null)
            {
                FindAndReplaceResult result = FindAndReplaceHandler.ReplaceAll();
                DisplayResultMessage(result);
            }
        }

        public bool IsEnabledReplaceAll()
        {
            return !String.IsNullOrEmpty(FindTextInput) && !String.IsNullOrEmpty(ReplaceTextInput);
        }

        private void DisplayResultMessage(FindAndReplaceResult result)
        {
            switch (result)
            {
                case FindAndReplaceResult.WordNotFound:
                    MessageText = "Der Suchtext wurde nicht gefunden - bzw. Dokumentende erreicht.";
                    break;
                case FindAndReplaceResult.WordFound:
                    MessageText = "";
                    break;
                case FindAndReplaceResult.Replaced_NextWordNotFound:
                    MessageText = "Suchtext wurde ersetzt. Dokumentende erreicht.";
                    break;
                case FindAndReplaceResult.Replaced_NextWordFound:
                    MessageText = "";
                    break;
                case FindAndReplaceResult.SelectionRequired:
                    MessageText = "Bitte zuerst den Suchbereich mit der Maus markieren!";
                    break;
                case FindAndReplaceResult.ReplacedAll:
                    MessageText = "Alle Vorkommen in der Auswahl ersetzt.";
                    break;
                default:
                    MessageText = "";
                    break;
            }
        }


        [ACMethodInfo("Configuration", "en{'Open FindAndReplaceDialog'}de{'Öffne Suchen und Ersetzen'}", 9999)]
        public void ShowFindAndReplaceDialog(IACInteractiveObject acElement)
        {
            IACObject window = FindGui("", "", "*VBBSOFindAndReplace", this.ACIdentifier);

            if (window == null)
            {
                ShowWindow(this, "VBBSOFindAndReplace", true,
                    Global.VBDesignContainer.DockableWindow,
                    Global.VBDesignDockState.Docked,
                    Global.VBDesignDockPosition.Right);
            }
        }

        [ACMethodInfo("", "en{'Find in Database'}de{'Suche in Datenbank'}", 9999, false)]
        public void SearchDB()
        {
            if (!IsEnabledSearchDB())
                return;
            IVBFindAndReplaceDBSearch parentFind = ParentACComponent as IVBFindAndReplaceDBSearch;
            _EntityList = parentFind.FARSearchInDB(this, FindTextInput).ToArray();
            OnPropertyChanged("EntityList");
        }

        public bool IsEnabledSearchDB()
        {
            if (ParentACComponent == null || String.IsNullOrEmpty(FindTextInput))
                return false;
            IVBFindAndReplaceDBSearch parentFind = ParentACComponent as IVBFindAndReplaceDBSearch;
            if (parentFind == null)
                return false;
            return parentFind.IsEnabledFARSearchInDB(this);

        }

        #endregion

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "UpdateFindTextFromSelection":
                    UpdateFindTextFromSelection();
                    return true;
                case "Search":
                    Search();
                    return true;
                case "Bookmark":
                    Bookmark();
                    return true;
                case "Replace":
                    Replace();
                    return true;
                case "ReplaceAll":
                    ReplaceAll();
                    return true;
                case "SearchDB":
                    SearchDB();
                    return true;
                case "ShowFindAndReplaceDialog":
                    ShowFindAndReplaceDialog(acParameter[0] as IACInteractiveObject);
                    return true;
                case Const.IsEnabledPrefix + "Search":
                    result = IsEnabledSearch();
                    return true;
                case Const.IsEnabledPrefix + "Bookmark":
                    result = IsEnabledBookmark();
                    return true;
                case Const.IsEnabledPrefix + "Replace":
                    result = IsEnabledReplace();
                    return true;
                case Const.IsEnabledPrefix + "ReplaceAll":
                    result = IsEnabledReplaceAll();
                    return true;
                case Const.IsEnabledPrefix + "SearchDB":
                    result = IsEnabledSearchDB();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

    }
}
