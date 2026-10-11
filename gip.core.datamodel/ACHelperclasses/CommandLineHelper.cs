// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
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
using System.Runtime.Serialization;

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
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //{
            //    for (int i = 0; i < args.Length; i++)
            //    {
            //        if (args[i].ToLower() == "-u" && i + 1 < args.Length)
            //        {
            //            LoginUser = args[i + 1];
            //            i++;
            //        }
            //        else if (args[i].ToLower() == "-p" && i + 1 < args.Length)
            //        {
            //            LoginPassword = args[i + 1];
            //            i++;
            //        }
            //    }
            //}

            //else
            //{
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
            //}

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
                Console.WriteLine("ConfigCurrentDir: getter entered, CurrentDirectory=" + Environment.CurrentDirectory + " BaseDirectory=" + AppContext.BaseDirectory);
                ExeConfigurationFileMap configFile = new ExeConfigurationFileMap();
                configFile.ExeConfigFilename = Path.Combine(Environment.CurrentDirectory, "vbiplus.config");
                Console.WriteLine("ConfigCurrentDir: OpenMappedExeConfiguration for " + configFile.ExeConfigFilename);
                try
                {
                    Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None);
                    Console.WriteLine("ConfigCurrentDir: OpenMappedExeConfiguration returned, HasFile=" + config.HasFile);
                    if (config != null && config.HasFile)
                        _ConfigCurrentDir = config;
                }
                catch (Exception e)
                {
                    Console.WriteLine("ConfigCurrentDir: OpenMappedExeConfiguration failed. " + e.GetType().Name + ": " + e.Message + " :: " + e.StackTrace);
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("CommandLineHelper", "ConfigCurrentDir", msg);
                }

                if (_ConfigCurrentDir == null)
                {
                    Console.WriteLine("ConfigCurrentDir: falling back to OpenExeConfiguration");
                    try
                    {
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        Console.WriteLine("ConfigCurrentDir: OpenExeConfiguration returned, HasFile=" + config.HasFile);
                        if (config != null)
                            _ConfigCurrentDir = config;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ConfigCurrentDir: OpenExeConfiguration failed. " + e.GetType().Name + ": " + e.Message + " :: " + e.StackTrace);
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("CommandLineHelper", "ConfigCurrentDir", msg);
                    }
                }

                Console.WriteLine("ConfigCurrentDir: returning " + (_ConfigCurrentDir != null ? "config" : "null"));
                return _ConfigCurrentDir;
            }
            set
            {
                _ConfigCurrentDir = value;
            }
        }

        private static string _SettingsPath = Path.Combine(AppContext.BaseDirectory, "settingsiPlus.config");

        /// <summary>
        /// Optional storage backend for the settings (e.g. browser localStorage via JS interop).
        /// When set, <see cref="Settings"/> are loaded from this backend instead of the local
        /// file. Must return the serialized <see cref="ACValueItemList"/> XML or null if
        /// nothing is stored yet.
        /// </summary>
        public static Func<string> LoadSettingsFromStorage = null;

        /// <summary>
        /// Optional storage backend for the settings (e.g. browser localStorage via JS interop).
        /// When set, <see cref="SaveSettings"/> persists the serialized
        /// <see cref="ACValueItemList"/> XML to this backend instead of the local file.
        /// </summary>
        public static Action<string> SaveSettingsToStorage = null;

        private static ACValueItemList _Settings;
        public static ACValueItemList Settings
        {
            get
            {
                if (_Settings != null)
                    return _Settings;

                if (LoadSettingsFromStorage != null)
                {
                    try
                    {
                        string xml = LoadSettingsFromStorage();
                        if (!String.IsNullOrEmpty(xml))
                        {
                            DataContractSerializer serializer = new DataContractSerializer(typeof(ACValueItemList));
                            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
                            {
                                _Settings = serializer.ReadObject(ms) as ACValueItemList;
                            }
                            if (_Settings != null)
                                return _Settings;
                        }
                    }
                    catch (Exception)
                    {
                        // ignore — fall back to an empty list
                    }
                }

                if (File.Exists(_SettingsPath))
                {
                    try
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(ACValueItemList));
                        using (FileStream fs = new FileStream(_SettingsPath, FileMode.Open))
                        {
                            _Settings = serializer.ReadObject(fs) as ACValueItemList;
                        }
                        return _Settings;
                    }
                    catch(Exception)
                    {
                        // ignore
                    }
                }
                _Settings = new ACValueItemList();
                return _Settings;

            }
        }

        public static void SaveSettings()
        {
            if (_Settings == null)
                return;
            DataContractSerializer serializer = new DataContractSerializer(typeof(ACValueItemList));
            if (SaveSettingsToStorage != null)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        serializer.WriteObject(ms, _Settings);
                        SaveSettingsToStorage(Encoding.UTF8.GetString(ms.ToArray()));
                    }
                }
                catch (Exception)
                {
                    // ignore — storage backend may be unavailable
                }
                return;
            }
            using (FileStream fs = new FileStream(_SettingsPath, FileMode.Create))
            {
                serializer.WriteObject(fs, _Settings);
            }
        }
    }

}
