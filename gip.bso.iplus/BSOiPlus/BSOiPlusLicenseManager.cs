// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.datamodel.Licensing;
using gip.core.licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
                throw new Exception("Licensemanager can only be used by developers that have a DeveloperIssuer license installed");
            }            
#endif
            var result = base.ACInit(startChildMode);
            Search();
            return result;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            if (_AccessPrimary != null)
                await _AccessPrimary.ACDeInit(true);
            _AccessPrimary = null;
            _SelectedAssignedPackage = null;
            _SelectedAvailablePackage = null;
            _RemoteUserCode = null;
            _RemoteUserKey = null;
            _LicenseFileDirPath = null;
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

        private bool _IsLicenseIssued = true;
        /// <summary>
        /// <c>false</c> while a freshly added license has not been issued yet. Such a
        /// record is incomplete (no packages, no license file) and must not be saved.
        /// </summary>
        public bool IsLicenseIssued
        {
            get
            {
                return _IsLicenseIssued;
            }
            private set
            {
                if (_IsLicenseIssued == value)
                    return;
                _IsLicenseIssued = value;
                OnPropertyChanged();
            }
        }

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
                // The fingerprint belongs to the imported activation data of the
                // currently selected record; reset it when switching records.
                CustomerDatabaseFingerprint = null;
                CustomerDatabaseName = null;
                CustomerDatabaseServerName = null;
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

        private string _CustomerDatabaseName;
        /// <summary>
        /// Gets the name of the customer's database as read from the imported
        /// activation data file.
        /// </summary>
        [ACPropertyInfo(516, "", "en{'Customer database'}de{'Datenbank des Kunden'}")]
        public string CustomerDatabaseName
        {
            get
            {
                return _CustomerDatabaseName;
            }
            private set
            {
                _CustomerDatabaseName = value;
                OnPropertyChanged("CustomerDatabaseName");
            }
        }

        private string _CustomerDatabaseServerName;
        /// <summary>
        /// Gets the SQL Server instance of the customer's database as read from the
        /// imported activation data file.
        /// </summary>
        [ACPropertyInfo(517, "", "en{'Customer database server'}de{'Datenbankserver des Kunden'}")]
        public string CustomerDatabaseServerName
        {
            get
            {
                return _CustomerDatabaseServerName;
            }
            private set
            {
                _CustomerDatabaseServerName = value;
                OnPropertyChanged("CustomerDatabaseServerName");
            }
        }

        private string _CustomerDatabaseFingerprint;
        /// <summary>
        /// Gets the fingerprint of the customer's database, derived from the
        /// identification code in the imported activation data file.
        /// </summary>
        /// <remarks>
        /// A product license is issued on the developer's installation but activated
        /// on the customer's database, so it must be bound to the customer's
        /// fingerprint — not to the developer's own one.
        /// </remarks>
        [ACPropertyInfo(518, "", "en{'Customer database fingerprint'}de{'Fingerabdruck der Kundendatenbank'}")]
        public string CustomerDatabaseFingerprint
        {
            get
            {
                return _CustomerDatabaseFingerprint;
            }
            private set
            {
                _CustomerDatabaseFingerprint = value;
                OnPropertyChanged("CustomerDatabaseFingerprint");
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

        #region Properties -> Developer key

        private string _DeveloperPrivateKeyPath;
        /// <summary>
        /// Gets or sets the path of the developer's private key (PEM) that is used
        /// to sign product licenses for the developer's own packages.
        /// </summary>
        /// <remarks>
        /// In V5 there is no developer dongle any more. The developer keeps the
        /// private key locally; the matching public key is embedded in the
        /// <c>DeveloperIssuer</c> license that gip signed for the developer, so end
        /// customers can verify the product license offline.
        /// </remarks>
        [ACPropertyInfo(514, "", "en{'Developer private key (*.pem)'}de{'Privater Schlüssel des Entwicklers (*.pem)'}")]
        public string DeveloperPrivateKeyPath
        {
            get
            {
                return _DeveloperPrivateKeyPath;
            }
            set
            {
                _DeveloperPrivateKeyPath = value;
                OnPropertyChanged("DeveloperPrivateKeyPath");
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
        public async Task Save()
        {
            await OnSave();
        }

        /// <summary>
        /// Determines whether [is enabled save].
        /// </summary>
        /// <returns><c>true</c> if [is enabled save]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledSave()
        {
            return IsLicenseIssued ? OnIsEnabledSave() : false;
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
            return OnIsEnabledUndoSave();
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
            VBLicense license = VBLicense.NewACObject(Db, null, secondaryKey);
            // These columns are NOT NULL in the database but only carry V4 key material.
            // Signed V5 license files don't use them, so they stay empty.
            license.PackageSystem = Array.Empty<byte>();
            license.PackageSystem1 = string.Empty;
            license.SystemRemote = string.Empty;
            license.SystemCommon = Array.Empty<byte>();
            CurrentVBLicense = license;
            AccessPrimary.NavList.Add(CurrentVBLicense);
            IsLicenseIssued = false;
        }

        /// <summary>
        /// A license that was added but never issued is incomplete. Closing the
        /// dialog discards it instead of asking to save it.
        /// </summary>
        public override async Task<bool> ACSaveOrUndoChanges()
        {
            if (!IsLicenseIssued)
            {
                DiscardUnissuedLicense();
                return true;
            }
            return await base.ACSaveOrUndoChanges();
        }

        protected override bool OnUndoSave()
        {
            bool done = base.OnUndoSave();
            if (done && !IsLicenseIssued)
                DiscardUnissuedLicense();
            return done;
        }

        private void DiscardUnissuedLicense()
        {
            if (Database.IsChanged)
                ACUndoChanges();
            IsLicenseIssued = true;
            if (AccessPrimary != null)
            {
                AccessPrimary.Current = null;
                AccessPrimary.NavSearch(Db);
            }
            OnPropertyChanged("CurrentVBLicense");
            OnPropertyChanged("VBLicenseList");
            LoadAvailableAssignedPackages();
            DbChangeCount++;
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
        public async Task BrowseActivationDataFile()
        {
            string initialDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            ActivationDataPath = await mediaController.OpenFileDialog(false, initialDir, true) ?? ActivationDataPath;
        }

        public bool IsEnabledBrowseActivationDataFile()
        {
            if (CurrentVBLicense == null)
                return false;
            return true;
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

                        case "db":
                            CustomerDatabaseName = node.InnerText;
                            break;

                        case "ds":
                            CustomerDatabaseServerName = node.InnerText;
                            break;

                        case "code":
                            UniqueCustomerCode = node.InnerText;
                            CustomerDatabaseFingerprint = DatabaseFingerprint.FromLegacyChecksumCode(node.InnerText);
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

        public bool IsEnabledImportDataDialog()
        {
            if (CurrentVBLicense == null || String.IsNullOrEmpty(ActivationDataPath))
                return false;
            return true;
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
            
            if (string.IsNullOrEmpty(CurrentVBLicense.ReadablePackages))
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
        /// <remarks>
        /// The product license is issued as a signed <c>*.gip</c> file with the
        /// developer's own private key (<see cref="DeveloperPrivateKeyPath"/>).
        /// The V4 XML license format is no longer generated: the V5 verifier rejects it.
        /// </remarks>
        [ACMethodInfo("", "en{'Generate license file'}de{'Lizenzdatei erzeugen'}", 508,true)]
        public void GenerateLicenseFile()
        {
            string keyError = ValidateDeveloperPrivateKey();
            if (keyError != null)
            {
                Messages.WarningAsync(this, keyError, true);
                return;
            }

            List<string> packages = new List<string>();
            if (AssignedPackages != null && AssignedPackages.Any())
                packages.AddRange(AssignedPackages);

            CurrentVBLicense.PackageSystem = Encoding.UTF8.GetBytes(CreatePackagesString(packages));

            var system = CurrentVBLicense;

            string outputPath = Path.Combine(LicenseFileDirPath, CurrentVBLicense.CustomerName + "-license" + LicenseFormat.FileExtension);
            string error;
            if (Root.Environment.License.IssueProductLicenseFile(
                    File.ReadAllText(DeveloperPrivateKeyPath),
                    CurrentVBLicense.CustomerName,
                    CurrentVBLicense.ProjectNo,
                    packages,
                    outputPath,
                    CustomerDatabaseFingerprint,
                    out error))
            {
                CurrentVBLicense = system;
                ACSaveChanges();
                Messages.InfoAsync(this, "Info50025");
                IsLicenseIssued = true;
                DbChangeCount++;
                return;
            }

            Messages.LogException("BSOiPlusLicenseManager", "GenerateLicenseFile", error);
            Messages.WarningAsync(this, error, true);
        }

        /// <summary>
        /// Checks that <see cref="DeveloperPrivateKeyPath"/> points to a private key
        /// (PEM or legacy XML) and not, for example, to a license file.
        /// </summary>
        /// <returns>An error text, or <c>null</c> when the key file looks usable.</returns>
        private string ValidateDeveloperPrivateKey()
        {
            if (string.IsNullOrEmpty(DeveloperPrivateKeyPath) || !File.Exists(DeveloperPrivateKeyPath))
                return "The developer private key file was not found: " + DeveloperPrivateKeyPath;

            string text = File.ReadAllText(DeveloperPrivateKeyPath);
            bool isPrivateKey = text.Contains("PRIVATE KEY") || text.Contains("<RSAKeyValue");
            if (!isPrivateKey)
            {
                return "'" + Path.GetFileName(DeveloperPrivateKeyPath) + "' is not a private key file. Select the developer's private key (e.g. 'iPlusMES-private.pem'), not a license file.";
            }
            return null;
        }

        public bool IsEnabledGenerateLicenseFile()
        {
            if (CurrentVBLicense == null || string.IsNullOrEmpty(CurrentVBLicense.CustomerName) || string.IsNullOrEmpty(LicenseFileDirPath))
                return false;
            // The product license is signed with the developer's private key and bound
            // to the customer's database (imported from the activation data). Without
            // the fingerprint it would be bound to the developer's own database and
            // could never be activated at the customer.
            if (string.IsNullOrEmpty(DeveloperPrivateKeyPath) || string.IsNullOrEmpty(CustomerDatabaseFingerprint))
                return false;
            return Root.Environment.License.IsEnabledGenerateLicenseFile();
        }

        /// <summary>
        /// Opens the dialog to select the directory in which the license file will be saved.
        /// </summary>
        [ACMethodInfo("","en{'...'}de{'...'}",509)]
        public async Task BrowseLicenseDir()
        {
            string initialDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            LicenseFileDirPath = await mediaController.OpenFileDialog(true, initialDir, true) ?? LicenseFileDirPath;
        }

        public bool IsEnabledBrowseLicenseDir()
        {
            return CurrentVBLicense != null;
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

        #region Methods -> Developer key

        /// <summary>
        /// Opens the dialog to select the developer's private key file (PEM).
        /// </summary>
        [ACMethodInfo("", "en{'...'}de{'...'}", 515)]
        public async Task BrowseDeveloperPrivateKey()
        {
            string initialDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            DeveloperPrivateKeyPath = await mediaController.OpenFileDialog(false, initialDir, true) ?? DeveloperPrivateKeyPath;
        }

        public bool IsEnabledBrowseDeveloperPrivateKey()
        {
            return CurrentVBLicense != null;
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
                case nameof(Save):
                    result = Save();
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
                case nameof(Search):
                    Search();
                    return true;
                case nameof(ImportActivationData):
                    ImportActivationData();
                    return true;
                case nameof(IsEnabledImportActivationData):
                    result = IsEnabledImportActivationData();
                    return true;
                case nameof(IsEnabledBrowseActivationDataFile):
                    result = IsEnabledBrowseActivationDataFile();
                    return true;
                case nameof(BrowseActivationDataFile):
                    result = BrowseActivationDataFile();
                    return true;
                case nameof(ImportDataDialog):
                    ImportDataDialog();
                    return true;
                case nameof(AssignAllPackages):
                    AssignAllPackages();
                    return true;
                case nameof(IsEnabledAssignAllPackages):
                    result = IsEnabledAssignAllPackages();
                    return true;
                case nameof(AssignSelectedPackage):
                    AssignSelectedPackage();
                    return true;
                case nameof(IsEnabledAssignSelectedPackage):
                    result = IsEnabledAssignSelectedPackage();
                    return true;
                case nameof(RemoveAllPackages):
                    RemoveAllPackages();
                    return true;
                case nameof(IsEnabledRemoveAllPackages):
                    result = IsEnabledRemoveAllPackages();
                    return true;
                case nameof(RemoveSelectedPackage):
                    RemoveSelectedPackage();
                    return true;
                case nameof(IsEnabledRemoveSelectedPackage):
                    result = IsEnabledRemoveSelectedPackage();
                    return true;
                case nameof(GenerateLicenseFile):
                    GenerateLicenseFile();
                    return true;
                case nameof(IsEnabledGenerateLicenseFile):
                    result = IsEnabledGenerateLicenseFile();
                    return true;
                case nameof(BrowseLicenseDir):
                    result = BrowseLicenseDir();
                    return true;
                case nameof(IsEnabledBrowseLicenseDir):
                    result = IsEnabledBrowseLicenseDir();
                    return true;
                case nameof(BrowseDeveloperPrivateKey):
                    result = BrowseDeveloperPrivateKey();
                    return true;
                case nameof(IsEnabledBrowseDeveloperPrivateKey):
                    result = IsEnabledBrowseDeveloperPrivateKey();
                    return true;
                case nameof(GenerateRemoteUserKey):
                    GenerateRemoteUserKey();
                    return true;
                case nameof(IsEnabledGenerateRemoteUserKey):
                    result = IsEnabledGenerateRemoteUserKey();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public override IEnumerable<string> GetPropsToObserveForIsEnabled(string acMethodName)
        {
            switch (acMethodName)
            {
                case nameof(IsEnabledSave):
                case nameof(Save):
                    return new string[] { nameof(DbChangeCount), nameof(IsLicenseIssued) };
                case nameof(IsEnabledUndoSave):
                case nameof(UndoSave):
                    return new string[] { nameof(DbChangeCount) };
                case nameof(IsEnabledNew):
                case nameof(New):
                    return new string[] { nameof(InitState) };
                case nameof(IsEnabledImportActivationData):
                case nameof(ImportActivationData):
                    return new string[] { nameof(CurrentVBLicense) };
                case nameof(IsEnabledAssignAllPackages):
                case nameof(AssignAllPackages):
                    return new string[] { nameof(AvailablePackages) };
                case nameof(IsEnabledAssignSelectedPackage):
                case nameof(AssignSelectedPackage):
                    return new string[] { nameof(SelectedAvailablePackage) };
                case nameof(IsEnabledRemoveAllPackages):
                case nameof(RemoveAllPackages):
                    return new string[] { nameof(AssignedPackages) };
                case nameof(IsEnabledRemoveSelectedPackage):
                case nameof(RemoveSelectedPackage):
                    return new string[] { nameof(SelectedAssignedPackage) };
                case nameof(IsEnabledGenerateLicenseFile):
                case nameof(GenerateLicenseFile):
                    return new string[] { nameof(CurrentVBLicense), nameof(LicenseFileDirPath), nameof(DeveloperPrivateKeyPath), nameof(CustomerDatabaseFingerprint) };
                case nameof(IsEnabledGenerateRemoteUserKey):
                case nameof(GenerateRemoteUserKey):
                    return new string[] { nameof(RemoteUserCode), nameof(CurrentVBLicense) };
                case nameof(ImportDataDialog):
                    return new string[] { nameof(CurrentVBLicense), nameof(ActivationDataPath) };
                case nameof(IsEnabledBrowseActivationDataFile):
                case nameof(BrowseActivationDataFile):
                case nameof(IsEnabledBrowseLicenseDir):
                case nameof(BrowseLicenseDir):
                case nameof(IsEnabledBrowseDeveloperPrivateKey):
                case nameof(BrowseDeveloperPrivateKey):
                    return new string[] { nameof(CurrentVBLicense) };
                default:
                    return base.GetPropsToObserveForIsEnabled(acMethodName);
            }
        }

        #endregion
    }
}
