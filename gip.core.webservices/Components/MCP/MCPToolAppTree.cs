// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using Azure.Core;
using gip.core.autocomponent;
using gip.core.datamodel;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static gip.core.webservices.MCPIPlusTools;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace gip.core.webservices
{
    public class MCPToolAppTree : MCPToolBase
    {
        #region Properties
        private Dictionary<Guid, gip.core.datamodel.ACClass> _ThesaurusByCId;
        public Dictionary<Guid, gip.core.datamodel.ACClass> ThesaurusByCId
        {
            get
            {
                using (ACMonitor.Lock(_80000_Lock))
                {
                    if (_ThesaurusByCId != null)
                        return _ThesaurusByCId;
                }
                InitializeThesaurus();
                using (ACMonitor.Lock(_80000_Lock))
                {
                    return _ThesaurusByCId;
                }
            }
        }

        private Dictionary<string, gip.core.datamodel.ACClass>[] _ThesaurusByACId = null;
        /// <summary>
        /// Index/Categories:
        /// 0 = Types for static components that are instantiated during startup and added to the application tree, thus existing statically throughout runtime.
        /// 1 = Types for dynamic components that are created during runtime and automatically added to or removed from the application tree when they are no longer needed. These are usually workflow components.
        /// 2 = Types for dynamic components (so-called business objects or apps) that are instantiated at the request of a user and used exclusively by that user to operate apps and primarily work with database data.
        /// 3 = Types of database objects (Entity Framework) or tables.
        /// </summary>
        public Dictionary<string, gip.core.datamodel.ACClass>[] ThesaurusByACId
        {
            get
            {
                using (ACMonitor.Lock(_80000_Lock))
                {
                    if (_ThesaurusByACId != null)
                        return _ThesaurusByACId;
                }
                InitializeThesaurus();
                using (ACMonitor.Lock(_80000_Lock))
                {
                    return _ThesaurusByACId;
                }
            }
        }

        protected override Dictionary<string, gip.core.datamodel.ACClass> EntityTypes 
        { 
            get
            {
                return ThesaurusByACId[3];
            }
        }

        #endregion

        #region Methods

        #region Thesaurus
        public string AppGetThesaurus(IACComponent requester, string i18nLangTag, int category = 0)
        {
            try
            {
                MCP_BaseType[] thesaurus = ThesaurusByACId[category].Select(kvp => new MCP_BaseType
                {
                    ACIdentifier = kvp.Key,
                    Description = Translator.GetTranslation(null, kvp.Value.ACCaption, string.IsNullOrEmpty(i18nLangTag) ? "en" : i18nLangTag.ToLower())
                }).ToArray();
                return JsonSerializer.Serialize(thesaurus, new JsonSerializerOptions { WriteIndented = false });
            }
            catch (Exception ex)
            {
                requester.Messages.LogException(requester.GetACUrl(), nameof(AppGetThesaurus), ex, true);
                return CreateExceptionResponse(ex);
            }
        }

        protected virtual void InitializeThesaurus()
        {
            // TODO: Thread-Safety
            Dictionary<Guid, gip.core.datamodel.ACClass> thesaurusByCId = new Dictionary<Guid, gip.core.datamodel.ACClass>();
            Dictionary<string, gip.core.datamodel.ACClass>[] thesaurusByACId = new Dictionary<string, gip.core.datamodel.ACClass>[4];
            for (int i = 0; i < thesaurusByACId.Length; i++)
            {
                thesaurusByACId[i] = new Dictionary<string, gip.core.datamodel.ACClass>();
            }

            // Populate thesaurus for static components (category 0)
            foreach (var rootApp in ACRoot.SRoot.ACComponentChilds)
            {
                if (rootApp is IAppManager manager)
                {
                    bool skipChilds = (rootApp == ACRoot.SRoot.Businessobjects
                         || rootApp == ACRoot.SRoot.Messages
                         //|| rootApp == ACRoot.SRoot.LocalServiceObjects
                         || rootApp == ACRoot.SRoot.WPFServices
                         || rootApp == ACRoot.SRoot.Environment
                         || rootApp == ACRoot.SRoot.Queries
                         || rootApp == ACRoot.SRoot.Communications
                         );
                    PopulateThesaurus(manager, ref thesaurusByCId, ref thesaurusByACId[0], skipChilds);
                }
            }

            // Populate thesaurus for dynamic components (category 1)
            Database db = ACRoot.SRoot.Database as Database;
            IEnumerable<gip.core.datamodel.ACClass> dynamicClasses;
            using (ACMonitor.Lock(db.QueryLock_1X000))
            //using (Database db = new Database())
            {
                dynamicClasses = db.ACClass.Where(c => c.ACKindIndex >= (short)Global.ACKinds.TPWMethod && c.ACKindIndex <= (short)Global.ACKinds.TPWNodeEnd).AsEnumerable();
            }
            foreach (gip.core.datamodel.ACClass cls in dynamicClasses)
            {
                PopulateThesaurus(cls, ref thesaurusByCId, ref thesaurusByACId[1]);
            }

            // Populate thesaurus for business objects (category 2)
            dynamicClasses = null;
            using (ACMonitor.Lock(db.QueryLock_1X000))
            //using (Database db = new Database())
            {
                dynamicClasses = db.ACClass.Where(c => c.ACKindIndex >= (short)Global.ACKinds.TACBSO && c.ACKindIndex <= (short)Global.ACKinds.TACBSOReport).AsEnumerable();
            }
            if (dynamicClasses != null)
            {
                foreach (gip.core.datamodel.ACClass cls in dynamicClasses)
                {
                    PopulateThesaurus(cls, ref thesaurusByCId, ref thesaurusByACId[2]);
                }
            }

            // Populate thesaurus for tables (category 3)
            dynamicClasses = null;
            using (ACMonitor.Lock(db.QueryLock_1X000))
            //using (Database db = new Database())
            {
                dynamicClasses = db.ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.TACDBA).AsEnumerable();
            }
            if (dynamicClasses != null)
            {
                foreach (gip.core.datamodel.ACClass cls in dynamicClasses)
                {
                    PopulateThesaurus(cls, ref thesaurusByCId, ref thesaurusByACId[3]);
                }
            }

            using (ACMonitor.Lock(_80000_Lock))
            {
                _ThesaurusByCId = thesaurusByCId;
                _ThesaurusByACId = thesaurusByACId;
            }
        }

        protected virtual void PopulateThesaurus(IACComponent aCComponent, ref Dictionary<Guid, gip.core.datamodel.ACClass> thesaurusByCId, ref Dictionary<string, gip.core.datamodel.ACClass> thesaurusByACId, bool skipChilds = false)
        {
            gip.core.datamodel.ACClass cls = GetBaseClassesForThesaurus(aCComponent.ComponentClass);
            PopulateThesaurus(cls, ref thesaurusByCId, ref thesaurusByACId);
            if (skipChilds)
                return;

            foreach (IACComponent child in aCComponent.ACComponentChilds)
            {
                PopulateThesaurus(child, ref thesaurusByCId, ref thesaurusByACId);
            }
        }

        protected virtual void PopulateThesaurus(gip.core.datamodel.ACClass cls, ref Dictionary<Guid, gip.core.datamodel.ACClass> thesaurusByCId, ref Dictionary<string, gip.core.datamodel.ACClass> thesaurusByACId)
        {
            while (cls != null)
            {
                if (!thesaurusByCId.ContainsKey(cls.ACClassID))
                    thesaurusByCId.Add(cls.ACClassID, cls);
                if (!thesaurusByACId.ContainsKey(cls.ACIdentifier))
                    thesaurusByACId.Add(cls.ACIdentifier, cls);
                cls = cls.BaseClass; // Traverse up to the base class in the class library
            }
        }

        protected virtual gip.core.datamodel.ACClass GetBaseClassesForThesaurus(gip.core.datamodel.ACClass acClass)
        {
            gip.core.datamodel.ACClass cls = acClass;
            do
            {
                if (cls == null)
                    break;
                if (cls.ACProject?.ACProjectType == Global.ACProjectTypes.ClassLibrary
                    || (cls.ACProject?.ACProjectType == Global.ACProjectTypes.Root && (cls.ACKind == Global.ACKinds.TACBSO || cls.ACKind == Global.ACKinds.TPARole)))
                {
                    return cls;
                }
                cls = cls.BaseClass;
            }
            while (cls != null);

            return null;
        }
        #endregion


        #region TypeInfo
        public string AppGetTypeInfos(IACComponent requester, string acIdentifiers, string i18nLangTag, bool getDerivedTypes = false, bool getBaseTypes = false)
        {
            try
            {
                // TODO: mehrere Classids in Instance suche
                List<MCP_TypeInfo> typeInfos = new List<MCP_TypeInfo>();
                string[] arrIdentifiers = SplitParamsToArray(acIdentifiers);
                if (arrIdentifiers != null && arrIdentifiers.Any())
                {
                    foreach (string acId in arrIdentifiers)
                    {
                        gip.core.datamodel.ACClass cls = null;
                        if (Guid.TryParse(acId, out Guid classGuid))
                        {
                            if (!ThesaurusByCId.TryGetValue(classGuid, out cls))
                                cls = null;
                        }
                        else
                        {
                            foreach (var thesaurus in ThesaurusByACId)
                            {
                                if (thesaurus.TryGetValue(acId, out cls))
                                    break;
                            }
                        }
                        if (cls == null)
                            cls = ACRoot.SRoot.Database.ContextIPlus.GetACType(classGuid);

                        if (cls != null)
                        {
                            typeInfos.Add(new MCP_TypeInfo
                            {
                                ACIdentifier = cls.ACIdentifier,
                                Description = Translator.GetTranslation(null, cls.ACCaption, string.IsNullOrEmpty(i18nLangTag) ? "en" : i18nLangTag.ToLower()),
                                ClassID = cls.ACClassID.ToString(),
                                BaseClassID = cls.BaseClass?.ACClassID.ToString() ?? Guid.Empty.ToString(),
                                IsTypeOfAInstance = cls.ACProject?.ACProjectType == Global.ACProjectTypes.Application
                                                  || cls.ACProject?.ACProjectType == Global.ACProjectTypes.Service,
                                IsTypeOfADbTable = cls.ACKind == Global.ACKinds.TACDBA,
                                IsWorkflowType = cls.IsWorkflowType,
                                IsMultiInstanceType = cls.IsMultiInstance || cls.IsWorkflowType,
                                IsCodeOnGithub = !string.IsNullOrEmpty(cls.AssemblyQualifiedName),
                                ManualMCP = cls.Comment
                            });

                            if (getDerivedTypes || getBaseTypes)
                            {
                                foreach (gip.core.datamodel.ACClass tClass in ThesaurusByCId.Values)
                                {
                                    if ((getDerivedTypes && tClass.IsDerivedClassFrom(cls))
                                        || (getBaseTypes && cls.IsDerivedClassFrom(tClass)))
                                    {
                                        string sClId = tClass.ACClassID.ToString();
                                        if (typeInfos.Any(ti => ti.ClassID == sClId))
                                            continue; // Skip if already added
                                        typeInfos.Add(new MCP_TypeInfo
                                        {
                                            ACIdentifier = cls.ACIdentifier,
                                            Description = Translator.GetTranslation(null, tClass.ACCaption, string.IsNullOrEmpty(i18nLangTag) ? "en" : i18nLangTag.ToLower()),
                                            ClassID = tClass.ACClassID.ToString(),
                                            BaseClassID = tClass.BaseClass?.ACClassID.ToString() ?? Guid.Empty.ToString(),
                                            IsTypeOfAInstance = tClass.ACProject?.ACProjectType == Global.ACProjectTypes.Application
                                                                || tClass.ACProject?.ACProjectType == Global.ACProjectTypes.Service,
                                            IsTypeOfADbTable = tClass.ACKind == Global.ACKinds.TACDBA,
                                            IsWorkflowType = tClass.IsWorkflowType,
                                            IsMultiInstanceType = tClass.IsMultiInstance || tClass.IsWorkflowType,
                                            IsCodeOnGithub = !string.IsNullOrEmpty(cls.AssemblyQualifiedName),
                                            ManualMCP = tClass.Comment
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                return JsonSerializer.Serialize(typeInfos, new JsonSerializerOptions { WriteIndented = false });
            }
            catch (Exception ex)
            {
                requester.Messages.LogException(requester.GetACUrl(), nameof(AppGetTypeInfos), ex, true);
                return CreateExceptionResponse(ex);
            }
        }
        #endregion


        #region InstanceInfo
        public virtual string AppGetInstanceInfo(IACComponent requester, string classIDs, string searchConditions, bool isCompositeSearch)
        {
            try
            { 
                MCP_InstanceInfo instanceInfo = null;
                string[] arrClassIDs = SplitParamsToArray(classIDs);
                string[] arrSearch = SplitParamsToArray(searchConditions);

                bool sendRecommendation = false;
                StringBuilder sb = new StringBuilder();
                if (arrClassIDs != null && arrClassIDs.Any())
                {
                    List<SearchCondition> classSearchConditions = new List<SearchCondition>();
                    foreach (string classId in arrClassIDs)
                    {
                        if (!Guid.TryParse(classId, out Guid classGuid))
                        {
                            gip.core.datamodel.ACClass thesaurausCls = null;
                            foreach (var thesaurus in ThesaurusByACId)
                            {
                                if (thesaurus.TryGetValue(classId, out thesaurausCls))
                                    break;
                            }

                            if (thesaurausCls != null)
                            {
                                classGuid = thesaurausCls.ACClassID;
                                sb.AppendLine(string.Format("You didn't provide a valid ClassID (CId field) but you passed a valid ACIdentifer '{0}' instead. It was resolved to {1}. Next time, please call the {2} method first.", classId, classGuid, nameof(AppGetTypeInfos)));
                                sendRecommendation = true;
                            }
                            else
                            {
                                sb.AppendLine($"You didn't provide a valid ClassID (CId field) with '{classId}'");
                                sendRecommendation = true;
                            }
                        }
                        if (classGuid != Guid.Empty && ThesaurusByCId.TryGetValue(classGuid, out gip.core.datamodel.ACClass cls))
                        {
                            string searchCondition = null;
                            if (arrSearch != null && arrSearch.Length > 0)
                            {
                                int index = Array.IndexOf(arrClassIDs, classId);
                                if (index >= 0 && index < arrSearch.Length)
                                {
                                    searchCondition = arrSearch[index];
                                }
                            }
                            classSearchConditions.Add(new SearchCondition() { Class = cls, SearchTerm = searchCondition });
                        }
                    }
                    if (classSearchConditions.Any())
                    {
                        List<Tuple<MatchingCondition, IACComponent>> matchedComponents = new List<Tuple<MatchingCondition, IACComponent>>();
                        foreach (var rootApp in ACRoot.SRoot.ACComponentChilds)
                        {
                            //if (rootApp is IAppManager manager)
                            //{
                                // TODO: Optimize, Ignore unecessary applications
                                SearchMatchingComponents(rootApp, ref matchedComponents, classSearchConditions, isCompositeSearch);
                            //}
                        }
                        instanceInfo = BuildInstanceInfo(ACRoot.SRoot, matchedComponents);
                    }
                }

                if (instanceInfo == null)
                {
                    sb.AppendLine("No instances found for the given criteria.");
                    instanceInfo = new MCP_InstanceInfo
                    {
                        ACIdentifier = ACRoot.SRoot.ACIdentifier,
                        Description = sb.ToString(),
                        ClassID = ACRoot.SRoot.ComponentClass.ACClassID.ToString(),
                        BaseClassID = Guid.Empty.ToString()
                    };
                }
                else if (sendRecommendation)
                {
                    instanceInfo.Description = sb.ToString();
                }
                return JsonSerializer.Serialize(instanceInfo, new JsonSerializerOptions { WriteIndented = false });
            }
            catch (Exception ex)
            {
                requester.Messages.LogException(requester.GetACUrl(), nameof(AppGetInstanceInfo), ex, true);
                return CreateExceptionResponse(ex);
            }
        }


        public class SearchCondition
        {
            public gip.core.datamodel.ACClass Class { get; set; }
            public string SearchTerm { get; set; }
        }

        public class MatchingCondition : SearchCondition
        {
            public bool IsMatched { get; set; } = false;
        }

        public class MatchingConditions : List<MatchingCondition>
        {
            public MatchingConditions(List<SearchCondition> conditions)
            {
                foreach (var condition in conditions)
                {
                    this.Add(new MatchingCondition
                    {
                        Class = condition.Class,
                        SearchTerm = condition.SearchTerm
                    });
                }
            }

            public bool IsFullyMatched
            {
                get
                {
                    return this.All(c => c.IsMatched);
                }
            }

            public bool IsAnyMatched
            {
                get
                {
                    return this.Any(c => c.IsMatched);
                }
            }
        }

        protected void SearchMatchingComponents(IACComponent component, ref List<Tuple<MatchingCondition, IACComponent>> matchedComponents, List<SearchCondition> classSearchConditions, bool isCompositeSearch)
        {
            MatchingConditions matchingConditions = new MatchingConditions(classSearchConditions);
            IACComponent nextComp = component;
            MatchingCondition mcNextComp = null;
            IACComponent firstMatch = null;
            MatchingCondition mcFirstMatch = null;
            while (nextComp != null && nextComp != ACRoot.SRoot)
            {
                // Check if the component matches any of the search conditions
                foreach (MatchingCondition matchingCondition in matchingConditions)
                {
                    if (matchingCondition.IsMatched)
                        continue; // Skip if this condition is already matched
                    if (nextComp.ComponentClass.IsDerivedClassFrom(matchingCondition.Class)
                        && (string.IsNullOrEmpty(matchingCondition.SearchTerm)
                           || nextComp.ComponentClass.ACIdentifier.Contains(matchingCondition.SearchTerm, StringComparison.InvariantCultureIgnoreCase)
                           || nextComp.ComponentClass.ACCaption.Contains(matchingCondition.SearchTerm, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        mcNextComp = matchingCondition;
                        if (firstMatch == null)
                        {
                            firstMatch = nextComp;
                            mcFirstMatch = matchingCondition;
                        }
                        matchingCondition.IsMatched = true; // Mark this condition as matched
                        if (!isCompositeSearch)
                            matchedComponents.Add(new Tuple<MatchingCondition, IACComponent>(matchingCondition, nextComp));
                        break;
                    }
                }
                if (isCompositeSearch && matchingConditions.IsFullyMatched)
                {
                    matchedComponents.Add(firstMatch != null ? new Tuple<MatchingCondition, IACComponent>(mcFirstMatch, firstMatch) : new Tuple<MatchingCondition, IACComponent>(mcNextComp,nextComp));
                    break;
                }
                if (!isCompositeSearch)
                    break;
                // Move to the next component in the hierarchy
                nextComp = nextComp.ParentACComponent; // This should be set to the next component in your logic
            }

            foreach (var child in component.ACComponentChilds)
            {
                // Recursively search in child components
                SearchMatchingComponents(child, ref matchedComponents, classSearchConditions, isCompositeSearch);
            }
        }

        protected MCP_InstanceInfo BuildInstanceInfo(IACComponent root, List<Tuple<MatchingCondition, IACComponent>> matchedComponents)
        {
            if (matchedComponents == null || !matchedComponents.Any())
                return null;
            MCP_InstanceInfo mcpRoot = new MCP_InstanceInfo
            {
                BaseClassID = Guid.Empty.ToString(), // Default value if no base class is found
                ACIdentifier = root.ACIdentifier,
                Description = root.ACCaption,
                ClassID = root.ComponentClass.ACClassID.ToString()
            };

            foreach (var matchingTuple in matchedComponents)
            {
                MCP_InstanceInfo mcpInstance = mcpRoot;
                List<IACComponent> reversePath = GetReversePath(root, matchingTuple.Item2);
                foreach (IACComponent instanceInPath in reversePath)
                {
                    MCP_InstanceInfo mcpChild = mcpInstance.Childs.Where(c => c.ACIdentifier == instanceInPath.ACIdentifier).FirstOrDefault();
                    if (mcpChild == null)
                    {
                        mcpChild = new MCP_InstanceInfo
                        {
                            ACIdentifier = instanceInPath.ACIdentifier,
                            Description = instanceInPath.ACCaption,
                            ClassID = instanceInPath.ComponentClass?.ACClassID.ToString(),
                            BaseClassID = instanceInPath.ComponentClass?.BaseClass?.ACClassID.ToString(),
                            MatchingBaseClassID = matchingTuple.Item2 == instanceInPath ? matchingTuple.Item1.Class.ACClassID.ToString() : null
                        };
                        mcpInstance.Childs.Add(mcpChild);
                    }
                    mcpInstance = mcpChild;
                }
            }

            return mcpRoot;
        }

        protected List<IACComponent> GetReversePath(IACComponent root, IACComponent instance)
        {
            List<IACComponent> reversePath = new List<IACComponent>();
            do
            {
                reversePath.Insert(0, instance); // Insert at the beginning to reverse the order
                instance = instance.ParentACComponent; // Assuming ParentACComponent is the property to get the parent
                if (instance == root)
                    break;
            }
            while (instance != null);
            return reversePath;
        }
        #endregion


        #region Property Info
        public string AppGetPropertyInfo(IACComponent requester, string classID)
        {
            try
            {
                if (!string.IsNullOrEmpty(classID) && Guid.TryParse(classID, out Guid classGuid))
                {
                    gip.core.datamodel.ACClass acType = ACRoot.SRoot.Database.ContextIPlus.GetACType(classGuid);
                    if (acType != null)
                    {
                        var properties = new List<MCP_PropertyInfo>();

                        foreach (gip.core.datamodel.ACClassProperty acProp in acType.Properties)
                        {
                            if (acProp.ACPropUsage <= Global.ACPropUsages.Property
                                || acProp.ACPropUsage == Global.ACPropUsages.Configuration
                                || acProp.ACPropUsage == Global.ACPropUsages.AccessPrimary
                                || acProp.ACPropUsage == Global.ACPropUsages.Access)
                            {
                                gip.core.datamodel.ACClass genericACClass = null;
                                if (!string.IsNullOrEmpty(acProp.GenericType))
                                {
                                    int indexOfType = acProp.GenericType.LastIndexOf(".");
                                    string acTypeName = indexOfType > 0 ? acProp.GenericType.Substring(indexOfType + 1) : acProp.GenericType;
                                    genericACClass = ACRoot.SRoot.Database.ContextIPlus.GetACType(acTypeName);
                                }
                                var propInfo = new MCP_PropertyInfo
                                {
                                    ACIdentifier = acProp.ACIdentifier,
                                    Description = acProp.ACCaption,
                                    DataType = acProp.ObjectFullType != null ? acProp.ObjectFullType.Name : acProp.ObjectType?.Name,
                                    IsReadOnly = !acProp.IsInput,
                                    Group = acProp.ACGroup,
                                    InnerDataTypeClassID = acProp.ValueTypeACClassID.ToString(),
                                    GenericTypeClassID = genericACClass != null ? genericACClass.ACClassID.ToString() : null
                                };

                                properties.Add(propInfo);
                            }
                        }

                        // Wrap properties in a type context
                        var response = new MCP_TypeInfoWithProperties
                        {
                            ACIdentifier = acType.ACIdentifier,
                            Description = acType.ACCaption,
                            ClassID = acType.ACClassID.ToString(),
                            BaseClassID = acType.BaseClass?.ACClassID.ToString() ?? Guid.Empty.ToString(),
                            IsTypeOfAInstance = acType.ACProject?.ACProjectType == Global.ACProjectTypes.Application
                                              || acType.ACProject?.ACProjectType == Global.ACProjectTypes.Service,
                            IsTypeOfADbTable = acType.ACKind == Global.ACKinds.TACDBA,
                            IsWorkflowType = acType.IsWorkflowType,
                            IsMultiInstanceType = acType.IsMultiInstance || acType.IsWorkflowType,
                            IsCodeOnGithub = !string.IsNullOrEmpty(acType.AssemblyQualifiedName),
                            ManualMCP = acType.Comment,
                            Properties = properties
                        };

                        return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false });
                    }
                }
            }
            catch (Exception ex)
            {
                requester.Messages.LogException(requester.GetACUrl(), nameof(AppGetPropertyInfo), ex, true);
                return CreateExceptionResponse(ex);
            }

            return JsonSerializer.Serialize(new MCP_TypeInfoWithProperties(), new JsonSerializerOptions { WriteIndented = false });
        }
        #endregion


        #region Method Info
        public string AppGetMethodInfo(IACComponent requester, string classID)
        {
            try
            {
                if (!string.IsNullOrEmpty(classID) && Guid.TryParse(classID, out Guid classGuid))
                {
                    gip.core.datamodel.ACClass acType = ACRoot.SRoot.Database.ContextIPlus.GetACType(classGuid);
                    if (acType != null)
                    {
                        var methods = new List<MCP_MethodInfo>();

                        foreach (var method in acType.Methods)
                        {
                            if (method.ACKind == Global.ACKinds.MSMethod)
                            {
                            }

                            var methodInfo = new MCP_MethodInfo
                            {
                                ACIdentifier = method.ACIdentifier,
                                Description = method.ACCaption,
                                ManualMCP = method.Comment
                            };

                            // Try to get parameter information from XML
                            if (!string.IsNullOrEmpty(method.XMLACMethod))
                            {
                                try
                                {
                                    var acMethod = method.TypeACSignature();
                                    //var acMethod = ACClassMethod.DeserializeACMethod(method.XMLACMethod);
                                    if (acMethod != null)
                                    {
                                        methodInfo.Parameters = acMethod.ParameterValueList?.Select(p => new MCP_ParameterInfo
                                        {
                                            Name = p.ACIdentifier,
                                            DataType = p.ObjectType?.Name,
                                            IsRequired = p.Option == Global.ParamOption.Required
                                        }).ToList() ?? new List<MCP_ParameterInfo>();

                                        methodInfo.ReturnType = acMethod.ResultValueList?.FirstOrDefault()?.ObjectType?.Name;
                                    }
                                }
                                catch
                                {
                                    // If XML parsing fails, continue without parameter info
                                }
                            }
                            else
                            {
                                methodInfo.ReturnType = method.ObjectType != null ? method.ObjectType.Name : "void";
                            }

                            methods.Add(methodInfo);

                            if (!method.IsAutoenabled)
                            {
                                methodInfo = new MCP_MethodInfo
                                {
                                    ACIdentifier = Const.IsEnabledPrefix + method.ACIdentifier,
                                    Description = Const.IsEnabledPrefix + ": " + method.ACCaption,
                                    ReturnType = "bool"
                                };
                                methods.Add(methodInfo);
                            }
                        }

                        // Wrap methods in a type context
                        var response = new MCP_TypeInfoWithMethods
                        {
                            ACIdentifier = acType.ACIdentifier,
                            Description = acType.ACCaption,
                            ClassID = acType.ACClassID.ToString(),
                            BaseClassID = acType.BaseClass?.ACClassID.ToString() ?? Guid.Empty.ToString(),
                            IsTypeOfAInstance = acType.ACProject?.ACProjectType == Global.ACProjectTypes.Application
                                              || acType.ACProject?.ACProjectType == Global.ACProjectTypes.Service,
                            IsTypeOfADbTable = acType.ACKind == Global.ACKinds.TACDBA,
                            IsWorkflowType = acType.IsWorkflowType,
                            IsMultiInstanceType = acType.IsMultiInstance || acType.IsWorkflowType,
                            IsCodeOnGithub = !string.IsNullOrEmpty(acType.AssemblyQualifiedName),
                            ManualMCP = acType.Comment,
                            Methods = methods
                        };

                        return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false });
                    }
                }
            }
            catch (Exception ex)
            {
                requester.Messages.LogException(requester.GetACUrl(), nameof(AppGetMethodInfo), ex, true);
                return CreateExceptionResponse(ex);
            }

            return JsonSerializer.Serialize(new MCP_TypeInfoWithMethods(), new JsonSerializerOptions { WriteIndented = false });
        }
        #endregion

        #region ACUrlCommand
        public string ExecuteACUrlCommand(IACComponent requester, string acUrl, bool writeProperty = false, ushort detailLevel = 0, string parametersJson = "")
        {
            try
            {
                List<KeyValuePair<string, object>> parametersKVP = null;
                string[] bulkValues = null;
                StringBuilder sbErr = new StringBuilder();
                StringBuilder sbRec = new StringBuilder();
                // Normalize ACUrl if LLM tries to use dots
                if (!string.IsNullOrEmpty(acUrl))
                {
                    if (acUrl.IndexOf(ACUrlHelper.Delimiter_RelativePath) >= 0)
                    {
                        acUrl = acUrl.Replace(ACUrlHelper.Delimiter_RelativePath, ACUrlHelper.Delimiter_DirSeperator);
                        sbRec.AppendLine("Do not use dots '.' for addressing in ACUrl. Only backslashes '\\'!");
                    }
                }

                if (!string.IsNullOrEmpty(parametersJson))
                {
                    // Use the enhanced method that can handle both JSON arrays and key-value pairs
                    parametersKVP = ConvertJsonParametersToObjects(parametersJson);
                    if (parametersKVP == null)
                        bulkValues = SplitParamsToArray(parametersJson);
                }

                List<MCP_ACUrlCommandResult> results = new List<MCP_ACUrlCommandResult>();
                string[] acUrlCommands = SplitParamsToArray(acUrl);
                if (acUrlCommands != null && acUrlCommands.Length > 0)
                {
                    int countACUrlCommands = acUrlCommands.Length;
                    int i = 0;
                    foreach (var command in acUrlCommands)
                    {
                        string acUrlCommand = command.Replace("\\Root", "");
                        acUrlCommand = acUrlCommand.Replace("\\ACComponentManager", "");

                        object result = null;
                        int indexMethodDelimiter = acUrlCommand.IndexOf(ACUrlHelper.Delimiter_InvokeMethod);
                        // If Method invocation is specified
                        if (indexMethodDelimiter > 0)
                        {
                            string acUrlOfInstance = acUrlCommand.Substring(0, indexMethodDelimiter);
                            string methodName = acUrlCommand.Substring(indexMethodDelimiter + 1);
                            if (!string.IsNullOrEmpty(acUrlOfInstance) && !string.IsNullOrEmpty(methodName))
                            {
                                ACComponent acComp = ACRoot.SRoot.ACUrlCommand(acUrlOfInstance, null) as ACComponent;
                                if (acComp != null)
                                {
                                    object[] parameters = null;
                                    if (parametersKVP != null && parametersKVP.Any())
                                        parameters = ConvertKVPValues(acComp, methodName, parametersKVP, i, countACUrlCommands, sbErr, sbRec);
                                    else if (bulkValues != null && bulkValues.Any())
                                        parameters = ConvertBulkValues(acComp, methodName, bulkValues, i, countACUrlCommands, sbErr, sbRec);
                                    result = acComp.ExecuteMethod(methodName, parameters);
                                }
                                else
                                {
                                    ACUrlTypeInfo acUrlTypeInfo = new ACUrlTypeInfo();
                                    if (ACRoot.SRoot.ACUrlTypeInfo(acUrlOfInstance, ref acUrlTypeInfo))
                                    {
                                        ACUrlTypeSegmentInfo lastComp = acUrlTypeInfo.GetLastComponent();
                                        ACUrlTypeSegmentInfo lastElement = acUrlTypeInfo.LastOrDefault();
                                        if (lastComp != lastElement)
                                        {
                                            sbErr.AppendLine(string.Format("Incorrect addressing of method calls! You cannot make method calls on properties. Use the next parent element with address {0}, which is an instance derived from ACComponent. " +
                                                "Check the method signature of method {1} using AppGetMethodInfo and you may need to pass the property name {2} as a parameter.", lastComp.ACUrl, methodName, lastElement.ACUrl.Replace(lastComp.ACUrl, "")));
                                        }
                                        else
                                        {
                                            sbErr.AppendLine("Incorrect addressing of method calls! You cannot make method calls on properties. Use the next parent element which is an instance derived from ACComponent. " +
                                                "Check the method signature of method {0} using AppGetMethodInfo and you may need to pass the property name as a parameter.");
                                        }
                                    }
                                    else
                                    {

                                        // You can't make method calls on properties. Use the parent element, which is a component, for that.
                                        sbErr.AppendLine("Incorrect addressing of method calls! You cannot make method calls on properties. Use the next parent element which is an instance derived from ACComponent. " +
                                            "Check the method signature of method {0} using AppGetMethodInfo and you may need to pass the property name as a parameter.");
                                    }
                                }
                            }
                            else
                            {
                                if (parametersKVP  != null && parametersKVP.Any())
                                    result = ACRoot.SRoot.ACUrlCommand(acUrlCommand, parametersKVP.Select(c => c.Value).ToArray());
                                else if (bulkValues != null && bulkValues.Any())
                                    result = ACRoot.SRoot.ACUrlCommand(acUrlCommand, bulkValues.Select(c => (object) c).ToArray());
                                else
                                    result = ACRoot.SRoot.ACUrlCommand(acUrlCommand, null);
                            }
                        }
                        // Property read / write
                        else
                        {
                            // type conversion:
                            if (writeProperty && bulkValues != null && bulkValues.Any())
                            {
                                ACUrlTypeInfo acUrlTypeInfo = new ACUrlTypeInfo();
                                if (ACRoot.SRoot.ACUrlTypeInfo(acUrlCommand, ref acUrlTypeInfo))
                                {
                                    ACUrlTypeSegmentInfo componentInfo = acUrlTypeInfo.GetLastComponent();
                                    ACUrlTypeSegmentInfo propertyInfo = acUrlTypeInfo.LastOrDefault();
                                    result = SetACPropertyValue(acUrlCommand, acUrlTypeInfo, componentInfo, propertyInfo, null, bulkValues[i], sbErr, sbRec);                                }
                                else
                                {
                                    sbErr.AppendLine(string.Format("Path {0} not found", acUrlCommand));
                                }
                            }
                            else if (writeProperty && parametersKVP != null && parametersKVP.Any())
                            {
                                ACUrlTypeInfo acUrlTypeInfo = new ACUrlTypeInfo();
                                if (ACRoot.SRoot.ACUrlTypeInfo(acUrlCommand, ref acUrlTypeInfo))
                                {
                                    ACUrlTypeSegmentInfo componentInfo = acUrlTypeInfo.GetLastComponent();
                                    if (componentInfo != null)
                                    {
                                        if (parametersKVP[0].Key == "0")
                                        {
                                            ACUrlTypeSegmentInfo propertyInfo = acUrlTypeInfo.LastOrDefault();
                                            result = SetACPropertyValue(acUrlCommand, acUrlTypeInfo, componentInfo, propertyInfo, null, parametersKVP[i].Value.ToString(), sbErr, sbRec);
                                        }
                                        else
                                        {
                                            // If one command and mulle parameters are given, then each parameter is a ACIdentifier of child of the component
                                            if (countACUrlCommands == 1)
                                            {
                                                foreach (var kvpValue in parametersKVP)
                                                {
                                                    ACUrlTypeInfo acUrlTypeInfo2 = new ACUrlTypeInfo();
                                                    string acUrlCommandChild = acUrlCommand + ACUrlHelper.Delimiter_DirSeperator + kvpValue.Key;
                                                    if (ACRoot.SRoot.ACUrlTypeInfo(acUrlCommandChild, ref acUrlTypeInfo2))
                                                    {
                                                        ACUrlTypeSegmentInfo propertyInfoParent = acUrlTypeInfo.LastOrDefault();
                                                        ACUrlTypeSegmentInfo propertyInfo = acUrlTypeInfo2.LastOrDefault();
                                                        result = SetACPropertyValue(acUrlCommandChild, acUrlTypeInfo2, componentInfo, propertyInfo, propertyInfoParent, kvpValue.Value.ToString(), sbErr, sbRec);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ACUrlTypeInfo acUrlTypeInfo2 = new ACUrlTypeInfo();
                                                string acUrlCommandChild = acUrlCommand + ACUrlHelper.Delimiter_DirSeperator + parametersKVP[i].Key;
                                                if (ACRoot.SRoot.ACUrlTypeInfo(acUrlCommandChild, ref acUrlTypeInfo2))
                                                {
                                                    ACUrlTypeSegmentInfo propertyInfoParent = acUrlTypeInfo.LastOrDefault();
                                                    ACUrlTypeSegmentInfo propertyInfo = acUrlTypeInfo2.LastOrDefault();
                                                    result = SetACPropertyValue(acUrlCommandChild, acUrlTypeInfo2, componentInfo, propertyInfo, propertyInfoParent, parametersKVP[i].Value.ToString(), sbErr, sbRec);
                                                }
                                                else
                                                {
                                                    sbErr.AppendLine(string.Format("Path {0} not found", acUrlCommandChild));
                                                }
                                            }
                                        }
                                    }
                                    else
                                        sbErr.AppendLine(string.Format("Path {0} not found", acUrlCommand));
                                }
                                else
                                {
                                    sbErr.AppendLine(string.Format("Path {0} not found", acUrlCommand));
                                }
                            }
                            else
                                result = ACRoot.SRoot.ACUrlCommand(acUrlCommand);
                        }

                        string errorMsg = sbErr.ToString();
                        string recommendation = sbRec.ToString();
                        var response = new MCP_ACUrlCommandResult
                        {
                            Success = string.IsNullOrEmpty(errorMsg),
                            ACUrl = command,
                            Result = ConvertResultToJson(result, detailLevel, parametersKVP, bulkValues),
                            ResultType = result?.GetType()?.Name,
                            Error = errorMsg,
                            Recommendation = recommendation
                        };
                        results.Add(response);
                        i++;
                    }
                }

                return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                requester.Messages.LogException(requester.GetACUrl(), nameof(ExecuteACUrlCommand), ex, true);
                return CreateExceptionResponse(ex);
            }
        }

        protected readonly static Type _TypeGuid = typeof(Guid);
        protected object SetACPropertyValue(string acUrlCommand, ACUrlTypeInfo acUrlTypeInfo, ACUrlTypeSegmentInfo componentInfo, ACUrlTypeSegmentInfo propertyInfo, ACUrlTypeSegmentInfo parentPropertyInfo, string value, StringBuilder sbErr, StringBuilder sbRec)
        {
            if (componentInfo == null)
            {
                sbErr.AppendLine(string.Format("Component not found for ACUrl '{0}'\r\n", acUrlCommand));
                return null;
            }
            if (propertyInfo == null)
            {
                sbErr.AppendLine(string.Format("Property not found for ACUrl '{0}'\r\n", acUrlCommand));
                return null;
            }
            if (string.IsNullOrEmpty(value))
            {
                sbErr.AppendLine(string.Format("Value is empty for ACUrl '{0}'\r\n", acUrlCommand));
                return null;
            }
            IACComponent component = componentInfo.Value as IACComponent;
            if (component == null)
            {
                sbErr.AppendLine(string.Format("Component not found for ACUrl '{0}'\r\n", acUrlCommand));
                return null;
            }
            if (componentInfo == propertyInfo)
            {
                sbErr.AppendLine(string.Format("You cannot set a value on the component '{0}' itself. Append the ACIdentifier (Name) of the property to the ACUrl.\r\n", propertyInfo.ACUrl));
                return null;
            }


            object convertedValue = null;
            ACUrlTypeSegmentInfo selectionPropertyInfo = propertyInfo;

            if (_TypeGuid == propertyInfo.ObjectFullType)
            {
                Guid guidKey;
                if (!Guid.TryParse(value, out guidKey))
                {
                    sbErr.AppendLine(string.Format("You cannot set a GUID value on '{0}' because passed value is not a GUID\r\n", acUrlCommand));
                    return null;
                }

                if (parentPropertyInfo == null)
                {
                    int countInfos = acUrlTypeInfo.Count;
                    if (countInfos > 2)
                    {
                        var penultimate = acUrlTypeInfo[countInfos - 2];
                        if (penultimate != propertyInfo && penultimate != componentInfo)
                            parentPropertyInfo = penultimate;
                    }
                }
                if (parentPropertyInfo != null)
                {
                    string fieldName = propertyInfo.SegmentName;
                    if (typeof(VBEntityObject).IsAssignableFrom(parentPropertyInfo.ObjectFullType))
                    {
                        string typeNameOfEntityObject = parentPropertyInfo.ObjectFullType.Name;
                        // Check if the property is a primary key of an EntityObject
                        if (fieldName.Equals(typeNameOfEntityObject + "ID", StringComparison.OrdinalIgnoreCase))
                        {
                            // LLM want's to set the primary key of the EntityObject but means a selection change
                            if (parentPropertyInfo.ACType is gip.core.datamodel.ACClassProperty aCClassPropertySelOrCurrA
                                && ((aCClassPropertySelOrCurrA.ACPropUsage == Global.ACPropUsages.Current || aCClassPropertySelOrCurrA.ACPropUsage == Global.ACPropUsages.Selected) && !string.IsNullOrEmpty(aCClassPropertySelOrCurrA.ACGroup)))
                            {
                                selectionPropertyInfo = parentPropertyInfo;
                            }
                            else
                            {
                                sbErr.AppendLine(string.Format("Changing primary Keys on Entity framework objects is not allowed! ACUrl: '{0}'\r\n", acUrlCommand));
                                return null;
                            }
                        }
                    }
                    else if (typeof(IACObjectKeyComparer).IsAssignableFrom(selectionPropertyInfo.ObjectFullType))
                    {
                        // LLM want's to set the primary key of the EntityObject but means a selection change
                        if (parentPropertyInfo.ACType is gip.core.datamodel.ACClassProperty aCClassPropertySelOrCurrA
                            && ((aCClassPropertySelOrCurrA.ACPropUsage == Global.ACPropUsages.Current || aCClassPropertySelOrCurrA.ACPropUsage == Global.ACPropUsages.Selected) && !string.IsNullOrEmpty(aCClassPropertySelOrCurrA.ACGroup)))
                        {
                            selectionPropertyInfo = parentPropertyInfo;
                        }
                        else if (parentPropertyInfo.Value != null && parentPropertyInfo.Value is IACObjectKeyComparer comparer && comparer.IsKey(fieldName))
                        {
                            sbErr.AppendLine(string.Format("Changing keys on complex objects is not allowed! ACUrl: '{0}'\r\n", acUrlCommand));
                            return null;
                        }
                    }
                }
            }

            if (selectionPropertyInfo.ACType != null
                && selectionPropertyInfo.ACType is gip.core.datamodel.ACClassProperty aCClassPropertySelOrCurr
                && ((aCClassPropertySelOrCurr.ACPropUsage == Global.ACPropUsages.Current || aCClassPropertySelOrCurr.ACPropUsage == Global.ACPropUsages.Selected) && !string.IsNullOrEmpty(aCClassPropertySelOrCurr.ACGroup)))
            {
                gip.core.datamodel.ACClassProperty aCClassPropertyList = component.ComponentClass.Properties.Where(c => c.ACGroup == aCClassPropertySelOrCurr.ACGroup && c.ACPropUsage == Global.ACPropUsages.List).FirstOrDefault();
                if (aCClassPropertyList != null)
                {
                    IACPropertyBase propListInstance = component.GetProperty(aCClassPropertyList.ACIdentifier);
                    if (typeof(IACObjectKeyComparer).IsAssignableFrom(selectionPropertyInfo.ObjectFullType))
                    {
                        foreach (IACObjectKeyComparer objectRef in propListInstance.Value as System.Collections.IEnumerable)
                        {
                            if (objectRef != null && objectRef.KeyEquals(value))
                            {
                                convertedValue = objectRef;
                                break;
                            }
                        }
                        if (convertedValue == null)
                        {
                            sbErr.AppendLine(string.Format("This property with ACUrl '{0}' is a complex value. " +
                                "You can only set references by passing a key {1}. " +
                                "First, read the list value {2} of the common group {3} with a detail level of 1 or 2 to determine the ID and execute this command again.",
                                selectionPropertyInfo.ACUrl, selectionPropertyInfo.ObjectFullType.Name + "ID", aCClassPropertyList.ACIdentifier, aCClassPropertyList.ACGroup));
                            return null;
                        }
                    }
                }
            }

            if (convertedValue == null)
            {
                convertedValue = ACConvert.XMLToObject(propertyInfo.ObjectFullType, value, true, component != null && component.Database != null ? component.Database : ACRoot.SRoot.Database.ContextIPlus);
                if (convertedValue == null)
                {
                    sbErr.AppendLine(string.Format("Failed to convert value '{0}' to type '{1}' for ACUrl '{2}'", value, propertyInfo.ObjectFullType?.Name ?? "unknown", acUrlCommand));
                    return null;
                }
            }
            return ACRoot.SRoot.ACUrlCommand(acUrlCommand, convertedValue);
        }
        #endregion


        #region Create new Instance
        public virtual string AppCreateNewInstance(IACComponent requester, string classID, string acUrl)
        {
            try
            {
                if (!String.IsNullOrEmpty(acUrl))
                    acUrl = acUrl.Replace("\\Root", "");
                else
                    acUrl = Const.BusinessobjectsACUrl;

                MCP_InstanceInfo instanceInfo = null;
                bool sendRecommendation = false;
                StringBuilder sb = new StringBuilder();
                if (!Guid.TryParse(classID, out Guid classGuid))
                {
                    gip.core.datamodel.ACClass thesaurausCls = null;
                    foreach (var thesaurus in ThesaurusByACId)
                    {
                        if (thesaurus.TryGetValue(classID, out thesaurausCls))
                            break;
                    }

                    if (thesaurausCls == null)
                    {
                        sb.AppendLine($"You didn't provide a valid ClassID (CId field) with '{classID}'");
                        sendRecommendation = true;
                    }
                }
                if (classGuid != Guid.Empty && ThesaurusByCId.TryGetValue(classGuid, out gip.core.datamodel.ACClass cls))
                {
                    IACComponent parentComp = ACRoot.SRoot.ACUrlCommand(acUrl, null) as IACComponent;
                    if (parentComp != null)
                    {
                        IACComponent newInstance = parentComp.StartComponent(cls, cls, new ACValueList()
                                    {
                                            // Always use a separate Context for new instances when using MCP to avoid conflicts if multiple Agents are working with iplus at the same time
                                            new ACValue(Const.ParamSeperateContext, typeof(bool), true)
                                            //,new ACValue(Const.SkipSearchOnStart, typeof(bool), true)
                                    }) as IACComponent;
                        if (newInstance != null)
                        {
                            instanceInfo = BuildInstanceInfo(ACRoot.SRoot, 
                                new List<Tuple<MatchingCondition, IACComponent>>() 
                                { 
                                    new Tuple<MatchingCondition, IACComponent>(new MatchingCondition() { Class = cls, IsMatched = true }, newInstance) 
                                }
                                );
                        }
                        else
                        {
                            sb.AppendLine($"Failed to create new instance of class '{cls.ACIdentifier}' at ACUrl '{acUrl}'");
                            sendRecommendation = true;
                        }
                    }
                    else
                    {
                        sb.AppendLine($"Parent component not found for ACUrl '{acUrl}'");
                        sendRecommendation = true;
                    }
                }
                else
                {
                    sb.AppendLine($"Class with ID '{classID}' not found in thesaurus.");
                    sendRecommendation = true;
                }

                if (instanceInfo == null)
                {
                    sb.AppendLine("Instance not created.");
                    instanceInfo = new MCP_InstanceInfo
                    {
                        ACIdentifier = ACRoot.SRoot.ACIdentifier,
                        Description = sb.ToString(),
                        ClassID = ACRoot.SRoot.ComponentClass.ACClassID.ToString(),
                        BaseClassID = Guid.Empty.ToString()
                    };
                }
                else if (sendRecommendation)
                {
                    instanceInfo.Description = sb.ToString();
                }
                return JsonSerializer.Serialize(instanceInfo, SerializerOptions);
            }
            catch (Exception ex)
            {
                requester.Messages.LogException(requester.GetACUrl(), nameof(AppGetInstanceInfo), ex, true);
                return CreateExceptionResponse(ex);
            }
        }
        #endregion

        #endregion

    }

}