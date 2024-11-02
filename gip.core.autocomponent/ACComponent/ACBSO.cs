// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Reflection;
using System.Data;
using System.IO;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Basisklasse für alle Businessobjekte
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Baseclass BSO stateless'}de{'Basisklasse BSO zustandslos'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, true, true)]
    public abstract class ACBSO : ACComponentState, IACBSO, IACComponentTaskSubscr
    {
        #region const

        public const string Const_PrinterPreConfigACUrl = @"Printer";
        public const string DesignNameProgressBar = "Progress";

        #endregion

        #region c´tors
        public ACBSO(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            VerifyLicense();

            _TaskSubscriptionPoint = new ACPointAsyncRMISubscr(this, Const.TaskSubscriptionPoint, 0);

            if (!base.ACInit(startChildMode))
                return false;
            InitBgWorker();
            return true;
        }

        private IACComponent _TempParentACComponent;
        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_ChangeLogBSO != null)
            {
                _ChangeLogBSO.Detach();
                _ChangeLogBSO = null;
            }
            bool hasOneBSOThisContext = false;
            foreach (ACBSO bso in Root.Businessobjects.FindChildComponents<ACBSO>(c => c is ACBSO))
            {
                if (bso != this && bso.Database == this.Database)
                {
                    hasOneBSOThisContext = true;
                    break;
                }
            }
            _TempParentACComponent = ParentACComponent;

            bool result = base.ACDeInit(deleteACClassTask);

            if (InitState == ACInitState.Destructed)
            {
                _TaskSubscriptionPoint = null;
            }


            if (!hasOneBSOThisContext)
            {
                OnDisposeDatabase();
            }
            _TempParentACComponent = null;
            _CurrentProgressInfo = null;
            _TrialExpiredLock = null;
            return result;
        }

        private object _TrialExpiredLock = new object();
        private static bool _TrialExpired = false;
        private static DateTime? _MaxWinSessionChecked;

        private void VerifyLicense()
        {
            if (ParentACComponent != null
                && ParentACComponent.Root != null
                && ParentACComponent.Root.RootPageWPF != null
                && Root.Environment.License.IsTrial && Root.Environment.License.IsTrialTimeExpired)
            {
                if (!_TrialExpired)
                {
                    Messages.Warning(this, "Warning50025");
                    lock (_TrialExpiredLock)
                        _TrialExpired = true;
                }
                throw new Exception("Trial expired");
            }
        }
        private void VerifyMaxSessions()
        {
            if (ParentACComponent != null
                && ParentACComponent.Root != null
                && ParentACComponent.Root.RootPageWPF != null
                && (!_MaxWinSessionChecked.HasValue || (DateTime.Now - _MaxWinSessionChecked.Value).TotalHours >= 4)
                && ParentACComponent is Businessobjects)
            {
                _MaxWinSessionChecked = DateTime.Now;
                if (Root.Environment.IsMaxWinSessionsExceeded)
                {
                    ACComponent childBSO = ACUrlCommand("?BSOiPlusHelp") as ACComponent;
                    if (childBSO == null)
                        childBSO = StartComponent("BSOiPlusHelp", null, new object[] { }) as ACComponent;
                    if (childBSO == null)
                        return;
                    childBSO.ACUrlCommand("!ShowInfo");
                }
            }
        }

        /// <summary>
        /// Every time a business object is closed, the ACDeInit method checks whether there are any other business objects that use the same context. If not, the virtual method OnDisposeDatabase () is called for the last business object, which instructs the ACObjectContextManager to close the database context. If you override this method, don't forget to call the base. Otherwise the context remains open and all objects remain in the change tracker.
        /// </summary>
        protected virtual void OnDisposeDatabase()
        {
            if (_TempParentACComponent != null && _TempParentACComponent.Database != this.Database)
            {
                ACObjectContextManager.DisposeAndRemove(this.Database);
                _Database = null;
            }
        }

        #endregion

        #region IACBSO
        /// <summary>
        /// WPF-Control that register itself and the bounded object (in most cases a ACComponentProxy-Object) to this Reference-Point
        /// </summary>
        /// <param name="hashOfDepObj">Hashcode of the calling WPF-Control</param>
        /// <param name="boundedObject">IACObject which is bound via WPF-Binding to the WPF-Control</param>
        public void AddWPFRef(int hashOfDepObj, IACObject boundedObject)
        {
            if (this.ReferencePoint == null || boundedObject == null)
                return;
            this.ReferencePoint.AddWPFRef(hashOfDepObj, boundedObject);
            OnWPFRefAdded(hashOfDepObj, boundedObject);
        }


        /// <summary>
        /// Called when AddWPFRef added a control to the ReferencePoint
        /// </summary>
        /// <param name="hashOfDepObj">The hash of dep object.</param>
        /// <param name="boundedObject">The bounded object.</param>
        protected virtual void OnWPFRefAdded(int hashOfDepObj, IACObject boundedObject)
        {
        }


        /// <summary>
        /// WPF-Control that removes itself
        /// </summary>
        /// <param name="hashOfDepObj">Hashcode of the calling WPF-Control</param>
        /// <param name="searchInChilds"></param>
        /// <returns>true if WPF-Control was remove from ReferencePoint</returns>
        public bool RemoveWPFRef(int hashOfDepObj, bool searchInChilds = false)
        {
            if (this.ReferencePoint == null)
                return false;
            bool done = this.ReferencePoint.RemoveWPFRef(hashOfDepObj);
            if (!done && searchInChilds && InitState == ACInitState.Initialized)
            {
                foreach (var child in this.ACComponentChilds)
                {
                    IACBSO childBSO = child as IACBSO;
                    if (childBSO != null)
                    {
                        if (childBSO.RemoveWPFRef(hashOfDepObj, true))
                        {
                            OnWPFRefRemoved(hashOfDepObj);
                            return true;
                        }
                    }
                }
            }
            return done;
        }


        /// <summary>
        /// Called when RemoveWPFRef remove a control from the ReferencePoint
        /// </summary>
        /// <param name="hashOfDepObj">The hash of dep object.</param>
        protected virtual void OnWPFRefRemoved(int hashOfDepObj)
        {
        }


        /// <summary>
        /// Creates a new business object instance.
        /// </summary>
        protected object CreateNewBSOInstanceOfThis()
        {
            ACClass acClass = this.ComponentClass;
            if (!acClass.IsMultiInstanceInherited || IsProxy)
                return null;

            ACValueList acValueList = this.Parameters != null ? (ACValueList)this.Parameters.Clone() : acClass.GetACParameter(null);
            if (acValueList == null)
                acValueList = new ACValueList();
            if (acValueList.GetACValue(Const.SkipSearchOnStart) == null)
                acValueList.Add(new ACValue(Const.SkipSearchOnStart, typeof(bool), true));
            return (ParentACComponent as ACComponent).StartComponent(acClass, this.Content, acValueList, Global.ACStartTypes.Automatic, IsProxy);
        }


        /// <summary>
        /// Creates a new business object instance. It doesn't copy members of this instance. You have to do this yourself!
        /// </summary>
        public virtual object Clone()
        {
            return CreateNewBSOInstanceOfThis();
        }


        /// <summary>
        /// Its invoked from a WPF-Itemscontrol that wants to refresh its CollectionView because the user has changed the LINQ-Expressiontree in the ACQueryDefinition-Property of IAccess. 
        /// The BSO should execute the query on the database first, to get the new results for refreshing the CollectionView of the control.
        /// If the bso don't want to handle this request or manipulate the ACQueryDefinition it returns false. The WPF-control invokes then the IAccess.NavSearch()-Method itself.  
        /// </summary>
        /// <param name="acAccess">Reference to IAccess that contains the changed query (Property NavACQueryDefinition)</param>
        /// <returns>True if the bso has handled this request and queried the database context. Otherwise it returns false.</returns>
        public virtual bool ExecuteNavSearch(IAccess acAccess)
        {
            return false;
        }

        #endregion

        #region IACComponentTaskSubscr
        /// <summary>
        /// The _ task subscription point
        /// </summary>
        protected ACPointAsyncRMISubscr _TaskSubscriptionPoint;
        /// <summary>
        /// Gets the task subscription point.
        /// </summary>
        /// <value>The task subscription point.</value>
        [ACPropertyAsyncMethodPointSubscr(9999, false, 0, "TaskCallback")]
        public ACPointAsyncRMISubscr TaskSubscriptionPoint
        {
            get
            {
                return _TaskSubscriptionPoint;
            }
        }

        /// <summary>
        /// Gets the task callback delegate.
        /// </summary>
        /// <value>The task callback delegate.</value>
        public ACPointNetEventDelegate TaskCallbackDelegate
        {
            get
            {
                return TaskCallback;
            }
        }

        [ACMethodInfo("Function", "en{'TaskCallback'}de{'TaskCallback'}", 9999)]
        public virtual void TaskCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            ACUrlCommand("!ScriptTaskCallback", sender, e, wrapObject);
        }
        #endregion

        #region Common-Save-Handling

        #region Motify-Methods to inform all BSO's from the same ObjectContext

        /// <summary>Asks all BSO's that shares this database-context if Saving-Changes is allowed by calling their OnIsEnabledSave()-Method</summary>
        /// <returns>True, if each BSO has agreed.</returns>
        protected bool NotifyAllIsEnabledSave()
        {
            bool isEnabled = true;
            if (!this.OnIsEnabledSave())
                return false;

            foreach (ACBSO bso in Root.Businessobjects.FindChildComponents<ACBSO>(c => c is ACBSO))
            {
                if (bso != this && bso.Database == this.Database)
                {
                    if (!bso.OnIsEnabledSave())
                        isEnabled = false;
                }
            }
            return isEnabled;
        }


        /// <summary>Informs all BSO's about the saving-action that shares this database-context by calling the OnPreSave()-Method</summary>
        /// <returns>A message list with reasons why some BSO's doesn't agree.</returns>
        protected MsgWithDetails NotifyAllPreSave()
        {
            MsgWithDetails resultList = null;
            Msg subResult = this.OnPreSave();
            if (subResult != null)
            {
                resultList = new MsgWithDetails();
                {
                    MsgWithDetails subResultDetails = subResult as MsgWithDetails;
                    resultList.Message = subResult.Message;
                    if (subResultDetails != null)
                        resultList.UpdateFrom(subResultDetails);
                    else
                        resultList.AddDetailMessage(subResult);
                }
                resultList.MessageLevel = (eMsgLevel)resultList.MsgDetails.Max(x => (int)x.MessageLevel); // Max level to message (int enum parsing)
                return resultList;
            }

            foreach (ACBSO bso in Root.Businessobjects.FindChildComponents<ACBSO>(c => c is ACBSO))
            {
                if (bso != this && bso.Database == this.Database)
                {
                    subResult = bso.OnPreSave();
                    if (subResult != null && resultList == null)
                        resultList = new MsgWithDetails();
                    if (subResult != null)
                    {
                        MsgWithDetails subResultDetails = subResult as MsgWithDetails;
                        if (subResultDetails != null)
                            resultList.UpdateFrom(subResultDetails);
                        else
                            resultList.AddDetailMessage(subResult);
                    }
                }
            }
            return resultList;
        }


        /// <summary>Informs all BSO's that the saving of changes was sucessfull.</summary>
        protected void NotifyAllPostSave()
        {
            foreach (ACBSO bso in Root.Businessobjects.FindChildComponents<ACBSO>(c => c is ACBSO))
            {
                if (bso.Database == this.Database)
                {
                    bso.OnPostSave();
                }
            }
        }


        /// <summary>Asks all BSO's that shares this database-context if undoing is allowed by calling their OnIsEnabledUndoSave()-Method</summary>
        /// <returns>True, if each BSO has agreed.</returns>
        protected bool NotifyAllIsEnabledUndoSave()
        {
            bool isEnabled = true;
            if (!this.OnIsEnabledUndoSave())
                return false;

            foreach (ACBSO bso in Root.Businessobjects.FindChildComponents<ACBSO>(c => c is ACBSO))
            {
                if (bso != this && bso.Database == this.Database)
                {
                    if (!bso.OnIsEnabledUndoSave())
                        isEnabled = false;
                }
            }
            return isEnabled;
        }


        /// <summary>Informs all BSO's about the undoing-action that shares this database-context by calling the OnPreUndoSave()-Method</summary>
        /// <returns>A message list with reasons why some BSO's doesn't agree.</returns>
        protected MsgWithDetails NotifyAllPreUndoSave()
        {
            MsgWithDetails resultList = null;
            Msg subResult = this.OnPreUndoSave();
            if (subResult != null)
            {
                resultList = new MsgWithDetails();
                resultList.AddDetailMessage(subResult);
                return resultList;
            }

            foreach (ACBSO bso in Root.Businessobjects.FindChildComponents<ACBSO>(c => c is ACBSO))
            {
                if (bso.Database == this.Database)
                {
                    subResult = bso.OnPreUndoSave();
                    if (subResult != null && resultList == null)
                        resultList = new MsgWithDetails();
                    if (subResult != null)
                        resultList.AddDetailMessage(subResult);
                }
            }
            return resultList;
        }


        /// <summary>Informs all BSO's that the undoing of changes was sucessfull.</summary>
        protected void NotifyAllPostUndoSave()
        {
            foreach (ACBSO bso in Root.Businessobjects.FindChildComponents<ACBSO>(c => c is ACBSO))
            {
                if (bso.Database == this.Database)
                {
                    bso.OnPostUndoSave();
                }
            }
        }


        /// <summary>
        /// This method automatically reloads the data for the current or selected entity-object.
        /// Use this method inside your "Load()"-Method.
        /// This method has three delegates that you use to define how the Selected and Current properties are to be accessed. If you have only defined one property (Current or Selected), then set the first parameter to zero
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="requery">The requery parameter is always false when navigating. When you press the button in the ribbon, it is transferred with  true . Then the query is carried out with the MergeOption "OverwriteChanges" so that already materialized entity objects are updated with fresh data. However, if the database context is in a changed state and the user has refused to save it, the query is only made with "AppendOnly"</param>
        /// <param name="selectedGetter">A delegate for invoking the getter-method of the Selected-Property. e.g. "() => SelectedMaterial"</param>
        /// <param name="currentGetter">A delegate for invoking the getter-method of the Current-Property. e.g. "() => CurrentMaterial"</param>
        /// <param name="currentSetter">A delegate for invoking the setter-method of the Current-Property. e.g. "c => CurrentMaterial = c"</param>
        /// <param name="query">A LINQ-Expression to query the currently selected object. Remember to add additional include statements to avoid lazy loading later and to improve the performance of your application.</param>
        /// <exception cref="ArgumentNullException">
        /// Both delegates selectedGetter and currentGetter are null
        /// or
        /// currentSetter must not be null
        /// </exception>
        public virtual void LoadEntity<TEntity>(bool requery, Func<TEntity> selectedGetter, Func<TEntity> currentGetter, Action<TEntity> currentSetter, IQueryable<TEntity> query) where TEntity : class
        {
            if (selectedGetter == null && currentGetter == null)
                throw new ArgumentNullException("Both delegates selectedGetter and currentGetter are null");
            else if (currentSetter == null)
                throw new ArgumentNullException("currentSetter must not be null");

            // If Businessobject with Selected-Property and Current-Rroperty
            if (selectedGetter != null && currentGetter != null)
            {
                if (selectedGetter() != null
                    && (requery
                        || currentGetter() != selectedGetter())
                   )
                {
                    ACSaveOrUndoChanges();
                    if (requery)
                    {
                        currentSetter(query.AutoMergeOption().FirstOrDefault());

                        // Falls Neu angelegt und nicht gespeichert, dann ist Datenbankabfrage natürlich leer
                        if (currentGetter() == null)
                            currentSetter(selectedGetter());
                    }
                    else
                        currentSetter(selectedGetter());
                }
                else if (selectedGetter() == null)
                    currentSetter(null);
            }
            // Businesobject with either a Selected-Property or a Current-Property
            else
            {
                Func<TEntity> oneGetter = selectedGetter != null ? selectedGetter : currentGetter;
                if (oneGetter() != null)
                {
                    currentSetter(query.AutoMergeOption().FirstOrDefault());
                }
            }
        }

        #endregion


        #region Save and Undo-Methods

        /// <summary>
        /// Initiates the saving of the database-context. First NotifyAllPreSave() is called to inform all BSO's that shares this database-context by calling the OnPreSave()-Method. If all BSO's agree the saving then ACSaveChanges() is called. If ACSaveChanges() was successful, then NotifyAllPostSave() is called to inform all BSO's are informed by calling OnPostSave().
        /// </summary>
        /// <returns>True if changes were saved on the database context.</returns>
        protected virtual bool OnSave()
        {
            VerifyLicense();
            VerifyMaxSessions();

            MsgWithDetails msg = NotifyAllPreSave();
            gip.core.datamodel.Global.MsgResult msgResult = Global.MsgResult.Yes;
            if (msg != null)
            {
                if (((int)msg.MessageLevel) >= ((int)eMsgLevel.Error))
                {
                    msgResult = Messages.Msg(msg);
                    return false;
                }
                msgResult = Messages.Msg(msg, Global.MsgResult.No, eMsgButton.YesNo);
                if (msgResult == Global.MsgResult.No || msgResult == Global.MsgResult.Cancel)
                    return false;
            }
            if (!ACSaveChanges())
                return false;
            NotifyAllPostSave();
            return true;
        }


        /// <summary>
        /// Initiates the undoing of changes on the database-context. First NotifyAllPreUndoSave() is called to inform all BSO's that shares this database-context by calling the OnPreUndoSave()-Method. If all BSO's agree the saving then ACUndoChanges() is called. If ACUndoChanges() was successful, then NotifyAllPostUndoSave() is called to inform all BSO's are informed by calling OnPostUndoSave().
        /// </summary>
        /// <returns></returns>
        protected virtual bool OnUndoSave()
        {
            MsgWithDetails msg = NotifyAllPreUndoSave();
            if (msg != null)
            {
                Messages.Msg(msg);
                return false;
            }
            if (!ACUndoChanges())
                return false;
            NotifyAllPostUndoSave();
            return true;
        }


        /// <summary>When the database context has changed, a dialog is opened that asks the user whether they want to save the changes. If yes then the OnSave()-Method will be invoked to inform all BSO's which uses the same database-context. If not then ACUndoChanges() will be invoked. If cancelled then nothing will happen.</summary>
        /// <returns>Fals, if user has cancelled saving or undoing.</returns>
        public virtual bool ACSaveOrUndoChanges()
        {
            if (Database == null)
                return false;
            if (!Database.IsChanged)
                return true;

            Global.MsgResult result = Messages.YesNoCancel(this, "Question00005");

            try
            {
                switch (result)
                {
                    case Global.MsgResult.Yes:
                        return OnSave();
                    case Global.MsgResult.No:
                        return OnUndoSave();
                    case Global.MsgResult.Cancel:
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("ACBSO", "ACSaveOrUndoChanges", msg);
                return false;
            }
        }

        #endregion


        #region overrideable methods

        /// <summary>Is called from the GUI (Command-Handler of a Button or Ribbon-Button) to enable the Button.</summary>
        /// <returns>True, if database context is changed.</returns>
        protected virtual bool OnIsEnabledUndoSave()
        {
            return Database.IsChanged;
        }

        /// <summary>
        /// Called when a BSO wants to undo its changes that shares the same database-context like this instance. Override this method if you want to prevent the undoing by returning a message with a reason.
        /// </summary>
        /// <returns>Returns a message if this instance doesn't agree to the undoing-request.</returns>
        protected virtual Msg OnPreUndoSave()
        {
            Msg result = PreExecuteMsg("UndoSave");
            return result;
        }

        /// <summary>Called when when undoing changes was sucessfull.</summary>
        protected virtual void OnPostUndoSave()
        {
            PostExecute("UndoSave");
        }

        /// <summary>Is called from the GUI (Command-Handler of a Button or Ribbon-Button) to enable the Button.</summary>
        /// <returns>True, if database context is changed.</returns>
        protected virtual bool OnIsEnabledSave()
        {
            return Database.IsChanged;
        }


        /// <summary>
        /// Called when a BSO wants to save its changes that shares the same database-context like this instance. Override this method if you want to prevent the saving by returning a message with a reason.
        /// </summary>
        /// <returns>Returns a message if this instance doesn't agree to the saving-request.</returns>
        protected virtual Msg OnPreSave()
        {
            Msg result = PreExecuteMsg("Save");
            return result;
        }

        /// <summary>Called when when saving changes was sucessfull.</summary>
        protected virtual void OnPostSave()
        {
            PostExecute("Save");
        }
        #endregion

        #endregion

        #region Printing

        /// <summary>
        /// Opens the dialog for printing.
        /// </summary>
        [ACMethodCommand("Query", "en{'Print'}de{'Drucken'}", (short)MISort.QueryPrintDlg)]
        public void QueryPrintDlg()
        {
            ACUrlCommand("VBBSOReportDialog!ReportPrintDlg");
        }

        public bool IsEnabledQueryPrintDlg()
        {
            object result = ACUrlCommand("VBBSOReportDialog!IsEnabledReportPrintDlg");
            if (result == null)
                return false;
            return (bool)result;
        }


        /// <summary>
        /// Opens the dialog for previewing the printing result.
        /// </summary>
        [ACMethodCommand("Query", "en{'Preview'}de{'Vorschau'}", (short)MISort.QueryPreviewDlg)]
        public void QueryPreviewDlg()
        {
            ACUrlCommand("VBBSOReportDialog!ReportPreviewDlg");
        }

        public bool IsEnabledQueryPreviewDlg()
        {
            object result = ACUrlCommand("VBBSOReportDialog!IsEnabledReportPreviewDlg");
            if (result == null)
                return false;
            return (bool)result;
        }


        /// <summary>
        /// Opens the window for managing reports for printing.
        /// </summary>
        [ACMethodCommand("Query", "en{'Design'}de{'Entwurf'}", (short)MISort.QueryDesignDlg)]
        public void QueryDesignDlg()
        {
            ACUrlCommand("VBBSOReportDialog!ReportDesignDlg");
        }

        public bool IsEnabledQueryDesignDlg()
        {
            object result = ACUrlCommand("VBBSOReportDialog!IsEnabledReportDesignDlg");
            if (result == null)
                return false;
            return (bool)result;
        }


        protected ACClassDesign _CurrentQueryACClassDesign = null;
        [ACPropertyCurrent(9999, "", "en{'Current Report'}de{'Aktueller Bericht'}")]
        public virtual ACClassDesign CurrentQueryACClassDesign
        {
            get
            {
                return _CurrentQueryACClassDesign;
            }
            set
            {
                _CurrentQueryACClassDesign = value;
                OnPropertyChanged("CurrentQueryACClassDesign");
            }
        }

        /// <summary>
        /// Override in subclass to filter the Main Entity
        /// </summary>
        /// <param name="paOrderInfo"></param>
        /// <returns></returns>
        public virtual Msg FilterByOrderInfo(PAOrderInfo paOrderInfo)
        {
            return null;
        }

        public virtual PAOrderInfo GetOrderInfo()
        {
            return null;
        }

        /// <summary>
        /// Prints a default report via windows print driver in background
        /// </summary>
        /// <param name="paOrderInfo">Order Info</param>
        /// <param name="designName">Name of the design.</param>
        /// <param name="printerName">Name of the printer.</param>
        /// <param name="numberOfCopies">The number of copies.</param>
        /// <param name="maxPrintJobsInSpooler">Max Print Jobs in Queue</param>
        /// <param name="preventClone">Prevent generating a clone for the BSO</param>
        /// <returns></returns>
        public virtual Msg PrintByOrderInfo(PAOrderInfo paOrderInfo, string printerName, short numberOfCopies, string designName = null, int maxPrintJobsInSpooler = 0, bool preventClone = true)
        {
            var vbDump = Root.VBDump;

            PerformanceEvent pEvent = vbDump?.PerfLoggerStart(this.GetACUrl() + "!" + nameof(FilterByOrderInfo), 120);
            Msg msg = FilterByOrderInfo(paOrderInfo);
            vbDump?.PerfLoggerStop(this.GetACUrl() + "!" + nameof(FilterByOrderInfo), 120, pEvent);
            //if (EnablePrintLogging)
            //{
            //    string msgLog = pEvent.InstanceName + " " + pEvent.Elapsed + paOrderInfo != null ? paOrderInfo.ToString() : "";
            //    Messages.LogMessageMsg(new Msg(msgLog, this, eMsgLevel.Info, nameof(ACBSO), nameof(PrintByOrderInfo), 735));
            //}

            if (msg != null)
                return msg;

            //pEvent = vbDump?.PerfLoggerStart(this.GetACUrl() + "!" + nameof(GetDesignForPrinting), 130);
            ACClassDesign printDesign = GetDesignForPrinting(printerName, designName, paOrderInfo);
            //vbDump?.PerfLoggerStop(this.GetACUrl() + "!" + nameof(GetDesignForPrinting), 130, pEvent);
            //if (EnablePrintLogging)
            //{
            //    string msgLog = pEvent.InstanceName + " " + pEvent.Elapsed + paOrderInfo != null ? paOrderInfo.ToString() : "";
            //    Messages.LogMessageMsg(new Msg(msgLog, this, eMsgLevel.Info, nameof(ACBSO), nameof(PrintByOrderInfo), 746));
            //}

            if (printDesign == null)
            {
                // TODO Translate
                Messages.Error(this, string.Format(@"Report {0} doesn't exist!", designName));
                return new Msg();
            }
            msg = PrintDesign(printDesign, printerName, numberOfCopies, false, maxPrintJobsInSpooler: maxPrintJobsInSpooler, preventClone: preventClone);
            return msg;
        }

        /// <summary>
        /// Returns the first matching report (design) for the passed printer
        /// </summary>
        /// <param name="printerName"></param>
        /// <param name="designName"></param>
        /// <param name="paOrderInfo"></param>
        /// <returns></returns>
        public ACClassDesign GetDesignForPrinting(string printerName, string designName = null, PAOrderInfo paOrderInfo = null)
        {
            ACClassDesign printDesign = null;
            if (!String.IsNullOrEmpty(designName))
                printDesign = GetDesign(designName);
            else
            {
                var result = OnGetDefaultPrintDesigns(printerName, paOrderInfo);
                if (result != null)
                    printDesign = result.FirstOrDefault();
            }
            return printDesign;
        }

        /// <summary>
        /// Method for option implementation in Subclass to determine which design is to be printed depending on the passed printer and order info
        /// </summary>
        /// <param name="printerName"></param>
        /// <param name="paOrderInfo"></param>
        /// <returns></returns>
        protected virtual IEnumerable<ACClassDesign> OnGetDefaultPrintDesigns(string printerName, PAOrderInfo paOrderInfo = null)
        {
            List<ACClassDesign> list = this.ComponentClass
                        .GetDesigns()
                        .Where(c => c.ACKind == Global.ACKinds.DSDesignReport)
                        .OrderByDescending(c => c.SortIndex)
                        .ToList();

            var query = this.ComponentClass.ConfigurationEntriesInClassHierarchy.Where(c =>    c.LocalConfigACUrl == Const_PrinterPreConfigACUrl 
                                                                            && (c.Value as string) == printerName);
            if (query != null && query.Any())
            {
                List<ACClassDesign> filteredList = new List<ACClassDesign>();
                foreach (ACClassDesign design in list)
                {
                    if (query.Where(c => c.KeyACUrl == design.ACConfigKeyACUrl).Any())
                        filteredList.Add(design);
                }
                return filteredList;
            }
            else
                return list;
        }

        #endregion

        #region DataExport

        private string _DataExportFilePath;
        [ACPropertyInfo(9999, "DataExportFilePath", "en{'Export file'}de{'Export datei'}")]
        public string DataExportFilePath
        {
            get
            {
                return _DataExportFilePath;
            }
            set
            {
                if (_DataExportFilePath != value)
                {
                    _DataExportFilePath = value;
                    OnPropertyChanged("DataExportFilePath");
                }
            }
        }


        /// <summary>
        /// Opens the dialog for exporting data.
        /// </summary>
        [ACMethodCommand("Query", "en{'Export'}de{'Export'}", (short)MISort.QueryDesignDlg, true)]
        public virtual void DataExportDialog()
        {
            DataExportFilePath = DataExportGenerateFileName();
            ShowDialog(this, "DataExportDlg");
        }

        public virtual bool IsEnabledDataExportDialog()
        {
            return false;
        }


        /// <summary>
        /// Excecutes the Export-Logic when user has confirmed with the OK-Button.
        /// </summary>
        [ACMethodInfo("Query", Const.Ok, (short)MISort.QueryDesignDlg, true)]
        public virtual void DataExportOk()
        {
            if (!IsEnabledDataExportOk())
                return;
            CloseTopDialog();
            BackgroundWorker.RunWorkerAsync("DataExport");
            ShowDialog(this, DesignNameProgressBar);
        }

        public virtual bool IsEnabledDataExportOk()
        {
            return IsEnabledDataExportDialog();
        }


        /// <summary>
        /// Cancels the export and closes the Dialog for exporting.
        /// </summary>
        [ACMethodInfo("Query", Const.Cancel, (short)MISort.QueryDesignDlg, true)]
        public virtual void DataExportCancel()
        {
            CloseTopDialog();
        }

        public virtual bool IsEnabledDataExport()
        {
            return false;
        }


        private bool DataExportFilderPathExist(string DataExportFilePath)
        {
            int indexSlash = DataExportFilePath.LastIndexOf("\\");
            return Directory.Exists(DataExportFilePath.Substring(0, indexSlash)) && DataExportFilePath.EndsWith(".xml");
        }

        public virtual string DataExportGenerateFileName()
        {
            return string.Format(Root.Environment.Datapath + @"\{0}_{1}.xml", ACIdentifier, DateTime.Now.ToString("yyyy-MM-dd_HH-mm"));
        }
        #endregion

        #region Background worker

        ACBackgroundWorker _BackgroundWorker;
        ProgressInfo _CurrentProgressInfo;

        /// <summary>
        /// You can use this instance to start an asynchronous task by calling the RunWorkerAsync(object argument) method . Pass the name of the method that is to be executed asynchronously as a parameter.
        /// </summary>
        /// <value>
        /// The background worker.
        /// </value>
        public ACBackgroundWorker BackgroundWorker
        {
            get
            {
                return _BackgroundWorker;
            }
        }

        /// <summary>
        /// Use ProgressInfo to inform the user about the progress state of the current task that is running in the BackgroundWorker.
        /// </summary>
        /// <value>
        /// The current progress information.
        /// </value>
        [ACPropertyCurrent(9999, "ProgressInfo")]
        public ProgressInfo CurrentProgressInfo
        {
            get
            {
                return _CurrentProgressInfo;
            }
        }


        #region Background worker -> Event handlers

        /// <summary>
        /// Cancels the current asynchronous task in the BackgroundWorker
        /// </summary>
        [ACMethodCommand("CancelBackgroundWorker", Const.Cancel, 9999, false, Global.ACKinds.MSMethodPrePost)]
        public void CancelBackgroundWorker()
        {
            if (!BackgroundWorker.IsBusy) return;
            BackgroundWorker.CancelAsync();
            CloseTopDialog();
        }



        /// <summary>
        /// When the asynchronous task is started via BackgroundWorker.RunWorkerAsync(), this virtual method is called because it is connected as a callback method in the ACBSO class to the DoWork event of the background worker.
        /// If you override this method, call base.BgWorkerDoWork() first!
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        public virtual void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            worker.EventArgs = e;
        }

        /// <summary>
        /// After your asynchronous task has been executed, this virtual method is called because it is connected to the RunWorkerCompleted event of the background worker as a callback method in the ACBSO class.
        /// If you override this method, call base.BgWorkerCompleted() first!
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        public virtual void BgWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Falls Exception geworfen wurde
            if (e.Error != null)
            {
            }
            // Falls Hintergrundprozess vorzeitig vom Benutzer abgebrochen worden ist
            else if (e.Cancelled)
            {
            }
            // Sonst erfolgreicher Durchlauf
            else
            {
            }
            // Lösche Balken
            if(CurrentProgressInfo != null)
            {
                CurrentProgressInfo.Complete();
            }
            if(BackgroundWorker != null && BackgroundWorker.ProgressInfo != null)
            {
                BackgroundWorker.ProgressInfo.Complete();
            }
            CloseTopDialog();
        }

        /// <summary>
        /// Every time when CurrentProgressInfo has changed, then this method is invoked. 
        /// If you override this method, call base.BgWorkerProgressChanged() first!
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ProgressChangedEventArgs"/> instance containing the event data.</param>
        public virtual void BgWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null && e.UserState is Msg)
            {

            }
            else
            {
                // Hole Backgroundworker, der das Event ausgelöst hat
                ProgressInfo info = ACBackgroundWorker.GetProgressInfo(e);
                // Aktualisiere Inhalt der ProgressInfo-Entität zur Aktualisierung Oberfläche
                if (CurrentProgressInfo != null && info != null)
                    CurrentProgressInfo.CloneProgressInfo(info);
            }
        }

        /// <summary>
        /// Inits the BackgroundWorker
        /// </summary>
        protected virtual void InitBgWorker()
        {
            _CurrentProgressInfo = new ProgressInfo(null);
            _BackgroundWorker = new ACBackgroundWorker();
            _BackgroundWorker.WorkerReportsProgress = true;
            _BackgroundWorker.WorkerSupportsCancellation = true;
            _BackgroundWorker.DoWork += new DoWorkEventHandler(BgWorkerDoWork);
            _BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BgWorkerCompleted);
            _BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BgWorkerProgressChanged);
        }

        #endregion

        #endregion

        #region ChangeLog

        /// <summary>
        /// A ACMenuItem contains a ACUrl of the Method that should be invoked.
        /// GetMenu() is called from gip.core.autocomponent.MenuManager.GetMenu()-Method.
        /// The MenuManager calls GetMenu() at all instances that implement IACMenuBuilder and which have a relationship inside the MVVM-Pattern.
        /// All ACMenuItemList's are afterwards merged together to one menu that is displayed as a contextmenu on the GUI.
        /// </summary>
        /// <param name="vbContent">VBContent of the WPF-Control where the user has requested the menu first</param>
        /// <param name="vbControl">Type.FullName of the WPF-Control where the user has requested the menu first</param>
        /// <returns>List of menu entries</returns>
        public override ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            var menuList = base.GetMenu(vbContent, vbControl);

            ACMenuItem menuItemEntity = CreateMenuItemForEntity();
            if (menuItemEntity != null)
                menuList.Add(menuItemEntity);

            ACMenuItem menuItemProperty = CreateMenuItemForProperty(vbContent);
            if (menuItemProperty != null)
                menuList.Add(menuItemProperty);

            return menuList;
        }

        private ACMenuItem CreateMenuItemForEntity()
        {
            ACGenericObject accessPrimary = this.GetValue("AccessPrimary") as ACGenericObject;
            if (accessPrimary == null)
                return null;

            PropertyInfo currentProperty = accessPrimary.GetType().GetProperty("CurrentNavObject");
            if (currentProperty == null)
                return null;

            object current = currentProperty.GetValue(accessPrimary);
            if (current == null || !(current is IACObject) || !(current is VBEntityObject)
                || ((IACObject)current).ACType == null || ((VBEntityObject)current).EntityKey == null || ((VBEntityObject)current).EntityKey.EntityKeyValues == null)
                return null;

            if (ChangeLogBSO == null)
                return null;

            ACValueList param = new ACValueList();
            param.Add(new ACValue("ACClassID", ((((IACObject)current).ACType as ACClass).ACClassID)));
            param.Add(new ACValue("EntityKey", ((VBEntityObject)current).EntityKey.EntityKeyValues[0].Value));
            bool? isEnabled = ChangeLogBSO.ExecuteMethod("IsEnabledLogForClass", param[0].Value, param[1].Value) as bool?;
            if (!isEnabled.HasValue || !isEnabled.Value)
                return null;

            param.Add(new ACValue("Database", ((VBEntityObject)current).GetObjectContext()));

            var acclassText = this.ComponentClass.GetText("ChangeLogMenuText");
            string menuCaption;

            if (acclassText != null)
                menuCaption = string.Format(acclassText.ACCaption, ((IACObject)current).ACIdentifier);
            else
                menuCaption = string.Format("Show change log for {0}", ((IACObject)current).ACIdentifier);

            return new ACMenuItem(null, menuCaption, ChangeLogBSO.GetACUrl() + ACUrlHelper.Delimiter_InvokeMethod + "ShowChangeLogForClass", 200, param);
        }

        private ACMenuItem CreateMenuItemForProperty(string vbContent)
        {
            if (string.IsNullOrEmpty(vbContent) || vbContent.StartsWith("*"))
                return null;

            string entityACUrl = "";
            string propertyACUrl = vbContent;
            object targetEntity = this;

            if (vbContent.Contains("\\"))
            {
                int indexOfLastDelim = vbContent.LastIndexOf('\\');
                entityACUrl = vbContent.Substring(0, indexOfLastDelim);
                propertyACUrl = vbContent.Substring(indexOfLastDelim + 1);
                targetEntity = ACUrlCommand(entityACUrl);
            }

            if (targetEntity == null || !(targetEntity is IACObject) || !(targetEntity is VBEntityObject))
                return null;

            ACClass typeAsACClass = (targetEntity as IACObject).ACType as ACClass;
            ACClassProperty acClassProperty = typeAsACClass == null ? null : typeAsACClass.GetProperty(propertyACUrl);
            if (acClassProperty == null)
                return null;

            EntityKey entityKey = ((VBEntityObject)targetEntity).EntityKey;
            if (entityKey == null || entityKey.EntityKeyValues == null || !entityKey.EntityKeyValues.Any())
                return null;

            ACValueList param = new ACValueList();
            param.Add(new ACValue("EntityKey", entityKey.EntityKeyValues[0].Value));
            param.Add(new ACValue(ACClassProperty.ClassName + "ID", acClassProperty.ACClassPropertyID));

            bool? isEnabled = ACUrlCommand(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_DirSeperator + "BSOChangeLog" + ACUrlHelper.Delimiter_InvokeMethod + "IsEnabledLogForProperty", param[0].Value, param[1].Value) as bool?;
            if (!isEnabled.HasValue || !isEnabled.Value)
                return null;

            var acclassText = this.ComponentClass.GetText("ChangeLogMenuText");
            string menuCaption;

            if (acclassText != null)
                menuCaption = string.Format(acclassText.ACCaption, acClassProperty.ACCaption);
            else
                menuCaption = string.Format("Show change log for {0}", acClassProperty.ACCaption);

            return new ACMenuItem(null, menuCaption, Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_DirSeperator + "BSOChangeLog" + ACUrlHelper.Delimiter_InvokeMethod + "ShowChangeLogForProperty", 200, param);
        }

        private ACRef<ACBSO> _ChangeLogBSO;
        protected ACBSO ChangeLogBSO
        {
            get
            {
                if (_ChangeLogBSO != null)
                    return _ChangeLogBSO.ValueT;
                ACBSO changeLog = Root.Businessobjects.ACComponentChilds.Where(c => c.ACIdentifier.StartsWith("BSOChangeLog")).FirstOrDefault() as ACBSO;
                if (changeLog == null)
                    changeLog = Root.Businessobjects.StartComponent("BSOChangeLog", null, null, ACStartCompOptions.Default) as ACBSO;
                if (changeLog != null)
                    _ChangeLogBSO = new ACRef<ACBSO>(changeLog, this, false);
                if (_ChangeLogBSO != null)
                    return _ChangeLogBSO.ValueT;
                return null;
            }
        }

        #endregion

        #region ShowVBContent

        [ACMethodInteraction("", "en{'Show/Hide content info'}de{'Inhaltsinfo anzeigen/ausblenden'}", 9999, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.Utilities)]
        public virtual void ShowHideVBContentInfo()
        {
            BroadcastToVBControls(Const.CmdShowHideVBContentInfo, null, new object[] { this });
        }

        public bool IsEnabledShowHideVBContentInfo()
        {
            return true;
        }
        #endregion

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(TaskCallback):
                    TaskCallback(acParameter[0] as IACPointNetBase, acParameter[1] as ACEventArgs, acParameter[2] as IACObject);
                    return true;
                case nameof(QueryPrintDlg):
                    QueryPrintDlg();
                    return true;
                case nameof(QueryDesignDlg):
                    QueryDesignDlg();
                    return true;
                case nameof(QueryPreviewDlg):
                    QueryPreviewDlg();
                    return true;
                case nameof(CancelBackgroundWorker):
                    CancelBackgroundWorker();
                    return true;
                case nameof(DataExportDialog):
                    DataExportDialog();
                    return true;
                case nameof(DataExportOk):
                    DataExportOk();
                    return true;
                case nameof(DataExportCancel):
                    DataExportCancel();
                    return true;
                case nameof(IsEnabledQueryPrintDlg):
                    result = IsEnabledQueryPrintDlg();
                    return true;
                case nameof(IsEnabledQueryDesignDlg):
                    result = IsEnabledQueryDesignDlg();
                    return true;
                case nameof(IsEnabledQueryPreviewDlg):
                    result = IsEnabledQueryPreviewDlg();
                    return true;
                case nameof(IsEnabledDataExportDialog):
                    result = IsEnabledDataExportDialog();
                    return true;
                case nameof(IsEnabledDataExportOk):
                    result = IsEnabledDataExportOk();
                    return true;
                case nameof(ShowHideVBContentInfo):
                    ShowHideVBContentInfo();
                    return true;
                case nameof(IsEnabledShowHideVBContentInfo):
                    result = IsEnabledShowHideVBContentInfo();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

    }
}
