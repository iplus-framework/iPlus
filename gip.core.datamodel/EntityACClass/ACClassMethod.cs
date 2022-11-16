// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 12-14-2012
// ***********************************************************************
// <copyright file="ACClassMethod.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACClassMethod contains metainformation about the methods that an ACClass provides. 
    /// Entries in the ACMethod table are mainly added by reflection when starting iPlus with the Ctrl key. 
    /// All methods that are marked with the ACMethodInfo attribute classes in the source code are entered into this table. 
    /// Methods added by iPlus development environment are also entered in this table. 
    /// During runtime, they are added as C#-scripts (server side, client side, pre/post). Virtual methods (ACMethod) and workflow methods are also entered in this table.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Method'}de{'Methode'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, Const.ACIdentifierPrefix, "en{'Method Name/ID'}de{'Methodenname/ID'}", "", "", true)]
    [ACPropertyEntity(3, "ACIdentifierKey", "en{'Key'}de{'Schlüssel'}", "", "", true)]
    [ACPropertyEntity(4, ACClass.ClassName, "en{'Class'}de{'Klasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(5, Const.ACGroup, "en{'Group'}de{'Gruppe'}", "", "", true)]
    [ACPropertyEntity(6, Const.ACKindIndex, "en{'Method Type'}de{'Methodenart'}", typeof(Global.ACKinds), Const.ContextDatabaseIPlus + "\\ACKindMSList", "", true)]
    [ACPropertyEntity(7, "SortIndex", "en{'Sortindex'}de{'Sortierung'}", "", "", true)]
    [ACPropertyEntity(8, "IsCommand", "en{'Command Method'}de{'Kommando Methode'}", "", "", true)]
    [ACPropertyEntity(9, "IsInteraction", "en{'Interaction Method'}de{'Interaktions Methode'}", "", "", true)]
    [ACPropertyEntity(10, "IsAsyncProcess", "en{'Async.Process Method'}de{'Async.Prozess Methode'}", "", "", true)]
    [ACPropertyEntity(11, "IsPeriodic", "en{'Periodic'}de{'Periodisch'}", "", "", true)]
    [ACPropertyEntity(12, "IsRightmanagement", "en{'Rights Management'}de{'Rechteverwaltung'}", "", "", true)]
    [ACPropertyEntity(13, "IsAutoenabled", "en{'Autoenabled'}de{'Autom.Aktiv'}", "", "", true)]
    [ACPropertyEntity(14, "IsSubMethod", "en{'Submethod'}de{'Untermethode'}", "", "", true)]
    [ACPropertyEntity(15, "ContinueByError", "en{'Continue on Error'}de{'Fortsetzen bei Fehler'}", "", "", true)]
    [ACPropertyEntity(16, "Comment", "en{'Comment'}de{'Bemerkung'}", "", "", true)]
    [ACPropertyEntity(17, "IsPersistable", "en{'Persistable'}de{'Persistierbar'}", "", "", true)]
    [ACPropertyEntity(18, "IsSystem", "en{'System'}de{'System'}", "", "", true)]
    [ACPropertyEntity(19, "ContextMenuCategoryIndex", "en{'Contextmenu Category'}de{'Kontextmenükategorie'}", "", "", true)]
    [ACPropertyEntity(20, "IsRPCEnabled", "en{'Is RPC Enabled'}de{'Ist RPC aktiviert'}", "", "", true)]
    [ACPropertyEntity(21, "AttachedFromACClass", "en{'Attached from Class'}de{'Angehängt von der Klasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(22, "IsStatic", "en{'Static'}de{'Statisch'}", "", "", true)]
    [ACPropertyEntity(22, "ExecuteByDoubleClick", "en{'Execute by DoubleClick'}de{'Ausführen mit DoubleClick'}", "", "", true)]
    [ACPropertyEntity(496, Const.EntityInsertDate, Const.EntityTransInsertDate)]
    [ACPropertyEntity(497, Const.EntityInsertName, Const.EntityTransInsertName)]
    [ACPropertyEntity(498, Const.EntityUpdateDate, Const.EntityTransUpdateDate)]
    [ACPropertyEntity(499, Const.EntityUpdateName, Const.EntityTransUpdateName)]
    [ACPropertyEntity(9999, "ACCaptionTranslation", "en{'Translation'}de{'Übersetzung'}", "", "", true)]
    [ACPropertyEntity(9999, "PWACClass", "en{'Workflowclass'}de{'Workflowklasse'}", Const.ContextDatabaseIPlus + "\\PWNodeList", "", true)]
    [ACPropertyEntity(9999, "Sourcecode", "en{'Sourcecode'}de{'Quellcode'}", "", "", true)]
    [ACPropertyEntity(9999, "InteractionVBContent", "en{'Interaction Content'}de{'Interaktionsinhalt'}", "", "", true)]
    [ACPropertyEntity(9999, "ValueTypeACClass", "en{'Data Type'}de{'Datentyp'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(9999, "ACClassMethod1_ParentACClassMethod", "en{'Parent Method'}de{'Elternmethode'}", Const.ContextDatabaseIPlus + "\\" + ACClassMethod.ClassName, "", true)]
    [ACPropertyEntity(9999, "XMLACMethod", "en{'Method Definition'}de{'Methodendefinition'}")]
    [ACDeleteAction("VBGroupRight_ACClassMethod", Global.DeleteAction.CascadeManual)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassMethod.ClassName, "en{'Method'}de{'Methode'}", typeof(ACClassMethod), ACClassMethod.ClassName, Const.ACIdentifierPrefix, "SortIndex," + Const.ACIdentifierPrefix)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassMethod>) })]
    public partial class ACClassMethod : IACObjectEntityWithCheckTrans, IACEntityProperty, IACWorkflowDesignContext, IACType, IACClassEntity, IACConfigStore, ICloneable, IInsertInfo, IUpdateInfo
    {
        public const string ClassName = "ACClassMethod";
        public readonly ACMonitorObject _10020_LockValue = new ACMonitorObject(10020);

        #region New/Delete

        /// <summary>
        /// Creates a new ACClassMethod-Instance
        /// </summary>
        /// <param name="database"></param>
        /// <param name="parentACObject"></param>
        /// <returns></returns>
        public static ACClassMethod NewACObject(Database database, IACObject parentACObject)
        {
            ACClassMethod entity = new ACClassMethod();
            entity.Database = database;
            entity.ACClassMethodID = Guid.NewGuid();
            entity.ACKind = Global.ACKinds.MSMethodExt;
            entity.ContinueByError = false;
            entity.ACIdentifier = "";
            entity.IsAutoenabled = true;
            entity.IsRightmanagement = false;
            entity.IsAsyncProcess = false;
            entity.IsPeriodic = false;
            entity.IsParameterACMethod = false;
            entity.IsSubMethod = false;
            entity.IsPersistable = false;
            entity.SortIndex = 9999;
            entity.IsStatic = false;
            entity.ExecuteByDoubleClick = false;
            if (parentACObject is ACClass)
            {
                entity.ACClass = parentACObject as ACClass;
            }
            if (parentACObject is ACClassMethod)
            {
                entity.ACClassMethod1_ParentACClassMethod = parentACObject as ACClassMethod;
                entity.ACClass = entity.ACClassMethod1_ParentACClassMethod.ACClass;
            }
            entity.IsRPCEnabled = false;
            entity.BranchNo = 0;

            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }


        /// <summary>
        /// Deletes this entity-object from the database
        /// UNSAFE, use (QueryLock_1X000)
        /// </summary>
        /// <param name="database">Entity-Framework databasecontext</param>
        /// <param name="withCheck">If set to true, a validation happens before deleting this EF-object. If Validation fails message ís returned.</param>
        /// <param name="softDelete">If set to true a delete-Flag is set in the dabase-table instead of a physical deletion. If  a delete-Flag doesn't exit in the table the record will be deleted.</param>
        /// <returns>If a validation or deletion failed a message is returned. NULL if sucessful.</returns>
        public override MsgWithDetails DeleteACObject(IACEntityObjectContext database, bool withCheck, bool softDelete = false)
        {
            if (withCheck)
            {
                MsgWithDetails msg = IsEnabledDeleteACObject(database);
                if (msg != null)
                    return msg;
            }
            var query2 = (database as Database).VBGroupRight.Where(c => c.ACClassMethod.ACClassMethodID == this.ACClassMethodID).ToList();
            foreach (var vbGroupRight in query2)
            {
                vbGroupRight.DeleteACObject(database, false);
            }

            this.ParentACClassMethodID = null;

            if (this.ACKind == Global.ACKinds.MSWorkflow)
            {
                foreach (var acClassWF in ACClassWF_ACClassMethod.ToList())
                {
                    acClassWF.DeleteACObject(database, false);
                }

                foreach (var acClassWFEdge in ACClassWFEdge_ACClassMethod.ToList())
                {
                    acClassWFEdge.SourceACClassProperty = null;
                    acClassWFEdge.TargetACClassProperty = null;
                    acClassWFEdge.DeleteACObject(database, false);

                }

                foreach (var config in this.ACClassMethodConfig_ACClassMethod.ToList())
                    config.DeleteACObject(Database, false);

                foreach (var program in this.ACProgram_ProgramACClassMethod.ToList())
                {
                    program.DeleteACObject(Database, false);
                }
            }

            database.Remove(this);
            return null;
        }


        /// <summary>Check if entity-object can be deleted from the database</summary>
        /// <param name="database">Entity-Framework databasecontext</param>
        /// <returns>If deletion is not allowed or the validation failed a message is returned. NULL if sucessful.</returns>
        public override MsgWithDetails IsEnabledDeleteACObject(IACEntityObjectContext database)
        {
            if (ACKind == Global.ACKinds.MSMethod || ACKind == Global.ACKinds.MSMethodPrePost || ACKind == Global.ACKinds.MSMethodClient)
            {
                return this.NewMessage(eMsgLevel.Error,
                    Database.Root.Environment.TranslateMessage(Database.Root, "Error00009"),
                    (ACType as ACClass).ACCaption + ": " + this.ACCaption, this.GetACUrl());
            }

            return this.ReflectIsEnabledDelete();
        }


        /// <summary>
        /// Creates a new workflow-method
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acClass">The ac class.</param>
        /// <returns>ACClassMethod.</returns>
        public static ACClassMethod NewWorkACClassMethod(Database database, ACClass acClass)
        {
            ACClassMethod entity = ACClassMethod.NewACObject(database, acClass);

            entity.ACIdentifier = GetNextMethodName(acClass, "Workflow");

            entity.PWACClass = database.PWNodeWorkFlowList.Where(c => c.ACIdentifier == Const.ACClassIdentifierOfPWMethod).FirstOrDefault();
            entity.ACKind = Global.ACKinds.MSWorkflow;
            entity.IsAutoenabled = true;
            entity.ContinueByError = false;
            entity.IsInteraction = false;
            entity.IsCommand = false;
            entity.IsPersistable = true;
            entity.IsAsyncProcess = true;
            entity.IsRPCEnabled = false;
            entity.UpdateACMethodWorkflow();
            return entity;
        }


        /// <summary>
        /// Creates a new script-method (Server-Side)
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acKindMethod">The ac kind method.</param>
        /// <returns>ACClassMethod.</returns>
        public static ACClassMethod NewScriptACClassMethod(Database database, ACClass acClass, Global.ACKinds acKindMethod)
        {
            ACClassMethod entity = ACClassMethod.NewACObject(database, acClass);

            entity.ACIdentifier = GetNextMethodName(acClass, "MethodName");
            entity.ValueTypeACClass = database.ACClass.Where(c => c.ACIdentifier == "Void" && c.ACKindIndex == (Int16)Global.ACKinds.TACLRBaseTypes).First();
            entity.ACKind = acKindMethod;
            entity.IsAutoenabled = true;
            entity.ContinueByError = true;
            entity.IsInteraction = false;
            entity.IsCommand = false;
            entity.IsRPCEnabled = false;

            entity.Sourcecode = "/// <Precompiler>\n";
            //entity.Sourcecode += "/// refassembly C:\\Program Files\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.0\\System.Core.dll;\n";

            entity.Sourcecode += "/// refassembly " + acClass.ObjectType.Assembly.ManifestModule.Name + ";\n";
            entity.Sourcecode += "using System.Linq;\n";
            entity.Sourcecode += "using " + acClass.ObjectType.Namespace + ";\n";
            entity.Sourcecode += "/// </Precompiler>\n\n";

            entity.Sourcecode += "public Boolean ";
            entity.Sourcecode += entity.ACIdentifier;

            string name = acClass.ACIdentifier.Substring(0, 1).ToLower() + acClass.ACIdentifier.Substring(1);

            if ((acKindMethod == Global.ACKinds.MSMethodExtClient) || (acClass.AssemblyQualifiedName.Contains("gip.core.autocomponent.Environment")))
            {
                entity.Sourcecode += "(" + ScriptTrigger.MethodSignatureThisParam + ")";
            }
            else
            {
                entity.Sourcecode += "(" + acClass.ObjectType.Name + " " + name + ")";
            }
            entity.Sourcecode += "\n{";
            entity.Sourcecode += "\n\t return true;";
            entity.Sourcecode += "\n}";

            return entity;
        }


        /// <summary>
        /// Creates a new script-method (Client-Side)
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acClass">The ac class.</param>
        /// <returns>ACClassMethod.</returns>
        public static ACClassMethod NewScriptClientACClassMethod(Database database, ACClass acClass)
        {
            ACClassMethod entity = NewScriptACClassMethod(database, acClass, Global.ACKinds.MSMethodExtClient);
            return entity;
        }


        /// <summary>
        /// Creates a new Pre-Execution script-method
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassMethod">The ac class method.</param>
        /// <returns>ACClassMethod.</returns>
        public static ACClassMethod NewPreACClassMethod(Database database, ACClass acClass, ACClassMethod acClassMethod)
        {
            return NewScriptTriggerACClassMethod(ScriptTrigger.Type.PreExecute, database, acClass, acClassMethod.ACIdentifier);
        }


        /// <summary>
        /// Creates a new Post-Execution script-method
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassMethod">The ac class method.</param>
        /// <returns>ACClassMethod.</returns>
        public static ACClassMethod NewPostACClassMethod(Database database, ACClass acClass, ACClassMethod acClassMethod)
        {
            return NewScriptTriggerACClassMethod(ScriptTrigger.Type.PostExecute, database, acClass, acClassMethod.ACIdentifier);
        }


        /// <summary>
        /// Creates a new script-method which is called on special property/point-Events
        /// </summary>
        /// <param name="scriptTriggerType">Type of the script trigger.</param>
        /// <param name="database">The database.</param>
        /// <param name="acClass">The ac class.</param>
        /// <param name="methodNamePostfix">The method name postfix.</param>
        /// <returns>ACClassMethod.</returns>
        public static ACClassMethod NewScriptTriggerACClassMethod(ScriptTrigger.Type scriptTriggerType, Database database, ACClass acClass, string methodNamePostfix)
        {
            ScriptTrigger methodSignature = ScriptTrigger.ScriptTriggers.Where(c => c.TriggerType == scriptTriggerType).First();

            ACClassMethod entity = ACClassMethod.NewACObject(database, acClass);

            entity.ACIdentifier = GetNextMethodName(acClass, methodSignature.GetMethodName(methodNamePostfix));

            entity.ACKind = Global.ACKinds.MSMethodExtTrigger;
            entity.IsAutoenabled = true;
            entity.ContinueByError = true;
            entity.IsRPCEnabled = false;

            entity.Sourcecode = "/// <Precompiler>\n";
            entity.Sourcecode += "/// refassembly " + acClass.ObjectType.Assembly.ManifestModule.Name + ";\n";
            entity.Sourcecode += "/// using System.Linq;\n";
            entity.Sourcecode += "/// using " + acClass.ObjectType.Namespace + ";\n";
            entity.Sourcecode += "/// </Precompiler>\n\n";

            if (String.IsNullOrEmpty(methodSignature.MethodReturnSignature))
                entity.Sourcecode += "public void ";
            else
                entity.Sourcecode += "public " + methodSignature.MethodReturnSignature + " ";
            entity.Sourcecode += entity.ACIdentifier;
            entity.Sourcecode += methodSignature.GetMethodSignatureForACClass(acClass);
            entity.Sourcecode += "\n{";
            entity.Sourcecode += "\n\t// TODO: Insert Sourcecode";
            if (!String.IsNullOrEmpty(methodSignature.MethodReturnSignature))
            {
                entity.Sourcecode += "\n\t return (" + methodSignature.MethodReturnSignature + ")[ReturnValue];";
            }
            entity.Sourcecode += "\n}";
            return entity;
        }



        /// <summary>
        /// Creates a new attached Method
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acClassOfProcessModule">Class whre method should appear</param>
        /// <param name="virtualMethodOfChild"></param>
        /// <param name="acClassOfFunction"></param>
        /// <param name="combineMethodNameWithFunc"></param>
        /// <returns>ACClassMethod.</returns>
        public static ACClassMethod NewAttachedFunctionMethod(Database database, ACClass acClassOfProcessModule,
            ACClassMethod virtualMethodOfChild, ACClass acClassOfFunction,
            bool combineMethodNameWithFunc)
        {
            ACClassMethod acClassMethod1 = ACClassMethod.NewACObject(database, acClassOfProcessModule);
            acClassMethod1.ACKind = Global.ACKinds.MSMethodFunction;
            acClassMethod1.ACGroup = virtualMethodOfChild.ACClass.ACIdentifier;
            acClassMethod1.SortIndex = 9999;
            acClassMethod1.IsRightmanagement = false;
            if (combineMethodNameWithFunc)
                acClassMethod1.ACIdentifier = virtualMethodOfChild.ACIdentifier + ACUrlHelper.AttachedMethodIDConcatenator + acClassOfFunction.ACIdentifier;
            else
                acClassMethod1.ACIdentifier = virtualMethodOfChild.ACIdentifier;
            acClassMethod1.ACCaptionTranslation = virtualMethodOfChild.ACCaptionTranslation;
            acClassMethod1.Comment = virtualMethodOfChild.Comment;
            acClassMethod1.IsCommand = virtualMethodOfChild.IsCommand;
            acClassMethod1.IsInteraction = virtualMethodOfChild.IsInteraction;
            acClassMethod1.IsAsyncProcess = virtualMethodOfChild.IsAsyncProcess;
            acClassMethod1.InteractionVBContent = virtualMethodOfChild.InteractionVBContent;
            acClassMethod1.IsAutoenabled = true;
            acClassMethod1.IsRPCEnabled = false;
            acClassMethod1.PWACClass = virtualMethodOfChild.PWACClass;
            acClassMethod1.Sourcecode = null;
            acClassMethod1.ContinueByError = virtualMethodOfChild.ContinueByError;
            acClassMethod1.ValueTypeACClass = virtualMethodOfChild.ValueTypeACClass;
            acClassMethod1.ACClassMethod1_ParentACClassMethod = virtualMethodOfChild;
            acClassMethod1.XMLACMethod = virtualMethodOfChild.XMLACMethod;
            acClassMethod1.GenericType = virtualMethodOfChild.GenericType;
            acClassMethod1.XMLConfig = virtualMethodOfChild.XMLConfig;
            acClassMethod1.AttachedFromACClass = acClassOfFunction;
            acClassOfProcessModule.AddNewACClassMethod(acClassMethod1);

            return acClassMethod1;
        }


        /// <summary>
        /// Creates a uniqe Pre/Pos-Methodname
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="preMethodName">Name of the pre method.</param>
        /// <returns>System.String.</returns>
        private static string GetNextMethodName(ACClass acClass, string preMethodName)
        {
            Database db = acClass.GetObjectContext<Database>();
            string entityNo = ACClassMethod.ClassName + "No";
            string secondaryKey = gip.core.datamodel.Database.Root.NoManager.GetNewNo(db, typeof(ACClassMethod), entityNo, null, null);
            return preMethodName + secondaryKey;
            // return preMethodName + NoConfigurationManager.GetNewNo(ACClassMethod.ClassName + "No");
        }


        public Database Database { get; private set; } = null;

        void IACClassEntity.OnObjectMaterialized(Database db)
        {
            if (Database == null)
                Database = db;
            LastReadUpdateACClassWF = UpdateDate;
        }

        #region Cloning

        public object Clone()
        {
            ACClassMethod clonedObject = ACClassMethod.NewACObject(this.Database, null);
            clonedObject.Database = Database;
            clonedObject.ACClassMethodID = this.ACClassMethodID;
            clonedObject.ACClassID = this.ACClassID;
            clonedObject.ACIdentifier = this.ACIdentifier;
            clonedObject.ACIdentifierKey = this.ACIdentifierKey;
            clonedObject.ACCaptionTranslation = this.ACCaptionTranslation;
            clonedObject.ACGroup = this.ACGroup;
            clonedObject.Sourcecode = this.Sourcecode;
            clonedObject.ACKindIndex = this.ACKindIndex;
            clonedObject.SortIndex = this.SortIndex;
            clonedObject.IsRightmanagement = this.IsRightmanagement;
            clonedObject.Comment = this.Comment;
            clonedObject.IsCommand = this.IsCommand;
            clonedObject.IsInteraction = this.IsInteraction;
            clonedObject.IsAsyncProcess = this.IsAsyncProcess;
            clonedObject.IsPeriodic = this.IsPeriodic;
            clonedObject.IsParameterACMethod = this.IsParameterACMethod;
            clonedObject.IsSubMethod = this.IsSubMethod;
            clonedObject.InteractionVBContent = this.InteractionVBContent;
            clonedObject.IsAutoenabled = this.IsAutoenabled;
            clonedObject.IsRPCEnabled = this.IsRPCEnabled;
            clonedObject.IsPersistable = this.IsPersistable;
            clonedObject.PWACClassID = this.PWACClassID;
            clonedObject.ContinueByError = this.ContinueByError;
            clonedObject.ValueTypeACClassID = this.ValueTypeACClassID;
            clonedObject.GenericType = this.GenericType;
            clonedObject.ParentACClassMethodID = this.ParentACClassMethodID;
            clonedObject.XMLACMethod = this.XMLACMethod;
            clonedObject.XMLDesign = this.XMLDesign;
            clonedObject.XMLConfig = this.XMLConfig;
            clonedObject.BranchNo = this.BranchNo;
            clonedObject.InsertDate = this.InsertDate;
            clonedObject.InsertName = this.InsertName;
            clonedObject.UpdateDate = this.UpdateDate;
            clonedObject.UpdateName = this.UpdateName;

            return clonedObject;
        }

        /// <summary>
        /// Clone a workflow
        /// </summary>
        /// <returns></returns>
        public ACClassMethod WorkflowClone(string newACIdentifier)
        {
            if (this.ACKindIndex != (Int16)Global.ACKinds.MSWorkflow)
                throw new NotSupportedException("This method is used only for cloning workflow type!");

            // Step 1. Prepare  new objecs
            ACClassMethod clonedWF = (ACClassMethod)this.Clone();
            clonedWF.ACIdentifier = newACIdentifier;
            List<ACClassWF> clonedWFNodes = ACClassWF_ACClassMethod.ToList().Select(x => (ACClassWF)x.Clone()).ToList();
            List<ACClassWFEdge> clonedWFEdges = ACClassWFEdge_ACClassMethod.ToList().Select(x => (ACClassWFEdge)x.Clone()).ToList();

            // Step 2. Prepare new IDs connection data: new IDs, ACIdentifiers and XNames (Nos)
            KeyValuePair<Guid, Guid> newIDWF = new KeyValuePair<Guid, Guid>(clonedWF.ACClassMethodID, Guid.NewGuid());
            List<NewIDWFNodeModel> newIDWFNodes = clonedWFNodes.Select(x =>
               new NewIDWFNodeModel
               {
                   OriginalACClassWFID = x.ACClassWFID,
                   OriginalXName = x.XName,
                   NewID = Guid.NewGuid(),
                   NewXName = Database.Root.NoManager.GetNewNo(Database, typeof(ACClassWF), ACClassWF.NoColumnName, ACClassWF.FormatNewNo, null)
               }).ToList();

            List<NewIDWFEdgeModel> newIDWFEdges = clonedWFEdges.Select(x =>
                 new NewIDWFEdgeModel
                 {
                     OriginalACClassWFEdgeID = x.ACClassWFEdgeID,
                     OriginalACIdentifier = x.ACIdentifier,
                     NewID = Guid.NewGuid(),
                     NewACIdentifier = Database.Root.NoManager.GetNewNo(Database, typeof(ACClassWFEdge), ACClassWFEdge.NoColumnName, ACClassWFEdge.FormatNewNo, null)
                 }).ToList();

            // Step 3. Query for rewrite WFEdge
            var queryWFEdges = clonedWFEdges
                    // Join Edge values
                    .Join(newIDWFEdges,
                    OriginalWFEdge => OriginalWFEdge.ACClassWFEdgeID,
                    NewWFEdgeValues => NewWFEdgeValues.OriginalACClassWFEdgeID,
                    (OriginalWFEdge, NewWFEdgeValues) =>
                        new
                        {
                            OriginalWFEdge = OriginalWFEdge,
                            NewWFEdgeValues = NewWFEdgeValues
                        })
                    // Join WFNode values for SourceACClassWFID
                    .Join(newIDWFNodes,
                    OriginalWFEdge => OriginalWFEdge.OriginalWFEdge.SourceACClassWFID,
                    newIDWFNodesMap => newIDWFNodesMap.OriginalACClassWFID,
                    (OriginalWFEdge, newIDWFNodesMap) =>
                        new
                        {
                            OriginalWFEdge = OriginalWFEdge.OriginalWFEdge,
                            NewWFEdgeValues = OriginalWFEdge.NewWFEdgeValues,
                            SourceWFNodeMap = newIDWFNodesMap
                        })
                    // Join WFNode values for TargetACClassWFID
                    .Join(newIDWFNodes,
                    OriginalWFEdge => OriginalWFEdge.OriginalWFEdge.TargetACClassWFID,
                    newIDWFNodesMap => newIDWFNodesMap.OriginalACClassWFID,
                    (OriginalWFEdge, newIDWFNodesMap) =>
                        new
                        {
                            OriginalWFEdge = OriginalWFEdge.OriginalWFEdge,
                            NewWFEdgeValues = OriginalWFEdge.NewWFEdgeValues,
                            SourceWFNodeMap = OriginalWFEdge.SourceWFNodeMap,
                            TargetWFNodeMap = newIDWFNodesMap
                        })
                        ;

            List<ACClassWFEdge> processedEdges =
                queryWFEdges.Select(x => new ACClassWFEdge()
                {
                    ACClassWFEdgeID = x.NewWFEdgeValues.NewID,
                    ACClassMethodID = newIDWF.Value,
                    XName = x.NewWFEdgeValues.NewACIdentifier,
                    ACIdentifier = x.NewWFEdgeValues.NewACIdentifier,
                    SourceACClassWFID = x.SourceWFNodeMap.NewID,
                    SourceACClassPropertyID = x.OriginalWFEdge.SourceACClassPropertyID,
                    SourceACClassMethodID = x.OriginalWFEdge.SourceACClassMethodID,
                    TargetACClassWFID = x.TargetWFNodeMap.NewID,
                    TargetACClassPropertyID = x.OriginalWFEdge.TargetACClassPropertyID,
                    TargetACClassMethodID = x.OriginalWFEdge.TargetACClassMethodID,
                    ConnectionTypeIndex = x.OriginalWFEdge.ConnectionTypeIndex,
                    BranchNo = x.OriginalWFEdge.BranchNo,
                }).ToList();

            // Step 4. Query for rewrite WF nodes
            var queryWFNodes = clonedWFNodes
                .Join(newIDWFNodes,
                OriginalWFNode => OriginalWFNode.ACClassWFID,
                    newIDWFNodesMap => newIDWFNodesMap.OriginalACClassWFID,
                    (OriginalWFNode, newIDWFNodesMap) =>
                    new
                    {
                        OriginalWFNode = OriginalWFNode,
                        NewWFNodeValues = newIDWFNodesMap
                    })
                    .GroupJoin(newIDWFNodes, originalID => originalID.OriginalWFNode.ParentACClassWFID, parentID => parentID.OriginalACClassWFID,
                    (originalID, parentID) =>
                        new
                        {
                            OriginalWFNode = originalID.OriginalWFNode,
                            NewWFNodeValues = originalID.NewWFNodeValues,
                            ParentNodeMap = parentID
                        })
                    ;

            List<ACClassWF> processedNodes =
                queryWFNodes.Select(x => x.OriginalWFNode.Clone(x.NewWFNodeValues.NewID, x.NewWFNodeValues.NewXName, newIDWF.Value)).ToList();

            // Step 5. Fix ParentACClassWF - for some reason not working propertly
            foreach (var node in processedNodes)
            {
                var nodeMapper = newIDWFNodes.FirstOrDefault(x => x.NewID == node.ACClassWFID);
                var originalItem = clonedWFNodes.FirstOrDefault(x => x.ACClassWFID == nodeMapper.OriginalACClassWFID);
                if (originalItem.ParentACClassWFID != null)
                {
                    var parentMapper = newIDWFNodes.FirstOrDefault(x => x.OriginalACClassWFID == originalItem.ParentACClassWFID);
                    var parentNewNode = processedNodes.FirstOrDefault(x => x.ACClassWFID == parentMapper.NewID);
                    node.ACClassWF1_ParentACClassWF = parentNewNode;
                    node.ParentACClassWFID = parentNewNode.ACClassWFID;
                }
                clonedWF.ACClassWF_ACClassMethod.Add(node);
            }

            foreach (var edge in processedEdges)
                clonedWF.ACClassWFEdge_ACClassMethod.Add(edge);

            // Step 6. XAML process
            string xaml = clonedWF.XMLDesign;
            foreach (var item in newIDWFNodes)
                xaml = xaml.Replace(item.OriginalXName, item.NewXName);

            foreach (var item in newIDWFEdges)
                xaml = xaml.Replace(item.OriginalACIdentifier, item.NewACIdentifier);

            // Step 7. Write new values to root: ACClassMethod (ACKindIndex == (Int16)Global.ACKinds.MSWorkflow)
            clonedWF.ACClassMethodID = newIDWF.Value;
            //NameEnumerator nameEnumerator = new NameEnumerator();
            //clonedWF.ACIdentifier = nameEnumerator.GetNextName(clonedWF.ACIdentifier, Database.ACClassMethod.Select(x => x.ACIdentifier));
            var test = clonedWF.ACConfigKeyACUrl;
            clonedWF.XMLDesign = xaml;

            // Step 8. Clone configs
            //List<ACClassMethodConfig> clonedConfigs =
            //    ACClassMethodConfig_ACClassMethod.ToList().Select(c => new ACClassMethodConfig()
            //    {
            //        ACClassMethodConfigID = Guid.NewGuid(),
            //        ParentACClassMethodConfigID = null,
            //        ACClassMethod = clonedWF,
            //        ACClassWFID = newIDWFNodes.FirstOrDefault(w=>w.OriginalACClassWFID == c.ACClassWFID).NewID,
            //        VBiACClass = c.VBiACClass,
            //        VBiACClassPropertyRelationID = c.VBiACClassPropertyRelationID,
            //        ValueTypeACClass = c.ValueTypeACClass,
            //        KeyACUrl = clonedWF.ACConfigKeyACUrl,
            //        PreConfigACUrl = c.PreConfigACUrl,
            //        LocalConfigACUrl = c.LocalConfigACUrl,
            //        Expression = c.Expression,
            //        Comment = c.Comment,
            //        XMLConfig = c.XMLConfig,
            //        InsertDate = DateTime.Now,
            //        InsertName = Database.Root.CurrentInvokingUser.Initials,
            //        UpdateDate = DateTime.Now,
            //        UpdateName = Database.Root.CurrentInvokingUser.Initials
            //    }).ToList();

            List<ACClassMethodConfig> clonedConfigs = new List<ACClassMethodConfig>();
            foreach (ACClassMethodConfig org in ACClassMethodConfig_ACClassMethod.ToList())
            {
                var newIDWFNode = newIDWFNodes.FirstOrDefault(w => w.OriginalACClassWFID == org.ACClassWFID);
                if (newIDWFNode == null)
                    continue;

                ACClassMethodConfig acMCf = ACClassMethodConfig.NewACObject(this.Database, clonedWF);
                acMCf.ACClassWFID = newIDWFNode.NewID;
                acMCf.VBiACClass = org.VBiACClass;
                acMCf.VBiACClassPropertyRelationID = org.VBiACClassPropertyRelationID;
                acMCf.ValueTypeACClass = org.ValueTypeACClass;
                acMCf.KeyACUrl = clonedWF.ACConfigKeyACUrl;
                acMCf.PreConfigACUrl = org.PreConfigACUrl;
                acMCf.LocalConfigACUrl = org.LocalConfigACUrl;
                acMCf.Expression = org.Expression;
                acMCf.Comment = org.Comment;
                acMCf.XMLConfig = org.XMLConfig;
                acMCf.InsertDate = DateTime.Now;
                acMCf.InsertName = Database.Root.CurrentInvokingUser.Initials;
                acMCf.UpdateDate = DateTime.Now;
                acMCf.UpdateName = Database.Root.CurrentInvokingUser.Initials;
                clonedConfigs.Add(acMCf);
            }

            foreach (var config in clonedConfigs)
                clonedWF.ACClassMethodConfig_ACClassMethod.Add(config);

            return clonedWF;
        }

        #endregion


        #endregion


        #region Overrides VBEntityObject

        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context)
        {
            if (string.IsNullOrEmpty(ACIdentifier))
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = Const.ACIdentifierPrefix,
                    Message = "ACIdentifier is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", Const.ACIdentifierPrefix), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            base.EntityCheckAdded(user, context);
            return null;
        }

        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for changed entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckModified(string user, IACEntityObjectContext context)
        {
            UpdateACMethod();
            base.EntityCheckModified(user, context);
            return null;
        }

        #endregion


        #region Identification and ACURL

        /// <summary>
        /// Primary Key of a Entity in the Database/Table
        /// (Uniqued Identifier of a type in the iPlus-Framework)
        /// </summary>
        public Guid ACTypeID
        {
            get { return ACClassMethodID; }
        }

        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        static public string KeyACIdentifier
        {
            get
            {
                return Const.ACIdentifierPrefix;
            }
        }

        /// <summary>
        /// Returns the ACUrl for calling this method during runtime. (Url + Exclamation + Methodname)
        /// Otherwise a empty string will be returned
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="rootACObject">If null, then a absolute ACUrl will be returned. Else a relative url to the passed object.</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("", "", 9999)]
        public override string GetACUrlComponent(IACObject rootACObject = null)
        {
            if (rootACObject != ParentACObject && ParentACObject is IACObjectEntity)
            {
                IACObjectEntity parent = ParentACObject as IACObjectEntity;
                return parent.GetACUrlComponent(rootACObject) + "!" + ACIdentifier;
            }
            else
            {
                return "!" + ACIdentifier;
            }
        }


        /// <summary>
        /// Returns the Database-ACUrl
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999)]
        public string ACUrl
        {
            get
            {
                return GetACUrl();
            }
        }


        /// <summary>
        /// Returns ACClass, where this method belongs to
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>Reference to ACClass</value>
        [ACPropertyInfo(9999)]
        public override IACObject ParentACObject
        {
            get
            {
                return Safe_ACClass;
            }
        }


        public override string ToString()
        {
            return ACCaption;
        }


        /// <summary>
        /// Returns a related EF-Object which is in a Child-Relationship to this.
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="className">Classname of the Table/EF-Object</param>
        /// <param name="filterValues">Search-Parameters</param>
        /// <returns>A Entity-Object as IACObject</returns>
        public override IACObject GetChildEntityObject(string className, params string[] filterValues)
        {
            if (filterValues.Any())
            {
                switch (className)
                {
                    case ACClassWF.ClassName:

                        using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                        {
                            return ACClassWF_ACClassMethod.Where(c => c.ACClassWF1_ParentACClassWF == null && c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        }
                    case ACClassWFEdge.ClassName:

                        using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                        {
                            return ACClassWFEdge_ACClassMethod.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        }
                }
            }

            return null;
        }


        public String GetACPath(bool first)
        {
            switch (ACKind)
            {
                case Global.ACKinds.PSPropertyExt:
                    return "[" + ACIdentifier + "]";
                case Global.ACKinds.PSProperty:
                    return first ? ACIdentifier : "." + ACIdentifier;
                default:
                    return "";
            }
        }


        /// <summary>
        /// Tests if passed url is used in this workflow
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="localConfigACUrl"></param>
        /// <returns></returns>
        public bool UrlBelongsTo(string localConfigACUrl)
        {
            List<ACClassWF> acClassWFList = null;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                acClassWFList = ACClassWF_ACClassMethod.ToList();
            }

            List<string> listUrls = acClassWFList.Select(x => x.ConfigACUrl).ToList();
            return listUrls.Where(x => localConfigACUrl.StartsWith(x)).Any();
        }

        #endregion


        #region Signature and Assembly-Infos


        /// <summary>
        /// Returns the Signature of a dynamic parameterized Method or constructor
        /// THREAD-SAFE  (QueryLock_1X000)
        /// </summary>
        /// <param name="acUrl"></param>
        /// <param name="attachToObject"></param>
        /// <returns></returns>
        public ACMethod ACUrlACTypeSignature(string acUrl, IACObject attachToObject = null)
        {
            return TypeACSignature();
        }


        /// <summary>
        /// Resturns the dynamic signature of this Method
        /// THREAD-SAFE  (QueryLock_1X000)
        /// </summary>
        /// <returns></returns>
        public ACMethod TypeACSignature()
        {
            ACMethod acMethod = ACMethod.GetVirtualMethod(this.ObjectTypeParent, this.AssemblyMethodName, this.ACIdentifier, true);

            // If acMethod is null, then this virtual method was defined in database an not in source
            if (acMethod == null && !String.IsNullOrWhiteSpace(this.XMLACMethod))
            {
                try
                {
                    acMethod = DeserializeACMethod(this.XMLACMethod);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACClassMethod", "TypeACSignature", String.Format("{0}/{1}/{2}/{3}", this.ACClass.ACIdentifier, this.AssemblyMethodName, this.ACIdentifier, msg));
                }
            }
            if (acMethod == null)
                return null;

            acMethod.ACRequestID = Guid.NewGuid();
            // If there are more PAFunctions from same type inside a Process-Module,
            // Then the ACIdentifiers of the Methods are combined with the ACIdentifier of the Function
            // therfore the ACIdentifier inside a ACMethod must be replaced
            if (this.AttachedFromACClassID.HasValue
                && this.BasedOnACClassMethod != null
                && this.BasedOnACClassMethod.ACClass != null
                && this.BasedOnACClassMethod.ACClass.ACKind == Global.ACKinds.TPAProcessFunction)
            {
                acMethod.ACIdentifier = this.ACIdentifier;
            }
            return acMethod;
        }


        private ACMethod _ACMethod = null;
        /// <summary>
        /// CACHED Signature of this Method
        /// </summary>
        public ACMethod ACMethod
        {
            get
            {
                if (ACKind != Global.ACKinds.MSMethod &&
                    ACKind != Global.ACKinds.MSMethodClient &&
                    ACKind != Global.ACKinds.MSMethodPrePost &&
                    ACKind != Global.ACKinds.MSMethodExt &&
                    ACKind != Global.ACKinds.MSWorkflow &&
                    ACKind != Global.ACKinds.MSMethodFunction)
                    return null;

                else if (_ACMethod != null)
                    return _ACMethod;

                _ACMethod = TypeACSignature();
                if (_ACMethod != null)
                    _ACMethod.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_ACMethod_PropertyChanged);

                return _ACMethod;
            }
            set
            {
                string TMP;

                if (_ACMethod != null)
                    _ACMethod.PropertyChanged -= _ACMethod_PropertyChanged;

                _ACMethod = value;

                if (_ACMethod != null)
                    _ACMethod.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_ACMethod_PropertyChanged);

                TMP = SerializeACMethod(_ACMethod);

                if (XMLACMethod != TMP)
                    XMLACMethod = TMP;
            }
        }


        void _ACMethod_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string xmlACMethod = SerializeACMethod(_ACMethod);
            if (XMLACMethod != xmlACMethod)
                XMLACMethod = xmlACMethod;
        }


        public IEnumerable<ACValue> GetParamList()
        {
            return ACMethod.ParameterValueList.Where(c => c is ACValue).Select(c => c as ACValue);
        }


        public void UpdateParamListFromACClassConstructor(ACClass acClass)
        {
            if (acClass == null)
                return;
            if (ACMethod != null)
            {
                ACMethod clone = ACMethod.Clone() as ACMethod;
                clone.ParameterValueList.Clear();
                clone.ParameterValueList.CloneValues(acClass.ACParameter);
                ACMethod = clone;
            }
        }


        public static string SerializeACMethod(ACMethod acMethod)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(ACMethod), new DataContractSerializerSettings() { DataContractResolver = ACConvert.MyDataContractResolver, MaxItemsInObjectGraph = 99999999, KnownTypes = ACKnownTypes.GetKnownType(), IgnoreExtensionDataObject = true });
            StringBuilder sb1 = new StringBuilder();
            string result = null;
            using (StringWriter sw1 = new StringWriter(sb1))
            using (XmlTextWriter xmlWriter1 = new XmlTextWriter(sw1))
            {
                // Clone ACMethod, because Enumerator-Reference will not be Disposed from DataContractSerializer (Bug) therefore the ReadLock will remain
                //if (acMethod.ParameterValueList is ISafeList)
                acMethod = acMethod.Clone() as ACMethod;
                serializer.WriteObject(xmlWriter1, acMethod);
                result = sw1.ToString();
                //serializer.WriteObject(xmlWriter1, new ACMethod());
            }
            return result;
        }


        public static ACMethod DeserializeACMethod(string acMethodXML)
        {
            using (StringReader ms = new StringReader(acMethodXML))
            using (XmlTextReader xmlReader = new XmlTextReader(ms))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ACMethod), new DataContractSerializerSettings() { KnownTypes = ACKnownTypes.GetKnownType(), MaxItemsInObjectGraph = 99999999, IgnoreExtensionDataObject = true, DataContractResolver = ACConvert.MyDataContractResolver });
                ACMethod acMethod = (ACMethod)serializer.ReadObject(xmlReader);
                // Clone ACMethod, because Enumerator-Reference will not be Disposed from DataContractSerializer (Bug) therefore the ReadLock will remain
                //if (acMethod.ParameterValueList is ISafeList)
                acMethod = acMethod.Clone() as ACMethod;
                return acMethod;
            }
        }


        /// <summary>
        /// Aktualisieren des Method
        /// (Assembly-Methoden werden vom ACClassManager analysiert, Andere Methodenarten werden nicht unterstützt)
        /// </summary>
        internal void UpdateACMethod()
        {
            ACMethod acMethod;
            if (ACKind != Global.ACKinds.MSMethodExt)
            {

                acMethod = ACMethod;
                // Falls siche der ACIdentifier geändert hat, dann auch in ACMethod ändern
                if (acMethod != null && acMethod.ACIdentifier != this.ACIdentifier)
                {
                    acMethod.ACIdentifier = this.ACIdentifier;
                    ACMethod = acMethod;
                }

                return;
            }

            acMethod = new ACMethod(ACIdentifier);

            if (!String.IsNullOrEmpty(Sourcecode))
            {

                string methodSignSearch = " " + this.ACIdentifier + "(";
                int found = Sourcecode.IndexOf(methodSignSearch);
                if (found > 0)
                {
                    // Rückgabe-Datentyp ermitteln
                    string resultType = "";
                    for (int i = found - 1; i > 0; i--)
                    {
                        var c = Sourcecode[i];
                        if (c == ' ' || c == '\n' || c == '\t' || c == '\r')
                            break;

                        resultType = c + resultType;
                    }
                    Type valueType = TypeAnalyser.GetTypeByShortTypeName(resultType, Database.GlobalDatabase);
                    acMethod.ResultValueList.Add(new ACValue("result", valueType));

                    // Eingabe-Parameter ermitteln
                    int end = Sourcecode.IndexOf(")", found);
                    if (end > 0 && found < end)
                    {
                        found = found + methodSignSearch.Length;
                        string parameters = Sourcecode.Substring(found + 1, end - found - 1).Trim();
                        if (!string.IsNullOrEmpty(parameters))
                        {
                            string[] paramList = parameters.Split(',');
                            bool first = true;
                            foreach (var param in paramList)
                            {
                                if (first)
                                {
                                    first = false;
                                    continue;
                                }
                                string[] oneParam = param.Trim().Split(' ');
                                if (oneParam.Count() == 2)
                                {
                                    valueType = TypeAnalyser.GetTypeByShortTypeName(oneParam[0], Database.GlobalDatabase);
                                    if (valueType != null)
                                    {
                                        ACValue acParameterDefinition = new ACValue(oneParam[1], valueType);
                                        acParameterDefinition.SetDefaultValue();
                                        acMethod.ParameterValueList.Add(acParameterDefinition);
                                    }
                                }
                            }
                        }
                    }
                }

            }

            bool isParameterACMethod = (acMethod.ParameterValueList.Count() == 1 && typeof(ACMethod).IsAssignableFrom(acMethod.ParameterValueList.First().ValueTypeACClass.ObjectType));

            if (IsParameterACMethod != isParameterACMethod)
            {
                IsParameterACMethod = isParameterACMethod;

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    foreach (var acClassMethodChild in this.ACClassMethod_ParentACClassMethod)
                    {
                        acClassMethodChild.IsParameterACMethod = isParameterACMethod;
                    }
                }
            }

            string xmlACMethod = ACClassMethod.SerializeACMethod(acMethod);
            if (XMLACMethod != xmlACMethod)
            {
                XMLACMethod = xmlACMethod;
                if (IsParameterACMethod != isParameterACMethod)
                {
                    IsParameterACMethod = isParameterACMethod;

                    using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                    {
                        foreach (var acClassMethodChild in this.ACClassMethod_ParentACClassMethod)
                        {
                            acClassMethodChild.IsParameterACMethod = isParameterACMethod;
                        }
                    }
                }
            }
        }


        internal void UpdateACMethodWorkflow()
        {
            if (ACKind != Global.ACKinds.MSWorkflow)
                return;

            ACMethod acMethod = new ACMethod(ACIdentifier);

            ACValue acParameterDefinition = new ACValue(ACProgram.ClassName, typeof(Guid), null, Global.ParamOption.Required);
            acMethod.ParameterValueList.Add(acParameterDefinition);

            string xmlACMethod = ACClassMethod.SerializeACMethod(acMethod);
            if (XMLACMethod != xmlACMethod)
            {
                XMLACMethod = xmlACMethod;
            }
        }


        string _AssemblyMethodName = null;
        /// <summary>
        /// Returns the methodname if method is hardocded in assembly
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        public string AssemblyMethodName
        {
            get
            {
                using (ACMonitor.Lock(_10020_LockValue))
                {
                    if (!String.IsNullOrEmpty(_AssemblyMethodName))
                        return _AssemblyMethodName;
                }

                string assemblyMethodName;

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    assemblyMethodName = AssemblyMethodNameLocked;
                }
                using (ACMonitor.Lock(_10020_LockValue))
                {
                    _AssemblyMethodName = assemblyMethodName;
                }

                return assemblyMethodName;
            }
        }


        /// <summary>
        /// Gets the assembly method name locked.
        /// </summary>
        /// <value>The assembly method name locked.</value>
        private string AssemblyMethodNameLocked
        {
            get
            {
                if (this.ACClassMethod1_ParentACClassMethod != null)
                    return ACClassMethod1_ParentACClassMethod.AssemblyMethodNameLocked;
                return ACIdentifier;
            }
        }



        private bool _BasedOnACClassMethodChecked = false;
        private ACClassMethod _BasedOnACClassMethod = null;
        /// <summary>
        /// THREADS-SAFE but Cached!
        /// </summary>
        public ACClassMethod BasedOnACClassMethod
        {
            get
            {
                using (ACMonitor.Lock(_10020_LockValue))
                {
                    if (_BasedOnACClassMethodChecked)
                        return _BasedOnACClassMethod;
                }

                ACClassMethod basedOnACClassMethod = null;
                if (ACClassMethod1_ParentACClassMethodReference.IsLoaded)
                    basedOnACClassMethod = ACClassMethod1_ParentACClassMethodReference.Value;
                if (basedOnACClassMethod == null)
                {
                    using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                    {
                        basedOnACClassMethod = ACClassMethod1_ParentACClassMethod;
                    }
                }
                using (ACMonitor.Lock(_10020_LockValue))
                {
                    _BasedOnACClassMethodChecked = true;
                    _BasedOnACClassMethod = basedOnACClassMethod;
                }

                return basedOnACClassMethod;
            }
        }


        /// <summary>
        /// Methodtype-Name
        /// </summary>
        [ACPropertyInfo(9999, "", "en{'Methodtype'}de{'Methodentyp'}")]
        public string MethodCaption
        {
            get
            {
                if (ACGroup == Const.ACState)
                    return "Statemethod";

                switch (ACKind)
                {
                    case Global.ACKinds.MSMethod:
                    case Global.ACKinds.MSMethodClient:
                        return "Assemblymethod";
                    case Global.ACKinds.MSMethodExt:
                    case Global.ACKinds.MSMethodExtClient:
                    case Global.ACKinds.MSMethodExtTrigger:
                        return "Scriptmethod";
                    case Global.ACKinds.MSWorkflow:
                        if (this.RootWFNode != null)
                            return ((ACClassWF)this.RootWFNode).PWACClass.ACCaption;
                        else
                            return "Workflow";
                    case Global.ACKinds.MSMethodFunction:
                        return "Submethod";
                    default:
                        return "Method";
                }
            }
        }

        #endregion


        #region Type-Informations

        /// <summary>
        /// Returns the category of this method
        /// </summary>
        public Global.ACKinds ACKind
        {
            get
            {
                return (Global.ACKinds)ACKindIndex;
            }
            set
            {
                ACKindIndex = (short)value;
            }
        }


        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(2, "", "en{'Description'}de{'Bezeichnung'}")]
        public override string ACCaption
        {
            get
            {
                return Translator.GetTranslation(ACIdentifier, ACCaptionTranslation);
            }
            set
            {
                ACCaptionTranslation = Translator.SetTranslation(ACCaptionTranslation, value);
                OnPropertyChanged(Const.ACCaptionPrefix);
            }
        }

        [ACPropertyInfo(3, "", "en{'Description 2'}de{'Bezeichnung 2'}")]
        public string ACCaptionAttached
        {
            get
            {
                string text = Translator.GetTranslation(ACIdentifier, ACCaptionTranslation);
                if (AttachedFromACClassID.HasValue && AttachedFromACClass != null
                    && BasedOnACClassMethod != null && BasedOnACClassMethod.ACIdentifier != this.ACIdentifier)
                    text = String.Format("{0} {1} {2}", text, ACUrlHelper.AttachedMethodIDConcatenator, AttachedFromACClass.ACCaption);
                return text;
            }
        }

        /// <summary>
        /// Tooltip
        /// </summary>
        [ACPropertyInfo(9999)]
        public string Tooltip
        {
            get
            {
                string tooltip = ACCaptionAttached;
                if (!string.IsNullOrEmpty(Comment))
                {
                    tooltip += "\n" + Comment;
                }
                return tooltip;
            }
        }


        /// <summary>
        /// Method for getting the translated text from ACCaptionTranslation
        /// </summary>
        /// <param name="VBLanguageCode">I18N-code</param>
        /// <returns>Translated text</returns>
        public string GetTranslation(string VBLanguageCode)
        {
            return Translator.GetTranslation(ACIdentifier, ACCaptionTranslation, VBLanguageCode);
        }


        /// <summary>
        /// Returns the .NET-Type (If return value is a generic it returns the inner type)
        /// </summary>
        public Type ObjectType
        {
            get
            {
                return Safe_ValueTypeACClass.ObjectType;
            }
        }


        /// <summary>
        /// Returns the .NET-Type (If return value is a generic it returns the outer+inner type)
        /// </summary>
        public Type ObjectFullType
        {
            get
            {
                return ObjectType;
            }
        }


        /// <summary>
        /// Returns the .NET-Type  of the parent class in a composition tree
        /// </summary>
        public Type ObjectTypeParent
        {
            get
            {
                return Safe_ACClass.ObjectType;
            }
        }


        /// <summary>
        /// Gets the member.
        /// </summary>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <param name="forceRefreshFromDB"></param>
        /// <returns>IACType.</returns>
        public IACType GetMember(string acIdentifier, bool forceRefreshFromDB = false)
        {
            return null;
        }


        /// <summary>
        /// Returns all Properties of a Entity-Object which are visible for presentation
        /// THREAD-SAFE while using QueryLock_1X000
        /// </summary>
        /// <param name="maxColumns">The max columns.</param>
        /// <param name="acColumns">The ac columns.</param>
        /// <returns>List{ACColumnItem}.</returns>
        public List<ACColumnItem> GetColumns(int maxColumns = 9999, string acColumns = null)
        {
            if (ObjectType == null)
                return null;
            return Safe_ValueTypeACClass.GetColumns(maxColumns, acColumns);
        }
        #endregion


        #region Workflow (IACWorkflowContext & IACObjectDesignWF)

        /// <summary>
        /// Returns the Root-Node (ACClassWF) of this Workflow-Method
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IACWorkflowNode RootWFNode
        {
            get
            {
                return MethodWFList.Where(c => c.IsRootWFNode(this)).FirstOrDefault();
                //using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                //{
                //    return this.ACClassWF_ACClassMethod.Where(c => c.IsRootWFNode(this)).FirstOrDefault();
                //}
            }
        }


        /// <summary>
        /// Returns the Type (ACClass) of the Root-Workflownode
        /// </summary>
        [ACPropertyInfo(9999)]
        public ACClass WorkflowTypeACClass
        {
            get
            {
                ACClass workflowTypeACClass = null;
                var acClassWF = RootWFNode as ACClassWF;
                if (acClassWF != null)
                {
                    if (acClassWF.PWACClassReference.IsLoaded)
                        workflowTypeACClass = acClassWF.PWACClassReference.Value;
                    if (workflowTypeACClass == null)
                    {
                        using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                        {
                            workflowTypeACClass = acClassWF.PWACClass;
                        }
                    }
                }
                else
                {
                    if (PWACClassReference.IsLoaded)
                        workflowTypeACClass = PWACClassReference.Value;
                    if (workflowTypeACClass == null)
                    {
                        using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                        {
                            workflowTypeACClass = PWACClass;
                        }
                    }
                }
                return workflowTypeACClass;
            }
        }


        /// <summary>
        /// Returns all Nodes in this Workflow
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<ACClassWF> MethodWFList
        {
            get
            {
                if (this.EntityState != EntityState.Added && !ACClassWF_ACClassMethod.IsLoaded)
                {
                    using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                    {
                        ACClassWF_ACClassMethod.Load();
                    }
                }
                return ACClassWF_ACClassMethod.ToList(); // ToList, damit Query Thread-Safe
            }
        }


        /// <summary>
        /// Returns all Nodes in this Workflow
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<IACWorkflowNode> AllWFNodes
        {
            get
            {
                return MethodWFList;
            }
        }


        /// <summary>
        /// Returns als Edges in this workflow
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<IACWorkflowEdge> AllWFEdges
        {
            get
            {

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return ACClassWFEdge_ACClassMethod.Select(c => c as IACWorkflowEdge);
                }
            }
        }


        /// <summary>
        /// Adds a new node to this workflow
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="node"></param>
        public void AddNode(IACWorkflowNode node)
        {
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                if (node is ACClassWF)
                {
                    ACClassWF acClassWF = node as ACClassWF;
                    acClassWF.ACClassMethod = this;
                    ACClassWF_ACClassMethod.Add(acClassWF);
                }
            }
        }


        /// <summary>
        /// Removes a node from this workflow
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="database"></param>
        /// <param name="node"></param>
        /// <param name="configACUrl"></param>
        public void DeleteNode(IACEntityObjectContext database, IACWorkflowNode node, string configACUrl)
        {
            ACClassWF acClassWF = node as ACClassWF;
            if (acClassWF is null)
                return;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                DeleteInnerWFs(database, acClassWF);

                // @aagincic -> Delete config is removed to up instance where PWOfflineNodeMethod is present - hold info about localConfigACUrl

                ACClassWF_ACClassMethod.Remove(acClassWF);
                database.Remove(acClassWF);
            }
        }


        protected void DeleteInnerWFs(IACEntityObjectContext database, IACWorkflowNode acVisualWF)
        {
            if (!(acVisualWF is ACClassWF))
                return;
            ACClassWF acClassWF = acVisualWF as ACClassWF;
            // Bei einem Gruppenknoten sind auch die Kindelemente zu löschen
            if (acClassWF.GetChildWFNodes(this).Any())
            {
                foreach (var childClass in acClassWF.GetChildWFNodes(this).ToList())
                {
                    foreach (var edge1 in childClass.GetIncomingWFEdges(this).ToList())
                    {
                        DeleteEdge(database, edge1);
                    }
                    foreach (var edge2 in childClass.GetOutgoingWFEdges(this).ToList())
                    {
                        DeleteEdge(database, edge2);
                    }
                    DeleteNode(database, childClass as IACWorkflowNode, null);
                }
            }
        }


        /// <summary>
        /// Adds a Edge
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="edge"></param>
        public void AddEdge(IACWorkflowEdge edge)
        {
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                if (edge is ACClassWFEdge)
                {
                    ACClassWFEdge acClassWFEdge = edge as ACClassWFEdge;
                    acClassWFEdge.ACClassMethod = this;
                    ACClassWFEdge_ACClassMethod.Add(acClassWFEdge);
                }
            }
        }


        /// <summary>
        /// Removes a Edge
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="edge"></param>
        public void DeleteEdge(IACEntityObjectContext database, IACWorkflowEdge edge)
        {
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                if (edge is ACClassWFEdge)
                {
                    ACClassWFEdge acClassWFEdge = edge as ACClassWFEdge;
                    if (acClassWFEdge.ACClassMethod == this)
                    {
                        ACClassWFEdge_ACClassMethod.Remove(acClassWFEdge);
                        database.Remove(acClassWFEdge);
                    }
                }
            }
        }


        /// <summary>
        /// Creates a New Edge
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public IACWorkflowEdge CreateNewEdge(IACEntityObjectContext database)
        {
            string secondaryKey = Database.Root.NoManager.GetNewNo(database, typeof(ACClassWFEdge), ACClassWFEdge.NoColumnName, ACClassWFEdge.FormatNewNo, null);
            ACClassWFEdge edge = ACClassWFEdge.NewACObject(database as Database, this, secondaryKey);
            return edge;
        }


#endregion


#region ACClassDesign

        /// <summary>
        /// Returns all Designs over complete class hierarchy including overridden Designs
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>ACClassDesign List</value>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<ACClassDesign> Designs
        {
            get
            {
                if (IsCommand || IsInteraction)
                    return Safe_ValueTypeACClass.Designs;
                return null;
            }
        }


        /// <summary>
        /// Returns the first Design which matches the identifier over complete class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="acIdentifier"></param>
        /// <param name="forceRefreshFromDB"></param>
        /// <returns></returns>
        public ACClassDesign GetDesign(string acIdentifier, bool forceRefreshFromDB = false)
        {
            return Designs.Where(c => c.ACIdentifier == acIdentifier && c.ACKindIndex == (short)Global.ACKinds.DSDesignLayout).FirstOrDefault();
        }


        /// <summary>
        /// Returns the first Design which matches the criteria
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="acObject"></param>
        /// <param name="acUsage"></param>
        /// <param name="acKind"></param>
        /// <param name="vbDesignName"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public ACClassDesign GetDesign(IACObject acObject, Global.ACUsages acUsage, Global.ACKinds acKind, string vbDesignName = "", MsgWithDetails msg = null)
        {

            ACClass acClass = (acObject as ACClassMethod).ACClass;

            var query = GetConfigListOfType(acClass).Where(c => c.LocalConfigACUrl == "Design_" + acUsage.ToString());
            if (query.Any())
            {
                IACConfig acClassConfig = query.First();
                ACComposition acComposition = acClassConfig[Const.Value] as ACComposition;
                return acComposition.GetComposition(this.Database) as ACClassDesign;
            }

            if (acUsage == Global.ACUsages.DUIcon)
            {
                return Database.Root.Environment.GetIcon(this.ACIdentifier);
            }
            if (acUsage == Global.ACUsages.DUBitmap)
            {
                return Database.Root.Environment.GetBitmap(this.ACIdentifier);
            }

            if (IsCommand || IsInteraction)
                return Safe_ValueTypeACClass.GetDesign(ValueTypeACClass, acUsage, acKind, vbDesignName);
            return null;
        }



        private string IconACUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the Database-ACUrl for a Icon-Desing
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <returns></returns>
        public string GetIconACUrl()
        {
            if (IconACUrl != null)
                return IconACUrl;
            else
            {
                ACClassDesign acClassDesign = GetIconDesign(this.ACClass);
                if (acClassDesign != null && acClassDesign.DesignBinary != null)
                    IconACUrl = acClassDesign.GetACUrl();
                else
                    IconACUrl = "";
            }
            return IconACUrl;
        }


        private ACClassDesign GetIconDesign(ACClass acClass)
        {
            ACClassDesign acClassDesign = acClass.Designs.FirstOrDefault(c => c.ACUsageIndex == (short)Global.ACUsages.DUIcon && c.ACIdentifier == "Icon" + ACIdentifier);
            if (acClassDesign == null && acClass.ACClass1_BasedOnACClass != null)
            {
                acClassDesign = GetIconDesign(acClass.ACClass1_BasedOnACClass);
            }
            return acClassDesign;
        }
#endregion


#region Misc Properties and Methods

        bool bRefreshConfig = false;
        partial void OnXMLConfigChanging(global::System.String value)
        {
            bRefreshConfig = false;
            if (!(String.IsNullOrEmpty(value) && String.IsNullOrEmpty(XMLConfig)) && value != XMLConfig)
                bRefreshConfig = true;
        }

        partial void OnXMLConfigChanged()
        {
            if (bRefreshConfig)
                ACProperties.Refresh();
        }


        internal ACClass Safe_ACClass
        {
            get
            {
                ACClass sc = null;
                if (ACClassReference.IsLoaded)
                    sc = ACClassReference.Value;
                if (sc == null)
                {
                    using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                    {
                        sc = this.ACClass;
                    }
                }
                return sc;
            }
        }


        internal ACClass Safe_ValueTypeACClass
        {
            get
            {
                ACClass sc = null;
                if (ValueTypeACClassReference.IsLoaded)
                    sc = ValueTypeACClassReference.Value;
                if (sc == null)
                {
                    using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                    {
                        sc = this.ValueTypeACClass;
                    }
                }
                return sc;
            }
        }

        /// <summary>
        /// This flag marks the method which will be executed from a VBVisual's double click. It's only enabled for a static and interaction methods. 
        /// </summary>
        [ACPropertyInfo(999, "", "en{'Execute by mouse double click'}de{'Ausführen per Maus-Doppelklick'}", "", true)]
        public bool ExecuteByMouseDoubleClick
        {
            get => ExecuteByDoubleClick;
            set
            {
                ExecuteByDoubleClick = value;
                if (value)
                {
                    ACClassMethod methodToRemoveExcByDoubleClick = Safe_ACClass.Methods.FirstOrDefault(c => c.ExecuteByDoubleClick && c != this);
                    if (methodToRemoveExcByDoubleClick != null)
                        methodToRemoveExcByDoubleClick.ExecuteByDoubleClick = false;
                }
            }
        }

        public DateTime? LastReadUpdateACClassWF
        {
            get; set;
        }

        public bool MustRefreshACClassWF
        {
            get
            {
                return !LastReadUpdateACClassWF.HasValue || UpdateDate != LastReadUpdateACClassWF;
            }
        }

        public void SetACClassWFRefreshed()
        {
            LastReadUpdateACClassWF = UpdateDate;
        }

#endregion


#region Configuration

        private string configStoreName;
        public string ConfigStoreName
        {
            get
            {
                if (configStoreName == null)
                {
                    ACClassInfo acClassInfo = (ACClassInfo)GetType().GetCustomAttributes(typeof(ACClassInfo), false)[0];
                    string caption = Translator.GetTranslation(acClassInfo.ACCaptionTranslation);
                    configStoreName = string.Format("{0}({1})", caption, ACIdentifier);
                    configStoreName = ACCaption;
                }
                return configStoreName;
            }
        }

        /// <summary>
        /// ACConfigKeyACUrl returns the relative Url to the "main table" in group a group of semantically related tables.
        /// This property is used when NewACConfig() is called. NewACConfig() creates a new IACConfig-Instance and set the IACConfig.KeyACUrl-Property with this ACConfigKeyACUrl.
        /// </summary>
        public string ACConfigKeyACUrl
        {
            get
            {
                return ".\\ACClassMethod(" + ACIdentifier + ")";
            }
        }


        /// <summary>
        /// Returns all configuration-entries for this method
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="acObject"></param>
        /// <returns></returns>
        public IEnumerable<IACConfig> GetConfigListOfType(IACObjectEntity acObject = null)
        {
            ACClassMethod acClassMethod = acObject as ACClassMethod;
            if (acClassMethod == null)
                acClassMethod = this;
            if (acClassMethod != this)
            {
                using (ACMonitor.Lock(acClassMethod.Database.QueryLock_1X000))
                {
                    if (acClassMethod.ACClassMethodConfig_ACClassMethod.IsLoaded)
                    {
                        acClassMethod.ACClassMethodConfig_ACClassMethod.AutoRefresh(acClassMethod.Database);
                        acClassMethod.ACClassMethodConfig_ACClassMethod.AutoLoad(acClassMethod.Database);
                    }
                    return acClassMethod.ACClassMethodConfig_ACClassMethod.ToList().Select(x => (IACConfig)x); //.Where(c => c.KeyACUrl == ACConfigKeyACUrl); Without ACConfigKeyACUrl-Filter because ACClassMethodConfig is a seperate table 
                }
            }

            using (ACMonitor.Lock(_10020_LockValue))
            {
                if (_ACConfigListCache != null)
                    return _ACConfigListCache;
            }

            SafeList<IACConfig> newSafeList = new SafeList<IACConfig>();
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                if (this.ACClassMethodConfig_ACClassMethod.IsLoaded)
                {
                    this.ACClassMethodConfig_ACClassMethod.AutoRefresh(this.Database);
                    this.ACClassMethodConfig_ACClassMethod.AutoLoad(this.Database);
                }
                newSafeList = new SafeList<IACConfig>(this.ACClassMethodConfig_ACClassMethod); //.Where(c => c.KeyACUrl == ACConfigKeyACUrl); Without ACConfigKeyACUrl-Filter because ACClassMethodConfig is a seperate table 
            }
            using (ACMonitor.Lock(_10020_LockValue))
            {
                _ACConfigListCache = newSafeList;
                return _ACConfigListCache;
            }
        }

        /// <summary>
        /// Creates and adds a new IACConfig-Entry to ConfigItemsSource.
        /// The implementing class creates a new entity object an add it to its "own Configuration-Table".
        /// It sets automatically the IACConfig.KeyACUrl-Property with this ACConfigKeyACUrl.
        /// </summary>
        /// <param name="acObject">Optional: Reference to another Entity-Object that should be related for this new configuration entry.</param>
        /// <param name="valueTypeACClass">The iPlus-Type of the "Value"-Property.</param>
        /// <param name="localConfigACUrl"></param>
        /// <returns>IACConfig as a new entry</returns>
        public IACConfig NewACConfig(IACObjectEntity acObject = null, gip.core.datamodel.ACClass valueTypeACClass = null, string localConfigACUrl = null)
        {
            ACClassMethodConfig acConfig = null;
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                acConfig = ACClassMethodConfig.NewACObject(this.Database, this);
                acConfig.KeyACUrl = ACConfigKeyACUrl;
                acConfig.LocalConfigACUrl = localConfigACUrl;
                acConfig.ValueTypeACClass = valueTypeACClass;
                this.ACClassMethodConfig_ACClassMethod.Add(acConfig);
                if (acObject != null && acObject is ACClassWF)
                    acConfig.ACClassWFID = ((ACClassWF)acObject).ACClassWFID;
            }
            ACConfigListCache.Add(acConfig);
            return acConfig;
        }


        /// <summary>Removes a configuration from ConfigurationEntries and the database-context.</summary>
        /// <param name="acObject">Entry as IACConfig</param>
        public void RemoveACConfig(IACConfig acObject)
        {
            ACClassMethodConfig acConfig = acObject as ACClassMethodConfig;
            if (acConfig == null)
                return;
            if (ACConfigListCache.Contains(acConfig))
                ACConfigListCache.Remove(acConfig);
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                this.ACClassMethodConfig_ACClassMethod.Remove(acConfig);
                if (acConfig.EntityState != EntityState.Detached)
                    acConfig.DeleteACObject(this.Database, false);
            }
        }

        /// <summary>
        /// Deletes all IACConfig-Entries in the Database-Context as well as in ConfigurationEntries.
        /// </summary>
        public void DeleteAllConfig()
        {
            if (!GetConfigListOfType().Any())
                return;
            ClearCacheOfConfigurationEntries();
            var list = GetConfigListOfType().ToArray();
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                foreach (var acConfig in list)
                {
                    (acConfig as ACClassMethodConfig).DeleteACObject(this.Database, false);
                }
            }
            ClearCacheOfConfigurationEntries();
        }

        public decimal OverridingOrder { get; set; }

        /// <summary>
        /// A thread-safe and cached list of Configuration-Values of type IACConfig.
        /// </summary>
        public IEnumerable<IACConfig> ConfigurationEntries
        {
            get
            {
                return ACConfigListCache;
            }
        }

        private SafeList<IACConfig> _ACConfigListCache = null;
        private SafeList<IACConfig> ACConfigListCache
        {
            get
            {
                using (ACMonitor.Lock(_10020_LockValue))
                {
                    if (_ACConfigListCache != null)
                        return _ACConfigListCache;
                }
                GetConfigListOfType();
                return _ACConfigListCache;
            }
        }


        /// <summary>Clears the cache of configuration entries. (ConfigurationEntries)
        /// Re-accessing the ConfigurationEntries property rereads all configuration entries from the database.</summary>
        public void ClearCacheOfConfigurationEntries()
        {
            using (ACMonitor.Lock(_10020_LockValue))
            {
                _ACConfigListCache = null;
            }
        }

        /// <summary>
        /// Checks if cached configuration entries are loaded from database successfully
        /// </summary>
        public bool ValidateConfigurationEntriesWithDB(ConfigEntriesValidationMode mode = ConfigEntriesValidationMode.AnyCheck)
        {
            if (mode == ConfigEntriesValidationMode.AnyCheck)
            {
                if (ConfigurationEntries.Any())
                    return true;
            }
            using (Database database = new Database())
            {
                var query = database.ACClassMethodConfig.Where(c => c.ACClassMethodID == ACClassMethodID);
                if (mode == ConfigEntriesValidationMode.AnyCheck)
                {
                    if (query.Any())
                        return false;
                }
                else if (mode == ConfigEntriesValidationMode.CompareCount
                         || mode == ConfigEntriesValidationMode.CompareContent)
                    return query.Count() == ConfigurationEntries.Count();
            }
            return true;
        }

#endregion

    }

    public class NewIDWFNodeModel
    {
        public Guid OriginalACClassWFID { get; set; }
        public string OriginalXName { get; set; }
        public Guid NewID { get; set; }
        public string NewXName { get; set; }
    }

    public class NewIDWFEdgeModel
    {
        public Guid OriginalACClassWFEdgeID { get; set; }
        public string OriginalACIdentifier { get; set; }
        public Guid NewID { get; set; }
        public string NewACIdentifier { get; set; }
    }
}


