using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.iplus.startup
{
    public class ScriptFile
    {
        public string DeveloperName
        {
            get;
            set;
        }

        public DateTime ScriptDate
        {
            get;
            set;
        }

        public ScriptMode InstallScriptMode
        {
            get;
            set;
        }

        public string ScriptSourceMethod
        {
            get;
            set;
        }

        public string MethodName
        {
            get;
            set;
        }

        /// <summary>
        /// Parses the file name and reads the file(script) content.
        /// </summary>
        /// <param name="scriptFilePath">The script file path.</param>
        /// <returns>The script file instance if is file name successfully parsed and file readed, otherwise false</returns>
        public static ScriptFile InitializeScriptFile(string scriptFilePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(scriptFilePath);
            string[] fileNameParts = fileName.Split('_');
            if(fileNameParts.Count() != 3)
            {
                Console.WriteLine(string.Format("The script file name {0} isn't correct!", fileName));
                Console.Read();
                throw new Exception();
            }

            DateTime scriptDateTime; 
            if(!DateTime.TryParseExact(fileNameParts[1], "yyyy-MM-dd HH-mm", CultureInfo.CurrentCulture, DateTimeStyles.None, out scriptDateTime))
            {
                Console.WriteLine(string.Format("The date and time in the script file name {0} is't correct!", fileName));
                Console.Read();
                throw new Exception();
            }

            ScriptMode scriptMode;
            string scriptModeName = fileNameParts[2];
            if (scriptModeName.ToLower() == "pre")
                scriptMode = ScriptMode.PreStartup;
            else if (scriptModeName.ToLower() == "post")
                scriptMode = ScriptMode.PostStartup;
            else
            {
                Console.WriteLine(string.Format("The script mode in the script file name {0} is't correct! Expected mode is 'pre' or 'post'", fileName));
                Console.Read();
                throw new Exception();
            }

            string scriptContent = "";
            try
            {
                scriptContent = File.ReadAllText(scriptFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("File read exception: {0} {2} ScriptFile:{1}", e.Message, fileName, System.Environment.NewLine));
                Console.Read();
                throw new Exception();
            }

            if(string.IsNullOrEmpty(scriptContent))
            {
                Console.WriteLine(string.Format("The script content is empty! Script file: {0}", fileName));
                Console.Read();
                throw new Exception();
            }

            string methodName = GetMethodName(scriptContent);
            string methodNameNew = methodName + fileName.Replace(" ", "").Replace("-","");
            scriptContent = scriptContent.Replace(methodName, methodNameNew);

            return new ScriptFile() { DeveloperName = fileNameParts[0], ScriptDate = scriptDateTime, InstallScriptMode = scriptMode,
                                      ScriptSourceMethod = scriptContent, MethodName = methodNameNew };
        }

        private static string GetMethodName(string content)
        {
            string startString = "public bool ";
            int start = content.IndexOf(startString) + startString.Length;
            int end = content.IndexOf("(string");

            string methodName = content.Substring(start, end - start);
            methodName = methodName.Trim();
            return methodName;
        }
    }

    public enum ScriptMode : short
    {
        PreStartup = 0,
        PostStartup = 10
    }
}
