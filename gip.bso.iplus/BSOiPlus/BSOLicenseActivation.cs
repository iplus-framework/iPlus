// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.licensing;
using System.IO;
using gip.core.media;

namespace gip.bso.iplus
{
    /// <summary>
    /// Bussiness object for license activation.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioLicense, "en{'License Activation'}de{'Lizenzaktivierung'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, true, Const.QueryPrefix + ACPackage.ClassName)]
    public sealed class BSOLicenseActivation : ACBSO
    {
        #region c'tors

        public BSOLicenseActivation(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") 
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (Root.Environment.License.IsLicenseValid || HasInstalledProductLicense)
            {
                IsInfoDataVisible = false;
                IsActivationDataVisible = false;
                IsGetCompanyNameVisible = false;
                IsProductActivatedVisible = true;
                SelectedLicenseCompany = LicenseCompanyList?.LastOrDefault();
            }
            else
            {
                IsInfoDataVisible = true;
                IsActivationDataVisible = false;
                IsGetCompanyNameVisible = true;
                IsProductActivatedVisible = false;
            }
            if(Root.Environment.License.NeedRestart)
                RestartText = ComponentClass.GetText("RestartText").ACCaption;
            return base.ACInit(startChildMode);
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            return await base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Properties

        private string _CompanyName;
        /// <summary>
        /// Gets or sets the company name in a license.
        /// </summary>
        [ACPropertyInfo(401,"", "en{'Company name'}de{'Firmenname'}")]
        public string CompanyName
        {
            get
            {
                return _CompanyName;
            }
            set
            {
                _CompanyName = value;
                OnPropertyChanged("CompanyName");
            }
        }

        private string _DatabaseName;
        /// <summary>
        /// Gets or sets the database name in a license.
        /// </summary>
        [ACPropertyInfo(402, "", "en{'Database name'}de{'Databankname'}")]
        public string DatabaseName
        {
            get
            {
                return _DatabaseName;
            }
            private set
            {
                _DatabaseName = value;
                OnPropertyChanged("DatabaseName");
            }
        }

        private string _DatabaseServerName;
        /// <summary>
        /// Gets or sets the database server name in license.
        /// </summary>
        [ACPropertyInfo(403, "", "en{'Database server name'}de{'Name des Datenbankservers'}")]
        public string DatabaseServerName
        {
            get
            {
                return _DatabaseServerName;
            }
            private set
            {
                _DatabaseServerName = value;
                OnPropertyChanged("DatabaseServerName");
            }
        }

        private string _IdentificationCode;
        /// <summary>
        /// Gets or sets the identification code.
        /// </summary>
        [ACPropertyInfo(404,"", "en{'Identification code'}de{'Identifikationscode'}")]
        public string IdentificationCode
        {
            get
            {
                return _IdentificationCode;
            }
            private set
            {
                _IdentificationCode = value;
                OnPropertyChanged("IdentificationCode");
            }
        }

        private string _LicenceFilePath;
        /// <summary>
        /// Gets or sets the license file path.
        /// </summary>
        [ACPropertyInfo(405)]
        public string LicenceFilePath
        {
            get
            {
                return _LicenceFilePath;
            }
            set
            {
                _LicenceFilePath = value;
                OnPropertyChanged("LicenceFilePath");
            }
        }

        private string _RestartText;
        /// <summary>
        /// Gets or sets the restart text in license dialog.
        /// </summary>
        [ACPropertyInfo(406)]
        public string RestartText
        {
            get
            {
                return _RestartText;
            }
            set
            {
                _RestartText = value;
                OnPropertyChanged("RestartText");
            }
        }

        /// <summary>
        /// <c>true</c> when at least one product license (VBP row) is installed. It is shown
        /// even without a runtime license, so the customer sees what is installed.
        /// </summary>
        private bool HasInstalledProductLicense
        {
            get
            {
                return Root.Environment.License.SystemList?.Any() == true;
            }
        }

        /// <summary>
        /// Gets the licensed to text.
        /// </summary>
        [ACPropertyInfo(407)]
        public string LicensedToText
        {
            get
            {
                string text = "Licensed to {0}";
                var acClassText = ComponentClass.GetText("LicensedTo");
                if(acClassText != null && !string.IsNullOrEmpty(acClassText.ACCaption))
                    text = acClassText.ACCaption;
                return string.Format(text, Root.Environment.License.LicensedTo);
            }
        }

        //private string _ProjectNo;
        //[ACPropertyInfo(999,"","en{'Project number'}de{'Projekt nummer'}")]
        //public string ProjectNo
        //{
        //    get
        //    {
        //        if (_ProjectNo == null)
        //            _ProjectNo = Root.Environment.Licence.ProjectNo;
        //        return _ProjectNo;
        //    }
        //}

        private VBSystem _SelectedLicenseCompany;
        /// <summary>
        /// Gets or sets the selected license company.
        /// </summary>
        [ACPropertySelected(408,"Company")]
        public VBSystem SelectedLicenseCompany
        {
            get
            {
                return _SelectedLicenseCompany;
            }
            set
            {
                _SelectedLicenseCompany = value;
                _LicensedPackages.ReplaceAll(GetLicensedPackages(value));
                OnPropertyChanged("SelectedLicenseCompany");
            }
        }

        /// <summary>
        /// Extracts the licensed package names from an activated license row.
        /// </summary>
        /// <remarks>
        /// V5 stores the complete signed license file in <see cref="VBSystem.SystemInternal"/>,
        /// so the payload has to be parsed. The V4 format (a comma separated list of
        /// package names) is still understood as a fallback.
        /// </remarks>
        private static List<string> GetLicensedPackages(VBSystem licenseRow)
        {
            if (licenseRow == null || licenseRow.SystemInternal == null || licenseRow.SystemInternal.Length == 0)
                return null;

            string text = Encoding.UTF8.GetString(licenseRow.SystemInternal);
            if (LicenseFile.TryParse(text, out _, out byte[] payloadBytes, out _))
            {
                try
                {
                    LicensePayload payload = LicenseFile.DeserializePayload(Encoding.UTF8.GetString(payloadBytes));
                    return payload.Packages != null && payload.Packages.Count > 0
                        ? new List<string>(payload.Packages)
                        : new List<string>();
                }
                catch (LicenseFormatException)
                {
                    // Fall through to the legacy format.
                }
            }

            return text.Split(',').Select(c => c.Trim()).Where(c => c.Length > 0).ToList();
        }

        /// <summary>
        /// Gets the list of available companies.
        /// </summary>
        [ACPropertyList(409, "Company")]
        public List<VBSystem> LicenseCompanyList
        {
            get
            {
                // License.SystemList runs a query and returns a new list on every call. A new instance
                // on each read would reset the ListBox source while its selection is being changed.
                if (_LicenseCompanyList == null)
                    _LicenseCompanyList = Root.Environment.License.SystemList;
                return _LicenseCompanyList;
            }
        }
        private List<VBSystem> _LicenseCompanyList;

        // One instance for the lifetime of the BSO: replacing it while the ListBox is changing its
        // selection makes Avalonia throw "Cannot change source while update is in progress".
        // The property is registered as List<string>, so the type must stay a List<string>.
        private readonly NotifyingStringList _LicensedPackages = new NotifyingStringList();
        /// <summary>
        /// Gets the list of the licensed packages of the selected license.
        /// </summary>
        [ACPropertyInfo(410)]
        public List<string> LicensedPackages
        {
            get
            {
                return _LicensedPackages;
            }
        }

        private sealed class NotifyingStringList : List<string>, INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public void ReplaceAll(IEnumerable<string> items)
            {
                Clear();
                if (items != null)
                    AddRange(items);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private string _ExportDirPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        /// <summary>
        /// Gets or sets the export directory path.
        /// </summary>
        [ACPropertyInfo(411,"", "en{'Export directory'}de{'Exportverzeichnis'}")]
        public string ExportDirPath
        {
            get
            {
                return _ExportDirPath;
            }
            set
            {
                _ExportDirPath = value;
                OnPropertyChanged("ExportDirPath");
            }
        }

        #region Properties -> Visibility

        private bool _IsInfoDataVisible;
        /// <summary>
        /// Gets or sets the value is information data visible.
        /// </summary>
        [ACPropertyInfo(412)]
        public bool IsInfoDataVisible
        {
            get
            {
                return _IsInfoDataVisible;
            }
            set
            {
                _IsInfoDataVisible = value;
                OnPropertyChanged("IsInfoDataVisible");
            }
        }

        private bool _IsGetCompanyNameVisible;
        /// <summary>
        /// Gets or sets the value is get company name visible.
        /// </summary>
        [ACPropertyInfo(413)]
        public bool IsGetCompanyNameVisible
        {
            get
            {
                return _IsGetCompanyNameVisible;
            }
            set
            {
                _IsGetCompanyNameVisible = value;
                OnPropertyChanged("IsGetCompanyNameVisible");
            }
        }

        private bool _IsActivationDataVisible;
        /// <summary>
        /// Gets or sets the value is activation data visible.
        /// </summary>
        [ACPropertyInfo(414)]
        public bool IsActivationDataVisible
        {
            get
            {
                return _IsActivationDataVisible;
            }
            set
            {
                _IsActivationDataVisible = value;
                OnPropertyChanged("IsActivationDataVisible");
            }
        }

        private bool _IsProductActivatedVisible;
        /// <summary>
        /// Gets or sets the value is product activated visible.
        /// </summary>
        [ACPropertyInfo(415)]
        public bool IsProductActivatedVisible
        {
            get
            {
                return _IsProductActivatedVisible;
            }
            set
            {
                _IsProductActivatedVisible = value;
                OnPropertyChanged("IsProductActivatedVisible");
            }
        }

        /// <summary>
        /// Human readable summary of the installed license: edition, licensed
        /// company and the number of concurrent users granted by the framework
        /// runtime license.
        /// </summary>
        [ACPropertyInfo(416, "", "en{'License status'}de{'Lizenzstatus'}")]
        public string LicenseStatusText
        {
            get
            {
                gip.core.datamodel.Licensing.License license = Root.Environment.License;
                if (license == null)
                    return "";

                if (license.IsOpenEdition)
                {
                    if (HasInstalledProductLicense)
                        return ComponentClass.GetText("ProductWithoutRuntimeText")?.ACCaption
                            ?? "Product license installed, but no framework runtime license. The product packages stay locked until a runtime license is activated (use 'Add license').";
                    return ComponentClass.GetText("OpenEditionText")?.ACCaption ?? "Open edition (GPL-3) — no runtime license installed";
                }

                int maxUsers = license.LicensedMaxUsers;
                string users = maxUsers > 0
                    ? string.Format(ComponentClass.GetText("MaxUsersText")?.ACCaption ?? "{0} concurrent users", maxUsers)
                    : ComponentClass.GetText("UnlimitedUsersText")?.ACCaption ?? "unlimited users";

                string licensedTo = license.LicensedTo;
                if (string.IsNullOrEmpty(licensedTo))
                    return users;
                return string.Format("{0} — {1}", licensedTo, users);
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Shows the dialog for the license activation.
        /// </summary>
        [ACMethodInfo("", "en{'Product activation'}de{'Produktaktivierung'}", 401, true)]
        public async void ShowLicenseActivation()
        {
            await ShowDialogAsync(this, "LicenceActivation");
        }

        /// <summary>
        /// Get and read activation data.
        /// </summary>
        [ACMethodInfo("", "en{'Read activation data'}de{'Aktivierungsdaten einlesen'}", 402,true)]
        public void GetActivationData()
        {
            Tuple<string,string,string> userInfo = null;
            if(!Root.Environment.License.GetUniqueUserInfo(out userInfo))
            {
                Messages.WarningAsync(this, "Warning50022");
                return;
            }

            DatabaseName = userInfo.Item1;
            DatabaseServerName = userInfo.Item2;
            IdentificationCode = userInfo.Item3;
            IsActivationDataVisible = true;
            IsGetCompanyNameVisible = false;
        }

        public bool IsEnabledGetActivationData()
        {
            if (string.IsNullOrEmpty(CompanyName))
                return false;
            return true;
        }

        /// <summary>
        /// Opens file dialog to select a license file.
        /// </summary>
        [ACMethodInfo("", "en{'...'}de{'...'}", 403, true)]
        public async Task BrowseLicenceFile()
        {
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            LicenceFilePath = await mediaController.OpenFileDialog(false, LicenceFilePath, true) ?? LicenceFilePath;
        }

        public bool IsEnabledBrowseLicenceFile()
        {
            return true;
        }

        /// <summary>
        /// This method try activate the application.
        /// </summary>
        /// <remarks>
        /// V5 path: signed <c>*.gip</c> license files are validated offline and
        /// stored in the database (<see cref="License.ActivateLicenseFile"/>).
        /// </remarks>
        [ACMethodInfo("", "en{'Activate'}de{'Freischalten'}", 404, true)]
        public void ActivateProduct()
        {
            bool result = false;
            string activationError = null;
            try
            {
                string licenseText = File.ReadAllText(LicenceFilePath);
                string error;

                // Signed license file (framework runtime, product or developer license).
                result = Root.Environment.License.ActivateLicenseFile(licenseText, out error);
                activationError = error;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("BSOLicenseActivation", "ActivateProduct", msg);

                activationError = msg;
                result = false;
            }

            if(result)
            {
                IsProductActivatedVisible = true;
                IsActivationDataVisible = false;
                IsInfoDataVisible = false;
                OnPropertyChanged("LicensedToText");
                OnPropertyChanged("LicenseStatusText");
                // A second or third license adds a row: refresh the list and select it.
                _LicenseCompanyList = null;
                OnPropertyChanged("LicenseCompanyList");
                SelectedLicenseCompany = LicenseCompanyList?.LastOrDefault();
                if (Root.Environment.License.NeedRestart)
                {
                    RestartText = null;
                    RestartText = ComponentClass.GetText("RestartText").ACCaption;
                }
                Messages.InfoAsync(this, "Info50027");
            }
            else
            {
                if (string.IsNullOrEmpty(activationError))
                    Messages.WarningAsync(this, "Warning50020");
                else
                    Messages.WarningAsync(this, activationError, true);
            }
        }

        public bool IsEnabledActivateProduct()
        {
            if (string.IsNullOrEmpty(LicenceFilePath))
                return false;
            return true;
        }

        /// <summary>
        /// Adds a new license.
        /// </summary>
        [ACMethodInfo("", "en{'Add license'}de{'Lizenz hinzufügen'}", 405,true)]
        public void AddNewLicense()
        {
            // Reuse the customer name of an already activated product license so that
            // the activation data of the same installation is exported again.
            VBSystem sys = Root.Environment.License.SystemList?.FirstOrDefault(c => !string.IsNullOrEmpty(c.CustomerName));
            if (sys != null)
                CompanyName = sys.CustomerName;
            GetActivationData();
            IsInfoDataVisible = false;
            IsActivationDataVisible = true;
            IsGetCompanyNameVisible = false;
            IsProductActivatedVisible = false;
        }

        public bool IsEnabledAddNewLicense()
        {
            return true;
        }

        /// <summary>
        /// Closes the license activation dialog.
        /// </summary>
        [ACMethodInfo("", "en{'Close'}de{'Schließen'}", 406, true)]
        public async Task CloseDialog()
        {
            CloseTopDialog();
            await StopComponent(this);
        }

        public bool IsEnabledCloseDialog()
        {
            return true;
        }

        /// <summary>
        /// Removes the selected license from the database.
        /// </summary>
        [ACMethodInfo("", "en{'Remove license'}de{'Lizenz entfernen'}", 409, true)]
        public async Task RemoveLicense()
        {
            VBSystem licenseRow = SelectedLicenseCompany;
            if (licenseRow == null)
                return;

            string question = string.Format("Remove the license of '{0}' ({1})? The change becomes effective after a restart.",
                licenseRow.Company, licenseRow.ProjectNo);
            if (await Messages.QuestionAsync(this, question, Global.MsgResult.No, true) != Global.MsgResult.Yes)
                return;

            if (!Root.Environment.License.RemoveActivatedLicense(licenseRow, out string error))
            {
                await Messages.WarningAsync(this, error, true);
                return;
            }

            _LicenseCompanyList = null;
            OnPropertyChanged("LicenseCompanyList");
            SelectedLicenseCompany = LicenseCompanyList?.LastOrDefault();
            if (!LicenseCompanyList.Any())
            {
                IsProductActivatedVisible = false;
                IsActivationDataVisible = false;
                IsInfoDataVisible = true;
                IsGetCompanyNameVisible = true;
            }
            OnPropertyChanged("LicensedToText");
            OnPropertyChanged("LicenseStatusText");
            if (Root.Environment.License.NeedRestart)
            {
                RestartText = null;
                RestartText = ComponentClass.GetText("RestartText").ACCaption;
            }
        }

        public bool IsEnabledRemoveLicense()
        {
            return SelectedLicenseCompany != null;
        }

        /// <summary>
        /// Opens a dialog for exporting activation data.
        /// </summary>
        [ACMethodInfo("", "en{'Export activation data'}de{'Aktivierungsdaten exportieren'}", 407, true)]
        public async void ExportActivationData()
        {
            await ShowDialogAsync(this, "ExportDialog");
        }

        public bool IsEnabledExportActivationData()
        {
            return !string.IsNullOrEmpty(CompanyName) && !string.IsNullOrEmpty(IdentificationCode);
        }

        public bool IsEnabledExportActivationDataDialog()
        {
            return !string.IsNullOrEmpty(ExportDirPath);
        }

        /// <summary>
        /// Exports the data for activation.
        /// </summary>
        [ACMethodInfo("", "en{'Export'}de{'Export'}", 408, true)]
        public async void ExportActivationDataDialog()
        {
            string exportData = string.Format("<xml><companyName>{0}</companyName><db>{1}</db><ds>{2}</ds><code>{3}</code></xml>",CompanyName, DatabaseName, DatabaseServerName, 
                                                                                                                                 IdentificationCode);
            try
            {
                File.WriteAllText(Path.Combine(ExportDirPath, string.Format("{0}-activation data.gip", CompanyName)), exportData);
                await Messages.InfoAsync(this, "Info50026");
                CloseTopDialog();
            }
            catch (Exception e)
            {
                await Messages.WarningAsync(this, "Warning50023", false, e.Message);
            }
        }

        /// <summary>
        /// Opens a file dialog to select a folder where activation data will be exported.
        /// </summary>
        [ACMethodInfo("", "en{'...'}de{'...'}", 409, true)]
        public async Task BrowseExportActivationDataDir()
        {
            string intialPaht = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            ExportDirPath = await mediaController.OpenFileDialog(true, intialPaht, true) ?? ExportDirPath;
        }

        public bool IsEnabledBrowseExportActivationDataDir()
        {
            return true;
        }

        /// <summary>Called inside the GetControlModes-Method to get the Global.ControlModes from derivations.
        /// This method should be overriden in the derivations to dynmically control the presentation mode depending on the current value which is bound via VBContent</summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            switch(vbControl.VBContent)
            {
                case "IsInfoDataVisible":
                    if (IsInfoDataVisible)
                        return Global.ControlModes.Enabled;
                    else
                        return Global.ControlModes.Hidden;
                case "IsGetCompanyNameVisible":
                    if (IsGetCompanyNameVisible)
                        return Global.ControlModes.Enabled;
                    else
                        return Global.ControlModes.Hidden;
                case "IsActivationDataVisible":
                    if (IsActivationDataVisible)
                        return Global.ControlModes.Enabled;
                    else
                        return Global.ControlModes.Hidden;
                case "IsProductActivatedVisible":
                    if (IsProductActivatedVisible)
                        return Global.ControlModes.Enabled;
                    else
                        return Global.ControlModes.Hidden;
                case "RestartText":
                    if (Root.Environment.License.NeedRestart)
                        return Global.ControlModes.Enabled;
                    else if(IsProductActivatedVisible)
                        return Global.ControlModes.Collapsed;
                    break;
                case "CompanyName":
                    if (string.IsNullOrEmpty(CompanyName))
                        return Global.ControlModes.Enabled;
                    else
                        return Global.ControlModes.Disabled;
            }
            return base.OnGetControlModes(vbControl);
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(ShowLicenseActivation):
                    ShowLicenseActivation();
                    return true;
                case nameof(GetActivationData):
                    GetActivationData();
                    return true;
                case nameof(IsEnabledGetActivationData):
                    result = IsEnabledGetActivationData();
                    return true;
                case nameof(BrowseLicenceFile):
                    result = BrowseLicenceFile();
                    return true;
                case nameof(IsEnabledBrowseLicenceFile):
                    result = IsEnabledBrowseLicenceFile();
                    return true;
                case nameof(ActivateProduct):
                    ActivateProduct();
                    return true;
                case nameof(IsEnabledActivateProduct):
                    result = IsEnabledActivateProduct();
                    return true;
                case nameof(AddNewLicense):
                    AddNewLicense();
                    return true;
                case nameof(IsEnabledAddNewLicense):
                    result = IsEnabledAddNewLicense();
                    return true;
                case nameof(RemoveLicense):
                    result = RemoveLicense();
                    return true;
                case nameof(IsEnabledRemoveLicense):
                    result = IsEnabledRemoveLicense();
                    return true;
                case nameof(CloseDialog):
                    result = CloseDialog();
                    return true;
                case nameof(IsEnabledCloseDialog):
                    result = IsEnabledCloseDialog();
                    return true;
                case nameof(ExportActivationData):
                    ExportActivationData();
                    return true;
                case nameof(IsEnabledExportActivationData):
                    result = IsEnabledExportActivationData();
                    return true;
                case nameof(ExportActivationDataDialog):
                    ExportActivationDataDialog();
                    return true;
                case nameof(IsEnabledExportActivationDataDialog):
                    result = IsEnabledExportActivationDataDialog();
                    return true;
                case nameof(BrowseExportActivationDataDir):
                    result = BrowseExportActivationDataDir();
                    return true;
                case nameof(IsEnabledBrowseExportActivationDataDir):
                    result = IsEnabledBrowseExportActivationDataDir();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public override IEnumerable<string> GetPropsToObserveForIsEnabled(string acMethodName)
        {
            switch (acMethodName)
            {
                case nameof(IsEnabledGetActivationData):
                case nameof(GetActivationData):
                case nameof(BrowseLicenceFile):
                case nameof(BrowseExportActivationDataDir):
                    return new string[] { nameof(CompanyName) };
                case nameof(IsEnabledActivateProduct):
                case nameof(ActivateProduct):
                    return new string[] { nameof(LicenceFilePath) };
                case nameof(IsEnabledExportActivationData):
                case nameof(ExportActivationData):
                    return new string[] { nameof(CompanyName), nameof(IdentificationCode) };
                case nameof(IsEnabledExportActivationDataDialog):
                case nameof(ExportActivationDataDialog):
                    return new string[] { nameof(ExportDirPath) };
                case nameof(IsEnabledRemoveLicense):
                case nameof(RemoveLicense):
                    return new string[] { nameof(SelectedLicenseCompany) };
                case nameof(IsEnabledAddNewLicense):
                case nameof(AddNewLicense):
                case nameof(IsEnabledCloseDialog):
                case nameof(CloseDialog):
                    return new string[] { nameof(IsProductActivatedVisible) };
                default:
                    return base.GetPropsToObserveForIsEnabled(acMethodName);
            }
        }

        #endregion
    }
}
