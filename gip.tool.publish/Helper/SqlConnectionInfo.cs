// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.tool.publish
{
    public class SqlConnectionInfo
    {
        public SqlConnectionInfo(string serverName, string databaseName, int authMode, string userName, string pass)
        {
            ServerName = serverName;
            DatabaseName = databaseName;
            AuthMode = authMode;
            Username = userName;
            Password = pass;
        }

        public string ServerName
        {
            get;
            set;
        }

        public string DatabaseName
        {
            get;
            set;
        }

        public int AuthMode
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public string ConnectionStrings
        {
            get
            {
                string sqlConnectionString = "";
                if (this.AuthMode == 0 && !string.IsNullOrEmpty(this.ServerName))
                    sqlConnectionString = String.Format("Integrated Security=SSPI;Persist Security Info=True; Initial Catalog=master;Data Source={0}", this.ServerName);

                else if (this.AuthMode == 1 && !string.IsNullOrEmpty(this.ServerName) && !string.IsNullOrEmpty(this.Username)
                                                      && !string.IsNullOrEmpty(this.Password))
                    sqlConnectionString = String.Format(@"Integrated Security=False;Persist Security Info=True;user id={0};password={1};Initial Catalog=master;Data Source={2}",
                                                        this.Username, this.Password, this.ServerName);
                return sqlConnectionString;
            }
        }
    }
}
