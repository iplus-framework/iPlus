// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public class ACConfigReplicationCommand
    {

        public void ReplicationProcess(IACEntityObjectContext db, IACConfigStore configStore, List<IACConfig> updatedConfigItems, List<IACComponentPWNode> multiSelectItems)
        {
            var multiSelectNodes = multiSelectItems
                .Select(x => new
                {
                    pwNodeInfo = x,
                    PWNodeMethod = x.ContentACClassWF.PWACClass.Methods.FirstOrDefault(c => c.ACGroup == Const.ACState && c.ACIdentifier == ACStateConst.SMStarting),
                    PAFunctionMethod = x.ContentACClassWF.RefPAACClassMethod
                })
            .Select(c => new ComponentPWNodeConfigModel
            {
                // pwNodeInfo = c.pwNodeInfo,
                ContentACClassWF = c.pwNodeInfo.ContentACClassWF,
                PWNodeMethod = c.PWNodeMethod,
                PAFunctionMethod = c.PAFunctionMethod,
                PreACUrl = (c.pwNodeInfo as IACConfigURL).PreValueACUrl,
                PWNodeLocalConfigACUrl = c.pwNodeInfo.ContentACClassWF.ConfigACUrl + @"\" + c.PWNodeMethod.ACIdentifier,
                PWNodeParams = c.PWNodeMethod
                                .ACMethod
                                .ParameterValueList
                                .Select(x =>
                                    new ACConfigParam()
                                    {
                                        ACIdentifier = x.ACIdentifier,
                                        ACCaption = c.PWNodeMethod.ACMethod.GetACCaptionForACIdentifier(x.ACIdentifier),
                                        ValueTypeACClassID = x.ValueTypeACClass.ACClassID
                                    })
                                .ToList(),
                PAFunctionLocalConfigACUrl = c.PAFunctionMethod == null ? null : c.pwNodeInfo.ContentACClassWF.ConfigACUrl + @"\" + c.PAFunctionMethod.ACIdentifier,
                PAFunctionParams = c.PAFunctionMethod == null ? null : c.PAFunctionMethod
                                                                        .ACMethod
                                                                        .ParameterValueList
                                                                        .Select(x =>
                                                                            new ACConfigParam()
                                                                            {
                                                                                ACIdentifier = x.ACIdentifier,
                                                                                ACCaption = c.PWNodeMethod.ACMethod.GetACCaptionForACIdentifier(x.ACIdentifier),
                                                                                ValueTypeACClassID = x.ValueTypeACClass.ACClassID
                                                                            })
                                                                        .ToList()
            })
            .ToList();
            if (multiSelectNodes != null)
                foreach (ComponentPWNodeConfigModel item in multiSelectNodes)
                {
                    item.Rules = RulesCommand.ListOfRuleInfoPatterns.Where(x => x.RuleApplyedWFACKindTypes.Contains(item.ContentACClassWF.PWACClass.ACKind))
                        .ToDictionary(key => key.RuleType, val => new List<object>());
                }

            foreach (IACConfig originalConfigItem in updatedConfigItems)
            {

                if (originalConfigItem.LocalConfigACUrl.Contains(@"\Rules\"))
                {
                    ReplicationProcessRuleParam(db, configStore, originalConfigItem, multiSelectNodes);
                }
                else
                {
                    ReplicationProcessNodeFunctionParam(db, configStore, originalConfigItem, multiSelectNodes);
                }
            }
        }

        private void ReplicationProcessNodeFunctionParam(IACEntityObjectContext db, IACConfigStore configStore, IACConfig originalConfigItem, List<ComponentPWNodeConfigModel> multiSelectNodes)
        {
            string acIdentifier = originalConfigItem.LocalConfigACUrl.Split('\\').Reverse().FirstOrDefault();
            IEnumerable<ComponentPWNodeConfigModel> targetNodes = null;
            ACConfigParam configParam = null;
            if (originalConfigItem.LocalConfigACUrl.Contains(@"SMStarting"))
            {
                targetNodes = multiSelectNodes.Where(nod => nod.PWNodeParams != null && nod.PWNodeParams.Where(np => np.ValueTypeACClassID == originalConfigItem.ValueTypeACClass.ACClassID && np.ACIdentifier == acIdentifier).Any());
                if (targetNodes != null)
                {
                    foreach (ComponentPWNodeConfigModel targetNode in targetNodes)
                    {
                        configParam = targetNode.PWNodeParams.FirstOrDefault(np => np.ValueTypeACClassID == originalConfigItem.ValueTypeACClass.ACClassID && np.ACIdentifier == acIdentifier);
                        ReplicationSimpleParamForward(db, configStore, originalConfigItem, configParam, targetNode.PreACUrl, targetNode.PWNodeLocalConfigACUrl, acIdentifier);
                    }
                }
            }
            else
            {
                targetNodes = multiSelectNodes.Where(nod => nod.PAFunctionParams != null && nod.PAFunctionParams.Where(np => np.ValueTypeACClassID == originalConfigItem.ValueTypeACClass.ACClassID && np.ACIdentifier == acIdentifier).Any());
                if (targetNodes != null)
                {
                    foreach (ComponentPWNodeConfigModel targetNode in targetNodes)
                    {
                        configParam = targetNode.PAFunctionParams.FirstOrDefault(np => np.ValueTypeACClassID == originalConfigItem.ValueTypeACClass.ACClassID && np.ACIdentifier == acIdentifier);
                        ReplicationSimpleParamForward(db, configStore, originalConfigItem, configParam, targetNode.PreACUrl, targetNode.PAFunctionLocalConfigACUrl, acIdentifier);
                    }
                }
            }
        }

        private void ReplicationProcessRuleParam(IACEntityObjectContext db, IACConfigStore configStore, IACConfig originalConfigItem, List<ComponentPWNodeConfigModel> multiSelectNodes)
        {
            string ruleTypeName = originalConfigItem.LocalConfigACUrl.Split('\\').Reverse().FirstOrDefault();
            ACClassWFRuleTypes ruleType = (ACClassWFRuleTypes)Enum.Parse(typeof(ACClassWFRuleTypes), ruleTypeName);
            IEnumerable<ComponentPWNodeConfigModel> targetNodes = multiSelectNodes.Where(x => x.Rules.Keys.Any(keyRuleType => keyRuleType == ruleType));
            if (targetNodes != null)
            {
                foreach (ComponentPWNodeConfigModel targetNode in targetNodes)
                {
                    List<RuleValue> targetRuleValueList = null;
                    List<RuleValue> sourceRuleValueList = RulesCommand.ReadIACConfig(originalConfigItem);
                    IACConfig targetConfigItem = ACConfigHelper.GetStoreConfiguration(configStore.ConfigurationEntries, targetNode.PreACUrl, targetNode.ContentACClassWF.ConfigACUrl + @"\Rules\" + ruleTypeName, false, null);
                        
                    if(targetConfigItem != null && (originalConfigItem as VBEntityObject).EntityState == EntityState.Deleted)
                    {
                        (targetConfigItem as VBEntityObject).DeleteACObject(db, false);
                    }
                    else
                    {
                        if (targetConfigItem == null)
                        {
                            targetConfigItem = configStore.NewACConfig();
                            targetConfigItem.LocalConfigACUrl = targetNode.ContentACClassWF.ConfigACUrl + @"\Rules\" + ruleTypeName;
                            targetConfigItem.PreConfigACUrl = targetNode.PreACUrl;
                            //configStore.ConfigurationEntries.Add(targetConfigItem);
                        }


                        if (ruleType == ACClassWFRuleTypes.Parallelization || ruleType == ACClassWFRuleTypes.Breakpoint)
                        {
                            targetRuleValueList = sourceRuleValueList;
                        }
                        else
                        {
                            List<object> sourceObjects = RulesCommand.ObjectListFromRuleValue(db.ContextIPlus, sourceRuleValueList.FirstOrDefault());
                            List<object> availableObjects = RulesCommand.GetWFNodeRuleObjects(ruleType, targetNode.ContentACClassWF).ToList();
                            sourceObjects.RemoveAll(x => !availableObjects.Select(b => (b as ACClass).ACClassID).Contains((x as ACClass).ACClassID));
                            targetRuleValueList = new List<RuleValue>() { RulesCommand.RuleValueFromObjectList(ruleType, sourceObjects) };
                        }
                        if (targetRuleValueList != null && targetRuleValueList.Any())
                            RulesCommand.WriteIACConfig(db, targetConfigItem, targetRuleValueList);
                        else
                        {
                            (targetConfigItem as VBEntityObject).DeleteACObject(db, false);
                        }
                    }
                }
            }
        }

        public void ReplicationSimpleParamForward(IACEntityObjectContext db, IACConfigStore configStore, IACConfig originalConfigItem, ACConfigParam configParam, string preACUrl, string localConfigACUrl, string acIdentifier)
        {
            IACConfig targetConfigItem = ACConfigHelper.GetStoreConfiguration(configStore.ConfigurationEntries, preACUrl, localConfigACUrl + @"\" + acIdentifier, false, null);

            if(targetConfigItem != null && (originalConfigItem as VBEntityObject).EntityState == EntityState.Deleted)
            {
                (targetConfigItem as VBEntityObject).DeleteACObject(db, false);
            }
            else
            {
                if (targetConfigItem == null)
                {
                    targetConfigItem = ConfigManagerIPlus.ACConfigFactory(configStore, configParam, preACUrl, localConfigACUrl + @"\" + acIdentifier, originalConfigItem.VBiACClassID);
                    //configStore.ConfigurationEntries.Add(targetConfigItem);
                }
                targetConfigItem.XMLConfig = originalConfigItem.XMLConfig;
            }
        }

    }
}
