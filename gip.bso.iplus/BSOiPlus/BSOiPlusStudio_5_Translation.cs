// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-27-2012
// ***********************************************************************
// <copyright file="BSOiPlusStudio_5_Translation.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using gip.core.datamodel;
using gip.core.manager;
using gip.core.autocomponent;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSOiPlusStudio
    /// </summary>
    public partial class BSOiPlusStudio
    {
        #region Properties
        #region Show Properties
        /// <summary>
        /// Dies sind die vom BSO verwendeten Datenentitäten.
        /// Für die Bereitstellung für einen Service, sind diese mittels AddEntityData
        /// zu registrieren
        /// </summary>
        bool _ShowTextACClass = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show text AC class].
        /// </summary>
        /// <value><c>true</c> if [show text AC class]; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(9999, "", "en{'Show Classes'}de{'Klassen anzeigen'}")]
        public bool ShowTextACClass
        {
            get
            {
                return _ShowTextACClass;
            }
            set
            {
                _ShowTextACClass = value;
                OnPropertyChanged("ShowTextACClass");
                RefreshTranslation();
            }
        }

        /// <summary>
        /// The _ show text AC class method
        /// </summary>
        bool _ShowTextACClassMethod = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show text AC class method].
        /// </summary>
        /// <value><c>true</c> if [show text AC class method]; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(9999, "", "en{'Show Methods'}de{'Methoden anzeigen'}")]
        public bool ShowTextACClassMethod
        {
            get
            {
                return _ShowTextACClassMethod;
            }
            set
            {
                _ShowTextACClassMethod = value;
                OnPropertyChanged("ShowTextACClassMethod");
                RefreshTranslation();
            }
        }

        /// <summary>
        /// The _ show text AC class property
        /// </summary>
        bool _ShowTextACClassProperty = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show text AC class property].
        /// </summary>
        /// <value><c>true</c> if [show text AC class property]; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(9999, "", "en{'Show Properties'}de{'Eigenschaften anzeigen'}")]
        public bool ShowTextACClassProperty
        {
            get
            {
                return _ShowTextACClassProperty;
            }
            set
            {
                _ShowTextACClassProperty = value;
                OnPropertyChanged("ShowTextACClassProperty");
                RefreshTranslation();
            }
        }

        /// <summary>
        /// The _ show text AC class design
        /// </summary>
        bool _ShowTextACClassDesign = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show text AC class design].
        /// </summary>
        /// <value><c>true</c> if [show text AC class design]; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(9999, "", "en{'Show Designs'}de{'Designs anzeigen'}")]
        public bool ShowTextACClassDesign
        {
            get
            {
                return _ShowTextACClassDesign;
            }
            set
            {
                _ShowTextACClassDesign = value;
                OnPropertyChanged("ShowTextACClassDesign");
                RefreshTranslation();
            }
        }

        /// <summary>
        /// The _ show text AC class text
        /// </summary>
        bool _ShowTextACClassText = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show text AC class text].
        /// </summary>
        /// <value><c>true</c> if [show text AC class text]; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(9999, "", "en{'Show Text'}de{'Texte anzeigen'}")]
        public bool ShowTextACClassText
        {
            get
            {
                return _ShowTextACClassText;
            }
            set
            {
                _ShowTextACClassText = value;
                OnPropertyChanged("ShowTextACClassText");
                RefreshTranslation();
            }
        }

        /// <summary>
        /// The _ show text AC class message
        /// </summary>
        bool _ShowTextACClassMessage = true;
        /// <summary>
        /// Gets or sets a value indicating whether [show text AC class message].
        /// </summary>
        /// <value><c>true</c> if [show text AC class message]; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(9999, "", "en{'Show Messages'}de{'Meldungen anzeigen'}")]
        public bool ShowTextACClassMessage
        {
            get
            {
                return _ShowTextACClassMessage;
            }
            set
            {
                _ShowTextACClassMessage = value;
                OnPropertyChanged("ShowTextACClassMessage");
                RefreshTranslation();
            }
        }

        /// <summary>
        /// The _ show text all
        /// </summary>
        bool _ShowTextAll = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show text all].
        /// </summary>
        /// <value><c>true</c> if [show text all]; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(9999, "", "en{'Show from all Classes'}de{'Von allen Klassen anzeigen'}")]
        public bool ShowTextAll
        {
            get
            {
                return _ShowTextAll;
            }
            set
            {
                _ShowTextAll = value;
                OnPropertyChanged("ShowTextAll");
                RefreshTranslation();
            }
        }

        /// <summary>
        /// The _ show text base
        /// </summary>
        bool _ShowTextBase = false;
        /// <summary>
        /// Gets or sets a value indicating whether [show text base].
        /// </summary>
        /// <value><c>true</c> if [show text base]; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(9999, "", "en{'Show with Baseclasses'}de{'Auch Basisklassen anzeigen'}")]
        public bool ShowTextBase
        {
            get
            {
                return _ShowTextBase;
            }
            set
            {
                _ShowTextBase = value;
                OnPropertyChanged("ShowTextBase");
                RefreshTranslation();
            }
        }

        /// <summary>
        /// The _ merge sorting caption
        /// </summary>
        bool _MergeSortingCaption = false;
        /// <summary>
        /// Gets or sets a value indicating whether [merge sorting caption].
        /// </summary>
        /// <value><c>true</c> if [merge sorting caption]; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(9999, "", "en{'Sorting Caption'}de{'Sortierung Bezeichnung'}")]
        public bool MergeSortingCaption
        {
            get
            {
                return _MergeSortingCaption;
            }
            set
            {
                _MergeSortingCaption = value;
                OnPropertyChanged("MergeSortingCaption");
                OnPropertyChanged("ACTranslationList");
            }
        }

        /// <summary>
        /// The _ new prefix
        /// </summary>
        string _NewPrefix;
        /// <summary>
        /// The _ current new AC identifier
        /// </summary>
        string _CurrentNewACIdentifier;

        /// <summary>
        /// Gets or sets the current new AC identifier.
        /// </summary>
        /// <value>The current new AC identifier.</value>
        [ACPropertyCurrent(9999, "", "en{'Identifier'}de{'Bezeichner'}")]
        public string CurrentNewACIdentifier
        {
            get
            {
                return _CurrentNewACIdentifier;
            }
            set
            {
                _CurrentNewACIdentifier = value;
                OnPropertyChanged("CurrentNewACIdentifier");
            }
        }

        /// <summary>
        /// The _ current new message
        /// </summary>
        string _CurrentNewMessage;
        /// <summary>
        /// Gets or sets the current new message.
        /// </summary>
        /// <value>The current new message.</value>
        [ACPropertyCurrent(9999, "", "en{'Messagetext'}de{'Meldungstext'}")]
        public string CurrentNewMessage
        {
            get
            {
                return _CurrentNewMessage;
            }
            set
            {
                _CurrentNewMessage = value;
                if (String.IsNullOrEmpty(_NewPrefix))
                    CurrentNewACIdentifier = value;
                OnPropertyChanged("CurrentNewMessage");
            }
        }

        /// <summary>
        /// The _ is system
        /// </summary>
        bool _IsSystem = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value><c>true</c> if this instance is system; otherwise, <c>false</c>.</value>
        [ACPropertyCurrent(9999, "", "en{'System'}de{'System'}")]
        public bool IsSystem
        {
            get
            {
                return _IsSystem;
            }
            set
            {
                _IsSystem = value;
                if (!string.IsNullOrEmpty(_NewPrefix))
                {
                    CurrentNewACIdentifier = ACClassMessage.GetUniqueNameIdentifier(Database.ContextIPlus, _NewPrefix, IsSystem);
                }
                OnPropertyChanged("IsSystem");
            }
        }

        bool _WithDynamisationProp = false;
        [ACPropertyCurrent(9999, "", "en{'With dynamisation Property'}de{'Mit Dynamisier-Eigenschaft'}")]
        public bool WithDynamisationProp
        {
            get
            {
                return _WithDynamisationProp;
            }
            set
            {
                _WithDynamisationProp = value;
                OnPropertyChanged("WithDynamisationProp");
            }
        }
        #endregion

        #region ACTranslation
        /// <summary>
        /// The _ access AC translation
        /// </summary>
        IAccessNav _AccessACTranslation;
        /// <summary>
        /// Gets the access AC translation.
        /// </summary>
        /// <value>The access AC translation.</value>
        [ACPropertyAccessPrimary(9999, "ACTranslation")]
        public IAccessNav AccessACTranslation
        {
            get
            {
                if (_AccessACTranslation == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, ComponentClass.PrimaryNavigationquery(), ACType.ACIdentifier);
                    _AccessACTranslation = navACQueryDefinition.NewAccessNav("", this);
                }
                return _AccessACTranslation;
            }
        }

        /// <summary>
        /// The _ current AC translation
        /// </summary>
        List<IACObjectEntityWithCheckTrans> _CurrentACTranslation;
        /// <summary>
        /// Gets or sets the current AC translation.
        /// </summary>
        /// <value>The current AC translation.</value>
        [ACPropertySelected(9999, "ACTranslation")]
        public List<IACObjectEntityWithCheckTrans> CurrentACTranslation
        {
            get
            {
                return _CurrentACTranslation;
            }
            set
            {
                if (_CurrentACTranslation == value)
                    return;
                if (_CurrentACTranslation != null && _CurrentACTranslation.Any())
                    UpdateTextGroupItem(_CurrentACTranslation, TranslationList);
                _CurrentACTranslation = value;
                if (_CurrentACTranslation != null && _CurrentACTranslation.Any())
                {
                    if (_CurrentACTranslation[0] != CurrentFirstACTranslation)
                    {
                        CurrentFirstACTranslation = _CurrentACTranslation[0];
                    }
                }
                else
                {
                    CurrentFirstACTranslation = null;
                }

                OnPropertyChanged("CurrentACTranslation");
            }
        }

        /// <summary>
        /// The _ current first AC translation
        /// </summary>
        IACObjectEntityWithCheckTrans _CurrentFirstACTranslation = null;
        /// <summary>
        /// Gets or sets the current first AC translation.
        /// </summary>
        /// <value>The current first AC translation.</value>
        [ACPropertyCurrent(9999, "ACTranslation")]
        public IACObjectEntityWithCheckTrans CurrentFirstACTranslation
        {
            get
            {
                return _CurrentFirstACTranslation;
            }
            set
            {
                if (_CurrentFirstACTranslation == value)
                    return;
                _CurrentFirstACTranslation = value;
                _TranslationList = null;
                if (_CurrentFirstACTranslation != null)
                {
                    _TranslationList = new List<ACValueItem>();
                    foreach (var VBLanguage in Database.ContextIPlus.VBLanguage.Where(c => c.IsTranslation))
                    {
                        ACValueItem acValueItem = new ACValueItem(VBLanguage.VBLanguageName, _CurrentFirstACTranslation.GetTranslation(VBLanguage.VBLanguageCode), null);
                        acValueItem.ParentACObject = this;
                        _TranslationList.Add(acValueItem);
                    }
                }

                OnPropertyChanged("CurrentFirstACTranslation");
                OnPropertyChanged("TranslationList");
                if (_TranslationList != null && _TranslationList.Any())
                {
                    CurrentTranslation = _TranslationList.First();
                }
            }
        }

        /// <summary>
        /// The _ AC translation list
        /// </summary>
        List<IACObjectEntityWithCheckTrans> _ACTranslationList;
        /// <summary>
        /// Gets the AC translation list.
        /// </summary>
        /// <value>The AC translation list.</value>
        [ACPropertyList(9999, "ACTranslation")]
        public IEnumerable<IACObjectEntityWithCheckTrans> ACTranslationList
        {
            get
            {
                if (MergeSortingCaption)
                {
                    return _ACTranslationList.OrderBy(c => c.ACCaption).ToList();
                }
                else
                {
                    return _ACTranslationList.OrderBy(c => c.ACIdentifier).ToList();
                }
            }
        }
        #endregion

        #region Translation
        /// <summary>
        /// The _ translation list
        /// </summary>
        List<ACValueItem> _TranslationList;
        /// <summary>
        /// Gets the translation list.
        /// </summary>
        /// <value>The translation list.</value>
        [ACPropertyList(9999, "Translation")]
        public IEnumerable<ACValueItem> TranslationList
        {
            get
            {
                return _TranslationList;
            }
        }

        /// <summary>
        /// The _ current translation
        /// </summary>
        ACValueItem _CurrentTranslation;
        /// <summary>
        /// Gets or sets the current translation.
        /// </summary>
        /// <value>The current translation.</value>
        [ACPropertyCurrent(9999, "Translation")]
        public ACValueItem CurrentTranslation
        {
            get
            {
                return _CurrentTranslation;
            }
            set
            {
                _CurrentTranslation = value;
                UpdateTextGroupItem(CurrentACTranslation, TranslationList);
                OnPropertyChanged("CurrentTranslation");
            }
        }
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Diese Methoden sind nur innerhalb der Serverseite verfügbar und können nicht
        /// direkt über Services bereitgestellt werden.
        /// Für die Bereitstellung für einen Service, sind diese mittels AddCommand
        /// zu registrieren
        /// </summary>
        [ACMethodInteraction("ACTranslation", "en{'New Text'}de{'Neuer Text'}", (short)MISort.New, true, "CurrentACTranslation")]
        public void NewText()
        {
            _NewPrefix = "";
            CurrentNewACIdentifier = "";
            ShowDialog(this, "NewText");
        }

        /// <summary>
        /// Determines whether [is enabled new text].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new text]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewText()
        {
            return ShowTextACClassText;
        }

        /// <summary>
        /// News the message info.
        /// </summary>
        [ACMethodInteraction("MDTextGroupItem", "en{'New Info'}de{'Neue Info'}", (short)MISort.New, true, "CurrentACTranslation")]
        public void NewMessageInfo()
        {
            _NewPrefix = "Info";
            CurrentNewACIdentifier = ACClassMessage.GetUniqueNameIdentifier(Database.ContextIPlus, _NewPrefix, IsSystem);
            ShowDialog(this, "NewMessage");
        }

        /// <summary>
        /// Determines whether [is enabled new message info].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new message info]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewMessageInfo()
        {
            return ShowTextACClassMessage;
        }
        /// <summary>
        /// News the message warning.
        /// </summary>
        [ACMethodInteraction("MDTextGroupItem", "en{'New Warning'}de{'Neue Warnung'}", (short)MISort.New, true, "CurrentACTranslation")]
        public void NewMessageWarning()
        {
            _NewPrefix = "Warning";
            CurrentNewACIdentifier = ACClassMessage.GetUniqueNameIdentifier(Database.ContextIPlus, _NewPrefix, IsSystem);
            ShowDialog(this, "NewMessage");
        }

        /// <summary>
        /// Determines whether [is enabled new message warning].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new message warning]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewMessageWarning()
        {
            return ShowTextACClassMessage;
        }

        /// <summary>
        /// News the message failure.
        /// </summary>
        [ACMethodInteraction("MDTextGroupItem", "en{'New Failure'}de{'Neue Ausfall'}", (short)MISort.New, true, "CurrentACTranslation")]
        public void NewMessageFailure()
        {
            _NewPrefix = "Failure";
            CurrentNewACIdentifier = ACClassMessage.GetUniqueNameIdentifier(Database.ContextIPlus, _NewPrefix, IsSystem);
            ShowDialog(this, "NewMessage");
        }

        /// <summary>
        /// Determines whether [is enabled new message failure].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new message failure]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewMessageFailure()
        {
            return ShowTextACClassMessage;
        }

        /// <summary>
        /// News the message error.
        /// </summary>
        [ACMethodInteraction("MDTextGroupItem", "en{'New Error'}de{'Neue Fehler'}", (short)MISort.New, true, "CurrentACTranslation")]
        public void NewMessageError()
        {
            _NewPrefix = "Error";
            CurrentNewACIdentifier = ACClassMessage.GetUniqueNameIdentifier(Database.ContextIPlus, _NewPrefix, IsSystem);
            ShowDialog(this, "NewMessage");
        }

        /// <summary>
        /// Determines whether [is enabled new message error].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new message error]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewMessageError()
        {
            return ShowTextACClassMessage;
        }

        /// <summary>
        /// News the message exception.
        /// </summary>
        [ACMethodInteraction("MDTextGroupItem", "en{'New Exception'}de{'Neue Ausnahme'}", (short)MISort.New, true, "CurrentACTranslation")]
        public void NewMessageException()
        {
            _NewPrefix = "Exception";
            CurrentNewACIdentifier = ACClassMessage.GetUniqueNameIdentifier(Database.ContextIPlus, _NewPrefix, IsSystem);
            ShowDialog(this, "NewMessage");
        }

        /// <summary>
        /// Determines whether [is enabled new message exception].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new message exception]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewMessageException()
        {
            return ShowTextACClassMessage;
        }

        /// <summary>
        /// News the message question.
        /// </summary>
        [ACMethodInteraction("MDTextGroupItem", "en{'New Question'}de{'Neue Frage'}", (short)MISort.New, true, "CurrentACTranslation")]
        public void NewMessageQuestion()
        {
            _NewPrefix = "Question";
            CurrentNewACIdentifier = ACClassMessage.GetUniqueNameIdentifier(Database.ContextIPlus, _NewPrefix, IsSystem);
            ShowDialog(this, "NewMessage");
        }

        /// <summary>
        /// Determines whether [is enabled new message question].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new message question]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewMessageQuestion()
        {
            return ShowTextACClassMessage;
        }

        /// <summary>
        /// News the message status.
        /// </summary>
        [ACMethodInteraction("MDTextGroupItem", "en{'New Status'}de{'Neuer Status'}", (short)MISort.New, true, "CurrentACTranslation")]
        public void NewMessageStatus()
        {
            _NewPrefix = "Status";
            CurrentNewACIdentifier = ACClassMessage.GetUniqueNameIdentifier(Database.ContextIPlus, _NewPrefix, IsSystem);
            ShowDialog(this, "NewMessage");
        }

        /// <summary>
        /// Determines whether [is enabled new message status].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new message status]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewMessageStatus()
        {
            return ShowTextACClassMessage;
        }

        /// <summary>
        /// News the OK.
        /// </summary>
        [ACMethodCommand("NewText", "en{'OK'}de{'OK'}", (short)MISort.Okay)]
        public void NewOK()
        {
            CloseTopDialog();
            if (string.IsNullOrEmpty(_NewPrefix))
            {
                ACClassText acClassText = ACClassText.NewACObject(Database.ContextIPlus, CurrentACClass);
                acClassText.ACIdentifier = CurrentNewACIdentifier;
                acClassText.ACCaption = CurrentNewMessage;
                Database.ContextIPlus.ACClassText.Add(acClassText);
                ACSaveChanges();
                _ACTranslationList.Add(acClassText);
                OnPropertyChanged("ACTranslationList");
                // TODO: Multiselect CurrentACTranslation = acClassText;
            }
            else
            {
                ACClassMessage acClassMessage = ACClassMessage.NewACObject(Database.ContextIPlus, CurrentACClass);
                acClassMessage.ACIdentifier = CurrentNewACIdentifier;
                acClassMessage.ACCaption = CurrentNewMessage;
                Database.ContextIPlus.ACClassMessage.Add(acClassMessage);
                ACSaveChanges();
                _ACTranslationList.Add(acClassMessage);
                OnPropertyChanged("ACTranslationList");
                // TODO: Multiselect CurrentACTranslation = acClassMessage;
            }

            if (WithDynamisationProp)
            {
                NewACClassProperty();
                CurrentACClassProperty.ValueTypeACClass = Database.ContextIPlus.GetACType(typeof(Boolean));
                CurrentACClassProperty.ACIdentifier = CurrentNewACIdentifier;
                CurrentACClassProperty.ACCaption = CurrentNewACIdentifier;
                CurrentACClassProperty.IsProxyProperty = true;
                CurrentACClassProperty.IsBroadcast = true;
            }
        }

        /// <summary>
        /// Determines whether [is enabled new OK].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new OK]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewOK()
        {
            if (string.IsNullOrEmpty(CurrentNewACIdentifier) || string.IsNullOrEmpty(CurrentNewMessage))
                return false;
            return true;
        }

        /// <summary>
        /// News the cancel.
        /// </summary>
        [ACMethodCommand("NewText", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Cancel)]
        public void NewCancel()
        {
            CloseTopDialog();
            CurrentNewACIdentifier = null;
            _NewPrefix = null;
            CurrentNewACIdentifier = "";
        }


        /// <summary>
        /// Deletes the AC translation.
        /// </summary>
        [ACMethodInteraction("ACTranslation", "en{'Delete'}de{'Löschen'}", (short)MISort.Delete, true, "CurrentACTranslation", Global.ACKinds.MSMethodPrePost)]
        public void DeleteACTranslation()
        {
            if (!PreExecute("Delete")) return;
            if (!IsEnabledDeleteACTranslation())
                return;
            Msg msg = CurrentFirstACTranslation.DeleteACObject(Database.ContextIPlus, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }
            PostExecute("Delete");

            ACSaveChanges();
            RefreshTranslation();
        }

        /// <summary>
        /// Determines whether [is enabled delete AC translation].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete AC translation]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteACTranslation()
        {
            if (CurrentACTranslation == null)
                return false;
            if (CurrentACTranslation == null || CurrentACTranslation.Count() != 1)
                return false;
            if (!(CurrentFirstACTranslation is ACClassText) && !(CurrentFirstACTranslation is ACClassMessage))
                return false;
            return true;
        }

        /// <summary>
        /// Refreshes the translation.
        /// </summary>
        [ACMethodCommand("ACTranslation", "en{'Refresh'}de{'Aktualisieren'}", (short)MISort.Search)]
        public void RefreshTranslation()
        {
            _ACTranslationList = new List<IACObjectEntityWithCheckTrans>();
            if (ShowTextAll)
            {
                if (ShowTextACClass)
                {
                    IEnumerable<ACClass> query;
                    if (string.IsNullOrEmpty(AccessACTranslation.NavACQueryDefinition.SearchWord))
                        query = Database.ContextIPlus.ACClass;
                    else
                        query = Database.ContextIPlus.ACClass.Where(c => c.ACIdentifier.Contains(AccessACTranslation.NavACQueryDefinition.SearchWord));
                    _ACTranslationList.Add(CurrentACClass);
                }
                if (ShowTextACClassMethod)
                {
                    IEnumerable<ACClassMethod> query;
                    if (string.IsNullOrEmpty(AccessACTranslation.NavACQueryDefinition.SearchWord))
                        query = Database.ContextIPlus.ACClassMethod;
                    else
                        query = Database.ContextIPlus.ACClassMethod.Where(c => c.ACIdentifier.Contains(AccessACTranslation.NavACQueryDefinition.SearchWord));
                    _ACTranslationList.AddRange(query);
                }
                if (ShowTextACClassProperty)
                {
                    IEnumerable<ACClassProperty> query;
                    if (string.IsNullOrEmpty(AccessACTranslation.NavACQueryDefinition.SearchWord))
                        query = Database.ContextIPlus.ACClassProperty;
                    else
                        query = Database.ContextIPlus.ACClassProperty.Where(c => c.ACIdentifier.Contains(AccessACTranslation.NavACQueryDefinition.SearchWord));
                    _ACTranslationList.AddRange(query);
                }
                if (ShowTextACClassDesign)
                {
                    IEnumerable<ACClassDesign> query;
                    if (string.IsNullOrEmpty(AccessACTranslation.NavACQueryDefinition.SearchWord))
                        query = Database.ContextIPlus.ACClassDesign;
                    else
                        query = Database.ContextIPlus.ACClassDesign.Where(c => c.ACIdentifier.Contains(AccessACTranslation.NavACQueryDefinition.SearchWord));
                    _ACTranslationList.AddRange(query);
                }
                if (ShowTextACClassText)
                {
                    IEnumerable<ACClassText> query;
                    if (string.IsNullOrEmpty(AccessACTranslation.NavACQueryDefinition.SearchWord))
                        query = Database.ContextIPlus.ACClassText;
                    else
                        query = Database.ContextIPlus.ACClassText.Where(c => c.ACIdentifier.Contains(AccessACTranslation.NavACQueryDefinition.SearchWord));
                    _ACTranslationList.AddRange(query);
                }
                if (ShowTextACClassMessage)
                {
                    IEnumerable<ACClassMessage> query;
                    if (string.IsNullOrEmpty(AccessACTranslation.NavACQueryDefinition.SearchWord))
                        query = Database.ContextIPlus.ACClassMessage;
                    else
                        query = Database.ContextIPlus.ACClassMessage.Where(c => c.ACIdentifier.Contains(AccessACTranslation.NavACQueryDefinition.SearchWord));
                    _ACTranslationList.AddRange(query);
                }
            }
            else
            {
                if (ShowTextACClass)
                {
                    _ACTranslationList.Add(CurrentACClass);
                }
                if (ShowTextACClassMethod)
                {
                    IEnumerable<ACClassMethod> query;
                    if (string.IsNullOrEmpty(AccessACTranslation.NavACQueryDefinition.SearchWord))
                        query = (ShowTextBase ? CurrentACClass.Methods : CurrentACClass.ACClassMethod_ACClass);
                    else
                        query = (ShowTextBase ? CurrentACClass.Methods : CurrentACClass.ACClassMethod_ACClass).Where(c => c.ACIdentifier.Contains(AccessACTranslation.NavACQueryDefinition.SearchWord));
                    _ACTranslationList.AddRange(query);
                }
                if (ShowTextACClassProperty)
                {
                    IEnumerable<ACClassProperty> query;
                    if (string.IsNullOrEmpty(AccessACTranslation.NavACQueryDefinition.SearchWord))
                        query = ShowTextBase ? CurrentACClass.Properties : CurrentACClass.ACClassProperty_ACClass;
                    else
                        query = (ShowTextBase ? CurrentACClass.Properties : CurrentACClass.ACClassProperty_ACClass).Where(c => c.ACIdentifier.Contains(AccessACTranslation.NavACQueryDefinition.SearchWord));
                    _ACTranslationList.AddRange(query);
                }
                if (ShowTextACClassDesign)
                {
                    IEnumerable<ACClassDesign> query;
                    if (string.IsNullOrEmpty(AccessACTranslation.NavACQueryDefinition.SearchWord))
                        query = ShowTextBase ? CurrentACClass.Designs : CurrentACClass.ACClassDesign_ACClass;
                    else
                        query = (ShowTextBase ? CurrentACClass.Designs : CurrentACClass.ACClassDesign_ACClass).Where(c => c.ACIdentifier.Contains(AccessACTranslation.NavACQueryDefinition.SearchWord));
                    _ACTranslationList.AddRange(query);
                }
                if (ShowTextACClassText)
                {
                    IEnumerable<ACClassText> query;
                    if (string.IsNullOrEmpty(AccessACTranslation.NavACQueryDefinition.SearchWord))
                        query = ShowTextBase ? CurrentACClass.Texts : CurrentACClass.ACClassText_ACClass;
                    else
                        query = (ShowTextBase ? CurrentACClass.Texts : CurrentACClass.ACClassText_ACClass).Where(c => c.ACIdentifier.Contains(AccessACTranslation.NavACQueryDefinition.SearchWord));
                    _ACTranslationList.AddRange(query);
                }
                if (ShowTextACClassMessage)
                {
                    IEnumerable<ACClassMessage> query;
                    if (string.IsNullOrEmpty(AccessACTranslation.NavACQueryDefinition.SearchWord))
                        query = ShowTextBase ? CurrentACClass.Messages : CurrentACClass.ACClassMessage_ACClass;
                    else
                        query = (ShowTextBase ? CurrentACClass.Messages : CurrentACClass.ACClassMessage_ACClass).Where(c => c.ACIdentifier.Contains(AccessACTranslation.NavACQueryDefinition.SearchWord));
                    _ACTranslationList.AddRange(query);
                }
            }
            OnPropertyChanged("ACTranslationList");
        }
        #endregion

        #region Functions
        /// <summary>
        /// Updates the text group item.
        /// </summary>
        /// <param name="textGroupItemList">The text group item list.</param>
        /// <param name="translationList">The translation list.</param>
        void UpdateTextGroupItem(List<IACObjectEntityWithCheckTrans> textGroupItemList, IEnumerable<ACValueItem> translationList)
        {
            foreach (var textGroupItem in textGroupItemList)
            {
                string acCaptionTranslation = "";
                foreach (var translation in translationList)
                {
                    string text = translation.Value as string;
                    if (!string.IsNullOrEmpty(text))
                    {
                        // Warum wird per translation.ACCaption gesucht und nicht per MDLanguageCode? 
                        VBLanguage language = Database.ContextIPlus.VBLanguage.Where(c => c.IsTranslation && c.VBNameTrans.Contains(translation.ACCaption)).FirstOrDefault();
                        if (language != null)
                        {
                            acCaptionTranslation += language.VBLanguageCode + "{'";
                            acCaptionTranslation += text;
                            acCaptionTranslation += "'}";
                        }
                    }
                }

                if (textGroupItem.ACCaptionTranslation != acCaptionTranslation)
                    textGroupItem.ACCaptionTranslation = acCaptionTranslation;
            }
        }

        #endregion

    }
}
