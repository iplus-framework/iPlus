// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace gip.tool.publish
{
    [DataContract]
    public class UserData : INotifyPropertyChanged
    {
        [DataMember]
        public string UserDataName
        {
            get;
            set;
        }

        [IgnoreDataMember]
        private string _FtpServerHost;
        [DataMember]
        public string FtpServerHost
        {
            get
            {
                return _FtpServerHost;
            }
            set
            {
                _FtpServerHost = value;
                OnPropertyChanged("FtpServerHost");
            }
        }

        [IgnoreDataMember]
        private string _FtpUserName;
        [DataMember]
        public string FtpUserName
        {
            get
            {
                return _FtpUserName;
            }
            set
            {
                _FtpUserName = value;
                OnPropertyChanged("FtpUserName");
            }
        }

        [IgnoreDataMember]
        private string _FtpPassword;
        [DataMember]
        public string FtpPassword
        {
            get
            {
                return _FtpPassword;
            }
            set
            {
                _FtpPassword = value;
                OnPropertyChanged("FtpPassword");
            }
        }

        [IgnoreDataMember]
        private int _VersionControl;
        [DataMember]
        public int VersionControl
        {
            get
            {
                return _VersionControl;
            }
            set
            {
                _VersionControl = value;
                OnPropertyChanged("VersionControl");
            }
        }

        [IgnoreDataMember]
        private string _VersionControlServer;
        [DataMember]
        public string VersionControlServer
        {
            get
            {
                return _VersionControlServer;
            }
            set
            {
                _VersionControlServer = value;
                OnPropertyChanged("VersionControlServer");
            }
        }

        [IgnoreDataMember]
        private string _VersionControlDeployFilePath;
        [DataMember]
        public string VersionControlDeployFilePath
        {
            get
            {
                return _VersionControlDeployFilePath;
            }
            set
            {
                _VersionControlDeployFilePath = value;
                OnPropertyChanged("VersionControlDeployFilePath");
            }
        }

        [IgnoreDataMember]
        private string _BinFolderPath;
        [DataMember]
        public string BinFolderPath
        {
            get
            {
                return _BinFolderPath;
            }
            set
            {
                _BinFolderPath = value;
                OnPropertyChanged("BinFolderPath");
            }
        }

        [IgnoreDataMember]
        private string _SqlScriptFilePath;
        [DataMember]
        public string SqlScriptFilePath
        {
            get
            {
                return _SqlScriptFilePath;
            }
            set
            {
                _SqlScriptFilePath = value;
                OnPropertyChanged("SqlScriptFilePath");
                OnPropertyChanged("DatabaseFileName");
            }
        }

        [IgnoreDataMember]
        private string _FtpPublishPath;
        [DataMember]
        public string FtpPublishPath
        {
            get
            {
                return _FtpPublishPath;
            }
            set
            {
                _FtpPublishPath = value;
                OnPropertyChanged("FtpPublishPath");
            }
        }

        [IgnoreDataMember]
        private string _ApplicationFileName;
        [DataMember]
        public string ApplicationFileName
        {
            get
            {
                return _ApplicationFileName;
            }
            set
            {
                _ApplicationFileName = value;
                if (_ApplicationFileName != null && !_ApplicationFileName.EndsWith(".zip"))
                    _ApplicationFileName = _ApplicationFileName + ".zip";
                OnPropertyChanged("ApplicationFileName");
            }
        }

        [IgnoreDataMember]
        public string DatabaseFileName
        {
            get
            {
                string name = "";
                if (!string.IsNullOrEmpty(SqlScriptFilePath))
                    name = SqlScriptFilePath.Split('\\').LastOrDefault().Replace("sql", "zip");
                return name;
            }
            set
            {
                //_DatabaseFileName = value;
                //if (_DatabaseFileName != null && !_DatabaseFileName.EndsWith(".zip"))
                //    _DatabaseFileName = _DatabaseFileName + ".zip";
                //OnPropertyChanged("DatabaseFileName");
            }
        }

        [IgnoreDataMember]
        public string ChangeLogMessage
        {
            get;
            set;
        }

        [IgnoreDataMember]
        public string ChangeLogMessageDE
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
