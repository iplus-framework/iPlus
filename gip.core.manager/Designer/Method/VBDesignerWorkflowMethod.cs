// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Data;

namespace gip.core.manager
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Process-Workflow Desginer'}de{'Prozess-Workflow Desginer'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public partial class VBDesignerWorkflowMethod : VBDesignerWorkflow
    {
        #region c´tors
        /// <summary>
        /// Konstruktor beim ACClassMethod verwenden
        /// </summary>
        public VBDesignerWorkflowMethod(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            ShowXMLEditor = true;
            IsToolSelection = false;
            if (!base.ACInit(startChildMode))
                return false;

            InitDesignManager(Const.VBPresenter_SelectedRootWFNode);
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (CurrentPWRootLive != null)
            {
                CurrentPWRootLive.Detach();
                CurrentPWRootLive = null;
            }
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion


        #region Properties
        ACProgram _CurrentProgramForConfig;
        /// <summary>
        /// 
        /// </summary>
        public ACProgram CurrentProcessConfig
        {
            get
            {
                return _CurrentProgramForConfig;
            }
        }

        ACClassMethod _CurrentACClassMethod;
        /// <summary>
        /// Methode dessen Workflow durch die untergeordneten "PWNodeMethod" repräsentiert wird
        /// </summary>
        public ACClassMethod CurrentACClassMethod
        {
            get
            {
                return _CurrentACClassMethod;
            }
        }

        /// <summary>
        /// ACComponent für aktuell ausgeführten Workflow
        /// </summary>
        public ACRef<ACComponent> CurrentPWRootLive
        {
            get;
            set;
        }

        /// <summary>
        /// XAML-Code for Presentation
        /// </summary>
        /// <value>
        /// XAML-Code for Presentation
        /// </value>
        public override string XAMLDesign
        {
            get
            {
                return CurrentACClassMethod.XMLDesign;
            }
            set
            {
                CurrentACClassMethod.XMLDesign = value;
            }
        }

        private bool _UseAutoLayoutElements = true;
        [ACPropertyInfo(999, "", "en{'Auto layout elements'}de{'Auto Layout-Elemente'}")]
        public bool UseAutoLayoutElements
        {
            get => _UseAutoLayoutElements;
            set
            {
                _UseAutoLayoutElements = value;
                OnPropertyChanged("UseAutoLayoutElements");
            }
        }

        protected IVBComponentDesignManagerWorkflowMethod WPFProxyWorkflowMethod
        {
            get
            {
                return WPFProxy as IVBComponentDesignManagerWorkflowMethod;
            }
        }

        #endregion


        #region Methods

        #region Loading
        public bool Load(ACClassMethod programACClassMethod, ACComponent pwRootLive, ACProgram programForConfig, PWOfflineNode parentPWObjectNode)
        {
            _CurrentACClassMethod = programACClassMethod;
            // Wird benötigt, damit die Konfiguration beginnend bein ACProgram verwendet wird
            _CurrentProgramForConfig = programForConfig;

            ParentPWNode = parentPWObjectNode;
            if (pwRootLive != null)
            {
                CurrentPWRootLive = new ACRef<ACComponent>(pwRootLive, this);
            }

            InitChilds(Global.ACStartTypes.Automatic);

            return true;
        }

        protected void InitChilds(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (CurrentPWRootLive != null)
                return;

            var database = ((ACClass)ACType).GetObjectContext<Database>();
            var acClassPWObjectMethod = database.ACClass.Where(c => c.ACIdentifier == "PWOfflineNodeMethod").First();
            if (CurrentACClassMethod.RootWFNode != null)
            {
                var pwRoot = ACActivator.CreateInstance(acClassPWObjectMethod, CurrentACClassMethod.RootWFNode, this, null, startChildMode) as PWOfflineNode;
                if (pwRoot is PWOfflineNode)
                {
                    PWRootNode = pwRoot;
                }
            }
        }

        public void RefreshModifiedChilds()
        {
            if (CurrentPWRootLive != null)
                return;
            InitChilds();
        }
        #endregion

        #region Override
        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public override object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            if (acUrl == "PWRootNode")
            {
                return PWRootNode;
            }
            if (acUrl.StartsWith("PWRootNode"))
            {
                ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
                return PWRootNode.ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
            }
            if (acUrl.StartsWith("ACClassWFEdge("))
            {
                return CurrentACClassMethod.ACUrlCommand(acUrl, acParameter);
            }
            if (CurrentPWRootLive != null && CurrentPWRootLive.IsObjLoaded)
            {
                ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
                if (acUrlHelper.UrlKey == ACUrlHelper.UrlKeys.Child)
                {
                    if (String.IsNullOrEmpty(acUrlHelper.NextACUrl))
                        return CurrentPWRootLive.ValueT;
                    else
                        return CurrentPWRootLive.ValueT.ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                }
            }
            return base.ACUrlCommand(acUrl, acParameter);
        }
        #endregion

        #region Manager->Hilfsmethoden
        public override void UpdateAvailableElements()
        {
            AvailableElementList.Clear();
            // Design muss eine Methode sein
            if (!(CurrentDesign is ACClassMethod))
                return;
            ACClassMethod acClassMethod = CurrentDesign as ACClassMethod;
            // Nur für Workflows gibt es einen Tree
            if (acClassMethod == null || acClassMethod.ACKind != Global.ACKinds.MSWorkflow)
                return;

            // Root-WF-Klasse (PWMethod oder PWMethodProduction, etc.
            // Bestimmt welche Einträge für den Workflow zum einfügen zur Verfügung gestellt werden
            ACClass pwMethodACClass = null;
            if (acClassMethod.RootWFNode != null)
                pwMethodACClass = (acClassMethod.RootWFNode as IACWorkflowNode).PWACClass;

            ACClass acClass = acClassMethod.ACClass;

            if (acClass == null)
                return;

            // 1. Klassen und Methoden entsprechend des Anwendungs-Trees einfügen
            if (acClass.ACClass1_ParentACClass == null)
            {
                ACObjectItem rootClasses = InsertACModelClass(null, acClass, null, pwMethodACClass);
                _AvailableElementList.Add(rootClasses);
            }
            else
            {
                ACObjectItem rootClasses = InsertACModelClass(null, acClass.ACClass1_ParentACClass, acClass, pwMethodACClass);
                _AvailableElementList.Add(rootClasses);
            }

            // 2. Vorhandene Workflow-Methoden aus gleichem ACProject (Anwendung und Anwendungsdefinition) einfügen
            ACObjectItem rootWorkflow = InsertWorkflows(acClass, acClassMethod);
            if (rootWorkflow != null)
            {
                _AvailableElementList.Add(rootWorkflow);
            }

            // 3. Vorhandene Workflow-Methoden aus anderen (nicht globalen) ACProject (Anwendungsdefinition) einfügen
            InsertOtherWorkflow(acClass.ACProject);

            // 4. Workflows aus globalen Projekten einfügen
            foreach (var acProjectGlobal in Database.ContextIPlus.ACProject.Where(c => c.IsGlobal && c.ACProjectID != acClass.ACProject.ACProjectID))
            {
                ACObjectItem rootGlobalWorkflow = InsertWorkflowClass(null, acProjectGlobal.RootClass);
                if (rootGlobalWorkflow != null)
                    _AvailableElementList.Add(rootGlobalWorkflow);
            }

            // 5. Statische Workflowklassen einfügen (Und-/Oder-Knoten etc.)
            ACObjectItem treeEntry = InsertStaticClasses();
            if (treeEntry != null)
            {
                _AvailableElementList.Add(treeEntry);
            }
        }

        private ACObjectItem InsertWorkflowClass(ACObjectItem parentEntry, ACClass paACClass)
        {
            ACObjectItem treeEntry = null;

            var acClassMethodList = paACClass.GetWorkflowStartMethod(true);

            if ((acClassMethodList == null || !acClassMethodList.Any()) && !paACClass.AllChildsInHierarchy.Any())
                return null;

            NodeInfo nodeInfo = new NodeInfo(paACClass.RelatedWorkflowClass);
            treeEntry = new ACObjectItem(nodeInfo, paACClass.ACCaption);
            if (parentEntry != null)
            {
                parentEntry.Add(treeEntry);
            }

            if (acClassMethodList != null && acClassMethodList.Any())
            {
                foreach (var acClassMethod in acClassMethodList)
                {
                    NodeInfo nodeInfo2 = new NodeInfo(acClassMethod.PWACClass);
                    ACObjectItem treeEntry2 = new ACObjectItem(nodeInfo2, acClassMethod.ACCaption);
                    treeEntry.Add(treeEntry2);
                }
            }
            foreach (var acClassChild in paACClass.AllChildsInHierarchy)
            {
                InsertWorkflowClass(treeEntry, acClassChild);
            }
            return treeEntry;
        }

        private ACObjectItem InsertACModelClass(ACObjectItem parentEntry, ACClass paACClass, ACClass ingnoreACClass, ACClass pwMethodACClass)
        {
            var acClassMethodList = paACClass.GetWorkflowStartMethod(false).ToList();

            foreach (var acClassMethod in acClassMethodList.ToList())
            {
                if (!acClassMethod.PWACClass.ACClass1_PWMethodACClass.IsDerivedClassFrom(pwMethodACClass)
                    && !typeof(PWProcessFunction).IsAssignableFrom(acClassMethod.PWACClass.ACClass1_PWMethodACClass.ObjectType))
                {
                    acClassMethodList.Remove(acClassMethod);
                }
            }

            if (   parentEntry != null
                && paACClass.ACKind != Global.ACKinds.TPAProcessModuleGroup
                && (acClassMethodList == null || !acClassMethodList.Any()))
            {
                return null;
            }
            // Zuerst paClass einfügen
            ACObjectItem treeEntry = null;
            NodeInfo nodeInfo;
            if (parentEntry != null && parentEntry.ACObject is NodeInfo)
            {
                nodeInfo = new NodeInfo(paACClass.RelatedWorkflowClass != null ? paACClass.RelatedWorkflowClass : paACClass, paACClass, ((NodeInfo)parentEntry.ACObject).PAACClass);
            }
            else
            {
                nodeInfo = new NodeInfo(paACClass.RelatedWorkflowClass != null ? paACClass.RelatedWorkflowClass : paACClass, paACClass, null);
            }
            treeEntry = new ACObjectItem(nodeInfo, paACClass.ACIdentifier);
            if (parentEntry != null)
            {
                parentEntry.Add(treeEntry);
            }



            // Alle Workflow-Methoden (PWNode, etc.) der paACClass ermitteln

            if (acClassMethodList != null && acClassMethodList.Any())
            {
                //if (ingnoreACClass != null && ingnoreACClass == paACClass)
                //    return null;
                foreach (var acClassMethod in acClassMethodList)
                {
                    NodeInfo nodeInfoMethod;
                    if (parentEntry != null && parentEntry.ACObject is NodeInfo)
                    {
                        nodeInfoMethod = new NodeInfo(acClassMethod.PWACClass, paACClass, ((NodeInfo)parentEntry.ACObject).PAACClass, acClassMethod);
                    }
                    else
                    {
                        nodeInfoMethod = new NodeInfo(acClassMethod.PWACClass, paACClass, null, acClassMethod);
                    }
                    ACObjectItem treeEntryMethod = new ACObjectItem(nodeInfoMethod, acClassMethod.ACCaptionAttached);
                    if (treeEntry != null)
                    {
                        treeEntry.Add(treeEntryMethod);
                    }
                }
            }
            foreach (var acClassChild in paACClass.ACClass_ParentACClass.Where(c => c.ACKindIndex != (Int16)Global.ACKinds.TPAProcessFunction).OrderBy(c => c.ACKindIndex).ThenBy(c => c.ACIdentifier))
            {
                if (   (acClassChild.ACKind == Global.ACKinds.TPAProcessModuleGroup)
                    || (acClassChild.ACKind == Global.ACKinds.TPAProcessModule && acClassChild.AllChildsInHierarchy.Where(c => c.ACKind == Global.ACKinds.TPAProcessFunction).Any()))
                    InsertACModelClass(treeEntry, acClassChild, ingnoreACClass, pwMethodACClass);
            }
            return treeEntry;
        }

        private ACObjectItem InsertWorkflows(ACClass acClass, ACClassMethod acClassMethod)
        {
            List<ACProject> acProjectList = new List<ACProject>();
            // Project selbst
            acProjectList.Add(acClassMethod.ACClass.ACProject);

            // Falls Anwendung, dann auch von Anwendungsdefinition
            if ((acClassMethod.ACClass.ACProject.ACProjectType == Global.ACProjectTypes.Application
                    || acClassMethod.ACClass.ACProject.ACProjectType == Global.ACProjectTypes.Service)
                && acClassMethod.ACClass.ACProject.ACProject1_BasedOnACProject != null)
            {
                acProjectList.Add(acClassMethod.ACClass.ACProject.ACProject1_BasedOnACProject);
            }

            ACObjectItem root = null;
            foreach (var acProject in acProjectList)
            {
                var query = acProject.RootClass.ACClassMethod_ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.MSWorkflow && c.PWACClass != null && acClassMethod.ACClassMethodID != c.ACClassMethodID && c.IsSubMethod).OrderBy(c => c.ACCaption);
                if (query.Any())
                {
                    if (root == null)
                    {
                        root = new ACObjectItem("Workflows");
                    }
                    foreach (ACClassMethod acClassMethodWorkflow in query)
                    {
                        NodeInfo nodeInfo = new NodeInfo(acClassMethodWorkflow.PWACClass, acProject.RootClass, null, acClassMethodWorkflow);

                        ACObjectItem treeEntry = new ACObjectItem(nodeInfo, acClassMethodWorkflow.ACCaption);
                        root.Add(treeEntry);
                    }
                }
            }
            return root;
        }

        /// <summary>
        /// Einfügen aller Sub-Workflows von anderen Anwendungsdefinitionen
        /// </summary>
        /// <param name="acProject"></param>
        /// <returns></returns>
        private void InsertOtherWorkflow(ACProject acProject)
        {
            var queryOtherProjects = Database.ContextIPlus.ACProject.Where(c => !c.IsGlobal && c.ACProjectID != acProject.ACProjectID && c.ACProjectTypeIndex == (Int16)Global.ACProjectTypes.AppDefinition).OrderBy(c => c.ACProjectName);

            foreach (var acProjectOther in queryOtherProjects)
            {
                var query = acProjectOther.RootClass.ACClassMethod_ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.MSWorkflow && c.PWACClass != null && c.IsSubMethod).OrderBy(c => c.ACCaption);

                if (query.Any())
                {
                    ACObjectItem root = new ACObjectItem(acProjectOther.ACProjectName);
                    foreach (ACClassMethod acClassMethodWorkflow in query)
                    {
                        NodeInfo nodeInfo = new NodeInfo(acClassMethodWorkflow.PWACClass, acProjectOther.RootClass, null, acClassMethodWorkflow);

                        ACObjectItem treeEntry = new ACObjectItem(nodeInfo, acClassMethodWorkflow.ACCaption);
                        root.Add(treeEntry);
                    }
                    _AvailableElementList.Add(root);
                }
            }
        }

        private ACObjectItem InsertStaticClasses()
        {
            var query = Database.ContextIPlus.ACClass.Where(c =>   c.ACProject.ACProjectTypeIndex == (Int16)Global.ACProjectTypes.ClassLibrary 
                                                                && c.ACKindIndex == (Int16)Global.ACKinds.TPWNodeStatic)
                                                     .OrderBy(c => c.ACIdentifier);

            if (!query.Any())
                return null;
            ACObjectItem treeEntryRoot = new ACObjectItem("Functions");

            foreach (var acClass in query)
            {
                NodeInfo nodeInfo = new NodeInfo(acClass, null, null);
                ACObjectItem treeEntry = new ACObjectItem(nodeInfo, ">" + acClass.ACCaption);
                treeEntryRoot.Add(treeEntry);
            }

            return treeEntryRoot;
        }
        #endregion

        #region IWorkflowHandler Members

        public override bool IsEnabledDoModifyAction(IACWorkflowDesignContext visualMain, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            NodeInfo nodeInfo = dropObject.GetACValue(typeof(NodeInfo)) as NodeInfo;
            if (nodeInfo == null)
                return false;                                           //           1                  2                   3                           4                       5
            ACClass pwACClass = nodeInfo.PWACClass;                     // Beispiel: PWFlowMeasure      PWGroup,            PWNodeProcessMethod         PWNodeProcessWorkflow   PWNodeAnd
            ACClass paACClass = nodeInfo.PAACClass;                     // Beispiel: DF                 DF                                              DF                      null
            ACClassMethod paACClassMethod = nodeInfo.PAACClassMethod;   // Beispiel: Flowmeasure        NULL                SubMethod1 (Sub-Workflow)   TestDF (SubWorkflow)    null
            ACClass parentPAACClass = nodeInfo.ParentPAACClass;         // Beispiel: Produktionslinie   Produktionslinie                                Produktionslinie        null

            // 1, 3, 4, 5
            if (pwACClass != null && ((paACClass != null && paACClassMethod != null) || (paACClass == null && paACClassMethod == null)))
            {
                return IsEnabledInsertACClassMethod(visualMain as ACClassMethod, pwACClass, paACClass, paACClassMethod, parentPAACClass, targetVBDataObject.GetACValue(typeof(IACWorkflowObject)) as IACWorkflowObject, x, y);
            }
            // 2
            if (pwACClass != null && paACClass != null)
            {
                return IsEnabledInsertACClass(visualMain as ACClassMethod, pwACClass, paACClass, parentPAACClass, targetVBDataObject.GetACValue(typeof(IACWorkflowObject)) as IACWorkflowObject, x, y);
            }

            return false;
        }

        public override IACWorkflowNode DoModifyAction(IACWorkflowDesignContext visualMain, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            // TODO: VisualChangeList
            WPFProxyWorkflowMethod.ClearVisualChangeList();
            // Erst mal prüfen, ob erlaubt (Sicher ist sicher)
            if (!IsEnabledModifyAction(visualMain, dropObject, targetVBDataObject, x, y))
                return null;
            NodeInfo nodeInfo = dropObject.GetACValue(typeof(NodeInfo)) as NodeInfo;
            if (nodeInfo == null)
                return null;
            ACClass pwACClass = nodeInfo.PWACClass;
            ACClass paACClass = nodeInfo.PAACClass;
            ACClassMethod paACClassMethod = nodeInfo.PAACClassMethod;
            ACClass parentPAACClass = nodeInfo.ParentPAACClass;

            IACWorkflowNode result = null;
            if (pwACClass != null && ((paACClass != null && paACClassMethod != null) || (paACClass == null && paACClassMethod == null)))
            {
                result = InsertACClassMethod(visualMain as ACClassMethod, pwACClass, paACClass, paACClassMethod, parentPAACClass, targetVBDataObject.GetACValue(typeof(IACWorkflowObject)) as IACWorkflowObject, x, y);
            }

            if (pwACClass != null && paACClass != null)
            {
                result = InsertACClass(visualMain as ACClassMethod, pwACClass, paACClass, parentPAACClass, targetVBDataObject.GetACValue(typeof(IACWorkflowObject)) as IACWorkflowObject, x, y);
            }

            //RefreshModifiedChilds();
            UpdateVisual();

            return result;
        }

        /// <summary>
        /// Beim Einfügen des Root steht noch kein DesignItemtree zur verfügung. 
        /// Daher wird das XAML manuell erstellt.
        /// </summary>
        /// <param name="vbWorkflow"></param>
        /// <param name="methodACClass"></param>
        /// <returns></returns>
        public override bool DoInsertRoot(IACWorkflowDesignContext vbWorkflow, ACClass methodACClass)
        {
            return WPFProxyWorkflowMethod.DoInsertRoot(vbWorkflow, methodACClass);
        }
        #endregion

        #region Element Action
        /// <summary>
        ///  Fügt ein komplettes ACClassMethod in WorkOrder ein. 
        ///  Diese Funktionalität ist nur für das WorkOrder möglich
        /// </summary>
        ACClassWF InsertACClassMethod(ACClassMethod rootACClassMethod, ACClass pwACClass, ACClass paACClass, ACClassMethod paACClassMethod, ACClass parentACClass, IACWorkflowObject visualTarget, double x, double y)
        {
            // 1. Steuerrezept auf Steuerschritt einfügen
            if (visualTarget is ACClassWF)
            {
                ACClassWF visualTargetClass = visualTarget as ACClassWF;
                ACClassWF methodWF = CreateNewMethodWF(rootACClassMethod, Database.ContextIPlus, pwACClass, paACClass, paACClassMethod, visualTargetClass.WFGroup);
                InsertMethodWFIntoMethod(methodWF, visualTarget, rootACClassMethod);
                return methodWF;
            }
            // 2. Steuerrezept auf Kante einfügen
            else if (visualTarget is IACWorkflowEdge)
            {
                IACWorkflowEdge visualTargetEdge = visualTarget as IACWorkflowEdge;

                IACWorkflowNode topClass = visualTargetEdge.FromWFNode;
                IACWorkflowNode bottomClass = visualTargetEdge.ToWFNode;

                ACClassWF methodWF = CreateNewMethodWF(rootACClassMethod, Database.ContextIPlus, pwACClass, paACClass, paACClassMethod, visualTargetEdge.WFGroup);
                //ACClassWF methodWF = CreateNewMethodWF(rootACClassMethod, Database.ContextIPlus, pwACClass, paACClass, paACClassMethod, topClass.VisualGroup);
                InsertMethodWFIntoMethod(methodWF, visualTarget, rootACClassMethod);
                return methodWF;
            }
            return null;
        }

        ACClassWF InsertACClass(ACClassMethod rootACClassMethod, ACClass pwACClass, ACClass paACClass, ACClass parentACClass, IACWorkflowObject visualTarget, double x, double y)
        {
            if (!IsEnabledInsertACClass(rootACClassMethod, pwACClass, paACClass, parentACClass, visualTarget, x, y))
                return null;

            // 1. Steuerschritt auf Steuerschritt einfügen
            if (visualTarget is ACClassWF)
            {
                ACClassWF visualTargetClass = visualTarget as ACClassWF;
                ACClassWF methodWF = CreateGroupClass(rootACClassMethod, pwACClass, paACClass, parentACClass, visualTargetClass.WFGroup);
                InsertMethodWFIntoMethod(methodWF, visualTarget, rootACClassMethod);
                return methodWF;
            }
            // 2. Steuerschritt auf Kante einfügen
            else if (visualTarget is IACWorkflowEdge)
            {
                IACWorkflowEdge visualTargetEdge = visualTarget as IACWorkflowEdge;

                IACWorkflowNode topClass = visualTargetEdge.FromWFNode;
                IACWorkflowNode bottomClass = visualTargetEdge.ToWFNode;

                ACClassWF methodWF = CreateGroupClass(rootACClassMethod, pwACClass, paACClass, parentACClass, topClass.WFGroup);
                InsertMethodWFIntoMethod(methodWF, visualTarget, rootACClassMethod);
                return methodWF;
            }
            return null;
        }

        public ACClassWF CreateGroupClass(ACClassMethod rootACClassMethod, ACClass pwACClass, ACClass paACClass, ACClass parentACClass, IACWorkflowNode groupVisualClass)
        {
            ACClassWF groupClass = CreateNewMethodWF(rootACClassMethod, Database.ContextIPlus, pwACClass, paACClass, null, groupVisualClass);

            // Startknoten
            var acClassNodeStart = Database.ContextIPlus.ACClass.Where(c => c.ACIdentifier == PWNodeStart.PWClassName).FirstOrDefault();
            IACWorkflowNode groupClassStart = CreateNewMethodWF(rootACClassMethod, Database.ContextIPlus, acClassNodeStart, null, null, groupClass);

            // Endknoten
            var acClassNodeEnd = Database.ContextIPlus.ACClass.Where(c => c.ACIdentifier == PWNodeEnd.PWClassName).FirstOrDefault();
            IACWorkflowNode groupClassEnd = CreateNewMethodWF(rootACClassMethod, Database.ContextIPlus, acClassNodeEnd, null, null, groupClass);

            CreateWFEdge(rootACClassMethod, groupClassStart, groupClassStart.GetConnector(Const.PWPointOut), groupClassEnd, groupClassEnd.GetConnector(Const.PWPointIn), Global.ConnectionTypes.StartTrigger);

            //_WFLayoutManager.WFUpdateGroupSize(groupClass);

            return groupClass;
        }

        protected void InsertMethodWFIntoMethod(ACClassWF methodWF, IACWorkflowObject visualTarget, ACClassMethod rootACClassMethod)
        {
            if (visualTarget is ACClassWF)
            {
                ACClassWF visualTargetClass = visualTarget as ACClassWF;
                foreach (var edgeTop in visualTargetClass.GetIncomingWFEdges(rootACClassMethod).ToArray())
                {
                    ACClassWF fromMethodWF = edgeTop.FromWFNode as ACClassWF;
                    IACWorkflowEdge newEdgeTop = CreateWFEdge(rootACClassMethod, edgeTop.FromWFNode, fromMethodWF.GetConnector(Const.PWPointOut), methodWF, methodWF.GetConnector(Const.PWPointIn), Global.ConnectionTypes.StartTrigger);
                }
                foreach (var edgeBottom in visualTargetClass.GetOutgoingWFEdges(rootACClassMethod).ToArray())
                {
                    ACClassWF toMethodWF = edgeBottom.ToWFNode as ACClassWF;
                    IACWorkflowEdge newEdgeTop = CreateWFEdge(rootACClassMethod, methodWF, methodWF.GetConnector(Const.PWPointOut), edgeBottom.ToWFNode, toMethodWF.GetConnector(Const.PWPointIn), Global.ConnectionTypes.StartTrigger);
                }
                //_WFLayoutManager.WFLayoutGroup(Designer.LayoutAction.Insert, visualTargetClass.VisualGroup as IACWorkflowNode, methodWF);
                OnPropertyChanged("DesignXAML");
            }
            // 2. Steuerrezept auf Kante einfügen
            else if (visualTarget is IACWorkflowEdge)
            {
                ACClassWFEdge visualTargetEdge = visualTarget as ACClassWFEdge;

                IACWorkflowNode topClass = visualTargetEdge.FromWFNode;
                IACWorkflowNode bottomClass = visualTargetEdge.ToWFNode;

                string outConnName = visualTargetEdge.SourceACClassProperty.ACIdentifier, inConnName = visualTargetEdge.TargetACClassProperty.ACIdentifier;

                WPFProxyWorkflowMethod.AddToVisualChangeList(visualTargetEdge, (short)VBDesigner.LayoutActionType.DeleteEdge, visualTargetEdge.SourceACConnector, visualTargetEdge.TargetACConnector);
                visualTargetEdge.DeleteACObject(Database.ContextIPlus, false);
                rootACClassMethod.ACClassWFEdge_ACClassMethod.Remove(visualTargetEdge);

                CreateWFEdge(rootACClassMethod, topClass, topClass.GetConnector(outConnName), methodWF, methodWF.GetConnector(Const.PWPointIn), Global.ConnectionTypes.StartTrigger);

                CreateWFEdge(rootACClassMethod, methodWF, methodWF.GetConnector(Const.PWPointOut), bottomClass, bottomClass.GetConnector(inConnName), Global.ConnectionTypes.StartTrigger);

                //_WFLayoutManager.WFLayoutGroup(Designer.LayoutAction.Insert, bottomClass.VisualGroup as IACWorkflowNode, methodWF);
                OnPropertyChanged("DesignXAML");
            }
        }

        #endregion

        #region IsEnabledModifyAction
        bool IsEnabledInsertACClassMethod(ACClassMethod rootACClassMethod, ACClass pwACClass, ACClass paACClass, ACClassMethod paACClassMethod, ACClass parentACClass, IACWorkflowObject visualTarget, double x, double y)
        {
            if (!IsDesignMode)
                return false;
            // 1. Steuerrezept auf Steuerschritt einfügen
            if (visualTarget is ACClassWF)
            {
                ACClassWF targetACClassWF = visualTarget as ACClassWF;
                if (!targetACClassWF.GetIncomingWFEdges(rootACClassMethod).Any() || !targetACClassWF.GetOutgoingWFEdges(rootACClassMethod).Any())
                    return false;
                if (pwACClass.ACKind == Global.ACKinds.TPWNodeStatic)
                    return true;
                if (targetACClassWF.ACClassWF1_ParentACClassWF == null)
                    return false;

                if (targetACClassWF.ACClassWF1_ParentACClassWF.RefPAACClass != paACClass && paACClass != null && !paACClassMethod.IsSubMethod)
                    return false;
                if (targetACClassWF.WFGroup == null || !(targetACClassWF.WFGroup is ACClassWF))
                    return false;
                return true;
            }
            // 2. Steuerrezept auf Kante einfügen
            else if (visualTarget is IACWorkflowEdge)
            {
                IACWorkflowEdge visualEdge = visualTarget as IACWorkflowEdge;
                ACClassWF targetACClassWF = visualEdge.WFGroup as ACClassWF;

                if (pwACClass.ACKind == Global.ACKinds.TPWNodeStatic)
                    return true;
                if ((targetACClassWF.RefPAACClass != paACClass && paACClass != null)
                    && ((paACClassMethod == null)
                        || ((paACClassMethod != null) && (!paACClassMethod.IsSubMethod))))
                    return false;
                return true;
            }
            return false;
        }

        bool IsEnabledInsertACClass(ACClassMethod rootACClassMethod, ACClass pwACClass, ACClass paACClass, ACClass parentACClass, IACWorkflowObject visualTarget, double x, double y)
        {
            // Ziel ist ein Knoten
            if (visualTarget is ACClassWF)
            {
                ACClassWF workflowClass = visualTarget as ACClassWF;

                // Knoten muß Vorgänger und Nachfolger haben
                if (!workflowClass.GetIncomingWFEdges(rootACClassMethod).Any() || !workflowClass.GetOutgoingWFEdges(rootACClassMethod).Any())
                    return false;

                if (   parentACClass == null 
                    || (parentACClass.ACKind != Global.ACKinds.TPAProcessModuleGroup && parentACClass != workflowClass.ParentACClass)
                    || (parentACClass.ACKind == Global.ACKinds.TPAProcessModuleGroup && parentACClass.ACClass1_ParentACClass != workflowClass.ParentACClass))
                    return false;

                if (workflowClass.WFGroup == null || !(workflowClass.WFGroup is ACClassWF))
                    return false;

                return true;
            }
            // Ziel ist eine Kante
            else if (visualTarget is IACWorkflowEdge)
            {
                IACWorkflowEdge visualTargetEdge = visualTarget as IACWorkflowEdge;

                if (!(visualTargetEdge.FromWFNode is ACClassWF))
                    return false;

                ACClassWF workflowClass = visualTargetEdge.FromWFNode as ACClassWF;

                if (
                        parentACClass != null 
                        &&
                        ((parentACClass.ACKind == Global.ACKinds.TPAProcessModuleGroup && parentACClass.ACClass1_ParentACClass != workflowClass.ParentACClass)
                        || (parentACClass.ACKind != Global.ACKinds.TPAProcessModuleGroup && parentACClass != workflowClass.ParentACClass))
                   )
                    return false;

                if (visualTargetEdge.WFGroup == null || !(visualTargetEdge.WFGroup is ACClassWF))
                    return false;

                return true;
            }
            return false;
        }
        #endregion

        #region Delete

        /// <summary>
        /// Removes a WPF-Element from the design
        /// </summary>
        /// <param name="item">Item for delete.</param>
        /// <param name="isFromDesigner">If true, then call is invoked from this manager, else from gui</param>
        /// <returns><c>true</c> if is removed; otherwise, <c>false</c>.</returns>
        public override bool DeleteItem(object item, bool isFromDesigner)
        {
            if (IsEnabledDeleteVBVisual())
                return DeleteVBVisual();
            return true;
        }

        public override bool DeleteVBVisual()
        {
            // TODO: VisualChangeList
            WPFProxyWorkflowMethod.ClearVisualChangeList();
            if (!IsEnabledDeleteVBVisual())
                return false;
            if (!(CurrentDesign is ACClassMethod))
                return false;
            ACClassMethod acClassMethod = CurrentDesign as ACClassMethod;

            if (CurrentContentACObject is PWOfflineNodeMethod)
            {
                PWOfflineNodeMethod pwObjectNode = CurrentContentACObject as PWOfflineNodeMethod;
                if (DeleteWF(acClassMethod, pwObjectNode))
                {
                    UpdateVisual();
                    return true;
                }
            }
            else if (CurrentContentACObject is IACWorkflowEdge)
            {
                IACWorkflowEdge visualTargetEdge = CurrentContentACObject as IACWorkflowEdge;

                if (DeleteEdge(acClassMethod, visualTargetEdge as IACWorkflowEdge, true))
                {
                    UpdateVisual();
                    return true;
                }
            }
            return false;
        }

        protected bool DeleteWF(IACWorkflowDesignContext vbWorkflow, PWOfflineNodeMethod pwObjectNode)
        {
            IACWorkflowNode visualGroup = pwObjectNode.ContentACClassWF as IACWorkflowNode;
            IACEntityObjectContext tmpDatabase = pwObjectNode.ContentACClassWF.Database;
            if (tmpDatabase == null) // if not exist iPlus is used
                tmpDatabase = Database;
            IACConfigProvider configProvider = tmpDatabase.ACUrlCommand("\\LocalServiceObjects\\VarioConfigManager") as IACConfigProvider;
            DeleteWFRekursiv(tmpDatabase, vbWorkflow, pwObjectNode.ContentACClassWF, configProvider);
            OnPropertyChanged("DesignXAML");
            return true;
        }

        private bool DeleteWFRekursiv(IACEntityObjectContext db, IACWorkflowDesignContext vbWorkflow, ACClassWF acClassWF, IACConfigProvider configProvider)
        {
            if (!DeleteWFEdge(vbWorkflow, acClassWF))
                return false;

            foreach (var childACClassWF in acClassWF.ACClassWF_ParentACClassWF.ToList())
            {
                DeleteWFRekursiv(db, vbWorkflow, childACClassWF, configProvider);
            }
            if (configProvider != null)
            {
                configProvider.DeleteConfigNode(db, acClassWF.ACClassWFID);
            }
            WPFProxyWorkflowMethod.AddToVisualChangeList(acClassWF, (short)VBDesigner.LayoutActionType.Delete, acClassWF.VisualACUrl);
            vbWorkflow.DeleteNode(Database.ContextIPlus, acClassWF, ""/*pwObjectNode.ConfigACUrl*/);

            return true;
        }

        protected bool DeleteWFEdge(IACWorkflowDesignContext vbWorkflow, IACWorkflowNode vbVisualWF)
        {
            List<IACWorkflowNode> fromClass = new List<IACWorkflowNode>();
            List<ACClassProperty> fromPoints = new List<ACClassProperty>();
            List<IACWorkflowNode> toClass = new List<IACWorkflowNode>();
            List<ACClassProperty> toPoints = new List<ACClassProperty>();

            // Alle oberen Kanten löschen
            while (vbVisualWF.GetIncomingWFEdges(vbWorkflow).Any())
            {
                var edge1 = vbVisualWF.GetIncomingWFEdges(vbWorkflow).ElementAt(0);
                fromClass.Add(edge1.FromWFNode);
                fromPoints.Add(edge1.SourceACClassProperty);
                WPFProxyWorkflowMethod.AddToVisualChangeList(edge1, (short)VBDesigner.LayoutActionType.DeleteEdge, edge1.SourceACConnector, edge1.TargetACConnector);
                vbWorkflow.DeleteEdge(Database.ContextIPlus, edge1, vbVisualWF, true);
            }
            // Alle unteren Kanten löschen
            while (vbVisualWF.GetOutgoingWFEdges(vbWorkflow).Any())
            {
                var edge2 = vbVisualWF.GetOutgoingWFEdges(vbWorkflow).ElementAt(0);
                toClass.Add(edge2.ToWFNode);
                toPoints.Add(edge2.TargetACClassProperty);
                WPFProxyWorkflowMethod.AddToVisualChangeList(edge2, (short)VBDesigner.LayoutActionType.DeleteEdge, edge2.SourceACConnector, edge2.TargetACConnector);
                vbWorkflow.DeleteEdge(Database.ContextIPlus, edge2, vbVisualWF, false);
            }

            for (int i = 0; i < fromClass.Count; i++)
            {
                for (int j = 0; j < toClass.Count; j++)
                {
                    if (!vbWorkflow.AllWFEdges.Any(c => c.FromWFNode.XName == fromClass[i].XName && c.ToWFNode.XName == toClass[j].XName))
                        CreateWFEdge(vbWorkflow, fromClass[i], fromPoints[i], toClass[j], toPoints[j], Global.ConnectionTypes.StartTrigger);
                }
            }
            return true;
        }

        protected bool DeleteEdge(IACWorkflowDesignContext vbWorkflow, IACWorkflowEdge vbEdge, bool isFromDesinger)
        {
            try
            {
                if (isFromDesinger)
                {
                    IACWorkflowNode visualClass = ((IACWorkflowObject)vbEdge.FromWFNode).WFGroup;
                    WPFProxyWorkflowMethod.AddToVisualChangeList(vbEdge, (short)VBDesigner.LayoutActionType.DeleteEdge, vbEdge.SourceACConnector, vbEdge.TargetACConnector);
                    IACObjectEntity edgeAsEntity = vbEdge as IACObjectEntity;
                    if (edgeAsEntity != null)
                        edgeAsEntity.DeleteACObject(Database.ContextIPlus, false);
                    OnPropertyChanged("DesignXAML");
                    //if (VBDesignControl != null)
                    //    VBDesignControl.ACUrlCommand("!LoadDesign");
                    return true;
                }
                return true;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("VBDesignerWorkflowMethod", "DeleteEdge", msg);
                return false;
            }
        }
        #endregion

        #region IsEnabledDelete
        public override bool IsEnabledDeleteVBVisual()
        {
            if (!IsDesignMode)
                return false;
            if (!(CurrentDesign is ACClassMethod))
                return false;
            ACClassMethod acClassMethod = CurrentDesign as ACClassMethod;
            if (acClassMethod == null || CurrentContentACObject == null)
                return false;

            if (CurrentContentACObject is PWOfflineNodeMethod)
            {
                PWOfflineNodeMethod pwNodeMethod = CurrentContentACObject as PWOfflineNodeMethod;
                return IsEnabledDeleteWF(acClassMethod, pwNodeMethod.ContentACClassWF);
            }
            else if (CurrentContentACObject is IACWorkflowEdge)
            {
                return IsEnabledDeleteWFEdge(acClassMethod, CurrentContentACObject as IACWorkflowEdge);
            }
            return false;
        }

        protected bool IsEnabledDeleteWF(IACWorkflowDesignContext vbWorkflow, IACWorkflowNode vbVisualWF)
        {
            // Muss Bestandteil des Steuerrezepts sein
            if (!vbWorkflow.AllWFNodes.Contains(vbVisualWF))
                return false;

            // Löschen nur möglich, wenn Vorgänger und Nachfolger existieren
            // Ansonsten ist es ein Start- oder Endknoten
            if (vbVisualWF.PWACClass != null)
            {
                if (vbVisualWF.PWACClass.ACKind == Global.ACKinds.TPWNodeStart
                    || vbVisualWF.PWACClass.ACKind == Global.ACKinds.TPWNodeEnd)
                    return false;
            }
            //if (vbVisualWF.GetIncomingWFEdges(vbWorkflow).Count() > 1 && vbVisualWF.GetOutgoingWFEdges(vbWorkflow).Count() > 1)
            //    return false;

            if (vbVisualWF.IsRootWFNode(vbWorkflow) || vbWorkflow.AllWFNodes.Count() <= 3)
                return false;

            return true;
        }

        protected bool IsEnabledDeleteWFEdge(IACWorkflowDesignContext vbWorkflow, IACWorkflowEdge vbEdge)
        {
            // Muss Bestandteil des Steuerrezepts sein
            if (!vbWorkflow.AllWFEdges.Contains(vbEdge))
                return false;

            //// Wenn Edge innerhalb einer Gruppe, dann prüfen ob noch eine zweite gültige Verbindung besteht
            //if (((IACWorkflowObject)vbEdge.FromVisualClass).VisualGroup == ((IACWorkflowObject)vbEdge.ToVisualClass).VisualGroup)
            //{

            //    bool bFromOK = false;
            //    bool bToOK = false;

            //    // Vorgänger muss mindestens noch eine weitere Kante besitzen
            //    foreach (var edge in ((IACWorkflowNode)vbEdge.FromVisualClass).ChildVisualEdges)
            //    {
            //        if (edge == vbEdge)
            //            continue;
            //        if (((IACWorkflowObject)edge.FromVisualClass).VisualGroup == ((IACWorkflowObject)edge.ToVisualClass).VisualGroup &&
            //            edge.ConnectionType == Global.ConnectionTypes.StartTrigger)
            //        {
            //            bFromOK = true;
            //            break;
            //        }
            //    }
            //    // Nachfolger muss mindestens noch eine weitere Kante besitzen
            //    foreach (var edge in ((IACWorkflowNode)vbEdge.ToVisualClass).ParentVisualEdges)
            //    {
            //        if (edge == vbEdge)
            //            continue;
            //        if (((IACWorkflowObject)edge.ToVisualClass).VisualGroup == ((IACWorkflowObject)edge.ToVisualClass).VisualGroup &&
            //            edge.ConnectionType == Global.ConnectionTypes.StartTrigger)
            //        {
            //            bToOK = true;
            //            break;
            //        }
            //    }

            //    if (!bFromOK || !bToOK)
            //        return false;
            //}
            return true;
        }

        #endregion

        #region Change PWClass Derivation
        public override IEnumerable<ACClass> BaseACClassList
        {
            get
            {
                if (CurrentContentACObject == null || !(CurrentContentACObject is PWOfflineNodeMethod))
                    return null;
                PWOfflineNodeMethod pwOfflineNode = CurrentContentACObject as PWOfflineNodeMethod;

                List<ACClass> derivations = new List<ACClass>();
                var currentClass = pwOfflineNode.ContentACClassWF.PWACClass;
                derivations.Add(currentClass);
                FillDerivations(derivations, currentClass);
                while (currentClass != null)
                {
                    currentClass = currentClass.ACClass1_BasedOnACClass;
                    if (currentClass != null && typeof(PWBase).IsAssignableFrom(currentClass.ObjectType))
                    {
                        if (!currentClass.IsAbstract)
                            derivations.Add(currentClass);
                    }
                    else
                        break;
                }
                return derivations;
            }
        }

        private void FillDerivations(List<ACClass> derivations, ACClass currentClass)
        {
            if (currentClass == null)
                return;
            foreach (var childClass in currentClass.ACClass_BasedOnACClass.ToArray())
            {
                derivations.Add(childClass);
                FillDerivations(derivations, childClass);
            }
        }

        public override bool IsEnabledSwitchPWClass()
        {
            if (!IsDesignMode)
                return false;
            if (!(CurrentDesign is ACClassMethod))
                return false;
            ACClassMethod acClassMethod = CurrentDesign as ACClassMethod;
            if (acClassMethod == null || CurrentContentACObject == null)
                return false;

            if (CurrentContentACObject is PWOfflineNodeMethod)
            {
                return true;
            }
            return false;
        }

        public override void SwitchACClassOK()
        {
            CloseTopDialog();
            if (PWClassToSwitch != null)
            {
                PWOfflineNodeMethod pwOfflineNode = CurrentContentACObject as PWOfflineNodeMethod;
                if (pwOfflineNode != null)
                {
                    pwOfflineNode.ContentACClassWF.PWACClass = PWClassToSwitch;
                    pwOfflineNode.OnPropertyChanged("ContentACClassWF");
                    pwOfflineNode.OnPropertyChanged(nameof(XAMLDesign));
                    ACClassMethod acClassMethod = CurrentDesign as ACClassMethod;
                    if (acClassMethod != null)
                        acClassMethod.UpdateDate = DateTime.Now;
                }
            }
        }

        public override bool IsEnabledSwitchACClassOK()
        {
            if (PWClassToSwitch == null || PWClassToSwitch.ObjectType == null)
                return false;
            PWOfflineNodeMethod pwOfflineNode = CurrentContentACObject as PWOfflineNodeMethod;
            if (pwOfflineNode == null || pwOfflineNode.ContentACClassWF == null)
                return false;
            // Falls eine Workflow-Klasse aus den Assemblies entfernt worden ist, dann dar ausgetauscht werdne:
            if (pwOfflineNode.ContentACClassWF.PWACClass == null || pwOfflineNode.ContentACClassWF.PWACClass.ObjectType == null)
            {
                if (PWClassToSwitch.ObjectType != null)
                    return true;
                else
                    return false;
            }
            if (pwOfflineNode.ContentACClassWF.PWACClass.ObjectType.IsAssignableFrom(PWClassToSwitch.ObjectType)
                || PWClassToSwitch.ObjectType.IsAssignableFrom(pwOfflineNode.ContentACClassWF.PWACClass.ObjectType))
                return true;
            return false;
        }

        #endregion

        #region DetailWorkflow
        [ACMethodInteraction("WF", "en{'Details'}de{'Details'}", 200, true)]
        public void ShowDetail()
        {
            if (!IsEnabledShowDetail())
                return;

            PWOfflineNodeMethod pwObjectNode = CurrentContentACObject as PWOfflineNodeMethod;

            this.ParentACComponent.ACUrlCommand("!LoadDetail", pwObjectNode);
        }

        public bool IsEnabledShowDetail()
        {
            PWOfflineNodeMethod pwObjectNode = CurrentContentACObject as PWOfflineNodeMethod;

            if (pwObjectNode == null)
                return false;
            if (pwObjectNode.ContentACClassWF.RefPAACClassMethod == null)
                return false;
            if (pwObjectNode.ContentACClassWF.RefPAACClassMethod.ACKind != Global.ACKinds.MSWorkflow)
                return false;

            return true;
        }
        #endregion

        #region ACClassMethodHelper
        public ACClassWF CreateNewMethodWF(ACClassMethod rootACClassMethod, Database database, ACClass pwACClass, ACClass paACClass, ACClassMethod paACClassMethod, IACWorkflowNode groupVisualClass)
        {
            ACClassWF acClassWF = null;
            string secondaryKey = Root.NoManager.GetNewNo(Database, typeof(ACClassWF), ACClassWF.NoColumnName, ACClassWF.FormatNewNo, this);
            acClassWF = ACClassWF.NewACClassWF(database, rootACClassMethod, pwACClass, paACClass, paACClassMethod, groupVisualClass, secondaryKey);
            database.ACClassWF.Add(acClassWF);
            var acClassDesign = acClassWF.PWACClass.GetDesign(acClassWF.PWACClass, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
            rootACClassMethod.AddNode(acClassWF);
            WPFProxyWorkflowMethod.AddToVisualChangeListRectParam(acClassWF, (short)VBDesigner.LayoutActionType.Insert, acClassDesign.VisualWidth, acClassDesign.VisualHeight, "", "");
            return acClassWF;
        }

        public ACClassWF CopyACClassWF(ACClassMethod rootACClassMethod, ACClassWF parentACClassWF,
                                            ACClassWF fromACClassWF, Dictionary<ACClassWF, ACClassWF> methodWFList)
        {
            ACClassWF acClassWF = CreateNewMethodWF(rootACClassMethod, Database.ContextIPlus, fromACClassWF.PWACClass,
                                            fromACClassWF.RefPAACClass, fromACClassWF.RefPAACClassMethod, parentACClassWF);

            acClassWF.ACIdentifier = fromACClassWF.ACIdentifier;
            acClassWF.PhaseIdentifier = fromACClassWF.PhaseIdentifier;

            acClassWF.Comment = fromACClassWF.Comment;
            acClassWF.XMLConfig = fromACClassWF.XMLConfig;

            Database.ContextIPlus.ACClassWF.Add(acClassWF);

            methodWFList.Add(fromACClassWF, acClassWF);

            foreach (var childACClassWF in fromACClassWF.ACClassWF_ParentACClassWF)
            {
                CopyACClassWF(rootACClassMethod, acClassWF, childACClassWF, methodWFList);
            }

            return acClassWF;
        }

        public bool CopyACClassWFEdges(ACClassMethod rootACClassMethod, ACClassMethod acClassMethod, Dictionary<ACClassWF, ACClassWF> methodWFList)
        {
            foreach (ACClassWFEdge acClassWFEdge in acClassMethod.ACClassWFEdge_ACClassMethod)
            {
                string secondaryKey = Root.NoManager.GetNewNo(Database, typeof(ACClassWFEdge), ACClassWFEdge.NoColumnName, ACClassWFEdge.FormatNewNo, this);
                ACClassWFEdge acClassWFEdgeNew = ACClassWFEdge.NewACObject(Database.ContextIPlus, rootACClassMethod, secondaryKey);
                acClassWFEdgeNew.SourceACClassWF = methodWFList[acClassWFEdge.SourceACClassWF];
                acClassWFEdgeNew.TargetACClassWF = methodWFList[acClassWFEdge.TargetACClassWF];

                acClassWFEdgeNew.SourceACClassProperty = acClassWFEdge.SourceACClassProperty;
                acClassWFEdgeNew.TargetACClassProperty = acClassWFEdge.TargetACClassProperty;
                acClassWFEdgeNew.ConnectionType = acClassWFEdge.ConnectionType;
                Database.ContextIPlus.ACClassWFEdge.Add(acClassWFEdgeNew);
                WPFProxyWorkflowMethod.AddToVisualChangeList(acClassWFEdgeNew, (short)VBDesigner.LayoutActionType.InsertEdge, acClassWFEdgeNew.SourceACConnector, acClassWFEdgeNew.TargetACConnector);
            }

            return true;
        }

        #endregion

        #region IACComponentDesignManager

        /// <summary>
        /// Creates a Edge between two points
        /// </summary>
        /// <param name="sourceVBConnector">The source VB connector.</param>
        /// <param name="targetVBConnector">The target VB connector.</param>
        public override void CreateEdge(IVBConnector sourceVBConnector, IVBConnector targetVBConnector)
        {
            WPFProxyWorkflowMethod.ClearVisualChangeList();
            IACWorkflowNode sourceVBVisual = sourceVBConnector.ParentACObject as IACWorkflowNode;
            if (sourceVBVisual == null)
            {
                IACComponent contentObject = sourceVBConnector.ParentACObject as IACComponent;
                if (contentObject != null)
                    sourceVBVisual = contentObject.Content as IACWorkflowNode;
                if (sourceVBVisual == null)
                {
                    sourceVBVisual = sourceVBConnector.ParentACObject.ACContentList.First() as IACWorkflowNode;
                }
            }
            ACClassProperty sourceACClassProperty = null;
            IACWorkflowNode targetVBVisual = targetVBConnector.ParentACObject as IACWorkflowNode;
            if (targetVBVisual == null)
            {
                IACComponent contentObject = targetVBConnector.ParentACObject as IACComponent;
                if (contentObject != null)
                    targetVBVisual = contentObject.Content as IACWorkflowNode;
                if (targetVBVisual == null)
                {
                    targetVBVisual = targetVBConnector.ParentACObject.ACContentList.First() as IACWorkflowNode;
                }
            }
            ACClassProperty targetACClassProperty = null;

            sourceACClassProperty = sourceVBVisual.GetConnector(sourceVBConnector.VBContent);

            targetACClassProperty = targetVBVisual.GetConnector(targetVBConnector.VBContent);

            WFCreateWFEdge(sourceVBVisual, sourceACClassProperty, targetVBVisual, targetACClassProperty);
            UpdateVisual();
        }

        /// <summary>
        /// Checks if a Edge can be created between two points
        /// </summary>
        /// <param name="sourceVBConnector">The source VB connector.</param>
        /// <param name="targetVBConnector">The target VB connector.</param>
        /// <returns><c>true</c> if is enabled; otherwise, <c>false</c>.</returns>
        public override bool IsEnabledCreateEdge(IVBConnector sourceVBConnector, IVBConnector targetVBConnector)
        {
            if (!IsDesignMode)
                return false;

            IACWorkflowNode sourceVBVisual = sourceVBConnector.ParentACObject as IACWorkflowNode;
            if (sourceVBVisual == null)
            {
                IACComponent contentObject = sourceVBConnector.ParentACObject as IACComponent;
                if (contentObject != null)
                    sourceVBVisual = contentObject.Content as IACWorkflowNode;
                if (sourceVBVisual == null)
                {
                    if (sourceVBConnector.ParentACObject.ACContentList == null)
                        return false;
                    sourceVBVisual = sourceVBConnector.ParentACObject.ACContentList.First() as IACWorkflowNode;
                }
            }
            ACClassProperty sourceACClassProperty = null;
            IACWorkflowNode targetVBVisual = targetVBConnector.ParentACObject as IACWorkflowNode;
            if (targetVBVisual == null)
            {
                IACComponent contentObject = targetVBConnector.ParentACObject as IACComponent;
                if (contentObject != null)
                    targetVBVisual = contentObject.Content as IACWorkflowNode;
                if (targetVBVisual == null)
                {
                    targetVBVisual = targetVBConnector.ParentACObject.ACContentList.First() as IACWorkflowNode;
                }
            }
            ACClassProperty targetACClassProperty = null;

            sourceACClassProperty = sourceVBVisual.GetConnector(sourceVBConnector.VBContent);

            targetACClassProperty = targetVBVisual.GetConnector(targetVBConnector.VBContent);

            return IsEnabledWFCreateEdge(sourceVBVisual, sourceACClassProperty, targetVBVisual, targetACClassProperty);
        }


        /// <summary>Asks this design manager if he can create edges</summary>
        /// <returns><c>true</c> if this instance can create edges; otherwise, <c>false</c>.</returns>
        public override bool CanManagerCreateEdges()
        {
            return true;
        }

        #endregion

        #region Layout
        public override string DesignXAML
        {
            get
            {
                if (CurrentDesign == null)
                    return "";

                return CurrentDesign.XAMLDesign;
            }
            set
            {
                if (CurrentDesign == null)
                    return;

                if (CurrentDesign.XAMLDesign != value)
                {
                    CurrentDesign.XAMLDesign = value;
                    OnPropertyChanged("DesignXAML");
                }
            }
        }

        protected override void UpdateVisual()
        {
            WPFProxyWorkflowMethod.UpdateVisual();
        }

        //private void FillWFDesign(IACObjectDesignWF vbWorkflow, string rootPath, ACClassWF acClassWF, ref string wfDesignXAML)
        //{
        //    ACClassDesign acClassDesign = acClassWF.GetVisualDesign(Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
        //    string controlName = acClassDesign.ValueTypeACClass.ACIdentifier;

        //    //string vbContent = acClassWF.GetACUrl(acClassWF.ParentACObject);
        //    string vbContent = acClassWF.ACIdentifier;
        //    if (!string.IsNullOrEmpty(rootPath))
        //    {
        //        vbContent = rootPath + "\\" + vbContent;
        //    }
        //    if (controlName == Const.VBVisualGroup_ClassName)
        //    {
        //        wfDesignXAML += string.Format("<vb:VBVisualGroup VBContent=\"{0}\" Height=\"{1}\" Width=\"{2}\" Canvas.Top=\"{3}\" Canvas.Left=\"{4}\" Name=\"{5}\">\n",
        //            vbContent ,          // VBContent
        //            acClassWF.VisualHeight,    // Height
        //            acClassWF.VisualWidth,     // Width
        //            acClassWF.VisualTop,       // Top
        //            acClassWF.VisualLeft,      // Left
        //            acClassWF.XName);          
        //        wfDesignXAML += "<Canvas>\n";

        //        var query = vbWorkflow.VisualClasss.Where(c => c.VisualGroup == acClassWF).Select(c => c as ACClassWF);
        //        foreach (var acClassWFChild in query)
        //        {
        //            var xx = acClassWFChild.GetACUrl();
        //            FillWFDesign(vbWorkflow, "", acClassWFChild, ref wfDesignXAML);
        //        }
        //        wfDesignXAML += "</Canvas>\n";
        //        wfDesignXAML += "</vb:VBVisualGroup>\n";
        //    }
        //    else
        //    {
        //        wfDesignXAML += string.Format("<vb:VBVisual VBContent=\"{0}\" Height=\"{1}\" Width=\"{2}\" Canvas.Top=\"{3}\" Canvas.Left=\"{4}\" Name=\"{5}\"></vb:VBVisual>\n",
        //            vbContent,          // VBContent
        //            acClassWF.VisualHeight,    // Height
        //            acClassWF.VisualWidth,     // Width
        //            acClassWF.VisualTop,       // Top
        //            acClassWF.VisualLeft,      // Left
        //            acClassWF.XName);          
        //    }
        //}

        public override IEnumerable<IACObject> GetAvailableTools()
        {
            return WPFProxyWorkflowMethod.GetAvailableTools();
        }
        #endregion

        #region VBDesignerXAML Dialog

        /// <summary>
        /// Switches the this designer to design mode and the Designer-Tool (WPF-Control) appears on the gui.
        /// </summary>
        /// <param name="dockingManagerName">Name of the parent docking manager.</param>
        public override void ShowDesignManager(string dockingManagerName = "")
        {
            ACClassMethod acClassMethod = CurrentDesign as ACClassMethod;
            if (acClassMethod != null)
            {
                if (ConfigManagerIPlus.IsWorkflowActive(this, acClassMethod, this.Database.ContextIPlus))
                {
                    Messages.Info(this, "Info50016");
                }
            }

            if (GetWindow("ToolWindow") == null)
            {
                ShowWindow(this, "ToolWindow", false, Global.VBDesignContainer.DockableWindow, Global.VBDesignDockState.AutoHideButton, Global.VBDesignDockPosition.Left, Global.ControlModes.Hidden, dockingManagerName, Global.ControlModes.Hidden);
                base.ShowDesignManager(dockingManagerName);
            }
        }

        /// <summary>
        /// Switches the designer off and the Designer-Tool (WPF-Control) disappears on the gui.
        /// </summary>
        public override void HideDesignManager()
        {
            IACObject window = GetWindow("ToolWindow");
            if (window != null)
                CloseDockableWindow(window);

            base.HideDesignManager();

            VBPresenterMethod presenterMethod = this.ParentACComponent as VBPresenterMethod;
            if (presenterMethod != null)
                presenterMethod.Load(presenterMethod.SelectedWFContext);
        }
        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "IsEnabledDoModifyAction":
                    result = IsEnabledDoModifyAction(acParameter[0] as IACWorkflowDesignContext, (IACInteractiveObject)acParameter[1], (IACInteractiveObject)acParameter[2], (Double)acParameter[3], (Double)acParameter[4]);
                    return true;
                case "IsEnabledDeleteVBVisual":
                    result = IsEnabledDeleteVBVisual();
                    return true;
                case "IsEnabledSwitchPWClass":
                    result = IsEnabledSwitchPWClass();
                    return true;
                case "IsEnabledSwitchACClassOK":
                    result = IsEnabledSwitchACClassOK();
                    return true;
                case "ShowDetail":
                    ShowDetail();
                    return true;
                case "IsEnabledShowDetail":
                    result = IsEnabledShowDetail();
                    return true;
                case "IsEnabledCreateEdge":
                    result = IsEnabledCreateEdge((IVBConnector)acParameter[0], (IVBConnector)acParameter[1]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #endregion

    }
}
