// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

namespace gip.core.dbsyncer.helper
{
    /// <summary>
    /// provide settings information for exectuting DBSyncer
    /// </summary>
    public static class DbSyncerSettings
    {
        /// <summary>
        /// Name of folder where is located sql scripts
        /// </summary>
        public static string SqlScriptLocation = @"DbScripts";

        /// <summary>
        /// Define default SQL server connection name
        /// </summary>
        public static string DefaultConnectionStringName = "iPlusV5_Entities";

        /// <summary>
        /// Define name of initial sql scripts - builds infrastructure for dbsyncer
        /// </summary>
        public static string InitialSQLScriptName = @"InitialScript.sql";

        /// <summary>
        /// Default iplus context name
        /// </summary>
        public static string IPlusContext = @"varioiplus";

        /// <summary>
        /// examine and decide connection string for default and always present varioiplus context
        /// </summary>
        public static string GetDefaultConnectionString(Configuration configuration, string connectionStringName = null )
        {
            if (connectionStringName == null)
                connectionStringName = gip.core.datamodel.Database.C_DefaultContainerName;

            string defaultConnectionString = "name=" + gip.core.datamodel.Database.C_DefaultContainerName;
            if (configuration != null && configuration.ConnectionStrings != null)
            {
                try
                {
                    ConnectionStringSettings setting = configuration.ConnectionStrings.ConnectionStrings[connectionStringName];
                    defaultConnectionString = setting.ConnectionString;
                    if (!string.IsNullOrEmpty(defaultConnectionString))
                    {
                        defaultConnectionString = defaultConnectionString.Replace(Environment.NewLine, "").Replace("        ", "");
                        //defaultConnectionString = DbSyncerSettings.ConnectionStringRemoveEntityPart(defaultConnectionString);
                    }
                }
                catch (Exception ec)
                {
                    Console.WriteLine(@"Error getting DefaultConnectionString: {0}", ec.Message);
                }
            }
            return defaultConnectionString;
        }

        private static Configuration _ConfigCurrentDir;
        /// <summary>
        /// Search for configuration folder
        /// </summary>
        public static Configuration ConfigCurrentDir
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
                catch (Exception ec)
                {
                    Console.WriteLine(@"Error getting ConfigCurrentDir with OpenMappedExeConfiguration: {0}", ec.Message);
                }

                if (_ConfigCurrentDir == null)
                {
                    try
                    {
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        if (config != null)
                            _ConfigCurrentDir = config;
                    }
                    catch (Exception ec)
                    {
                        Console.WriteLine(@"Error getting ConfigCurrentDir (OpenExeConfiguration): {0}", ec.Message);
                    }
                }

                return _ConfigCurrentDir;
            }
            set
            {
                _ConfigCurrentDir = value;
            }
        }

        /// <summary>
        /// Get file script folder location
        /// </summary>
        /// <returns></returns>
        public static string GetScriptFolderPath(string context, string rootFolder)
        {
            string contextPath = "";
            if (!string.IsNullOrEmpty(context))
            {
                contextPath = context + @"\";
            }
            return rootFolder.TrimEnd('\\') +  @"\" + contextPath;
        }

        /// <summary>
        /// Clean connection string for edmx references - not needed by sql executing
        /// </summary>
        /// <param name="connString"></param>
        /// <returns></returns>
        public static string ConnectionStringRemoveEntityPart(string connString)
        {
            string rawConnString = "";
            Match match = Regex.Match(connString, regexConnectionStringExtract, RegexOptions.IgnoreCase);
            if(match.Success && match.Groups.Count > 2)
            {
                rawConnString = match.Groups[2].Value;
            }
            return rawConnString;
        }

        public static string regexConnectionStringExtract = @"connection string=(["",\',\s,\&quot;]+)(.*)multipleactiveresultsets";
    }
}
