using System;
using System.Configuration;
using System.IO;

namespace gip.core.ControlScriptSync.sql
{
    public class ConnectionSettings
    {
        #region Settings

        #endregion

        private Configuration _ConfigCurrentDir;
        public Configuration ConfigCurrentDir
        {
            get
            {
                if (_ConfigCurrentDir != null)
                    return _ConfigCurrentDir;
                ExeConfigurationFileMap configFile = new ExeConfigurationFileMap();
                configFile.ExeConfigFilename = Path.Combine(Environment.CurrentDirectory, "vbiplus.config");
                try
                {
                    Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None);
                    if (config != null && config.HasFile)
                        _ConfigCurrentDir = config;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBSQLCommand", "ConfigCurrentDir", msg);
                }

                if (_ConfigCurrentDir == null)
                {
                    try
                    {
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        if (config != null)
                            _ConfigCurrentDir = config;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("VBSQLCommand", "ConfigCurrentDir(10)", msg);
                    }
                }

                return _ConfigCurrentDir;
            }
        }


        public string _DefaultConnectionString;

        public string DefaultConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(_DefaultConnectionString))
                    return _DefaultConnectionString;
                _DefaultConnectionString = "name=" + gip.core.datamodel.Database.C_DefaultContainerName;
                if (ConfigCurrentDir != null && ConfigCurrentDir.ConnectionStrings != null)
                {
                    try
                    {
                        ConnectionStringSettings setting = ConfigCurrentDir.ConnectionStrings.ConnectionStrings[gip.core.datamodel.Database.C_DefaultContainerName];
                        _DefaultConnectionString = setting.ConnectionString;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("VBSQLCommand", "DefaultConnectionString", msg);
                    }
                }
                return _DefaultConnectionString;
            }
            set
            {
                _DefaultConnectionString = value;
            }
        }
    }
}
