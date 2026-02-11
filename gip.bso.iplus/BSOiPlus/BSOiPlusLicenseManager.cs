// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.ComponentModel;
using System.Threading;
using gip.core.media;
using SkiaSharp;

namespace gip.bso.iplus
{
    /// <summary>
    /// Bussiness object for license managing in iPlus
    /// </summary>
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'License Management'}de{'Lizenzverwaltung'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + VBLicense.ClassName)]
    public sealed class BSOiPlusLicenseManager : ACBSONav
    {
        public const string ClassName = Const.LicenseManager;

        #region c'tors

        public BSOiPlusLicenseManager(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") 
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
#if (!DEBUG)
            if (this.Root.Environment.License.CurrentLicenseType != core.datamodel.Licensing.LicenseType.Developer_Issuer
                && this.Root.Environment.License.CurrentLicenseType != core.datamodel.Licensing.LicenseType.Developer_EndUser)
            {
                throw new Exception("Licensemanager can only use developers with have a dongle");
            }            
#endif
            var result = base.ACInit(startChildMode);
            Search();
            return result;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            if (_AccessPrimary != null)
                _AccessPrimary.ACDeInit(true);
            _AccessPrimary = null;
            _SelectedAssignedPackage = null;
            _SelectedAvailablePackage = null;
            _RemoteUserCode = null;
            _RemoteUserKey = null;
            _LicenseFileDirPath = null;
            _LicenseFilePath = null;
            bool done = await base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return done;
        }

        #endregion

        #region DB

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

        public Database Db
        {
            get
            {
                return Database as Database;
            }
        }

        #endregion

        #region Properties

        #region Properties -> BSO

        private bool _IsNewLicense = true;

        public override IAccessNav AccessNav
        {
            get
            {
                return AccessPrimary;
            }
        }

        private ACAccessNav<VBLicense> _AccessPrimary;
        /// <summary>
        /// Gets the Access Primary.
        /// </summary>
        [ACPropertyAccessPrimary(590, VBLicense.ClassName)]
        public ACAccessNav<VBLicense> AccessPrimary
        {
            get
            {
                if (_AccessPrimary == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, PrimaryNavigationquery(), ACType.ACIdentifier);
                    _AccessPrimary = navACQueryDefinition.NewAccessNav<VBLicense>(VBLicense.ClassName, this);
                }
                return _AccessPrimary;
            }
        }

        /// <summary>
        /// Gets or sets the current VB license.
        /// </summary>
        [ACPropertyCurrent(501, VBLicense.ClassName)]
        public VBLicense CurrentVBLicense 
        { 
            get
            {
                return AccessPrimary.Current;
            }
            set
            {
                AccessPrimary.Current = value;
                LoadAvailableAssignedPackages();
                //GenerateiPlusPackageCode();
                OnPropertyChanged("CurrentVBLicense");
                _LicenseImage = null;
                OnPropertyChanged(nameof(LicenseImage));
                OnPropertyChanged("UniqueCustomerCode");
            }
        }

        /// <summary>
        /// Gets the list of a issued licenses.
        /// </summary>
        [ACPropertyList(502, VBLicense.ClassName)]
        public IEnumerable<VBLicense> VBLicenseList
        {
            get
            {
                return AccessPrimary.NavList;
            }
        }

        //private string _UniqueCustomerCode;
        /// <summary>
        /// Gets or sets the unique customer code.
        /// </summary>
        [ACPropertyInfo(503,"", "en{'Unique customer code'}de{'Eindeutiger Kundencode'}")]
        public string UniqueCustomerCode
        {
            get
            {
                if (CurrentVBLicense == null)
                    return null;
                return CurrentVBLicense.SystemDB;
            }
            set
            {
                if (CurrentVBLicense == null)
                    return;
                CurrentVBLicense.SystemDB = value;
                OnPropertyChanged("UniqueCustomerCode");
            }
        }

        private string _ActivationDataPath;
        /// <summary>
        /// Gets or sets the path of the activation data file.
        /// </summary>
        [ACPropertyInfo(505,"", "en{'Activation data file path'}de{'Pfad der Aktivierungsdatendatei'}")]
        public string ActivationDataPath
        {
            get
            {
                return _ActivationDataPath;
            }
            set
            {
                _ActivationDataPath = value;
                OnPropertyChanged("ActivationDataPath");
            }
        }

        #endregion

        #region Properties -> PackageAssignment

        private string _SelectedAvailablePackage;
        /// <summary>
        /// Gets or sets the selected available package.
        /// </summary>
        [ACPropertySelected(506,"AvailablePackage")]
        public string SelectedAvailablePackage
        {
            get
            {
                return _SelectedAvailablePackage;
            }
            set
            {
                _SelectedAvailablePackage = value;
                OnPropertyChanged("SelectedAvailablePackge");
            }
        }

        /// <summary>
        /// Gets or sets the list of available package.
        /// </summary>
        [ACPropertyList(507,"AvailablePackage")]
        public ObservableCollection<string> AvailablePackages
        {
            get;
            set;
        }

        private string _SelectedAssignedPackage;
        /// <summary>
        /// Gets or sets the selected assigned package.
        /// </summary>
        [ACPropertySelected(508,"AssignedPackage")]
        public string SelectedAssignedPackage
        {
            get
            {
                return _SelectedAssignedPackage;
            }
            set
            {
                _SelectedAssignedPackage = value;
                OnPropertyChanged("SelectedAssignedPackage");
            }
        }

        /// <summary>
        /// Gets or sets the list of assigned packages.
        /// </summary>
        [ACPropertyList(509,"AssignedPackage")]
        public ObservableCollection<string> AssignedPackages
        {
            get;
            set;
        }

        #endregion

        #region Properties -> GenerateFile

        private string _LicenseFileDirPath;
        /// <summary>
        /// Gets or sets the directory path of a license file.
        /// </summary>
        [ACPropertyInfo(510, "", "en{'License file directory path'}de{'Verzeichnispfad der Lizenzdatei'}")]
        public string LicenseFileDirPath
        {
            get
            {
                return _LicenseFileDirPath;
            }
            set
            {
                _LicenseFileDirPath = value;
                OnPropertyChanged("LicenseFileDirPath");
            }
        }

        #endregion

        #region Properties -> SignFile

        private string CurrentSignData
        {
            get;
            set;
        }

        private VBSystem CurrentSignLicense
        {
            get;
            set;
        }

        private string _LicenseFilePath;
        /// <summary>
        /// Gets or sets the path of license file for sign with gip private key.
        /// </summary>
        [ACPropertyInfo(511, "", "en{'License file path (*.gip)'}de{'Pfad der Lizenzdatei (*.gip)'}")]
        public string LicenseFilePath
        {
            get
            {
                return _LicenseFilePath;
            }
            set
            {
                _LicenseFilePath = value;
                OnPropertyChanged("LicenseFilePath");
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Methods -> BSO

        /// <summary>
        /// Saves this instance (CurrentVBLicense).
        /// </summary>
        [ACMethodCommand(VBLicense.ClassName, "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
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
            return _IsNewLicense ? OnIsEnabledSave() : false;
        }

        /// <summary>
        /// Undoes the save.
        /// </summary>
        [ACMethodCommand(VBLicense.ClassName, "en{'Undo'}de{'Rückgängig'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost)]
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
            return _IsNewLicense ? OnIsEnabledUndoSave() : false;
        }

        /// <summary>
        /// News this instance.
        /// </summary>
        [ACMethodInteraction(VBLicense.ClassName, Const.New, (short)MISort.New, true, "CurrentVBLicense", Global.ACKinds.MSMethodPrePost)]
        public void New()
        {
            if (!PreExecute("New"))
                return;
            if (AccessPrimary == null)
                return;
            string secondaryKey = Root.NoManager.GetNewNo(Database, typeof(VBLicense), VBLicense.NoColumnName, VBLicense.FormatNewNo, this);
            CurrentVBLicense = VBLicense.NewACObject(Db, null, secondaryKey);
            AccessPrimary.NavList.Add(CurrentVBLicense);
            _IsNewLicense = false;
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
        /// Deletes this instance.
        /// </summary>
        [ACMethodInteraction(VBLicense.ClassName, Const.Delete, (short)MISort.Delete, true, "CurrentVBLicense", Global.ACKinds.MSMethodPrePost)]
        public void Delete()
        {
            if (!PreExecute("Delete"))
                return;
            if (AccessPrimary == null)
                return;
            //Delete();
        }

        /// <summary>
        /// Executes the search operation on the VBLicense list.
        /// </summary>
        [ACMethodCommand(VBLicense.ClassName, "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            AccessPrimary.NavSearch(Db);
            OnPropertyChanged("VBLicenseList");
        }

        /// <summary>
        /// Shows the dialog for import activation data.
        /// </summary>
        [ACMethodInfo("", "en{'Import activation data'}de{'Aktivierungsdaten importieren'}", 501,true)]
        public async void ImportActivationData()
        {
            await ShowDialogAsync(this, "ImportDialog");
        }

        public bool IsEnabledImportActivationData()
        {
            if (CurrentVBLicense == null)
                return false;
            return true;
        }

        /// <summary>
        /// Opens the dialog to select the activation data file.
        /// </summary>
        [ACMethodInfo("","en{'...'}de{'...'}",502,true)]
        public void BrowseActivationDataFile()
        {
            string initialDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            ActivationDataPath = mediaController.OpenFileDialog(false, initialDir, true) ?? ActivationDataPath;
        }

        /// <summary>
        /// Imports the activation data from an XML file.
        /// </summary>
        [ACMethodInfo("", "en{'Import activation data'}de{'Aktivierungsdaten importieren'}", 503, true)]
        public void ImportDataDialog()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                string xml = File.ReadAllText(ActivationDataPath);
                doc.LoadXml(xml);
                foreach (XmlNode node in doc.SelectSingleNode("xml").ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "companyName":
                            CurrentVBLicense.CustomerName = node.InnerText;
                            break;

                        case "code":
                            UniqueCustomerCode = node.InnerText;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Messages.WarningAsync(this, "Warning50024", false, e.Message);
                return;
            }
            CloseTopDialog();
        }

        #endregion

        #region Methods -> PackageAssignment

        /// <summary>
        /// Assigns all available packages.
        /// </summary>
        [ACMethodInfo("","en{'>>'}de{'>>'}",504,true)]
        public void AssignAllPackages()
        {
            foreach(string package in AvailablePackages.ToList())
            {
                AssignedPackages.Add(package);
                AvailablePackages.Remove(package);
            }
        }

        public bool IsEnabledAssignAllPackages()
        {
            if (AvailablePackages != null && AvailablePackages.Any())
                return true;
            return false;
        }

        /// <summary>
        /// Assigns the selected available package.
        /// </summary>
        [ACMethodInfo("", "en{'>'}de{'>'}", 505, true)]
        public void AssignSelectedPackage()
        {
            if (SelectedAvailablePackage == null)
                return;

            AssignedPackages.Add(SelectedAvailablePackage);
            AvailablePackages.Remove(SelectedAvailablePackage);
        }

        public bool IsEnabledAssignSelectedPackage()
        {
            if(SelectedAvailablePackage != null)
                return true;
            return false;
        }

        /// <summary>
        /// Removes all assigned packages.
        /// </summary>
        [ACMethodInfo("", "en{'<<'}de{'<<'}", 506, true)]
        public void RemoveAllPackages()
        {
            foreach(string package in AssignedPackages.ToList())
            {
                AvailablePackages.Add(package);
                AssignedPackages.Remove(package);
            }
        }

        public bool IsEnabledRemoveAllPackages()
        {
            if (AssignedPackages != null && AssignedPackages.Any())
                return true;
            return false;
        }

        /// <summary>
        /// Removes the selected assigned package.
        /// </summary>
        [ACMethodInfo("", "en{'<'}de{'<'}", 507, true)]
        public void RemoveSelectedPackage()
        {
            if (SelectedAssignedPackage == null)
                return;

            AvailablePackages.Add(SelectedAssignedPackage);
            AssignedPackages.Remove(SelectedAssignedPackage);
        }

        public bool IsEnabledRemoveSelectedPackage()
        {
            if (SelectedAssignedPackage != null)
                return true;
            return false;
        }

        private void LoadAvailableAssignedPackages()
        {
            AvailablePackages = null;
            AssignedPackages = null;
            if (CurrentVBLicense == null)
                return;
            
            if(CurrentVBLicense.PackageSystem == null)
            {
                AssignedPackages = new ObservableCollection<string>();
                var query = Root.Environment.License.GetAvailablePackages();
                if (query != null)
                    AvailablePackages = new ObservableCollection<string>(query);
            }
            else
            {
                var query = Root.Environment.License.GetAvailablePackages();
                List<string> allPackages = null;
                if (query == null)
                    allPackages = new List<string>();
                else
                    allPackages = query.ToList();
                var assignedPackages = CurrentVBLicense.ReadablePackages.Split(',');
                AssignedPackages = new ObservableCollection<string>(assignedPackages);
                AvailablePackages = new ObservableCollection<string>(allPackages.Except(assignedPackages));
            }
            OnPropertyChanged("AvailablePackages");
            OnPropertyChanged("AssignedPackages");
        }

        #endregion

        #region Methods -> GenerateFile

        /// <summary>
        /// Generates the license file and saves a changes.
        /// </summary>
        [ACMethodInfo("", "en{'Generate license file'}de{'Lizenzdatei erzeugen'}", 508,true)]
        public void GenerateLicenseFile()
        {
            List<string> packages = new List<string>();
            if (AssignedPackages != null && AssignedPackages.Any())
                packages.AddRange(AssignedPackages);

            CurrentVBLicense.PackageSystem = Encoding.UTF8.GetBytes(CreatePackagesString(packages));

            var system = CurrentVBLicense;

            if (Root.Environment.License.GenerateLicenseFile(LicenseFileDirPath, UniqueCustomerCode, ref system))
            {
                CurrentVBLicense = system;
                ACSaveChanges();
                Messages.InfoAsync(this, "Info50025");
                _IsNewLicense = true;
            }
            else
            {
                Messages.WarningAsync(this, "Warning50021");
            }
        }

        public bool IsEnabledGenerateLicenseFile()
        {
            if (CurrentVBLicense == null || string.IsNullOrEmpty(CurrentVBLicense.CustomerName) || string.IsNullOrEmpty(UniqueCustomerCode) || string.IsNullOrEmpty(LicenseFileDirPath))
                return false;
            return Root.Environment.License.IsEnabledGenerateLicenseFile();
        }

        /// <summary>
        /// Opens the dialog to select the directory in which the license file will be saved.
        /// </summary>
        [ACMethodInfo("","en{'...'}de{'...'}",509)]
        public void BrowseLicenseDir()
        {
            string initialDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            LicenseFileDirPath = mediaController.OpenFileDialog(true, initialDir, true) ?? LicenseFileDirPath;
        }

        private string CreatePackagesString(IEnumerable<string> packageItems)
        {
            string packages = "";
            foreach (string package in packageItems)
            {
                packages += package + ",";
            }
            packages = packages.TrimEnd(',');
            return packages;
        }

        private List<string> ReadPackagesString(string package)
        {
            var packages = package.Split(',');
            return packages.ToList();
        }

        #endregion

        #region Methods -> SignFile

        /// <summary>
        /// Signs the license file with gip private key.
        /// </summary>
        [ACMethodInfo("", "en{'Sign license file'}de{'Lizenzdatei signieren'}", 510, true)]
        public async void SignLicenseFile()
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(VBSystem));
            VBSystem sys = null;
            try
            {
                using (FileStream fs = new FileStream(LicenseFilePath, FileMode.Open))
                {
                    sys = serializer.ReadObject(fs) as VBSystem;
                }
                if (sys == null)
                    return;

                CurrentSignLicense = sys;
                BackgroundWorker.RunWorkerAsync("SignLicense");
                await ShowDialogAsync(this, DesignNameProgressBar);
            }
            catch
            {
                await Messages.WarningAsync(this, "Warning50028");
            }
        }

        public bool IsEnabledSignLicenseFile()
        {
            if (string.IsNullOrEmpty(LicenseFilePath))
                return false;
            return true;
        }

        public override void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            if(e.Argument.ToString() == "SignLicense" && CurrentSignLicense != null)
            {
                BackgroundWorker.ProgressInfo.OnlyTotalProgress = true;
                BackgroundWorker.ProgressInfo.TotalProgress.ProgressText = ComponentClass.GetText("infoSignLicenseStart").ACCaption;
                BackgroundWorker.ProgressInfo.ReportProgress("", 0, "");
                Thread.Sleep(500);

                string data = CurrentSignLicense.GetChecksumSigned().ToByteStringKey();
                string result = SignLicense(data).Result;
                if (string.IsNullOrEmpty(result))
                {
                    Messages.WarningAsync(this, "Warning50028");
                    return;
                }
                CurrentSignLicense.SystemCommon1 = ByteExtension.FromByteStringKey(result);
                DataContractSerializer serializer = new DataContractSerializer(typeof(VBSystem));
                using (FileStream fs = new FileStream(LicenseFilePath, FileMode.Create))
                {
                    serializer.WriteObject(fs, CurrentSignLicense);
                }
                BackgroundWorker.ProgressInfo.TotalProgress.ProgressText = ComponentClass.GetText("infoSignLicenseComplete").ACCaption;
                BackgroundWorker.ProgressInfo.ReportProgress("", 100, "");
                Thread.Sleep(500);
            }
        }

        private async Task<string> SignLicense(string data)
        {
            string url = string.Format("http://iplus-framework.com/en/VarioBatchDeploy/SignLicenseFile?input={0}", data);
            string responseString;
            try
            {
                using (var client = new HttpClient())
                {
                    var responseGetUserStatus = await client.PostAsync(url, null);
                    responseString = await responseGetUserStatus.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("BSOiPlusLicenseManager", "SignLicense", msg);
                responseString = "";
            }
            return responseString;
        }

        /// <summary>
        /// Opens the dialog to select the license file for signing with gip private key.
        /// </summary>
        [ACMethodInfo("", "en{'...'}de{'...'}", 511)]
        public void BrowseLicenseFile()
        {
            string initialDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            LicenseFilePath = mediaController.OpenFileDialog(false, initialDir, true) ?? LicenseFilePath;
        }

        #endregion

        #region Methods -> Create License Image
        private void CreateLicenseImage()
        {
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            LicenseImage = mediaController.CreateLicenseImage(CurrentVBLicense);
        }

        SKBitmap _LicenseImage;
        // Dont publish System.Drawing.Bitmap because of conflict with System.Windows.Media.Imaging.BitmapImage
        //[ACPropertyInfo(540, "", "en{'LicenseImage'}de{'LicenseImage'}")]
        public SKBitmap LicenseImage
        {
            get
            {
                if (_LicenseImage == null)
                    CreateLicenseImage();
                return _LicenseImage;
            }
            set
            {
                _LicenseImage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #endregion

        #region RemoteLogin

        private string _RemoteUserCode;
        /// <summary>
        /// Gets or sets the code for remote login.
        /// </summary>
        [ACPropertyInfo(512,"", "en{'User code'}de{'Benutzercode'}")]
        public string RemoteUserCode
        {
            get
            {
                return _RemoteUserCode;
            }
            set
            {
                _RemoteUserCode = value;
                OnPropertyChanged("RemoteUserCode");
            }
        }

        private string _RemoteUserKey;
        /// <summary>
        /// Gets or sets the key for remote login.
        /// </summary>
        [ACPropertyInfo(513,"", "en{'User key'}de{'Benutzerschlüssel'}")]
        public string RemoteUserKey
        {
            get
            {
                return _RemoteUserKey;
            }
            set
            {
                _RemoteUserKey = value;
                OnPropertyChanged("RemoteUserKey");
            }
        }

        /// <summary>
        /// Generates the key for remote login.
        /// </summary>
        [ACMethodInfo("", "en{'Generate remote login key'}de{'Remote-Login-Schlüssel generieren'}", 512)]
        public void GenerateRemoteUserKey()
        {
            RemoteUserKey = Root.Environment.License.GenerateRemoteLoginKey(RemoteUserCode, CurrentVBLicense);
        }

        public bool IsEnabledGenerateRemoteUserKey()
        {
            if (string.IsNullOrEmpty(RemoteUserCode) || CurrentVBLicense == null)
                return false;
            return Root.Environment.License.IsEnabledGenerateRemoteUserKey();
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"Save":
                    Save();
                    return true;
                case"IsEnabledSave":
                    result = IsEnabledSave();
                    return true;
                case"UndoSave":
                    UndoSave();
                    return true;
                case"IsEnabledUndoSave":
                    result = IsEnabledUndoSave();
                    return true;
                case"New":
                    New();
                    return true;
                case"IsEnabledNew":
                    result = IsEnabledNew();
                    return true;
                case"Delete":
                    Delete();
                    return true;
                case"Search":
                    Search();
                    return true;
                case"ImportActivationData":
                    ImportActivationData();
                    return true;
                case"IsEnabledImportActivationData":
                    result = IsEnabledImportActivationData();
                    return true;
                case"BrowseActivationDataFile":
                    BrowseActivationDataFile();
                    return true;
                case"ImportDataDialog":
                    ImportDataDialog();
                    return true;
                case"AssignAllPackages":
                    AssignAllPackages();
                    return true;
                case"IsEnabledAssignAllPackages":
                    result = IsEnabledAssignAllPackages();
                    return true;
                case"AssignSelectedPackage":
                    AssignSelectedPackage();
                    return true;
                case"IsEnabledAssignSelectedPackage":
                    result = IsEnabledAssignSelectedPackage();
                    return true;
                case"RemoveAllPackages":
                    RemoveAllPackages();
                    return true;
                case"IsEnabledRemoveAllPackages":
                    result = IsEnabledRemoveAllPackages();
                    return true;
                case"RemoveSelectedPackage":
                    RemoveSelectedPackage();
                    return true;
                case"IsEnabledRemoveSelectedPackage":
                    result = IsEnabledRemoveSelectedPackage();
                    return true;
                case"GenerateLicenseFile":
                    GenerateLicenseFile();
                    return true;
                case"IsEnabledGenerateLicenseFile":
                    result = IsEnabledGenerateLicenseFile();
                    return true;
                case"BrowseLicenseDir":
                    BrowseLicenseDir();
                    return true;
                case"SignLicenseFile":
                    SignLicenseFile();
                    return true;
                case"IsEnabledSignLicenseFile":
                    result = IsEnabledSignLicenseFile();
                    return true;
                case"BrowseLicenseFile":
                    BrowseLicenseFile();
                    return true;
                case"GenerateRemoteUserKey":
                    GenerateRemoteUserKey();
                    return true;
                case"IsEnabledGenerateRemoteUserKey":
                    result = IsEnabledGenerateRemoteUserKey();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
