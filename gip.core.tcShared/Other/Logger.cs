using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace gip.core.tcShared
{
    public class Logger
    {
        public Logger()
        {
            string path = ConfigurationManager.AppSettings["LogPath"].ToString();
            if (!string.IsNullOrEmpty(path))
                _Path = path;
            
        }

        MonitorObject _lockLog2000 = new MonitorObject(2000);

        private string _Path
        {
            get;
            set;
        }

        private string _fileName = "ADSAgent-Log";
        private string _FileName
        {
            get
            {
                return _fileName + DateTime.Now.Month + "_" + DateTime.Now.Year+".txt";
            }
            
        }

        public void Log(string message)
        {
            lock (_lockLog2000)
            {
                using (StreamWriter streamWritter = File.AppendText(_Path + _FileName))
                {
                    streamWritter.WriteLine(DateTime.Now + "  " + message);
                }
            }
        }
    }
}
