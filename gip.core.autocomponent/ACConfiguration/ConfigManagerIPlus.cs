// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IPlus Config Manager'}de{'IPlus Config Manager'}", Global.ACKinds.TPARole, Global.ACStorableTypes.NotStorable, false, false)]
    public class ConfigManagerIPlus : PARole, IACConfigProvider
    {

        #region c´tors
        public ConfigManagerIPlus(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        public const string C_DefaultServiceACIdentifier = "VarioConfigManager";
        #endregion

        #region static Methods
        public static ConfigManagerIPlus GetServiceInstance(ACComponent requester)
        {
            return GetServiceInstance<ConfigManagerIPlus>(requester, C_DefaultServiceACIdentifier, CreationBehaviour.OnlyLocal);
        }

        public static ACRef<ConfigManagerIPlus> ACRefToServiceInstance(ACComponent requester)
        {
            ConfigManagerIPlus serviceInstance = GetServiceInstance(requester);
            if (serviceInstance != null)
                return new ACRef<ConfigManagerIPlus>(serviceInstance, requester);
            return null;
        }
        #endregion

        #region IACConfigProvider

        #region IACConfigProvider -> Config Stores

        public virtual List<IACConfigStore> GetACConfigStores(List<IACConfigStore> callingConfigStoreList)
        {
            List<IACConfigStore> resultList = new List<IACConfigStore>();
            foreach (IACConfigStore configStore in callingConfigStoreList)
            {
                if (configStore is ACClassMethod)
                {
                    resultList = GetACConfigStoresImplementation((configStore as gip.core.datamodel.ACClassMethod), callingConfigStoreList);
                }
                if (configStore is ACClass)
                {
                    resultList = GetACConfigStoresImplementation((configStore as gip.core.datamodel.ACClass), callingConfigStoreList);
                }
                if (configStore is ACProgram)
                {
                    resultList = GetACConfigStoresImplementation((configStore as gip.core.datamodel.ACProgram), callingConfigStoreList);
                }
            }
            return resultList;
        }

        #endregion

        #region  IACConfigProvider -> Configuration Lists

        public virtual List<IACConfig> GetConfigurationList(List<IACConfigStore> callingConfigStoreList, string preConfigACUrl, List<string> localConfigACUrlList, Guid? vbiACClassID, bool fetchFirstConfig = true, bool reloadParams = false)
        {
            List<IACConfig> resultList = new List<IACConfig>();
            List<IACConfigStore> configStoreList = GetACConfigStores(callingConfigStoreList).OrderByDescending(x => x.OverridingOrder).ToList();
            List<string> localConfigACUrlListFiltered = localConfigACUrlList.ToList();
            foreach (IACConfigStore configStore in configStoreList)
            {
                if (reloadParams)
                    configStore.ClearCacheOfConfigurationEntries(); // do refresh of config items
                List<IACConfig> tmpConfigList = ACConfigHelper.GetStoreConfigurationList(configStore.ConfigurationEntries, preConfigACUrl, localConfigACUrlListFiltered, !reloadParams, vbiACClassID);
                if (fetchFirstConfig && tmpConfigList != null && tmpConfigList.Any())
                    localConfigACUrlListFiltered.RemoveAll(x => tmpConfigList.Select(c => c.LocalConfigACUrl).Contains(x)); // Remove all founded configurations on higher level
                if (tmpConfigList != null)
                {
                    resultList.AddRange(tmpConfigList);
                }
            }
            return resultList;
        }

        /// <summary>
        /// Fetching configuration for GUI
        /// </summary>
        /// <param name="acMethod"></param>
        /// <param name="mandatoryConfigStores"></param>
        /// <param name="preACUrl"></param>
        /// <param name="localACURL"></param>
        /// <param name="reloadParams"></param>
        /// <returns></returns>
        public List<ACConfigParam> GetACConfigParamList(ACMethod acMethod, List<IACConfigStore> mandatoryConfigStores, string preACUrl, string localACURL, bool reloadParams = false)
        {
            List<string> listOfProperies = acMethod.ParameterValueList.Select(x => localACURL + @"\" + x.ACIdentifier).ToList();
            List<ACConfigParam> aCConfigParamList =
                acMethod
                .ParameterValueList
                .Select(x =>
                    new ACConfigParam()
                    {
                        ACIdentifier = x.ACIdentifier,
                        ACCaption = acMethod.GetACCaptionForACIdentifier(x.ACIdentifier),
                        ValueTypeACClassID = x.ValueTypeACClass != null ? x.ValueTypeACClass.ACClassID : Guid.Empty
                    }).ToList();
            List<IACConfig> configList = GetConfigurationList(mandatoryConfigStores, preACUrl, listOfProperies, null, false, reloadParams);
            // Add machine-specific config elements in list if exist
            if (configList != null && configList.Any(c => c.VBiACClassID != null))
            {
                var grouppedList = configList.Where(c => c.VBiACClassID != null).GroupBy(c => new { URL = c.LocalConfigACUrl, VBiACClassID = c.VBiACClassID });
                foreach (var tempLocalConfigACUrl in grouppedList.Select(c => c.Key))
                {
                    IACConfig configItem = grouppedList.FirstOrDefault(p => p.Key.VBiACClassID == tempLocalConfigACUrl.VBiACClassID && p.Key.URL == tempLocalConfigACUrl.URL).FirstOrDefault();
                    string acIdentifier = configItem.LocalConfigACUrl;
                    acIdentifier = acIdentifier.Substring(acIdentifier.LastIndexOf('\\') + 1, acIdentifier.Length - acIdentifier.LastIndexOf('\\') - 1);
                    ACConfigParam originalParam = aCConfigParamList.FirstOrDefault(p => p.ACIdentifier == acIdentifier && p.ValueTypeACClassID == configItem.ValueTypeACClass.ACClassID);
                    ACConfigParam additionalParam = ACConfigHelper.FactoryMachineParam(originalParam, configItem.VBACClass);
                    aCConfigParamList.Add(additionalParam);
                }
                aCConfigParamList =
                    aCConfigParamList
                    .OrderBy(c => c.ACIdentifier)
                    .ThenBy(c => c.VBiACClassID == null ? "" : c.VBACClass.ACIdentifier)
                    .ToList();
            }
            if (configList != null)
            {
                foreach (ACConfigParam acConfigParam in aCConfigParamList)
                {
                    acConfigParam.ConfigurationList =
                        configList
                        .Where(x =>
                            x.LocalConfigACUrl == (localACURL + @"\" + acConfigParam.ACIdentifier)
                            &&
                           x.VBiACClassID == acConfigParam.VBiACClassID
                        )
                        .OrderByDescending(x => x.ConfigStore != null ? x.ConfigStore.OverridingOrder : 1)
                        .ToList();
                    acConfigParam.DefaultConfiguration = acConfigParam.ConfigurationList.OrderByDescending(x => x.ConfigStore != null ? x.ConfigStore.OverridingOrder : 1).FirstOrDefault();
                }
            }
            return aCConfigParamList;
        }

        public short WriteConfigIntoACValue(ACMethod acMethod, List<IACConfigStore> mandatoryConfigStores, string preACUrl, string localACURL, Guid? vbiACClassID, bool fetchFirstConfig)
        {
            short hasRuleLevel = 0;
            foreach (ACValue acValue in acMethod.ParameterValueList)
            {
                string localPropertyACUrl = localACURL + @"\" + acValue.ACIdentifier;
                int priorityLevel = 0;
                IACConfig configItem = GetConfiguration(mandatoryConfigStores, preACUrl, localPropertyACUrl, vbiACClassID, out priorityLevel);
                if (configItem != null)
                {
                    if (hasRuleLevel == 0)
                        hasRuleLevel = priorityLevel == 1 ? (short)1 : (short)2;
                    else if (hasRuleLevel == 2 && priorityLevel == 1)
                        hasRuleLevel = 1;
                    Global.ControlModesInfo validationResult = ValidateExpressionParams(configItem, configItem, true);
                    acValue.Value = configItem.Value;
                    if (validationResult.Mode == Global.ControlModes.EnabledWrong && validationResult.FixedValue != null)
                        acValue.Value = validationResult.FixedValue;
                }
            }
            return hasRuleLevel;
        }

        public virtual Global.ControlModesInfo ValidateExpressionParams(IACConfig configItem, IACConfig configItemWithExpression = null, bool fixValue = false)
        {
            if (configItemWithExpression == null)
                configItemWithExpression = configItem;
            if (string.IsNullOrEmpty(configItemWithExpression.Expression))
                return Global.ControlModesInfo.Enabled;
            int codeStart = configItemWithExpression.Expression.IndexOf(ACUrlHelper.OpeningBrace);
            int codeEnd = configItemWithExpression.Expression.LastIndexOf(ACUrlHelper.ClosingBrace);
            if (codeEnd - 1 > codeStart)
            {
                string paramString = configItemWithExpression.Expression.Substring(codeStart + 1, codeEnd - 1);
                try
                {
                    Dictionary<string, double> kvpParams = new Dictionary<string, double>();
                    paramString = paramString.Trim();
                    NameValueCollection nvCol = HttpUtility.ParseQueryString(paramString);
                    foreach (string key in nvCol.AllKeys)
                    {
                        string sVal = nvCol.Get(key);
                        if (!string.IsNullOrEmpty(sVal))
                        {
                            sVal = sVal.Trim(';', ' ');
                            kvpParams.Add(key, Double.Parse(sVal, CultureInfo.InvariantCulture));
                        }
                    }

                    //Example: {MinValue=5&MaxValue=6}
                    return ValidateExpressionParams(configItem, kvpParams, fixValue);
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "OnGetControlModes(10)", ex);
                }
            }
            return Global.ControlModesInfo.Enabled;
        }


        public virtual Global.ControlModesInfo ValidateExpressionParams(IACConfig configItem, Dictionary<string, double> expressionParams, bool fixValue = false)
        {
            Type objectFullType = configItem.ValueTypeACClass?.ObjectFullType;
            if (objectFullType == null)
                return Global.ControlModesInfo.Enabled;

            double? minValue = null;
            double? maxValue = null;
            int? minLength = null;
            int? maxLength = null;
            double val = 0.0;
            if (expressionParams.TryGetValue(nameof(ACClassProperty.MinValue), out val))
                minValue = val;
            val = 0.0;
            if (expressionParams.TryGetValue(nameof(ACClassProperty.MaxValue), out val))
                maxValue = val;
            val = 0.0;
            if (expressionParams.TryGetValue(nameof(ACClassProperty.MinLength), out val))
                minLength = Convert.ToInt32(val);
            val = 0.0;
            if (expressionParams.TryGetValue(nameof(ACClassProperty.MaxLength), out val))
                maxLength = Convert.ToInt32(val);

            Global.ControlModesInfo result = IACObjectReflectionExtension.CheckPropertyMinMax(null, configItem.Value, null, objectFullType, false,
                                                minLength, maxLength,
                                                minValue, maxValue, null, fixValue);
            return result;
        }

        #endregion

        #region  IACConfigProvider -> Configuration one

        public virtual int HasConfiguration(IACConfigStore configStore, List<IACConfigStore> callingConfigStoreList, string preConfigACUrl, string startsWithLocalConfigACUrl, Guid? vbiACClassID)
        {
            List<IACConfigStore> configStoreList = GetACConfigStores(callingConfigStoreList).OrderByDescending(x => x.OverridingOrder).ToList();
            callingConfigStoreList.Remove(configStore);
            List<IACConfig> result = ACConfigHelper.GetStoreConfigurationList(configStore.ConfigurationEntries, preConfigACUrl, startsWithLocalConfigACUrl, false, vbiACClassID);
            if (result != null && result.Any())
                return 1;
            foreach (IACConfigStore tmpConfigStore in configStoreList)
            {
                result = ACConfigHelper.GetStoreConfigurationList(tmpConfigStore.ConfigurationEntries, preConfigACUrl, startsWithLocalConfigACUrl, false, vbiACClassID);
                if (result != null && result.Any())
                    return 2;
            }
            return 0;
        }

        public virtual IACConfig GetConfiguration(List<IACConfigStore> callingConfigStoreList, string preConfigACUrl, string localConfigACUrl, Guid? vbiACClassID, out int priorityLevel)
        {
            IACConfig config = null;
            List<IACConfigStore> configStoreList = GetACConfigStores(callingConfigStoreList).OrderByDescending(x => x.OverridingOrder).ToList();
            priorityLevel = 0;
            foreach (IACConfigStore configStore in configStoreList)
            {
                priorityLevel++;
                config = ACConfigHelper.GetStoreConfiguration(configStore.ConfigurationEntries, preConfigACUrl, localConfigACUrl, true, vbiACClassID);
                if (config != null)
                    break;
            }
            return config;
        }

        public virtual void DeleteConfigNode(IACEntityObjectContext db, Guid acClassWFID)
        {
            List<ACClassMethodConfig> configs = db.ContextIPlus.ACClassMethodConfig.Where(c => c.ACClassWFID == acClassWFID).ToList();
            configs.ForEach(x => x.DeleteACObject(db.ContextIPlus, false));
        }

        #endregion

        #endregion

        #region Implementation of overriding confing by this class

        private List<IACConfigStore> GetACConfigStoresImplementation(gip.core.datamodel.ACClassMethod aCClassMethod, List<IACConfigStore> callerConfigStores)
        {
            if (!callerConfigStores.Any(x => (x is gip.core.datamodel.ACClassMethod)
                && (x as gip.core.datamodel.ACClassMethod).ACClassMethodID == aCClassMethod.ACClassMethodID))
            {
                callerConfigStores.Add(aCClassMethod);
                callerConfigStores = GetACConfigStoresImplementation(aCClassMethod.Safe_ACClass, callerConfigStores);
            }
            return callerConfigStores;
        }

        private List<IACConfigStore> GetACConfigStoresImplementation(gip.core.datamodel.ACClass acClass, List<IACConfigStore> callerConfigStores)
        {
            //if (!callerConfigStores.Any(x => x is gip.core.datamodel.ACClass))
            //{
            //    callerConfigStores.Add(acClass);
            //}
            //acClass.Priority = 0;
            return callerConfigStores;
        }

        private List<IACConfigStore> GetACConfigStoresImplementation(gip.core.datamodel.ACProgram acProgram, List<IACConfigStore> callerConfigStores)
        {
            if (!callerConfigStores.Any(x => x is gip.core.datamodel.ACProgram))
            {
                callerConfigStores.Add(acProgram);
            }
            acProgram.OverridingOrder = 6 + GetCallingMethodCount(callerConfigStores);
            return callerConfigStores;
        }

        #endregion

        #region Helper methods

        public int GetCallingMethodCount(List<IACConfigStore> callingConfigStoreList)
        {
            int count = 0;
            if (callingConfigStoreList != null && callingConfigStoreList.Any())
                count = callingConfigStoreList.Count(x => x is gip.core.datamodel.ACClassMethod);
            return count;
        }

        public virtual List<IACConfigStore> AttachConfigStoresToDatabase(IACEntityObjectContext db, List<ACConfigStoreInfo> rmiResult)
        {
            List<IACConfigStore> mandatoryConfigStore = new List<IACConfigStore>();
            if (rmiResult != null)
            {
                foreach (ACConfigStoreInfo configStoreInfo in rmiResult)
                {
                    IACConfigStore dbConfigStoreItem = null;
                    if (db.GetType().Namespace == configStoreInfo.ConfigStoreEntity.EntityType.Namespace)
                        dbConfigStoreItem = GetObjectFromContext(db, configStoreInfo);
                    else if (db.ContextIPlus.DefaultContainerName == configStoreInfo.ConfigStoreEntity.EntityContainerName)
                        dbConfigStoreItem = GetObjectFromContext(db.ContextIPlus, configStoreInfo);
                    else if (!String.IsNullOrEmpty(configStoreInfo.ConfigStoreEntity.EntityContainerName))
                    {
                        IACEntityObjectContext objectContext = ACObjectContextManager.Contexts.Where(c => c.GetType().Namespace == configStoreInfo.ConfigStoreEntity.EntityType.Namespace).FirstOrDefault();
                        if (objectContext != null)
                            dbConfigStoreItem = GetObjectFromContext(objectContext, configStoreInfo);
                    }
                    if (dbConfigStoreItem != null)
                    {
                        dbConfigStoreItem.OverridingOrder = configStoreInfo.Priority;
                        mandatoryConfigStore.Add(dbConfigStoreItem);
                    }
                }
            }
            return mandatoryConfigStore;
        }

        protected virtual IACConfigStore GetObjectFromContext(IACEntityObjectContext db, ACConfigStoreInfo configStoreInfo)
        {

            using (ACMonitor.Lock(db.QueryLock_1X000))
            {
                return db.GetObjectByKey(configStoreInfo.ConfigStoreEntity) as IACConfigStore;
            }
        }

        public static IACConfig ACConfigFactory(IACConfigStore configStore, ACConfigParam acConfigParam, string preValueACUrl, string localConfigACUrl, Guid? vbiACClassID)
        {
            IACConfig newconfiguration = ACConfigFactory(configStore, acConfigParam.ACClassWF, acConfigParam.ValueTypeACClassID, preValueACUrl, localConfigACUrl, vbiACClassID);
            if (acConfigParam.DefaultConfiguration != null && newconfiguration != null)
            {
                newconfiguration.Value = acConfigParam.DefaultConfiguration.Value;
                newconfiguration.Comment = acConfigParam.DefaultConfiguration.Comment;
                newconfiguration.Expression = acConfigParam.DefaultConfiguration.Expression;
            }
            return newconfiguration;
        }

        public static IACConfig ACConfigFactory(IACConfigStore configStore, ACClassWF acClassWF, Guid valueTypeACClassID, string preValueACUrl, string localConfigACUrl, Guid? vbiACClassID)
        {
            ACClass typeForCreate = (configStore as VBEntityObject).GetObjectContext().ContextIPlus.ACClass.Where(c => c.ACClassID == valueTypeACClassID).FirstOrDefault();
            IACConfig configItem = configStore.NewACConfig(acClassWF, typeForCreate);
            configItem.PreConfigACUrl = preValueACUrl;
            configItem.LocalConfigACUrl = localConfigACUrl;
            configItem.VBiACClassID = vbiACClassID;
            Type typeToCreate = typeForCreate.ObjectType;
            try
            {
                object value = Activator.CreateInstance(typeToCreate);
                if (value != null)
                {
                    configItem.Value = value;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ConfigManagerIPlus", "ACConfigFactory", msg);
            }
            return configItem;
        }

        #endregion

        #region Rules

        /// <summary>
        /// Get stored Rule values from defined config store name
        /// </summary>
        /// <param name="callingConfigStores"></param>
        /// <param name="preConfigACUrl"></param>
        /// <param name="localConfigACUrl"></param>
        /// <param name="configStoreUrl"></param>
        /// <returns></returns>
        public List<RuleValue> GetDBStoredRuleValueList(List<IACConfigStore> callingConfigStores, string preConfigACUrl, string localConfigACUrl, string configStoreUrl)
        {
            IACConfigStore configStore = GetACConfigStores(callingConfigStores).Where(x => x.GetACUrl() == configStoreUrl).FirstOrDefault();
            if (configStore == null) return new List<RuleValue>();
            IACConfig configItem = ACConfigHelper.GetStoreConfiguration(configStore.ConfigurationEntries, preConfigACUrl, localConfigACUrl, false, null);
            RuleValueList ruleValueList = null;
            if (configItem != null && (configItem as VBEntityObject).EntityState != EntityState.Detached)
                ruleValueList = (RuleValueList)configItem[RuleValueList.ClassName];
            if (ruleValueList == null)
                return new List<RuleValue>();
            else
            {
                if (ruleValueList.Items == null)
                    ruleValueList.Items = new List<RuleValue>();
                return ruleValueList.Items;
            }
        }

        public RuleValueList GetRuleValueList(List<IACConfigStore> callingConfigStores, string preConfigACUrl, string localConfigACUrl)
        {
            RuleValueList ruleValueList = null;
            List<IACConfigStore> configStores = GetACConfigStores(callingConfigStores).OrderByDescending(x => x.OverridingOrder).ToList();
            IACConfig configItem = null;
            foreach (IACConfigStore configStore in configStores)
            {
                configItem = ACConfigHelper.GetStoreConfiguration(configStore.ConfigurationEntries, preConfigACUrl, localConfigACUrl, true, null);
                if (configItem != null)
                {
                    ruleValueList = (RuleValueList)configItem[RuleValueList.ClassName];
                    break;
                }
            }
            return ruleValueList;
        }

        #endregion

        #region Live-Workflow-Handling

        public static IEnumerable<ACClassTask> GetActiveWorkflows(ACComponent invoker, ACClassMethod acClassMethod, Database db)
        {
            if (invoker == null || acClassMethod == null || db == null)
                throw new ArgumentNullException();

            return db.ACClassTask.Where(c => c.IsDynamic
                                && c.ACProgramID.HasValue
                                && c.ACTaskTypeIndex == (short)Global.ACTaskTypes.WorkflowTask
                                && c.ContentACClassWF != null
                                && c.ContentACClassWF.ACClassMethodID == acClassMethod.ACClassMethodID);
        }

        public static bool IsWorkflowActive(ACComponent invoker, ACClassMethod acClassMethod, Database db)
        {
            if (invoker == null || acClassMethod == null || db == null)
                throw new ArgumentNullException();

            return GetActiveWorkflows(invoker, acClassMethod, db).Any();
        }

        public static bool IsActiveWorkflowChanged(ACComponent invoker, ACClassMethod acClassMethod, Database db)
        {
            if (!IsWorkflowActive(invoker, acClassMethod, db))
                return false;
            return db.ContextIPlus.GetChangedEntities<VBEntityObject>(c => c is ACClassWF || c is ACClassWFEdge).Any();
        }

        public static bool MustConfigBeReloadedOnServer(ACComponent invoker, List<ACClassMethod> visitedMethods, IACEntityObjectContext db)
        {
            if (visitedMethods == null || !visitedMethods.Any())
                return false;
            foreach (ACClassMethod acClassMethod in visitedMethods)
            {
                var query = ConfigManagerIPlus.GetActiveWorkflows(invoker, acClassMethod, db.ContextIPlus);
                if (query.Any())
                {
                    return db.GetChangedEntities<IACConfig>().Any();
                }
            }
            return false;
        }


        public static void ReloadConfigOnServerIfChanged(ACComponent invoker, List<ACClassMethod> visitedMethods, IACEntityObjectContext db, bool reloadWorkflow = false)
        {
            if (visitedMethods == null || !visitedMethods.Any())
                return;
            //bool? hasChangedConfig = null;
            foreach (ACClassMethod acClassMethod in visitedMethods)
            {
                bool wasReloaded = false;
                var query = ConfigManagerIPlus.GetActiveWorkflows(invoker, acClassMethod, db.ContextIPlus);
                if (query.Any())
                {
                    foreach (var acClassTask in query.ToArray())
                    {
                        if (acClassTask.ACClassTask1_ParentACClassTask != null)
                        {
                            string acUrl = acClassTask.ACClassTask1_ParentACClassTask.TaskTypeACClass.GetACUrlComponent();
                            if (!String.IsNullOrEmpty(acUrl))
                            {
                                ACComponent appManager = invoker.ACUrlCommand(acUrl) as ACComponent;
                                if (appManager != null)
                                {
                                    wasReloaded = true;
                                    appManager.ACUrlCommand("!ReloadConfig", acClassMethod.ACClassMethodID);
                                }
                            }
                        }
                    }
                }
                if (!wasReloaded && reloadWorkflow)
                {
                    foreach (ACProject project in acClassMethod.ACClass.ACProject.ACProject_BasedOnACProject)
                    {
                        if (project.RootClass == null || String.IsNullOrEmpty(project.RootClass.ACUrlComponent))
                            continue;
                        ACComponent pAppManager = invoker.ACUrlCommand(project.RootClass.ACUrlComponent, null) as ACComponent;
                        if (pAppManager != null)
                        {
                            pAppManager.ACUrlCommand("!ReloadConfig", acClassMethod.ACClassMethodID);
                        }
                    }
                }
            }
        }

        #endregion

        #region QueryAllCOnfigs

        public virtual List<IACConfig> QueryAllCOnfigs(IACEntityObjectContext db, IACConfigStore sameConfigStore, string preConfigACUrl, string localConfigACUrl, Guid? vbiACClassID)
        {
            List<IACConfig> result = null;
            switch (sameConfigStore.GetType().Name)
            {
                case "ACClass":
                    result =
                        ACConfigQuery<ACClassConfig>.QueryConfigSource(db.ContextIPlus.ACClassConfig, preConfigACUrl, localConfigACUrl, vbiACClassID)
                       .ToList()
                       .Select(c => (IACConfig)c)
                       .ToList();
                    break;
                case "ACClassMethod":
                    result =
                        ACConfigQuery<ACClassMethodConfig>.QueryConfigSource(db.ContextIPlus.ACClassMethodConfig, preConfigACUrl, localConfigACUrl, vbiACClassID)
                        .ToList()
                        .Select(c => (IACConfig)c)
                        .ToList();
                    break;
                case "ACProgram":
                    result =
                        ACConfigQuery<ACClassMethodConfig>.QueryConfigSource(db.ContextIPlus.ACClassMethodConfig, preConfigACUrl, localConfigACUrl, vbiACClassID)
                       .ToList()
                       .Select(c => (IACConfig)c)
                       .ToList();
                    break;
            }
            return result;
        }
        #endregion

        #region Additional 

        public virtual ValidateConfigStoreModel ValidateConfigStores(List<IACConfigStore> mandatoryConfigStores)
        {
            ValidateConfigStoreModel model = new ValidateConfigStoreModel();
            model.IsValid = true;
            model.NotValidConfigStores = new List<IACConfigStore>();
            try
            {
                foreach (IACConfigStore configStore in mandatoryConfigStores)
                {
                    if (!configStore.ValidateConfigurationEntriesWithDB())
                    {
                        model.IsValid = false;
                        model.NotValidConfigStores.Add(configStore);
                    }
                }
            }
            catch (Exception e)
            {
                model.IsValid = false;
                Messages.LogException(GetACUrl(), "ValidateConfigStores(10)", e);
            }

            return model;
        }
        #endregion

        #region ParametersInfo

        public static string GetParametersInfo(IEnumerable<IACConfigStore> configStores, string preConfigACUrl)
        {
            if (configStores == null || !configStores.Any())
                return null;

            try
            {
                var configEntries = 
                    configStores
                    .SelectMany(c => c.ConfigurationEntries.Select(x => new Tuple<decimal, IACConfig>(c.OverridingOrder, x)))
                    .GroupBy(x => x.Item2.LocalConfigACUrl)
                    .Where(x => x.Any(c => !string.IsNullOrEmpty(c.Item2.Expression) && c.Item2.Expression[0] != ACUrlHelper.OpeningBrace))
                    .OrderBy(x => x.Key).ToArray();

                string result = "";

                foreach (var configEntry in configEntries)
                {
                    // Search if any expresion is configured
                    string expression = configEntry.Where(c => c.Item2 != null && !string.IsNullOrEmpty(c.Item2.Expression) && c.Item2.Expression[0] != ACUrlHelper.OpeningBrace).Select(c => c.Item2.Expression).FirstOrDefault();
                    if (!string.IsNullOrEmpty(expression))
                    {
                        // by configuration order descending - last in hierarchy is first in list
                        var sortedItems = configEntry.OrderByDescending(c => c.Item1);
                        // search local (PreConfigACUrl) defined value
                        IACConfig configValue = sortedItems.Where(c => c.Item2 != null && (c.Item2.PreConfigACUrl ?? "") == (preConfigACUrl ?? "")).FirstOrDefault()?.Item2;
                        if (configValue == null)
                        {
                            // if null then value defined on WF definition level
                            configValue = sortedItems.Where(c => c.Item2 != null && string.IsNullOrEmpty(c.Item2.PreConfigACUrl)).FirstOrDefault()?.Item2;
                        }
                        if (configValue != null && configValue.Value != null)
                            result += expression + ": " + configValue.Value.ToString() + "; ";
                    }
                }

                return String.IsNullOrEmpty(result) ? null : result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        #endregion

        #region Misc

        public static List<ACClass> FilterByAllowedInstances(ACClassWF currentACClassWF, List<IACConfigStore> mandantoryConfigStores, ConfigManagerIPlus configManager,
                                                    List<ACClass> possibleInstances)
        {
            List<ACClass> result = new List<ACClass>();

            ACClassMethod wfMethod = currentACClassWF.RefPAACClassMethod;
            if (wfMethod != null)
            {
                IEnumerable<core.datamodel.ACClassWF> groups = wfMethod.ACClassWF_ACClassMethod.Where(c => c.PWACClass.ACKind == Global.ACKinds.TPWGroup);

                List<string> classes = new List<string>();

                int prio = 0;

                foreach (core.datamodel.ACClassWF group in groups)
                {
                    IACConfig config = configManager.GetConfiguration(mandantoryConfigStores, currentACClassWF.ConfigACUrl + "\\",
                                                                           group.ConfigACUrl + @"\Rules\" + ACClassWFRuleTypes.Allowed_instances.ToString(),
                                                                           null, out prio);
                    if (config == null)
                        continue;

                    List<RuleValue> allowedInstancesRuleValueList = RulesCommand.ReadIACConfig(config);
                    classes.AddRange(allowedInstancesRuleValueList.SelectMany(c => c.ACClassACUrl).Distinct());
                }

                if (classes.Any())
                {
                    foreach (var acClass in possibleInstances)
                    {
                        if (classes.Any(c => acClass.ACUrl == c))
                            result.Add(acClass);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Planning

        public virtual bool HasPlanning(IACEntityObjectContext db, IACConfigStore configStore, Guid acClassWFID)
        {
            return false;
        }

        #endregion
    }
}
