using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gip.tool.installerAndUpdater
{
    public class SqlConnectionInfo : INotifyPropertyChanged
    {
        public string ServerName
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

        public string DatabaseName
        {
            get;
            set;
        }

        public SqlConfigurationState ConnectionState
        {
            get;
            set;
        }

        private SqlServerLocation _ServerLocation;
        public SqlServerLocation ServerLocation
        {
            get
            {
                return _ServerLocation;
            }
            set
            {
                _ServerLocation = value;
                OnPropertyChanged("ServerLocation");
            }
        }

        private string _ServerBackupPath;
        public string ServerBackupPath
        {
            get
            {
                return _ServerBackupPath;
            }
            set
            {
                _ServerBackupPath = value;
                OnPropertyChanged("ServerBackupPath");
            }
        }

        public string ConnectionStrings
        {
            get
            {
                string sqlConnectionString = "";
                if (this.AuthMode == 0 && !string.IsNullOrEmpty(this.ServerName))
                    sqlConnectionString = String.Format("Integrated Security=SSPI;Persist Security Info=True; MultipleActiveResultSets=True; Initial Catalog=master;Data Source={0}", this.ServerName);

                else if (this.AuthMode == 1 && !string.IsNullOrEmpty(this.ServerName) && !string.IsNullOrEmpty(this.Username)
                                                      && !string.IsNullOrEmpty(this.Password))
                    sqlConnectionString = String.Format(@"Integrated Security=False;Persist Security Info=True; MultipleActiveResultSets=True; user id={0};password={1};Initial Catalog=master;Data Source={2}",
                                                        this.Username, this.Password, this.ServerName);
                return sqlConnectionString;
            }
        }

        public bool CheckServerLocationAndBackupPath()
        {
            SqlConnection conn = new SqlConnection(ConnectionStrings);
            
            try
            {
                conn.Open();
            }
            catch
            {
                return false;
            }

            try
            {
                ServerBackupPath = GetSqlServerDefaultBackupDirectory(conn);
            }
            catch
            {
                conn.Close();
                return false;
            }

            string script = "SELECT SERVERPROPERTY ('MachineName')";
            try
            {
                SqlCommand comm = new SqlCommand(script, conn);
                var result = comm.ExecuteScalar();
                comm.Dispose();
                if (System.Environment.MachineName == result.ToString())
                    ServerLocation = SqlServerLocation.Local;
                else
                    ServerLocation = SqlServerLocation.Remote;
            }
            catch
            {
                conn.Close();
                ServerLocation = SqlServerLocation.Remote;
            }
            conn.Close();
            return true;
        }

        private string GetSqlServerDefaultBackupDirectory(SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand("EXEC  master.dbo.xp_instance_regread  N'HKEY_LOCAL_MACHINE', N'Software\\Microsoft\\MSSQLServer\\MSSQLServer',N'BackupDirectory'",
                                            conn);
            SqlDataReader myDataReader = cmd.ExecuteReader();
            cmd.Dispose();
            myDataReader.Read();
            return myDataReader.GetString(1);
        }

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
