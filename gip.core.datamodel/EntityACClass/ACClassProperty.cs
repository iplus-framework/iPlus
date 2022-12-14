// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-08-2012
// ***********************************************************************
// <copyright file="ACClassProperty.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace gip.core.datamodel
{
    /// <summary>Metadata for describing and defining a property of a ACComponent.
    /// Entries in the ACClassProperty table are mainly added by reflection when starting iPlus with the Ctrl key. 
    /// All properties that are marked with the ACPropertyInfo attribute classes in the source code are entered into this table. 
    /// Properties added by iPlus development environment are also entered in this table. 
    /// </summary>    
    /// <seealso cref="gip.core.datamodel.VBEntityObject" />
    /// <seealso cref="gip.core.datamodel.IACObjectEntityWithCheckTrans" />
    /// <seealso cref="gip.core.datamodel.IACEntityProperty" />
    /// <seealso cref="gip.core.datamodel.IACType" />
    /// <seealso cref="gip.core.datamodel.IACConfigStore" />
    /// <seealso cref="gip.core.datamodel.IACClassEntity" />
    /// <seealso cref="System.ICloneable" />
    /// <seealso cref="gip.core.datamodel.IInsertInfo" />
    /// <seealso cref="gip.core.datamodel.IUpdateInfo" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Property'}de{'Eigenschaft'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, Const.ACIdentifierPrefix, "en{'Property Name/ID'}de{'Eigenschaftsname/ID'}", "", "", true)]
    [ACPropertyEntity(3, "ACIdentifierKey", "en{'Key'}de{'Schlüssel'}", "", "", true)]
    [ACPropertyEntity(4, ACClass.ClassName, "en{'Class'}de{'Klasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(5, Const.ACGroup, "en{'Group'}de{'Gruppe'}", "", "", true)]
    [ACPropertyEntity(6, Const.ACKindIndex, "en{'Propertytype'}de{'Eigenschaftsart'}", typeof(Global.ACKinds), Const.ContextDatabaseIPlus + "\\ACKindPSList", "", true, DefaultValue = (short)Global.ACKinds.PSPropertyExt)]
    [ACPropertyEntity(7, "ValueTypeACClass", "en{'Data Type'}de{'Datentyp'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(8, "SortIndex", "en{'Sortindex'}de{'Sortierung'}", "", "", true)]
    [ACPropertyEntity(9, "IsInteraction", "en{'Interaction Property'}de{'Interaktionseigenschaft'}", "", "", true)]
    [ACPropertyEntity(10, "IsRightmanagement", "en{'Rights Management'}de{'Rechteverwaltung'}", "", "", true)]
    [ACPropertyEntity(11, "IsBroadcast", "en{'Broadcast in Netw.'}de{'Im Netzw. verbreiten'}", "", "", true)]
    [ACPropertyEntity(12, "ForceBroadcast", "en{'Force Broadcast'}de{'Erzwinge Verbreitung'}", "", "", true)]
    [ACPropertyEntity(13, "IsProxyProperty", "en{'Data-proxy/-target'}de{'Daten-Stellvertreter/-Ziel'}", "", "", true)]
    [ACPropertyEntity(14, "IsInput", "en{'Input'}de{'Input'}", "", "", true, DefaultValue = true)]
    [ACPropertyEntity(15, "IsOutput", "en{'Output'}de{'Output'}", "", "", true, DefaultValue = true)]
    [ACPropertyEntity(16, "ACPropUsageIndex", "en{'Usage'}de{'Verwendung'}", typeof(Global.ACPropUsages), Const.ContextDatabaseIPlus + "\\ACPropUsageList", "", true, DefaultValue = (short)Global.ACPropUsages.Property)]
    [ACPropertyEntity(17, "ACPointCapacity", "en{'Capacity'}de{'Kapazität'}", "", "", true)]
    [ACPropertyEntity(18, "IsPersistable", "en{'Persistable'}de{'Speicherbar'}", "", "", true)]
    [ACPropertyEntity(19, "DataTypeLength", "en{'Datatypelength'}de{'Länge Datentyp'}", "", "", true)]
    [ACPropertyEntity(20, "GenericType", "en{'Generic Type'}de{'Generischer Typ'}", "", "", true)]
    [ACPropertyEntity(21, "IsNullable", "en{'Nullable'}de{'Nullable'}", "", "", true)]
    [ACPropertyEntity(22, "InputMask", "en{'Inputmask'}de{'Eingabemaske'}", "", "", true)]
    [ACPropertyEntity(23, "MinLength", "en{'Minlength'}de{'Min.Länge'}", "", "", true)]
    [ACPropertyEntity(24, "MaxLength", "en{'Maxlength'}de{'Max.Länge'}", "", "", true)]
    [ACPropertyEntity(25, "MinValue", "en{'Minvalue'}de{'Min.Wert'}", "", "", true)]
    [ACPropertyEntity(26, "MaxValue", "en{'Maxvalue'}de{'Max.Wert'}", "", "", true)]
    [ACPropertyEntity(27, "LogRefreshRateIndex", "en{'Refreshrate'}de{'Aktualisierungsrate'}", typeof(Global.MaxRefreshRates), "", "", true, DefaultValue = (short)Global.MaxRefreshRates.Off)]
    [ACPropertyEntity(28, "IsSerializable", "en{'Serializable'}de{'Serialisierbar'}", "", "", true)]
    [ACPropertyEntity(29, "IsEnumerable", "en{'Enumerable'}de{'Enumerierbar'}", "", "", true)]
    [ACPropertyEntity(30, "Comment", "en{'Comment'}de{'Bemerkung'}", "", "", true)]
    [ACPropertyEntity(31, "ACClassProperty1_BasedOnACClassProperty", "en{'Based On Property'}de{'Basiert auf Eigenschaft'}", Const.ContextDatabaseIPlus + "\\" + ACClassProperty.ClassName, "", true)]
    [ACPropertyEntity(32, "ACSource", "en{'Source'}de{'Quelle'}", "", "", true)]
    [ACPropertyEntity(33, "CallbackMethodName", "en{'Callback Method'}de{'Rückrufmethode'}", "", "", true)]
    [ACPropertyEntity(34, "LogFilter", "en{'Log Filter'}de{'Log Filter'}", "", "", true)]
    [ACPropertyEntity(35, "IsRPCEnabled", "en{'Is RPC Enabled'}de{'Ist RPC aktiviert'}", "", "", true)]
    [ACPropertyEntity(36, "RemotePropID", "en{'Remote property ID'}de{'Remote property ID'}", "", "", true)]
    [ACPropertyEntity(37, "IsStatic", "en{'Static'}de{'Statisch'}", "", "", true)]
    [ACPropertyEntity(9999, "XMLValue", "en{'Value'}de{'Wert'}")]
    [ACPropertyEntity(9999, "ACCaptionTranslation", "en{'Translation'}de{'Übersetzung'}", "", "", true)]
    [ACPropertyEntity(9999, "ConfigACClass", "en{'Configuration'}de{'Konfiguration'}", Const.ContextDatabaseIPlus + "\\ConfigACClassList", "", true)]
    [ACPropertyEntity(9999, "IsContent", "en{'Content'}de{'Inhalt'}", "", "", true)]
    [ACPropertyEntity(9999, "MDTextGroup", "en{'Textroup'}de{'Textgruppe'}", Const.ContextDatabaseIPlus + "\\MDTextGroup", "", true)]
    [ACPropertyEntity(9999, "ParentACClassProperty", "en{'Parent Property'}de{'Elterneigenschaft'}", Const.ContextDatabaseIPlus + "\\" + ACClassProperty.ClassName, "", true)]
    [ACPropertyEntity(9999, "DeleteActionIndex", "en{'Delete Action'}de{'Löschaktion'}", "", "", true, DefaultValue = (short)Global.DeleteAction.None)]
    [ACPropertyEntity(9999, "ChangeLogMax", "en{'Max change logs'}de{'Max Änderungsprotokolle'}", "", "Null - change log off, 0 - unlimited change logs, n - log last n changes", true)]
    [ACPropertyEntity(9999, "LogBufferSize", "en{'Log buffer size'}de{'Größe des Log-Puffers'}", "", "", true)]
    [ACDeleteAction("ACClassPropertyRelation_TargetACClassProperty", Global.DeleteAction.CascadeManual)]
    [ACDeleteAction("ACClassPropertyRelation_SourceACClassProperty", Global.DeleteAction.CascadeManual)]
    [ACDeleteAction("ACClassPropertyRelation_SourceACClass", Global.DeleteAction.CascadeManual)]
    [ACDeleteAction("ACClassPropertyRelation_TargetACClass", Global.DeleteAction.CascadeManual)]
    [ACDeleteAction("ACClassProperty_BasedOnACClassProperty", Global.DeleteAction.CascadeManual)]
    [ACDeleteAction("ACClassTaskValue_ACClassProperty", Global.DeleteAction.CascadeManual)]
    [ACDeleteAction("VBGroupRight_ACClassProperty", Global.DeleteAction.CascadeManual)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassProperty.ClassName, "en{'Property'}de{'Eigenschaft'}", typeof(ACClassProperty), ACClassProperty.ClassName, Const.ACIdentifierPrefix, "SortIndex," + Const.ACIdentifierPrefix, new object[]
        {
            new object[] {Const.QueryPrefix + ACClassPropertyRelation.ClassName, "en{'Propertyrelation'}de{'Eigenschaftsbeziehung'}", typeof(ACClassPropertyRelation), ACClassPropertyRelation.ClassName + "_TargetACClassProperty", "", ACClassPropertyRelation.ClassName + "ID"}
        }
    )]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassProperty>) })]
    public partial class ACClassProperty : IACObjectEntityWithCheckTrans, IACEntityProperty, IACType, IACConfigStore, IACClassEntity, ICloneable
    {
        public const string ClassName = "ACClassProperty";
        private static readonly ACMonitorObject _10020_LockValue = new ACMonitorObject(10100);

        #region New/Delete

        /// <summary>
        /// Creates a new ACClassProperty-Instance
        /// UNSAFE, use (QueryLock_1X000)
        /// </summary>
        /// <param name="database"></param>
        /// <param name="parentACObject"></param>
        /// <returns></returns>
        public static ACClassProperty NewACObject(Database database, IACObject parentACObject)
        {
            ACClassProperty entity = new ACClassProperty();
            entity.Database = database;
            entity.ACClassPropertyID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            if (parentACObject is ACClass)
            {
                entity.ACClass = parentACObject as ACClass;
                // TODO: Zwischenlösung, Funktioniert nur wenn es nur eine Configuration gibt
                var configClass = entity.ACClass.Properties.Where(c => c.ACPropUsage == Global.ACPropUsages.ConfigPointProperty && c.ConfigACClass != null).FirstOrDefault();
                if (configClass != null)
                    entity.ConfigACClass = configClass.ConfigACClass;
            }

            if (entity.ACClass != null)
                entity.SortIndex = Convert.ToInt16(entity.ACClass.Properties.Where(c => c.SortIndex < 9999).Max(c => c.SortIndex) + 1);
            entity.ACClassProperty1_BasedOnACClassProperty = entity;
            entity.IsRPCEnabled = false;
            entity.RemotePropID = 0;
            entity.LogBufferSize = 0;
            entity.IsStatic = false;
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

            // Alle Relationen vom Source löschen
            foreach (var acClassPropertyRelationS in this.ACClassPropertyRelation_SourceACClassProperty.ToList())
            {
                acClassPropertyRelationS.DeleteACObject(database, withCheck);
            }

            // Alle Relationen vom Target löschen
            foreach (var acClassPropertyRelationT in this.ACClassPropertyRelation_TargetACClassProperty.ToList())
            {
                acClassPropertyRelationT.DeleteACObject(database, withCheck);
            }

            // Alle Properties in den abgeleiteten Klassen löschen
            var query1 = (database as Database).ACClassProperty.Where(c => c.ACClassProperty1_BasedOnACClassProperty.ACClassPropertyID == this.ACClassPropertyID && c.ACClassPropertyID != this.ACClassPropertyID).ToList();
            foreach (var overrideACClassProperty in query1)
            {
                overrideACClassProperty.DeleteACObject(database, withCheck);
            }

            var query4 = (database as Database).VBGroupRight.Where(c => c.ACClassProperty.ACClassPropertyID == this.ACClassPropertyID).ToList();
            foreach (var vbGroupRight in query4)
            {
                vbGroupRight.DeleteACObject(database, withCheck);
            }

            database.Remove(this);
            return null;
        }


        /// <summary>
        /// Method for overriding a Property
        /// UNSAFE, use (QueryLock_1X000)
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acClass">The ac class.</param>
        /// <param name="baseACClassProperty">The base AC class property.</param>
        /// <returns>ACClassProperty.</returns>
        public static ACClassProperty NewACClassProperty(Database database, ACClass acClass, ACClassProperty baseACClassProperty)
        {
            if (acClass == null || baseACClassProperty == null)
                return null;
            // Falls Property schon überschrieben in dieser Ableitungsstufe
            if (acClass == baseACClassProperty.ACClass)
                return null;
            // Falls Property gar nicht Basis-Member dieser Klasse ist
            if (!acClass.Properties.Contains(baseACClassProperty))
                return null;
            ACClassProperty entity = NewACObject(database, acClass);
            entity.CopyBaseValues(baseACClassProperty);
            return entity;
        }


        public Database Database { get; private set; } = null;

        void IACClassEntity.OnObjectMaterialized(Database db)
        {
            if (Database == null)
                Database = db;
        }


        #region Cloning

        /// <summary>
        /// Copies all Properties from a passed ACClassProperty-Instance
        /// </summary>
        /// <param name="baseACClassProperty"></param>
        public void CopyBaseValues(ACClassProperty baseACClassProperty)
        {
            ACKindIndex = baseACClassProperty.ACKindIndex;
            ACGroup = baseACClassProperty.ACGroup;
            SortIndex = baseACClassProperty.SortIndex;
            IsRightmanagement = baseACClassProperty.IsRightmanagement;
            ACIdentifier = baseACClassProperty.ACIdentifier;
            ACCaptionTranslation = baseACClassProperty.ACCaptionTranslation;
            ACSource = baseACClassProperty.ACSource;
            Comment = baseACClassProperty.Comment;
            IsInteraction = baseACClassProperty.IsInteraction;
            ValueTypeACClass = baseACClassProperty.ValueTypeACClass;
            ConfigACClass = baseACClassProperty.ConfigACClass;
            ACPropUsageIndex = baseACClassProperty.ACPropUsageIndex;
            IsBroadcast = baseACClassProperty.IsBroadcast;
            IsProxyProperty = baseACClassProperty.IsProxyProperty;
            IsInput = baseACClassProperty.IsInput;
            IsOutput = baseACClassProperty.IsOutput;
            IsContent = baseACClassProperty.IsContent;
            ACPointCapacity = baseACClassProperty.ACPointCapacity;
            IsPersistable = baseACClassProperty.IsPersistable;
            IsSerializable = baseACClassProperty.IsSerializable;
            IsEnumerable = baseACClassProperty.IsEnumerable;
            RemotePropID = baseACClassProperty.RemotePropID;
            IsRPCEnabled = baseACClassProperty.IsRPCEnabled;

            ACClassProperty1_ParentACClassProperty = baseACClassProperty.ACClassProperty1_ParentACClassProperty;
            DataTypeLength = baseACClassProperty.DataTypeLength;
            GenericType = baseACClassProperty.GenericType;
            IsNullable = baseACClassProperty.IsNullable;
            InputMask = baseACClassProperty.InputMask;
            MinLength = baseACClassProperty.MinLength;
            MaxLength = baseACClassProperty.MaxLength;
            MinValue = baseACClassProperty.MinValue;
            MaxValue = baseACClassProperty.MaxValue;
            XMLValue = baseACClassProperty.XMLValue;
            LogRefreshRateIndex = baseACClassProperty.LogRefreshRateIndex;
            LogFilter = baseACClassProperty.LogFilter;
            Precision = baseACClassProperty.Precision;
            XMLConfig = baseACClassProperty.XMLConfig;

            ACClassProperty1_BasedOnACClassProperty = baseACClassProperty.ACClassProperty1_BasedOnACClassProperty;
        }


        public object Clone()
        {
            ACClassProperty clonedObject = ACClassProperty.NewACObject(this.Database, this.ACClass);

            clonedObject.ACClassID = this.ACClassID;
            clonedObject.ACIdentifier = this.ACIdentifier;
            clonedObject.ACIdentifierKey = this.ACIdentifierKey;
            clonedObject.ACCaptionTranslation = this.ACCaptionTranslation;
            clonedObject.ACGroup = this.ACGroup;
            clonedObject.BasedOnACClassPropertyID = this.BasedOnACClassPropertyID;
            clonedObject.ACKindIndex = this.ACKindIndex;
            clonedObject.SortIndex = this.SortIndex;
            clonedObject.IsRightmanagement = this.IsRightmanagement;
            clonedObject.ACSource = this.ACSource;
            clonedObject.Comment = this.Comment;
            clonedObject.IsInteraction = this.IsInteraction;
            clonedObject.ValueTypeACClassID = this.ValueTypeACClassID;
            clonedObject.GenericType = this.GenericType;
            clonedObject.ConfigACClassID = this.ConfigACClassID;
            clonedObject.ACPropUsageIndex = this.ACPropUsageIndex;
            clonedObject.DeleteActionIndex = this.DeleteActionIndex;
            clonedObject.IsBroadcast = this.IsBroadcast;
            clonedObject.ForceBroadcast = this.ForceBroadcast;
            clonedObject.IsProxyProperty = this.IsProxyProperty;
            clonedObject.IsInput = this.IsInput;
            clonedObject.IsOutput = this.IsOutput;
            clonedObject.IsContent = this.IsContent;
            clonedObject.IsPersistable = this.IsPersistable;
            clonedObject.IsSerializable = this.IsSerializable;
            clonedObject.IsEnumerable = this.IsEnumerable;
            clonedObject.ACPointCapacity = this.ACPointCapacity;
            clonedObject.CallbackMethodName = this.CallbackMethodName;
            clonedObject.ParentACClassPropertyID = this.ParentACClassPropertyID;
            clonedObject.DataTypeLength = this.DataTypeLength;
            clonedObject.IsNullable = this.IsNullable;
            clonedObject.InputMask = this.InputMask;
            clonedObject.MinLength = this.MinLength;
            clonedObject.MaxLength = this.MaxLength;
            clonedObject.MinValue = this.MinValue;
            clonedObject.MaxValue = this.MaxValue;
            clonedObject.XMLValue = this.XMLValue;
            clonedObject.LogRefreshRateIndex = this.LogRefreshRateIndex;
            clonedObject.LogFilter = this.LogFilter;
            clonedObject.Precision = this.Precision;
            clonedObject.XMLACEventArgs = this.XMLACEventArgs;
            clonedObject.XMLConfig = this.XMLConfig;
            clonedObject.BranchNo = this.BranchNo;
            clonedObject.IsRPCEnabled = this.IsRPCEnabled;
            clonedObject.RemotePropID = this.RemotePropID;

            return clonedObject;
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
            if (ValueTypeACClass == null)
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "ValueTypeACClass",
                    Message = "ValueTypeACClass is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "ValueTypeACClass"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            if (ACClassProperty1_BasedOnACClassProperty == null)
                ACClassProperty1_BasedOnACClassProperty = this;
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
            base.EntityCheckModified(user, context);
            return null;
        }
        #endregion


        #region Idenification and ACURL

        /// <summary>
        /// Primary Key of a Entity in the Database/Table
        /// (Uniqued Identifier of a type in the iPlus-Framework)
        /// </summary>
        [NotMapped]
        public Guid ACTypeID
        {
            get { return ACClassPropertyID; }
        }


        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return Const.ACIdentifierPrefix;
            }
        }


        /// <summary>
        /// Returns the ACUrl of the Property in the Application-Tree.
        /// This ACClass must be a type which ist defined in the Application-Tree (physical model) 
        /// Otherwise a empty string will be returned
        /// This call should only be done on TopBaseACClassProperty
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="rootACObject">If you pass a ACClass of a parent object, the the ARUrl will be shorten by a realtive address</param>
        /// <returns></returns>
        [ACMethodInfo("", "", 9999)]
        public override string GetACUrlComponent(IACObject rootACObject = null)
        {
            if (rootACObject != ParentACObject)
            {
                ACClass rootACClass = rootACObject as ACClass;
                if (rootACClass == null)
                    return this.Safe_ACClass.GetACUrlComponent(rootACObject) + "\\" + ACIdentifier;
                else
                {
                    if (rootACClass.IsDerivedClassFrom(this.Safe_ACClass))
                        return ACIdentifier;
                    else
                        return this.Safe_ACClass.GetACUrlComponent(rootACObject) + "\\" + ACIdentifier;
                }
            }
            else
            {
                return ACIdentifier;
            }
        }


        /// <summary>
        /// Returns the Database-ACUrl
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public string ACUrl
        {
            get
            {
                return GetACUrl();
            }
        }


        /// <summary>
        /// Returns ACClass, where this property belongs to
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999)]
        [NotMapped]
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
        #endregion


        #region Signature
        /// <summary>
        /// Returns the Signature of the property
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
        /// Resturns the dynamic signature of this Property
        /// THREAD-SAFE  (QueryLock_1X000)
        /// </summary>
        /// <returns></returns>
        public ACMethod TypeACSignature()
        {
            ACMethod acMethod = new ACMethod(this.ACIdentifier);
            if (IsInput)
            {
                ACValue acValueValue = new ACValue();
                acValueValue.ACIdentifier = "value";
                acValueValue.Option = Global.ParamOption.Required;
                acValueValue.ValueTypeACClass = this.ValueTypeACClass;

                acMethod.ParameterValueList.Add(acValueValue);
            }
            if (IsOutput)
            {
                ACValue acValueResult = new ACValue();
                acValueResult.ACIdentifier = "result";
                acValueResult.Option = Global.ParamOption.Required;
                acValueResult.ValueTypeACClass = this.ValueTypeACClass;
                acMethod.ResultValueList.Add(acValueResult);
            }
            return acMethod;
        }
        #endregion


        #region Type-Informations

        /// <summary>
        /// Returns the category of this property
        /// </summary>
        [NotMapped]
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


        /// <summary>
        /// Returns a enum which describes how this property is used
        /// </summary>
        [NotMapped]
        public Global.ACPropUsages ACPropUsage
        {
            get
            {
                return (Global.ACPropUsages)ACPropUsageIndex;
            }
            set
            {
                ACPropUsageIndex = (short)value;
            }
        }


        /// <summary>
        /// Gets or sets the log refresh rate.
        /// </summary>
        /// <value>The log refresh rate.</value>
        [NotMapped]
        public Global.MaxRefreshRates LogRefreshRate
        {
            get
            {
                return (Global.MaxRefreshRates)LogRefreshRateIndex;
            }
            set
            {
                LogRefreshRateIndex = (short)value;
            }
        }


        private ulong _LastPointSequenceNo = ulong.MinValue;
        internal ulong NextPointSeqNo
        {
            get
            {
                using (ACMonitor.Lock(_10020_LockValue))
                {
                    if (_LastPointSequenceNo == ulong.MaxValue)
                        _LastPointSequenceNo = ulong.MinValue;
                    else
                        _LastPointSequenceNo++;
                    return _LastPointSequenceNo;
                }
            }
        }


        /// <summary>
        /// Returns the most top ACClassProperty if the property is overridden in subclasses
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public ACClassProperty TopBaseACClassProperty
        {
            get
            {
                //using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                //{
                //    return TopBaseACClassPropertyLocked;
                //}
                if (this.ACClassPropertyID == this.BasedOnACClassPropertyID)
                    return this;

                ACClassProperty basedProperty = null;
                if (this.ACClassProperty1_BasedOnACClassPropertyReference.IsLoaded)
                    basedProperty = (ACClassProperty) this.ACClassProperty1_BasedOnACClassPropertyReference.CurrentValue;
                else
                {
                    using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                    {
                        basedProperty = this.ACClassProperty1_BasedOnACClassProperty;
                    }
                }
                if (basedProperty != null)
                    return basedProperty.TopBaseACClassProperty;
                return this;

            }
        }


        //private ACClassProperty TopBaseACClassPropertyLocked
        //{
        //    get
        //    {
        //        if (this.ACClassPropertyID == this.BasedOnACClassPropertyID)
        //            return this;

        //        ACClassProperty basedProperty = null;
        //        if (this.ACClassProperty1_BasedOnACClassPropertyReference.IsLoaded)
        //            basedProperty = this.ACClassProperty1_BasedOnACClassPropertyReference.Value;
        //        else
        //        {
        //            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
        //            {
        //                basedProperty = this.ACClassProperty1_BasedOnACClassProperty;
        //            }
        //        }
        //        if (basedProperty != null)
        //            return basedProperty.TopBaseACClassPropertyLocked;
        //        return this;
        //    }
        //}


        /// <summary>
        /// Returns all overrides (Properties with same ACIndentifier in subclasses)
        /// THREAD-SAFE  (QueryLock_1X000)
        /// </summary>
        /// <value>The overridden properties.</value>
        [NotMapped]
        public IEnumerable<ACClassProperty> OverriddenProperties
        {
            get
            {
                List<ACClassProperty> overriddenProperties = new List<ACClassProperty>();
                overriddenProperties.Add(this);

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    ACClass acClass = this.ACClass.ACClass1_BasedOnACClass;
                    while (acClass != null)
                    {
                        var query = acClass.ACClassProperty_ACClass.Where(c => c.ACIdentifier == this.ACIdentifier);
                        if (query.Any())
                            overriddenProperties.Add(query.First());
                        acClass = acClass.ACClass1_BasedOnACClass;
                    }
                }
                return overriddenProperties;
            }
        }


        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(2, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
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
        [NotMapped]
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

        public bool GetBindingForStaticProperty(ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            if (!this.IsStatic)
                return false;
            Type typeOfClass = this.Safe_ACClass.ObjectType;
            if (typeOfClass == null)
                return false;
            PropertyInfo pi = null;
            while (typeOfClass != null)
            {
                pi = typeOfClass.GetProperty(this.ACIdentifier, BindingFlags.Static | BindingFlags.Public);
                if (pi != null)
                    break;
                typeOfClass = typeOfClass.BaseType;
            }
            if (pi == null)
                return false;
            source = pi.GetValue(null, null);
            path = "";
            rightControlMode = this.Safe_ACClass.GetRight(this);
            //this.IsRightmanagement
            return true;
        }


        /// <summary>
        /// Returns the AssemblyQualifiedName
        /// </summary>
        [NotMapped]
        public string AssemblyQualifiedName
        {
            get
            {
                var valueTypeACClass = Safe_ValueTypeACClass;
                if (valueTypeACClass == null)
                    return "";
                return valueTypeACClass.InheritedASQN;
            }
        }


        Type _ObjectType = null;
        /// <summary>
        /// Returns the .NET-Type (If type of property value is a generic it returns the inner type)
        /// </summary>
        [NotMapped]
        public Type ObjectType
        {
            get
            {
                if (_ObjectType == null)
                    _ObjectType = Type.GetType(AssemblyQualifiedName);
                return _ObjectType;
            }
        }


        /// <summary>
        /// Returns the .NET-Type (If type of property value is a generic it returns the outer type)
        /// </summary>
        [NotMapped]
        public Type ObjectGenericType
        {
            get
            {

                if (String.IsNullOrEmpty(GenericType))
                    return null;
                Type typeTGeneric = Type.GetType(GenericType);
                if (typeTGeneric != null)
                    return typeTGeneric;
                return TypeAnalyser.GetTypeInAssembly(GenericType);
            }
        }


        Type _ObjectFullType = null;
        /// <summary>
        /// Returns the .NET-Type (If type of property value is a generic it returns the outer+inner type)
        /// </summary>
        [NotMapped]
        public Type ObjectFullType
        {
            get
            {
                if (_ObjectFullType != null)
                    return _ObjectFullType;
                if (String.IsNullOrEmpty(GenericType))
                {
                    if (IsNullable && ObjectType != null && ObjectType.IsValueType)
                    {
                        _ObjectFullType = typeof(Nullable<>).MakeGenericType(new Type[] { ObjectType });
                        return _ObjectFullType;
                    }
                    else
                        return ObjectType;
                }
                _ObjectFullType = GetGenericACPropertyType(ObjectGenericType, ObjectType, null);
                return _ObjectFullType;
            }
        }



        /// <summary>
        /// Returns the .NET-Type  of the parent class in a composition tree
        /// </summary>
        [NotMapped]
        public Type ObjectTypeParent
        {
            get
            {
                return Safe_ACClass.ObjectType;
            }
        }
        #endregion


        #region Selected/List and Columns Methods

        /// <summary>
        /// Returns the path to a Property which should be used as a ItemsSource for WPF-ItemsControls
        /// The method searches for a Property which is declared with a ACPropertyListAttribute and has the same ACGroup-Name like this
        /// THREAD-SAFE  (QueryLock_1X000)
        /// </summary>
        /// <param name="acSource"></param>
        /// <param name="acAccess"></param>
        /// <param name="sourceProperty"></param>
        /// <param name="includeStatic"></param>
        /// <returns></returns>
        public string GetACSource(string acSource, out string acAccess, out ACClassProperty sourceProperty, bool includeStatic = true)
        {
            var acClassPropertyAccess = Safe_ACClass.GetProperty(ACGroup, Global.ACPropUsages.AccessPrimary);
            if (acClassPropertyAccess == null)
                acClassPropertyAccess = Safe_ACClass.GetProperty(ACGroup, Global.ACPropUsages.Access);
            if (acClassPropertyAccess != null)
                acAccess = acClassPropertyAccess.ACIdentifier;
            else
                acAccess = "";
            sourceProperty = null;
            if (!string.IsNullOrEmpty(acSource))
                return acSource;

            if (!string.IsNullOrEmpty(ACGroup))
            {
                sourceProperty = Safe_ACClass.GetProperty(ACGroup, Global.ACPropUsages.List, false, includeStatic);
                if (sourceProperty != null)
                {
                    return sourceProperty.ACIdentifier;
                }
            }

            // ACSource ist definiert
            if (!string.IsNullOrEmpty(this.ACSource))
                return ACSource;

            // Enum
            if (ObjectType != null && ObjectType.IsEnum)
            {
                PropertyInfo pi = this.ObjectTypeParent.GetProperty(this.ACIdentifier);
                if (pi == null)
                    return String.Format("\\!{0}(#{1}\\{1}#)", Const.MN_GetEnumList, ObjectType.AssemblyQualifiedName);
                return String.Format("\\!{0}(#{1}\\{2}#)", Const.MN_GetEnumList, ObjectType.AssemblyQualifiedName, pi.PropertyType.AssemblyQualifiedName);
            }

            if (this.ValueTypeACClass != null)
            {
                acAccess = "";
                if (!String.IsNullOrEmpty(acSource))
                    return acSource;
            }

            return this.ACSource;
        }


        /// <summary>
        /// Returns the path to a Property which should be used for binding (VBContent)
        /// The method searches for a Property which is declared with a ACPropertyCurrentAttribute and has the same ACGroup-Name like this
        /// THREAD-SAFE  (QueryLock_1X000)
        /// </summary>
        /// <param name="acCurrent"></param>
        /// <returns></returns>
        public string GetACCurrent(string acCurrent)
        {
            if (!string.IsNullOrEmpty(acCurrent))
                return acCurrent;
            if (!string.IsNullOrEmpty(ACGroup))
            {
                ACClassProperty acClassProperty = Safe_ACClass.GetProperty(ACGroup, Global.ACPropUsages.Current);
                if (acClassProperty != null)
                    return acClassProperty.ACIdentifier;
            }
            return "";
        }


        /// <summary>
        /// Returns the default Columns which can be presented in a Items-Control
        /// THREAD-SAFE  (QueryLock_1X000)
        /// </summary>
        /// <param name="maxColumns"></param>
        /// <param name="acColumns"></param>
        /// <returns></returns>
        public List<ACColumnItem> GetColumns(int maxColumns = 9999, string acColumns = null)
        {
            if (ObjectType == null)
                return null;
            if (ObjectType.IsEnum)
            {
                List<ACColumnItem> acColumnItemList = new List<ACColumnItem>();
                acColumnItemList.Add(new ACColumnItem(Const.ACCaptionPrefix));
                return acColumnItemList;
            }
            else
            {
                var valueTypeACClass = Safe_ValueTypeACClass;
                if (valueTypeACClass == null)
                    return null;
                return valueTypeACClass.GetColumns(maxColumns, acColumns);
            }
        }


        /// <summary>
        /// Returns a member of the Property-Type (ValueTypeACClass)
        /// THREAD-SAFE  (QueryLock_1X000)
        /// </summary>
        /// <param name="acIdentifier"></param>
        /// <param name="forceRefreshFromDB"></param>
        /// <returns></returns>
        public IACType GetMember(string acIdentifier, bool forceRefreshFromDB = false)
        {
            var valueTypeACClass = Safe_ValueTypeACClass;
            if (valueTypeACClass == null)
                return null;
            return valueTypeACClass.GetMember(acIdentifier, forceRefreshFromDB);
        }

        #endregion


        #region Property-Binding

        /// <summary>
        /// Only for internal usage
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        [NotMapped]
        public ACClass MyCurrentAClassOfProperty
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the binding to the Source-Property
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [ACPropertyInfo(9999, "", "", "", true)]
        [NotMapped]
        public ACClassPropertyRelation ACClassPropertyBindingToSource
        {
            get
            {
                return GetMyPropertyBindingToSource(MyCurrentAClassOfProperty);
            }
        }


        /// <summary>
        /// Returns the bound Source-Property
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="myAClassOfProperty">My A class of property.</param>
        /// <returns>ACClassPropertyRelation.</returns>
        public ACClassPropertyRelation GetMyPropertyBindingToSource(ACClass myAClassOfProperty)
        {
            if (myAClassOfProperty == null)
                return null;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return ACClassPropertyRelation_TargetACClassProperty.Where(c => c.TargetACClass.ACClassID == myAClassOfProperty.ACClassID && c.ConnectionTypeIndex == (Int16)Global.ConnectionTypes.Binding).FirstOrDefault();
            }
        }


        /// <summary>
        /// Returns the bound Source-Property
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [NotMapped]
        public ACClassProperty ACClassPropertySource
        {
            get
            {
                if (ACClassPropertyBindingToSource == null)
                    return null;

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return ACClassPropertyBindingToSource.SourceACClassProperty;
                }
            }
        }


        /// <summary>
        /// Returns Source-bindings 
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [NotMapped]
        public IEnumerable<ACClassPropertyRelation> SourceBindingList
        {
            get
            {
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return this.ACClassPropertyRelation_SourceACClassProperty.Where(c => c.ConnectionTypeIndex == (Int16)Global.ConnectionTypes.Binding).Select(c => c);
                }
            }
        }


        /// <summary>
        /// Returns Source-bindings 
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        public IEnumerable<ACClassPropertyRelation> GetMySourceBindingList(ACClass myAClassOfProperty)
        {
            if (myAClassOfProperty == null)
                return null;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return this.ACClassPropertyRelation_SourceACClassProperty.Where(c => c.SourceACClass.ACClassID == myAClassOfProperty.ACClassID && c.ConnectionTypeIndex == (Int16)Global.ConnectionTypes.Binding).Select(c => c);
            }
        }


        /// <summary>
        /// Returns Target-bindings 
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [NotMapped]
        public IEnumerable<ACClassPropertyRelation> TargetBindingList
        {
            get
            {

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return this.ACClassPropertyRelation_TargetACClassProperty.Where(c => c.ConnectionTypeIndex == (Int16)Global.ConnectionTypes.Binding).Select(c => c);
                }
            }
        }


        /// <summary>
        /// Returns Target-bindings 
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        public IEnumerable<ACClassPropertyRelation> GetMyTargetBindingList(ACClass myAClassOfProperty)
        {
            if (myAClassOfProperty == null)
                return null;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return this.ACClassPropertyRelation_TargetACClassProperty.Where(c => c.TargetACClass.ACClassID == myAClassOfProperty.ACClassID && c.ConnectionTypeIndex == (Int16)Global.ConnectionTypes.Binding).Select(c => c);
            }
        }


        /// <summary>
        /// Returns physical source-relations
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [NotMapped]
        public IEnumerable<ACClassPropertyRelation> SourceConnectionPhysicalList
        {
            get
            {
                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return this.ACClassPropertyRelation_SourceACClassProperty.Where(c => c.ConnectionTypeIndex == (Int16)Global.ConnectionTypes.ConnectionPhysical).Select(c => c);
                }
            }
        }


        /// <summary>
        /// Returns physical source-relations
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        public IEnumerable<ACClassPropertyRelation> GetMySourceConnectionPhysicalList(ACClass myAClassOfProperty, bool includeLogicalBridges = true)
        {
            if (myAClassOfProperty == null)
                return null;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return this.ACClassPropertyRelation_SourceACClassProperty
                            .Where(c =>    c.SourceACClass.ACClassID == myAClassOfProperty.ACClassID 
                                        && (  c.ConnectionTypeIndex == (Int16)Global.ConnectionTypes.ConnectionPhysical
                                            || (includeLogicalBridges && c.ConnectionTypeIndex == (Int16)Global.ConnectionTypes.LogicalBridge)))
                            .Select(c => c);
            }
        }


        /// <summary>
        /// Returns physical target-relations
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        [NotMapped]
        public IEnumerable<ACClassPropertyRelation> TargetConnectionPhysicalList
        {
            get
            {

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return this.ACClassPropertyRelation_TargetACClassProperty.Where(c => c.ConnectionTypeIndex == (Int16)Global.ConnectionTypes.ConnectionPhysical).Select(c => c);
                }
            }
        }


        /// <summary>
        /// Returns physical target-relations
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        public IEnumerable<ACClassPropertyRelation> GetMyTargetConnectionPhysicalList(ACClass myAClassOfProperty)
        {
            if (myAClassOfProperty == null)
                return null;

            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                return this.ACClassPropertyRelation_TargetACClassProperty.Where(c => c.TargetACClass.ACClassID == myAClassOfProperty.ACClassID && c.ConnectionTypeIndex == (Int16)Global.ConnectionTypes.ConnectionPhysical).Select(c => c);
            }
        }
        #endregion


        #region ACClassDesign

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
            ACClass acClass = (acObject as ACClassProperty).Safe_ACClass;

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

            switch (this.ACPropUsage)
            {
                case Global.ACPropUsages.Current:
                    {

                        using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                        {
                            var query2 = this.Database.ACClass.Where(c => c.ACIdentifier == "ObjectCurrent" && c.ACKindIndex == (Int16)Global.ACKinds.TACLRBaseTypes);
                            if (query2.Any())
                            {
                                var acSimpleList = query2.First();
                                var acClassDesign = acSimpleList.GetDesign(acSimpleList, acUsage, acKind, vbDesignName);
                                if (acClassDesign != null)
                                    return acClassDesign;
                            }
                        }
                    }
                    break;
                case Global.ACPropUsages.Selected:
                    {

                        using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                        {
                            var query2 = this.Database.ACClass.Where(c => c.ACIdentifier == "ObjectSelected" && c.ACKindIndex == (Int16)Global.ACKinds.TACLRBaseTypes);
                            if (query2.Any())
                            {
                                var acSimpleList = query2.First();
                                var acClassDesign = acSimpleList.GetDesign(acSimpleList, acUsage, acKind, vbDesignName);
                                if (acClassDesign != null)
                                    return acClassDesign;
                            }
                        }
                    }
                    break;
                default:
                    if (!string.IsNullOrEmpty(ACSource))
                    {

                        using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                        {
                            var query2 = this.Database.ACClass.Where(c => c.ACIdentifier == "ObjectItem" && c.ACKindIndex == (Int16)Global.ACKinds.TACLRBaseTypes);
                            if (query2.Any())
                            {
                                var acSimpleItem = query2.First();
                                var acClassDesign = acSimpleItem.GetDesign(acSimpleItem, acUsage, acKind, vbDesignName);
                                if (acClassDesign != null)
                                    return acClassDesign;
                            }
                        }
                    }
                    break;
            }

            var valueTypeACClass = Safe_ValueTypeACClass;
            if (valueTypeACClass == null)
                return null;
            return valueTypeACClass.GetDesign(ValueTypeACClass, acUsage, acKind, vbDesignName);
        }


        /// <summary>
        /// Returns all Designs over complete class hierarchy including overridden Designs
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <value>ACClassDesign List</value>
        [ACPropertyInfo(9999, "", "", "", true)]
        [NotMapped]
        public IEnumerable<ACClassDesign> Designs
        {
            get
            {
                List<ACClassDesign> acClassDesignList = new List<ACClassDesign>();

                var valueTypeACClass = Safe_ValueTypeACClass;
                if (valueTypeACClass != null)
                {
                    var designs = valueTypeACClass.Designs;
                    if (designs != null && designs.Any())
                    {
                        foreach (var acClassDesign in designs)
                        {
                            acClassDesignList.Add(acClassDesign);
                        }
                    }
                }

                switch (this.ACPropUsage)
                {
                    case Global.ACPropUsages.Current:
                        {

                            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                            {
                                var query2 = this.Database.ACClass.Where(c => c.ACIdentifier == "ObjectCurrent" && c.ACKindIndex == (Int16)Global.ACKinds.TACLRBaseTypes);
                                if (query2.Any())
                                {
                                    var acSimpleList = query2.First();
                                    if (acSimpleList.Designs != null && acSimpleList.Designs.Any())
                                    {
                                        foreach (var acClassDesign in acSimpleList.Designs)
                                        {
                                            acClassDesignList.Add(acClassDesign);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Global.ACPropUsages.Selected:
                        {

                            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                            {
                                var query2 = this.Database.ACClass.Where(c => c.ACIdentifier == "ObjectSelected" && c.ACKindIndex == (Int16)Global.ACKinds.TACLRBaseTypes);
                                if (query2.Any())
                                {
                                    var acSimpleList = query2.First();
                                    if (acSimpleList.Designs != null && acSimpleList.Designs.Any())
                                    {
                                        foreach (var acClassDesign in acSimpleList.Designs)
                                        {
                                            acClassDesignList.Add(acClassDesign);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        if (!string.IsNullOrEmpty(ACSource))
                        {

                            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                            {
                                var query2 = this.Database.ACClass.Where(c => c.ACIdentifier == "ObjectItem" && c.ACKindIndex == (Int16)Global.ACKinds.TACLRBaseTypes);
                                if (query2.Any())
                                {
                                    var acSimpleItem = query2.First();
                                    if (acSimpleItem.Designs != null && acSimpleItem.Designs.Any())
                                    {
                                        foreach (var acClassDesign in acSimpleItem.Designs)
                                        {
                                            acClassDesignList.Add(acClassDesign);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }

                return acClassDesignList;
            }
        }

        #endregion


        #region Configuration

        [NotMapped]
        public string ConfigStoreName
        {
            get
            {
                ACClassInfo acClassInfo = (ACClassInfo)Safe_ACClass.GetType().GetCustomAttributes(typeof(ACClassInfo), false)[0];
                string caption = Translator.GetTranslation(acClassInfo.ACCaptionTranslation);
                return caption;
            }
        }


        /// <summary>
        /// ACConfigKeyACUrl returns the relative Url to the "main table" in group a group of semantically related tables.
        /// This property is used when NewACConfig() is called. NewACConfig() creates a new IACConfig-Instance and set the IACConfig.KeyACUrl-Property with this ACConfigKeyACUrl.
        /// </summary>
        [NotMapped]
        public string ACConfigKeyACUrl
        {
            get
            {
                return ".\\ACClassProperty(" + this.ACIdentifier + ")";
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
            ACClass acClass = acObject as ACClass;
            if (acClass == null)
                acClass = this.Safe_ACClass;

            ACClassConfig acConfig = null;
            using (ACMonitor.Lock(this.Database.QueryLock_1X000))
            {
                acConfig = ACClassConfig.NewACObject(this.Database, acClass);
                acConfig.KeyACUrl = ACConfigKeyACUrl;
                acConfig.LocalConfigACUrl = localConfigACUrl;
                ACClass dataType = valueTypeACClass;
                if ((dataType == null) && (this.ConfigACClass != null))
                    dataType = this.ConfigACClass;
                if ((dataType == null) && (this.ValueTypeACClass != null))
                {
                    // TODO: throw Exception
                    dataType = this.ValueTypeACClass;
                }
                acConfig.ValueTypeACClass = dataType;
                acClass.ACClassConfig_ACClass.Add(acConfig);
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
                if (acConfig.ACClass == this.Safe_ACClass)
                    this.Safe_ACClass.ACClassConfig_ACClass.Remove(acConfig);
                else
                    acConfig.ACClass.ACClassConfig_ACClass.Remove(acConfig);
                if (acConfig.EntityState != EntityState.Detached)
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


        /// <summary>
        /// Returns all configuration-entries for this ACClassProperty
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="acObject"></param>
        /// <returns></returns>
        public IEnumerable<IACConfig> GetConfigListOfType(IACObjectEntity acObject = null)
        {
            ACClass acClass = acObject as ACClass;
            if (acClass == null)
                acClass = this.Safe_ACClass;
            if (acClass != this.Safe_ACClass)
            {
                using (ACMonitor.Lock(acClass.Database.QueryLock_1X000))
                {
                    if (acClass.ACClassConfig_ACClassReference.IsLoaded)
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
            using (ACMonitor.Lock(this.ACClass.Database.QueryLock_1X000))
            {
                if (this.ACClass.ACClassConfig_ACClassReference.IsLoaded)
                {
                    this.ACClass.ACClassConfig_ACClass.AutoRefresh(this.ACClass.Database);
                    this.ACClass.ACClassConfig_ACClass.AutoLoad(this.ACClass.Database);
                }
                newSafeList = new SafeList<IACConfig>(this.ACClass.ACClassConfig_ACClass.ToList().Select(x => (IACConfig)x).Where(c => c.KeyACUrl == ACConfigKeyACUrl));
            }
            using (ACMonitor.Lock(_10020_LockValue))
            {
                _ACConfigListCache = newSafeList;
                return _ACConfigListCache;
            }
        }

        [NotMapped]
        public decimal OverridingOrder { get; set; }


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

        /// <summary>
        /// A thread-safe and cached list of Configuration-Values of type IACConfig.
        /// </summary>
        [NotMapped]
        public IEnumerable<IACConfig> ConfigurationEntries
        {
            get
            {
                return ACConfigListCache;
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
                var query = database.ACClassConfig.Where(c => c.ACClassID == this.ACClassID && c.KeyACUrl == ACConfigKeyACUrl);
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


#region Misc Properties and Methods

#region XMLValue and ConfigValue

        /// <summary>
        /// Persistable Value auf this Property
        /// </summary>
        [ACPropertyInfo(34, "", "en{'Value'}de{'Wert'}")]
        [NotMapped]
        public object Value
        {
            get
            {
                return XMLValue;
            }
            set
            {
                XMLValue = (String)value;
            }
        }


        /// <summary>
        /// If the Class ACClassProperty is extended with a virtual Property itsself ->
        /// Thas necessary to be able to define additional project specific fields in the iPlus-Type-System, 
        /// than this Property returns the additional value.
        /// For example this ist for OPCItemConfig and similar types.
        /// </summary>
        [ACPropertyInfo(9999, "", "en{'Extended Property-Configuration'}de{'Erweiterte Eigenschafts-Konfiguration'}")]
        [NotMapped]
        public object ConfigValue
        {
            get
            {
                var configClass = Safe_ConfigACClass;
                if (configClass == null)
                    return null;
                return this[configClass.ACIdentifier];
            }
            set
            {
                var configClass = Safe_ConfigACClass;
                if (configClass == null)
                    return;
                this[configClass.ACIdentifier] = value;
            }
        }


        bool bRefreshConfig = false;
        protected override void OnPropertyChanging<T>(T newValue, string propertyName, bool afterChange)
        {
            if (propertyName == nameof(XMLConfig))
            {
                string xmlConfig = newValue as string;
                if (afterChange)
                {
                    if (bRefreshConfig)
                        ACProperties.Refresh();
                }
                else
                {
                    bRefreshConfig = false;
                    if (!(String.IsNullOrEmpty(xmlConfig) && String.IsNullOrEmpty(XMLConfig)) && xmlConfig != XMLConfig)
                        bRefreshConfig = true;
                }
            }
            base.OnPropertyChanging(newValue, propertyName, afterChange);
        }

        #endregion


        #region Internal and Static

        internal static Type GetGenericACPropertyType(Type acPropertyType, Type typeT, Type typeTGeneric = null)
        {
            if (typeTGeneric == null)
                return acPropertyType.MakeGenericType(new Type[] { typeT });
            else
            {
                Type genericOfTypeT = typeTGeneric.MakeGenericType(new Type[] { typeT });
                return acPropertyType.MakeGenericType(new Type[] { genericOfTypeT });
            }
        }

        internal static Type GetConvertibleACPropertyType(Type acPropertyType, Type typeT, Type typeS)
        {
            return acPropertyType.MakeGenericType(new Type[] { typeT, typeS });
        }


        /// <summary>
        /// Raises the on property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        public void RaiseOnPropertyChanged(string property)
        {
            this.OnPropertyChanged(property);
        }


        internal ACClass Safe_ACClass
        {
            get
            {
                ACClass sc = null;
                if (ACClassReference.IsLoaded)
                    sc = (ACClass) ACClassReference.CurrentValue;
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
                    sc = (ACClass) ValueTypeACClassReference.CurrentValue;
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



        internal ACClass Safe_ConfigACClass
        {
            get
            {
                if (!this.ConfigACClassID.HasValue)
                    return null;

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return this.ConfigACClass;
                }
            }
        }
#endregion

#endregion

    }
}
