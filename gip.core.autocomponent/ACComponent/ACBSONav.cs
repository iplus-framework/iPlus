using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.IO;

namespace gip.core.autocomponent
{
    /// <summary>
    /// The ACBSONav class is an abstract base class for business objects with navigation functionality in the iPlus Framework. 
    /// It extends the ACBSO (AC Business Object) class and provides standardized navigation, state management, and CRUD operations for data-driven applications.
    /// Purpose and Core Functionality:
    /// This class serves as the foundation for business objects that need to navigate through data records, handle data filtering and searching, provide standardized delete operations and manage queries and reports.
    /// Navigation Controls:
    /// The class provides comprehensive navigation methods for moving through data records.
    /// The NavigateFirst method moves to the first record in the dataset, NavigatePrev navigates to the previous record, NavigateNext moves to the next record, and NavigateLast jumps to the last record.
    /// Each navigation method has a corresponding IsEnabled method that should be checked before execution to ensure the operation is valid in the current context.
    /// Search and Filtering Operations:
    /// Search functionality is implemented through the SearchWord property which allows setting text-based filtering criteria for the dataset. 
    /// The NavTakeCount property controls the maximum number of records to retrieve, enabling pagination and performance optimization for large datasets. 
    /// The class supports auto-filter capability through constructor parameters, allowing automatic search execution during initialization.
    /// To perform search operations, set the SearchWord property with the desired search criteria, optionally set NavTakeCount to limit results, then execute the search by calling Search-Method in the derived class or using ACUrlCommand with the search constant.
    /// The search will filter the navigation dataset based on the specified criteria.
    /// Method Invocation Order for MCP:
    /// For navigation operations, always check the enabled state using IsEnabledNavigateFirst, IsEnabledNavigatePrev, IsEnabledNavigateNext, or IsEnabledNavigateLast before executing the corresponding navigation method.
    /// This ensures the UI remains consistent and prevents invalid operations.
    /// For search operations, set search parameters by assigning values to SearchWord and NavTakeCount properties, then trigger the search by calling the Search method in the derived (implementing) class or using the search command through ACUrlCommand.
    /// Key Properties for MCP Access:
    /// SearchWord allows getting and setting search filter text for dataset filtering. 
    /// NavTakeCount controls the maximum number of records to retrieve.
    /// Once the user has completed the task and the value have been changed, call the Save method in the derived class to save the changes to the database. 
    /// Call Save whenever changes have been made and before navigating to another record or when a Delete-Method was called in the derived class.
    /// Implementation Requirements:
    /// Derived classes must implement the abstract AccessNav property which returns an IAccessNav instance.
    /// This property provides access to the underlying navigation functionality and dataset management.
    /// The AccessNav instance should be created as an ACAccessNav typed to the specific entity class being managed.
    /// Visit the https://github.com/search?q=org%3Aiplus-framework+ACBSONav&amp;type=code on github to read the source code and get a full understanding, or use the github MCP API and search for the class name.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, @"en{'Baseclass for navigatable Businessobjects'}de{'Basisklasse für navigierbare Geschäftsobjekte'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, true, true, 
        Description = @"The ACBSONav class is an abstract base class for business objects with navigation functionality in the iPlus Framework. 
                        It extends the ACBSO (AC Business Object) class and provides standardized navigation, state management, and CRUD operations for data-driven applications.
                        Purpose and Core Functionality:
                        This class serves as the foundation for business objects that need to navigate through data records, handle data filtering and searching, provide standardized delete operations and manage queries and reports.
                        Navigation Controls:
                        The class provides comprehensive navigation methods for moving through data records.
                        The NavigateFirst method moves to the first record in the dataset, NavigatePrev navigates to the previous record, NavigateNext moves to the next record, and NavigateLast jumps to the last record.
                        Each navigation method has a corresponding IsEnabled method that should be checked before execution to ensure the operation is valid in the current context.
                        Search and Filtering Operations:
                        Search functionality is implemented through the SearchWord property which allows setting text-based filtering criteria for the dataset. 
                        The NavTakeCount property controls the maximum number of records to retrieve, enabling pagination and performance optimization for large datasets. 
                        The class supports auto-filter capability through constructor parameters, allowing automatic search execution during initialization.
                        To perform search operations, set the SearchWord property with the desired search criteria, optionally set NavTakeCount to limit results, then execute the search by calling Search-Method in the derived class or using ACUrlCommand with the search constant.
                        The search will filter the navigation dataset based on the specified criteria.
                        Method Invocation Order for MCP:
                        For navigation operations, always check the enabled state using IsEnabledNavigateFirst, IsEnabledNavigatePrev, IsEnabledNavigateNext, or IsEnabledNavigateLast before executing the corresponding navigation method.
                        This ensures the UI remains consistent and prevents invalid operations.
                        For search operations, set search parameters by assigning values to SearchWord and NavTakeCount properties, then trigger the search by calling the Search method in the derived (implementing) class or using the search command through ACUrlCommand.
                        Key Properties for MCP Access:
                        SearchWord allows getting and setting search filter text for dataset filtering. 
                        NavTakeCount controls the maximum number of records to retrieve.
                        Once the user has completed the task and the value have been changed, call the Save method in the derived class to save the changes to the database. 
                        Do this whenever changes have been made and before navigating to another record.
                        Implementation Requirements:
                        Derived classes must implement the abstract AccessNav property which returns an IAccessNav instance.
                        This property provides access to the underlying navigation functionality and dataset management.
                        The AccessNav instance should be created as an ACAccessNav typed to the specific entity class being managed.
                        Visit the https://github.com/search?q=org%3Aiplus-framework+ACBSONav&type=code on github to read the source code and get a full understanding, or use the github MCP API and search for the class name.
        ")]
    [ACClassConstructorInfo(
        new object[]
        {
            new object[] {"AutoFilter", Global.ParamOption.Optional, typeof(String)}
        }
    )]
    public abstract class ACBSONav : ACBSO
    {
        #region c´tors
        public ACBSONav(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACPostInit()
        {
            if (!base.ACPostInit())
                return false;

            string autoFilter = ParameterValue("AutoFilter") as string;
            if (!string.IsNullOrEmpty(autoFilter))
            {
                //IAccess access = this.ACUrlCommand("AccessPrimary") as IAccess;
                if (AccessNav != null)
                {
                    AccessNav.NavACQueryDefinition.SearchWord = autoFilter;
                    ACUrlCommand(Const.CmdSearch);
                }
            }
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (AutoSaveOnDeInit)
                ACSaveOrUndoChanges();
            return base.ACDeInit(deleteACClassTask);
        }


        /// <summary>
        /// Creates a new business object instance. 
        /// It copies the current filters from AccessNav.NavACQueryDefinition and calls the Search-Method afterwards.
        /// It doesn't copy other members of this instance. You have to do this yourself!
        /// </summary>
        public override object Clone()
        {
            ACBSONav clone = base.Clone() as ACBSONav;
            if (clone == null)
                return null;
            if (clone.AccessNav != null)
            {
                clone.AccessNav.NavACQueryDefinition.CopyFrom(this.AccessNav.NavACQueryDefinition, true, true, true);
                clone.ACUrlCommand(Const.CmdSearch);
            }
            return clone;
        }

        /// <summary>
        /// Gets a value indicating whether ACSaveChanges should be called when the Change-Tracker has changed entities and this businessobject should be shut down.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic save on deinitialize]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool AutoSaveOnDeInit
        {
            get
            {
                return true;
            }
        }

        public const string CDialogSoftDelete = "DialogSoftDelete";
        public const string CDialogUnDelete = "DialogUnDelete";
        #endregion

        #region ACState
        [ACMethodState("en{'Callback method on state change to SMReadOnly'}de{'Rückrufmethode bei Statusänderung SMReadOnly'}", 1000)]
        public virtual void SMReadOnly()
        {
            if (!PreExecute(Const.SMReadOnly))
                return;
            PostExecute(Const.SMReadOnly);
        }

        [ACMethodState("en{'Callback method on state change to SMNew'}de{'Rückrufmethode bei Statusänderung SMNew'}", 1010)]
        public virtual void SMNew()
        {
            if (!PreExecute(Const.SMNew))
                return;
            PostExecute(Const.SMNew);
        }

        [ACMethodState("en{'Callback method on state change to SMEdit'}de{'Rückrufmethode bei Statusänderung SMEdit'}", 1020)]
        public virtual void SMEdit()
        {
            if (!PreExecute(Const.SMEdit))
                return;
            PostExecute(Const.SMEdit);
        }

        [ACMethodState("en{'Callback method on state change to SMSearch'}de{'Rückrufmethode bei Statusänderung SMSearch'}", 1030)]
        public virtual void SMSearch()
        {
            if (!PreExecute(Const.SMSearch))
                return;
            PostExecute(Const.SMSearch);
        }

        /// <summary>Called inside the GetControlModes-Method to get the Global.ControlModes from derivations.
        /// This method should be overriden in the derivations to dynmically control the presentation mode depending on the current value which is bound via VBContent</summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            if (vbControl == null)
                return base.OnGetControlModes(vbControl);
            if (String.IsNullOrEmpty(vbControl.DisabledModes))
                return base.OnGetControlModes(vbControl);
            return vbControl.DisabledModes.IndexOf(ACState.ToString()) == -1 ? Global.ControlModes.Enabled : Global.ControlModes.Disabled;
        }

        #endregion

        #region Printing

        [ACPropertyInfo(9999, "Query", "en{'Design'}de{'Entwurf'}")]
        public bool HasQuerys
        {
            get
            {
                return this.NavigationqueryList.Any();
            }
        }

        public override ACClassDesign CurrentQueryACClassDesign
        {
            get
            {
                if (_CurrentQueryACClassDesign != null)
                    return _CurrentQueryACClassDesign;
                var query = NavigationqueryList;
                foreach (ACClassConfig acClassConfig in query)
                {
                    ACComposition acComposition = acClassConfig[Const.Value] as ACComposition;
                    if (acComposition == null)
                        continue;
                    ACClass acClass = acComposition.GetComposition(Database.ContextIPlus) as ACClass;
                    if (acClass != null)
                        _CurrentQueryACClassDesign = acClass.Designs.Where(c => c.ACKindIndex == (Int16)Global.ACKinds.DSDesignReport).OrderBy(c => c.SortIndex).FirstOrDefault();
                }
                return _CurrentQueryACClassDesign;
            }
            set
            {
                _CurrentQueryACClassDesign = value;
                OnPropertyChanged("CurrentQueryACClassDesign");
            }
        }
        #endregion

        #region Attached from Navigation
        [ACMethodCommand("Navigation", "en{'Navigate to the first record'}de{'Zum ersten Datensatz navigieren'}", (short)MISort.NavigateFirst)]
        public void NavigateFirst()
        {
            AccessNav?.NavigateFirst();
        }


        public bool IsEnabledNavigateFirst()
        {
            return AccessNav?.IsEnabledNavigateFirst() ?? false;
        }


        [ACMethodCommand("Navigate to the previous record", "en{'Previous'}de{'Zum vorherigen Datensatz navigieren'}", (short)MISort.NavigatePrev)]
        public void NavigatePrev()
        {
            AccessNav?.NavigatePrev();
        }


        public bool IsEnabledNavigatePrev()
        {
            return AccessNav?.IsEnabledNavigatePrev() ?? false;
        }


        [ACMethodCommand("Navigation", "en{'Navigate to the next record'}de{'Zum nächsten Datensatz navigieren'}", (short)MISort.NavigateNext)]
        public void NavigateNext()
        {
            AccessNav?.NavigateNext();
        }


        public bool IsEnabledNavigateNext()
        {
            return AccessNav?.IsEnabledNavigateNext() ?? false;
        }


        [ACMethodCommand("Navigation", "en{'Navigate to the last record'}de{'Zum letzten Datensatz navigieren'}", (short)MISort.NavigateLast)]
        public void NavigateLast()
        {
            AccessNav?.NavigateLast();
        }


        public bool IsEnabledNavigateLast()
        {
            return AccessNav?.IsEnabledNavigateLast() ?? false;
        }

        [ACPropertyInfo(1, "", "en{'Search text for data set filtering'}de{'Suchtext zur Datensatzfilterung'}")]
        public virtual string SearchWord
        {
            get
            {
                return AccessNav?.NavACQueryDefinition?.SearchWord;
            }
            set
            {
                if (AccessNav != null && AccessNav.NavACQueryDefinition != null)
                {
                    if (AccessNav.NavACQueryDefinition.SearchWord != value)
                    {
                        AccessNav.NavACQueryDefinition.SearchWord = value;
                        OnPropertyChanged();
                        ExecuteMethod(Const.CmdNameSearch, null);
                        NavigateFirst();
                    }
                }
            }
        }

        [ACPropertyInfo(1, "", "en{'Limit Record Count'}de{'Limit Anzahl Datensätze'}")]
        public int NavTakeCount
        {
            get
            {
                return AccessNav?.NavACQueryDefinition.TakeCount ?? 0;
            }
            set
            {
                if (AccessNav != null)
                    AccessNav.NavACQueryDefinition.TakeCount = value;
            }
        }


        #endregion

        #region Configuration
        public ACClass PrimaryNavigationquery()
        {
            return ComponentClass.PrimaryNavigationquery();
        }

        /// <summary>
        /// Returns all Queries that are related to this Businessobject.
        /// The IACConfig.Value-Property contains an object of type ACComposition.
        /// Call ACComposition.GetComposition() to read the ACClass of the QueryDefinition.
        /// </summary>
        /// <value>
        /// A list of Queries
        /// </value>
        [ACPropertyPointConfig(9999, "", typeof(ACComposition), "en{'Navigation'}de{'Navigation'}")]
        public IEnumerable<IACConfig> NavigationqueryList
        {
            get
            {
                return ComponentClass.ConfigurationEntries.Where(c => c.KeyACUrl == Const.KeyACUrl_NavigationqueryList);
            }
        }

        #endregion

        #region abstract member        
        /// <summary>Generate an ACAccessNav&lt;T&gt; instance in your derived class an assign it to a private field. Overwrite this abstract property and return your ACAccessNav&lt;T&gt; instance.</summary>
        /// <value>An ACAccessNav&lt;T&gt; instance</value>
        public abstract IAccessNav AccessNav { get; }
        #endregion

        #region DataExport

        public override bool IsEnabledDataExportDialog()
        {
            return AccessNav != null && AccessNav.NavObjectList != null;
        }

        public override bool IsEnabledDataExport()
        {
            return     AccessNav != null 
                   &&  AccessNav.NavObjectList != null
                   && !string.IsNullOrEmpty(DataExportFilePath)
                   && DataExportFolderPathExist(DataExportFilePath);
        }

        private bool DataExportFolderPathExist(string DataExportFilePath)
        {
            int indexSlash = DataExportFilePath.LastIndexOf("\\");
            return Directory.Exists(DataExportFilePath.Substring(0, indexSlash)) && DataExportFilePath.EndsWith(".xml");
        }


        public override string DataExportGenerateFileName()
        {
            Type type = AccessNav.NavObjectList.GetType().GetGenericArguments()[0];
            return string.Format(Root.Environment.Datapath + @"\{0}_{1}.xml", type.Name + "s", DateTime.Now.ToString("yyyy-MM-dd_HH-mm"));
        }
        #endregion

        #region Background worker

        public override void BgWorkerDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            base.BgWorkerDoWork(sender, e);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            string command = e.Argument.ToString();
            switch (command)
            {
                case "DataExport":
                    DoExport();
                    break;
            }
        }

        public override void BgWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            base.BgWorkerCompleted(sender, e);
            CloseWindow(this, DesignNameProgressBar);
        }

        private void DoExport()
        {
            ACEntitySerializer ser = new ACEntitySerializer();
            FileStream fs = new FileStream(DataExportFilePath, FileMode.OpenOrCreate);
            BackgroundWorker.ProgressInfo.TotalProgress.ProgressText = string.Format(@"Deserialize items  {1} to file {0}.", DataExportFilePath, AccessNav.NavACQueryDefinition.ACIdentifier);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(ser.Serialize(Database, AccessNav.NavACQueryDefinition, Root.Environment.Datapath, true).ToString());
            }
        }

        #endregion

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(SMReadOnly):
                    SMReadOnly();
                    return true;
                case nameof(SMNew):
                    SMNew();
                    return true;
                case nameof(SMEdit):
                    SMEdit();
                    return true;
                case nameof(SMSearch):
                    SMSearch();
                    return true;
                case nameof(DeleteDialogSoftOk):
                    DeleteDialogSoftOk();
                    return true;
                case nameof(DeleteDialogHardOk):
                    DeleteDialogHardOk();
                    return true;
                case nameof(DeleteDialogCancel):
                    DeleteDialogCancel();
                    return true;
                case nameof(IsEnabledDeleteDialogSoftOk):
                    result = IsEnabledDeleteDialogSoftOk();
                    return true;
                case nameof(IsEnabledNavigateFirst):
                    result = IsEnabledNavigateFirst();
                    return true;
                case nameof(IsEnabledNavigatePrev):
                    result = IsEnabledNavigatePrev();
                    return true;
                case nameof(IsEnabledNavigateNext):
                    result = IsEnabledNavigateNext();
                    return true;
                case nameof(IsEnabledNavigateLast):
                    result = IsEnabledNavigateLast();
                    return true;
                case nameof(NavigateFirst):
                    NavigateFirst();
                    return true;
                case nameof(NavigatePrev):
                    NavigatePrev();
                    return true;
                case nameof(NavigateNext):
                    NavigateNext();
                    return true;
                case nameof(NavigateLast):
                    NavigateLast();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #region Delete logic (IDeleteInfo) implementation

        /// <summary>
        /// Button for confirming a temporary deletion (Soft-Delete). 
        /// Closes the confirmation dialog.
        /// </summary>
        [ACMethodInfo("DeleteDialogSoftOk", "en{'Temporary delete'}de{'Vorübergehendes Löschen'}", (short) MISort.DeleteSoft, false)]
        public void DeleteDialogSoftOk()
        {
            OnDelete(true);
            CloseTopDialog();
        }

        public bool IsEnabledDeleteDialogSoftOk()
        {
            return AccessNav != null 
                && AccessNav.CurrentNavObject != null 
                && (AccessNav.CurrentNavObject is IDeleteInfo) 
                && (AccessNav.CurrentNavObject as IDeleteInfo).DeleteDate == null;
        }

        /// <summary>
        /// Button for confirming a deleting the current entity on the database.
        /// Closes the confirmation dialog.
        /// </summary>
        [ACMethodInfo("DeleteDialogHardOk", "en{'Permanently delete'}de{'Dauerhaft löschen'}", (short) MISort.DeleteHard, false)]
        public void DeleteDialogHardOk()
        {
            OnDelete(false);
            CloseTopDialog();
        }

        public virtual void OnDelete(bool softDelete)
        {
        }


        /// <summary>
        /// Cancelling a delete operation. Closes the confirmation dialog.
        /// </summary>
        [ACMethodInfo("DeleteDialogCancel", Const.Cancel, (short) MISort.Cancel, false)]
        public void DeleteDialogCancel()
        {
            CloseTopDialog();
        }

        public virtual bool IsEnabledRestore()
        {
            return AccessNav != null
                    && AccessNav.CurrentNavObject != null
                    && (AccessNav.CurrentNavObject is IDeleteInfo)
                    && (AccessNav.CurrentNavObject as IDeleteInfo).DeleteDate != null;
        }

        /// <summary>
        /// Restores a soft-deleted entity object by setting the IDeleteInfo.DeleteDate and IDeleteInfo.DeleteName to null
        /// </summary>
        public virtual void OnRestore()
        {
            IDeleteInfo deleteInfo = AccessNav.CurrentNavObject as IDeleteInfo;
            if (deleteInfo != null)
            {
                deleteInfo.DeleteDate = null;
                deleteInfo.DeleteName = null;
            }
        }

        protected virtual string GetDeleteDialogMessage()
        {
            string template = "";
            if (AccessNav.CurrentNavObject is IDeleteInfo)
            {
                IDeleteInfo deleteInfo = AccessNav.CurrentNavObject as IDeleteInfo;
                if (deleteInfo.DeleteDate == null)
                    template = Root.Environment.TranslateMessage(this, "lblDeleteSoft", new object[] { GetDeleteItemCaption() });
                else
                    template = Root.Environment.TranslateMessage(this, "lblUnDeleteSoft", new object[] { GetDeleteItemCaption() }); ;
            }
            else
                template = string.Format(template, GetDeleteItemCaption());
            return template;
        }

        protected virtual string GetDeleteItemCaption()
        {
            if (AccessNav == null || AccessNav.CurrentNavObject == null || !(AccessNav.CurrentNavObject is IACObject)) 
                return null;
            return (AccessNav.CurrentNavObject as IACObject).ACCaption;
        }

        [ACPropertyInfo(999, "DeleteDialogMessage", "en{'Delete message'}de{'Löschen Meldung'}")]
        public string DeleteDialogMessage
        {
            get
            {
                return GetDeleteDialogMessage();
            }
        }
        #endregion

    }
}
