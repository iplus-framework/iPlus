// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACClassPropertyRelation.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACClassPropertyRelation describes physical and logical relationships between ACClassProperty.
    /// Entries in the ACClassPropertyRelation table are mainly added by iPlus development environment.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Propertyrelation'}de{'Eigenschaftsbeziehung'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "SourceACClass", "en{'Source Class'}de{'Quellklasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(2, "SourceACClassProperty", "en{'Source Property'}de{'Quelleigenschaft'}", Const.ContextDatabaseIPlus + "\\" + ACClassProperty.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(3, "TargetACClass", "en{'Target Class'}de{'Zielklasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(4, "TargetACClassProperty", "en{'Target Property'}de{'Zieleigenschaft'}", Const.ContextDatabaseIPlus + "\\" + ACClassProperty.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(5, "ConnectionTypeIndex", "en{'Index'}de{'Index'}", typeof(Global.ConnectionTypes), "", "", true)]
    [ACPropertyEntity(6, "DirectionIndex", "en{'Direction'}de{'Richtung'}", typeof(Global.Directions), "", "", true)]
    [ACPropertyEntity(7, "LogicalOperationIndex", "en{'Logical Operator'}de{'Verknüpfungsoperator'}", typeof(Global.Operators), "", "", true)]
    [ACPropertyEntity(8, "Multiplier", "en{'Multiplier source->target'}de{'Multiplikator Quelle->Ziel'}","", "", true)]
    [ACPropertyEntity(9, "Divisor", "en{'Divisor source->target'}de{'Teiler Quelle->Ziel'}","", "", true)]
    [ACPropertyEntity(10, "ConvExpressionT", "en{'Expression source->target'}de{'Formel Quelle->Ziel'}","", "", true)]
    [ACPropertyEntity(11, "ConvExpressionS", "en{'Expression target->source'}de{'Formel Ziel->Quelle'}","", "", true)]
    [ACPropertyEntity(9999, "XMLValue", "en{'Value'}de{'Wert'}")]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassPropertyRelation.ClassName, "en{'Propertyrelation'}de{'Eigenschaftsbeziehung'}", typeof(ACClassPropertyRelation), ACClassPropertyRelation.ClassName, "", ACClassPropertyRelation.ClassName + "ID")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassPropertyRelation>) })]
    [NotMapped]
    public partial class ACClassPropertyRelation : IACObjectEntity, IACClassEntity, ICloneable
    {
        public const string ClassName = "ACClassPropertyRelation";

        [NotMapped]
        public DateTime UpdateTime { get; set; }
        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <returns>ACClassPropertyRelation.</returns>
        public static ACClassPropertyRelation NewACObject(Database database, IACObject parentACObject)
        {
            ACClassPropertyRelation entity = new ACClassPropertyRelation();
            entity.ACClassPropertyRelationID = Guid.NewGuid();

            entity.DefaultValuesACObject();
            if (parentACObject is ACClass)
            {
                entity.SourceACClass = parentACObject as ACClass;
                switch (entity.SourceACClass.ACKind)
                {
                    case Global.ACKinds.TPWGroup:
                    case Global.ACKinds.TPWNode:
                    case Global.ACKinds.TPWNodeMethod:
                    case Global.ACKinds.TPWNodeWorkflow:
                    case Global.ACKinds.TPWNodeStart:
                    case Global.ACKinds.TPWNodeEnd:
                    case Global.ACKinds.TPWNodeStatic:
                        entity.ConnectionType = Global.ConnectionTypes.StartTrigger;
                        entity.Direction = Global.Directions.Forward;
                        entity.SourceACClassProperty = entity.SourceACClass.GetPoint(Const.PWPointOut);
                        break;

                    case Global.ACKinds.TPAProcessFunction:
                        entity.ConnectionType = Global.ConnectionTypes.LogicalBridge;
                        entity.Direction = Global.Directions.Forward;
                        entity.SourceACClassProperty = entity.SourceACClass.GetPoint(Const.PAPointMatOut1);
                        break;

                    default:
                        entity.ConnectionType = Global.ConnectionTypes.ConnectionPhysical;
                        entity.Direction = Global.Directions.Forward;
                        entity.SourceACClassProperty = entity.SourceACClass.GetPoint(Const.PAPointMatOut1);
                        break;
                }
            }
            entity.LogicalOperation = Global.Operators.none;
            entity.UseFactor = 1;
            entity.LastManipulationDT = new DateTime(2000,1,1);

            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        /// <summary>
        /// News the AC class property relation.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="sourceACClass">The source AC class.</param>
        /// <param name="sourceACClassProperty">The source AC class property.</param>
        /// <param name="targetACClass">The target AC class.</param>
        /// <param name="targetACClassProperty">The target AC class property.</param>
        /// <returns>ACClassPropertyRelation.</returns>
        public static ACClassPropertyRelation NewACClassPropertyRelation(Database database,
                                                                ACClass sourceACClass, ACClassProperty sourceACClassProperty,
                                                                ACClass targetACClass, ACClassProperty targetACClassProperty)
        {

            ACClassPropertyRelation entity = NewACObject(database, sourceACClass);
            entity.SourceACClassProperty = sourceACClassProperty;
            entity.TargetACClass = targetACClass;
            entity.TargetACClassProperty = targetACClassProperty;

            if (entity.TargetACClass.ACKind == Global.ACKinds.TPAProcessFunction) 
                entity.ConnectionType = Global.ConnectionTypes.LogicalBridge;

            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        #endregion

        #region IACObjectEntity Members
        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context)
        {
            //BranchNo = Database.Root.Environment.SystemBranchNo;
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
            //BranchNo = Database.Root.Environment.SystemBranchNo;
            base.EntityCheckModified(user, context);
            return null;
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
                return "SourceACClassProperty\\ACIdentifier,TargetACClass\\ACIdentifier,TargetACClassProperty\\ACIdentifier,ConnectionTypeIndex";
            }
        }

        #endregion

        #region IVisualEdge Members
        /// <summary>
        /// Gets or sets the type of the connection.
        /// </summary>
        /// <value>The type of the connection.</value>
        [NotMapped]
        public Global.ConnectionTypes ConnectionType
        {
            get
            {
                return (Global.ConnectionTypes)ConnectionTypeIndex;
            }
            set
            {
                ConnectionTypeIndex = (short)value;
            }
        }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>The direction.</value>
        [NotMapped]
        public gip.core.datamodel.Global.Directions Direction
        {
            get
            {
                return (Global.Directions)DirectionIndex;
            }
            set
            {
                DirectionIndex = (short)value;
            }
        }

        /// <summary>
        /// Gets or sets the logical operation.
        /// </summary>
        /// <value>The logical operation.</value>
        [NotMapped]
        public Global.Operators LogicalOperation
        {
            get
            {
                return (Global.Operators)LogicalOperationIndex;
            }
            set
            {
                LogicalOperationIndex = (short)value;
            }
        }
        #endregion

        #region IVisual Members

        /// <summary>
        /// Returns SourceACClass
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to SourceACClass</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public override IACObject ParentACObject
        {
            get
            {
                return SourceACClass;
            }
        }

        public override string ToString()
        {
            return ACCaption;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                return SourceACClass.ACCaption + "<->" + TargetACClass.ACCaption;
            }
            set
            {

            }
        }

        #endregion

        /// <summary>
        /// Gets the class config list.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns>IEnumerable{IACConfig}.</returns>
        public List<IACConfig> GetClassConfigList(ACClass acClass)
        {
            string keyACUrl = ".\\" + GetKey();

            using (ACMonitor.Lock(acClass.Database.QueryLock_1X000))
            {
                return acClass.ACClassConfig_ACClass.ToList().Select(x=>(IACConfig)x).Where(c => c.KeyACUrl == keyACUrl).ToList();
            }
        }
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetKey()
        {
            return SourceACClass.GetACUrl(SourceACClass.ACProject) + "\\" + SourceACClassProperty.ACIdentifier + ACUrlHelper.Delimiter_Relationship + TargetACClass.GetACUrl(TargetACClass.ACProject) + "\\" + TargetACClassProperty.ACIdentifier;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [ACPropertyInfo(8, "", "en{'Value'}de{'Wert'}")]
        [NotMapped]
        public object Value
        {
            get
            {
                return XMLValue;
            }
            set
            {
                XMLValue = value != null ? value.ToString():null;
            }
        }

#region Binding
        /// <summary>
        /// Kennzeichnet, falls eine Property instanziert worden ist, deren Binding-Partner noch nicht
        /// im Model-Baum geladen war, dass die Bindung bei der Instanzierung des Partners gemacht werden muss.
        /// </summary>
        private bool _LateBindingNeedDuringACInit = false;
        /// <summary>
        /// Gets or sets a value indicating whether [late binding need during AC init].
        /// </summary>
        /// <value><c>true</c> if [late binding need during AC init]; otherwise, <c>false</c>.</value>
        [NotMapped]
        public bool LateBindingNeedDuringACInit
        {
            get
            {
                return _LateBindingNeedDuringACInit;
            }
            set
            {
                _LateBindingNeedDuringACInit = value;
            }
        }

        /// <summary>
        /// Gets the source AC URL.
        /// </summary>
        /// <value>The source AC URL.</value>
        [ACPropertyInfo(9999, "", "en{'Source'}de{'Quelle'}")]
        [NotMapped]
        public string SourceACUrl
        {
            get
            {
                return this.SourceACClass.GetACUrlComponent() + "\\" + SourceACClassProperty.ACIdentifier;
            }
        }

        /// <summary>
        /// Gets the target AC URL.
        /// </summary>
        /// <value>The target AC URL.</value>
        [ACPropertyInfo(9999, "", "en{'Target'}de{'Ziel'}")]
        [NotMapped]
        public string TargetACUrl
        {
            get
            {
                return this.TargetACClass.GetACUrlComponent() + "\\" + TargetACClassProperty.ACIdentifier;
            }
        }
        #endregion

        [NotMapped]
        public Database Database
        {
            get
            {
                return Context as Database;
            }
        }

        [NotMapped]
        public bool AreUnitConversionParamsSet
        {
            get
            {
                if (Multiplier.HasValue 
                    || Divisor.HasValue 
                    || (!String.IsNullOrEmpty(ConvExpressionT) && !String.IsNullOrEmpty(ConvExpressionS)))
                    return true;
                return false;
            }
        }

        #region Clone
        public object Clone()
        {
            ACClassPropertyRelation clonedObject = ACClassPropertyRelation.NewACObject(this.Database, null);

            clonedObject.SourceACClassID = this.SourceACClassID;
            clonedObject.SourceACClassPropertyID = this.SourceACClassPropertyID;
            clonedObject.TargetACClassID = this.TargetACClassID;
            clonedObject.TargetACClassPropertyID = this.TargetACClassPropertyID;
            clonedObject.ConnectionTypeIndex = this.ConnectionTypeIndex;
            clonedObject.DirectionIndex = this.DirectionIndex;
            clonedObject.XMLValue = this.XMLValue;
            clonedObject.LogicalOperationIndex = this.LogicalOperationIndex;
            //clonedObject.BranchNo = this.BranchNo;
            clonedObject.Multiplier = this.Multiplier;
            clonedObject.Divisor = this.Divisor;
            clonedObject.ConvExpressionT = this.ConvExpressionT;
            clonedObject.ConvExpressionS = this.ConvExpressionS;

            return clonedObject;
        }
        #endregion
    }
}
