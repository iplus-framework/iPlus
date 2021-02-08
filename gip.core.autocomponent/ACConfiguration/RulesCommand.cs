using System;
using System.Collections.Generic;
using System.Linq;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public static class RulesCommand
    {
        #region RuleTypeDefinition  and belong objects

        static List<RuleTypeDefinition> listOfRuleInfoPatterns;
        public static List<RuleTypeDefinition> ListOfRuleInfoPatterns
        {
            get
            {
                if (listOfRuleInfoPatterns == null)
                    listOfRuleInfoPatterns = new List<RuleTypeDefinition>()
                    {
                        // Group rules parameters
                        // Parallelization currently not in use. Parallelization should done by using more Groups in the Workflw.
                        //new RuleTypeDefinition()
                        //    {
                        //           RuleApplyedWFACKindTypes     = new List<Global.ACKinds>(){Global.ACKinds.TPWGroup, Global.ACKinds.TPWNodeWorkflow, Global.ACKinds.TPWMethod}, 
                        //           RuleType                     = ACClassWFRuleTypes.Parallelization, 
                        //           Translation                  = "en{'Parallelization'}de{'Parallelisierung'}"
                        //    },
                        new RuleTypeDefinition()
                            {
                                   RuleApplyedWFACKindTypes     = new List<Global.ACKinds>(){Global.ACKinds.TPWGroup, Global.ACKinds.TPWNodeWorkflow, Global.ACKinds.TPWMethod},
                                   RuleType                     = ACClassWFRuleTypes.Allowed_instances,
                                   Translation                  = "en{'Allowed instances'}de{'Erlaubte Instanzen'}"
                            },

                        new RuleTypeDefinition()
                            {
                                    RuleApplyedWFACKindTypes    = new List<Global.ACKinds>(){Global.ACKinds.TPWNodeMethod},
                                    RuleType                    = ACClassWFRuleTypes.Excluded_module_types,
                                    Translation                 = "en{'Excluded module types'}de{'Ausgenommene Modularten'}"
                            },
                        new RuleTypeDefinition()
                            {
                                    RuleApplyedWFACKindTypes    = new List<Global.ACKinds>(){Global.ACKinds.TPWNodeMethod},
                                    RuleType                    = ACClassWFRuleTypes.ActiveRoutes,
                                    Translation                 = "en{'Routes preference'}de{'Routen Voreinstellung'}"
                            },
                        new RuleTypeDefinition()
                            {
                                    RuleApplyedWFACKindTypes    = new List<Global.ACKinds>(){Global.ACKinds.TPWNodeMethod},
                                    RuleType                    = ACClassWFRuleTypes.Excluded_process_modules,
                                    Translation                 = "en{'Excluded process modules'}de{'Ausgenommene Prozessmodule'}"
                            },
                        new RuleTypeDefinition()
                            {
                                   RuleApplyedWFACKindTypes     = new List<Global.ACKinds>(){Global.ACKinds.TPWGroup, Global.ACKinds.TPWNodeMethod, Global.ACKinds.TPWNodeWorkflow},
                                   RuleType                     = ACClassWFRuleTypes.Breakpoint,
                                   Translation                  = "en{'Breakpoint'}de{'Haltepunkt'}"
                            }
                    };
                return listOfRuleInfoPatterns;
            }
        }

        public static IEnumerable<object> GetWFNodeRuleObjects(ACClassWFRuleTypes ruleType, ACClassWF parentWF)
        {
            switch (ruleType)
            {
                case ACClassWFRuleTypes.Parallelization:
                    return new object[] { true, false };

                case ACClassWFRuleTypes.Allowed_instances:
                    if (parentWF == null)
                        return null;
                    else
                        return GetInstances(parentWF);

                case ACClassWFRuleTypes.Excluded_module_types:
                    if (parentWF == null)
                        return null;
                    else
                        return GetExcludedModuleTypes(parentWF, parentWF.Database);

                case ACClassWFRuleTypes.ActiveRoutes:
                    return new List<object>();

                case ACClassWFRuleTypes.Excluded_process_modules:
                    if (parentWF == null)
                        return null;
                    else
                        return GetProcessModules(parentWF, parentWF.Database);

                case ACClassWFRuleTypes.Breakpoint:
                    return new object[] { true, false };

                default:
                    return null;
            }
        }

        #endregion

        #region  IACConfig <=> RuleValue

        public static void WriteIACConfig(IACEntityObjectContext db, IACConfig configItem, List<RuleValue> ruleValues)
        {
            IACEntityProperty entityProperty = configItem as IACEntityProperty;
            ACPropertyExt acPropertyExt = entityProperty.ACProperties.Properties.Where(x => x.ACIdentifier == RuleValueList.ClassName).FirstOrDefault();
            if (acPropertyExt == null)
            {
                acPropertyExt = new ACPropertyExt();
                acPropertyExt.ACIdentifier = RuleValueList.ClassName;
                acPropertyExt.ObjectType = typeof(RuleValueList);
                acPropertyExt.AttachTo(entityProperty.ACProperties);
                configItem.SetValueTypeACClass(db.ContextIPlus.GetACType(RuleValueList.ClassName));
            }
            acPropertyExt.Value = new RuleValueList() { Items = ruleValues };
            entityProperty.ACProperties.Properties.Add(acPropertyExt);
            entityProperty.ACProperties.Serialize();
        }

        public static List<RuleValue> ReadIACConfig(IACConfig configItem)
        {
            IACEntityProperty entityProperty = configItem as IACEntityProperty;
            ACPropertyExt acPropertyExt = entityProperty.ACProperties.Properties.Where(x => x.ACIdentifier == RuleValueList.ClassName).FirstOrDefault();
            return (acPropertyExt.Value as RuleValueList).Items;
        }

        #endregion

        #region RuleValue <=> IEnumerable<object>

        public static RuleValue RuleValueFromObjectList(ACClassWFRuleTypes ruleType, IEnumerable<object> values)
        {
            RuleValue ruleValue = new RuleValue();
            ruleValue.RuleType = ruleType;
            ruleValue.ACClassACUrl = new List<string>();
            ruleValue.RuleObjectValue = new List<object>();
            foreach (object item in values)
            {
                gip.core.datamodel.ACClass acClassItem = item as gip.core.datamodel.ACClass;
                if (acClassItem != null)
                {
                    ruleValue.ACClassACUrl.Add(acClassItem.GetACUrl());
                    ruleValue.RuleObjectValue = null;
                }
                else if (item is Route)
                {
                    ruleValue.RuleObjectValue.Add(item);
                }
                else if (item != null)
                {
                    var acClassURL = @"ValueType\" + item.GetType().FullName;
                    ruleValue.ACClassACUrl.Add(acClassURL);
                    ruleValue.RuleObjectValue.Add(item);
                }
            }
            return ruleValue;
        }

        public static List<object> ObjectListFromRuleValue(Database db, RuleValue ruleValue)
        {
            List<object> objectList = new List<object>();
            foreach (string classUrl in ruleValue.ACClassACUrl)
            {
                if (classUrl.Contains(ACClass.ClassName))
                {
                    string tmpClassUrl = classUrl;
                    if (tmpClassUrl.StartsWith(Const.ContextDatabase + "\\"))
                        tmpClassUrl = tmpClassUrl.Replace(Const.ContextDatabase + "\\", "");
                    ACClass acClass = db.ACUrlCommand(tmpClassUrl) as ACClass;
                    objectList.Add(acClass);
                }
            }

            if (ruleValue.RuleObjectValue != null)
                foreach (var item in ruleValue.RuleObjectValue)
                {
                    objectList.Add(item);
                }

            return objectList;
        }

        public static List<object> ObjectListFromRuleValue(Database db, List<RuleValue> ruleValueList)
        {
            List<object> selectedValues = new List<object>();
            if (ruleValueList != null)
            {
                foreach (RuleValue ruleValue in ruleValueList)
                {
                    List<object> tmpList = ObjectListFromRuleValue(db, ruleValue);
                    if (tmpList != null)
                        selectedValues.AddRange(tmpList);
                }
            }
            return selectedValues;
        }

        public static IEnumerable<ACClass> GetProcessModulesRouting(ACClassWF item, Database db, IEnumerable<ACClass> allowedInstances, out RouteDirections direction)
        {
            bool? isRecival = null;
            var temp = GetProcessModulesInternal(item, db, out isRecival, allowedInstances);
            direction = isRecival != null && isRecival.Value ? RouteDirections.Backwards : RouteDirections.Forwards;
            return temp;
        }

        #endregion

        public static bool IsRuleValueListSame(List<RuleValue> oldRuleValueList, List<RuleValue> newRuleValueList)
        {
            bool isSame = true;
            if (newRuleValueList == null)
                newRuleValueList = new List<RuleValue>();
            if (oldRuleValueList == null)
                oldRuleValueList = new List<RuleValue>();

            // test count matching
            isSame = isSame && newRuleValueList.Count == oldRuleValueList.Count;

            // test rule type definition matching
            if (isSame)
            {
                List<string> oldRuleTypes = oldRuleValueList.Select(x => x.RuleType.ToString()).OrderBy(x => x).ToList();
                List<string> newRuleTypes = newRuleValueList.Select(x => x.RuleType.ToString()).OrderBy(x => x).ToList();
                isSame = isSame && oldRuleTypes.SequenceEqual(newRuleTypes);
            }

            // test selected acclass equality by rule type
            if (isSame)
            {
                List<string> oldRuleTypes = oldRuleValueList.Select(x => x.RuleType.ToString()).ToList();
                foreach (var ruleTypeName in oldRuleTypes)
                {
                    List<string> oldACClasses = oldRuleValueList.Where(x => x.RuleType.ToString() == ruleTypeName).SelectMany(x => x.ACClassACUrl).OrderBy(x => x).ToList();
                    List<string> newACClasses = newRuleValueList.Where(x => x.RuleType.ToString() == ruleTypeName).SelectMany(x => x.ACClassACUrl).OrderBy(x => x).ToList();
                    isSame = isSame && oldACClasses.SequenceEqual(newACClasses);
                    if (!isSame)
                        break;
                }
            }

            if (isSame)
            {
                List<string> oldRuleTypes = oldRuleValueList.Select(x => x.RuleType.ToString()).ToList();
                foreach (var ruleTypeName in oldRuleTypes)
                {
                    List<string> oldACClasses = oldRuleValueList.Where(x => x.RuleType.ToString() == ruleTypeName).SelectMany(x => x.ACClassACUrl).OrderBy(x => x).ToList();
                    List<string> newACClasses = newRuleValueList.Where(x => x.RuleType.ToString() == ruleTypeName).SelectMany(x => x.ACClassACUrl).OrderBy(x => x).ToList();
                    isSame = isSame && oldACClasses.SequenceEqual(newACClasses);
                    if (!isSame)
                        break;
                }
            }

            if (isSame)
            {
                List<string> oldRuleTypes = oldRuleValueList.Select(x => x.RuleType.ToString()).ToList();
                foreach (var ruleTypeName in oldRuleTypes)
                {
                    List<object> oldACClasses = oldRuleValueList.Where(x => x.RuleType.ToString() == ruleTypeName && x.RuleObjectValue != null).SelectMany(x => x.RuleObjectValue).ToList();
                    List<object> newACClasses = newRuleValueList.Where(x => x.RuleType.ToString() == ruleTypeName && x.RuleObjectValue != null).SelectMany(x => x.RuleObjectValue).ToList();
                    isSame = isSame && oldACClasses.SequenceEqual(newACClasses);
                    if (!isSame)
                        break;
                }
            }

            return isSame;
        }

        #region private members

        private static IEnumerable<ACClass> GetInstances(ACClassWF item)
        {
            return item.RefPAACClass.DerivedClassesInProjects;
        }

        public static IEnumerable<ACClass> GetExcludedModuleTypes(ACClassWF item, Database db)
        {
            if (item.RefPAACClassMethod == null)
            {
                return null;
            }
            else
            {
                ACClass baseClass;
                List<Route> routes = new List<Route>();
                List<ACClass> result = new List<ACClass>();

                Type materialTransportType = null;
                if (item.PWACClass != null)
                    materialTransportType = item.PWACClass.ObjectType;

                // Dosing type
                if (materialTransportType != null && typeof(IPWNodeReceiveMaterial).IsAssignableFrom(materialTransportType))
                {
                    foreach (ACClass instance in item.ParentACClass.DerivedClassesInProjects)
                    {
                        ACClass instanceFunc = instance.Childs.FirstOrDefault(c => item.RefPAACClassMethod.AttachedFromACClass != null &&
                                                                                   c.IsDerivedClassFrom(item.RefPAACClassMethod.AttachedFromACClass));
                        ACClassProperty paPointMatIn1 = instanceFunc?.GetProperty(Const.PAPointMatIn1);

                        if (paPointMatIn1 != null)
                        {
                            var sourcePoint = ACRoutingService.DbSelectRoutesFromPoint(db, instanceFunc, paPointMatIn1,
                                                                                        (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule, null, RouteDirections.Backwards, true);

                            RouteItem routeSource = sourcePoint?.FirstOrDefault()?.GetRouteSource();

                            if (routeSource != null)
                            {
                                RoutingResult routeResult = ACRoutingService.FindSuccessorsFromPoint(null, db, true,
                                                                                            instance, routeSource.SourceProperty, PAProcessModule.SelRuleID_ProcessModule, RouteDirections.Backwards, new object[] { },
                                                                                            (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule,
                                                                                            null,
                                                                                            0, true, true, false, false, 3);
                                if (routeResult.Routes != null && routeResult.Routes.Any())
                                    routes.AddRange(routeResult.Routes);
                            }
                        }
                        else
                        {
                            RoutingResult routeResult = ACRoutingService.FindSuccessors(null, db, true,
                                                                                            instance, PAProcessModule.SelRuleID_ProcessModule, RouteDirections.Backwards, new object[] { },
                                                                                            (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule,
                                                                                            null,
                                                                                            0, true, true, false, false, 3);
                            if (routeResult.Routes != null && routeResult.Routes.Any())
                                routes.AddRange(routeResult.Routes);
                        }
                    }
                }
                // Discharging type
                else if (materialTransportType != null && typeof(IPWNodeDeliverMaterial).IsAssignableFrom(materialTransportType))
                {
                    foreach (ACClass instance in item.ParentACClass.DerivedClassesInProjects)
                    {
                        RoutingResult routeResult = ACRoutingService.FindSuccessors(null, db, true,
                                                            instance, PAProcessModule.SelRuleID_ProcessModule, RouteDirections.Forwards, new object[] { },
                                                            (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule,
                                                            null,
                                                            0, true, true, false, false, 3);
                        if (routeResult.Routes != null && routeResult.Routes.Any())
                            routes.AddRange(routeResult.Routes);
                    }
                }

                // If route count is 1, it means that route is a connection between item and ProcessModule and there are no any modules in between
                foreach (Route rItem in routes.Where(r => r.Count > 1))
                {
                    // Last RouteItem is ignored (i < rItem.Count-1) because its Target property points to ProcessModule
                    for (int i = 0; i < rItem.Count - 1; i++)
                    {
                        baseClass = rItem[i].Target.ACClass1_BasedOnACClass;
                        if (baseClass.ACProject.ACProjectType == Global.ACProjectTypes.AppDefinition && !result.Contains(baseClass)) result.Add(baseClass);
                    }
                }

                return result.OrderBy(x => x.ACCaption);
            }
        }

        public static IEnumerable<ACClass> GetProcessModules(ACClassWF item, Database db)
        {
            bool? help = null;
            return GetProcessModulesInternal(item, db, out help);
        }

        private static IEnumerable<ACClass> GetProcessModulesInternal(ACClassWF item, Database db, out bool? isRecivalMaterial, IEnumerable<ACClass> allowedInstances = null)
        {
            isRecivalMaterial = null;
            if (item.RefPAACClassMethod == null)
            {
                return null;
            }
            else
            {
                ACClass moduleItem;
                List<Route> routes = new List<Route>();
                List<ACClass> result = new List<ACClass>();
                IEnumerable<ACClass> myInstances = item.ParentACClass.DerivedClassesInProjects;

                Type materialTransportType = null;
                if (item.PWACClass != null)
                    materialTransportType = item.PWACClass.ObjectType;

                if (allowedInstances != null && allowedInstances.Any())
                    myInstances = myInstances.Intersect(allowedInstances);

                // Dosing type
                if (materialTransportType != null && typeof(IPWNodeReceiveMaterial).IsAssignableFrom(materialTransportType))
                {
                    foreach (ACClass instance in myInstances)
                    {
                        ACClass instanceFunc = instance.Childs.FirstOrDefault(c => item.RefPAACClassMethod.AttachedFromACClass != null && 
                                                                                   c.IsDerivedClassFrom(item.RefPAACClassMethod.AttachedFromACClass));
                        ACClassProperty paPointMatIn1 = instanceFunc?.GetProperty(Const.PAPointMatIn1);

                        if (paPointMatIn1 != null)
                        {
                            var sourcePoint = ACRoutingService.DbSelectRoutesFromPoint(db, instanceFunc, paPointMatIn1,
                                                                                        (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule, null, RouteDirections.Backwards, true);

                            RouteItem routeSource = sourcePoint?.FirstOrDefault()?.GetRouteSource();

                            if (routeSource != null)
                            {
                                RoutingResult routeResult = ACRoutingService.FindSuccessorsFromPoint(null, db, true,
                                                                                            instance, routeSource.SourceProperty, PAProcessModule.SelRuleID_ProcessModule, RouteDirections.Backwards, new object[] { },
                                                                                            (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule,
                                                                                            null,
                                                                                            0, true, true, false, false, 3);
                                if (routeResult.Routes != null && routeResult.Routes.Any())
                                    routes.AddRange(routeResult.Routes);
                            }
                        }
                        else
                        {
                            RoutingResult routeResult = ACRoutingService.FindSuccessors(null, db, true,
                                                                                            instance, PAProcessModule.SelRuleID_ProcessModule, RouteDirections.Backwards, new object[] { },
                                                                                            (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule,
                                                                                            null,
                                                                                            0, true, true, false, false, 3);
                            if (routeResult.Routes != null && routeResult.Routes.Any())
                                routes.AddRange(routeResult.Routes);
                        }
                    }

                    for (int i = 0; i < routes.Count; i++)
                    {
                        moduleItem = routes[i][0].Source;
                        if (!result.Contains(moduleItem)) result.Add(moduleItem);
                    }
                    isRecivalMaterial = true;
                }
                // Discharging type
                else if (materialTransportType != null && typeof(IPWNodeDeliverMaterial).IsAssignableFrom(materialTransportType))
                {
                    foreach (ACClass instance in myInstances)
                    {
                        RoutingResult routeResult = ACRoutingService.FindSuccessors(null, db, true,
                                                            instance, PAProcessModule.SelRuleID_ProcessModule, RouteDirections.Forwards, new object[] { },
                                                            (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule,
                                                            null,
                                                            0, true, true, false, false, 3);
                        if (routeResult.Routes != null && routeResult.Routes.Any())
                            routes.AddRange(routeResult.Routes);
                    }

                    for (int i = 0; i < routes.Count; i++)
                    {
                        moduleItem = routes[i].Last().Target;
                        if (!result.Contains(moduleItem)) result.Add(moduleItem);
                    }
                    isRecivalMaterial = false;
                }

                return result.OrderBy(x => x.ACCaption);
            }
        }

        #endregion

    }
}
