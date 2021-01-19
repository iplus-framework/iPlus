using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.ControlScriptSync.sql
{
    /// <summary>
    /// Read static sql resoruces stored into dll (file option: Embedded resource, do not copy)
    /// </summary>
    public class VBSQLResource
    {
        public static string Insert
        { 
            get
            {
                return Read("gip.core.ControlScriptSync.sql.insert.sql");
            }
        }

        public static string Create
        {
            get
            {
                return Read("gip.core.ControlScriptSync.sql.creation.sql");
            }
        }

        public static string MaxVersion
        {
            get
            {
                return Read("gip.core.ControlScriptSync.sql.maxversion.sql");
            }
        }

        public static string AllVersions
        {
            get
            {
                return Read("gip.core.ControlScriptSync.sql.allversions.sql");
            }
        }

        public static string ExistVersion
        {
            get
            {
                return Read("gip.core.ControlScriptSync.sql.existverision.sql");
            }
        }

        private static string Read(string resourceName)
        {
            string result = "";
            var assembly = Assembly.GetAssembly(typeof(VBSQLResource));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
    }
}
