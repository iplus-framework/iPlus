// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="CommandLineHelper.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class CommandLineHelper
    /// </summary>
    public class CommandLineHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineHelper"/> class.
        /// </summary>
        /// <param name="args">The args.</param>
        public CommandLineHelper(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-U" && i + 1 < args.Length)
                    {
                        LoginUser = args[i + 1];
                        i++;
                    }
                    else if (args[i] == "-P" && i + 1 < args.Length)
                    {
                        LoginPassword = args[i + 1];
                        i++;
                    }
                }
            }

            else
            {
                foreach (string arg in args)
                {
                    string kennung = arg.Substring(1, 1).ToUpper();
                    switch (kennung)
                    {
                        case "U":
                            LoginUser = arg.Substring(2);
                            break;
                        case "P":
                            LoginPassword = arg.Substring(2);
                            break;
                    }
                }
            }

        }
        /// <summary>
        /// LoginUser = /U
        /// </summary>
        /// <value>The login user.</value>
        public string LoginUser
        {
            get;
            set;
        }

        /// <summary>
        /// LoginPassword = /P
        /// </summary>
        /// <value>The login password.</value>
        public string LoginPassword
        {
            get;
            set;
        }

        private static Configuration _ConfigCurrentDir;
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
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("CommandLineHelper", "ConfigCurrentDir", msg);
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

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("CommandLineHelper", "ConfigCurrentDir", msg);
                    }
                }

                return _ConfigCurrentDir;
            }
            set
            {
                _ConfigCurrentDir = value;
            }
        }

    }
}
