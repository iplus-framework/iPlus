using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace gip.core.autocomponent
{
    public class ACClassCleanManager : IMsgObserver
    {
        #region Settings
        public string[] AssemblySearchPatterns = new string[] { "gip.*", "kse.*", "elg.*", "eckel.*" };

        #endregion

        #region DI

        public Database Database { get; private set; }

        public string MainDir { get; private set; }

        private string _AssemblyNameSearchPattern = "*.core.*.dll,*.bso.*.dll,*.solution.*.dll,*.variobatch.*.dll,*Wpf.dll,gip.ext.graphics.dll,";
        public string AssemblyNameSearchPattern
        {
            get
            {
                return _AssemblyNameSearchPattern;
            }
            private set
            {
                _AssemblyNameSearchPattern = value;
            }
        }

        public string[] NamespacesToIgnore { get; set; }


        public string[] ClassesToIgnore { get; set; }
        #endregion

        #region Properties

        public List<ACClass> DBACClasses { get; private set; }

        public List<string> DllFiles { get; private set; }

        public List<string> DBClassNamespaces { get; private set; }


        public List<ACCleanAssembly> DllClasses { get; set; }
        public List<ACCleanAssembly> DBClasses { get; set; }


        public List<string> DBAssembilesWithoutFile { get; set; }

        public List<ACCleanAssembly> MissingClasses { get; set; }


        public List<string> NotCollectedDlls { get; private set; }

        #endregion

        #region ctor's
        public ACClassCleanManager(Database database, 
                                    string mainDir = "", 
                                    string namespacesToIgnore = "gip.core.layoutengine", 
                                    string classesToIgnore = Const.UnknownClass, 
                                    string assemblyNameSearchPattern = null)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Database = database;
            DBACClasses = Database.ACClass.Where(c => c.IsAssembly && !string.IsNullOrEmpty(c.AssemblyQualifiedName)).ToList();
            MainDir = mainDir;
            if (string.IsNullOrEmpty(MainDir))
                if (Assembly.GetEntryAssembly() != null)
                    MainDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                else
                    MainDir = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            if (!string.IsNullOrEmpty(assemblyNameSearchPattern))
                _AssemblyNameSearchPattern = assemblyNameSearchPattern;


            NamespacesToIgnore = namespacesToIgnore.Split(',');
            ClassesToIgnore = classesToIgnore.Split(',');
        }

        #endregion

        #region Methods

        #region Methods -> Collect data

        public void CollectData()
        {
            CollectDllData();
            CollectDBData();
            CollectInfoAboutMissing();
        }

        public void CollectDllData()
        {
            string[] patterns = AssemblyNameSearchPattern.Split(',');
            DllFiles = new List<string>();
            foreach (var pattern in patterns)
            {
                var files = Directory.GetFiles(MainDir, pattern).Where(c => !c.Contains("unittest")).ToList();
                DllFiles.AddRange(files);
            }

            DllClasses = GetDllClasses();
        }

        public void CollectDBData()
        {
            DBClassNamespaces = GetDBClassNamespaces();
            DBClasses = GetDBClasses();
        }

        public void CollectInfoAboutMissing()
        {
            DBAssembilesWithoutFile = GetDBAssembilesWithoutFile();
            MissingClasses = GetMissingClasses();
        }

        #endregion

        #region Methods -> Get Informations

        #region Methods -> Get Informations -> Database Info

        public List<ACCleanAssembly> GetDBClasses()
        {
            List<ACCleanAssembly> result = new List<ACCleanAssembly>();
            foreach (var acClass in DBACClasses)
            {
                ACCleanItem aCCleanItem = new ACCleanItem(acClass.AssemblyQualifiedName);
                result.AddACCleanItem(aCCleanItem);
            }
            return result;
        }

        public List<string> GetDBClassNamespaces()
        {
            List<string> classAssembiles = new List<string>();
            var query = DBACClasses.Select(c => c.AssemblyQualifiedName).Distinct();
            string namespaceName = "";
            foreach (var item in query)
            {

                namespaceName = GetNamespaceFromAssemblyQualifiedName(item);
                if (!string.IsNullOrEmpty(namespaceName) && !classAssembiles.Contains(namespaceName))
                    classAssembiles.Add(namespaceName);
            }
            return classAssembiles;
        }

        public List<string> GetDBAssembilesWithoutFile()
        {
            List<string> assemblyNamespaces = Database.ACAssembly.Select(c => c.AssemblyName).Distinct().AsEnumerable().Select(c => c.Replace(".dll", "")).ToList();
            var list = assemblyNamespaces.ToList();
            list.RemoveAll(c => DBClassNamespaces.Contains(c));
            list = list.OrderBy(c => c).ToList();
            return list;
        }
        #endregion

        #region Methods -> Get Informations -> DLL Info
        public List<ACCleanAssembly> GetDllClasses()
        {
            List<ACCleanAssembly> result = new List<ACCleanAssembly>();
            NotCollectedDlls = new List<string>();
            foreach (string dllFile in DllFiles)
            {
                string assemblyFile = Path.Combine(MainDir, dllFile);
                string nmSpace = Path.GetFileNameWithoutExtension(Path.GetFileName(dllFile));
                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyFile);
                    // dismiss class like: <>f__AnonymousType0`2
                    var assemblyQualifiedNames = assembly.GetTypes().Select(c=>c.AssemblyQualifiedName).Where(c=>char.IsLetter(c[0])).Distinct().ToList();
                    foreach (var assemblyQualifiedName in assemblyQualifiedNames)
                    {
                        ACCleanItem aCCleanItem = new ACCleanItem(assemblyQualifiedName);
                        result.AddACCleanItem(aCCleanItem);
                    }
                }
                catch (Exception)
                {
                    NotCollectedDlls.Add(nmSpace);
                }

            }
            return result;
        }
        #endregion

        public string GetNamespaceFromAssemblyQualifiedName(string assemblyQualifiedName)
        {
            string nmSpace = "";
            string[] parts = assemblyQualifiedName.Split('.');
            if (parts.Count() >= 3)
            {
                nmSpace = parts[0] + "." + parts[1] + "." + parts[2];
            }
            nmSpace = nmSpace.Trim();
            return nmSpace;
        }


        /// <summary>
        /// Extract class name corresponding assembly name
        /// </summary>
        /// <param name="assemblyQualifiedName"></param>
        /// <returns></returns>
        public string GetClassNameFromAssemblyName(string assemblyQualifiedName)
        {
            string className = "";
            string[] parts = assemblyQualifiedName.Split('.');
            if (parts.Length >= 4)
            {
                className = parts[3].Trim();
                if (className.Contains('+') && className.Contains(','))
                    className = className.Substring(className.IndexOf('+') + 1, className.IndexOf(',') - className.IndexOf('+') - 1);
                else if (className.Contains(","))
                    className = className.Substring(0, className.IndexOf(','));
            }
            return className;
        }

        private List<ACCleanAssembly> GetMissingClasses()
        {
            List<ACCleanAssembly> result = new List<ACCleanAssembly>();
            var query = DBClasses
                .Where(c => 
                    !NamespacesToIgnore.Contains(c.AssemblyName)
                    &&
                    !NotCollectedDlls.Contains(c.AssemblyName)
                )
                .Select(c => new ACCleanAssembly(c.AssemblyName))
                .ToList();
            foreach (ACCleanAssembly cleanedItem in query)
            {
                ACCleanAssembly dbItem = DBClasses.FirstOrDefault(c => c.AssemblyName == cleanedItem.AssemblyName);
                ACCleanAssembly dllItem = DllClasses.FirstOrDefault(c => c.AssemblyName == cleanedItem.AssemblyName);
                if (dllItem != null)
                    cleanedItem.CleanItemList =
                        dbItem
                        .CleanItemList
                        .Where(c =>
                            !ClassesToIgnore.Contains(c.FullClassName)
                            &&
                            !dllItem.CleanItemList.Select(x => x.FullClassName).Contains(c.FullClassName))
                        .OrderBy(c => c.FullClassName)
                        .ToList();
                else
                    cleanedItem.CleanItemList = dbItem.CleanItemList.Where(c => !ClassesToIgnore.Contains(c.FullClassName)).OrderBy(c => c.FullClassName).ToList();

                if (cleanedItem.CleanItemList != null && cleanedItem.CleanItemList.Any())
                    result.Add(cleanedItem);
            }
            result = result.OrderBy(c => c.AssemblyName).ToList();
            return result;
        }

        #endregion

        #region Methods -> Remove Data

        public void Remove(bool save = false)
        {
            RemoveAssembiles(save);
            RemoveClasses(save);
        }

        public MsgWithDetails RemoveAssembiles(bool save = false)
        {
            MsgWithDetails msgWithDetails = null;
            foreach (var assemblyName in DBAssembilesWithoutFile)
            {
                ACAssembly assembly = Database.ACAssembly.AsEnumerable().FirstOrDefault(c => c.AssemblyName.StartsWith(assemblyName));
                if (assembly != null)
                    assembly.DeleteACObject(Database, false);
            }
            if (save)
                try
                {
                    Database.ACSaveChanges();
                }
                catch (Exception ec)
                {
                    msgWithDetails = new MsgWithDetails() { Message = ec.ToString(), MessageLevel = eMsgLevel.Error };

                }
            return msgWithDetails;
        }

        public List<RemoveClassReport> RemoveClasses(bool save = false)
        {
            List<RemoveClassReport> result = new List<RemoveClassReport>();
            foreach (ACCleanAssembly aCCleanAssembly in MissingClasses)
            {
                foreach (ACCleanItem aCCleanItem in aCCleanAssembly.CleanItemList)
                {
                    Trace.WriteLine(string.Format("Remove class: {0}.{1}", aCCleanAssembly.AssemblyName, aCCleanItem.FullClassName));
                    RemoveClassReport partResult = RemoveClass(aCCleanItem, save);
                    result.Add(partResult);
                }
            }
            return result;
        }

        public RemoveClassReport RemoveClass(ACCleanItem aCCleanItem, bool save = false)
        {
            RemoveClassReport report = new RemoveClassReport()
            {
                AssemblyName = aCCleanItem.AssemblyName,
                FullClassName = aCCleanItem.FullClassName,
                ACIdentifier = aCCleanItem.ClassName
            };
            ACClass aCClass = 
                DBACClasses
                .Where(c =>
                    c.AssemblyQualifiedName.StartsWith(aCCleanItem.FullClassName)
                    && c.AssemblyQualifiedName.Contains(aCCleanItem.AssemblyName)
                )
                .FirstOrDefault();
            if (aCClass != null)
            {
                report.ACClassID = aCClass.ACClassID;
                try
                {
                    aCClass.DeleteACClassRecursive(Database, false);
                    if (save)
                        Database.ACSaveChanges();
                    report.Success = true;
                }
                catch (Exception ec)
                {
                    Database.ACUndoChanges();
                    report.MsgWithDetails = new MsgWithDetails() { Message = ec.ToString(), MessageLevel = eMsgLevel.Error };
                    report.ErrorMessage = ec.ToString();
                }
                if (report.MsgWithDetails == null || report.MsgWithDetails.MessageLevel <= eMsgLevel.Warning)
                    report.Success = true;
            }
            return report;
        }
        #endregion

        #endregion

        #region IMsgObserver
        public void SendMessage(Msg msg)
        {

        }

        #endregion
    }
}
