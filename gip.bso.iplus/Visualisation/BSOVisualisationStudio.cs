// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-08-2012
// ***********************************************************************
// <copyright file="BSOVisualisationStudio.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.manager;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Input;

namespace gip.bso.iplus
{
    /// <summary>
    /// BSOVBVisualCenter dient der Visualisierung
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Visualisation'}de{'Visualisierung'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
    public class BSOVisualisationStudio : ACBSO, IACComponentDesignManagerHost, IACBSOAlarmPresenter
    {
        public const string BSOClassName = "BSOVisualisationStudio";

        #region private members
        /// <summary>
        /// The _ AC project manager
        /// </summary>
        ACProjectManager _ACProjectManager;
        #endregion

        #region c´tors

        /// <summary>
        /// Initializes a new instance of the <see cref="BSOVisualisationStudio"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOVisualisationStudio(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        /// <summary>
        /// ACs the init.
        /// </summary>
        /// <param name="startChildMode">The start child mode.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            _ACProjectManager = new ACProjectManager(Database.ContextIPlus, Root);

            var visualisationList = VisualisationList;
            if (visualisationList != null && visualisationList.Any())
            {
                VBUserACClassDesign defaultVisualisation = Database.ContextIPlus.VBUserACClassDesign.Where(c => c.VBUserID == Root.Environment.User.VBUserID
                                                                                                                      && c.ACClassDesign != null
                                                                                                                      && c.ACClassDesign.ACUsageIndex == (short)Global.ACUsages.DUVisualisation)
                                                                                                    .ToList().FirstOrDefault(x => x.XMLDesign == "" && x.ACIdentifier == "DefaultVisualisation");
                
                if(defaultVisualisation != null && defaultVisualisation.ACClassDesign != null)
                {
                    ACClassDesign visualisation = visualisationList.FirstOrDefault(c => c.ACIdentifier == defaultVisualisation.ACClassDesign.ACIdentifier);
                    if (visualisation != null)
                        CurrentVisualisation = visualisation;
                    else
                        CurrentVisualisation = visualisationList.First();      
                }
                else
                    CurrentVisualisation = visualisationList.First();
            }

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            CurrentVisualisation = null;
            this._CurrentDesignItem = null;
            this._CurrentInfo = null;
            this._CurrentNewVisualisation = null;
            this._CurrentVisualisation = null;
            this._MessageWorkOrderMethodID = null;
            this._MessageWorkOrderWFID = null;
            this._SelectedVisualisation = null;
            this._VisualisationList = null;
            bool done = base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return done;
        }

        private Database _BSODatabase = null;
        /// <summary>
        /// Overriden: Returns a separate database context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_BSODatabase == null)
                    _BSODatabase = ACObjectContextManager.GetOrCreateContext<Database>(this.GetACUrl());
                return _BSODatabase;
            }
        }

        #endregion

        #region BSO->ACDropData
        /// <summary>
        /// Called from a WPF-Control inside it's ACAction-Method when a relevant interaction-event as occured (e.g. Drag-And-Drop).
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source.</param>
        public override void ACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
        {
            base.ACActionToTarget(targetVBDataObject, actionArgs);
        }

        /// <summary>
        /// Called from a WPF-Control when a relevant interaction-event as occured (e.g. Drag-And-Drop) and the related component should check if this interaction-event should be handled.
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public override bool IsEnabledACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
        {
            return base.IsEnabledACActionToTarget(targetVBDataObject, actionArgs);
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public override void ACAction(ACActionArgs actionArgs)
        {
            if (actionArgs.ElementAction == Global.ElementActionType.VBDesignChanged && !_SwitchingOnAlarm)
            {
                _HighlightParentOnSwitch = null;
                _CompForSwitchingView = null;
                _CurrentHighlightedParent = null;
                base.ACAction(actionArgs);
            }
            else
                base.ACAction(actionArgs);
        }

        protected override void OnWPFRefAdded(int hashOfDepObj, IACObject boundedObject)
        {
            base.OnWPFRefAdded(hashOfDepObj, boundedObject);
            if (_CompForSwitchingView != null
                && boundedObject is IACComponent)
            {
                string boundedObjUrl = boundedObject.GetACUrl();
                if (  (_CompForSwitchingView.Item2 != null && _CompForSwitchingView.Item2 == boundedObject)
                    || _CompForSwitchingView.Item1 == boundedObjUrl)
                {
                    VBBSOSelectionManager.HighlightContentACObject(boundedObject, false);
                    _HighlightParentOnSwitch = null;
                    _CompForSwitchingView = null;
                }
                else if (!String.IsNullOrEmpty(_HighlightParentOnSwitch) && boundedObjUrl.StartsWith(_HighlightParentOnSwitch))
                {
                    if (String.IsNullOrEmpty(_CurrentHighlightedParent))
                    {
                        _CurrentHighlightedParent = boundedObjUrl;
                        VBBSOSelectionManager.HighlightContentACObject(boundedObject, false);
                    }
                    else
                    {
                        int distanceCurrent = ACUrlHelper.CalcDistance(_CompForSwitchingView.Item1, _CurrentHighlightedParent);
                        int distanceThis = ACUrlHelper.CalcDistance(_CompForSwitchingView.Item1, boundedObjUrl);
                        if (distanceCurrent >= 0 && distanceThis >= 0 && distanceThis < distanceCurrent)
                        {
                            _CurrentHighlightedParent = boundedObjUrl;
                            VBBSOSelectionManager.HighlightContentACObject(boundedObject, false);
                        }
                    }
                }
            }
        }
        #endregion

        #region BSO->ACProperty

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public override string ACCaption
        {
            get
            {
                if (CurrentVisualisation == null) return base.ACCaption;
                return CurrentVisualisation.ACCaption;
            }
        }

        #region BSO->ACProperty->Visualisation
        /// <summary>
        /// The _ current visualisation
        /// </summary>
        ACClassDesign _CurrentVisualisation;
        /// <summary>
        /// Gets or sets the current visualisation.
        /// </summary>
        /// <value>The current visualisation.</value>
        [ACPropertyCurrent(401, "Visualisation")]
        public ACClassDesign CurrentVisualisation
        {
            get
            {
                return _CurrentVisualisation;
            }
            set
            {
                if (_CurrentVisualisation != value)
                {
                    // Nur wenn kein VBDesigner aktiv
                    if (!this.ACComponentChilds.Where(c => c is VBDesigner).Any())
                    {
                        bool changed = _CurrentVisualisation != value;
                        _CurrentVisualisation = value;
                        OnPropertyChanged("CurrentVisualisation");
                        OnPropertyChanged("CurrentVisualisationXAML");
                        OnPropertyChanged(Const.ACCaptionPrefix);
                        if (changed)
                            this.ReferencePoint.CheckWPFRefsAndDetachUnusedProxies();
                    }
                }
            }
        }

        /// <summary>
        /// The _ selected visualisation
        /// </summary>
        ACClassDesign _SelectedVisualisation;
        /// <summary>
        /// Gets or sets the selected visualisation.
        /// </summary>
        /// <value>The selected visualisation.</value>
        [ACPropertySelected(402, "Visualisation")]
        public ACClassDesign SelectedVisualisation
        {
            get
            {
                return _SelectedVisualisation;
            }
            set
            {
                _SelectedVisualisation = value;
                OnPropertyChanged("SelectedVisualisation");
            }
        }

        /// <summary>
        /// The _ visualisation list
        /// </summary>
        List<ACClassDesign> _VisualisationList = null;
        /// <summary>
        /// Gets the visualisation list.
        /// </summary>
        /// <value>The visualisation list.</value>
        [ACPropertyList(403, "Visualisation")]
        public IEnumerable<ACClassDesign> VisualisationList
        {
            get
            {
                if (_VisualisationList == null)
                {
                    ACClass bsoVisualisationStudio = Database.ContextIPlus.ACClass.FirstOrDefault(c => c.ACIdentifier == this.ACType.ACIdentifier);
                    if (bsoVisualisationStudio == null)
                        return null;
                    _VisualisationList = bsoVisualisationStudio.ACClassDesign_ACClass.Where(c => c.ACUsageIndex == (int)Global.ACUsages.DUVisualisation && c.SortIndex < 9999)
                                                                                     .OrderBy(c => c.SortIndex)
                                                                                     .ThenBy(c => c.ACCaption).ToList();
                    //                    Refresh(System.Data.Objects.RefreshMode.StoreWins, entityList);
                }
                return _VisualisationList;
            }
        }

        /// <summary>
        /// The _ current new visualisation
        /// </summary>
        ACClassDesign _CurrentNewVisualisation;
        /// <summary>
        /// Gets or sets the current new visualisation.
        /// </summary>
        /// <value>The current new visualisation.</value>
        [ACPropertyCurrent(404, "NewVisualisation")]
        public ACClassDesign CurrentNewVisualisation
        {
            get
            {
                return _CurrentNewVisualisation;
            }
            set
            {
                _CurrentNewVisualisation = value;
                OnPropertyChanged("CurrentNewVisualisation");
            }
        }

        public VBBSOSelectionManager VBBSOSelectionManager
        {
            get
            {
                VBBSOSelectionManager selectionManager = FindChildComponents<VBBSOSelectionManager>(c => c is VBBSOSelectionManager).FirstOrDefault();
                if (selectionManager != null)
                    return selectionManager;
                selectionManager = StartComponent(Const.SelectionManagerCDesign_ClassName, null, null) as VBBSOSelectionManager;
                return selectionManager;
            }
        }

        #endregion


        #region BSO->ACProperty->ACVisualItem
        //ACVisualItem _CurrentACVisualItem;
        //[ACPropertyCurrent(9999, "ACVisualItem")]
        //public ACVisualItem CurrentACVisualItem
        //{
        //   get
        //   {
        //       return _CurrentACVisualItem;
        //   }
        //   set
        //   {
        //       _CurrentACVisualItem = value;
        //       OnPropertyChanged("CurrentACVisualItem");
        //   }
        //}

        //ACVisualItem _SelectedACVisualItem;
        //[ACPropertySelected(9999, "ACVisualItem")]
        //public ACVisualItem SelectedACVisualItem
        //{
        //    get
        //    {
        //        return _SelectedACVisualItem;
        //    }
        //    set
        //    {
        //        _SelectedACVisualItem = value;
        //        OnPropertyChanged("SelectedACVisualItem");
        //    }
        //}

        //[ACPropertyList(9999, "ACVisualItem")]
        //public IEnumerable<ACVisualItem> ACVisualItemList
        //{
        //    get
        //    {
        //        if (CurrentVisualisation == null)
        //            return null;
        //        if (!CurrentVisualisation.ACVisualItem_ACVisual.IsLoaded)
        //        {
        //            CurrentVisualisation.ACVisualItem_ACVisual.Load();
        //        }
        //        return CurrentVisualisation.ACVisualItem_ACVisual;
        //    }
        //}

        /// <summary>
        /// The _ current design item
        /// </summary>
        IACInteractiveObject _CurrentDesignItem;
        /// <summary>
        /// Gets or sets the current design item.
        /// </summary>
        /// <value>The current design item.</value>
        [ACPropertyCurrent(9999, "VisualItem")]
        public IACInteractiveObject CurrentDesignItem
        {
            get
            {
                return _CurrentDesignItem;
            }
            set
            {
                _CurrentDesignItem = value;
                //if (value is VBVisual)
                //{
                //    CurrentACVisualItem = ((gip.core.layoutengine.VBVisual)(value)).ACPosition as ACVisualItem;
                //}
                //else
                //{
                //    CurrentACVisualItem = null;
                //}
                OnPropertyChanged("CurrentDesignItem");
            }
        }

        //ACVisualItem _SelectedLoadedACVisualItem;
        //[ACPropertySelected(9999, "LoadedACVisualItem")]
        //public ACVisualItem SelectedLoadedACVisualItem
        //{
        //    get
        //    {
        //        return _SelectedLoadedACVisualItem;
        //    }
        //    set
        //    {
        //        _SelectedLoadedACVisualItem = value;
        //        OnPropertyChanged("SelectedLoadedACVisualItem");
        //    }
        //}

        /// <summary>
        /// The _ current info
        /// </summary>
        string _CurrentInfo;
        /// <summary>
        /// Gets or sets the current info.
        /// </summary>
        /// <value>The current info.</value>
        [ACPropertyInfo(9999)]
        public string CurrentInfo
        {
            get
            {
                return _CurrentInfo;
            }
            set
            {
                _CurrentInfo = value;
                OnPropertyChanged("CurrentInfo");
            }
        }
        #endregion

        #region BSO->ACProperty->Visualisation
        /// <summary>
        /// Gets the current visualisation XAML.
        /// </summary>
        /// <value>The current visualisation XAML.</value>
        public string CurrentVisualisationXAML
        {
            get
            {
                if (CurrentVisualisation == null)
                    return null;
                return CurrentVisualisation.XMLDesign;
            }
        }
        #endregion

        #region BSO=>ACPropety=>Others

        private string _SearchText;
        [ACPropertyInfo(9999, "", "en{'Search text'}de{'Suchtext'}")]
        public string SearchText
        {
            get => _SearchText;
            set
            {
                _SearchText = value;
                OnPropertyChanged();
                if (string.IsNullOrEmpty(value))
                {
                    AvailableSearchedItemsList = null;
                    SelectedAvailableSearchedItem = null;
                }
                else
                {
                    SearchComponents();
                }
            }
        }

        private ACClass _SelectedAvailableSearchedItem;
        [ACPropertySelected(9999, "AvailableSearchedItem", "en{'Available searched item'}de{'Verfügbares gesuchtes Element'}")]
        public ACClass SelectedAvailableSearchedItem
        {
            get => _SelectedAvailableSearchedItem;
            set
            {
                _SelectedAvailableSearchedItem = value;
                OnPropertyChanged();
            }
        }

        private List<ACClass> _AvailableSearchedItemsList;
        [ACPropertyList(9999, "AvailableSearchedItem")]
        public List<ACClass> AvailableSearchedItemsList
        {
            get => _AvailableSearchedItemsList;
            set
            {
                _AvailableSearchedItemsList = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #endregion

        #region BSO->ACMethod

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(Save):
                    Save();
                    return true;
                case nameof(IsEnabledSave):
                    result = IsEnabledSave();
                    return true;
                case nameof(UndoSave):
                    UndoSave();
                    return true;
                case nameof(IsEnabledUndoSave):
                    result = IsEnabledUndoSave();
                    return true;
                case nameof(New):
                    New();
                    return true;
                case nameof(IsEnabledNew):
                    result = IsEnabledNew();
                    return true;
                case nameof(Delete):
                    Delete();
                    return true;
                case nameof(IsEnabledDelete):
                    result = IsEnabledDelete();
                    return true;
                case nameof(Search):
                    Search();
                    return true;
                case nameof(NewVisualisationOK):
                    NewVisualisationOK();
                    return true;
                case nameof(IsEnabledNewVisualisationOK):
                    result = IsEnabledNewVisualisationOK();
                    return true;
                case nameof(NewVisualisationCancel):
                    NewVisualisationCancel();
                    return true;
                case nameof(SetDefaultVisualisation):
                    SetDefaultVisualisation();
                    return true;
                case nameof(IsEnabledSetDefaultVisualisation):
                    result = IsEnabledSetDefaultVisualisation();
                    return true;
                case nameof(Screenshot):
                    Screenshot();
                    return true;
                case nameof(CopyVisualisationToClipboard):
                    CopyVisualisationToClipboard();
                    return true;
                case nameof(IsEnabledCopyVisualisationToClipboard):
                    result = IsEnabledCopyVisualisationToClipboard();
                    return true;
                case nameof(ExportDesigns):
                    ExportDesigns();
                    return true;
                case nameof(IsEnabledExportDesigns):
                    result = IsEnabledExportDesigns();
                    return true;
                //case nameof(NavigateToDesign):
                    //NavigateToDesign(acParameter[0] as MouseButtonEventArgs, acParameter[1] as string);
                    //return true;
                case nameof(DesignOpen):
                    DesignOpen(acParameter[0] as string);
                    return true;
                case nameof(ValidateInput):
                    result = ValidateInput((String)acParameter[0], (Object)acParameter[1], (System.Globalization.CultureInfo)acParameter[2]);
                    return true;
                case nameof(SearchComponents):
                    SearchComponents();
                    return true;
                case nameof(IsEnabledSearchComponents):
                    result = IsEnabledSearchComponents();
                    return true;
                case nameof(NavigateToComponent):
                    NavigateToComponent();
                    return true;
                case nameof(IsEnabledNavigateToComponent):
                    result = IsEnabledNavigateToComponent();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


        /// <summary>
        /// Saves this instance.
        /// </summary>
        [ACMethodCommand("ACVisual", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void Save()
        {
            OnSave();
        }

        /// <summary>
        /// Determines whether [is enabled save].
        /// </summary>
        /// <returns><c>true</c> if [is enabled save]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledSave()
        {
            return OnIsEnabledSave();
        }

        /// <summary>
        /// Undoes the save.
        /// </summary>
        [ACMethodCommand("ACVisual", "en{'Undo'}de{'Nicht speichern'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost)]
        public void UndoSave()
        {
            OnUndoSave();
        }

        /// <summary>
        /// Determines whether [is enabled undo save].
        /// </summary>
        /// <returns><c>true</c> if [is enabled undo save]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledUndoSave()
        {
            return OnIsEnabledUndoSave();
        }

        /// <summary>
        /// News this instance.
        /// </summary>
        [ACMethodInteraction("Visualisation", "en{'New'}de{'Neu'}", (short)MISort.New, true, "SelectedVisualisation", Global.ACKinds.MSMethodPrePost)]
        public void New()
        {
            if (!PreExecute("New")) return;
            if (ComponentClass == null)
                return;
            CurrentNewVisualisation = _ACProjectManager.NewVisualisation(ComponentClass);

            ShowDialog(this, "VisualisationNew");

            PostExecute("New");
        }

        /// <summary>
        /// Determines whether [is enabled new].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNew()
        {
            return true;
        }

        /// <summary>
        /// News the visualisation OK.
        /// </summary>
        [ACMethodCommand("Visualisation", "en{'OK'}de{'OK'}", (short)MISort.Okay)]
        public void NewVisualisationOK()
        {
            CloseTopDialog();
            Save();
            _VisualisationList = null;
            OnPropertyChanged("VisualisationList");
            CurrentVisualisation = CurrentNewVisualisation;
        }

        /// <summary>
        /// Determines whether [is enabled new visualisation OK].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new visualisation OK]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewVisualisationOK()
        {
            if (CurrentNewVisualisation == null 
                || string.IsNullOrEmpty(CurrentNewVisualisation.ACCaption) 
                || string.IsNullOrEmpty(CurrentNewVisualisation.ACIdentifier)
                || CurrentNewVisualisation.ACIdentifier.ContainsACUrlDelimiters() 
                || CurrentNewVisualisation.ACIdentifier.Contains(" "))
                return false;
            return true;
        }

        /// <summary>
        /// News the visualisation cancel.
        /// </summary>
        [ACMethodCommand("Visualisation", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Cancel)]
        public void NewVisualisationCancel()
        {
            CloseTopDialog();
            CurrentNewVisualisation = null;
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        [ACMethodInteraction("ACVisual", "en{'Delete'}de{'Löschen'}", (short)MISort.Delete, true, "SelectedVisualisation", Global.ACKinds.MSMethodPrePost)]
        public void Delete()
        {
            if (!PreExecute("Delete")) return;
            Msg msg = CurrentVisualisation.DeleteACObject(Database.ContextIPlus, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }

            _VisualisationList = null;
            OnPropertyChanged("VisualisationList");
            CurrentVisualisation = VisualisationList.First();
            PostExecute("Delete");
        }

        /// <summary>
        /// Determines whether [is enabled delete].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDelete()
        {
            return CurrentVisualisation != null;
        }


        [ACMethodCommand("Visualisation", "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            ShowDialog(this, "SearchDialog");
        }


        //[ACMethodInfo("NavigateToDesign", "", 490)]
        //public void NavigateToDesign(MouseButtonEventArgs e, string Parameter)
        //{
            //if (e.ClickCount > 1)
            //{
                //ACUrlCommand("BSOVisualisationStudio!DesignOpen", Parameter);
            //}
            //else
            //{
                //DesignOpen(Parameter);
            //}
        //}


        [ACMethodInfo("NavigateToDesign", "", 491)]
        public void DesignOpen(string Parameter)
        {
            ACClassDesign designItem = VisualisationList.FirstOrDefault(x => x.ACIdentifier == Parameter);
            if (designItem != null)
            {
                SelectedVisualisation = designItem;
                CurrentVisualisation = designItem;
            }
        }

        [ACMethodInteraction("", "en{'Set as start diagram'}de{'Als Startbild setzen'}", 400, true, "SelectedVisualisation")]
        public void SetDefaultVisualisation()
        {
            VBUser currentUser = Root.Environment.User;
            VBUserACClassDesign defaultVisualisation = Database.ContextIPlus.VBUserACClassDesign.Where(c => c.VBUserID == currentUser.VBUserID
                                                                                                           && c.ACClassDesign != null
                                                                                                           && c.ACClassDesign.ACUsageIndex == (short)Global.ACUsages.DUVisualisation)
                                                                                                .ToList().FirstOrDefault(x => x.XMLDesign == "" && x.ACIdentifier == "DefaultVisualisation");
            if (defaultVisualisation != null)
                defaultVisualisation.ACClassDesignID = CurrentVisualisation.ACClassDesignID;
            else
            {
                defaultVisualisation = VBUserACClassDesign.NewACObject(Database.ContextIPlus, null);
                defaultVisualisation.VBUserID = currentUser.VBUserID;
                defaultVisualisation.ACClassDesignID = CurrentVisualisation.ACClassDesignID;
                defaultVisualisation.XMLDesign = "";
                defaultVisualisation.ACIdentifier = "DefaultVisualisation";
                Database.ContextIPlus.VBUserACClassDesign.Add(defaultVisualisation);
            }
            Database.ContextIPlus.ACSaveChanges();
        }

        public bool IsEnabledSetDefaultVisualisation()
        {
            if (CurrentVisualisation != null)
                return true;
            return false;
        }

        [ACMethodInfo("ACClass", "en{'Validate input'}de{'Überprüfe Eingabe'}", 492, false)]
        public Msg ValidateInput(string vbContent, object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new Msg() { MessageLevel = eMsgLevel.Info };
            }
            else
            {
                switch (vbContent)
                {
                    case "CurrentNewVisualisation\\ACIdentifier":
                    case "CurrentVisualisation\\ACIdentifier":
                        {
                            String strValue = value as String;
                            if (String.IsNullOrEmpty(strValue))
                                return new Msg() { MessageLevel = eMsgLevel.Info };
                            if (strValue.ContainsACUrlDelimiters() || strValue.Contains(" "))
                            {
                                Msg msg = new Msg { ACIdentifier = this.ACCaption, Message = Root.Environment.TranslateMessage(this, "Warning00002"), MessageLevel = eMsgLevel.Error };
                                return msg;
                            }
                            break;
                        }
                }
            }
            return new Msg() { MessageLevel = eMsgLevel.Info };
        }


        private bool _SwitchingOnAlarm = false;
        private Tuple<string, IACComponent> _CompForSwitchingView = null;
        private string _HighlightParentOnSwitch = "";
        private string _CurrentHighlightedParent = "";
        public void SwitchToViewOnAlarm(Msg msgAlarm)
        {
            _HighlightParentOnSwitch = null;
            _CurrentHighlightedParent = null;

            if (msgAlarm == null || msgAlarm.SourceComponent == null || VBBSOSelectionManager == null)
                return;

            string acUrlToFind = msgAlarm.Source;
            if (msgAlarm.SourceComponent != null && msgAlarm.SourceComponent.ValueT != null)
                acUrlToFind = msgAlarm.SourceComponent.ValueT.GetACUrl();
            if (!IsValidAlarmUrl(acUrlToFind))
                return;

            SwitchToViewOnComponent(msgAlarm.Source, msgAlarm.SourceComponent?.ValueT);
        }

        public void SwitchToViewOnComponent(string acUrl, IACComponent acComponent = null)
        {
            _HighlightParentOnSwitch = null;
            _CurrentHighlightedParent = null;

            if (acUrl == null ||  VBBSOSelectionManager == null)
                return;

            string findPattern = String.Format("VBContent=\"{0}\"", acUrl);
            ACClassDesign foundImage = null;

            // On Proxy-side, WPF-Refs with proxy-objects are stored in ReferencePoint
            IACPointReference<IACObject> refPoint = this.ReferencePoint;
            bool wpfRefs = false;
            if (refPoint != null && acComponent != null)
                wpfRefs = refPoint.HasWPFRefsForComp(acComponent);
            // If wpf-Refs exist, then current visualisation conatins the object
            if (wpfRefs)
            {
                foundImage = CurrentVisualisation;
                if (foundImage != null && !foundImage.XMLDesign.Contains(findPattern))
                    foundImage = null;
            }

            // If no WPF-Refs found: Component is real or CurrentVisualisation doesn't contain it
            if (foundImage == null)
            {
                // Try to find exact url in current visualisation
                if (CurrentVisualisation != null && CurrentVisualisation.XMLDesign.Contains(findPattern))
                    foundImage = CurrentVisualisation;
                else
                {
                    // Does any Image exist with the exact url?
                    ACClassDesign designWithComp = VisualisationList.Where(c => c.XMLDesign.Contains(findPattern)
                                                                        && c.ValueTypeACClassID.HasValue
                                                                        && c.ValueTypeACClass.ACIdentifier == Const.VBDesign_ClassName)
                                                                    .FirstOrDefault();
                    if (designWithComp != null)
                        foundImage = designWithComp;
                    else
                    {
                        designWithComp = VisualisationList.Where(c => c.XMLDesign.Contains(findPattern)
                                                                && (!c.ValueTypeACClassID.HasValue || c.ValueTypeACClass.ACIdentifier != Const.VBDesign_ClassName))
                                                            .FirstOrDefault();
                        if (designWithComp != null)
                            foundImage = designWithComp;
                    }
                    // Resolve Url to parents and search backwards
                    if (foundImage == null)
                    {
                        List<string> resolvedParents = ACUrlHelper.ResolveParents(acUrl, true);
                        if (resolvedParents.Any())
                        {
                            resolvedParents.Reverse();
                            foreach (string parentUrl in resolvedParents)
                            {
                                if (parentUrl == acUrl)
                                    continue;
                                findPattern = String.Format("VBContent=\"{0}\"", parentUrl);
                                if (CurrentVisualisation != null
                                    && CurrentVisualisation.ValueTypeACClassID.HasValue
                                    && CurrentVisualisation.ValueTypeACClass.ACIdentifier == Const.VBDesign_ClassName
                                    && CurrentVisualisation.XMLDesign.Contains(findPattern))
                                {
                                    _HighlightParentOnSwitch = parentUrl;
                                    foundImage = CurrentVisualisation;
                                    break;
                                }
                                else
                                {
                                    designWithComp = VisualisationList.Where(c => c.XMLDesign.Contains(findPattern)
                                                                            && c.ValueTypeACClassID.HasValue
                                                                            && c.ValueTypeACClass.ACIdentifier == Const.VBDesign_ClassName)
                                                                      .FirstOrDefault();
                                    if (designWithComp != null)
                                    {
                                        _HighlightParentOnSwitch = parentUrl;
                                        foundImage = designWithComp;
                                        break;
                                    }
                                    else
                                    {
                                        designWithComp = VisualisationList.Where(c => c.XMLDesign.Contains(findPattern)
                                                                                        && (!c.ValueTypeACClassID.HasValue || c.ValueTypeACClass.ACIdentifier != Const.VBDesign_ClassName))
                                                                        .FirstOrDefault();
                                        if (designWithComp != null)
                                        {
                                            _HighlightParentOnSwitch = parentUrl;
                                            foundImage = designWithComp;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (foundImage != null)
                {
                    // Switch view
                    if (foundImage != CurrentVisualisation)
                    {
                        try
                        {
                            _SwitchingOnAlarm = true;
                            _CompForSwitchingView = new Tuple<string, IACComponent>(acUrl, acComponent);
                            CurrentVisualisation = foundImage;
                            SelectedVisualisation = foundImage;
                        }
                        finally
                        {
                            _SwitchingOnAlarm = false;
                        }
                    }
                    else
                    {
                        BroadcastToVBControls(Const.CmdInitSelectionManager, "CurrentVisualisation", this);
                        VBBSOSelectionManager.HighlightContentACObject(GetComponentToHighlight(acUrl, acComponent), !String.IsNullOrEmpty(_HighlightParentOnSwitch));
                    }
                }
            }
            else
            {
                BroadcastToVBControls(Const.CmdInitSelectionManager, "CurrentVisualisation", this);
                VBBSOSelectionManager.HighlightContentACObject(GetComponentToHighlight(acUrl, acComponent), !String.IsNullOrEmpty(_HighlightParentOnSwitch));
            }
        }

        public IACComponent GetComponentToHighlight(string acUrl, IACComponent acComponent = null)
        {
            if (acComponent != null)
                return acComponent;

            List<string> parents = ACUrlHelper.ResolveParents(acUrl);
            if (!parents.Any())
                return null;
            parents.Reverse();
            foreach (var parent in parents)
            {
                if (parent == acUrl)
                    continue;
                IACComponent parentComp = ACUrlCommand("?" + parent) as IACComponent;
                if (parentComp != null)
                    return parentComp;
            }
            return null;
        }

        public static bool IsValidAlarmUrl(string acUrlToFind)
        {
            var urlHelper = new ACUrlHelper(acUrlToFind);
            return urlHelper.UrlKey == ACUrlHelper.UrlKeys.Root 
                && !String.IsNullOrEmpty(urlHelper.NextACUrl) 
                && !ACUrlHelper.IsUrlDynamicInstance(acUrlToFind);
        }

        [ACMethodInfo("", "en{'Search components'}de{'Suchkomponenten'}", 9999)]
        public void SearchComponents()
        {
            string searchText = SearchText.ToLower();

            SelectedAvailableSearchedItem = null;

            AvailableSearchedItemsList = Database.ContextIPlus.ACClass.Where(c => (c.ACKindIndex == (short)Global.ACKinds.TPAProcessModule || c.ACKindIndex == (short)Global.ACKinds.TPAModule)
                                                                               && c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application
                                                                               && (c.ACIdentifier.ToLower().Contains(searchText) || c.ACCaptionTranslation.ToLower().Contains(searchText) || c.Comment.ToLower().Contains(searchText)))
                                                                      .OrderBy(c => c.ACURLComponentCached)
                                                                      .ToList();

            if (AvailableSearchedItemsList.Count == 1)
            {
                SelectedAvailableSearchedItem = AvailableSearchedItemsList.FirstOrDefault();
                NavigateToComponent();
            }
        }

        public bool IsEnabledSearchComponents()
        {
            return !string.IsNullOrEmpty(SearchText);
        }

        [ACMethodInfo("", "en{'Navigate'}de{'Navigieren'}", 9999)]
        public void NavigateToComponent()
        {
            CloseTopDialog();
            IACComponent component = ACUrlCommand(SelectedAvailableSearchedItem.ACUrlComponent) as IACComponent;
            SwitchToViewOnComponent(SelectedAvailableSearchedItem.ACUrlComponent, component);
        }

        public bool IsEnabledNavigateToComponent()
        {
            return SelectedAvailableSearchedItem != null;
        }

        #endregion

        #region IListener Members
        /// <summary>
        /// The _ message work order method ID
        /// </summary>
        Guid? _MessageWorkOrderMethodID = null;
        /// <summary>
        /// The _ message work order WFID
        /// </summary>
        Guid? _MessageWorkOrderWFID = null;
        /// <summary>
        /// The _ batch no
        /// </summary>
        int _BatchNo = 0;
        /// <summary>
        /// Receives the work order method client request.
        /// </summary>
        /// <param name="WorkOrderMethodID">The work order method ID.</param>
        /// <param name="batchNo">The batch no.</param>
        /// <param name="WorkOrderWFID">The work order WFID.</param>
        /// <param name="message">The message.</param>
        /// <param name="xmlData">The XML data.</param>
        public void ReceiveWorkOrderMethodClientRequest(Guid WorkOrderMethodID, int batchNo, Guid WorkOrderWFID, string message, string xmlData)
        {
            _MessageWorkOrderMethodID = WorkOrderMethodID;
            _BatchNo = batchNo;
            _MessageWorkOrderWFID = WorkOrderWFID;

            switch (Messages.Question(this, "Message00001", Global.MsgResult.Yes, false, message))
            {
                case Global.MsgResult.Yes:
                    MessageYesOrder();
                    break;
                case Global.MsgResult.No:
                    MessageNoOrder();
                    break;
            }
        }

        /// <summary>
        /// Messages the yes order.
        /// </summary>
        public void MessageYesOrder()
        {
            if (_MessageWorkOrderMethodID.HasValue && _MessageWorkOrderWFID.HasValue)
            {
                _MessageWorkOrderMethodID = null;
                _MessageWorkOrderWFID = null;
            }
        }

        /// <summary>
        /// Messages the no order.
        /// </summary>
        public void MessageNoOrder()
        {
            if (_MessageWorkOrderMethodID.HasValue && _MessageWorkOrderWFID.HasValue)
            {
                _MessageWorkOrderMethodID = null;
                _MessageWorkOrderWFID = null;
            }
        }


        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public override object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            if (acUrl == Const.CmdPrintScreenToImage)
            {
                byte[] result = (byte[])acParameter[0];
                if (result != null)
                {
                    CurrentVisualisation.DesignBinary = result;
                    this.ACSaveChanges();
                    OnPropertyChanged("SelectedVisualisation");
                }
                return null;
            }
            else
                return base.ACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Take a screenshot for current visualisation and save it in byte array
        /// </summary>
        [ACMethodInteraction("", "en{'Screenshot'}de{'Bildschirmaufnahme'}", 401, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.Image)]
        public void Screenshot()
        {
            BroadcastToVBControls(Const.CmdPrintScreenToImage, "CurrentVisualisation", this);
            OnPropertyChanged("CurrentVisualisation");
        }

        [ACMethodInteraction("", "en{'Copy screenshot to clipboard'}de{'Bildschirmaufnahme in Zwischenablage kopieren'}", 9999, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.Image)]
        public void CopyVisualisationToClipboard()
        {
            if (!IsEnabledCopyVisualisationToClipboard()) return;
            BroadcastToVBControls(Const.CmdPrintScreenToClipboard, "CurrentVisualisation", this);
            OnPropertyChanged("CurrentVisualisation");
        }

        public bool IsEnabledCopyVisualisationToClipboard()
        {
            return CurrentVisualisation != null;
        }

        [ACMethodInteraction("", "en{'Export designs'}de{'Export designs'}", 402, false, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.Image)]
        public void ExportDesigns()
        {
            if (!IsEnabledExportDesigns()) return;
            ACClassDesign currentVisualisation = CurrentVisualisation;
            foreach (var visualisation in VisualisationList)
            {
                CurrentVisualisation = visualisation;
                string filename = Root.Environment.Datapath + @"\" + CurrentVisualisation.ACIdentifier + ".png";
                BroadcastToVBControls(Const.CmdExportDesignToFile, "CurrentVisualisation", this, filename);
            }
            CurrentVisualisation = currentVisualisation;
            OnPropertyChanged("CurrentVisualisation");
        }

        public bool IsEnabledExportDesigns()
        {
            return VisualisationList != null && VisualisationList.Any();
        }
        #endregion

        /// <summary>
        /// Gets the current design context.
        /// </summary>
        /// <value>The current design context.</value>
        public IACObject CurrentDesignContext
        {
            get { return this.Root; }
        }
    }
}
