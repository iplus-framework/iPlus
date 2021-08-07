// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-14-2013
// ***********************************************************************
// <copyright file="ACClass.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Data.Objects;

namespace gip.core.datamodel
{
    /// <summary>
    ///   <para>The table ACClass contains meta information for a class that is necessary for iPlus runtime to be able to instantiate this class. This includes the following properties/table fields:</para>
    ///   <para>ParentACClass (ParentACClassID): Relation Relation to parent class. This relationship defines the tree structure of an application is defined (Composition).</para>
    ///   <para>BasedOnACClass (BasedOnACClassID): Relation to the base class from which this class is derived. This relationship defines the inheritance of a class.</para>
    ///   <para>AssemblyQualifiedName: Specifies which .NET-Type to instantiate using Reflection. </para>
    ///   <para>ACClass inheritance relationships are often virtual. This means that although the same .NET type is created, it is two different types or instances from the point of view of the iPlus framework. Each instance defined in the application project is automatically a new ACClass that is virtually inherited. However, from a .NET point of view, the same .NET type is instantiated if the AssemblyQualifiedName is the same for two different instances.</para>
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Class'}de{'Klasse'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true, "", "BSOiPlusStudio")]
    [ACPropertyEntity(1, Const.ACIdentifierPrefix, "en{'Classname/ID'}de{'Klassenname/ID'}", "", "", true)]
    [ACPropertyEntity(3, "ACIdentifierKey", "en{'Key'}de{'Schlüssel'}", "", "", true)]
    [ACPropertyEntity(4, Const.ACKindIndex, "en{'Classkind'}de{'Klassenart'}", typeof(Global.ACKinds), Const.ContextDatabaseIPlus + "\\ACKindCLList", "", true)]
    [ACPropertyEntity(5, "IsRightmanagement", "en{'Rightmanagement'}de{'Rechteverwaltung'}", "", "", true)]
    [ACPropertyEntity(9999, "ACCaptionTranslation", "en{'Translation'}de{'Übersetzung'}", "", "", true)]
    [ACPropertyEntity(9999, "AssemblyQualifiedName", "en{'Assembly-qualified name'}de{'Assembly qualifizierter Name'}", "", "", true)]
    [ACPropertyEntity(9999, "IsMultiInstance", "en{'Multiinstance'}de{'Mehrf.Instanzen'}", "", "", true)]
    [ACPropertyEntity(9999, ACPackage.ClassName, "en{'Package'}de{'Paket'}", Const.ContextDatabaseIPlus + "\\" + ACPackage.ClassName, "", true)]
    [ACPropertyEntity(9999, "ACStartTypeIndex", "en{'Starttype'}de{'ACStarttyp'}", typeof(Global.ACStartTypes), Const.ContextDatabaseIPlus + "\\ACStartTypeList", "", true)]
    [ACPropertyEntity(9999, "ACStorableTypeIndex", "en{'Persistence'}de{'Persistierung'}", typeof(Global.ACStorableTypes), Const.ContextDatabaseIPlus + "\\ACStorableTypeList", "", true)]
    [ACPropertyEntity(9999, ACProject.ClassName, "en{'Project'}de{'Projekt'}", Const.ContextDatabaseIPlus + "\\" + ACProject.ClassName, "", true)]
    [ACPropertyEntity(9999, "Comment", "en{'Comment'}de{'Bemerkung'}", "", "", true)]
    [ACPropertyEntity(9999, "IsAssembly", "en{'Assemblyclass'}de{'Assemblyklasse'}", "", "", true)]
    [ACPropertyEntity(9999, "IsAbstract", "en{'Abstract'}de{'Abstrakt'}", "", "Nicht instanziierbare Klasse")]
    [ACPropertyEntity(9999, "IsAutostart", "en{'Autostart'}de{'Autostart'}", "", "", true)]
    [ACPropertyEntity(9999, "IsStatic", "en{'Static'}de{'Statisch'}", "", "", true)]
    [ACPropertyEntity(9999, "ACClass1_PWACClass", "en{'Workflowclass'}de{'Workflowklasse'}", Const.ContextDatabaseIPlus + "\\PWClassList", "", true)]
    [ACPropertyEntity(9999, "ACClass1_PWMethodACClass", "en{'Workflowmethodtype'}de{'Workflowmethodentyp'}", Const.ContextDatabaseIPlus + "\\WorkflowTypeMethodACClassList", "", true)]
    [ACPropertyEntity(9999, "ACClass1_BasedOnACClass", "en{'Baseclass'}de{'Basisklasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(9999, "SortIndex", "en{'Sortindex'}de{'Sortierung'}", "", "", true)]
    [ACPropertyEntity(9999, "ACClass1_ParentACClass", "en{'Parent Class'}de{'Elternklasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(9999, "ChangeLogMax", "en{'Max change logs'}de{'Max Änderungsprotokolle'}", "", "", true)]
    [ACDeleteAction("ACClassComposition_CompositionACClass", Global.DeleteAction.CascadeManual)]
    [ACDeleteAction("ACClassPropertyValue_ACClass", Global.DeleteAction.CascadeManual)]
    [ACDeleteAction("ACClass_ParentACClass", Global.DeleteAction.CascadeManual)]
    [ACDeleteAction("ACClassPropertyRelation_SourceACClass", Global.DeleteAction.CascadeManual)]
    [ACDeleteAction("ACClassPropertyRelation_TargetACClass", Global.DeleteAction.CascadeManual)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClass.ClassName, "en{'Class'}de{'Klasse'}", typeof(ACClass), gip.core.datamodel.ACClass.ClassName, "ACCaptionTranslation,ACIdentifier", Const.ACIdentifierPrefix, new object[]
        {
                new object[] {Const.QueryPrefix + ACClassProperty.ClassName, "en{'Property'}de{'Eigenschaft'}", typeof(ACClassProperty), ACClassProperty.ClassName + "_" + ACClass.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix},
                new object[] {Const.QueryPrefix + ACClassMethod.ClassName, "en{'Method'}de{'Methode'}", typeof(ACClassMethod), ACClassMethod.ClassName + "_" + ACClass.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix, new object[]
                    {
                        new object[] {Const.QueryPrefix + ACClassWF.ClassName, "en{'Workflow'}de{'Workflow'}", typeof(ACClassWF), ACClassWF.ClassName + "_" + ACClassMethod.ClassName, "ACCaptionTranslation", Const.ACIdentifierPrefix},
                        new object[] {Const.QueryPrefix + ACClassWFEdge.ClassName, "en{'Workflowedge'}de{'Workflowbeziehung'}", typeof(ACClassWFEdge), ACClassWFEdge.ClassName + "_" + ACClassMethod.ClassName, "", Const.ACIdentifierPrefix},
                    },
                },
                new object[] {Const.QueryPrefix + ACClassDesign.ClassName, "en{'Design'}de{'Design'}", typeof(ACClassDesign), ACClassDesign.ClassName + "_" + ACClass.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix},
                new object[] {Const.QueryPrefix + ACClassPropertyRelation.ClassName, "en{'Propertyrelation'}de{'Eigenschaftsbeziehung'}", typeof(ACClassPropertyRelation), ACClassPropertyRelation.ClassName + "_SourceACClass", "", ACClassPropertyRelation.ClassName + "ID"},
                new object[] {Const.QueryPrefix + ACClassMessage.ClassName, "en{'Message'}de{'Meldung'}", typeof(ACClassMessage), ACClassMessage.ClassName + "_" + ACClass.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix},
                new object[] {Const.QueryPrefix + ACClassText.ClassName, "en{'Text'}de{'Text'}", typeof(ACClassText), ACClassText.ClassName + "_" + ACClass.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix},
                new object[] {Const.QueryPrefix + ACClassConfig.ClassName, "en{'Classconfig'}de{'Klassenkonfiguration'}", typeof(ACClassConfig), ACClassConfig.ClassName + "_" + ACClass.ClassName, Const.PN_LocalConfigACUrl, Const.PN_LocalConfigACUrl},
        })
    ]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClass>) })]
    public partial class ACClass : IACObjectEntityWithCheckTrans, IACType, IACConfigStore, IACClassEntity
    {
        public const string ClassName = "ACClass";
        public readonly ACMonitorObject _10020_LockValue = new ACMonitorObject(10020);


        #region New/Delete
        /// <summary>
        /// Thread-critical: If database is GlobalDatabase, then lock operation together with SaveChanges!!!
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <returns>ACClass.</returns>
        public static ACClass NewACObject(Database database, IACObject parentACObject)
        {
            ACClass entity = new ACClass();
            entity.Database = database;
            entity.ACClassID = Guid.NewGuid();
            entity.IsAbstract = false;
            entity.AssemblyQualifiedName = "";
            entity.ACStartType = Global.ACStartTypes.Manually;
            entity.ACStorableType = Global.ACStorableTypes.NotStorable;
            entity.IsAssembly = false;
            entity.IsMultiInstance = false;
            entity.IsRightmanagement = false;
            entity.IsStatic = false;
            entity.Comment = "";
            entity.XMLConfig = "";
            entity.SortIndex = 9999;
            entity.BranchNo = 0;
            //entity.ACPackage = ACPackage.DefaultACPackage(database);

            if (parentACObject is ACProject)
            {
                entity.ACProject = parentACObject as ACProject;
                entity.ACClass1_ParentACClass = null;
            }
            else if (parentACObject is ACClass)
            {
                entity.ACClass1_ParentACClass = parentACObject as ACClass;
                entity.ACProject = entity.ACClass1_ParentACClass.ACProject;
            }
            entity.ACIdentifier = "NewClass";

            entity.SetInsertAndUpdateInfo(Database.Initials, database);
            return entity;
        }

        /// <summary>
        /// News the AC object with baseclass.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="basedOnACClass">The based on AC class.</param>
        /// <returns>ACClass.</returns>
        public static ACClass NewACObjectWithBaseclass(Database database, IACObject parentACObject, ACClass basedOnACClass)
        {
            ACClass entity = ACClass.NewACObject(database, parentACObject);
            if (entity != null && basedOnACClass != null)
            {
                entity.CopyAll(basedOnACClass);
                entity.ACClass1_PWACClass = basedOnACClass.ACClass1_PWACClass;
            }
            return entity;
        }

        public Database Database { get; private set; } = null;

        void IACClassEntity.OnObjectMaterialized(Database db)
        {
            if (Database == null)
                Database = db;
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

            List<ACClassConfig> configs = ACClassConfig_ACClass.ToList();
            foreach (var item in configs)
                item.DeleteACObject(database, false);


            // Remove ACClassProperty references and replace with unknown member
            ACClass unknownClass = database.ContextIPlus.ACClass.FirstOrDefault(c => c.ACIdentifier == Const.UnknownClass);
            List<ACClassProperty> referencedProperties = database.ContextIPlus.ACClassProperty.Where(c => c.ValueTypeACClassID == ACClassID).ToList();
            foreach (var item in referencedProperties)
                item.ValueTypeACClass = unknownClass;


            var properties = ACClassProperty_ACClass.ToList();
            foreach(var property in properties)
            {
                List<ACClassProperty> basedProperties = database.ContextIPlus.ACClassProperty.Where(c => c.BasedOnACClassPropertyID == property.ACClassPropertyID).ToList();
                foreach (var item in basedProperties)
                    item.DeleteACObject(database, false);
            }

            var valueReferencedClassMethods = database.ContextIPlus.ACClassMethod.Where(c => c.ValueTypeACClassID == ACClassID).ToList();
            foreach (var method in valueReferencedClassMethods)
                method.ValueTypeACClass = unknownClass;



            // Disconnect with VBGroupRight
            //foreach (var item in ACClassProperty_ACClass)
            //{
            //    var propertyRights = item.VBGroupRight_ACClassProperty.ToList();
            //    foreach (var right in propertyRights)
            //    {
            //        right.ACClassProperty = null;
            //        right.ACClassPropertyID = null;
            //        item.VBGroupRight_ACClassProperty.Remove(right);
            //    }
            //}


            //foreach (var item in ACClassMethod_ACClass)
            //{
            //    var methodRights = item.VBGroupRight_ACClassMethod.ToList();
            //    foreach (var right in methodRights)
            //    {
            //        right.ACClassMethod = null;
            //        right.ACClassMethodID = null;
            //        item.VBGroupRight_ACClassMethod.Remove(right);
            //    }
            //}


            //foreach (var item in ACClassDesign_ACClass)
            //{
            //    var designRights = item.VBGroupRight_ACClassDesign.ToList();
            //    foreach (var right in designRights)
            //    {
            //        right.ACClassDesign = null;
            //        right.ACClassDesignID = null;
            //        item.VBGroupRight_ACClassDesign.Remove(right);
            //    }
            //}

            var classRights = VBGroupRight_ACClass.ToList();
            foreach (var right in classRights)
            {
                right.DeleteACObject(database, false);
            }


            //// Delete attached Methods
            //if (this.ACKind == Global.ACKinds.TPAProcessFunction)
            //{
            //    var queryVirtualMethods = this.ACClassMethod_ACClass.Where(c => c.PWACClassID != null);
            //    foreach (var virtualMethod in queryVirtualMethods)
            //    {
            //        foreach (var attachedMethod in virtualMethod.ACClassMethod_ParentACClassMethod)
            //        {
            //            if (attachedMethod.ACKind == Global.ACKinds.MSMethodFunction)
            //            {
            //                //attachedMethod.ACClassMethod1_ParentACClassMethod = null;
            //                foreach (var groupRight in attachedMethod.VBGroupRight_ACClassMethod.AsEnumerable())
            //                    groupRight.DeleteACObject(database, false);
            //                attachedMethod.DeleteACObject(database, false);
            //            }
            //        }
            //    }
            //}

            foreach (var acClassPropertyBinding in ACClassPropertyRelation_SourceACClass.ToList())
            {
                acClassPropertyBinding.DeleteACObject(database, withCheck);
            }

            foreach (var acClassPropertyBinding in ACClassPropertyRelation_TargetACClass.ToList())
            {
                acClassPropertyBinding.DeleteACObject(database, withCheck);
            }

            database.DeleteObject(this);
            return null;
        }

        /// <summary>
        /// Thread-critical: If Entity is from GlobalDatabase-context, then lock operation together with SaveChanges!!!
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="withCheck">if set to <c>true</c> [with check].</param>
        /// <returns>Msg.</returns>
        public Msg DeleteACClassRecursive(Database database, bool withCheck)
        {
            if (withCheck)
            {
                Msg msg = IsEnabledDeleteRecursive(database);
                if (msg != null)
                    return msg;
            }

            DeleteACClassRecursiveInternal(database);
            return null;
        }

        /// <summary>
        /// Deletes the AC class rekursive internal.
        /// </summary>
        /// <param name="database">The database.</param>
        void DeleteACClassRecursiveInternal(Database database)
        {
            if (ACClass_ParentACClass.Any())
            {
                foreach (var acClassChild in ACClass_ParentACClass.ToList())
                {
                    acClassChild.DeleteACClassRecursiveInternal(database);
                }
            }

            List<ACClass> basedACClasses = database.ACClass.Where(c => c.BasedOnACClassID == ACClassID).ToList(); ;
            foreach (var basedACClass in basedACClasses)
                basedACClass.DeleteACClassRecursiveInternal(database);

            DeleteACObject(database, false);
        }


        /// <summary>
        /// Determines whether [is enabled delete rekursive] [the specified database].
        /// </summary>
        /// <param name="database">The database.</param>
        public Msg IsEnabledDeleteRecursive(Database database)
        {

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return IsEnabledDeleteRecursiveLocked(database);
            }
        }

        /// <summary>
        /// Determines whether [is enabled delete rekursive locked] [the specified database].
        /// </summary>
        /// <param name="database">The database.</param>
        private Msg IsEnabledDeleteRecursiveLocked(Database database)
        {
            // Diese Klasse prüfen
            MsgWithDetails msg = IsEnabledDeleteACObject(database);
            if (msg != null)
                return msg;

            // Unter-ACClass überprüfen
            foreach (var acClassChild in ACClass_ParentACClass)
            {
                Msg msg2 = acClassChild.IsEnabledDeleteRecursiveLocked(database);
                if (msg2 != null)
                    return msg2;
            }

            return null;
        }

        /// <summary>
        /// Counts the designs rekursiv.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int CountDesignsRecursive()
        {

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return CountDesignsRecursiveLocked();
            }
        }

        /// <summary>
        /// Counts the designs rekursiv locked.
        /// </summary>
        /// <returns>System.Int32.</returns>
        private int CountDesignsRecursiveLocked()
        {
            int count = this.ACClassDesign_ACClass.Count();
            foreach (var acClass in this.ACClass_ParentACClass)
            {
                count += acClass.CountDesignsRecursiveLocked();
            }
            return count;
        }

        /// <summary>
        /// Copies all.
        /// </summary>
        /// <param name="fromACClass">From AC class.</param>
        public void CopyAll(ACClass fromACClass)
        {
            ACClass1_BasedOnACClass = fromACClass;
            ACIdentifier = fromACClass.ACIdentifier;
            ACCaptionTranslation = fromACClass.ACCaptionTranslation;
            ACKindIndex = fromACClass.ACKindIndex;
            ACPackage = fromACClass.ACPackage;
            AssemblyQualifiedName = fromACClass.AssemblyQualifiedName;
            PWACClassID = fromACClass.PWACClassID;
            Comment = fromACClass.Comment;
            IsAbstract = fromACClass.IsAbstract;
            ACStartTypeIndex = fromACClass.ACStartTypeIndex;
            ACStorableTypeIndex = fromACClass.ACStorableTypeIndex;
            XMLConfig = fromACClass.XMLConfig;
            IsRightmanagement = fromACClass.IsRightmanagement;
        }
        #endregion


        #region License

        private bool? _IsPackageLicensed = null;
        /// <summary>
        /// Gets a value indicating whether this instance has licence.
        /// </summary>
        /// <value><c>true</c> if this instance has licence; otherwise, <c>false</c>.</value>
        public bool IsLicensed
        {
            get
            {

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    if (ACPackage == null)
                        return false;
                }

                if (_IsPackageLicensed.HasValue && !_IsPackageLicensed.Value)
                    return false;

                if (!_IsPackageLicensed.HasValue && !string.IsNullOrEmpty(this.AssemblyQualifiedName) && ObjectType != null && ObjectType.GetCustomAttributes(typeof(ACClassInfo), false).Any())
                {
                    ACClassInfo acClassInfo = ObjectType.GetCustomAttributes(typeof(ACClassInfo), false).First() as ACClassInfo;

                    if (acClassInfo.ACPackageName == Const.PackName_VarioSystem || acClassInfo.ACPackageName == Const.PackName_System)
                    {
                        _IsPackageLicensed = true;
                        return true;
                    }

                    if (acClassInfo.ACPackageName != ACPackage.ACPackageName) // Es wurde die Packagezuordnung in der Datenbank manipuliert
                    {
                        _IsPackageLicensed = false;
                        return _IsPackageLicensed.Value;
                    }
                }
                return ACPackage.IsLicensed;
            }
        }
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
            UpdateACKindIndex();
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
            UpdateACKindIndex();
            base.EntityCheckModified(user, context);
            return null;
        }

        #endregion


        #region Idenification and ACURL
        /// <summary>
        /// Primary Key of a Entity in the Database/Table
        /// (Uniqued Identifier of a type in the iPlus-Framework)
        /// </summary>
        public Guid ACTypeID
        {
            get { return ACClassID; }
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


        private string _ACUrlComponentCached = null;
        /// <summary>
        /// Returns the ACUrl of the ACComponent in the Application-Tree.
        /// This ACClass must be a type which ist defined in the Application-Tree (physical model) 
        /// Otherwise a empty string will be returned
        /// Attention: The reuslt is cached. Once you have called this method, the ACUrl will never change and you have to restart the application if you have made changes in the Application-Tree
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <returns></returns>
        public string GetACUrlComponentCached()
        {
            if (!String.IsNullOrEmpty(_ACUrlComponentCached))
                return _ACUrlComponentCached;

            if (!string.IsNullOrEmpty(ACURLComponentCached))
            {
                _ACUrlComponentCached = ACURLComponentCached;
                return _ACUrlComponentCached;
            }

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                _ACUrlComponentCached = GetACUrlComponentLocked(null);
            }
            return _ACUrlComponentCached;
        }


        /// <summary>
        /// Returns the ACUrl that a reel instance will have in runtime. (ACUrl of the ACComponent in the Application-Tree)
        /// This ACClass must be a type which ist defined in the Application-Tree (physical model) 
        /// Otherwise a empty string will be returned
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="rootACObject">If null, then a absolute ACUrl will be returned. Else a relative url to the passed object. If you pass a ACClass of a parent object, then the ARUrl will be shorten by a realtive address</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("", "", 9999)]
        public override string GetACUrlComponent(IACObject rootACObject = null)
        {

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return GetACUrlComponentLocked(rootACObject);
            }
        }

        private string GetACUrlComponentLocked(IACObject rootACObject = null)
        {
            ACClass acClassOfRoot = null;
            if (rootACObject != null)
            {
                if (!(rootACObject is ACClass))
                {
                    if (rootACObject.ACType is ACClass)
                        acClassOfRoot = rootACObject.ACType as ACClass;
                }
                else
                    acClassOfRoot = rootACObject as ACClass;
            }
            if ((acClassOfRoot != null) && (ACClassID == acClassOfRoot.ACClassID))
                return "";

            string acUrl = "";
            if (ParentACObject != null)
            {
                ACClass parentACClass = ParentACObject as ACClass;
                if (parentACClass != null)
                {
                    if ((acClassOfRoot == null)
                        || (parentACClass.ACClassID != acClassOfRoot.ACClassID))
                    {
                        acUrl = parentACClass.GetACUrlComponentLocked(acClassOfRoot);
                    }
                }
                // Project
                else
                {
                }
            }

            if (ACClass1_ParentACClass == null)
            {
                switch (ACProject.ACProjectType)
                {
                    case Global.ACProjectTypes.Application:
                    case Global.ACProjectTypes.Service:
                        if (!acUrl.EndsWith("\\"))
                            acUrl += "\\";
                        acUrl += ACIdentifier;
                        break;
                    case Global.ACProjectTypes.Root:
                        acUrl = "\\";
                        break;
                    default:
                        if (!acUrl.EndsWith("\\"))
                            acUrl += "\\";
                        if (!IsMultiInstanceInherited)
                            acUrl += ACIdentifier;
                        else
                            acUrl += ACIdentifier + "()";
                        break;
                }
            }
            else
            {
                switch (ACProject.ACProjectType)
                {
                    case Global.ACProjectTypes.AppDefinition:
                    case Global.ACProjectTypes.Application:
                    case Global.ACProjectTypes.Service:
                        //if (!acUrl.EndsWith("\\"))
                        if (!String.IsNullOrEmpty(acUrl) && !acUrl.EndsWith("\\"))
                            acUrl += "\\";
                        acUrl += ACIdentifier;
                        break;
                    case Global.ACProjectTypes.ClassLibrary:
                    case Global.ACProjectTypes.Root:
                    default:
                        if ((acUrl.Length > 0) && !acUrl.EndsWith("\\"))
                            acUrl += "\\";
                        if (!IsMultiInstanceInherited)
                            acUrl += ACIdentifier;
                        else
                            acUrl += ACIdentifier + "()";
                        break;
                }
            }

            return acUrl;
        }


        /// <summary>
        /// Returns a ACClass, ACClassProperty, ACClassMethod through a given acUrl (Applicationtree)
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="acUrlComponent"></param>
        /// <returns></returns>
        public IACType GetTypeByACUrlComponent(string acUrlComponent)
        {

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return GetTypeByACUrlComponentLocked(acUrlComponent);
            }
        }

        private IACType GetTypeByACUrlComponentLocked(string acUrlComponent)
        {
            if (string.IsNullOrEmpty(acUrlComponent))
                return null;

            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrlComponent);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    {
                        if (acUrlHelper.NextACUrl.Length <= 0)
                            return ACProject.RootClass;
                        else
                            return ACProject.RootClass.GetTypeByACUrlComponentLocked(acUrlHelper.NextACUrl);
                    }
                case ACUrlHelper.UrlKeys.Child:
                    {
                        ACClass acClassChild;
                        var query = ACClass_ParentACClass.Where(c => c.ACIdentifier == acUrlHelper.ACUrlPart);
                        if (!query.Any())
                        {
                            var queryProp = Properties.Where(c => c.ACIdentifier == acUrlHelper.ACUrlPart);
                            if (queryProp.Any())
                                return queryProp.First();
                            if (this.ACKind == Global.ACKinds.TACRoot)
                            {
                                var query2 = this.Database.ACProject.Where(c => c.ACProjectName == acUrlHelper.ACUrlPart);
                                if (query2.Any())
                                {
                                    ACProject acProject = query2.First();
                                    acClassChild = acProject.RootClass;
                                    if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                        return acClassChild;
                                    return acClassChild.GetTypeByACUrlComponentLocked(acUrlHelper.NextACUrl);
                                }
                            }
                            return null;
                        }
                        acClassChild = query.First();
                        if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                            return acClassChild;
                        return acClassChild.GetTypeByACUrlComponentLocked(acUrlHelper.NextACUrl);
                    }
                case ACUrlHelper.UrlKeys.Parent:
                    if (ACClass1_ParentACClass != null)
                        return ACClass1_ParentACClass.GetTypeByACUrlComponentLocked(acUrlHelper.NextACUrl);
                    else
                        return null;
                default:
                    return null;
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
                if (!string.IsNullOrEmpty(ACURLCached))
                    return ACURLCached;
                return GetACUrl();
            }
        }

        /// <summary>
        /// Returns ACUrl in the Application-Tree if this Class is in a physical model
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999)]
        public string ACUrlComponent
        {
            get
            {
                if (!string.IsNullOrEmpty(ACURLComponentCached))
                    return ACURLComponentCached;
                return GetACUrlComponent();
            }
        }

        /// <summary>
        /// Returns the Parent-ACClass if this Class is in a Application-Tree
        /// If there is no Parent-Class then a ACProject-Instance is returned where this Class belongs to
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999)]
        public override IACObject ParentACObject
        {
            get
            {

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    if (ACClass1_ParentACClass != null)
                        return ACClass1_ParentACClass;
                    return ACProject;
                }
            }
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
            if (filterValues == null)
                return null;
            int countParams = filterValues.Count();
            if (countParams <= 0)
                return null;
            IACObject childObject = null;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                switch (className)
                {
                    case ACClassProperty.ClassName:
                        childObject = ACClassProperty_ACClass.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        if (childObject != null)
                            return childObject;
                        // Suche in Vererbungshierarchie
                        else if ((countParams >= 2) && (filterValues[1] == Const.ParamInheritedMember))
                        {
                            childObject = Properties.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                            if (childObject != null)
                                return childObject;
                            else
                                return Points.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        }
                        break;
                    case ACClassMethod.ClassName:
                        childObject = ACClassMethod_ACClass.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        if (childObject != null)
                            return childObject;
                        // Suche in Vererbungshierarchie
                        else if ((countParams >= 2) && (filterValues[1] == Const.ParamInheritedMember))
                            return Methods.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        break;
                    case ACClassDesign.ClassName:
                        childObject = ACClassDesign_ACClass.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        if (childObject != null)
                            return childObject;
                        else if ((countParams >= 2) && (filterValues[1] == Const.ParamInheritedMember))
                            return Designs.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        break;
                    case ACClass.ClassName:
                        return ACClass_ParentACClass.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                    case ACClassConfig.ClassName:
                        return ConfigurationEntries.Where(c => c.LocalConfigACUrl == filterValues[0]).FirstOrDefault();
                    case ACClassPropertyRelation.ClassName:
                        // KeyACIdentifier: "SourceACClassProperty\\ACIdentifier,TargetACClass\\ACIdentifier,TargetACClassProperty\\ACIdentifier,ConnectionTypeIndex";
                        string[] subFilterValues = filterValues[0].Split(',');
                        string sourcePropertyACIdentifier = subFilterValues[0];
                        string targetClassACIdentifier = subFilterValues[1];
                        string targetPropertyACIdentifier = subFilterValues[2];
                        int connectionTypeIndex = int.Parse(subFilterValues[3]);
                        return ACClassPropertyRelation_SourceACClass
                            .Where(c =>
                                c.SourceACClassProperty.ACIdentifier == sourcePropertyACIdentifier &&
                                c.TargetACClass.ACIdentifier == targetClassACIdentifier &&
                                c.TargetACClassProperty.ACIdentifier == targetPropertyACIdentifier &&
                                c.ConnectionTypeIndex == connectionTypeIndex
                            )
                            .FirstOrDefault();
                    case ACClassText.ClassName:
                        childObject = ACClassText_ACClass.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        if (childObject != null)
                            return childObject;
                        // Suche in Vererbungshierarchie
                        else if ((countParams >= 2) && (filterValues[1] == Const.ParamInheritedMember))
                            return Texts.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        break;
                    case ACClassMessage.ClassName:
                        childObject = ACClassMessage_ACClass.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        if (childObject != null)
                            return childObject;
                        // Suche in Vererbungshierarchie
                        else if ((countParams >= 2) && (filterValues[1] == Const.ParamInheritedMember))
                            return Messages.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                        break;
                }
            }

            return null;
        }

        public String GetACPath(bool first)
        {
            return null;
        }

        #endregion


        #region Inheritance and Derivation

        /// <summary>
        /// Determines if this class is derived from a given baseclass
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="baseClass"></param>
        /// <returns></returns>
        public bool IsDerivedClassFrom(ACClass baseClass)
        {
            if (baseClass == null)
                return false;

            bool? result = this.Database.IsDerived(baseClass.ACClassID, this.ACClassID);
            if (result != null)
                return result.Value;

            bool resultDB;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                resultDB = baseClass.IsBaseClassFromLocked(this);
            }
            this.Database.RegisterDerivedClass(baseClass.ACClassID, this.ACClassID, resultDB);
            return resultDB;
        }

        public bool IsDerivedOrEqual(Type type)
        {
            if (type == null)
                throw new NullReferenceException("type is null");
            Type objectType = this.ObjectType;
            if (objectType == null)
                throw new NullReferenceException("ObjectType is null");
            return type.IsAssignableFrom(objectType);
        }

        /// <summary>
        /// Determines if this class is derived from a given named baseclass
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="baseClass"></param>
        /// <returns></returns>
        public bool IsDerivedClassFrom(string baseClassACIdentifier)
        {
            if (String.IsNullOrEmpty(baseClassACIdentifier))
                return false;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return IsBaseClassFromLocked(this, baseClassACIdentifier);
            }
        }


        /// <summary>
        /// Returns all derived classes from this class
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        public ACClass[] DerivedClassesInProjects
        {
            get
            {
                List<ACClass> derivedClasses = new List<ACClass>();
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    FillDerivedClasses(ref derivedClasses, this, new Global.ACProjectTypes[] { Global.ACProjectTypes.Application, Global.ACProjectTypes.Service }, 0);
                    return derivedClasses.OrderBy(x => x.ACCaption).ToArray();
                }
            }
        }

        /// <summary>
        /// Returns all derived classes from this class
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        public List<ACClass> DerivedClasses
        {
            get
            {
                List<ACClass> derivedClasses = new List<ACClass>();
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    FillDerivedClasses(ref derivedClasses, this, null, 0);
                    return derivedClasses;
                }
            }
        }


        private bool IsBaseClassFromLocked(ACClass derivedClass)
        {
            if (derivedClass == null)
                return false;
            ACClass superClass = derivedClass;
            int i = 0;
            while (superClass != null)
            {
                i++;
                if (superClass._ACClassID == this.ACClassID)
                    return true;
                superClass = superClass.ACClass1_BasedOnACClass;
                if (i > 1000)
                {
                    throw new StackOverflowException("Class is recursive derived!");
                }
            }
            return false;
        }


        private bool IsBaseClassFromLocked(ACClass derivedClass, string baseClassACIdentifier)
        {
            if (derivedClass == null)
                return false;
            ACClass superClass = derivedClass;
            int i = 0;
            while (superClass != null)
            {
                i++;
                if (superClass.ACIdentifier == baseClassACIdentifier)
                    return true;
                superClass = superClass.ACClass1_BasedOnACClass;
                if (i > 1000)
                {
                    throw new StackOverflowException("Class is recursive derived!");
                }
            }
            return false;
        }

        private void FillDerivedClasses(ref List<ACClass> classList, ACClass currentClass, IEnumerable<Global.ACProjectTypes> filterProjectTypes, int currentRecursion)
        {
            currentRecursion++;
            if (currentRecursion > 1000)
                throw new StackOverflowException("Classes are recursive derived!");
            foreach (ACClass derivedClass in currentClass.ACClass_BasedOnACClass)
            {
                if (filterProjectTypes == null || filterProjectTypes.Contains(derivedClass.ACProject.ACProjectType))
                    classList.Add(derivedClass);
                FillDerivedClasses(ref classList, derivedClass, filterProjectTypes, currentRecursion);
            }
        }

        #endregion


        #region Class and Hierarchy

        #region public

        #region Properties
        /// <summary>
        /// Returns the Base-Class (ACClass1_BasedOnACClass)
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        public ACClass BaseClass
        {
            get
            {
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return ACClass1_BasedOnACClass;
                }
            }
        }


        /// <summary>
        /// Returns all childs of current class (composition-tree)
        /// Thread-Safe access to ACClass_ParentACClass. Returns a copy of ACClass_ParentACClass;
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<ACClass> Childs
        {
            get
            {
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return this.ACClass_ParentACClass.ToArray();
                }
            }
        }

        /// <summary>
        /// Returns the Class-Hierarchy without interfaces
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        public IEnumerable<ACClass> ClassHierarchy
        {
            get
            {
                List<ACClass> acClassList = new List<ACClass>();
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    FillClassHierarchyList(ref acClassList);
                }
                return acClassList;
            }
        }

        /// <summary>
        /// Returns the Class-Hierarchy with interfaces
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<ACClass> ClassHierarchyWithInterfaces
        {
            get
            {
                List<ACClass> acClassList = ClassHierarchy as List<ACClass>;

                if (this.ObjectType != null)
                {
                    foreach (var interfaceType in this.ObjectType.GetInterfaces().OrderBy(c => c.Name))
                    {
                        ACClass interfaceACType = null;

                        using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                        {
                            interfaceACType = this.Database.ACClass.Where(c => c.AssemblyQualifiedName == interfaceType.AssemblyQualifiedName).FirstOrDefault();
                        }
                        if (interfaceACType != null && !acClassList.Contains(interfaceACType))
                            acClassList.Add(interfaceACType);
                    }
                }
                return acClassList;
            }
        }


        /// <summary>
        /// Returns the first base-class which is a hardcoded class and has a Assembly-Qualified-Name
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public ACClass BaseClassWithASQN
        {
            get
            {
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return BaseClassWithASQNLocked;
                }
            }
        }


        /// <summary>
        /// Returns the all child classes which appears while interating to root-Node
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<ACClass> AllChildsInHierarchy
        {
            get
            {
                List<ACClass> acClassList = new List<ACClass>();
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    FillListWithChildsInHierarchy(ref acClassList);
                }
                return acClassList.OrderBy(c => c.ACIdentifier);
            }
        }


        private int? _InheritanceLevel = null;
        /// <summary>
        /// Returns the depth of Inheritance
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "en{'Inheritancelevel'}de{'Vererbungstiefe'}", "", false)]
        public int InheritanceLevel
        {
            get
            {
                if (_InheritanceLevel.HasValue)
                    return _InheritanceLevel.Value;
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    _InheritanceLevel = ACClass1_BasedOnACClass != null ? ACClass1_BasedOnACClass.InheritanceLevel + 1 : 0;
                }
                return _InheritanceLevel.Value;
            }
        }

        /// <summary>
        /// Returns the Assembly-Qualifiend-Name through class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "en{'Assembly-qualified name'}de{'Assembly qualifizierter Name'}", "", true)]
        public string InheritedASQN
        {
            get
            {
                var baseClassWithASQN = BaseClassWithASQN;
                return baseClassWithASQN != null ? baseClassWithASQN.AssemblyQualifiedName : "";
            }
        }

        /// <summary>
        /// Returns the IsMultiInstance-Property through class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public bool IsMultiInstanceInherited
        {
            get
            {
                var baseClassWithASQN = BaseClassWithASQN;
                return baseClassWithASQN != null ? baseClassWithASQN.IsMultiInstance : false;
            }
        }

        /// <summary>
        /// If Class represents a physical instance and relates to a workflow class
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public ACClass RelatedWorkflowClass
        {
            get
            {
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return RelatedWorkflowClassLocked;
                }
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Returns first Member with this name in complete class hierarchy.
        /// 1. Searches for Child-Classes in Composition-Tree
        /// 2. Searches for Properties
        /// 3. Searches for Methods
        /// THREAD-SAFE while using QueryLock_1X000
        /// </summary>
        /// <param name="acIdentifier">Name of property</param>
        /// <param name="forceRefreshFromDB">Refresh the property cache through reload from database</param>
        /// <returns>ACClassProperty.</returns>
        public IACType GetMember(string acIdentifier, bool forceRefreshFromDB = false)
        {
            ACClass acClassMember = Childs.Where(c => c.ACIdentifier == acIdentifier
                                                        && c.ACKindIndex != (Int16)Global.ACKinds.TACBSOGlobal
                                                        && c.ACKindIndex != (Int16)Global.ACKinds.TACBSOReport
                                                        && c.ACKindIndex != (Int16)Global.ACKinds.TACDBA)
                                                    .FirstOrDefault();
            if (acClassMember != null)
                return acClassMember;

            ACClassProperty acClassProperty = GetProperty(acIdentifier, forceRefreshFromDB);
            if (acClassProperty != null)
                return acClassProperty;

            return GetMethod(acIdentifier);
        }

        #endregion

        #region private

        #region Properties
        /// <summary>
        /// UNSAFE
        /// </summary>
        private ACClass BaseClassWithASQNLocked
        {
            get
            {
                if (!string.IsNullOrEmpty(this.AssemblyQualifiedName))
                {
                    return this;
                }

                if (this.ACClass1_BasedOnACClass != null)
                {
                    return this.ACClass1_BasedOnACClass.BaseClassWithASQNLocked;
                }
                return null;
            }
        }

        /// <summary>
        /// UNSAFE
        /// </summary>
        private ACClass RelatedWorkflowClassLocked
        {
            get
            {
                if (ACKind == Global.ACKinds.TPWGroup ||
                    ACKind == Global.ACKinds.TPWNode ||
                    ACKind == Global.ACKinds.TPWNodeMethod ||
                    ACKind == Global.ACKinds.TPWNodeWorkflow ||
                    ACKind == Global.ACKinds.TPWNodeStart ||
                    ACKind == Global.ACKinds.TPWNodeEnd ||
                    ACKind == Global.ACKinds.TPWNodeStatic)
                    return this;
                if (ACClass1_PWACClass != null)
                    return ACClass1_PWACClass;
                if (ACClass1_BasedOnACClass != null)
                    return ACClass1_BasedOnACClass.RelatedWorkflowClassLocked;
                return null;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// UNSAFE:
        /// </summary>
        private void FillClassHierarchyList(ref List<ACClass> acClassList)
        {
            acClassList.Add(this);
            if (ACClass1_BasedOnACClass != null)
                ACClass1_BasedOnACClass.FillClassHierarchyList(ref acClassList);
        }


        /// <summary>
        /// UNSAFE:
        /// </summary>
        /// <param name="acClassList">The ac class list.</param>
        private void FillListWithChildsInHierarchy(ref List<ACClass> acClassList)
        {
            foreach (var acClass in ACClass_ParentACClass)
            {
                acClassList.Add(acClass);
            }

            if (ACClass1_BasedOnACClass != null && ACClass1_BasedOnACClass.ACProject.ACProjectTypeIndex != 100 /*ACClassLibrary*/)
                ACClass1_BasedOnACClass.FillListWithChildsInHierarchy(ref acClassList);
        }
        #endregion

        #endregion

        #endregion
        #endregion


        #region ACClassMethod

        #region public

        /// <summary>
        /// Returns all Methods over complete class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<ACClassMethod> Methods
        {
            get
            {
                return GetMethods();
            }
        }


        List<ACClassMethod> _CachedMethodList = null;
        /// <summary>
        /// Returns all Methods over complete class hierarchy
        /// Holds the result in a private cached List to avoid querying the database
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<ACClassMethod> MethodsCached
        {
            get
            {
                using (ACMonitor.Lock(_10020_LockValue))
                {
                    if (_CachedMethodList != null)
                        return _CachedMethodList;
                    _CachedMethodList = Methods as List<ACClassMethod>;
                    return _CachedMethodList;
                }
            }
        }

        public void RefreshCachedMethods()
        {
            using (ACMonitor.Lock(_10020_LockValue))
            {
                _CachedMethodList = null;
            }
        }

        public void AddNewACClassMethod(ACClassMethod newMethod)
        {
            ACClassMethod_ACClass.Add(newMethod);
            RefreshCachedMethods();
        }

        /// <summary>
        /// Returns all Methods over complete class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="forceRefreshFromDB"></param>
        /// <param name="withOverrides"></param>
        /// <returns>New threadsafe list</returns>
        public List<ACClassMethod> GetMethods(bool forceRefreshFromDB = false, bool withOverrides = false)
        {
            List<ACClassMethod> acClassMethodList = new List<ACClassMethod>();

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                BuildInheritedMethodList(ref acClassMethodList, forceRefreshFromDB, withOverrides);
            }
            return acClassMethodList;
        }

        /// <summary>
        /// Returns first found Method in class hierarchy.
        /// THREAD-SAFE while using QueryLock_1X000
        /// </summary>
        /// <param name="acIdentifier">Name of method</param>
        /// <param name="forceRefreshFromDB">Refresh the method cache through reload from database</param>
        /// <returns>ACClassMethod.</returns>
        public ACClassMethod GetMethod(string acIdentifier, bool forceRefreshFromDB = false)
        {

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return GetMethodLocked(acIdentifier, forceRefreshFromDB);
            }
        }

        /// <summary>
        /// Returns first found Workflow-Method in class hierarchy.
        /// THREAD-SAFE while using QueryLock_1X000
        /// </summary>
        /// <returns>ACClassMethod.</returns>
        public IEnumerable<ACClassMethod> GetWorkflowStartMethod(bool withRootWF)
        {

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return GetWorkflowStartMethodLocked(withRootWF);
            }
        }


        #endregion

        #region Private
        /// <summary>
        /// UNSAFE: Fills methodlist through hierarchy
        /// </summary>
        private void BuildInheritedMethodList(ref List<ACClassMethod> acClassMethodList, bool forceRefreshFromDB, bool withOverrides)
        {
            ACClassMethod[] allMethods = LoadMethodListWithRelations(forceRefreshFromDB);
            foreach (var acClassMethod in allMethods)
            {
                if (withOverrides || !acClassMethodList.Where(c => c.ACIdentifier == acClassMethod.ACIdentifier).Any())
                    acClassMethodList.Add(acClassMethod);
            }

            if (ACClass1_BasedOnACClass != null)
                ACClass1_BasedOnACClass.BuildInheritedMethodList(ref acClassMethodList, forceRefreshFromDB, withOverrides);
        }

        /// <summary>
        /// UNSAFE: Loads ACClassProperty_ACClass-Collection on this Class-Level an includes the most relevant relations
        /// </summary>
        private ACClassMethod[] LoadMethodListWithRelations(bool forceRefreshFromDB)
        {
            ACClassMethod[] allMethods = null;
            try
            {
                if (!ACClassMethod_ACClass.IsLoaded && EntityState != System.Data.EntityState.Added
                    || forceRefreshFromDB)
                {
                    ACClassMethod_ACClass.Load(forceRefreshFromDB ? MergeOption.OverwriteChanges : MergeOption.AppendOnly);
                    allMethods = ACClassMethod_ACClass.CreateSourceQuery()
                                                .Include(c => c.PWACClass)
                                                .Include(c => c.ValueTypeACClass)
                                                .Include(c => c.ACClass)
                                                .Include(c => c.ACClassMethod1_ParentACClassMethod)
                                                .Include(c => c.AttachedFromACClass)
                                                .OrderBy(c => c.ACIdentifier)
                                                .ToArray();
                }
                else
                    allMethods = ACClassMethod_ACClass.ToArray();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClass", "LoadMethodListWithRelations", msg);
            }
            return allMethods;
        }


        /// <summary>
        /// UNSAFE: Returns first found Method in class hierarchy.
        /// </summary>
        /// <param name="acIdentifier">Name of property</param>
        /// <param name="forceRefreshFromDB">Refresh the property cache through reload from database</param>
        /// <returns>ACClassProperty.</returns>
        private ACClassMethod GetMethodLocked(string acMethodName, bool forceRefreshFromDB)
        {
            ACClassMethod[] allMethods = LoadMethodListWithRelations(forceRefreshFromDB);
            if (allMethods != null && allMethods.Any())
            {
                ACClassMethod foundMethod = allMethods.Where(c => c.ACIdentifier == acMethodName).FirstOrDefault();
                if (foundMethod != null)
                    return foundMethod;
            }
            if (this.ACClass1_BasedOnACClass != null)
            {
                return this.ACClass1_BasedOnACClass.GetMethodLocked(acMethodName, forceRefreshFromDB);
            }
            return null;
        }


        /// <summary>
        /// UNSAFE
        /// </summary>
        /// <param name="withRootWF">if set to <c>true</c> [with root WF].</param>
        /// <returns>IEnumerable{ACClassMethod}.</returns>
        private IEnumerable<ACClassMethod> GetWorkflowStartMethodLocked(bool withRootWF)
        {
            List<ACClassMethod> acClassMethodList = new List<ACClassMethod>();
            IEnumerable<ACClassMethod> methods;
            if (withRootWF || this.ACProject.RootClass != this)
            {
                methods = Methods.Where(c => c.PWACClass != null &&
                    (c.PWACClass.ACKindIndex == (Int16)Global.ACKinds.TPWNodeMethod || (c.PWACClass.ACKindIndex == (Int16)Global.ACKinds.TPWNodeWorkflow && c.IsSubMethod)));
            }
            else
            {
                methods = Methods.Where(c => c.PWACClass != null &&
                    c.PWACClass.ACKindIndex == (Int16)Global.ACKinds.TPWNodeMethod);
            }
            if (methods.Any())
            {
                foreach (var acClassMethod in methods)
                {
                    acClassMethodList.Add(acClassMethod);
                }
            }
            if ((this.ACProject.ACProjectType == Global.ACProjectTypes.Application
                    || this.ACProject.ACProjectType == Global.ACProjectTypes.Service)
                && ACClass1_BasedOnACClass != null)
            {
                if (ACClass1_BasedOnACClass.ACProject.ACProjectType == Global.ACProjectTypes.AppDefinition
                    || ACClass1_BasedOnACClass.ACProject.ACProjectType == Global.ACProjectTypes.Service)
                {
                    var parentACClassMethodList = this.ACClass1_BasedOnACClass.GetWorkflowStartMethodLocked(withRootWF);
                    if (parentACClassMethodList != null && parentACClassMethodList.Any())
                    {
                        foreach (var acClassMethod in parentACClassMethodList)
                        {
                            acClassMethodList.Add(acClassMethod);
                        }
                    }
                }
            }
            return acClassMethodList;
        }

        #endregion

        #region Signature

        /// <summary>
        /// Returns the Signature of a dynamic parameterized Method or constructor
        /// THREAD-SAFE  (QueryLock_1X000)
        /// </summary>
        /// <param name="acUrl"></param>
        /// <param name="attachToObject"></param>
        /// <returns></returns>
        public ACMethod ACUrlACTypeSignature(string acUrl, IACObject attachToObject = null)
        {

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return ACUrlACTypeSignatureLocked(acUrl, attachToObject);
            }
        }

        /// <summary>
        /// Resturns the dynamic signature of a Class or Method
        /// THREAD-SAFE  (QueryLock_1X000)
        /// </summary>
        /// <returns></returns>
        public ACMethod TypeACSignature()
        {
            ACMethod acMethod = new ACMethod(this.ACIdentifier);
            acMethod.ParameterValueList = ACParameter;

            ACValue acValueResult = new ACValue();
            acValueResult.ACIdentifier = "result";
            acValueResult.Option = Global.ParamOption.Required;
            acValueResult.ValueTypeACClass = this;
            acMethod.ResultValueList.Add(acValueResult);
            return acMethod;
        }

        private ACMethod ACUrlACTypeSignatureLocked(string acUrl, IACObject attachToObject = null)
        {
            if (string.IsNullOrEmpty(acUrl))
                return null;

            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);

            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    {
                        var root = this.Database.ACClass.Where(c => c.ACKindIndex == (Int16)Global.ACKinds.TACRoot).FirstOrDefault();
                        if (root == null)
                            return null;
                        return root.ACUrlACTypeSignatureLocked(acUrlHelper.NextACUrl, attachToObject);
                    }
                case ACUrlHelper.UrlKeys.Child:
                case ACUrlHelper.UrlKeys.Start:
                    {
                        var query = this.Properties.Where(c => c.ACIdentifier == acUrlHelper.ACUrlPart);
                        if (query.Any())
                        {
                            ACClassProperty acTypeInfo = query.First();
                            ACMethod acMethod = acTypeInfo.TypeACSignature();
                            if ((acMethod != null) && (attachToObject != null))
                                acMethod.AttachTo(attachToObject);
                            return acMethod;
                        }
                        else
                        {
                            var query2 = this.AllChildsInHierarchy.Where(c => c.ACIdentifier == acUrlHelper.ACUrlPart);
                            if (!query2.Any())
                                return null;
                            ACClass acTypeInfo = query2.First();
                            if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                            {
                                ACMethod acMethod = acTypeInfo.TypeACSignature();
                                if ((acMethod != null) && (attachToObject != null))
                                    acMethod.AttachTo(attachToObject);
                                return acMethod;
                            }
                            else
                            {
                                return acTypeInfo.ACUrlACTypeSignatureLocked(acUrlHelper.NextACUrl, attachToObject);
                            }
                        }
                    }
                case ACUrlHelper.UrlKeys.InvokeMethod:
                    {
                        var query3 = this.Methods.Where(c => c.ACIdentifier == acUrlHelper.ACUrlPart);
                        if (query3.Any())
                        {
                            ACClassMethod acTypeInfo = query3.First();
                            ACMethod acMethod = acTypeInfo.TypeACSignature();
                            if ((acMethod != null) && (attachToObject != null))
                                acMethod.AttachTo(attachToObject);
                            return acMethod;
                        }
                    }
                    break;
            }
            return null;
        }

        #endregion

        #endregion


        #region ACClassProperty

        #region Public

        #region Properties
        /// <summary>
        /// Returns all Properties over complete class hierarchy including Points
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>ACClassProperty List</value>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<ACClassProperty> Properties
        {
            get
            {
                return GetProperties();
            }
        }

        /// <summary>
        /// Returns all Properties over complete class hierarchy which are declared in C#-Code
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>ACClassProperty List</value>
        public IEnumerable<IACType> HardcodedProperties
        {
            get
            {
                return Properties.Where(c => c.ACKind == Global.ACKinds.PSProperty);
            }
        }

        /// <summary>
        /// Returns all Properties over complete class hierarchy which are virtually added
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>ACClassProperty List</value>
        public IEnumerable<IACType> ExtendedProperties
        {
            get
            {
                return Properties.Where(c => c.ACKind == Global.ACKinds.PSPropertyExt);
            }
        }

        /// <summary>
        /// Returns Properties which are defined on the most super/top level in class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>The AC class property top base list.</value>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<ACClassProperty> BaseProperties
        {
            get
            {
                List<ACClassProperty> acClassPropertyList = new List<ACClassProperty>();

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    BuildInheritedPropertyList(ref acClassPropertyList, this, false, false, true);
                }
                return acClassPropertyList;
            }
        }

        /// <summary>
        /// Returns all Points over complete class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>ACClassProperty List</value>
        [ACPropertyInfo(9999, "", "", "", true)]
        public IEnumerable<ACClassProperty> Points
        {
            get
            {
                return Properties.Where(c => c.ACPropUsage >= Global.ACPropUsages.ConnectionPoint && c.ACPropUsage < Global.ACPropUsages.ChangeInfo);
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Returns all Properties over complete class hierarchy including Points
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="forceRefreshFromDB"></param>
        /// <param name="withOverrides"></param>
        /// <param name="onlyBaseProperties"></param>
        /// <param name="propUsage"></param>
        /// <returns>New threadsafe list</returns>
        public List<ACClassProperty> GetProperties(bool forceRefreshFromDB = false, bool withOverrides = false, bool onlyBaseProperties = false, Global.ACPropUsages? propUsage = null, bool includeStatic = false)
        {
            List<ACClassProperty> acClassPropertyList = new List<ACClassProperty>();

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                BuildInheritedPropertyList(ref acClassPropertyList, this, forceRefreshFromDB, withOverrides, onlyBaseProperties, propUsage, includeStatic);
            }
            return acClassPropertyList;
        }

        /// <summary>
        /// Returns first found Property in class hierarchy.
        /// THREAD-SAFE while using QueryLock_1X000
        /// </summary>
        /// <param name="acIdentifier">Name of property</param>
        /// <param name="forceRefreshFromDB">Refresh the property cache through reload from database</param>
        /// <returns>ACClassProperty.</returns>
        public ACClassProperty GetProperty(string acIdentifier, bool forceRefreshFromDB = false)
        {

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return GetPropertyLocked(acIdentifier, forceRefreshFromDB);
            }
        }

        /// <summary>
        /// Returns first found Property in class hierarchy by gropup-name and propertytype.
        /// THREAD-SAFE while using QueryLock_1X000
        /// </summary>
        /// <param name="acGroup">Name of group</param>
        /// <param name="acPropType">PropertyType</param>
        /// <param name="forceRefreshFromDB">Refresh the property cache through reload from database</param>
        /// <returns>ACClassProperty.</returns>
        public ACClassProperty GetProperty(string acGroup, Global.ACPropUsages acPropType, bool forceRefreshFromDB = false, bool includeStatic = false)
        {

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return GetPropertyLocked(acGroup, acPropType, forceRefreshFromDB, includeStatic);
            }
        }

        /// <summary>
        /// Returns first found Point in class hierarchy.
        /// THREAD-SAFE while using QueryLock_1X000
        /// </summary>
        /// <param name="acPropertyName">Name of the ac property.</param>
        /// <returns>ACClassProperty.</returns>
        public ACClassProperty GetPoint(string acPropertyName, bool forceRefreshFromDB = false)
        {
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return GetPointLocked(acPropertyName, forceRefreshFromDB);
            }
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
            List<ACColumnItem> acColumnItemList = new List<ACColumnItem>();
            if (!string.IsNullOrEmpty(acColumns))
            {
                string[] columns = acColumns.Split(',');
                foreach (var column in columns)
                {
                    acColumnItemList.Add(new ACColumnItem(column));
                }
                return acColumnItemList;
            }
            int curColumns = 0;


            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                BuildInheritedColumnsList(ref acColumnItemList, ref curColumns, maxColumns);
            }

            return acColumnItemList;
        }


        #endregion

        #endregion

        #region Private
        /// <summary>
        /// UNSAFE: Returns first found Property in class hierarchy.
        /// </summary>
        /// <param name="acIdentifier">Name of property</param>
        /// <param name="forceRefreshFromDB">Refresh the property cache through reload from database</param>
        /// <returns>ACClassProperty.</returns>
        private ACClassProperty GetPropertyLocked(string acIdentifier, bool forceRefreshFromDB)
        {
            ACClassProperty[] allProperties = LoadPropertyListWithRelations(forceRefreshFromDB);
            if (allProperties != null && allProperties.Any())
            {
                ACClassProperty foundProperty = allProperties.Where(c => c.ACIdentifier == acIdentifier).FirstOrDefault();
                if (foundProperty != null)
                    return foundProperty;
            }
            if (this.ACClass1_BasedOnACClass != null)
            {
                return this.ACClass1_BasedOnACClass.GetPropertyLocked(acIdentifier, forceRefreshFromDB);
            }
            return null;
        }

        /// <summary>
        /// UNSAFE: Returns first found Property in class hierarchy.
        /// </summary>
        /// <returns>ACClassProperty.</returns>
        private ACClassProperty GetPropertyLocked(string acGroup, Global.ACPropUsages acPropType, bool forceRefreshFromDB, bool includeStatic = false)
        {
            ACClassProperty[] allProperties = LoadPropertyListWithRelations(forceRefreshFromDB);
            if (allProperties != null && allProperties.Any())
            {
                ACClassProperty foundProperty = allProperties.Where(c => c.ACGroup == acGroup
                                                                    && c.ACPropUsage == acPropType
                                                                    && (includeStatic ? true : !c.IsStatic))
                                                             .FirstOrDefault();
                if (foundProperty != null)
                    return foundProperty;
            }
            if (this.ACClass1_BasedOnACClass != null)
            {
                return this.ACClass1_BasedOnACClass.GetPropertyLocked(acGroup, acPropType, forceRefreshFromDB, includeStatic);
            }
            return null;
        }

        /// <summary>
        /// UNSAFE: Returns first found Point in class hierarchy.
        /// </summary>
        /// <param name="acIdentifier">Name of property</param>
        /// <param name="forceRefreshFromDB">Refresh the property cache through reload from database</param>
        /// <returns>ACClassProperty.</returns>
        private ACClassProperty GetPointLocked(string acPropertyName, bool forceRefreshFromDB)
        {
            ACClassProperty[] allProperties = LoadPropertyListWithRelations(forceRefreshFromDB);

            if (allProperties != null && allProperties.Any())
            {
                // Points arbeiten immer mit Verweisen auf ACClassProperty in der Basisklasse, wo dieses Deklariert ist
                ACClassProperty foundPoint = allProperties.Where(c => c.ACIdentifier == acPropertyName &&
                                                        (c.ACPropUsageIndex == (Int16)Global.ACPropUsages.EventPoint ||
                                                         c.ACPropUsageIndex == (Int16)Global.ACPropUsages.EventPointSubscr ||
                                                         c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConnectionPoint))
                                                    .FirstOrDefault();
                if (foundPoint != null)
                    return foundPoint.ACClassProperty1_BasedOnACClassProperty;
            }
            if (ACClass1_BasedOnACClass != null)
            {
                ACClassProperty pointProperty = ACClass1_BasedOnACClass.GetPointLocked(acPropertyName, forceRefreshFromDB);
                if (pointProperty == null)
                    return null;
                return pointProperty.ACClassProperty1_BasedOnACClassProperty;
            }
            return null;
        }

        /// <summary>
        /// UNSAFE: Fills Propertylist through hierarchy
        /// </summary>
        private void BuildInheritedPropertyList(ref List<ACClassProperty> acClassPropertyList, ACClass myClass,
            bool forceRefreshFromDB, bool withOverrides, bool onlyBaseProperties, Global.ACPropUsages? propUsage = null, bool includeStatic = false)
        {
            ACClassProperty[] allProperties = LoadPropertyListWithRelations(forceRefreshFromDB);
            if (allProperties != null && allProperties.Any())
            {
                foreach (var acClassProperty in allProperties.Where(c => propUsage.HasValue ? c.ACPropUsage == propUsage : true
                                                                        && (includeStatic ? true : !c.IsStatic))
                                    .OrderBy(c => c.ACIdentifier)
                                    .ToArray())
                {
                    if (onlyBaseProperties && acClassProperty.ACClassPropertyID != acClassProperty.BasedOnACClassPropertyID)
                        continue;
                    else if (withOverrides || !acClassPropertyList.Where(c => c.ACIdentifier == acClassProperty.ACIdentifier).Any())
                    {
                        acClassProperty.MyCurrentAClassOfProperty = myClass;
                        acClassPropertyList.Add(acClassProperty);
                    }
                }
            }
            if (ACClass1_BasedOnACClass != null)
                ACClass1_BasedOnACClass.BuildInheritedPropertyList(ref acClassPropertyList, myClass, forceRefreshFromDB, withOverrides, onlyBaseProperties, propUsage, includeStatic);
        }

        /// <summary>
        /// UNSAFE: Loads ACClassProperty_ACClass-Collection on this Class-Level an includes the most relevant relations
        /// </summary>
        private ACClassProperty[] LoadPropertyListWithRelations(bool forceRefreshFromDB)
        {
            ACClassProperty[] allProperties = null;
            try
            {
                if (!ACClassProperty_ACClass.IsLoaded && EntityState != System.Data.EntityState.Added
                    || forceRefreshFromDB)
                {
                    ACClassProperty_ACClass.Load(forceRefreshFromDB ? MergeOption.OverwriteChanges : MergeOption.AppendOnly);
                    allProperties = ACClassProperty_ACClass.CreateSourceQuery()
                                                .Include(c => c.ValueTypeACClass)
                                                .Include(c => c.ACClass)
                                                .Include(c => c.ACClassProperty1_BasedOnACClassProperty)
                                                .Include(c => c.ACClassProperty1_ParentACClassProperty)
                                                .OrderBy(c => c.ACIdentifier)
                                                .ToArray();
                }
                else
                    allProperties = ACClassProperty_ACClass.ToArray();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClass", "LoadPropertyListWithRelations", msg);
            }
            return allProperties;
        }

        /// <summary>
        /// UNSAFE: Fills Propertylist through hierarchy
        /// </summary>
        private void BuildInheritedColumnsList(ref List<ACColumnItem> acColumnItemList, ref int curColumns, int maxColumns)
        {
            if (ACClass1_BasedOnACClass != null)
                ACClass1_BasedOnACClass.BuildInheritedColumnsList(ref acColumnItemList, ref curColumns, maxColumns);

            foreach (var acClassProperty in ACClassProperty_ACClass.Where(c => !c.IsStatic && c.SortIndex < 9000).OrderBy(c => c.SortIndex))
            {
                acColumnItemList.Add(new ACColumnItem(acClassProperty.ACIdentifier));
                curColumns++;
                if (curColumns >= maxColumns)
                    return;
            }
        }
        #endregion

        #endregion


        #region ACClassDesign

        #region Public
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
                return GetDesigns(false, true);
            }
        }

        /// <summary>
        /// Returns all Designs over complete class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="forceRefreshFromDB"></param>
        /// <param name="withOverrides"></param>
        /// <returns>New threadsafe list</returns>
        public List<ACClassDesign> GetDesigns(bool forceRefreshFromDB = false, bool withOverrides = false)
        {
            List<ACClassDesign> acClassDesignList = new List<ACClassDesign>();

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                BuildInheritedDesignList(ref acClassDesignList, forceRefreshFromDB, withOverrides);
            }
            return acClassDesignList;
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

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return GetDesignLocked(acIdentifier, forceRefreshFromDB);
            }
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
            if (acObject is ACClass)
            {
                ACClass acClass = acObject as ACClass;

                //if (database != this.Database)
                //{
                //    string message = "Thread-critical: acObject and acClass are from different Database-contexts";
                //    Database.Root.Messages.LogException("ACClass.GetDesign()", "0", message);
                //    #if DEBUG
                //    throw new ArgumentException(message);
                //    #endif
                //}


                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    if (!string.IsNullOrEmpty(vbDesignName))
                    {
                        ACClassDesign acClassDesign = acClass.GetDesignLocked(vbDesignName, false);
                        if (acClassDesign != null)
                            return acClassDesign;
                        else
                        {
                            acClassDesign = this.Database.ContextIPlus.ACClassDesign.Where(c => c.ACIdentifier == Const.UnknownDesign && c.ACClass.ACIdentifier == Const.UnknownClass).FirstOrDefault();
                            return acClassDesign;
                        }
                    }
                    var query2 = acClass.ACClassDesign_ACClass.Where(c => c.ACUsageIndex == (int)acUsage).OrderBy(c => c.SortIndex);
                    if (msg != null && query2.Any())
                    {
                        short prevSortIndex = -9999;
                        foreach (var design in query2)
                        {
                            if (prevSortIndex == design.SortIndex)
                            {
                                // "The SortIndex {1} of Design {0} is assigned many times to different Designs. A not unique SortIndex leads into unpredictable selection of the Standard-Design for this class."
                                msg.AddDetailMessage(new Msg()
                                {
                                    MessageLevel = eMsgLevel.Warning,
                                    Source = acClass.GetACUrl(),
                                    ACIdentifier = design.ACIdentifier,
                                    Message = gip.core.datamodel.Database.Root.Environment.TranslateMessage(gip.core.datamodel.Database.Root, "Warning00003", design.ACIdentifier, design.SortIndex)
                                });
                            }
                            prevSortIndex = design.SortIndex;
                        }
                    }


                    var query = acClass.ConfigurationEntries.Where(c => c.LocalConfigACUrl == "Design_" + acUsage.ToString());
                    if (query.Any())
                    {
                        IACConfig acClassConfig = query.First();
                        ACComposition acComposition = acClassConfig[Const.Value] as ACComposition;
                        var acUrlDesign = acComposition.ACUrlComposition;
                        if (acUrlDesign.StartsWith(Const.ContextDatabase + "\\"))
                            acUrlDesign = acUrlDesign.Substring(9);
                        return this.Database.ACUrlCommand(acUrlDesign) as ACClassDesign;
                    }
                    if (query2.Any())
                    {
                        return query2.First();
                    }

                    if (acClass.ACClass1_BasedOnACClass == null)
                        return null;

                    return acClass.ACClass1_BasedOnACClass.GetDesign(acClass.ACClass1_BasedOnACClass, acUsage, acKind, vbDesignName, msg);
                }
            }
            else if (acObject is IACClassDesignProvider)
            {
                IACClassDesignProvider designProvider = acObject as IACClassDesignProvider;
                return designProvider.GetDesign(acUsage, acKind, vbDesignName);
            }
            if (acObject is IACType)
            {
                IACType acType = acObject as IACType;
                return acType.GetDesign(acType, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
            }
            else if (acObject is ACObjectItem)
                return null;
            else
            {

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    if (!string.IsNullOrEmpty(vbDesignName))
                    {
                        ACClassDesign acClassDesign = this.GetDesignLocked(vbDesignName, false);
                        if (acClassDesign != null)
                            return acClassDesign;
                    }

                    var query = this.ConfigurationEntries.Where(c => c.LocalConfigACUrl == "Design_" + acUsage.ToString());
                    if (query.Any())
                    {
                        IACConfig acClassConfig = query.First();
                        ACComposition acComposition = acClassConfig[Const.Value] as ACComposition;
                        var acUrlDesign = acComposition.ACUrlComposition;
                        if (acUrlDesign.StartsWith(Const.ContextDatabase + "\\"))
                            acUrlDesign = acUrlDesign.Substring(9);
                        return this.Database.ACUrlCommand(acUrlDesign) as ACClassDesign;
                    }
                    var query2 = this.ACClassDesign_ACClass.Where(c => c.ACUsageIndex == (int)acUsage).OrderBy(c => c.SortIndex);
                    if (query2.Any())
                        return query2.First();

                    if (this.ACClass1_BasedOnACClass == null)
                        return null;

                    return this.ACClass1_BasedOnACClass.GetDesign(this.ACClass1_BasedOnACClass, acUsage, acKind, vbDesignName);
                }
            }
        }

        #endregion

        #region Private
        /// <summary>
        /// UNSAFE: Returns first found Design in class hierarchy.
        /// </summary>
        /// <param name="acIdentifier">Name of design</param>
        /// <param name="forceRefreshFromDB">Refresh the design cache through reload from database</param>
        /// <returns>ACClassProperty.</returns>
        private ACClassDesign GetDesignLocked(string acIdentifier, bool forceRefreshFromDB)
        {
            ACClassDesign[] allDesigns = LoadDesignListWithRelations(forceRefreshFromDB);
            if (allDesigns != null && allDesigns.Any())
            {
                ACClassDesign foundDesign = allDesigns.Where(c => c.ACIdentifier == acIdentifier).FirstOrDefault();
                if (foundDesign != null)
                    return foundDesign;
            }
            if (this.ACClass1_BasedOnACClass != null)
            {
                return this.ACClass1_BasedOnACClass.GetDesignLocked(acIdentifier, forceRefreshFromDB);
            }
            return null;
        }

        /// <summary>
        /// UNSAFE
        /// </summary>
        private void BuildInheritedDesignList(ref List<ACClassDesign> acClassDesignList, bool forceRefreshFromDB, bool withOverrides)
        {
            ACClassDesign[] allDesigns = LoadDesignListWithRelations(forceRefreshFromDB);
            foreach (var acClassDesign in allDesigns.OrderBy(c => c.ACKindIndex).ThenBy(c => c.SortIndex).ThenBy(c => c.ACIdentifier))
            {
                if (withOverrides || !acClassDesignList.Where(c => c.ACIdentifier == acClassDesign.ACIdentifier).Any())
                    acClassDesignList.Add(acClassDesign);
            }

            if (ACClass1_BasedOnACClass != null)
                ACClass1_BasedOnACClass.BuildInheritedDesignList(ref acClassDesignList, forceRefreshFromDB, withOverrides);
        }

        /// <summary>
        /// UNSAFE: Loads ACClassDesign_ACClass-Collection on this Class-Level an includes the most relevant relations
        /// </summary>
        private ACClassDesign[] LoadDesignListWithRelations(bool forceRefreshFromDB)
        {
            ACClassDesign[] allDesigns = null;
            try
            {
                if (!ACClassDesign_ACClass.IsLoaded && EntityState != System.Data.EntityState.Added
                    || forceRefreshFromDB)
                {
                    ACClassDesign_ACClass.Load(forceRefreshFromDB ? MergeOption.OverwriteChanges : MergeOption.AppendOnly);
                    allDesigns = ACClassDesign_ACClass.CreateSourceQuery()
                                                .Include(c => c.ValueTypeACClass)
                                                .Include(c => c.ACClass)
                                                .OrderBy(c => c.ACIdentifier)
                                                .ToArray();
                }
                else
                    allDesigns = ACClassDesign_ACClass.ToArray();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClass", "LoadDesignListWithRelations", msg);
            }
            return allDesigns;
        }

        #endregion

        #endregion


        #region ACClassText

        #region public
        /// <summary>
        /// Returns all Texts over complete class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>My AC class text list.</value>
        public IEnumerable<ACClassText> Texts
        {
            get
            {
                return GetTexts();
            }
        }

        /// <summary>
        /// Returns all Texts over complete class hierarchy including overridden Texts
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="forceRefreshFromDB"></param>
        /// <param name="withOverrides"></param>
        /// <returns>New threadsafe list</returns>
        public List<ACClassText> GetTexts(bool forceRefreshFromDB = false, bool withOverrides = true)
        {
            List<ACClassText> acClassTextList = new List<ACClassText>();

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                BuildInheritedTextList(ref acClassTextList, forceRefreshFromDB, withOverrides);
            }
            return acClassTextList;
        }

        /// <summary>
        /// Returns first found text in class hierarchy.
        /// THREAD-SAFE while using QueryLock_1X000
        /// </summary>
        /// <param name="acIdentifier">Name of text</param>
        /// <param name="forceRefreshFromDB">Refresh the property cache through reload from database</param>
        /// <returns>ACClassText.</returns>
        public ACClassText GetText(string acIdentifier, bool forceRefreshFromDB = false)
        {
            ACClassText foundText = null;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                foundText = GetTextLocked(acIdentifier, forceRefreshFromDB);
                if (foundText == null)
                {
                    string lower = acIdentifier.ToLower();
                    foundText = (Database.Root.ACType as ACClass).ACClassText_ACClass.Where(c => c.ACIdentifier.ToLower() == lower).FirstOrDefault();
                }
            }
            return foundText;
        }

        #endregion

        #region private
        /// <summary>
        /// UNSAFE: Returns first found Text in class hierarchy.
        /// </summary>
        /// <param name="acIdentifier">Name of design</param>
        /// <param name="forceRefreshFromDB">Refresh the design cache through reload from database</param>
        /// <returns>ACClassProperty.</returns>
        private ACClassText GetTextLocked(string acIdentifier, bool forceRefreshFromDB)
        {
            ACClassText[] allTexts = LoadTextListWithRelations(forceRefreshFromDB);
            if (allTexts != null && allTexts.Any())
            {
                string lower = acIdentifier.ToLower();
                ACClassText foundText = allTexts.Where(c => c.ACIdentifier.ToLower() == lower).FirstOrDefault();
                if (foundText != null)
                    return foundText;
            }
            if (this.ACClass1_BasedOnACClass != null)
            {
                return this.ACClass1_BasedOnACClass.GetTextLocked(acIdentifier, forceRefreshFromDB);
            }
            return null;
        }

        /// <summary>
        /// UNSAFE
        /// </summary>
        protected void BuildInheritedTextList(ref List<ACClassText> acClassTextList, bool forceRefreshFromDB, bool withOverrides)
        {
            ACClassText[] allTexts = LoadTextListWithRelations(forceRefreshFromDB);
            foreach (var acClassText in allTexts)
            {
                if (withOverrides || !acClassTextList.Where(c => c.ACIdentifier == acClassText.ACIdentifier).Any())
                    acClassTextList.Add(acClassText);
            }

            if (ACClass1_BasedOnACClass != null)
                ACClass1_BasedOnACClass.BuildInheritedTextList(ref acClassTextList, forceRefreshFromDB, withOverrides);
        }

        /// <summary>
        /// UNSAFE: Loads ACClassDesign_ACClass-Collection on this Class-Level an includes the most relevant relations
        /// </summary>
        private ACClassText[] LoadTextListWithRelations(bool forceRefreshFromDB)
        {
            ACClassText[] allTexts = null;
            try
            {
                if (!ACClassText_ACClass.IsLoaded && EntityState != System.Data.EntityState.Added
                    || forceRefreshFromDB)
                {
                    ACClassText_ACClass.Load(forceRefreshFromDB ? MergeOption.OverwriteChanges : MergeOption.AppendOnly);
                    allTexts = ACClassText_ACClass.CreateSourceQuery()
                                                .Include(c => c.ACClass)
                                                .OrderBy(c => c.ACIdentifier)
                                                .ToArray();
                }
                else
                    allTexts = ACClassText_ACClass.ToArray();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClass", "LoadTextListWithRelations", msg);
            }
            return allTexts;
        }
        #endregion

        #endregion


        #region ACClassMessage

        #region Public
        /// <summary>
        /// Returns all Messages over complete class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>ACClassDesign List</value>
        public IEnumerable<ACClassMessage> Messages
        {
            get
            {
                return GetMessages();
            }
        }

        /// <summary>
        /// Returns all Messages over complete class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="forceRefreshFromDB"></param>
        /// <param name="withOverrides"></param>
        /// <returns>New threadsafe list</returns>
        public List<ACClassMessage> GetMessages(bool forceRefreshFromDB = false, bool withOverrides = true)
        {

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                List<ACClassMessage> acClassMessageList = new List<ACClassMessage>();
                BuildInheritedMessageList(ref acClassMessageList, false, withOverrides);
                return acClassMessageList;
            }
        }

        /// <summary>
        /// Returns the first message which matches the identifier over complete class hierarchy
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="acIdentifier"></param>
        /// <param name="forceRefreshFromDB"></param>
        /// <returns></returns>
        public ACClassMessage GetMessage(string acIdentifier, bool forceRefreshFromDB = false)
        {
            ACClassMessage foundMessage = null;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                foundMessage = GetMessageLocked(acIdentifier, forceRefreshFromDB);
                if (foundMessage == null)
                {
                    foundMessage = (Database.Root.ACType as ACClass).ACClassMessage_ACClass.Where(c => c.ACIdentifier == acIdentifier).FirstOrDefault();
                }
            }
            return foundMessage;
        }


        #endregion

        #region Private

        /// <summary>
        /// UNSAFE: Returns first found ACClassMessage in class hierarchy.
        /// </summary>
        /// <param name="acIdentifier">Name of design</param>
        /// <param name="forceRefreshFromDB">Refresh the design cache through reload from database</param>
        /// <returns>ACClassProperty.</returns>
        private ACClassMessage GetMessageLocked(string acIdentifier, bool forceRefreshFromDB)
        {
            ACClassMessage[] allMessages = LoadMessageListWithRelations(forceRefreshFromDB);
            if (allMessages != null && allMessages.Any())
            {
                ACClassMessage foundMessage = allMessages.Where(c => c.ACIdentifier == acIdentifier).FirstOrDefault();
                if (foundMessage != null)
                    return foundMessage;
            }
            if (this.ACClass1_BasedOnACClass != null)
            {
                return this.ACClass1_BasedOnACClass.GetMessageLocked(acIdentifier, forceRefreshFromDB);
            }
            return null;
        }


        /// <summary>
        /// UNSAFE: Fills messagelist through hierarchy
        /// </summary>
        protected void BuildInheritedMessageList(ref List<ACClassMessage> acClassMessageList, bool forceRefreshFromDB, bool withOverrides)
        {
            ACClassMessage[] allMessages = LoadMessageListWithRelations(forceRefreshFromDB);
            foreach (var acClassMessage in allMessages)
            {
                if (withOverrides || !acClassMessageList.Where(c => c.ACIdentifier == acClassMessage.ACIdentifier).Any())
                    acClassMessageList.Add(acClassMessage);
            }

            if (ACClass1_BasedOnACClass != null)
                ACClass1_BasedOnACClass.BuildInheritedMessageList(ref acClassMessageList, forceRefreshFromDB, withOverrides);
        }

        /// <summary>
        /// UNSAFE: Loads ACClassProperty_ACClass-Collection on this Class-Level an includes the most relevant relations
        /// </summary>
        private ACClassMessage[] LoadMessageListWithRelations(bool forceRefreshFromDB)
        {
            ACClassMessage[] allMessages = null;
            try
            {
                if (!ACClassMessage_ACClass.IsLoaded && EntityState != System.Data.EntityState.Added
                    || forceRefreshFromDB)
                {
                    ACClassMessage_ACClass.Load(forceRefreshFromDB ? MergeOption.OverwriteChanges : MergeOption.AppendOnly);
                    allMessages = ACClassMessage_ACClass.CreateSourceQuery()
                                                .Include(c => c.ACClass)
                                                .OrderBy(c => c.ACIdentifier)
                                                .ToArray();
                }
                else
                    allMessages = ACClassMessage_ACClass.ToArray();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACClass", "LoadMessageListWithRelations", msg);
            }
            return allMessages;
        }

        #endregion

        #endregion


        #region ACClassComposition

        private bool _PrimaryNavigationqueryChecked = false;
        private ACClass _PrimaryNavigationquery = null;
        /// <summary>
        /// PrimaryNavigationquery
        /// </summary>
        /// <returns>ACClass.</returns>
        public ACClass PrimaryNavigationquery()
        {
            if (_PrimaryNavigationquery != null)
                return _PrimaryNavigationquery;
            if (_PrimaryNavigationqueryChecked)
                return null;
            _PrimaryNavigationqueryChecked = true;
            if (!ObjectType.IsEnum)
            {
                var query = ConfigurationEntries.Where(c => c.KeyACUrl == Const.KeyACUrl_NavigationqueryList).ToArray();
                if (!query.Any())
                {
                    var baseClass = BaseClass;
                    if (baseClass != null)
                    {
                        _PrimaryNavigationquery = baseClass.PrimaryNavigationquery();
                        return _PrimaryNavigationquery;
                    }
                    return null;
                }

                var acConfig = query.First();
                ACComposition acComposition = acConfig[Const.Value] as ACComposition;
                ACClass queryClass = acComposition.GetComposition(this.Database) as ACClass;
                if (queryClass == null)
                {
                    var baseClass = BaseClass;
                    if (baseClass != null)
                    {
                        _PrimaryNavigationquery =  baseClass.PrimaryNavigationquery();
                        return _PrimaryNavigationquery;
                    }
                }
                _PrimaryNavigationquery = queryClass;
                return _PrimaryNavigationquery;
            }
            _PrimaryNavigationquery = this.Database.GetACType(Const.QueryPrefix + typeof(ACValueItem).Name);
            return _PrimaryNavigationquery;
        }


        private bool _ManagingBSOChecked = false;
        private ACClass _ManagingBSO = null;
        /// <summary>
        /// Gets my AC class BSO.
        /// </summary>
        /// <value>My AC class BSO.</value>
        public ACClass ManagingBSO
        {
            get
            {
                if (_ManagingBSO != null)
                    return _ManagingBSO;
                if (_ManagingBSOChecked)
                    return null;
                _ManagingBSOChecked = true;
                if (ObjectType.IsEnum)
                    return null;

                if (!BussinessobjectList.Any())
                    return null;

                var acConfig = BussinessobjectList.First();
                ACComposition acComposition = acConfig[Const.Value] as ACComposition;
                _ManagingBSO = acComposition.GetComposition(this.Database) as ACClass;
                return _ManagingBSO;
            }
        }


        private bool _ACClassForEnumListChecked = false;
        private ACClass _ACClassForEnumList = null;
        /// <summary>
        /// Gets my AC class BSO.
        /// </summary>
        /// <value>My AC class BSO.</value>
        public ACClass ACClassForEnumList
        {
            get
            {
                if (_ACClassForEnumList != null)
                    return _ACClassForEnumList;
                if (_ACClassForEnumListChecked)
                    return null;
                _ACClassForEnumListChecked = true;
                if (!ObjectType.IsEnum)
                    return null;

                var acConfig = ConfigurationEntries.Where(c => c.KeyACUrl == Const.KeyACUrl_EnumACValueList).FirstOrDefault();
                if (acConfig == null)
                    return null;
                ACComposition acComposition = acConfig[Const.Value] as ACComposition;
                _ACClassForEnumList = acComposition.GetComposition(this.Database) as ACClass;
                return _ACClassForEnumList;
            }
        }

        private bool _ACValueListForEnumChecked = false;
        private ACValueItemList _ACValueListForEnum = null;
        public ACValueItemList ACValueListForEnum
        {
            get
            {
                if (_ACValueListForEnum != null)
                    return _ACValueListForEnum;
                if (_ACValueListForEnumChecked)
                    return null;
                _ACValueListForEnumChecked = true;
                ACClass classForEnumList = ACClassForEnumList;
                if (classForEnumList == null)
                    return null;
                try
                {
                    _ACValueListForEnum = Activator.CreateInstance(classForEnumList.ObjectType, false) as ACValueItemList;
                    return _ACValueListForEnum;
                }
                catch (Exception e)
                {
                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACClass", "ACValueListForEnum", e);
                }
                return null;
            }
        }

        #endregion


        #region Type-Informations

        /// <summary>
        /// Returns the category of this class
        /// </summary>
        public Global.ACKinds ACKind
        {
            get
            {
                if (ACKindIndex == 0)
                    UpdateACKindIndex();
                return (Global.ACKinds)this.ACKindIndex;
            }
            set
            {
                ACKindIndex = (short)value;
            }
        }


        /// <summary>
        /// Mode which decribes how and when this class will be instanced
        /// </summary>
        public Global.ACStartTypes ACStartType
        {
            get
            {
                return (Global.ACStartTypes)ACStartTypeIndex;
            }
            set
            {
                ACStartTypeIndex = (Int16)value;
            }
        }


        /// <summary>
        /// Information about Persistence-Behaviour
        /// </summary>
        public Global.ACStorableTypes ACStorableType
        {
            get
            {
                return (Global.ACStorableTypes)ACStorableTypeIndex;
            }
            set
            {
                ACStorableTypeIndex = (Int16)value;
            }
        }

        /// <summary>
        /// ACGroup
        /// </summary>
        public String ACGroup
        {
            get
            {

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return ACPackage.ACPackageName;
                }
            }
        }


        /// <summary>
        /// Is this type a Interface
        /// </summary>
        [ACPropertyInfo(9999, "", "en{'Interface'}de{'Schnittstelle'}", "", true)]
        public bool IsInterface
        {
            get
            {
                return ACKind == Global.ACKinds.TACInterface;
            }
        }

        /// <summary>
        /// Returns a long name over complete derivation-hierarchy
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        public string FullClassCaption
        {
            get
            {
                string fullClassCaption = ACIdentifier;


                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    if (this.ACProject.ACProjectType == Global.ACProjectTypes.Application
                        || this.ACProject.ACProjectType == Global.ACProjectTypes.AppDefinition
                        || this.ACProject.ACProjectType == Global.ACProjectTypes.Service)
                    {
                        ACClass acClass = ACClass1_ParentACClass;
                        while (acClass != null)
                        {
                            fullClassCaption = acClass.ACIdentifier + ">" + fullClassCaption;
                            acClass = acClass.ACClass1_ParentACClass;
                        }
                        fullClassCaption = ">" + fullClassCaption;
                    }
                }
                return fullClassCaption;
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance is class instantiable as proxy.
        /// </summary>
        /// <value><c>true</c> if this instance is class instantiable as proxy; otherwise, <c>false</c>.</value>
        public bool IsClassInstantiableAsProxy
        {
            get
            {
                if (ACKind == Global.ACKinds.TPAModule ||
                    ACKind == Global.ACKinds.TPAProcessModule ||
                    ACKind == Global.ACKinds.TPAProcessFunction ||
                    ACKind == Global.ACKinds.TPABGModule ||
                    ACKind == Global.ACKinds.TPWGroup ||
                    ACKind == Global.ACKinds.TPWNode ||
                    ACKind == Global.ACKinds.TPWNodeMethod ||
                    ACKind == Global.ACKinds.TPWNodeWorkflow ||
                    ACKind == Global.ACKinds.TPWNodeStart ||
                    ACKind == Global.ACKinds.TPWNodeEnd ||
                    ACKind == Global.ACKinds.TPWNodeStatic)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is workflow type.
        /// </summary>
        /// <value><c>true</c> if this instance is workflow type; otherwise, <c>false</c>.</value>
        public bool IsWorkflowType
        {
            get
            {
                if (ACKind == Global.ACKinds.TPWGroup ||
                    ACKind == Global.ACKinds.TPWMethod ||
                    ACKind == Global.ACKinds.TPWNode ||
                    ACKind == Global.ACKinds.TPWNodeMethod ||
                    ACKind == Global.ACKinds.TPWNodeWorkflow ||
                    ACKind == Global.ACKinds.TPWNodeStart ||
                    ACKind == Global.ACKinds.TPWNodeEnd ||
                    ACKind == Global.ACKinds.TPWNodeStatic)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is application type.
        /// </summary>
        /// <value><c>true</c> if this instance is application type; otherwise, <c>false</c>.</value>
        public bool IsApplicationType
        {
            get
            {
                if (ACKind == Global.ACKinds.TACApplicationManager ||
                    ACKind == Global.ACKinds.TPAModule ||
                    ACKind == Global.ACKinds.TPAProcessModule ||
                    ACKind == Global.ACKinds.TPAProcessFunction ||
                    ACKind == Global.ACKinds.TPABGModule ||
                    ACKind == Global.ACKinds.TPARole)
                    return true;
                return false;
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


        /// <summary>
        /// Tooltip
        /// </summary>
        [ACPropertyInfo(9999)]
        public string Tooltip
        {
            get
            {
                string tooltip = ACCaption;
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
        /// Self type
        /// </summary>
        public ACClass ValueTypeACClass
        {
            get { return this; }
        }

        /// <summary>
        /// AssemblyACClassInfo
        /// </summary>
        public ACClassInfo AssemblyACClassInfo
        {
            get
            {
                Type classType = ObjectType;
                if (classType == null)
                {
                    return new ACClassInfo("", "unknown", Global.ACKinds.TACUndefined, Global.ACStorableTypes.NotStorable, true, false);
                }
                else
                {
                    object[] attributes = classType.GetCustomAttributes(typeof(ACClassInfo), false);

                    if (attributes.Any())
                    {
                        return classType.GetCustomAttributes(typeof(ACClassInfo), false)[0] as ACClassInfo;
                    }
                    else
                    {
                        if (classType.BaseType != null && classType.BaseType.Name == "EntityObject")
                            return new ACClassInfo("", classType.Name, Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true);
                        else
                            return new ACClassInfo("", classType.Name, Global.ACKinds.TACUndefined, Global.ACStorableTypes.NotStorable, true, false);
                    }
                }
            }
        }

        Type _ObjectType = null;
        /// <summary>
        /// Returns the .NET-Type (If Property is a generic it returns the inner type)
        /// </summary>
        public Type ObjectType
        {
            get
            {
                if (_ObjectType == null)
                {
                    ACClass myAssemblyClass = BaseClassWithASQN;
                    if (myAssemblyClass == null)
                        return null;
                    _ObjectType = Type.GetType(myAssemblyClass.AssemblyQualifiedName);
                }
                return _ObjectType;
            }
        }


        /// <summary>
        /// Returns the .NET-Type (If Property is a generic it returns the outer+inner type)
        /// </summary>
        public Type ObjectFullType
        {
            get
            {
                return ObjectType;
            }
        }

        /// <summary>
        /// Returns the .NET-Type  of the parent object in a composition tree
        /// </summary>
        public Type ObjectTypeParent
        {
            get
            {
                return ObjectType; // ACClass.ObjectType;
            }
        }

        #endregion


        #region Misc Properties and Methods

        //#region IACWorkflowObject Member

        //public Guid VisualACObjectID
        //{
        //    get
        //    {
        //        return ACClassID;
        //    }
        //    set
        //    {
        //        ACClassID = value;
        //    }
        //}

        //[ACPropertyInfo(9999, "", "", "", true)]
        //public IACWorkflowNode VisualGroup
        //{
        //    get
        //    {
        //        return null;
        //    }
        //    set
        //    {
        //    }
        //}

        //public string XName
        //{
        //    get
        //    {
        //        return null;
        //    }
        //}

        //#endregion


        #region Rightmanagement
        /// <summary>
        /// Determines whether [is any child with rightmanagement].
        /// </summary>
        /// <returns><c>true</c> if [is any child with rightmanagement]; otherwise, <c>false</c>.</returns>
        public bool IsAnyChildWithRightmanagement
        {
            get
            {
                if (this.IsRightmanagement)
                    return true;
                if (Designs.Where(c => c.IsRightmanagement).Any()
                    || Methods.Where(c => c.IsRightmanagement).Any()
                    || Properties.Where(c => c.IsRightmanagement).Any())
                    return true;
                foreach (var acClass in Childs)
                {
                    if (acClass.IsAnyChildWithRightmanagement)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// The _ right manager
        /// </summary>
        ClassRightManager _RightManager = null;
        /// <summary>
        /// Gets the right manager.
        /// </summary>
        /// <value>The right manager.</value>
        public ClassRightManager RightManager
        {
            get
            {
                if (_RightManager != null)
                    return _RightManager;

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    if (_RightManager == null)
                        _RightManager = Database.Root.Environment.GetClassRightManager(this);
                }
                return _RightManager;
            }
        }

        /// <summary>
        /// Gets the right.
        /// </summary>
        /// <param name="rightItem">The right item.</param>
        /// <returns>Global.ControlModes.</returns>
        public Global.ControlModes GetRight(IACType rightItem)
        {
            if (!rightItem.IsRightmanagement)
                return Global.ControlModes.Enabled;
            return RightManager.GetControlMode(rightItem);
        }
        #endregion


        #region ACParameter
        /// <summary>
        /// Gets or sets the AC parameter.
        /// </summary>
        /// <value>The AC parameter.</value>
        public ACValueList ACParameter
        {
            get
            {
                string xml = GetConstructorParamsXML();
                if (string.IsNullOrEmpty(xml))
                    return new ACValueList();
                return DeserializeACClass(xml);
            }
            set
            {
                string xmlACClass = SerializeACClass(value);
                if (XMLACClass != xmlACClass)
                    XMLACClass = xmlACClass;
            }
        }

        /// <summary>
        /// Gets the AC parameter.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>ACValueList.</returns>
        public ACValueList GetACParameter(object[] parameters)
        {
            ACValueList acValueList = null;
            if (parameters != null && parameters.Any())
            {
                if (parameters[0] is ACValueList)
                    return parameters[0] as ACValueList;
                acValueList = ACParameter;
                if (acValueList != null && acValueList.Count <= parameters.Count())
                {
                    for (int i = 0; i < acValueList.Count(); i++)
                    {
                        acValueList[i].Value = parameters[i];
                    }
                }
            }
            return acValueList;
        }

        /// <summary>
        /// Serializes the AC class.
        /// </summary>
        /// <param name="acValueList">The ac value list.</param>
        /// <returns>System.String.</returns>
        public static string SerializeACClass(ACValueList acValueList)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(ACValueList), ACKnownTypes.GetKnownType(), 99999999, true, true, null, ACConvert.MyDataContractResolver);
            StringBuilder sb1 = new StringBuilder();
            using (StringWriter sw1 = new StringWriter(sb1))
            using (XmlTextWriter xmlWriter1 = new XmlTextWriter(sw1))
            {
                serializer.WriteObject(xmlWriter1, acValueList);
                return sw1.ToString();
            }
        }

        /// <summary>
        /// Deserializes the AC class.
        /// </summary>
        /// <param name="acClassXML">The ac class XML.</param>
        /// <returns>ACValueList.</returns>
        public static ACValueList DeserializeACClass(string acClassXML)
        {
            if (string.IsNullOrEmpty(acClassXML))
                return null;
            using (StringReader ms = new StringReader(acClassXML))
            using (XmlTextReader xmlReader = new XmlTextReader(ms))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ACValueList), ACKnownTypes.GetKnownType(), 99999999, true, true, null, ACConvert.MyDataContractResolver);
                ACValueList acValueList = (ACValueList)serializer.ReadObject(xmlReader);
                return acValueList;
            }
        }

        /// <summary>
        /// Mies the XMLAC class.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetConstructorParamsXML()
        {
            if (!string.IsNullOrEmpty(XMLACClass))
                return XMLACClass;
            if (ACClass1_BasedOnACClass != null)
                return ACClass1_BasedOnACClass.GetConstructorParamsXML();
            return null;
        }

        public override string ToString()
        {
            return ACCaption;
        }
        #endregion


        #region Private Methods

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

        private void UpdateACKindIndex()
        {
            short acKindIndex;

            if (ACIdentifier == Const.UnknownClass)
                return;
            if (!string.IsNullOrEmpty(this.AssemblyQualifiedName))
            {
                acKindIndex = this.ACKindIndex;
            }
            else if (ACClass1_BasedOnACClass == null)
            {
                acKindIndex = (short)Global.ACKinds.TACClass;
            }
            else
            {
                acKindIndex = (short)ACClass1_BasedOnACClass.ACKind;
            }
            if (acKindIndex == 0)
                acKindIndex = (short)Global.ACKinds.TACClass;

            if (ACKindIndex != acKindIndex && acKindIndex != (short)Global.ACKinds.TACAbstractClass)
                ACKindIndex = acKindIndex;
        }

        private void RefreshChildrenACUrlCacheLocked()
        {
            using (ACMonitor.Lock(Database.QueryLock_1X000))
            {
                if (this.ACProject.ACProjectType == Global.ACProjectTypes.Application || this.ACProject.ACProjectType == Global.ACProjectTypes.Service)
                {
                    this.ACURLComponentCached = GetACUrlComponent();
                    this._ACUrlComponentCached = this.ACURLComponentCached;
                }

                this.ACURLCached = GetACUrl();
            }

            foreach (ACClass child in Childs)
                child.RefreshChildrenACUrlCacheLocked();
        }

        #endregion

        #region Public methods

        public void RefreshChildrenACURLCache()
        {
            RefreshChildrenACUrlCacheLocked();

            OnPropertyChanged("ACUrl");
            OnPropertyChanged("ACUrlComponent");
        }

        #endregion

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
                    configStoreName = Translator.GetTranslation(acClassInfo.ACCaptionTranslation);
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
                return null;
            }
        }

        /// <summary>
        /// Returns all configuration-entries for this class
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="acObject"></param>
        /// <returns></returns>
        public IEnumerable<IACConfig> GetConfigListOfType(IACObjectEntity acObject = null)
        {
            ACClass acClass = acObject as ACClass;
            if (acClass == null)
                acClass = this;
            if (acClass != this)
            {
                using (ACMonitor.Lock(acClass.Database.QueryLock_1X000))
                {
                    if (acClass.ACClassConfig_ACClass.IsLoaded)
                    {
                        acClass.ACClassConfig_ACClass.AutoRefresh(acClass.Database);
                        acClass.ACClassConfig_ACClass.AutoLoad(acClass.Database);
                    }
                    return acClass.ACClassConfig_ACClass.ToList().Select(x => (IACConfig)x).Where(c => c.KeyACUrl == ACConfigKeyACUrl);
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
                if (ACClassConfig_ACClass.IsLoaded)
                {
                    ACClassConfig_ACClass.AutoRefresh(this.Database);
                    this.ACClassConfig_ACClass.AutoLoad(this.Database);
                }
                //newSafeList = new SafeList<IACConfig>(ACClassConfig_ACClass.ToList().Select(x => (IACConfig)x).Where(c => c.KeyACUrl == ACConfigKeyACUrl));
                newSafeList = new SafeList<IACConfig>(ACClassConfig_ACClass);
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
        /// <returns>IACConfig as a new entry</returns>
        public IACConfig NewACConfig(IACObjectEntity acObject = null, gip.core.datamodel.ACClass valueTypeACClass = null, string localConfigACUrl = null)
        {
            ACClassConfig acConfig = null;
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                acConfig = ACClassConfig.NewACObject(this.Database, this);
                acConfig.KeyACUrl = ACConfigKeyACUrl;
                acConfig.LocalConfigACUrl = localConfigACUrl;
                acConfig.ValueTypeACClass = valueTypeACClass;
                ACClassConfig_ACClass.Add(acConfig);
            }
            ACConfigListCache.Add(acConfig);
            return acConfig;
        }

        /// <summary>Removes a configuration from ConfigurationEntries and the database-context.</summary>
        /// <param name="acObject">Entry as IACConfig</param>
        public void RemoveACConfig(IACConfig acObject)
        {
            ACClassConfig acConfig = acObject as ACClassConfig;
            if (acConfig == null)
                return;
            if (ACConfigListCache.Contains(acConfig))
                ACConfigListCache.Remove(acConfig);
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                ACClassConfig_ACClass.Remove(acConfig);
                if (acConfig.EntityState != System.Data.EntityState.Detached)
                    acConfig.DeleteACObject(this.Database, false);
            }
        }

        /// <summary>
        /// Deletes all IACConfig-Entries in the Database-Context as well as in ConfigurationEntries.
        /// </summary>
        public void DeleteAllConfig()
        {
            if (!ACConfigListCache.Any())
                return;
            // Clear for reloading from database
            ClearCacheOfConfigurationEntries();
            var list = ACConfigListCache.ToArray();
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                foreach (var acConfig in list)
                {
                    (acConfig as ACClassConfig).DeleteACObject(this.Database, false);
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

        private SafeList<IACConfig> _ACConfigListCache;
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

        public bool ValidateConfigurationEntriesWithDB(ConfigEntriesValidationMode mode = ConfigEntriesValidationMode.AnyCheck)
        {
            if (mode == ConfigEntriesValidationMode.AnyCheck)
            {
                if (ConfigurationEntries.Any())
                    return true;
            }
            using (Database database = new Database())
            {
                var query = database.ACClassConfig.Where(c => c.ACClassID == this.ACClassID && c.KeyACUrl == ACConfigKeyACUrl);
                if (mode == ConfigEntriesValidationMode.AnyCheck)
                {
                    if (query.Any())
                        return false;
                }
                else if (   mode == ConfigEntriesValidationMode.CompareCount
                         || mode == ConfigEntriesValidationMode.CompareContent)
                    return query.Count() == ConfigurationEntries.Count();
            }
            return true;
        }

        #endregion
    }
}
