// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Help'}de{'Hilfe'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    [ACClassConstructorInfo(
        new object[]
        {
            new object[] { "ACComponentURL", Global.ParamOption.Required, typeof(string)},
            new object[] { "ACElementURLs", Global.ParamOption.Required, typeof(List<string>)},
        }
    )]
    public class VBBSOHelp : ACBSO
    {
        #region private fields
        private bool loginSuccess;
        #endregion


        #region Helper list

        [ACPropertyList(9999, "LanguageList")]
        public ACValueItemList LanguageList { get; set; }

        [ACPropertyList(9999, "GenderList")]
        public ACValueItemList GenderList { get; set; }

        [ACPropertyList(9999, "AddressTypeList")]
        public ACValueItemList AddressTypeList { get; set; }

        [ACPropertyList(9999, "PhoneTypeList")]
        public ACValueItemList PhoneTypeList { get; set; }

        private ACValueItemList SelectedItemTypeList { get; set; }

        #endregion

        #region c´tors
        public VBBSOHelp(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {


        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            // Temp load design
            SettingsDesignName = "Register";
            base.ACInit(startChildMode);
            LoadPrivateHelperList();
            _HelpManager = ACHelpManager.ACRefToServiceInstance(this);
            if (_HelpManager == null)
                throw new Exception("HelpManager not configured");
            _WebHelpLoginDataSaved = new ACPropertyConfigValue<Dictionary<string, Login>>(this, "WebHelpLoginDataSaved", new Dictionary<string, Login>());
            HelpLogin = LoginDataRetrive();
            if(HelpLogin.RememberLogin)
            {
                LoginOk();
            }
            if (Parameters != null && Parameters.Any())
            {
                string acComponentURL = ParameterValue("ACComponentURL").ToString();
                CurrentComponentRoot = FactoryACObjectItem(this.ACUrlCommand(acComponentURL) as IACObject);
                CurrentComponentTypeRoot = FactoryACObjectItemType(CurrentComponentRoot.ACObject.ACType as ACClass);

                List<string> acElementURLs = ParameterValue("ACElementURLs") as List<string>;
                // acElementURLs = acElementURLs.Select(acUrl => acUrl.Replace("Database", "")).ToList();
                _ElementsList = new List<ACObjectItem>();
                foreach (string acElementURL in acElementURLs)
                {
                    object item = ACUrlCommand(acElementURL);
                    if (item == null)
                        item = CurrentComponentRoot.ACObject.ACUrlCommand(acElementURL);
                    if (item != null)
                    {
                        IACObject acObjectForElementItem = item as IACObject;
                        if (acObjectForElementItem != null)
                        {
                            ACObjectItem tmpObjectItem = FactoryACObjectItem(acObjectForElementItem);
                            _ElementsList.Add(tmpObjectItem);
                        }
                    }
                }
                OnPropertyChanged("ElementsList");
            }
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            ACHelpManager.DetachACRefFromServiceInstance(this, _HelpManager);
            _HelpManager = null;
            LoginDataSave();
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Managers

        protected ACRef<ACHelpManager> _HelpManager = null;
        public ACHelpManager HelpManager
        {
            get
            {
                if (_HelpManager == null)
                    return null;
                return _HelpManager.ValueT;
            }
        }

        #endregion

        #region SelectedHelpItem

        public string _SelectedHelpItemType;
        [ACPropertyInfo(9999, "SelectedHelpItemType", "en{'Selected item type'}de{'Ausgewähltes Objekttyp'}")]
        public string SelectedHelpItemType
        {
            get
            {
                return _SelectedHelpItemType;
            }
            set
            {
                if (_SelectedHelpItemType != value)
                {
                    _SelectedHelpItemType = value;
                    OnPropertyChanged("SelectedHelpItemType");
                }
            }
        }

        private ACObjectItem _SelectedHelpItem;
        [ACPropertyInfo(9999, "SelectedHelpItem", "en{'Selected item'}de{'Ausgewähltes Objekt'}")]
        public ACObjectItem SelectedHelpItem
        {
            get
            {
                return _SelectedHelpItem;
            }
            set
            {
                if (_SelectedHelpItem != value)
                {
                    _SelectedHelpItem = value;
                    OnPropertyChanged("SelectedHelpItem");
                }
            }
        }



        #region SelectedHelpItem -> Properties

        #region SelectedHelpItem -> Properties ->  Component

        private ACObjectItem _CurrentComponentRoot;
        private ACObjectItem _CurrentComponent;

        [ACPropertyCurrent(9999, "ComponentRoot")]
        public ACObjectItem CurrentComponentRoot
        {
            get
            {
                return _CurrentComponentRoot;
            }
            set
            {
                if (_CurrentComponentRoot != value)
                {
                    _CurrentComponentRoot = value;
                    OnPropertyChanged("CurrentComponentRoot");
                }
            }

        }

        [ACPropertyCurrent(9999, "Component", "en{'Selected component'}de{'Ausgewählte Komponente'}")]
        public ACObjectItem CurrentComponent
        {
            get
            {
                return _CurrentComponent;
            }
            set
            {
                if (_CurrentComponent != value)
                {
                    _CurrentComponent = value;
                    OnPropertyChanged("CurrentComponent");
                }
                SetSelectedElement(value, HelpItemTypeEnum.Component);
            }
        }

        #endregion

        #region SelectedHelpItem -> Properties ->  ComponentType

        private ACObjectItem _CurrentComponentTypeRoot;
        private ACObjectItem _CurrentComponentType;

        [ACPropertyCurrent(9999, "ComponentTypeRoot")]
        public ACObjectItem CurrentComponentTypeRoot
        {
            get
            {
                return _CurrentComponentTypeRoot;
            }
            set
            {
                if (_CurrentComponentTypeRoot != value)
                {
                    _CurrentComponentTypeRoot = value;
                    OnPropertyChanged("CurrentComponentTypeRoot");
                }
            }

        }

        [ACPropertyCurrent(9999, "ComponentType", "en{'Selected component type'}de{'Ausgewählter Komponententyp'}")]
        public ACObjectItem CurrentComponentType
        {
            get
            {
                return _CurrentComponentType;
            }
            set
            {
                if (_CurrentComponentType != value)
                {
                    _CurrentComponentType = value;
                    OnPropertyChanged("CurrentComponentType");
                }
                SetSelectedElement(value, HelpItemTypeEnum.ComponentType);
            }
        }

        #endregion

        #region SelectedHelpItem -> Properites -> Elements
        private ACObjectItem _SelectedElements;
        /// <summary>
        /// Selected property for ACObjectItem
        /// </summary>
        /// <value>The selected Elements</value>
        [ACPropertySelected(9999, "PropertyGroupName", "en{'TODO: Elements'}de{'TODO: Elements'}")]
        public ACObjectItem SelectedElements
        {
            get
            {
                return _SelectedElements;
            }
            set
            {
                if (_SelectedElements != value)
                {
                    _SelectedElements = value;
                    CurrentElementRoot = value;
                    OnPropertyChanged("SelectedElements");
                }
            }
        }

        private List<ACObjectItem> _ElementsList;
        /// <summary>
        /// List property for ACObjectItem
        /// </summary>
        /// <value>The Elements list</value>
        [ACPropertyList(9999, "PropertyGroupName")]
        public List<ACObjectItem> ElementsList
        {
            get
            {
                return _ElementsList;
            }
            set
            {
                _ElementsList = value;
                OnPropertyChanged("ElementsList");
            }
        }

        #endregion

        #region SelectedHelpItem -> Properties ->  Element

        private ACObjectItem _CurrentElementRoot;
        private ACObjectItem _CurrentElement;

        [ACPropertyCurrent(9999, "ElementRoot")]
        public ACObjectItem CurrentElementRoot
        {
            get
            {
                return _CurrentElementRoot;
            }
            set
            {
                if (_CurrentElementRoot != value)
                {
                    _CurrentElementRoot = value;
                    CurrentElementTypeRoot = FactoryACObjectItemType(value.ACObject.ACType as ACClass);
                    OnPropertyChanged("CurrentElementRoot");
                }
            }

        }

        [ACPropertyCurrent(9999, "Element", "en{'Selected element'}de{'Ausgewähltes Element'}")]
        public ACObjectItem CurrentElement
        {
            get
            {
                return _CurrentElement;
            }
            set
            {
                if (_CurrentElement != value)
                {
                    _CurrentElement = value;
                    OnPropertyChanged("CurrentElement");
                }
                SetSelectedElement(value, HelpItemTypeEnum.Element);
            }
        }

        #endregion

        #region SelectedHelpItem -> Properties ->  ElementType

        private ACObjectItem _CurrentElementTypeRoot;
        private ACObjectItem _CurrentElementType;

        [ACPropertyCurrent(9999, "ElementTypeRoot")]
        public ACObjectItem CurrentElementTypeRoot
        {
            get
            {
                return _CurrentElementTypeRoot;
            }
            set
            {
                if (_CurrentElementTypeRoot != value)
                {
                    _CurrentElementTypeRoot = value;
                    OnPropertyChanged("CurrentElementTypeRoot");
                }
            }

        }

        [ACPropertyCurrent(9999, "ElementType", "en{'Selected element type'}de{'Ausgewählter Elementtyp'}")]
        public ACObjectItem CurrentElementType
        {
            get
            {
                return _CurrentElementType;
            }
            set
            {
                if (_CurrentElementType != value)
                {
                    _CurrentElementType = value;
                    OnPropertyChanged("CurrentElementType");
                }
                SetSelectedElement(value, HelpItemTypeEnum.ElementType);
            }
        }

        #endregion

        #region SelectHelpItem -> Methods

        [ACMethodInfo("LoadHelp", "en{'Load help'}de{'Hilfe laden'}", 999)]
        public void LoadHelp()
        {
            // TODO: implement help logic
        }

        public bool IsEnabledLoadHelp()
        {
            return loginSuccess && SelectedHelpItem != null && !string.IsNullOrEmpty(SelectedHelpItem.ACUrl);
        }

        #endregion

        #endregion

        #region SelectedHelpItem -> Methods

        public void SetSelectedElement(ACObjectItem item, HelpItemTypeEnum elementType)
        {
            SelectedHelpItem = item;
            SelectedHelpItemType = SelectedItemTypeList.GetEntryByIndex((short)elementType).ACCaption;
        }

        #endregion 

        #endregion

        #region Help

        #region Help -> Document

        private string _DocumentPreviewURL;
        /// <summary>
        /// Paht for preview in browser
        /// </summary>
        [ACPropertyInfo(9999, "DocumentPreviewURL", "en{'DocumentPreviewURL'}de{'DocumentPreviewURL'}")]
        public string DocumentPreviewURL
        {
            get
            {
                return _DocumentPreviewURL;
            }
            set
            {
                if (_DocumentPreviewURL != value)
                {
                    _DocumentPreviewURL = value;
                    OnPropertyChanged("DocumentPreviewURL");
                }
            }
        }

        #region Help -> Document -> Selection

        private ACObjectItem _SelectedDocument;
        /// <summary>
        /// Selected property for ACObjectItem
        /// </summary>
        /// <value>The selected Document</value>
        [ACPropertySelected(9999, "Document", "en{'TODO: Document'}de{'TODO: Document'}")]
        public ACObjectItem SelectedDocument
        {
            get
            {
                return _SelectedDocument;
            }
            set
            {
                if (_SelectedDocument != value)
                {
                    _SelectedDocument = value;
                    OnPropertyChanged("SelectedDocument");
                }
            }
        }


        //private List<ACObjectItem> _DocumentList;
        ///// <summary>
        ///// List property for ACObjectItem
        ///// </summary>
        ///// <value>The Document list</value>
        //[ACPropertyList(9999, "Document")]
        //public List<ACObjectItem> DocumentList
        //{
        //    get
        //    {
        //        return _DocumentList;
        //    }
        //}

        #endregion

        #region Help -> Document -> Methods

        [ACMethodInfo("OpenDocument", "en{'Open document'}de{'Dokument öffnen'}", 999)]
        public void OpenDocument()
        {

        }

        public bool IsEnabledOpenDocument()
        {
            return SelectedDocument != null && !string.IsNullOrEmpty(SelectedDocument.ACUrl);
        }

        [ACMethodInfo("OpenDocumentInBrowser", "en{'Open document in browser'}de{'Dokument im Browser öffnen'}", 999)]

        public void OpenDocumentInBrowser()
        { }

        public bool IsEnabledOpenDocumentInBrowser()
        {
            return IsEnabledOpenDocument();
        }
        #endregion

        #endregion

        #region Help -> File

        #region Help -> File -> Selection

        private ACObjectItem _SelectedFile;
        /// <summary>
        /// Selected property for ACObjectItem
        /// </summary>
        /// <value>The selected File</value>
        [ACPropertySelected(9999, "File", "en{'TODO: File'}de{'TODO: File'}")]
        public ACObjectItem SelectedFile
        {
            get
            {
                return _SelectedFile;
            }
            set
            {
                if (_SelectedFile != value)
                {
                    _SelectedFile = value;
                    OnPropertyChanged("SelectedFile");
                }
            }
        }

        //private List<ACObjectItem> _FileList;
        ///// <summary>
        ///// List property for ACObjectItem
        ///// </summary>
        ///// <value>The File list</value>
        //[ACPropertyList(9999, "File")]
        //public List<ACObjectItem> FileList
        //{
        //    get
        //    {
        //        return _FileList;
        //    }
        //}

        #endregion

        #region Help -> File -> Mehtods

        [ACMethodInfo("DownloadFile", "en{'Download file'}de{'Datei hinterladen'}", 999)]
        public void DownloadFile()
        {

        }

        public bool IsEnabledDownloadFile()
        {
            return SelectedFile != null;
        }

        [ACMethodInfo("UploadFile", "en{'Upload file'}de{'Datei hochladen'}", 999)]
        public void UploadFile()
        {

        }

        public bool IsEnabledUploadFile()
        {
            return IsEnabledLoadHelp();
        }

        [ACMethodInfo("RemoveFile", "en{'Remove file'}de{'Datei löschen'}", 999)]
        public void RemoveFile()
        {

        }

        public bool IsEnabledRemoveFile()
        {
            return SelectedFile != null;
        }

        [ACMethodInfo("PreviewFile", "en{'Preview file'}de{'Datei anzeigen'}", 999)]
        public void PreviewFile()
        { }

        public bool IsEnabledPreviewFile()
        {
            return SelectedFile != null;
        }

        #endregion

        #endregion
        
        #endregion

        #region Settings

        #region Settings -> SettingsDesign
        [ACPropertyInfo(9999, "SettingsDesign")]
        public string SettingsDesign
        {
            get
            {
                gip.core.datamodel.ACClassDesign acClassDesign = ACType.GetDesign(this, Global.ACUsages.DUMain, Global.ACKinds.DSDesignLayout, SettingsDesignName);
                string layoutXAML = null;
                if (acClassDesign != null && acClassDesign.ACIdentifier != "UnknowMainlayout")
                {
                    layoutXAML = acClassDesign.XAMLDesign;
                }
                else
                {
                    layoutXAML = "<vb:VBDockPanel><vb:VBTextBox ACCaption=\"Unknown:\" Text=\"" + SettingsDesignName + "\"></vb:VBTextBox></vb:VBDockPanel>";
                }
                return layoutXAML;
            }
        }

        private string _SettingsDesignName;
        public string SettingsDesignName
        {
            get
            {
                return _SettingsDesignName;
            }
            set
            {
                if (_SettingsDesignName != value)
                {
                    _SettingsDesignName = value;
                    OnPropertyChanged("SettingsDesign");
                }
            }
        }

        #endregion

        #region Settings -> Register

        public Register _HelpRegister;
        [ACPropertyInfo(9999, "HelpRegister", "en{'HelpRegister'}de{'HelpRegister'}")]
        public Register HelpRegister
        {
            get
            {
                if (_HelpRegister == null)
                    _HelpRegister = new Register();
                return _HelpRegister;
            }
            set
            {
                if (_HelpRegister != value)
                {
                    _HelpRegister = value;
                    OnPropertyChanged("HelpRegister");
                }
            }
        }

        #region Settings -> Register -> Message


        private Msg _SelectedRegisterMessage;
        /// <summary>
        /// Selected property for Msg
        /// </summary>
        /// <value>The selected RegisterMessage</value>
        [ACPropertySelected(9999, "RegisterMessage", "en{'TODO: RegisterMessage'}de{'TODO: RegisterMessage'}")]
        public Msg SelectedRegisterMessage
        {
            get
            {
                return _SelectedRegisterMessage;
            }
            set
            {
                if (_SelectedRegisterMessage != value)
                {
                    _SelectedRegisterMessage = value;
                    OnPropertyChanged("SelectedRegisterMessage");
                }
            }
        }


        private List<Msg> _RegisterMessageList;
        /// <summary>
        /// List property for Msg
        /// </summary>
        /// <value>The RegisterMessage list</value>
        [ACPropertyList(9999, "RegisterMessage")]
        public List<Msg> RegisterMessageList
        {
            get
            {
                if (_RegisterMessageList == null)
                    _RegisterMessageList = new List<Msg>();
                return _RegisterMessageList;
            }
            set
            {
                _RegisterMessageList = value;
                OnPropertyChanged("RegisterMessageList");
            }
        }

        #endregion

        #region Settings -> Register -> Methods

        [ACMethodInfo("LoginOk", "en{'Login'}de{'Anmeldung'}", 999)]
        public void RegisterOk()
        {
            ActionResult<Register> actionResult = HelpManager.ActionRegistration(HelpRegister);
            if(actionResult.Success)
            {
                CleanUpRegisterModel();
            }
            RegisterMessageList = actionResult.Messages.Select(p=>(BasicMessage)p).Select(p=>(Msg)p).ToList();
        }

        [ACMethodInfo("GotoLogin", "en{'Go to login'}de{'Gehe zum Login'}", 999)]
        public void GotoLogin()
        {
            SettingsDesignName = "Login";
        }

        #endregion 

        #endregion

        #region Settings -> Login
        private Login _HelpLogin;
        [ACPropertyInfo(9999, "HelpLogin", "en{'HelpLogin'}de{'HelpLogin'}")]
        public Login HelpLogin
        {
            get
            {
                if (_HelpLogin == null)
                    _HelpLogin = new Login();
                return _HelpLogin;
            }
            set
            {
                if (_HelpLogin != value)
                {
                    _HelpLogin = value;
                    OnPropertyChanged("HelpLogin");
                }
            }
        }

        #region Settings -> Login -> LoginMessage


        #region LoginMessage
        private Msg _SelectedLoginMessage;
        /// <summary>
        /// Selected property for Msg
        /// </summary>
        /// <value>The selected LoginMessage</value>
        [ACPropertySelected(9999, "LoginMessage", "en{'Login messages'}de{'Login-Nachrichten'}")]
        public Msg SelectedLoginMessage
        {
            get
            {
                return _SelectedLoginMessage;
            }
            set
            {
                if (_SelectedLoginMessage != value)
                {
                    _SelectedLoginMessage = value;
                    OnPropertyChanged("SelectedLoginMessage");
                }
            }
        }


        private List<Msg> _LoginMessageList;
        /// <summary>
        /// List property for Msg
        /// </summary>
        /// <value>The LoginMessage list</value>
        [ACPropertyList(9999, "LoginMessage")]
        public List<Msg> LoginMessageList
        {
            get
            {
                if (_LoginMessageList == null)
                    _LoginMessageList = new List<Msg>();
                return _LoginMessageList;
            }
        }

        #endregion

        #endregion

        #region Settings -> Login -> Methods
        [ACMethodInfo("LoginOk", "en{'Login'}de{'Anmeldung'}", 999)]
        public void LoginOk()
        {
            var task = HelpManager.Authenticate(HelpLogin);
            task.Wait();
            ActionResult<Tuple<Guid, string>> actionResult = task.Result;
            RegisterMessageList = actionResult.Messages.Select(p => (BasicMessage)p).Select(p => (Msg)p).ToList();
            loginSuccess = actionResult.Success;
            LoginDataSave();
        }

        [ACMethodInfo("GotoRegister", "en{'Go to register'}de{'Gehe zur Registrierung'}", 999)]
        public void GotoRegister()
        {
            SettingsDesignName = "Register";
        }

        #endregion

        #endregion

        #endregion

        #region BSOSettings

        private ACPropertyConfigValue<Dictionary<string, Login>> _WebHelpLoginDataSaved;
        [ACPropertyConfig("en{'Saved user login'}de{'Benztzerdatei speichern'}")]
        public Dictionary<string, Login> WebHelpLoginDataSaved
        {
            get
            {
                return _WebHelpLoginDataSaved.ValueT;
            }
            set
            {
                _WebHelpLoginDataSaved.ValueT = value;
            }
        }

        public void LoginDataSave()
        {
            string username = Root.CurrentInvokingUser.VBUserNo;
            if(!HelpLogin.RememberLogin)
            {
                HelpLogin.Password = "";
            }

            if(string.IsNullOrEmpty(HelpLogin.Email))
            {
                if (WebHelpLoginDataSaved.Keys.Contains(username))
                    WebHelpLoginDataSaved.Remove(username);
            }
            else
            {
                if (WebHelpLoginDataSaved.Keys.Contains(username))
                    WebHelpLoginDataSaved[username] = HelpLogin;
                else
                    WebHelpLoginDataSaved.Add(username, HelpLogin);
            }
        }

        public Login LoginDataRetrive()
        {
            string username = Root.CurrentInvokingUser.VBUserNo;
            if (WebHelpLoginDataSaved.Keys.Contains(username))
                return WebHelpLoginDataSaved[username];
            return null;
        }
        #endregion

        #region private mehtods

        private ACObjectItem FactoryACObjectItem(IACObject acObject)
        {
            ACObjectItem result = new ACObjectItem(acObject);
            result.ACUrl = acObject.GetACUrl();
            if (acObject.ParentACObject != null)
            {
                ACObjectItem parent = FactoryACObjectItem(acObject.ParentACObject);
                result.Add(parent);
            }
            return result;
        }

        private ACObjectItem FactoryACObjectItemType(ACClass acObject)
        {
            ACObjectItem result = new ACObjectItem(acObject);
            result.ACUrl = acObject.GetACUrl();
            if (acObject.ACClass1_BasedOnACClass != null)
            {
                ACObjectItem parent = FactoryACObjectItemType(acObject.ACClass1_BasedOnACClass);
                result.Add(parent);
            }
            return result;
        }

        private void LoadPrivateHelperList()
        {
            LanguageList = new ACValueItemList("LanguageList");
            LanguageList.AddEntry("en", "en{'English'}de{'Englisch'}");
            LanguageList.AddEntry("de", "en{'German'}de{'Deutsch'}");

            GenderList = new ACValueItemList("GenderList");
            GenderList.AddEntry("M", "en{'Male'}de{'Männlich'}");
            GenderList.AddEntry("F", "en{'Female'}de{'Weiblich'}");

            AddressTypeList = new ACValueItemList("AddressTypeList");
            AddressTypeList.AddEntry((short)1, "en{'Home'}de{'Haus'}");
            AddressTypeList.AddEntry((short)2, "en{'Work'}de{'Arbeit'}");
            AddressTypeList.AddEntry((short)3, "en{'General'}de{'Generell'}");

            PhoneTypeList = new ACValueItemList("PhoneTypeList");
            PhoneTypeList.AddEntry((short)1, "en{'home'}de{'Haus'}");
            PhoneTypeList.AddEntry((short)2, "en{'fax'}de{'Fax'}");
            PhoneTypeList.AddEntry((short)3, "en{'work'}de{'Arbeit'}");
            PhoneTypeList.AddEntry((short)4, "en{'mobile'}de{'Handy'}");
            PhoneTypeList.AddEntry((short)5, "en{'pagger'}de{'Pagger'}");

            SelectedItemTypeList = new ACValueItemList("SelectedItemElementType");
            SelectedItemTypeList.AddEntry((short)HelpItemTypeEnum.Component, "en{'Component (GUI)'}de{'Komponente (GUI)'}");
            SelectedItemTypeList.AddEntry((short)HelpItemTypeEnum.ComponentType, "en{'Component (GUI) type'}de{'Komponententyp (GUI)'}");
            SelectedItemTypeList.AddEntry((short)HelpItemTypeEnum.Element, "en{'Element (data record)'}de{'Element (Datensatz)'}");
            SelectedItemTypeList.AddEntry((short)HelpItemTypeEnum.ElementType, "en{'Element type (data record)'}de{'Elementtyp (Datensatz)'}");
        }

        private void CleanUpRegisterModel()
        {
            HelpRegister.email = "";
            HelpRegister.password = "";
            HelpRegister.passwordRepeated = "";
            HelpRegister.FirstName = "";
            HelpRegister.LastName = "";
            HelpRegister.Gender = "";
            HelpRegister.LangaugeCode = "";
            HelpRegister.AddressTypeID = 0;
            HelpRegister.Address = "";
            HelpRegister.PhoneType = 0;
            HelpRegister.PhonePrefix = "";
            HelpRegister.Phone = "";
            HelpRegister.StreetNo = "";
            HelpRegister.Street = "";
            HelpRegister.PostCode = "";
            HelpRegister.PlaceName = "";
            HelpRegister.CountryCode = "";
            HelpRegister.Latitude = "";
            HelpRegister.Longitude = "";
        }

        #endregion
    }
}
