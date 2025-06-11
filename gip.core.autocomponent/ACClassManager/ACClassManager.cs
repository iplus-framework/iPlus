// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using gip.core.datamodel;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using gip.core.ControlScriptSync;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Formats.Asn1;

namespace gip.core.autocomponent
{
    public class ACClassManager
    {

        #region Structs
        struct ACQueryItem
        {
            public Type ParentType { get; set; }
            public ACQueryInfo ACQueryInfo { get; set; }
        };

        struct ACClassItem
        {
            public Type ParentType { get; set; }
            public ACClassInfo ACClassInfo { get; set; }
        }

        struct ACFileItem
        {
            public string Filename { get; set; }
            public string Namespace { get; set; }
            public CodeSummaryReader XMLHelper { get; set; }
            public DateTime LastManipulationDate { get; set; }
        }

        class ACFileItemList : List<ACFileItem>
        {
            public string GetSummaryClass(ACClass acClass)
            {
                string fullName = acClass.ObjectType.FullName;
                int posP = fullName.LastIndexOf(".");
                string namespace1 = fullName.Substring(0, posP);

                var query = this.Where(c => c.Namespace == namespace1);
                if (!query.Any())
                    return null;
                ACFileItem item = query.First();
                return item.XMLHelper == null ? null : item.XMLHelper.GetSummaryClass(fullName);
            }

            public string GetSummaryMethod(ACClass acClass, MethodInfo mi)
            {
                string fullName = acClass.ObjectType.FullName;
                int posP = fullName.LastIndexOf(".");
                string namespace1 = fullName.Substring(0, posP);

                var query = this.Where(c => c.Namespace == namespace1);
                if (!query.Any())
                    return null;
                ACFileItem item = query.First();
                return item.XMLHelper == null ? null : item.XMLHelper.GetSummaryMethod(fullName, mi);
            }

            public string GetSummaryProperty(ACClass acClass, PropertyInfo pi, ACPropertyBase acPropertyInfo)
            {
                if (acClass.ACKind == Global.ACKinds.TACDBA && acPropertyInfo is ACPropertyEntity)
                {
                    return (acPropertyInfo as ACPropertyEntity).Comment;
                }
                string fullName = acClass.ObjectType.FullName;
                int posP = fullName.LastIndexOf(".");
                string namespace1 = fullName.Substring(0, posP);

                var query = this.Where(c => c.Namespace == namespace1);
                if (!query.Any())
                    return null;
                ACFileItem item = query.First();
                return item.XMLHelper == null ? null : item.XMLHelper.GetSummaryProperty(fullName, pi);
            }
        }

        #endregion

        #region PrecompiledQueries


        static readonly Func<Database, String, IEnumerable<ACClass>> s_compiledQueryACClass_AssemblyName =
        EF.CompileQuery<Database, String, IEnumerable<ACClass>>(
            (ctx, assemblyQualifiedName) => from c in ctx.ACClass where c.AssemblyQualifiedName == assemblyQualifiedName select c
        );

        static readonly Func<Database, String, IEnumerable<ACClass>> s_compiledQueryACClass_StartsWithAssemblyName =
        EF.CompileQuery<Database, String, IEnumerable<ACClass>>(
            (ctx, name) => from c in ctx.ACClass where c.AssemblyQualifiedName.StartsWith(name) select c
        );

        static readonly Func<Database, IEnumerable<ACClass>> s_compiledQueryACClass_CheckAssemblyName =
        EF.CompileQuery<Database, IEnumerable<ACClass>>(
            (ctx) => from c in ctx.ACClass where !string.IsNullOrEmpty(c.AssemblyQualifiedName) && !c.AssemblyQualifiedName.StartsWith("System.") select c
        );


        public static readonly Func<Database, string, gip.core.datamodel.ACClass> s_cQry_ACClassIdentifier =
       EF.CompileQuery<Database, string, gip.core.datamodel.ACClass>(
           (ctx, acIdentifier) => ctx.ACClass.Where(c => c.ACIdentifier == acIdentifier).FirstOrDefault()
       );

        public static readonly Func<Database, string, IEnumerable<gip.core.datamodel.ACClass>> s_cQry_GetAvailableModulesAsACClass =
        EF.CompileQuery<Database, string, IEnumerable<gip.core.datamodel.ACClass>>(
            (ctx, acIdentifier) => ctx.ACClass.Where(c => (c.BasedOnACClassID.HasValue
                                                            && (c.ACClass1_BasedOnACClass.ACIdentifier == acIdentifier // 1. Ableitungsstufe
                                                                || (c.ACClass1_BasedOnACClass.BasedOnACClassID.HasValue
                                                                        && (c.ACClass1_BasedOnACClass.ACClass1_BasedOnACClass.ACIdentifier == acIdentifier // 2. Ableitungsstufe
                                                                            || (c.ACClass1_BasedOnACClass.ACClass1_BasedOnACClass.BasedOnACClassID.HasValue
                                                                                        && (c.ACClass1_BasedOnACClass.ACClass1_BasedOnACClass.ACClass1_BasedOnACClass.ACIdentifier == acIdentifier // 3. Ableitungsstufe
                                                                                            || (c.ACClass1_BasedOnACClass.ACClass1_BasedOnACClass.ACClass1_BasedOnACClass.BasedOnACClassID.HasValue
                                                                                                && c.ACClass1_BasedOnACClass.ACClass1_BasedOnACClass.ACClass1_BasedOnACClass.ACClass1_BasedOnACClass.ACIdentifier == acIdentifier) // 4. Ableitungsstufe
                                                                                            )
                                                                                )
                                                                            )
                                                                    )
                                                                )
                                                            )
                                                            && c.ACProject != null && c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application)
                                                            .OrderBy(c => c.ACIdentifier)
        );

        #endregion


        #region Database

        Database _Database;
        ACFileItemList ACFileItems;
        List<IACEntityObjectContext> _ObjectContexts = new List<IACEntityObjectContext>();

        #endregion

        #region ctor's
        public ACClassManager()
        {
            _Database = gip.core.datamodel.Database.GlobalDatabase;
            _ObjectContexts.Add(_Database);
        }

        #endregion

        public void RegisterAndUpdateACObjects(bool bUpdateInDB)
        {

            using (ACMonitor.Lock(_Database.QueryLock_1X000))
            {
                DateTime dt1 = DateTime.Now;
                List<ACQueryItem> acQueryItemList = new List<ACQueryItem>();
                List<ACClassItem> acClassItemCompositionList = new List<ACClassItem>();
                List<ACClassItem> acClassItemWithChilds = new List<ACClassItem>();

                _OverwriteXMLACMethods = !_Database.ACAssembly.Any();
                string MainDir = "";
                if (Assembly.GetEntryAssembly() != null)
                    MainDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                else
                    MainDir = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

                string[] patterns = AssemblyNameSearchPattern.Split(',');
                ACFileItems = new ACFileItemList();
                foreach (var pattern in patterns)
                {
                    foreach (var fileName in Directory.GetFiles(MainDir, pattern).Where(c => !c.Contains("unittest")))
                    {
                        if (ACFileItems.Where(c => c.Filename == fileName).Any())
                            continue;
                        ACFileItem acFileItem = new ACFileItem();
                        acFileItem.Filename = fileName;
                        acFileItem.LastManipulationDate = File.GetLastWriteTime(acFileItem.Filename);

                        int posP = fileName.LastIndexOf(".");
                        string namespace1 = fileName.Substring(0, posP);
                        int pos2 = namespace1.LastIndexOf("\\");

                        acFileItem.Namespace = namespace1.Substring(pos2 + 1);
                        if (bUpdateInDB && File.Exists(namespace1 + ".xml"))
                        {
                            acFileItem.XMLHelper = new CodeSummaryReader();
                            acFileItem.XMLHelper.Open(namespace1 + ".xml");
                        }

                        if (acFileItem.Filename.EndsWith("gip.core.datamodel.dll"))
                        {
                            ACFileItems.Insert(0, acFileItem);
                        }
                        else
                        {
                            ACFileItems.Add(acFileItem);
                        }
                    }
                }

                if (bUpdateInDB && Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogDebug("ACClassManager", "RegisterAndUpdateACObjects", String.Format("Started analyzing {0} Assemblies", ACFileItems.Count));

                foreach (ACFileItem acFileItem in ACFileItems)
                {
                    try
                    {
                        Assembly classAssembly = Assembly.LoadFrom(acFileItem.Filename);

                        var classTypeList2 = classAssembly.GetTypes();
                        foreach (var classType in classTypeList2)
                        {
                            var querySerializable = classType.GetCustomAttributes(typeof(ACSerializeableInfo), false);
                            if (querySerializable.Any())
                            {
                                var acSerializeableInfo = querySerializable.First() as ACSerializeableInfo;
                                if (acSerializeableInfo.TypeList != null)
                                {
                                    foreach (var type in acSerializeableInfo.TypeList)
                                    {
                                        ACKnownTypes.RegisterUnKnownType(type);
                                    }
                                }
                                else
                                    ACKnownTypes.RegisterUnKnownType(classType);
                            }

                            var queryCTors = classType.GetCustomAttributes(typeof(ACInvokeStaticCtor), false);
                            if (queryCTors.Any())
                            {
                                try
                                {
                                    // Rufe statischen Konstruktor auf um evtl. einen Eintrag in _StaticExecuteHandlers zu erhalten
                                    ConstructorInfo constructor = classType.GetConstructor(BindingFlags.Static | BindingFlags.NonPublic, null, new Type[0], null);
                                    if (constructor != null)
                                        constructor.Invoke(null, null);
                                }
                                catch (Exception e)
                                {
                                    string msg = e.Message;
                                    if (e.InnerException != null && e.InnerException.Message != null)
                                        msg += " Inner:" + e.InnerException.Message;

                                    if (Database.Root != null && Database.Root.Messages != null)
                                        Database.Root.Messages.LogException("ACClassManager", "RegisterAndUpdateACObjects", msg);
                                }
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException rtExc)
                    {
                        string msg = rtExc.Message;
                        if (rtExc.InnerException != null && rtExc.InnerException.Message != null)
                            msg += " Inner:" + rtExc.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("ACClassManager", "RegisterAndUpdateACObjects(10)", msg);
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("ACClassManager", "RegisterAndUpdateACObjects(20)", msg);
                    }

                    ACKnownTypes.RegisterUnKnownType(typeof(ObservableCollection<string>));
                }

                WCFMessage.GetKnownType(); // KnowMessageTypes-Registieren, damit Lokale-Properties persistiert werden können auch wenn noch keine Netzwerkverbindung da ist!

                foreach (ACFileItem acFileItem in ACFileItems.OrderBy(c => c.LastManipulationDate))
                {
                    Assembly classAssembly = Assembly.LoadFrom(acFileItem.Filename);
                    if (Database.Root != null && Database.Root.WPFServices != null)
                        Database.Root.WPFServices.AddXamlNamespacesFromAssembly(classAssembly);
                    if (bUpdateInDB)
                    {
                        DateTime lastWriteTime = File.GetLastWriteTime(acFileItem.Filename);
                        string assemblyHashValue = ACClassFileHashManager.GetHash(acFileItem.Filename);
                        String assemblyFileName = classAssembly.ManifestModule.Name.ToLower();
                        var query = _Database.ACAssembly.Where(c => c.AssemblyName == assemblyFileName);
                        if (!query.Any())
                        {
                            ACAssembly acAssembly = ACAssembly.NewACObject(_Database, null);
                            acAssembly.AssemblyName = assemblyFileName;
                            acAssembly.LastReflectionDate = DateTime.Now;
                            acAssembly.AssemblyDate = lastWriteTime;
                            acAssembly.InsertName = "Init";
                            acAssembly.InsertDate = DateTime.Now;
                            acAssembly.UpdateName = "Init";
                            acAssembly.UpdateDate = DateTime.Now;
                            acAssembly.SHA1 = assemblyHashValue;
                            _Database.ACAssembly.Add(acAssembly);
                        }
                        else
                        {
                            ACAssembly acAssembly = query.First();
                            if (acAssembly.SHA1 != assemblyHashValue)
                            {
                                acAssembly.SHA1 = assemblyHashValue;
                                acAssembly.AssemblyDate = lastWriteTime;
                                acAssembly.LastReflectionDate = DateTime.Now;
                            }
                            else
                                continue;
                        }

                        List<Type> typeList = new List<Type>();

                        try
                        {
                            var classTypeList = classAssembly.GetTypes();
                            Messages.ConsoleMsg("System", "Updating Assembly " + classAssembly.ManifestModule.ToString() + "...");

                            foreach (var classType in classTypeList)
                            {
                                string typeName = classType.Name;
                                var queryClassInfoAttr = classType.GetCustomAttributes(typeof(ACClassInfo), false);
                                if (queryClassInfoAttr.Any())
                                {
                                    ACClassInfo acClassInfo = queryClassInfoAttr.First() as ACClassInfo;
                                    if (acClassInfo.ACClassChilds != null)
                                    {
                                        acClassItemWithChilds.Add(new ACClassItem { ParentType = classType, ACClassInfo = acClassInfo });
                                    }
                                    if (!string.IsNullOrEmpty(acClassInfo.QRYConfig) || !string.IsNullOrEmpty(acClassInfo.BSOConfig))
                                    {
                                        acClassItemCompositionList.Add(new ACClassItem { ParentType = classType, ACClassInfo = acClassInfo });
                                    }
                                    typeList.Add(classType);

                                    foreach (var acQueryInfo in classType.GetCustomAttributes(false).Where(c => c.GetType().Name.StartsWith("ACQueryInfo")))
                                    {
                                        acQueryItemList.Add(new ACQueryItem { ParentType = classType, ACQueryInfo = acQueryInfo as ACQueryInfo });
                                    }
                                }
                            }
                        }
                        catch (ReflectionTypeLoadException rcExc)
                        {
                            string msg = rcExc.Message;
                            if (rcExc.InnerException != null && rcExc.InnerException.Message != null)
                                msg += " Inner:" + rcExc.InnerException.Message;

                            if (Database.Root != null && Database.Root.Messages != null)
                                Database.Root.Messages.LogException("ACClassManager", "RegisterAndUpdateACObjects(30)", msg);
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (Database.Root != null && Database.Root.Messages != null)
                                Database.Root.Messages.LogException("ACClassManager", "RegisterAndUpdateACObjects(40)", msg);
                        }
                        UpdateEntitiesWithUnknownTypes(typeList, bUpdateInDB);
                    }

                }

                if (bUpdateInDB)
                {
                    InsertOrUpdateDummyEntries();

                    InsertOrUpdateACClassItemWithChilds(acClassItemWithChilds);

                    InsertOrUpdateACQueryInfos(acQueryItemList);

                    InsertOrUpdateACClassCompositions(acClassItemCompositionList);

                    // Überprüfen von VBShowColumns, VBFilterColumns und KeyACIdentifier
                    List<ACClass> xACClassList = new List<ACClass>();

                    var query = s_compiledQueryACClass_CheckAssemblyName.Invoke(_Database);
                    foreach (var acClass in query)
                    {
                        if (!_CheckedAssemblyACClassList.ContainsKey(acClass.AssemblyQualifiedName))
                        {
#if DEBUG
                            if (acClass.ObjectType == null)
                            {
                                System.Diagnostics.Debug.WriteLine(acClass.ACIdentifier + " => " + acClass.AssemblyQualifiedName);
                            }
#endif
                            xACClassList.Add(acClass);
                        }
                    }
                    UpdateScriptMethods();

                    UpdateACClassACURLCached(_Database);

                    foreach (IACEntityObjectContext context in _ObjectContexts)
                    {
                        if (context != gip.core.datamodel.Database.GlobalDatabase)
                        {
                            context.Dispose();
                        }
                    }

                    // Remove not existing assemblies
                    var assemblyNameList = ACFileItems.Select(x => Assembly.LoadFrom(x.Filename).ManifestModule.Name.ToLower()).ToList();
                    var assemblyNotMorePresentInFileSystem = _Database.ACAssembly.ToList().Where(c => !assemblyNameList.Contains(c.AssemblyName)).ToList();
                    assemblyNotMorePresentInFileSystem.ForEach(c => c.DeleteACObject(_Database, false));
                }
            }

            ACPropertyFactoryBase.GenerateUnKnownValueEventTypes();


            #region Designs update
            // aagincic: Control script sync - importing script and design (XAML) changes - placed there  on begin of register proces
            // later position is in foreach loop for loading each assembly

            if (bUpdateInDB)
            {
                ControlSync controlSync = new ControlSync();
                // pre-perapring Resources and Query for root - this resources is used for importing
                ACRoot.SRoot.PrepareQueriesAndResoruces();
                controlSync.OnMessage += controlSync_OnMessage;
                bool importSuccess = false;
                IResources rootResources = new Resources();

                using (ACMonitor.Lock(_Database.QueryLock_1X000))
                {
                    importSuccess = controlSync.Sync(ACRoot.SRoot, Database.GlobalDatabase);
                }

            }
            #endregion end designs update
        }



        #region ClassManager

        struct ACClassReq
        {
            public ACClass ACClass { get; set; }
            public Type TypeOfACClass { get; set; }
        }
        List<ACClassReq> _UpdateRequiredList = new List<ACClassReq>();
        List<String> _ReflectAssemblyIgnore = new List<string>();
        List<String> _ReflectAssemblyRequired = new List<string>();

        Dictionary<string, ACClass> _CheckedAssemblyACClassList = new Dictionary<string, ACClass>();

        public ACClass GetCheckedAssemblyACClass(string AssemblyQualifiedName)
        {
            if (String.IsNullOrEmpty(AssemblyQualifiedName))
                return null;
            if (_CheckedAssemblyACClassList.ContainsKey(AssemblyQualifiedName))
            {
                return _CheckedAssemblyACClassList[AssemblyQualifiedName];
            }
            return null;
        }

        public void UpdateEntitiesWithUnknownTypes(List<Type> acTypeList, bool bUpdateInDB)
        {
            try
            {
                if (!bUpdateInDB)
                    return;
                foreach (var acType in acTypeList)
                {
                    InsertOrUpdateValueTypeACClass(acType, true, false, 0);
                }
                // Jetzt alle Typen aktualisieren, bei denen nur ACClass, aber noch nicht ACClassProperty und ACClassMethod eingefügt wurde
                while (_UpdateRequiredList.Any())
                {
                    var updateRequiredList = _UpdateRequiredList.ToList();
                    _UpdateRequiredList.Clear();
                    foreach (var acClassInfo in updateRequiredList)
                    {
                         InsertOrUpdateValueTypeACClass(acClassInfo.TypeOfACClass, true, false, 0);
                    }
                }

                MsgWithDetails saveResult = _Database.ACSaveChanges(true, true, true);
                if (saveResult != null)
                {
                    Messages.GlobalMsg.AddDetailMessage(saveResult);
                    _Database.ACUndoChanges();
                    throw new Exception("UpdateEntitiesWithUnknownTypes() " + saveResult.InnerMessage);
                }
            }
            catch (Exception e)
            {
                Messages.GlobalMsg.AddDetailMessage(new Msg() { MessageLevel = eMsgLevel.Error, Message = e.Message });

                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClassManager", "UpdateEntitiesWithUnknownTypes", msg);

                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dotNETType">In ACClass einzufügender Type</param>
        /// <param name="updateIfExists">True=Auf jeden Fall aktualisieren</param>
        /// <param name="onlyClass">True=Nur Klasse wird eingefügt</param>
        /// <param name="recursionDepth">Rekusiondepth</param>
        /// <returns></returns>
        public ACClass InsertOrUpdateValueTypeACClass(Type dotNETType, bool updateIfExists, bool onlyClass, int recursionDepth)
        {
            recursionDepth++;
            #region Test debug insert / update class
            //string searchValue = "ACProgramLogView";
            //searchValue = searchValue.ToLower();
            //if (dotNETType.Name.ToLower().Contains(searchValue))
            //    System.Diagnostics.Debugger.Break();
            #endregion

            // Variable will be used later for different types of attributes so it is defined as object array
            object[] attributes;
            ACClassInfo acClassInfo;
            ACQueryInfoPrimary acQueryInfoPrimary = null;

            attributes = dotNETType.GetCustomAttributes(typeof(ACClassInfo), false);

            if (String.IsNullOrEmpty(dotNETType.FullName))
                return null;

            if (attributes.Any())
            {
                acClassInfo = (ACClassInfo)attributes[0];
            }
            else if (dotNETType.BaseType != null && dotNETType.BaseType.Name == "EntityObject")
            {
                acClassInfo = new ACClassInfo("", "", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true);
            }
            else if (dotNETType.FullName.StartsWith("System.") && dotNETType.FullName.IndexOf('.') == dotNETType.FullName.LastIndexOf('.'))
            {
                acClassInfo = new ACClassInfo("System", "", Global.ACKinds.TACLRBaseTypes, Global.ACStorableTypes.NotStorable, true, false);
            }
            else if (dotNETType.IsInterface)
            {
                acClassInfo = new ACClassInfo("", "", Global.ACKinds.TACInterface, Global.ACStorableTypes.NotStorable, true, false);
            }
            else if (dotNETType.IsEnum)
            {
                acClassInfo = new ACClassInfo("", "", Global.ACKinds.TACEnum, Global.ACStorableTypes.NotStorable, true, false);
            }
            else if (dotNETType.FullName.StartsWith("System.Windows."))
            {
                acClassInfo = new ACClassInfo("", "", Global.ACKinds.TACUIControl, Global.ACStorableTypes.NotStorable, true, false);
            }
            else if (dotNETType.IsClass)
            {
                acClassInfo = new ACClassInfo("", "", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false);
            }
            else
            {
                acClassInfo = new ACClassInfo("", "", Global.ACKinds.TACUndefined, Global.ACStorableTypes.NotStorable, true, false);
            }

            attributes = dotNETType.GetCustomAttributes(typeof(ACQueryInfoPrimary), false);

            if (attributes.Any())
                acQueryInfoPrimary = attributes[0] as ACQueryInfoPrimary;


            string acClassName = "";
            string acClassFullName = "";
            string assemblyQualifiedName = "";
            ExtractClassType(dotNETType, ref acClassName, ref acClassFullName, ref assemblyQualifiedName);
            ACClass acClass = GetCheckedAssemblyACClass(assemblyQualifiedName);
            // Wenn schon geprüft, dann keine weitere Bearbeitung notwendig
            if (acClass != null)
                return acClass;
            if (!updateIfExists)
            {
                var query1 = _UpdateRequiredList.Where(c => c.TypeOfACClass == dotNETType);
                if (query1.Any())
                {
                    return query1.First().ACClass;
                }
            }
            ACProject acProject = GetACProjectByACType(acClassInfo.ACKind, dotNETType);

            var query = s_compiledQueryACClass_AssemblyName.Invoke(_Database, assemblyQualifiedName);
            acClass = query.FirstOrDefault();

            // Da AssemblyQualifiedName auch die Versions-Nummer der dll enthält,
            // muss bei einer höhreren Versionnummer der AssemblyQualifiedName aktualisiert werden
            if (acClass == null)
            {
                query = s_compiledQueryACClass_StartsWithAssemblyName.Invoke(_Database, acClassFullName + ",");
                acClass = query.FirstOrDefault();
            }
            if (acClass == null)
            {
                query = s_compiledQueryACClass_StartsWithAssemblyName.Invoke(_Database, acClassName);
                acClass = query.FirstOrDefault();
            }

            if (acClass == null)
            {
                acClass = ACClass.NewACObject(_Database, acProject);

                acClass.ACIdentifier = acClassName.Length > 100 ? acClassName.Substring(0, 100) : acClassName;


                acClass.AssemblyQualifiedName = assemblyQualifiedName.Length > 250 ? assemblyQualifiedName.Substring(0, 250) : assemblyQualifiedName;
                acClass.ACKind = acClassInfo.ACKind;
                acClass.SortIndex = acClassInfo.SortIndex;
                acClass.IsAbstract = dotNETType.IsAbstract;
                acClass.IsAutostart = false;
                acClass.ACProjectID = acClass.ACProject.ACProjectID;

                acClass.ACStartType = GetACStartTypeByACType(acClassInfo.ACKind);
                acClass.ACStorableType = acClassInfo.ACStorableType;
                acClass.IsRightmanagement = acClassInfo.IsRightmanagement;
                acClass.IsAssembly = true;
                if (typeof(ACComponent).IsAssignableFrom(dotNETType))
                {
                    acClass.IsMultiInstance = acClassInfo.IsMultiInstance;
                }
                else
                {
                    acClass.IsMultiInstance = false;
                }

                if (acClassInfo.ACKind == Global.ACKinds.TPAProcessModule || acClassInfo.ACKind == Global.ACKinds.TPARole || acClassInfo.ACKind == Global.ACKinds.TACApplicationManager)
                {
                    UpdatePWACClass(acClass, acClassInfo.PWInfoACClass);
                }
                else if (acClassInfo.ACKind == Global.ACKinds.TPWNodeMethod || acClassInfo.ACKind == Global.ACKinds.TPWNodeWorkflow)
                {
                    UpdatePWMethodACClass(acClass, acClassInfo.PWInfoACClass);
                }


                if (acClass.EntityState == EntityState.Detached)
                    _Database.ACClass.Add(acClass);
                _CheckedAssemblyACClassList.Add(acClass.AssemblyQualifiedName, acClass);
                if (acProject.ACProjectType == Global.ACProjectTypes.Root)
                    acClass.ACClass1_ParentACClass = GetManagerACClass(acClass.ACKind, dotNETType);

                UpdatePackage(acClass, acClassInfo);
                if (onlyClass)
                {
                    if (acClass.ACKind != Global.ACKinds.TACUndefined)
                    {
                        if (!_UpdateRequiredList.Where(c => c.ACClass == acClass).Any())
                            AddToRequiredList(new ACClassReq { ACClass = acClass, TypeOfACClass = dotNETType });
                    }
                    return acClass;
                }
                else
                {
                    var query2 = _UpdateRequiredList.Where(c => c.ACClass == acClass);
                    if (query2.Any())
                    {
                        _UpdateRequiredList.Remove(query2.First());
                    }
                }
            }
            else
            {
                if (acClass.AssemblyQualifiedName != assemblyQualifiedName)
                    acClass.AssemblyQualifiedName = assemblyQualifiedName.Length > 250 ? assemblyQualifiedName.Substring(0, 250) : assemblyQualifiedName;

                if (acClassInfo.ACKind == Global.ACKinds.TPAProcessModule || acClassInfo.ACKind == Global.ACKinds.TPARole || acClassInfo.ACKind == Global.ACKinds.TACApplicationManager)
                {
                    UpdatePWACClass(acClass, acClassInfo.PWInfoACClass);
                }
                else if (acClassInfo.ACKind == Global.ACKinds.TPWNodeMethod || acClassInfo.ACKind == Global.ACKinds.TPWNodeWorkflow)
                {
                    UpdatePWMethodACClass(acClass, acClassInfo.PWInfoACClass);
                }

                if (acClass.ACKind != acClassInfo.ACKind)
                    acClass.ACKind = acClassInfo.ACKind;
                if (acClass.ACStorableType != acClassInfo.ACStorableType)
                    acClass.ACStorableType = acClassInfo.ACStorableType;
                if (acClass.SortIndex != acClassInfo.SortIndex)
                    acClass.SortIndex = acClassInfo.SortIndex;
                if (acClass.IsRightmanagement != acClassInfo.IsRightmanagement)
                    acClass.IsRightmanagement = acClassInfo.IsRightmanagement;
                if (!acClass.IsAssembly)
                    acClass.IsAssembly = true;
                if (acClass.ACProject != acProject)
                {
                    acClass.ACProject = acProject;
                    acClass.ACURLCached = "";
                }

                if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.Root)
                {
                    ACClass parentACClass = GetManagerACClass(acClass.ACKind, dotNETType);
                    if (acClass.ACClass1_ParentACClass != parentACClass)
                    {
                        acClass.ACClass1_ParentACClass = parentACClass;
                        acClass.ACURLCached = "";
                    }
                }
                else if (acClass.ACProject.ACProjectType == Global.ACProjectTypes.ClassLibrary
                         && acClass.ParentACClassID.HasValue)
                {
                    acClass.ACClass1_ParentACClass = null;
                    acClass.ACURLCached = "";
                }

                if (onlyClass)
                {
                    if (!_UpdateRequiredList.Where(c => c.ACClass == acClass).Any())
                        AddToRequiredList(new ACClassReq { ACClass = acClass, TypeOfACClass = dotNETType });
                    return acClass;
                }

                if (dotNETType.BaseType != null
                    && dotNETType.BaseType.Name != typeof(object).Name
                    && dotNETType.BaseType.Name != typeof(VBEntityObject).Name
                    && (acClass.ACClass1_BasedOnACClass == null || acClass.ACClass1_BasedOnACClass.ACIdentifier != dotNETType.BaseType.Name))
                {
                    updateIfExists = true;
                }

                if (!updateIfExists)
                    return acClass;

                if (acClass.IsAbstract != dotNETType.IsAbstract)
                    acClass.IsAbstract = dotNETType.IsAbstract;

                if (typeof(ACComponent).IsAssignableFrom(dotNETType))
                {
                    if (acClass.IsMultiInstance != acClassInfo.IsMultiInstance)
                    {
                        acClass.IsMultiInstance = acClassInfo.IsMultiInstance;
                    }
                }
                else
                {
                    if (acClass.IsMultiInstance != false)
                        acClass.IsMultiInstance = false;
                }
                if (acClass.ACStartType != GetACStartTypeByACType(acClassInfo.ACKind))
                    acClass.ACStartType = GetACStartTypeByACType(acClassInfo.ACKind);
            }
            if (acQueryInfoPrimary != null)
            {
                if (acClass.ACFilterColumns != acQueryInfoPrimary.ACFilterColumns)
                    acClass.ACFilterColumns = acQueryInfoPrimary.ACFilterColumns;
                if (acClass.ACSortColumns != acQueryInfoPrimary.ACSortColumns)
                    acClass.ACSortColumns = acQueryInfoPrimary.ACSortColumns;
            }
            if (acClass.ObjectType != null)
            {
                string summary = ACFileItems.GetSummaryClass(acClass);
                if (!string.IsNullOrEmpty(summary) && acClass.Comment != summary && summary != "Keine Dokumentation für Metadaten verfügbar.")
                    acClass.Comment = summary;
            }
            UpdatePackage(acClass, acClassInfo);

            Translator.UpdateTranslation(acClass, acClassInfo.ACCaptionTranslation);
            if (typeof(ACValueItemList) != acClass.ObjectType && typeof(ACValueItemList).IsAssignableFrom(acClass.ObjectType))
                Translator.UpdateTranslationACValueItemList(acClass);

            if (dotNETType.BaseType != null && dotNETType.BaseType != typeof(System.Object)
                && acClass.ACKind != Global.ACKinds.TACUndefined)
            {
                if (dotNETType.BaseType.IsGenericType)
                {
                    // z.B. MyList<T> : List<T>
                    if (dotNETType.BaseType.IsGenericTypeDefinition)
                    {
                        Type baseType = dotNETType.BaseType.GetGenericTypeDefinition();
                        string baseFullName = baseType.FullName;
                        ACClass basedACClass = InsertOrUpdateValueTypeACClass(baseType, !_CheckedAssemblyACClassList.ContainsKey(baseFullName), onlyClass, recursionDepth);
                        if (acClass.ACClass1_BasedOnACClass != basedACClass)
                            acClass.ACClass1_BasedOnACClass = basedACClass;
                    }
                    // z.B. MyList<ClassX> : List<ClassX>
                    else
                    {
                        Type baseType = dotNETType.BaseType.GetGenericTypeDefinition(); // Auch nur Type-Definition als Basisklasse, weil sonst tausende von Kombinationsmöglichkeiten erzeugt werden
                        string baseFullName = baseType.FullName;
                        ACClass basedACClass = InsertOrUpdateValueTypeACClass(baseType, !_CheckedAssemblyACClassList.ContainsKey(baseFullName), onlyClass, recursionDepth);
                        if (acClass.ACClass1_BasedOnACClass != basedACClass)
                            acClass.ACClass1_BasedOnACClass = basedACClass;
                    }
                }
                else if (dotNETType.BaseType.FullName != null)
                {
                    var tempbaseType = dotNETType.BaseType;
                    while (tempbaseType.IsGenericType)
                    {
                        tempbaseType = tempbaseType.BaseType;
                    }
                    string baseFullName = tempbaseType.FullName;
                    if (dotNETType.IsEnum)
                    {
                        var typeOfShort = typeof(short);
                        ACClass basedACClass = InsertOrUpdateValueTypeACClass(typeOfShort, !_CheckedAssemblyACClassList.ContainsKey(typeOfShort.Name), onlyClass, recursionDepth);
                        if (acClass.ACClass1_BasedOnACClass != basedACClass)
                            acClass.ACClass1_BasedOnACClass = basedACClass;
                    }
                    else
                    {
                        ACClass basedACClass = InsertOrUpdateValueTypeACClass(tempbaseType, !_CheckedAssemblyACClassList.ContainsKey(baseFullName), onlyClass, recursionDepth);
                        if (acClass.ACClass1_BasedOnACClass != basedACClass)
                            acClass.ACClass1_BasedOnACClass = basedACClass;
                    }
                }
            }
            else if (dotNETType.IsInterface)
            {
                Type[] basedOnInterfaces = dotNETType.GetInterfaces();
                if (basedOnInterfaces != null && basedOnInterfaces.Any())
                {
                    Type baseType = basedOnInterfaces.First();
                    if (baseType.IsGenericType)
                    {
                        // z.B. IMyList<T> : IList<T>
                        if (baseType.IsGenericTypeDefinition)
                        {
                            Type baseType2 = baseType.GetGenericTypeDefinition();
                            string baseFullName = baseType2.FullName;
                            ACClass basedACClass = InsertOrUpdateValueTypeACClass(baseType2, !_CheckedAssemblyACClassList.ContainsKey(baseFullName), onlyClass, recursionDepth);
                            if (acClass.ACClass1_BasedOnACClass != basedACClass)
                                acClass.ACClass1_BasedOnACClass = basedACClass;
                        }
                        // z.B. IMyList<ClassX> : IList<ClassX>
                        else
                        {
                            Type baseType2 = baseType.GetGenericTypeDefinition(); // Auch nur Type-Definition als Basisklasse, weil sonst tausende von Kombinationsmöglichkeiten erzeugt werden
                            string baseFullName = baseType2.FullName;
                            ACClass basedACClass = InsertOrUpdateValueTypeACClass(baseType2, !_CheckedAssemblyACClassList.ContainsKey(baseFullName), onlyClass, recursionDepth);
                            if (acClass.ACClass1_BasedOnACClass != basedACClass)
                                acClass.ACClass1_BasedOnACClass = basedACClass;
                        }
                    }
                    else if (baseType.FullName != null)
                    {
                        var tempbaseType = baseType;
                        while (tempbaseType.IsGenericType)
                        {
                            tempbaseType = tempbaseType.BaseType;
                        }
                        string baseFullName = tempbaseType.FullName;
                        ACClass basedACClass = InsertOrUpdateValueTypeACClass(tempbaseType, !_CheckedAssemblyACClassList.ContainsKey(baseFullName), onlyClass, recursionDepth);
                        if (acClass.ACClass1_BasedOnACClass != basedACClass)
                            acClass.ACClass1_BasedOnACClass = basedACClass;
                    }
                }
            }


            if (!_CheckedAssemblyACClassList.ContainsKey(acClass.AssemblyQualifiedName))
            {
                _CheckedAssemblyACClassList.Add(acClass.AssemblyQualifiedName, acClass);
            }


            UpdateACClassConstructorInfo(acClass, dotNETType);
            //UpdateACClassConstructorInfo2(acClass, classType);


            InsertOrUpdateMethods(acClass, dotNETType, recursionDepth);
            InsertOrUpdateACClassProperties(acClass, dotNETType, updateIfExists, recursionDepth);

            try
            {
                if (recursionDepth <= 1)
                {
                    MsgWithDetails saveResult = _Database.ACSaveChanges(true, true, true);
                    if (saveResult != null)
                    {
                        Messages.GlobalMsg.AddDetailMessage(saveResult);
                        _Database.ACUndoChanges();
                        throw new Exception("InsertOrUpdateValueTypeACClass(), ACClass(" + acClass.ACIdentifier + ") " + saveResult.InnerMessage);
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateValueTypeACClass", msg);
                throw;
            }
            return acClass;
        }

        private void AddToRequiredList(ACClassReq acClassReq)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                if (acClassReq.TypeOfACClass.FullName == "System.Drawing.Bitmap")
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
            _UpdateRequiredList.Add(acClassReq);
            return;
            //String assemblyFileName = acClassReq.TypeOfACClass.Assembly.ManifestModule.Name.ToLower();
            //if (_ReflectAssemblyIgnore.Where(c => c == assemblyFileName).Any())
            //    return;
            //if (_ReflectAssemblyRequired.Where(c => c == assemblyFileName).Any())
            //{
            //    _UpdateRequiredList.Add(acClassReq);
            //    return;
            //}

            //DateTime lastWriteTime = File.GetLastWriteTime(acClassReq.TypeOfACClass.Assembly.ManifestModule.FullyQualifiedName);
            //var query = _Database.ACAssembly.Where(c => c.AssemblyName == assemblyFileName);
            //if (!query.Any())
            //{
            //    ACAssembly acAssembly = ACAssembly.NewACObject(_Database, null);
            //    acAssembly.AssemblyName = assemblyFileName;
            //    acAssembly.LastReflectionDate = DateTime.Now;
            //    acAssembly.AssemblyDate = lastWriteTime;
            //    _Database.ACAssembly.AddObject(acAssembly);
            //    _ReflectAssemblyRequired.Add(assemblyFileName);
            //    _UpdateRequiredList.Add(acClassReq);
            //}
            //else
            //{
            //    ACAssembly acAssembly = query.First();
            //    if (Math.Abs((lastWriteTime - acAssembly.AssemblyDate).TotalSeconds) > 1)
            //    {
            //        acAssembly.AssemblyDate = lastWriteTime;
            //        acAssembly.LastReflectionDate = DateTime.Now;
            //        _ReflectAssemblyRequired.Add(assemblyFileName);
            //        _UpdateRequiredList.Add(acClassReq);
            //    }
            //    else
            //    {
            //        _ReflectAssemblyIgnore.Add(assemblyFileName);
            //    }
            //}
        }

        void UpdatePackage(ACClass acClass, ACClassInfo acClassInfo)
        {
            // Falls ACPackageName angegeben, dann entsprechendes ACPackage ermitteln
            if (!string.IsNullOrEmpty(acClassInfo.ACPackageName))
            {
                ACPackage acPackage = ACPackage.GetACPackage(_Database, acClassInfo.ACPackageName);
                if (acClass.ACPackage == null || acClass.ACPackage.ACPackageID != acPackage.ACPackageID)
                {
                    acClass.ACPackage = acPackage;
                    UpdateVirtualClassACPackage(acClass, acPackage);
                }
            }
            // ansonsten über die acClass ermitteln
            else
            {
                ACPackage acPackage;
                if (acClass.AssemblyQualifiedName.StartsWith("System.") || acClass.AssemblyQualifiedName.StartsWith("Microsoft."))
                {
                    acPackage = ACPackage.GetACPackage(_Database, "System");
                }
                else if (acClass.AssemblyQualifiedName.StartsWith("gip.ext.") || acClass.ACIdentifier == Database.C_DefaultContainerName)
                {
                    acPackage = ACPackage.GetACPackage(_Database, Const.PackName_VarioSystem);
                }
                else
                {
                    acPackage = ACPackage.GetACPackage(_Database, Const.PackName_VarioSystem);
                }
                if (acClass.ACPackage == null || acClass.ACPackage.ACPackageID != acPackage.ACPackageID)
                {
                    acClass.ACPackage = acPackage;
                    UpdateVirtualClassACPackage(acClass, acPackage);
                }
            }
        }

        void UpdateACClassConstructorInfo(ACClass acClass, Type classType)
        {
            object[] attributes = classType.GetCustomAttributes(typeof(ACClassConstructorInfo), false);
            if (attributes == null || !attributes.Any())
                return;

            ACClassConstructorInfo xmlACClassInfo = attributes[0] as ACClassConstructorInfo;
            ACValueList acValueList = new ACValueList();
            foreach (var parameter in xmlACClassInfo.ACParameters.Select(c => c as object[]))
            {
                ACValue acParameterDefinition = this.GetACParameterDefinition(parameter);

                acValueList.Add(acParameterDefinition);
            }
            string xmlACClass = ACClass.SerializeACClass(acValueList);
            if (acClass.XMLACClass != xmlACClass)
            {
                acClass.XMLACClass = xmlACClass;
                foreach (var acClassWF in acClass.ACClassWF_PWACClass)
                {
                    if (acClassWF.ACClassMethod != null && acClassWF.ACClassMethod.ACKind == Global.ACKinds.MSWorkflow)
                    {
                        acClassWF.ACClassMethod.UpdateParamListFromACClassConstructor(acClass);
                    }
                }
            }
        }

        private void UpdateVirtualClassACPackage(ACClass acClass, ACPackage package)
        {
            foreach (var item in acClass.ACClass_BasedOnACClass.Where(c => string.IsNullOrEmpty(c.AssemblyQualifiedName)))
            {
                item.ACPackageID = package.ACPackageID;
                UpdateVirtualClassACPackage(item, package);
            }
        }

        //void UpdateACClassConstructorInfo2(ACClass acClass, Type classType)
        //{
        //    object[] attributes = classType.GetCustomAttributes(typeof(ACClassConstructorInfo2), false);
        //    if (attributes == null || !attributes.Any())
        //        return;

        //    ACClassConstructorInfo2 xmlACClassInfo = attributes[0] as ACClassConstructorInfo2;
        //    ACValueList acValueList = new ACValueList();
        //    foreach (var parameter in xmlACClassInfo.ACParamters.Select(c => c as object[]))
        //    {
        //        ACValue acParameterDefinition = this.GetACParameterDefinition(parameter);
        //        acValueList.Add(acParameterDefinition);
        //    }
        //    string xmlACClass = ACClass.SerializeACClass(acValueList);
        //    if (acClass.XMLACClass != xmlACClass)
        //        acClass.XMLACClass = xmlACClass;
        //}

        ACClass GetManagerACClass(Global.ACKinds acKind, Type runtimeType)
        {
            int acTypeManager = 0;
            switch (acKind)
            {
                case Global.ACKinds.TACBusinessobjects:
                case Global.ACKinds.TACDBAManager:
                case Global.ACKinds.TACEnvironment:
                case Global.ACKinds.TACMessages:
                case Global.ACKinds.TACQueries:
                case Global.ACKinds.TACCommunications:
                case Global.ACKinds.TACLocalServiceObjects:
                    acTypeManager = (short)Global.ACKinds.TACRoot;
                    break;

                case Global.ACKinds.TACDBA:
                    acTypeManager = (short)Global.ACKinds.TACDBAManager;
                    break;
                case Global.ACKinds.TACBSO:
                case Global.ACKinds.TACBSOGlobal:
                case Global.ACKinds.TACBSOReport:
                    acTypeManager = (short)Global.ACKinds.TACBusinessobjects;
                    break;
                case Global.ACKinds.TACQRY:
                    acTypeManager = (short)Global.ACKinds.TACQueries;
                    break;
                case Global.ACKinds.TACWCFServiceManager:
                    acTypeManager = (short)Global.ACKinds.TACCommunications;
                    break;
                case Global.ACKinds.TACWCFServiceChannel:
                    acTypeManager = (short)Global.ACKinds.TACWCFServiceManager;
                    break;
                case Global.ACKinds.TACWCFClientManager:
                    acTypeManager = (short)Global.ACKinds.TACCommunications;
                    break;
                case Global.ACKinds.TACWCFClientChannel:
                    acTypeManager = (short)Global.ACKinds.TACWCFClientManager;
                    break;
                case Global.ACKinds.TACRuntimeDump:
                    acTypeManager = (short)Global.ACKinds.TACEnvironment;
                    break;
                default:
                    return null;
            }
            var query2 = RootProject.ACClass_ACProject.Where(c => c.ACKindIndex == acTypeManager);
            if (acTypeManager == (short)Global.ACKinds.TACDBAManager)
            {
                foreach (ACClass dbManager in query2)
                {
                    if (dbManager.AssemblyQualifiedName.Contains(runtimeType.Assembly.FullName))
                    {
                        return dbManager;
                    }
                }
            }
            return query2.FirstOrDefault();
        }

        #region Project
        /// <summary>
        /// Die Assemblyklassen werden entsprechend ihres ACKind unter "Root" oder "ClassLibrary"
        /// abgespeichert.
        /// TODO: Damir: Prüfen ob WCFServiceManager, WCFClientManager, WCFClientChannel und WCFServiceChannel
        ///            auch unter "Root" aufgehängt werden.
        /// </summary>
        /// <param name="acType"></param>
        /// <param name="classType"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        public ACProject GetACProjectByACType(Global.ACKinds acType, Type classType)
        {
            if (classType.IsAbstract)
                return ACProjectClassLibrary;
            switch (acType)
            {
                case Global.ACKinds.TACRoot:
                case Global.ACKinds.TACBusinessobjects:
                case Global.ACKinds.TACBSO:
                case Global.ACKinds.TACBSOGlobal:
                case Global.ACKinds.TACBSOReport:
                case Global.ACKinds.TACDBAManager:
                case Global.ACKinds.TACDBA:
                case Global.ACKinds.TACEnvironment:
                case Global.ACKinds.TACRuntimeDump:
                case Global.ACKinds.TACMessages:
                case Global.ACKinds.TACQueries:      // Für TACQRY gibt es nur eine Implementierung, die in der ClassLibrary gespeichert wird
                case Global.ACKinds.TACCommunications:
                case Global.ACKinds.TACWCFServiceManager:
                case Global.ACKinds.TACWCFServiceChannel:
                case Global.ACKinds.TACWCFClientManager:
                case Global.ACKinds.TACWCFClientChannel:
                case Global.ACKinds.TACLocalServiceObjects:
                    return RootProject;
                default:
                    return ACProjectClassLibrary;
            }
        }

        ACProject _RootProject = null;
        public ACProject RootProject
        {
            get
            {
                if (_RootProject == null)
                {
                    _RootProject = _Database.ACProject.Where(c => c.ACProjectTypeIndex == (Int16)Global.ACProjectTypes.Root).First();
                }

                return _RootProject;
            }
        }

        ACProject _ACProjectClassLibrary = null;
        public ACProject ACProjectClassLibrary
        {
            get
            {
                if (_ACProjectClassLibrary == null)
                {
                    _ACProjectClassLibrary = _Database.ACProject.Where(c => c.ACProjectTypeIndex == (Int16)Global.ACProjectTypes.ClassLibrary).First();
                }

                return _ACProjectClassLibrary;
            }
        }
        #endregion

        #region StartType
        public Global.ACStartTypes GetACStartTypeByACType(Global.ACKinds acType)
        {
            switch (acType)
            {
                // Root und alle Manager werden automatisch instanziiert
                case Global.ACKinds.TACRoot:
                    return Global.ACStartTypes.AutomaticOnDemand;
                case Global.ACKinds.TACDBAManager:
                    return Global.ACStartTypes.Manually;
                case Global.ACKinds.TACBusinessobjects:
                case Global.ACKinds.TACQueries:
                case Global.ACKinds.TACCommunications:
                case Global.ACKinds.TACMessages:
                case Global.ACKinds.TACEnvironment:
                case Global.ACKinds.TACRuntimeDump:
                case Global.ACKinds.TACLocalServiceObjects:
                    return Global.ACStartTypes.Automatic;
                case Global.ACKinds.TACAbstractClass:
                    return Global.ACStartTypes.None;
                case Global.ACKinds.TACSimpleClass:
                    return Global.ACStartTypes.None;
                case Global.ACKinds.TACInterface:
                    return Global.ACStartTypes.None;
                case Global.ACKinds.TACEnum:
                    return Global.ACStartTypes.None;
                case Global.ACKinds.TACClass:
                    return Global.ACStartTypes.Manually;
                case Global.ACKinds.TACBSO:
                    return Global.ACStartTypes.Manually;
                case Global.ACKinds.TACBSOGlobal:
                    return Global.ACStartTypes.AutomaticOnDemand;
                case Global.ACKinds.TACBSOReport:
                    return Global.ACStartTypes.AutomaticOnDemand;
                case Global.ACKinds.TACQRY:
                    return Global.ACStartTypes.Manually;
                case Global.ACKinds.TACDAClass:
                    return Global.ACStartTypes.AutomaticOnDemand;
                case Global.ACKinds.TACApplicationManager:
                    return Global.ACStartTypes.Manually;
                case Global.ACKinds.TPAModule:
                    return Global.ACStartTypes.Automatic;
                case Global.ACKinds.TPAProcessModule:
                case Global.ACKinds.TPAProcessModuleGroup:
                case Global.ACKinds.TPAProcessFunction:
                    return Global.ACStartTypes.Automatic;
                case Global.ACKinds.TPABGModule:
                    return Global.ACStartTypes.Automatic;
                case Global.ACKinds.TPWMethod:
                case Global.ACKinds.TPWGroup:
                case Global.ACKinds.TPWNode:
                case Global.ACKinds.TPWNodeMethod:
                case Global.ACKinds.TPWNodeWorkflow:
                case Global.ACKinds.TPWNodeStart:
                case Global.ACKinds.TPWNodeEnd:
                case Global.ACKinds.TPWNodeStatic:
                    return Global.ACStartTypes.Automatic;
                default:
                    return Global.ACStartTypes.Disabled;
            }
        }
        #endregion

        #region PWGroup
        ACClass _PWClass = null;
        void UpdatePWACClass(ACClass acClass, string pwACClass)
        {
            ACClass pwACClassnew = null;
            if (_PWClass == null)
                _PWClass = _Database.ACClass.Where(c => c.ACIdentifier == PWGroup.PWClassName && c.ACKindIndex == (Int16)Global.ACKinds.TPWGroup).First();
            if (string.IsNullOrEmpty(pwACClass) || pwACClass == PWGroup.PWClassName)
                pwACClassnew = _PWClass;
            else
            {
                pwACClassnew = _Database.GetACType(pwACClass);
                //pwACClassnew = ACProjectClassLibrary.ACClass_ACProject.Where(c => c.ACIdentifier == pwACClass).FirstOrDefault();
                if (pwACClassnew == null)
                    pwACClassnew = _PWClass;
            }
            if (acClass.ACClass1_PWACClass != pwACClassnew)
                acClass.ACClass1_PWACClass = pwACClassnew;
        }


        ACClass _PWMethodACClass = null;
        void UpdatePWMethodACClass(ACClass acClass, string pwMethodACClass)
        {
            ACClass pwACClassnew = null;
            if (_PWMethodACClass == null)
                _PWMethodACClass = _Database.ACClass.Where(c => c.ACIdentifier == PWProcessFunction.PWClassName && c.ACKindIndex == (Int16)Global.ACKinds.TPAProcessFunction).First();
            if (string.IsNullOrEmpty(pwMethodACClass) || pwMethodACClass == PWProcessFunction.PWClassName)
                pwACClassnew = _PWMethodACClass;
            else
            {
                pwACClassnew = _Database.GetACType(pwMethodACClass);
                //pwACClassnew = ACProjectClassLibrary.ACClass_ACProject.Where(c => c.ACIdentifier == pwMethodACClass).FirstOrDefault();
                if (pwACClassnew == null)
                    pwACClassnew = _PWMethodACClass;
            }
            if (acClass.ACClass1_PWMethodACClass != pwACClassnew)
                acClass.ACClass1_PWMethodACClass = pwACClassnew;
        }
        #endregion

        void InsertOrUpdateMethods(ACClass acClass, Type ClassType, int recursionDepth)
        {
            recursionDepth++;
            if (acClass.ACKind != Global.ACKinds.TACUndefined)
            {
                List<string> ClassMethodsList = new List<string>();
                if (typeof(IACComponent).IsAssignableFrom(ClassType))
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(ClassType.TypeHandle); // Invoke Static Constructor, to get virtual method infos

                // Iterate through all methods
                foreach (MethodInfo Method in ClassType.GetMethods())
                {
                    ACClass attachedFromACClass = null;
                    ACClass acClassToUpdate = acClass;
                    ACClass ValueClass = null;
                    ACMethodInfo Info = null;
                    ACClassMethod ClassMethod = null;
                    Type DataType = null;
                    Type GenericType = null;
                    string GenericTypeString = "";
                    string AssemblyQualifiedName = "";

                    // Check if method is declared in current class or it is inherited
                    if (ClassType != Method.DeclaringType) continue;

                    // Get ACMethodInfo attribute
                    Info = Method.GetCustomAttributes<ACMethodInfo>(false).FirstOrDefault();

                    if (Info == null)
                    {
                        // Don't refect methods from interfaces:
                        //if (acClassToUpdate.ACKind == Global.ACKinds.TACInterface && !Method.IsSpecialName && !Method.IsGenericMethod)
                            //Info = new ACMethodInfo("", "en{'" + Method.Name + "'}de{'" + Method.Name + "'}", 9999, false, false, false);
                        //else
                            continue;
                    }
                    else if (Info.AttachToClass != null)
                    {
                        string attClassName = "";
                        string attClassFullName = "";
                        string attAssemblyQualifiedName = "";
                        ExtractClassType(Info.AttachToClass, ref attClassName, ref attClassFullName, ref attAssemblyQualifiedName);
                        if (!String.IsNullOrEmpty(attAssemblyQualifiedName))
                        {
                            var query = s_compiledQueryACClass_AssemblyName.Invoke(_Database, attAssemblyQualifiedName);
                            var attachToACClass = query.FirstOrDefault();
                            if (attachToACClass != null)
                            {
                                attachedFromACClass = acClass;
                                acClassToUpdate = attachToACClass;
                            }
                        }

                        if (attachedFromACClass == null)
                            continue;
                    }

                    // Add method to the list
                    ClassMethodsList.Add(Method.Name);

                    // Check if method already exists in database and if not create new one
                    ClassMethod = acClassToUpdate.ACClassMethod_ACClass.Where(c => c.ACIdentifier == Method.Name).FirstOrDefault();
                    if (ClassMethod == null)
                    {
                        ClassMethod = ACClassMethod.NewACObject(_Database, acClassToUpdate);

                        ClassMethod.InsertDate = DateTime.Now;
                        ClassMethod.InsertName = ACRoot.SRoot.CurrentInvokingUser.VBUserName;

                        acClassToUpdate.AddNewACClassMethod(ClassMethod);
                        //_Database.ACClassMethod.AddObject(ClassMethod);

                        ClassMethod.ACIdentifier = Method.Name;

                        ClassMethod.IsRightmanagement = Info.IsRightmanagement;
                        ClassMethod.ContextMenuCategoryIndex = Info.ContextMenuCategoryIndex;
                        if (ClassMethod.EntityState == EntityState.Detached)
                            _Database.ACClassMethod.Add(ClassMethod);
                    }

                    // Get method description
                    if (acClassToUpdate.ObjectType != null)
                    {
                        string Summary = ACFileItems.GetSummaryMethod(acClassToUpdate, Method);

                        if (!string.IsNullOrWhiteSpace(Summary) && ClassMethod.Comment != Summary)
                            ClassMethod.Comment = Summary;
                    }

                    // Update translation
                    Translator.UpdateTranslation(ClassMethod, Info.ACCaptionTranslation);

                    // Check and update method properties
                    if (ClassMethod.ACGroup != Info.ACGroup)
                        ClassMethod.ACGroup = Info.ACGroup;

                    if (ClassMethod.ACKind != Info.ACKind)
                        ClassMethod.ACKind = Info.ACKind;

                    if (ClassMethod.IsAutoenabled != (ClassType.GetMethod(Const.IsEnabledPrefix + Method.Name) == null))
                        ClassMethod.IsAutoenabled = (ClassType.GetMethod(Const.IsEnabledPrefix + Method.Name) == null);

                    if (ClassMethod.IsInteraction != Info.IsInteraction)
                        ClassMethod.IsInteraction = Info.IsInteraction;

                    if (ClassMethod.IsCommand != Info.IsCommand && ClassMethod.IsCommand == false)
                        ClassMethod.IsCommand = Info.IsCommand;

                    if (ClassMethod.IsAsyncProcess != Info.IsAsyncProcess)
                        ClassMethod.IsAsyncProcess = Info.IsAsyncProcess;

                    if (ClassMethod.IsPeriodic != Info.IsPeriodic)
                        ClassMethod.IsPeriodic = Info.IsPeriodic;

                    if (ClassMethod.InteractionVBContent != Info.InteractionVBContent)
                        ClassMethod.InteractionVBContent = Info.InteractionVBContent;

                    if (ClassMethod.SortIndex != Info.SortIndex)
                        ClassMethod.SortIndex = Info.SortIndex;

                    if (ClassMethod.IsRightmanagement != Info.IsRightmanagement)
                        ClassMethod.IsRightmanagement = Info.IsRightmanagement;

                    if (ClassMethod.ContextMenuCategoryIndex != Info.ContextMenuCategoryIndex)
                        ClassMethod.ContextMenuCategoryIndex = Info.ContextMenuCategoryIndex;

                    if (ClassMethod.IsRPCEnabled != Info.IsRPCEnabled)
                        ClassMethod.IsRPCEnabled = Info.IsRPCEnabled;

                    if (ClassMethod.AttachedFromACClass != attachedFromACClass)
                        ClassMethod.AttachedFromACClass = attachedFromACClass;

                    if (ClassMethod.IsStatic != Method.IsStatic)
                        ClassMethod.IsStatic = Method.IsStatic;

                    // Check method return type
                    DetermineDataType(Method.ReturnType, ref AssemblyQualifiedName, ref GenericTypeString, ref GenericType, ref DataType);

                    // Check generic type string length
                    if (ClassMethod.GenericType != GenericTypeString)
                    {
                        if (GenericTypeString.Length > 100)
                            ClassMethod.GenericType = GenericTypeString.Substring(0, 100);
                        else
                            ClassMethod.GenericType = GenericTypeString;
                    }

                    // Try get class info for method return type
                    ValueClass = GetCheckedAssemblyACClass(AssemblyQualifiedName);

                    // If return type is not registered in database, create new one
                    if (ValueClass == null)
                        ValueClass = InsertOrUpdateValueTypeACClass(DataType, false, true, recursionDepth);

                    // Check and update return value type property, if needed
                    if (ClassMethod.ValueTypeACClass != ValueClass)
                        ClassMethod.ValueTypeACClass = ValueClass;

                    // Check PWClass property
                    if (!string.IsNullOrWhiteSpace(Info.PWClassMethod) && (ClassMethod.PWACClass == null || ClassMethod.PWACClass.ACIdentifier != Info.PWClassMethod))
                    {
                        IEnumerable<ACClass> Query = ACProjectClassLibrary.ACClass_ACProject.Where(c => c.ACIdentifier == Info.PWClassMethod);

                        if (Query.Any())
                            ClassMethod.PWACClass = Query.First();
                    }

                    // Update ACMethod (Method parameters) and virtual methods, if any
                    InsertOrUpdateMethodParameters(ClassType, ClassMethod, Method, recursionDepth);

                    if (ClassMethod.EntityState != EntityState.Unchanged)
                    {
                        ClassMethod.UpdateDate = DateTime.Now;
                        ClassMethod.UpdateName = ACRoot.SRoot.CurrentInvokingUser.VBUserName;
                    }
                }

                // If class is not added, delete obsolete methods from the class methods list
                if (acClass.EntityState != EntityState.Added)
                {
                    ACClassMethod[] Items = acClass.ACClassMethod_ACClass.Where(c => (c.ACKindIndex == (short)Global.ACKinds.MSMethod
                                                                                      || c.ACKindIndex == (short)Global.ACKinds.MSMethodPrePost
                                                                                      || c.ACKindIndex == (short)Global.ACKinds.MSMethodClient)
                                                                                   && c.ACClassMethod1_ParentACClassMethod == null
                                                                                   && !c.AttachedFromACClassID.HasValue
                                                                             ).ToArray();

                    for (int i = 0; i < Items.Length; i++)
                    {
                        if (!ClassMethodsList.Contains(Items[i].ACIdentifier))
                            Items[i].DeleteACObject(_Database, false);
                    }
                }
            }
        }

        /// <summary>
        /// Aktualisiert den ACClassMethod-Datensätze entsprechend der XMLACMethodInfo-Attribute
        /// 1. Aktualisierung des XMLACMethod für die Assemblymethode
        /// 2. Neuanlage/Aktualisierung der konfigurierten Methodenaufrufe (neue ACClassMethod Datensätze, die sich mit ParentACClassMethodID auf die Assemblymethode beziehen
        /// </summary>
        /// <param name="ClassType"></param>
        /// <param name="ClassMethod"></param>
        /// <param name="Method"></param>
        /// <param name="recursionDepth">Rekusiondepth</param>
        void InsertOrUpdateMethodParameters(Type ClassType, ACClassMethod ClassMethod, MethodInfo Method, int recursionDepth)
        {
            recursionDepth++;
            string XML = null;
            ACMethod Argument = null;
            ACMethodWrapper virtMethodInfoWithSameAssemblyMethodName = null;
            ParameterInfo[] Parameters = Method.GetParameters();

            Type T;
            Type DataType = null;
            Type GenericType = null;
            string GenericTypeString = "";
            string AssemblyQualifiedName = "";

            /*************************************************************************************
             * Since Norbert's concept is overcomplicated, here is a breif description of
             * different ways to insert or update method info (ACClassMethod with ACMethod as XML):
             * 
             * 1) If assembly method doesn't have any virtual overrides, ACMethod is generated
             *    and serialized to database as XML with ACClassMethod
             * 
             * 2) If assembly method does have virtual overrides, but none of them has the same
             *    name as assembly method, ACMethod is generated and serialized to database and
             *    all virtual overrides are saved to database as ACClassMethod with parent value
             *    set to assembly method. Virtual ACMethods are not serialized to database as XML
             *    since they can be retreived from static property defined in assembly code.
             *    
             * 3) If assembly method has virtual override with same name as assembly method name,
             *    virtual override will be saved instead of assembly method as ACClassMethod.
             *    The catch is that virtual override method can have parameters, even is assembly
             *    method doesn't, in which case these parameters are loaded using "Configuration
             *    concept".
             * 
             *************************************************************************************/

            // Check if there are any virtual methods defined for current method info
            IReadOnlyList<ACMethodWrapper> virtualMethods = ACMethod.GetVirtualMethodInfos(ClassType, Method.Name);
            if (virtualMethods != null)
            {
                // Check if there is a virtual method argument with same name as assembly method (Method argument override)
                virtMethodInfoWithSameAssemblyMethodName = virtualMethods.Where(c => c.Method.ACIdentifier == Method.Name).FirstOrDefault();
            }

            // If virtual method argument override with same name was not found create new method argument
            if (virtMethodInfoWithSameAssemblyMethodName == null)
                Argument = new ACMethod(Method.Name);

            // Fill argument from MethodInfo
            if (Argument != null)
            {
                // Get all parameters
                foreach (ParameterInfo Parameter in Parameters)
                {
                    if (Parameter.ParameterType.IsByRef && Parameter.ParameterType.HasElementType)
                        T = Parameter.ParameterType.GetElementType();
                    else
                        T = Parameter.ParameterType;

                    DetermineDataType(T, ref AssemblyQualifiedName, ref GenericTypeString, ref GenericType, ref DataType);

                    // Check if parameter type is registered in database
                    if (!DataType.IsGenericParameter && !_Database.ACClass.Where(c => c.AssemblyQualifiedName == DataType.AssemblyQualifiedName).Any())
                    {
                        InsertOrUpdateValueTypeACClass(DataType, false, true, recursionDepth);

                        MsgWithDetails Result = _Database.ACSaveChanges(true, true, true);
                        if (Result != null)
                        {
                            Messages.GlobalMsg.AddDetailMessage(Result);
                            _Database.ACUndoChanges();
                            throw new Exception("InsertOrUpdateMethodParameters(), Type(" + DataType.AssemblyQualifiedName + ") " + Result.InnerMessage);
                        }
                    }

                    Argument.ParameterValueList.Add(new ACValue(Parameter.Name, T));
                }

                // Get method result
                if (Method.ReturnType != typeof(void) && !String.IsNullOrEmpty(Method.ReturnType.FullName))
                {
                    // Check if return type is registered in database
                    DetermineDataType(Method.ReturnType, ref AssemblyQualifiedName, ref GenericTypeString, ref GenericType, ref DataType);

                    if (!_Database.ACClass.Where(c => c.AssemblyQualifiedName == DataType.AssemblyQualifiedName).Any())
                    {
                        InsertOrUpdateValueTypeACClass(DataType, false, true, recursionDepth);

                        MsgWithDetails Result = _Database.ACSaveChanges(true, true, true);
                        if (Result != null)
                        {
                            Messages.GlobalMsg.AddDetailMessage(Result);
                            _Database.ACUndoChanges();
                            throw new Exception("InsertOrUpdateMethodParameters(), Type(" + DataType.AssemblyQualifiedName + ") " + Result.InnerMessage);
                        }
                    }

                    Argument.ResultValueList.Add(new ACValue("result", Method.ReturnType));
                }

                if (virtualMethods != null && virtualMethods.Any())
                {
                    foreach (ACMethodWrapper MW in virtualMethods)
                    {
                        InsertOrUpdateVirtualMethod(ClassMethod, MW);
                    }
                }
            }
            // Assembly-method doesn't have a ACMethod as parameter (this is the most case for SMStarting()-MEthods of PWProcessFunctions
            else
            {
                Argument = virtMethodInfoWithSameAssemblyMethodName.Method;
                // Falls ACMethodInfo-Attribut über SMStarting-Methode gesetzt worden ist
                if (Method.DeclaringType == ClassType)
                    XML = ACClassMethod.SerializeACMethod(Argument);

                if (virtualMethods != null && virtualMethods.Any())
                {
                    foreach (ACMethodWrapper MW in virtualMethods.Where(c => c.Method != Argument))
                    {
                        InsertOrUpdateVirtualMethod(ClassMethod, MW);
                    }
                }
            }

            // Serialize Argument to XML if virtual override with same name was not found
            if (virtMethodInfoWithSameAssemblyMethodName == null)
                XML = ACClassMethod.SerializeACMethod(Argument);

            // Check if ClassMethod's XML is not eual to argument
            if (ClassMethod.XMLACMethod != XML)
            {
                // Get value indicating if Argument is method's parameter ??????
                bool IsParameter = false;
                int countParam = Argument.ParameterValueList.Count;
                if (countParam == 1)
                    IsParameter = typeof(ACMethod).IsAssignableFrom(Argument.ParameterValueList.First().ObjectType);

                ClassMethod.XMLACMethod = XML;

                if (ClassMethod.IsParameterACMethod != IsParameter)
                {
                    ClassMethod.IsParameterACMethod = IsParameter;

                    if (ClassMethod.ACClassMethod_ParentACClassMethod != null)
                    {
                        foreach (ACClassMethod Child in ClassMethod.ACClassMethod_ParentACClassMethod)
                        {
                            Child.IsParameterACMethod = IsParameter;
                        }
                    }
                }
            }
        }

        void InsertOrUpdateVirtualMethod(ACClassMethod ClassMethod, ACMethodWrapper VirtualMethod)
        {
            ACClassMethod ChildClassMethod = null;

            // Try to get child class method from database
            if (ClassMethod.ACClassMethod_ParentACClassMethod == null)
                ChildClassMethod = null;
            else
                ChildClassMethod = ClassMethod.ACClassMethod_ParentACClassMethod.Where(c => c.ACIdentifier == VirtualMethod.Method.ACIdentifier).FirstOrDefault();
            //if (VirtualMethod.PWClass != null)
            //    ChildClassMethod = ClassMethod.ACClassMethod_ParentACClassMethod.Where(c => c.ACIdentifier == VirtualMethod.Method.ACIdentifier && c.PWACClass != null && c.PWACClass.ACIdentifier == VirtualMethod.PWClass.Name).FirstOrDefault();

            //if (ChildClassMethod == null)
            //    ChildClassMethod = ClassMethod.ACClassMethod_ParentACClassMethod.Where(c => c.ACIdentifier == VirtualMethod.Method.ACIdentifier && c.PWACClass == null).FirstOrDefault();

            // If child is not found, create new with provided class method as parent
            if (ChildClassMethod == null)
            {
                ChildClassMethod = ACClassMethod.NewACObject(_Database, ClassMethod);

                ChildClassMethod.ACIdentifier = VirtualMethod.Method.ACIdentifier;
                ChildClassMethod.ACKind = ClassMethod.ACKind;

                ChildClassMethod.InsertDate = DateTime.Now;
                ChildClassMethod.InsertName = ACRoot.SRoot.CurrentInvokingUser.VBUserName;

                ClassMethod.ACClass.AddNewACClassMethod(ClassMethod);
                if (ChildClassMethod.EntityState == EntityState.Detached)
                    _Database.ACClassMethod.Add(ChildClassMethod);
            }

            if (ChildClassMethod.ACCaptionTranslation != VirtualMethod.CaptionTranslation && !string.IsNullOrEmpty(VirtualMethod.CaptionTranslation))
                ChildClassMethod.ACCaptionTranslation = VirtualMethod.CaptionTranslation;

            if (ChildClassMethod.IsAsyncProcess != ChildClassMethod.ACClassMethod1_ParentACClassMethod.IsAsyncProcess)
                ChildClassMethod.IsAsyncProcess = ChildClassMethod.ACClassMethod1_ParentACClassMethod.IsAsyncProcess;

            if (ChildClassMethod.IsPeriodic != ChildClassMethod.ACClassMethod1_ParentACClassMethod.IsPeriodic)
                ChildClassMethod.IsPeriodic = ChildClassMethod.ACClassMethod1_ParentACClassMethod.IsPeriodic;

            if (VirtualMethod.PWClass == null)
            {
                if (ChildClassMethod.PWACClass != null)
                    ChildClassMethod.PWACClass = null;
            }
            else if (ChildClassMethod.PWACClass == null || ChildClassMethod.PWACClass.ACIdentifier != VirtualMethod.PWClass.Name)
            {
                ACClass TMP = ACProjectClassLibrary.ACClass_ACProject.Where(c => c.ACIdentifier == VirtualMethod.PWClass.Name).FirstOrDefault();

                if (ChildClassMethod.PWACClass != TMP)
                    ChildClassMethod.PWACClass = TMP;
            }

            if (VirtualMethod.Method != null)
            {
                string XML = ACClassMethod.SerializeACMethod(VirtualMethod.Method);

                // Check if ClassMethod's XML is not eual to argument
                if (ChildClassMethod.XMLACMethod != XML)
                {
                    ChildClassMethod.XMLACMethod = XML;
                }
                // If there are any attached methods at ProcessModules in Library, ProjectDefinitions or Projects then overwrite XML
                if (ChildClassMethod.ACClassMethod_ParentACClassMethod != null)
                {
                    var queryAttachedMethods = ChildClassMethod.ACClassMethod_ParentACClassMethod.Where(c => c.ACKindIndex == (short)Global.ACKinds.MSMethodFunction);
                    if (queryAttachedMethods.Any())
                    {
                        foreach (var attachedVirtualMethod in queryAttachedMethods)
                        {
                            if (attachedVirtualMethod.XMLACMethod != XML)
                                attachedVirtualMethod.XMLACMethod = XML;
                        }
                    }
                }
            }

            if (ChildClassMethod.EntityState != EntityState.Unchanged)
            {
                ChildClassMethod.UpdateDate = DateTime.Now;
                ChildClassMethod.UpdateName = ACRoot.SRoot.CurrentInvokingUser.VBUserName;
            }
        }

        ACMethod GetACMethod(string ClassName)
        {
            // Try to get class (derivation of ACMethod)
            ACClass TMP = _Database.ACClass.Where(c => c.ACIdentifier == ClassName).FirstOrDefault();

            // If not found create new ACMethod
            if (TMP == null)
                return new ACMethod();
            else
                return (ACMethod)Activator.CreateInstance(TMP.ObjectType);
        }

        private bool _OverwriteXMLACMethods = false;

        void InsertOrUpdateACClassProperties(ACClass acClass, Type acClassType, bool updateIfExists, int recursionDepth)
        {
            recursionDepth++;
            if (acClass.ACKind == Global.ACKinds.TACUndefined)
                return;

            IEnumerable<PropertyInfo> propertyList;
            if (acClass.ACKind == Global.ACKinds.TACDBAManager)
                propertyList = acClassType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
            else
                propertyList = acClassType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

            foreach (PropertyInfo property in propertyList)
            {
                // Wenn Set- und Get-Accessor nicht public, dann weiter
                //if (!property.GetAccessors().Where(c => c.IsPublic).Any() || property.GetAccessors().Where(c => c.IsStatic).Any())
                //continue;

                ACPropertyBase acPropertyInfo = property.GetCustomAttributes(false).Where(c => c.GetType().Name.StartsWith(Const.ACPropertyPrefix)).FirstOrDefault() as ACPropertyBase;
                if (acPropertyInfo != null)
                {
                    // Falls "Configuration", dann kein ACClassProperty, sondern ein ACClassConfig-Eintrag
                    if (acPropertyInfo.ACPropUsage == Global.ACPropUsages.Configuration)
                    {
                        InsertOrUpdateConfiguration(acClass, property, acPropertyInfo as ACPropertyConfig, recursionDepth);
                        continue;
                    }
                }
                else
                {
                    switch (acClass.ACKind)
                    {
                        case Global.ACKinds.TACDBA:
                            {
                                //var query = _Database.VBSystemColumns.Where(c => c.tablename == acClass.ACIdentifier && c.columnname == property.Name);
                                //if (!query.Any())
                                //{
                                //    var test = property.GetCustomAttributes(false);
                                //    var test2 = _Database.VBSystemColumns.Where(c => c.tablename == acClass.ACIdentifier);
                                //    if (!property.GetCustomAttributes(false).Where(c => c.GetType().Name == "EdmRelationshipNavigationPropertyAttribute").Any())
                                //        continue;
                                //}
                                if (property.Name.EndsWith("ID")
                                    && (property.PropertyType == typeof(System.Guid)
                                        || property.PropertyType == typeof(Nullable<System.Guid>)))
                                    continue;

                                if (property.PropertyType.ToString().IndexOf("EntityCollection") != -1)
                                    acPropertyInfo = new ACPropertyRelationPoint(9999);
                                else if (property.Name == Const.EntityXMLConfig)
                                    continue;
                                else
                                {
                                    acPropertyInfo = null;
                                    object[] attributes = acClassType.GetCustomAttributes(typeof(ACPropertyEntity), false);
                                    if (attributes != null)
                                        acPropertyInfo = attributes.Where(c => ((ACPropertyEntity)c).ACIdentifier == property.Name).FirstOrDefault() as ACPropertyBase;
                                    if (acPropertyInfo == null)
                                    {
                                        if (property.Name == Const.EntityInsertName
                                            || property.Name == Const.EntityInsertDate
                                            || property.Name == Const.EntityUpdateName
                                            || property.Name == Const.EntityUpdateDate
                                            || property.Name == Const.EntityDeleteName
                                            || property.Name == Const.EntityDeleteDate)
                                        {
                                            if (property.Name == Const.EntityInsertName)
                                                acPropertyInfo = new ACPropertyInfo(497, "", Const.EntityTransInsertName);
                                            else if (property.Name == Const.EntityInsertDate)
                                                acPropertyInfo = new ACPropertyInfo(496, "", Const.EntityTransInsertDate);
                                            else if (property.Name == Const.EntityUpdateName)
                                                acPropertyInfo = new ACPropertyInfo(499, "", Const.EntityTransUpdateName);
                                            else if (property.Name == Const.EntityUpdateDate)
                                                acPropertyInfo = new ACPropertyInfo(498, "", Const.EntityTransUpdateDate);
                                            else if (property.Name == Const.EntityDeleteName)
                                                acPropertyInfo = new ACPropertyInfo(501, "", Const.EntityTransDeleteName);
                                            else //if (property.Name == Const.EntityDeleteDate)
                                                acPropertyInfo = new ACPropertyInfo(500, "", Const.EntityTransDeleteDate);
                                        }
                                        else
                                        {
                                            var foundProp = acClass.ACClassProperty_ACClass.Where(c => c.ACIdentifier == property.Name).FirstOrDefault();
                                            if (foundProp != null)
                                                foundProp.DeleteACObject(_Database, false);
                                            continue;
                                            // Rechteverwaltung bei Properties von Entitäten nur, wenn bei die Entität Rechteverwaltung benötigt
                                            //acPropertyInfo = new ACProperty("", "", "", acClass.IsRightmanagement);
                                        }
                                    }
                                }
                            }
                            break;
                        case Global.ACKinds.TACDBAManager:
                        case Global.ACKinds.TACInterface:
                            acPropertyInfo = new ACPropertyInfo(9999, "", "en{'" + property.Name + "'}de{'" + property.Name + "'}");
                            break;
                        default:
                            continue;
                    }
                }
                InsertOrUpdateACClassProperty(acClass, property, acPropertyInfo, acClassType, recursionDepth);
            }

            foreach (ACClassProperty acClassProperty in acClass.ACClassProperty_ACClass.Where(c => !c.IsStatic
                                                                                                    && (c.ACPropUsageIndex == (short)Global.ACPropUsages.ConnectionPoint
                                                                                                       || (c.ACPropUsageIndex == (short)Global.ACPropUsages.Property && c.ACKindIndex != (short)Global.ACKinds.PSPropertyExt)))
                                                                                        .ToArray())
            {
                PropertyInfo property = propertyList.Where(c => c.Name == acClassProperty.ACIdentifier).FirstOrDefault();
                if (property == null)
                    continue;
                object[] stateInfos = property.GetCustomAttributes(typeof(ACPointStateInfo), true);
                if (stateInfos != null && stateInfos.Any())
                {
                    foreach (ACPointStateInfo stateInfo in stateInfos)
                    {
                        stateInfo.InsertOrUpdate(_Database, acClass, acClassProperty);
                    }
                }
            }

            if (acClass.ACKind == Global.ACKinds.TACClass)
            {
                object[] attributes = acClassType.GetCustomAttributes(typeof(ACPropertyEntity), false);
                if (attributes != null)
                {
                    foreach (ACPropertyEntity propDekl in attributes)
                    {
                        PropertyInfo property = acClassType.GetProperty(propDekl.ACIdentifier);
                        if (property != null)
                        {
                            if (!property.GetAccessors().Where(c => c.IsPublic).Any() || property.GetAccessors().Where(c => c.IsStatic).Any())
                                continue;
                            InsertOrUpdateACClassProperty(acClass, property, propDekl, acClassType, recursionDepth);
                        }
                    }
                }
            }


            List<ACClassProperty> deleteProperties = new List<ACClassProperty>();
            foreach (ACClassProperty acClassProperty in acClass.ACClassProperty_ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.PSProperty).ToList())
            {
                try
                {
                    var query = acClassType.GetProperties().Where(c => c.Name == acClassProperty.ACIdentifier);
                    if (!query.Any())
                    {
                        if (acClassProperty.ACClassTaskValue_ACClassProperty.Any())
                        {
                            foreach (var taskValue in acClassProperty.ACClassTaskValue_ACClassProperty.ToArray())
                            {
                                taskValue.DeleteACObject(_Database, false);
                            }
                        }
                        acClassProperty.DeleteACObject(_Database, false);
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateACClassProperties", msg);
                }
            }
        }

        private void UpdateScriptMethods()
        {
            var query = _Database.ACClassMethod.Where(c => c.ACKindIndex == (Int16)Global.ACKinds.MSMethodExt);
            foreach (var acClassMethod in query)
            {
                acClassMethod.UpdateACMethod();
            }
            _Database.ACSaveChanges(true, true, true);
        }

        private void UpdateACClassACURLCached(Database db)
        {
            Messages.ConsoleMsg("System", "Updating ACURLCached and ACURLComponentCached...");

            IEnumerable<ACClass> acClassToUpdate = db.ACClass.Where(c => c.ACURLCached == null
                                                                || (c.ACURLComponentCached == null
                                                                    && (c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application
                                                                         || c.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Service)));
            foreach (ACClass acClass in acClassToUpdate)
            {
                if (string.IsNullOrEmpty(acClass.ACURLComponentCached)
                    && (acClass.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Application
                        || acClass.ACProject.ACProjectTypeIndex == (short)Global.ACProjectTypes.Service))
                {
                    acClass.ACURLComponentCached = acClass.ACUrlComponent;
                }

                if (string.IsNullOrEmpty(acClass.ACURLCached))
                    acClass.ACURLCached = acClass.ACUrl;

            }

            Msg msg = db.ACSaveChanges(true, true, true);
            if (msg != null)
                Messages.ConsoleMsg("System", msg.Message);
        }

        private void InsertOrUpdateConfiguration(ACClass acClass, PropertyInfo propertyInfo, ACPropertyConfig acPropertyConfig, int recursionDepth)
        {
            recursionDepth++;
            try
            {
                ACClassConfig acClassConfig = acClass.ConfigurationEntries.Where(c => c.KeyACUrl == acClass.ACConfigKeyACUrl && c.LocalConfigACUrl == propertyInfo.Name).FirstOrDefault() as ACClassConfig;
                var valueTypeACClass = GetCheckedAssemblyACClass(propertyInfo.PropertyType.AssemblyQualifiedName);
                if (valueTypeACClass == null)
                    valueTypeACClass = InsertOrUpdateValueTypeACClass(propertyInfo.PropertyType, false, true, recursionDepth);
                if (acClassConfig == null)
                    acClassConfig = acClass.NewACConfig(null, valueTypeACClass, propertyInfo.Name) as ACClassConfig;

                if ((valueTypeACClass != null) && (acClassConfig.ValueTypeACClass != valueTypeACClass))
                {
                    acClassConfig.ValueTypeACClass = valueTypeACClass;
                    if (valueTypeACClass == null)
                        throw new Exception(String.Format("Could not determine ValueTypeACClass for type {3} at ACClassConfig {0} at class {1} ASQN:{2}", propertyInfo.Name, acClass.ACIdentifier, acClass.AssemblyQualifiedName, propertyInfo.DeclaringType != null ? propertyInfo.DeclaringType.ToString() : ""));
                    if (acPropertyConfig.DefaultValue != null)
                    {
                        acClassConfig[Const.Value] = acPropertyConfig.DefaultValue;
                    }
                    else
                    {
                        acClassConfig.SetDefaultValue();
                    }
                }
                if (acClassConfig.ValueTypeACClass == null)
                    throw new Exception(String.Format("Could not determine ValueTypeACClass for type {3} at ACClassConfig {0} at class {1} ASQN:{2}", propertyInfo.Name, acClass.ACIdentifier, acClass.AssemblyQualifiedName, propertyInfo.DeclaringType != null ? propertyInfo.DeclaringType.ToString() : ""));

                if (acClassConfig.Comment != acPropertyConfig.ACCaptionTranslation)
                    acClassConfig.Comment = acPropertyConfig.ACCaptionTranslation;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateConfiguration", msg);
            }
        }

        private void InsertOrUpdateACClassProperty(ACClass acClass, PropertyInfo propertyInfo, ACPropertyBase acPropertyInfo, Type classType, int recursionDepth)
        {
            recursionDepth++;
            try
            {
                // TODO: Properties mit Delegate-Typen können nicht angelegt werden
                if (typeof(Delegate).IsAssignableFrom(propertyInfo.PropertyType))
                    return;
                ACClassProperty acClassProperty = acClass.ACClassProperty_ACClass.Where(c => c.ACIdentifier == propertyInfo.Name).FirstOrDefault();
                if (acClassProperty == null)
                {
                    acClassProperty = ACClassProperty.NewACObject(_Database, acClass);
                    acClassProperty.ConfigACClass = null;
                    if (acClassProperty.EntityState == EntityState.Detached)
                        _Database.ACClassProperty.Add(acClassProperty);
                    acClassProperty.ACIdentifier = propertyInfo.Name.Length > 100 ? propertyInfo.Name.Substring(0, 100) : propertyInfo.Name;
                    acClassProperty.ACKind = Global.ACKinds.PSProperty;
                    acClassProperty.IsStatic = propertyInfo.GetAccessors(true)[0].IsStatic;

                    acClassProperty.MaxLength = null;
                    //acClassProperty.IsNullable = true;
                    acClassProperty.IsRightmanagement = acPropertyInfo.IsRightmanagement;
                }

                if (propertyInfo.PropertyType.Name == typeof(ACChildItem<>).Name && propertyInfo.GetCustomAttribute<ACChildInfo>() != null)
                {
                    ACChildInfo childInfoAttribute = propertyInfo.GetCustomAttribute<ACChildInfo>();
                    ProcessComponentChildItem(_Database, acClass, childInfoAttribute);
                }

                if (acClass.ObjectType != null)
                {
                    string summary = ACFileItems.GetSummaryProperty(acClass, propertyInfo, acPropertyInfo);
                    if (!string.IsNullOrEmpty(summary) && acClassProperty.Comment != summary)
                        acClassProperty.Comment = summary;
                }

                if (!acClassProperty.IsCaptionCustomized)
                {
                    Translator.UpdateTranslation(acClassProperty, acPropertyInfo.ACCaptionTranslation);
                }

                if (acClassProperty.IsStatic != propertyInfo.GetAccessors(true)[0].IsStatic)
                {
                    acClassProperty.IsStatic = propertyInfo.GetAccessors(true)[0].IsStatic;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.IsStatic = acClassProperty.IsStatic;
                    }
                }

                if (acClassProperty.IsInput != (propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null))
                {
                    acClassProperty.IsInput = (propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null);
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.IsInput = acClassProperty.IsInput;
                    }
                }
                if (acClassProperty.IsOutput != propertyInfo.CanRead)
                {
                    acClassProperty.IsOutput = propertyInfo.CanRead;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.IsOutput = acClassProperty.IsOutput;
                    }
                }
                if (acClassProperty.ACGroup != acPropertyInfo.ACGroup)
                {
                    acClassProperty.ACGroup = acPropertyInfo.ACGroup;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.ACGroup = acPropertyInfo.ACGroup;
                    }
                }
                if (acClassProperty.ACPropUsage != acPropertyInfo.ACPropUsage)
                {
                    acClassProperty.ACPropUsage = acPropertyInfo.ACPropUsage;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.ACPropUsage = acPropertyInfo.ACPropUsage;
                    }
                }
                if (acClassProperty.IsBroadcast != acPropertyInfo.IsBroadcast)
                {
                    acClassProperty.IsBroadcast = acPropertyInfo.IsBroadcast;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.IsBroadcast = acPropertyInfo.IsBroadcast;
                    }
                }
                if (acClassProperty.IsProxyProperty != acPropertyInfo.IsProxyProperty)
                {
                    acClassProperty.IsProxyProperty = acPropertyInfo.IsProxyProperty;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.IsProxyProperty = acPropertyInfo.IsProxyProperty;
                    }
                }
                if (acClassProperty.ForceBroadcast != acPropertyInfo.ForceBroadcast)
                {
                    acClassProperty.ForceBroadcast = acPropertyInfo.ForceBroadcast;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.ForceBroadcast = acPropertyInfo.ForceBroadcast;
                    }
                }
                if (acClassProperty.IsPersistable != acPropertyInfo.IsPersistable)
                {
                    acClassProperty.IsPersistable = acPropertyInfo.IsPersistable;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.IsPersistable = acPropertyInfo.IsPersistable;
                    }
                }
                if (acClassProperty.SortIndex != acPropertyInfo.SortIndex)
                {
                    acClassProperty.SortIndex = acPropertyInfo.SortIndex;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.SortIndex = acClassProperty.SortIndex;
                    }
                }
                if (acClassProperty.ACSource != acPropertyInfo.ACSource)
                {
                    acClassProperty.ACSource = acPropertyInfo.ACSource;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.ACSource = acClassProperty.ACSource;
                    }
                }
                if (acClassProperty.IsRightmanagement != acPropertyInfo.IsRightmanagement)
                {
                    acClassProperty.IsRightmanagement = acPropertyInfo.IsRightmanagement;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.IsRightmanagement = acClassProperty.IsRightmanagement;
                    }
                }
                if (acClassProperty.IsRPCEnabled != acPropertyInfo.IsRPCEnabled)
                {
                    acClassProperty.IsRPCEnabled = acPropertyInfo.IsRPCEnabled;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.IsRPCEnabled = acClassProperty.IsRPCEnabled;
                    }
                }
                if (acClassProperty.RemotePropID != acPropertyInfo.RemotePropID)
                {
                    acClassProperty.RemotePropID = acPropertyInfo.RemotePropID;
                    if (acClassProperty.ACClassProperty_BasedOnACClassProperty.Any())
                    {
                        foreach (var overriddenProperty in acClassProperty.ACClassProperty_BasedOnACClassProperty)
                            overriddenProperty.RemotePropID = acClassProperty.RemotePropID;
                    }
                }
                Int32 pointCapacity = 0;
                if (acPropertyInfo is ACPropertyPoint)
                    pointCapacity = Convert.ToInt32((acPropertyInfo as ACPropertyPoint).PointCapacity);
                if (acClassProperty.ACPointCapacity != pointCapacity)
                    acClassProperty.ACPointCapacity = Convert.ToInt32(pointCapacity);

                // Default-Value
                if (String.IsNullOrEmpty(acClassProperty.XMLValue))
                {
                    if (acPropertyInfo.DefaultValue != null)
                    {
                        try
                        {
                            acClassProperty.Value = ACConvert.ChangeType(acPropertyInfo.DefaultValue, typeof(string), true, _Database);
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (Database.Root != null && Database.Root.Messages != null)
                                Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateACClassProperty", msg);
                        }
                    }
                }

                if ((acPropertyInfo.MinLength >= 0) && !acClassProperty.MinLength.HasValue)
                    acClassProperty.MinLength = acPropertyInfo.MinLength;
                if ((acPropertyInfo.MaxLength >= 0) && !acClassProperty.MaxLength.HasValue)
                    acClassProperty.MaxLength = acPropertyInfo.MaxLength;
                if (!Double.IsNaN(acPropertyInfo.MinValue) && !acClassProperty.MinValue.HasValue)
                    acClassProperty.MinValue = acPropertyInfo.MinValue;
                if (!Double.IsNaN(acPropertyInfo.MaxValue) && !acClassProperty.MaxValue.HasValue)
                    acClassProperty.MaxValue = acPropertyInfo.MaxValue;

                if (acPropertyInfo is ACPropertyEventPointSubscr)
                {
                    string callbackMethod = (acPropertyInfo as ACPropertyEventPointSubscr).CallbackMethod;
                    if (acClassProperty.CallbackMethodName != callbackMethod)
                        acClassProperty.CallbackMethodName = callbackMethod;
                }

                string assemblyQualifiedName = "";
                string genericTypeString = "";
                Type dataType = null;
                ACClass valueTypeACClass = null;
                Type genericType = null;

                // Norbert's Bug: Es wurde vermischt, dass der Typ der Eigenschaft mit dem acPropertyInfo.DataType gleichgesetzt wird
                // der darüber aussagt was der Datentyp des Values des IACConfig-Interfaces ist: 
                //if (string.IsNullOrEmpty(acPropertyInfo.DataType) || acPropertyInfo.ACPropUsage == Global.ACPropUsages.ConfigPointProperty)
                {
                    DetermineDataType(propertyInfo.PropertyType, ref assemblyQualifiedName, ref genericTypeString, ref genericType, ref dataType);
                    if (acClassProperty.GenericType != genericTypeString)
                    {
                        acClassProperty.GenericType = genericTypeString;
                        if (acClassProperty.GenericType.Length > 100)
                            acClassProperty.GenericType = acClassProperty.GenericType.Substring(0, 100);
                        acClassProperty.IsEnumerable = TypeAnalyser.IsEnumerable(genericTypeString);
                    }

                    if (acClassProperty.GenericType == TypeAnalyser._TypeName_EntityCollection)
                    {
                        Global.DeleteAction deleteAction = Global.DeleteAction.None;
                        DbContext context = FindOrCreateObjectContext(classType);
                        if (context != null)
                        {
                            Type tDatabase = context.GetType();
                            PropertyInfo pi1 = tDatabase.GetProperty(acClassProperty.ACClass.ACIdentifier);
                            var objectSet = pi1.GetValue(context, null);
                            PropertyInfo pi2 = objectSet.GetType().GetProperty("EntitySet");
                            var entitySet = pi2.GetValue(objectSet, null) as EntitySet;
                            //var navP = entitySet.ElementType.NavigationProperties.Where(c => c.Name == acClassProperty.ACIdentifier).First();
                            //switch (navP.FromEndMember.DeleteBehavior)
                            //{
                            //    case DeleteBehavior.NoAction:
                            //    case DeleteBehavior.ClientCascade:
                            //    case DeleteBehavior.ClientNoAction:
                            //    case DeleteBehavior.SetNull:
                            //        {
                            //            ACDeleteAction acDeleteAction = acClassProperty.ACClass.ObjectType
                            //                .GetCustomAttributes(typeof(ACDeleteAction), false)
                            //                .Where(c => ((ACDeleteAction)c).ACIdentifier == acClassProperty.ACIdentifier)
                            //                .FirstOrDefault() as ACDeleteAction;
                            //            if (acDeleteAction != null)
                            //                deleteAction = acDeleteAction.DeleteAction;
                            //            else
                            //                deleteAction = Global.DeleteAction.None;
                            //        }
                            //        break;
                            //    case DeleteBehavior.Cascade:
                            //        deleteAction = Global.DeleteAction.Cascade;
                            //        break;
                            //    case DeleteBehavior.Restrict:
                            //        {
                            //            ACDeleteAction acDeleteAction = acClassProperty.ACClass.ObjectType
                            //                .GetCustomAttributes(typeof(ACDeleteAction), false)
                            //                .Where(c => ((ACDeleteAction)c).ACIdentifier == acClassProperty.ACIdentifier)
                            //                .FirstOrDefault() as ACDeleteAction;
                            //            if (acDeleteAction != null)
                            //                deleteAction = acDeleteAction.DeleteAction;
                            //            else
                            //                deleteAction = Global.DeleteAction.None;
                            //        }
                            //        break;
                            //}
                            if (acClassProperty.DeleteActionIndex != (Int16)deleteAction)
                                acClassProperty.DeleteActionIndex = (Int16)deleteAction;
                        }
                    }

                    if (!dataType.IsGenericParameter)
                    {
                        valueTypeACClass = GetCheckedAssemblyACClass(assemblyQualifiedName);
                        if (valueTypeACClass == null)
                        {
                            valueTypeACClass = InsertOrUpdateValueTypeACClass(dataType, false, true, recursionDepth);
                        }
                    }
                    else
                    {
                        valueTypeACClass = GetCheckedAssemblyACClass(typeof(object).AssemblyQualifiedName);
                        if (valueTypeACClass == null)
                        {
                            valueTypeACClass = InsertOrUpdateValueTypeACClass(typeof(object), false, true, recursionDepth);
                        }
                    }
                }

                ACClass configACClass = null;
                string configAssemblyQualifiedName = "";
                string configGenericTypeString = "";
                Type configDataType = null;
                Type configGenericType = null;
                if (/*(acPropertyInfo.ACPropUsage == Global.ACPropUsages.ConfigPointProperty ||
                    acPropertyInfo.ACPropUsage == Global.ACPropUsages.ConfigPointData) && */
                    acPropertyInfo.ConfigDataType != null)
                {
                    DetermineDataType(acPropertyInfo.ConfigDataType, ref configAssemblyQualifiedName, ref configGenericTypeString, ref configGenericType, ref configDataType);
                    configACClass = GetCheckedAssemblyACClass(configAssemblyQualifiedName); // Fall 1: !Null: Datentyp schon vorhanden
                    if (configACClass == null)
                    {
                        if (configDataType != null)
                            configACClass = InsertOrUpdateValueTypeACClass(configDataType, false, true, recursionDepth); // Fall 2: Datentyp in Datenbank einfügen
                    }
                }

                // Falls DatenTyp in ACPropertyInfo vorgegeben/überschrieben
                if (configACClass != null)
                {
                    if (configDataType == null)
                        configDataType = configACClass.ObjectFullType;
                    // Fall 1: Index-Property(Int16) einer Entity, die einen Enum-Wert darstellt
                    if ((configDataType != null) && (dataType != null) && (dataType.IsValueType) && (configDataType.IsEnum))
                    {
                        if (acClassProperty.ValueTypeACClass != configACClass)
                        {
                            if (valueTypeACClass == null)
                                throw new Exception(String.Format("Could not determine configACClass for type {4} at property {0} ASQN:{1} at class {2} ASQN:{3}", acClassProperty.ACIdentifier, assemblyQualifiedName, acClass.ACIdentifier, acClass.AssemblyQualifiedName, dataType != null ? dataType.ToString() : ""));
                            acClassProperty.ValueTypeACClass = configACClass;
                        }
                        if (acClassProperty.ConfigACClass != null)
                            acClassProperty.ConfigACClass = null;
                    }
                    // Fall 2: Virtuelle Tabelle
                    else if (!String.IsNullOrEmpty(acPropertyInfo.VirtualTableType))
                    {
                        if (acClassProperty.ValueTypeACClass != configACClass)
                        {
                            acClassProperty.ValueTypeACClass = configACClass;
                            if (valueTypeACClass == null)
                                throw new Exception(String.Format("Could not determine configACClass for type {4} at property {0} ASQN:{1} at class {2} ASQN:{3}", acClassProperty.ACIdentifier, assemblyQualifiedName, acClass.ACIdentifier, acClass.AssemblyQualifiedName, dataType != null ? dataType.ToString() : ""));
                        }
                        if (acClassProperty.ConfigACClass != null)
                            acClassProperty.ConfigACClass = null;
                    }
                    // Sonst Fall 3: Datentyp der Config-Value Property
                    else
                    {
                        if (acClassProperty.ValueTypeACClass != valueTypeACClass)
                        {
                            acClassProperty.ValueTypeACClass = valueTypeACClass;
                            if (valueTypeACClass == null)
                                throw new Exception(String.Format("Could not determine ValueTypeACClass for type {4} at property {0} ASQN:{1} at class {2} ASQN:{3}", acClassProperty.ACIdentifier, assemblyQualifiedName, acClass.ACIdentifier, acClass.AssemblyQualifiedName, dataType != null ? dataType.ToString() : ""));
                        }
                        if (acClassProperty.ConfigACClass != configACClass)
                            acClassProperty.ConfigACClass = configACClass;
                    }
                }
                else
                {
                    if (acClassProperty.ValueTypeACClass != valueTypeACClass)
                    {
                        acClassProperty.ValueTypeACClass = valueTypeACClass;
                        if (valueTypeACClass == null)
                            throw new Exception(String.Format("Could not determine ValueTypeACClass for type {4} at property {0} ASQN:{1} at class {2} ASQN:{3}", acClassProperty.ACIdentifier, assemblyQualifiedName, acClass.ACIdentifier, acClass.AssemblyQualifiedName, dataType != null ? dataType.ToString() : ""));
                    }
                    if (acClassProperty.ConfigACClass != null)
                        acClassProperty.ConfigACClass = null;
                }

                if (acClassProperty.ValueTypeACClass == null)
                    throw new Exception(String.Format("ValueTypeACClass is null for type {4} at property {0} ASQN:{1} at class {2} ASQN:{3}", acClassProperty.ACIdentifier, assemblyQualifiedName, acClass.ACIdentifier, acClass.AssemblyQualifiedName, dataType != null ? dataType.ToString() : ""));

                bool isDBColumn = false;
                if (acClass.ACKind == Global.ACKinds.TACDBA)
                {
                    VBSystemColumns vbColumn = null;
                    if (!propertyInfo.PropertyType.IsValueType && !typeof(string).IsAssignableFrom(propertyInfo.PropertyType))
                    {
                        string columnName1 = acClassProperty.ACIdentifier;
                        string columnName2 = columnName1 + "ID";
                        vbColumn = _Database.VBSystemColumns.Where(c => c.tablename == acClass.ACIdentifier && (c.columnname == columnName1 || c.columnname == columnName2)).FirstOrDefault();
                    }
                    else
                        vbColumn = _Database.VBSystemColumns.Where(c => c.tablename == acClass.ACIdentifier && c.columnname == acClassProperty.ACIdentifier).FirstOrDefault();

                    if (vbColumn != null)
                    {
                        isDBColumn = true;
                        if (acClassProperty.IsNullable != vbColumn.columnnullable.Value > 0)
                            acClassProperty.IsNullable = vbColumn.columnnullable.Value > 0;
                        if (vbColumn.columntype == "text" || vbColumn.columntype == "ntext")
                            acClassProperty.DataTypeLength = -1;
                        else
                            acClassProperty.DataTypeLength = vbColumn.columnlength.Value;
                    }
                    else if ((acPropertyInfo.MaxLength >= 0) && acClassProperty.DataTypeLength != acPropertyInfo.MaxLength)
                        acClassProperty.DataTypeLength = acPropertyInfo.MaxLength;
                }

                if (!isDBColumn)
                {
                    if (genericType != null)
                    {
                        if (genericType == typeof(Nullable<>))
                        {
                            if (!acClassProperty.IsNullable)
                                acClassProperty.IsNullable = true;
                        }
                        else
                        {
                            if (acClassProperty.IsNullable != !genericType.IsValueType)
                                acClassProperty.IsNullable = !genericType.IsValueType;
                        }
                    }
                    else if (dataType != null && (acClassProperty.IsNullable != !dataType.IsValueType))
                        acClassProperty.IsNullable = !dataType.IsValueType;

                    if (dataType != null)
                    {
                        if (typeof(String).IsAssignableFrom(dataType))
                        {
                            if (acPropertyInfo.DataTypeLength > 0)
                                acClassProperty.DataTypeLength = acPropertyInfo.DataTypeLength;
                        }
                        else
                        {
                            switch (dataType.ToString())
                            {
                                case TypeAnalyser._TypeName_Boolean:
                                case TypeAnalyser._TypeName_Byte:
                                case TypeAnalyser._TypeName_SByte:
                                    acClassProperty.DataTypeLength = 1;
                                    break;
                                case TypeAnalyser._TypeName_Char:
                                case TypeAnalyser._TypeName_Int16:
                                case TypeAnalyser._TypeName_UInt16:
                                    acClassProperty.DataTypeLength = 2;
                                    break;
                                case TypeAnalyser._TypeName_Single:
                                case TypeAnalyser._TypeName_Int32:
                                case TypeAnalyser._TypeName_UInt32:
                                    acClassProperty.DataTypeLength = 4;
                                    break;
                                case TypeAnalyser._TypeName_Double:
                                case TypeAnalyser._TypeName_Int64:
                                case TypeAnalyser._TypeName_UInt64:
                                    acClassProperty.DataTypeLength = 8;
                                    break;
                                case TypeAnalyser._TypeName_Decimal:
                                    acClassProperty.DataTypeLength = 16;
                                    break;
                                default:
                                    if (acPropertyInfo.DataTypeLength > 0)
                                        acClassProperty.DataTypeLength = acPropertyInfo.DataTypeLength;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateACClassProperty(10)", msg);
            }
        }

        private void ProcessComponentChildItem(Database database, ACClass acClass, ACChildInfo childInfoAttribute)
        {
            if (!acClass.ACClass_ParentACClass.Any(c => c.ACIdentifier == childInfoAttribute.InstanceName))
            {
                ACClass basedOnAcclass = _Database.ACClass.FirstOrDefault(c => c.AssemblyQualifiedName == childInfoAttribute.Type.AssemblyQualifiedName);
                if (basedOnAcclass != null)
                {
                    ACClass childACClass = ACClass.NewACObject(database, acClass);
                    childACClass.ACIdentifier = childInfoAttribute.InstanceName;
                    childACClass.ACProject = acClass.ACProject;
                    childACClass.ACClass1_BasedOnACClass = basedOnAcclass;
                    childACClass.ACCaptionTranslation = basedOnAcclass.ACCaptionTranslation;
                    childACClass.ACKind = basedOnAcclass.ACKind;
                    childACClass.ACPackage = acClass.ACPackage;
                    childACClass.ACStartType = Global.ACStartTypes.Manually;
                    childACClass.IsRightmanagement = basedOnAcclass.IsRightmanagement;

                    if (childACClass.EntityState == EntityState.Detached)
                        _Database.ACClass.Add(childACClass);
                }
            }
        }

        void CheckExitsType(Type type, string columns)
        {
            if (string.IsNullOrEmpty(columns))
                return;
            string[] columnList = columns.Split(',');

            foreach (var column in columnList)
            {
                PropertyInfo pi = TypeAnalyser.GetPropertyPathInfo(type, column);
                if (pi == null || pi.PropertyType == null)
                {
                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateACClassProperty(10)", String.Format("Falsches Property {0} bei Klasse {1} ", column, type.FullName));
                }
            }
        }

        public string AssemblyNameSearchPattern
        {
            get
            {
                return "*.core.*.dll,*.bso.*.dll,*.solution.*.dll,*.variobatch.*.dll,*.mes.*.dll,*.package.*.dll,*.iplus.*.dll,*.iplusmes.*.dll,*Wpf.dll,gip.ext.graphics.dll,*datamodel.dll";
            }
        }

        #endregion

        #region EntitySchemaManager

        public void DetermineDataType(Type propertyInfo, ref string assemblyQualifiedName, ref string genericTypeString, ref Type genericType, ref Type dataType)
        {
            Type T = propertyInfo;

            if (propertyInfo.BaseType != null && propertyInfo.BaseType.IsGenericType && propertyInfo.BaseType.GetGenericTypeDefinition() == typeof(List<>))
                T = propertyInfo.BaseType;

            if (T.IsGenericType)
            {
                // Fälle:
                // 1. Nullable<int>
                // 2. List<int>
                // 3. IACProperty<int>
                // 4. IACProperty<List<int>>

                Type GenericArgument = T.GetGenericArguments()[0]; //1,2,3.: int, 4.: List<int>
                Type GenericDefinition = T.GetGenericTypeDefinition(); // Nullable<>,List<>,IACProperty<>
                bool isACProperty = GenericDefinition.FullName.Contains(Const.ACPropertyPrefix) || GenericDefinition.FullName.Contains(Const.IACContainerTNetPrefix);

                if (!GenericArgument.IsGenericType)
                {
                    dataType = GenericArgument;
                    assemblyQualifiedName = dataType.AssemblyQualifiedName;

                    // Fall 3. IACProperty<int>
                    if (isACProperty)
                    {
                        genericTypeString = "";
                        genericType = null;
                    }
                    // Fall 2. List<int>, 1. Nullable<int>
                    else
                    {
                        genericTypeString = GenericDefinition.FullName;
                        genericType = GenericDefinition;
                    }
                }
                else
                {
                    dataType = GenericArgument.GetGenericArguments()[0];
                    assemblyQualifiedName = dataType.AssemblyQualifiedName;

                    // Fall 3. IACProperty<List<int>>
                    if (isACProperty)
                    {
                        genericTypeString = GenericArgument.GetGenericTypeDefinition().FullName;
                        genericType = GenericArgument;
                    }
                    // Fall 2. List<List<int>>: Fehler!
                    else
                    {
                        genericTypeString = GenericArgument.GetGenericTypeDefinition().FullName;
                        genericType = GenericArgument;
                    }
                }
            }
            else
            {
                dataType = propertyInfo;
                assemblyQualifiedName = dataType.AssemblyQualifiedName;
                genericTypeString = "";
                genericType = null;
            }
        }


        #region Hilfsfunktionen


        public void ExtractClassType(Type classType, ref string typeName, ref string typeFullName, ref string AssemblyQualifiedName)
        {
            typeName = "";
            AssemblyQualifiedName = "";
            if (classType == null)
                return;
            if (classType.IsGenericType)
            {
                Type genericType = classType.GetGenericTypeDefinition();
                AssemblyQualifiedName = genericType.AssemblyQualifiedName;
                typeName = genericType.Name;
                typeFullName = genericType.FullName;
                //int pos = classType.FullName.IndexOf('`');
                ////AssemblyQualifiedName = classType.FullName.Substring(0, pos);
                //AssemblyQualifiedName = classType.AssemblyQualifiedName;
                //pos = classType.Name.IndexOf('`');
                //acClassName = classType.Name.Substring(0, pos);
            }
            else
            {
                AssemblyQualifiedName = classType.AssemblyQualifiedName;
                typeFullName = classType.FullName;
                typeName = classType.Name;
            }
        }

        Type GetTypeInfo(MemberInfo memberInfo)
        {
            PropertyInfo pi1 = (PropertyInfo)memberInfo;
            Type propType1 = pi1.PropertyType;

            // 1:n-Verweise werden nicht übernommen
            //if (propType.Name.Length >= 16 && propType.Name.Substring(0, 16) == "EntityCollection")
            //    continue;

            // Wenn es ein generischer Datentyp ist, dann nur den eigentlichen Typen ermitteln
            // Nur so werden auch Nullable Spalten gefunden

            if (propType1.IsGenericType)
            {
                propType1 = propType1.GetGenericArguments()[0];
            }
            return propType1;
        }
        #endregion
        #endregion

        #region ACClassItemWithChilds
        void InsertOrUpdateACClassItemWithChilds(List<ACClassItem> acClassItemWithChilds)
        {
            foreach (var acClassItem in acClassItemWithChilds)
            {
                ACClass parentACClass = _Database.ACClass.Where(c => c.AssemblyQualifiedName == acClassItem.ParentType.AssemblyQualifiedName).FirstOrDefault();
                if (parentACClass == null)
                    continue;
                foreach (var acClassChild in acClassItem.ACClassInfo.ACClassChilds)
                {
                    InsertOrUpdateACClassItemWithChild(parentACClass, acClassChild);
                }
            }
        }

        void InsertOrUpdateACClassItemWithChild(ACClass parentACClass, ACClassChild acClassChild)
        {
            ACClass baseACClass = _Database.ACClass.Where(c => c.AssemblyQualifiedName == acClassChild.BaseAssemblyQualifiedName).FirstOrDefault();
            if (baseACClass == null)
                return;
            ACClass acClass = parentACClass.ACClass_ParentACClass.Where(c => c.ACIdentifier == acClassChild.ACIdentifier).FirstOrDefault();
            if (acClass == null)
            {
                acClass = ACClass.NewACObjectWithBaseclass(_Database, ACProjectClassLibrary, baseACClass);

                acClass.ACClass1_ParentACClass = parentACClass;
                acClass.ACIdentifier = acClassChild.ACIdentifier;
                acClass.ACCaptionTranslation = acClassChild.ACCaptionTranslation;
                //acClass.ACFilterColumns = acClassInfo.ACFilterColumns;
                //acClass.ACSortColumns = acClassInfo.ACSortColumns;
                acClass.AssemblyQualifiedName = "";
                acClass.IsAssembly = true;
                acClass.ACKind = baseACClass.ACKind;
                acClass.IsAbstract = false;
                acClass.ACStartType = Global.ACStartTypes.Automatic;
                acClass.IsRightmanagement = false;
                if (acClass.EntityState == EntityState.Detached)
                    _Database.ACClass.Add(acClass);
            }
            else
            {
                acClass.ACKind = baseACClass.ACKind;
                acClass.ACClass1_BasedOnACClass = baseACClass;
                acClass.IsRightmanagement = baseACClass.IsRightmanagement;
                if (!acClass.IsAssembly)
                    acClass.IsAssembly = true;

                Translator.UpdateTranslation(acClass, acClassChild.ACCaptionTranslation);
                if (typeof(ACValueItemList) != acClass.ObjectType && typeof(ACValueItemList).IsAssignableFrom(acClass.ObjectType))
                    Translator.UpdateTranslationACValueItemList(acClass);
            }

            if (acClassChild.ACClassChilds != null)
            {
                foreach (var acClassChild1 in acClassChild.ACClassChilds)
                {
                    InsertOrUpdateACClassItemWithChild(acClass, acClassChild1);
                }
            }

            try
            {
                MsgWithDetails saveResult = _Database.ACSaveChanges(true, true, true);
                if (saveResult != null)
                {
                    Messages.GlobalMsg.AddDetailMessage(saveResult);
                    _Database.ACUndoChanges();
                    throw new Exception("InsertOrUpdateACClassItemWithChild(), ACClass(" + acClass.ACIdentifier + ") " + saveResult.InnerMessage);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateACClassItemWithChild", msg);

                throw;
            }
        }
        #endregion

        #region ACQueryInfo

        void InsertOrUpdateACQueryInfos(List<ACQueryItem> acQueryItemList)
        {
            foreach (var acQueryItem in acQueryItemList)
            {
                InsertOrUpdateACClassQuery(acQueryItem.ACQueryInfo, ACClassQryManager, acQueryItem.ParentType);
            }

            try
            {
                MsgWithDetails saveResult = _Database.ACSaveChanges(true, true, true);
                if (saveResult != null)
                {
                    Messages.GlobalMsg.AddDetailMessage(saveResult);
                    _Database.ACUndoChanges();
                    throw new Exception("InsertOrUpdateACQueryInfos()" + saveResult.InnerMessage);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateACQueryInfos", msg);
                throw;
            }

            try
            {
                MsgWithDetails saveResult = _Database.ACSaveChanges(true, true, true);
                if (saveResult != null)
                {
                    Messages.GlobalMsg.AddDetailMessage(saveResult);
                    _Database.ACUndoChanges();
                    throw new Exception("InsertOrUpdateACQueryInfos()" + saveResult.InnerMessage);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateACQueryInfos(10)", msg);
                throw;
            }

        }

        void InsertOrUpdateACClassQuery(ACQueryInfo acQueryInfo, ACClass parentACClass, Type rootObject)
        {
            ACClass acClassQueryDefinition = ACProjectClassLibrary.ACClass_ACProject.Where(c => c.ACIdentifier == ACQueryDefinition.ClassName).First();
            ACClass acClass = parentACClass.ACClass_ParentACClass.Where(c => c.ACIdentifier == acQueryInfo.ACIdentifier).FirstOrDefault();
            if (acClass == null)
            {
                acClass = ACClass.NewACObject(_Database, RootProject);

                acClass.ACIdentifier = acQueryInfo.ACIdentifier;
                acClass.ACCaptionTranslation = acQueryInfo.ACCaptionTranslation;
                acClass.ACClass1_BasedOnACClass = acClassQueryDefinition;
                acClass.ACClass1_ParentACClass = parentACClass;
                acClass.ACFilterColumns = acQueryInfo.ACFilterColumns;
                acClass.ACSortColumns = acQueryInfo.ACSortColumns;
                acClass.AssemblyQualifiedName = "";
                acClass.ACKind = Global.ACKinds.TACQRY;
                acClass.IsAbstract = false;
                acClass.IsAssembly = true;
                acClass.ACStartType = GetACStartTypeByACType(Global.ACKinds.TACQRY);
                acClass.IsRightmanagement = false;
                if (acClass.EntityState == EntityState.Detached)
                    _Database.ACClass.Add(acClass);
            }
            else
            {
                acClass.ACKind = Global.ACKinds.TACQRY;
                Translator.UpdateTranslation(acClass, acQueryInfo.ACCaptionTranslation);
                if (!acClass.IsAssembly)
                    acClass.IsAssembly = true;

            }
            var acClassPropertyList = acClass.Properties;

            var acPropertyChildACUrl = acClassPropertyList.Where(c => c.ACIdentifier == Const.ChildACUrlPrefix).First() as ACClassProperty;
            if (acPropertyChildACUrl.ACClass != acClass)
            {
                acPropertyChildACUrl = ACClassProperty.NewACClassProperty(_Database, acClass, acPropertyChildACUrl);
                _Database.ACClassProperty.Add(acPropertyChildACUrl);
            }
            if (acPropertyChildACUrl.XMLValue != acQueryInfo.ChildACUrl)
                acPropertyChildACUrl.XMLValue = acQueryInfo.ChildACUrl;

            var acPropertyQueryType = acClassPropertyList.Where(c => c.ACIdentifier == Const.ACQueryTypePrefix).First() as ACClassProperty;
            if (acPropertyQueryType.ACClass != acClass)
            {
                acPropertyQueryType = ACClassProperty.NewACClassProperty(_Database, acClass, acPropertyQueryType);
                _Database.ACClassProperty.Add(acPropertyQueryType);
            }
            string acUrl = _Database.GetACType(acQueryInfo.QueryType).GetACUrl();
            if (acPropertyQueryType.XMLValue != acUrl)
                acPropertyQueryType.XMLValue = acUrl;

            var acPropertyRootObject = acClassPropertyList.Where(c => c.ACIdentifier == Const.ACQueryRootObjectPrefix).First() as ACClassProperty;
            if (acPropertyRootObject.ACClass != acClass)
            {
                acPropertyRootObject = ACClassProperty.NewACClassProperty(_Database, acClass, acPropertyRootObject);
                _Database.ACClassProperty.Add(acPropertyRootObject);
            }
            if (acPropertyRootObject.XMLValue != rootObject.FullName)
                acPropertyRootObject.XMLValue = rootObject.FullName;
            if (acClass.ACFilterColumns != acQueryInfo.ACFilterColumns)
                acClass.ACFilterColumns = acQueryInfo.ACFilterColumns;
            if (acClass.ACSortColumns != acQueryInfo.ACSortColumns)
                acClass.ACSortColumns = acQueryInfo.ACSortColumns;

            ACPackage acPackage = ACPackage.GetACPackage(_Database, acQueryInfo.ACPackageName);
            if (acClass.ACPackage != acPackage)
                acClass.ACPackage = acPackage;

            if (acQueryInfo.ACQueryChilds != null)
            {
                foreach (var acQueryInfoChild in acQueryInfo.ACQueryChilds)
                {
                    InsertOrUpdateACClassQuery(acQueryInfoChild, acClass, acQueryInfo.QueryType);
                }
            }

            if (parentACClass == ACClassQryManager)
            {
                ACClass acClass1 = _Database.ACClass.Where(c => c.AssemblyQualifiedName == rootObject.AssemblyQualifiedName).First();
                InsertOrUpdateACClassComposition(acClass1, acClass, Const.KeyACUrl_NavigationqueryList, Const.LocalConfigACUrl_NavigationqueryList, null, true);
            }

            try
            {
                MsgWithDetails saveResult = _Database.ACSaveChanges(true, true, true);
                if (saveResult != null)
                {
                    Messages.GlobalMsg.AddDetailMessage(saveResult);
                    _Database.ACUndoChanges();
                    throw new Exception("InsertOrUpdateACClassQuery(), ACClass(" + acClass.ACIdentifier + ") " + saveResult.InnerMessage);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateACClassQuery", msg);
                throw new Exception(e.Message, e);
            }
        }
        #endregion

        #region ACClassComposition

        ACClass _ACClassQryManager;
        public ACClass ACClassQryManager
        {
            get
            {
                if (_ACClassQryManager == null)
                {
                    _ACClassQryManager = _Database.ACClass.Where(c => c.ACIdentifier == Queries.ClassName).First();
                }
                return _ACClassQryManager;
            }
        }

        ACClass _ACClassBusinessobjects;
        public ACClass ACClassBusinessobjects
        {
            get
            {
                if (_ACClassBusinessobjects == null)
                {
                    _ACClassBusinessobjects = _Database.ACClass.Where(c => c.ACIdentifier == Businessobjects.ClassName).First();
                }
                return _ACClassBusinessobjects;
            }
        }

        void InsertOrUpdateACClassCompositions(List<ACClassItem> acClassItemList)
        {
            foreach (var acClassItem in acClassItemList)
            {
                ACClass acClass = _Database.ACClass.Where(c => c.AssemblyQualifiedName == acClassItem.ParentType.AssemblyQualifiedName).FirstOrDefault();
                if (acClass == null)
                    continue;
                if (!string.IsNullOrEmpty(acClassItem.ACClassInfo.QRYConfig))
                {
                    if (acClass.ACKind == Global.ACKinds.TACEnum/* || acClass.ACKind == Global.ACKinds.TACEnumACValueList*/)
                    {
                        ACClass relatedClass = _Database.ACClass.Where(c => c.AssemblyQualifiedName.Contains(acClassItem.ACClassInfo.QRYConfig)).FirstOrDefault();
                        InsertOrUpdateACClassComposition(acClass, relatedClass, Const.KeyACUrl_EnumACValueList, Const.LocalConfigACUrl_EnumACValueList, null, false);
                    }
                    else
                    {
                        // QRY-Klasse ermitteln
                        ACClass qryACClass = ACClassQryManager.ACClass_ParentACClass.Where(c => c.ACIdentifier == acClassItem.ACClassInfo.QRYConfig).FirstOrDefault();
                        if (qryACClass != null)
                            InsertOrUpdateACClassComposition(acClass, qryACClass, Const.KeyACUrl_NavigationqueryList, Const.LocalConfigACUrl_NavigationqueryList, null, true);
                    }
                }
                if (!string.IsNullOrEmpty(acClassItem.ACClassInfo.BSOConfig))
                {
                    string[] bsoConfigList = acClassItem.ACClassInfo.BSOConfig.Split(',');
                    foreach (var bsoConfig in bsoConfigList)
                    {
                        string config;
                        string expression = null;
                        if (bsoConfig.IndexOf("/") == -1)
                        {
                            config = bsoConfig;
                        }
                        else
                        {
                            string[] configInfo = bsoConfig.Split('/');
                            config = configInfo[0];
                            expression = configInfo[1];
                        }
                        // BSO-Klasse ermitteln
                        ACClass bsoACClass = ACClassBusinessobjects.ACClass_ParentACClass.Where(c => c.ACIdentifier == config).FirstOrDefault();
                        if (bsoACClass != null)
                        {
                            InsertOrUpdateACClassComposition(acClass, bsoACClass, Const.KeyACUrl_BusinessobjectList, Const.LocalConfigACUrl_BusinessobjectList, null, false, expression);
                        }
                    }
                }
            }

            try
            {
                MsgWithDetails saveResult = _Database.ACSaveChanges(true, true, true);
                if (saveResult != null)
                {
                    Messages.GlobalMsg.AddDetailMessage(saveResult);
                    _Database.ACUndoChanges();
                    throw new Exception("InsertOrUpdateACClassCompositions() " + saveResult.InnerMessage);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateACClassCompositions", msg);
                throw;
            }
        }

        void InsertOrUpdateACClassComposition(ACClass acClass, ACClass compositionACClass, string keyACUrl, string valueACUrl, string appendix = null, bool isPrimary = false, string expression = "")
        {
            IACConfig acConfig = acClass.ACClassConfig_ACClass.Where(c => c.KeyACUrl == keyACUrl && c.LocalConfigACUrl == valueACUrl).FirstOrDefault();
            if (acConfig == null)
            {
                acConfig = acClass.NewACConfig(null, acClass.GetObjectContext<Database>().GetACType(typeof(ACComposition)));
                acConfig.KeyACUrl = keyACUrl;
                acConfig.LocalConfigACUrl = valueACUrl;
                acConfig.Expression = expression;

                ACComposition acComposition = new ACComposition();
                acComposition.SetComposition(compositionACClass);
                acComposition.Appendix = appendix;
                acComposition.IsSystem = true;
                acComposition.IsPrimary = isPrimary;


                acConfig[Const.Value] = acComposition;
            }
            else
            {
                ACComposition acComposition = acConfig[Const.Value] as ACComposition;
                if (acComposition != null
                    && acComposition.IsSystem
                    && (acComposition.ACUrlComposition != compositionACClass.GetACUrl()
                        || acComposition.Appendix != appendix
                        || acComposition.IsPrimary != isPrimary))
                {
                    acComposition.SetComposition(compositionACClass);
                    acComposition.Appendix = appendix;
                    acComposition.IsSystem = true;
                    acComposition.IsPrimary = isPrimary;

                    acConfig[Const.Value] = acComposition;
                }
            }
            if (acConfig.Expression != expression)
                acConfig.Expression = expression;
        }
        #endregion

        #region IACClassManager
        private DbContext FindOrCreateObjectContext(Type forEntityType)
        {
            IACEntityObjectContext context = _ObjectContexts.Where(c => c.GetType().Assembly == forEntityType.Assembly).FirstOrDefault();
            if (context != null)
                return context as DbContext;
            Type typeContext = forEntityType.Assembly.GetTypes().Where(c => typeof(IACEntityObjectContext).IsAssignableFrom(c)).FirstOrDefault();
            if (typeContext == null)
                return null;
            context = Activator.CreateInstance(typeContext) as IACEntityObjectContext;
            _ObjectContexts.Add(context);
            return context as DbContext;
        }
        #endregion

        #region DummyEntries
        void InsertOrUpdateDummyEntries()
        {
            // ACClass
            ACClass dummyACClass = ACProjectClassLibrary.ACClass_ACProject.Where(c => c.ACIdentifier == Const.UnknownClass && c.ACKindIndex == (Int16)Global.ACKinds.TACUndefined).FirstOrDefault();
            if (dummyACClass == null)
            {
                dummyACClass = ACClass.NewACObject(_Database, ACProjectClassLibrary);
                dummyACClass.ACPackage = _Database.ACPackage.Where(c => c.ACPackageName == Const.PackName_VarioSystem).First();
                dummyACClass.ACIdentifier = Const.UnknownClass;
                dummyACClass.ACCaptionTranslation = "en{'Unknown Class'}de{'Unbekannte Klasse'}";
                dummyACClass.ACKind = Global.ACKinds.TACUndefined;
                dummyACClass.IsAssembly = true;
                if (dummyACClass.EntityState == EntityState.Detached)
                    _Database.ACClass.Add(dummyACClass);
            }
            else
            {
                if (!dummyACClass.IsAssembly)
                    dummyACClass.IsAssembly = true;
            }

            // ACClassProperty
            ACClassProperty dummyACClassProperty = dummyACClass.ACClassProperty_ACClass.Where(c => c.ACKindIndex == (Int16)Global.ACKinds.TACUndefined).FirstOrDefault();
            if (dummyACClassProperty == null)
            {
                dummyACClassProperty = ACClassProperty.NewACObject(_Database, dummyACClass);
                dummyACClassProperty.ConfigACClass = null;
                dummyACClassProperty.ACIdentifier = Const.UnknownProperty;
                dummyACClassProperty.ACCaptionTranslation = "en{'Unknown Property'}de{'Unbekannte Eigenschaft'}";
                dummyACClassProperty.ACKind = Global.ACKinds.TACUndefined;
                dummyACClassProperty.ValueTypeACClass = GetCheckedAssemblyACClass(TypeAnalyser._TypeName_Boolean);
                if (dummyACClassProperty.ValueTypeACClass == null)
                    dummyACClassProperty.ValueTypeACClass = _Database.ACClass.Where(c => c.AssemblyQualifiedName.StartsWith(TypeAnalyser._TypeName_Boolean)).FirstOrDefault();
                if (dummyACClassProperty.EntityState == EntityState.Detached)
                    _Database.ACClassProperty.Add(dummyACClassProperty);
            }

            // ACClassMethod
            ACClassMethod dummyACClassMethod = dummyACClass.ACClassMethod_ACClass.Where(c => c.ACKindIndex == (Int16)Global.ACKinds.TACUndefined).FirstOrDefault();
            if (dummyACClassMethod == null)
            {
                dummyACClassMethod = ACClassMethod.NewACObject(_Database, dummyACClass);
                dummyACClassMethod.ACIdentifier = Const.UnknownMethod;
                dummyACClassMethod.ACCaptionTranslation = "en{'Unknown Method'}de{'Unbekannte Eigenschaft'}";
                dummyACClassMethod.ACKind = Global.ACKinds.TACUndefined;
                if (dummyACClassMethod.EntityState == EntityState.Detached)
                    _Database.ACClassMethod.Add(dummyACClassMethod);
            }

            // ACClassWF
            ACClassWF dummyACClassWF = dummyACClassMethod.ACClassWF_ACClassMethod.FirstOrDefault();
            if (dummyACClassWF == null)
            {
                string secondaryKey = ACRoot.SRoot.NoManager.GetNewNo(_Database, typeof(ACClassWF), ACClassWF.NoColumnName, ACClassWF.FormatNewNo, null);
                dummyACClassWF = ACClassWF.NewACObject(_Database, dummyACClassMethod, secondaryKey);
                dummyACClassWF.ACIdentifier = Const.UnknownWorkflow;
                dummyACClassWF.PWACClass = dummyACClass;
                if (dummyACClassWF.EntityState == EntityState.Detached)
                    _Database.ACClassWF.Add(dummyACClassWF);
            }

            // ACClassDesign
            ACClassDesign dummyACClassDesign = dummyACClass.ACClassDesign_ACClass.Where(c => c.ACKindIndex == (Int16)Global.ACKinds.TACUndefined).FirstOrDefault();
            if (dummyACClassDesign == null)
            {
                string secondaryKey = ACRoot.SRoot.NoManager.GetNewNo(_Database, typeof(ACClassDesign), ACClassDesign.NoColumnName, ACClassDesign.FormatNewNo, null);
                dummyACClassDesign = ACClassDesign.NewACObject(_Database, dummyACClass, secondaryKey);
                dummyACClassDesign.ACIdentifier = Const.UnknownDesign;
                dummyACClassDesign.ACCaptionTranslation = "en{'Unknown Design'}de{'Unbekannte Eigenschaft'}";
                dummyACClassDesign.ACKind = Global.ACKinds.TACUndefined;
                if (dummyACClassDesign.EntityState == EntityState.Detached)
                    _Database.ACClassDesign.Add(dummyACClassDesign);
            }

            try
            {
                MsgWithDetails saveResult = _Database.ACSaveChanges(true, true, true);
                if (saveResult != null)
                {
                    Messages.GlobalMsg.AddDetailMessage(saveResult);
                    _Database.ACUndoChanges();
                    throw new Exception("InsertOrUpdateDummyEntries() " + saveResult.InnerMessage);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClassManager", "InsertOrUpdateDummyEntries", msg);
                throw;
            }
        }
        #endregion

        ACValue GetACParameterDefinition(object[] parameter)
        {
            ACValue acParameterDefinition = null;
            int paramCount = parameter.Count();
            if (paramCount <= 2)
            {
                throw new ArgumentException("Less than 2 parameter in ParameterDefinition");
            }

            if (parameter[0] as String == "InwardQuantity" || parameter[0] as String == "QuantityIsAbsolute")
            {
                //int i = 10;
            }

            if (parameter[1] is Global.ParamOption)
            {
                if (parameter[2] is Type)
                {
                    acParameterDefinition = new
                        ACValue
                        (
                            parameter[0] as string,
                            parameter[2] as Type,
                            null,
                            (Global.ParamOption)parameter[1]
                        );
                }
                else
                {
                    if (paramCount <= 3)
                    {
                        acParameterDefinition = new
                            ACValue
                            (
                                parameter[0] as string,
                                parameter[2] as string,
                                "",
                                null,
                                (Global.ParamOption)parameter[1]
                            );
                    }
                    else
                    {
                        acParameterDefinition = new
                            ACValue
                            (
                                parameter[0] as string,
                                parameter[2] as string,
                                parameter[3] as string,
                                null,
                                (Global.ParamOption)parameter[1]
                            );
                    }
                }
            }
            else
            {
                if (parameter[2] is Type)
                {
                    acParameterDefinition = new
                        ACValue
                        (
                            parameter[0] as string,
                            parameter[2] as Type,
                            null,
                            (Global.ParamOption)Enum.Parse(typeof(Global.ParamOption), parameter[1] as string)
                        );
                }
                else
                {
                    if (paramCount <= 3)
                    {
                        acParameterDefinition = new
                            ACValue
                            (
                                parameter[0] as string,
                                parameter[2] as string,
                                "",
                                null,
                                (Global.ParamOption)Enum.Parse(typeof(Global.ParamOption), parameter[1] as string)
                            );
                    }
                    else
                    {
                        acParameterDefinition = new
                            ACValue
                            (
                                parameter[0] as string,
                                parameter[2] as string,
                                parameter[3] as string,
                                null,
                                (Global.ParamOption)Enum.Parse(typeof(Global.ParamOption), parameter[1] as string)
                            );
                    }
                }
            }

            if (paramCount > 4)
                acParameterDefinition.SetValueFromString(parameter[4] as string);
            //else
            //acParameterDefinition.SetDefaultValue();

            return acParameterDefinition;
        }

        #region Messages

        void controlSync_OnMessage(SyncMessage msg)
        {
            string source = "ControlSync";
            if (!string.IsNullOrEmpty(msg.Source))
                source = msg.Source;
            if (msg.MessageLevel == MessageLevel.Error)
                Database.Root.Messages.LogError(source, "Sync", msg.Message);
            else if (msg.MessageLevel == MessageLevel.Warning)
                Database.Root.Messages.LogWarning(source, "Sync", msg.Message);

            Messages.ConsoleMsg("ControlSync", msg.Message);
        }


        #endregion
    }

}
