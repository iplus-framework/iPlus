using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using gip.core.media;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Encryption of connection string'}de{'Verschlüsselung Verbindungszeichenfolge'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
    public class BSOConnectionStringProtection : ACBSO
    {
        #region ctor's

        public BSOConnectionStringProtection(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            AppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            AppConfig = null;
            return await base.ACDeInit(deleteACClassTask);
        }
        #endregion

        private Configuration _AppConfig;
        public Configuration AppConfig
        {
            get 
            {
                return _AppConfig;
            }
            set 
            {
                _AppConfig = value;
                if (_AppConfig != null)
                {
                    AppConfigPath = _AppConfig.FilePath;
                    OnPropertyChanged("AppConfigPath");
                    IsProtected = _AppConfig.ConnectionStrings.SectionInformation.IsProtected;
                    OnPropertyChanged("IsProtected");
                }
            }
        }

        private string _AppConfigPath;
        /// <summary>
        /// Gets or sets the path of the app.config file. 
        /// </summary>
        [ACPropertyInfo(401,"ConnectionString","en{'Path of *.config file'}de{'Pfad zur *.config Datei'}","", false)]
        public string AppConfigPath
        {
            get
            {
                return _AppConfigPath;
            }
            set
            {
                _AppConfigPath = value;
                OnPropertyChanged("AppConfigPath");
            }
        }

        private bool _IsProtected;
        /// <summary>
        /// Gets or sets the IsProtected property.
        /// When the value is true on this property, connection strings are encrypted
        /// When the value is false on this property, connection strings are human-readable
        /// </summary>
        [ACPropertyInfo(402, "ConnectionString", "en{'Connection string is encrypted'}de{'Verbindungszeichenfolge ist verschlüsselt'}")]
        public bool IsProtected
        {
            get
            {
                return _IsProtected;
            }
            set
            {
                _IsProtected = value;
                OnPropertyChanged("IsProtected");
            }
        }

        /// <summary>
        /// Shows the dialog to select the app.config file. 
        /// </summary>
        [ACMethodInfo("ConnectionString", "en{'...'}de{'...'}", 401, false, false, true)]
        public void SelectConfigFile()
        {
            ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
            string configPath = mediaController.OpenFileDialog(false, AppDomain.CurrentDomain.BaseDirectory, true, ".config", new Dictionary<string, string>() { { "Config file", ".config" } });
            if (configPath != null && configPath.ToLower().Contains(".config"))
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = configPath;
                AppConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            }
            else
            {
                AppConfig = null;
                AppConfigPath = "";
            }
        }

        /// <summary>
        /// Encrpyts the connection strings in the app.config file. 
        /// </summary>
        [ACMethodInfo("ConnectionStrings", "en{'Encrypt'}de{'Verschlüsseln'}", 402)]
        public void EncryptConfig()
        {
            if (AppConfig != null && AppConfig.ConnectionStrings != null && !AppConfig.ConnectionStrings.SectionInformation.IsProtected)
            {
                AppConfig.ConnectionStrings.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                AppConfig.ConnectionStrings.SectionInformation.ForceSave = true;
                AppConfig.Save(ConfigurationSaveMode.Modified);
                AppConfigPath = AppConfig.FilePath;
                IsProtected = AppConfig.ConnectionStrings.SectionInformation.IsProtected;
            }
        }

        /// <summary>
        /// Decrpyts the connection strings in the app.config file. 
        /// </summary>
        [ACMethodInfo("ConnectionStrings", "en{'Decrypt'}de{'Entschlüsseln'}", 403)]
        public void DecryptConfig()
        {
            if (AppConfig != null && AppConfig.ConnectionStrings != null && AppConfig.ConnectionStrings.SectionInformation.IsProtected)
            {
                AppConfig.ConnectionStrings.SectionInformation.UnprotectSection();
                AppConfig.ConnectionStrings.SectionInformation.ForceSave = true;
                AppConfig.Save(ConfigurationSaveMode.Modified);
                AppConfigPath = AppConfig.FilePath;
                IsProtected = AppConfig.ConnectionStrings.SectionInformation.IsProtected;
            }
        }

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"SelectConfigFile":
                    SelectConfigFile();
                    return true;
                case"EncryptConfig":
                    EncryptConfig();
                    return true;
                case"DecryptConfig":
                    DecryptConfig();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }
}
