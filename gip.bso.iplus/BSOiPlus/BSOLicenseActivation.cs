// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
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
            if (Root.Environment.License.IsLicenseValid)
            {
                IsInfoDataVisible = false;
                IsActivationDataVisible = false;
                IsGetCompanyNameVisible = false;
                IsProductActivatedVisible = true;
                
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
                if (value != null)
                    LicensedPackages = Encoding.UTF8.GetString(_SelectedLicenseCompany.SystemInternal).Split(',').ToList();
                else
                    LicensedPackages = null;
                OnPropertyChanged("SelectedLicenseCompany");
            }
        }

        /// <summary>
        /// Gets the list of available companies.
        /// </summary>
        [ACPropertyList(409, "Company")]
        public List<VBSystem> LicenseCompanyList
        {
            get
            {
                return Root.Environment.License.SystemList;
            }
        }

        private List<string> _LicensedPackages;
        /// <summary>
        /// Gets or sets the list of a licensed packages.
        /// </summary>
        [ACPropertyInfo(410)]
        public List<string> LicensedPackages
        {
            get
            {
                return _LicensedPackages;
            }
            set
            {
                _LicensedPackages = value;
                OnPropertyChanged("LicensedPackages");
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

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Shows the dialog for the license activation.
        /// </summary>
        [ACMethodInfo("", "en{'Product activation'}de{'Produktaktivierung'}", 401, true)]
        public void ShowLicenseActivation()
        {
            ShowDialog(this, "LicenceActivation");
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
        public void BrowseLicenceFile()
        {
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            LicenceFilePath = mediaController.OpenFileDialog(false, LicenceFilePath, true) ?? LicenceFilePath;
        }

        /// <summary>
        /// This method try activate the application.
        /// </summary>
        [ACMethodInfo("", "en{'Activate'}de{'Freischalten'}", 404, true)]
        public void ActivateProduct()
        {
            bool result = false;
            try
            {
                using (Database db = new core.datamodel.Database())
                {
                    result = Root.Environment.License.ActivateLicense(File.ReadAllText(LicenceFilePath), CompanyName, db);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("BSOLicenseActivation", "ActivateProduct", msg);

                result = false;
            }

            if(result)
            {
                IsProductActivatedVisible = true;
                IsActivationDataVisible = false;
                IsInfoDataVisible = false;
                if (Root.Environment.License.NeedRestart)
                {
                    RestartText = null;
                    RestartText = ComponentClass.GetText("RestartText").ACCaption;
                    OnPropertyChanged("LicensedToText");
                }
                Messages.InfoAsync(this, "Info50027");
            }
            else
            {
                Messages.WarningAsync(this, "Warning50020");
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
            using (Database db = new core.datamodel.Database())
            {
                VBSystem sys = db.VBSystem.FirstOrDefault(c => c.CustomerName != null);
                if (sys == null)
                    return;
                CompanyName = sys.CustomerName;
            }
            GetActivationData();
            IsInfoDataVisible = false;
            IsActivationDataVisible = true;
            IsGetCompanyNameVisible = false;
            IsProductActivatedVisible = false;
        }

        /// <summary>
        /// Closes the license activation dialog.
        /// </summary>
        [ACMethodInfo("", "en{'Close'}de{'Schließen'}", 406, true)]
        public void CloseDialog()
        {
            CloseTopDialog();
            StopComponent(this);
        }

        /// <summary>
        /// Opens a dialog for exporting activation data.
        /// </summary>
        [ACMethodInfo("", "en{'Export activation data'}de{'Aktivierungsdaten exportieren'}", 407, true)]
        public void ExportActivationData()
        {
            ShowDialog(this, "ExportDialog");
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
                File.WriteAllText(string.Format("{0}\\{1}-activation data.gip", ExportDirPath, CompanyName), exportData);
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
        public void BrowseExportActivationDataDir()
        {
            string intialPaht = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            ExportDirPath = mediaController.OpenFileDialog(true, intialPaht, true) ?? ExportDirPath;
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
                case"ShowLicenseActivation":
                    ShowLicenseActivation();
                    return true;
                case"GetActivationData":
                    GetActivationData();
                    return true;
                case"IsEnabledGetActivationData":
                    result = IsEnabledGetActivationData();
                    return true;
                case"BrowseLicenceFile":
                    BrowseLicenceFile();
                    return true;
                case"ActivateProduct":
                    ActivateProduct();
                    return true;
                case"IsEnabledActivateProduct":
                    result = IsEnabledActivateProduct();
                    return true;
                case"AddNewLicense":
                    AddNewLicense();
                    return true;
                case"CloseDialog":
                    CloseDialog();
                    return true;
                case"ExportActivationData":
                    ExportActivationData();
                    return true;
                case"ExportActivationDataDialog":
                    ExportActivationDataDialog();
                    return true;
                case"BrowseExportActivationDataDir":
                    BrowseExportActivationDataDir();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
