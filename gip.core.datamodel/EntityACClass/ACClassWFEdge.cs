// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACClassWFEdge.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACClassWFEdge describes the relationship between Workflow-Nodes.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Workflowedge'}de{'Workflowbeziehung'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "SourceACClassWF", "en{'Source Workflow'}de{'Quellworkflow'}", Const.ContextDatabaseIPlus + "\\" + ACClassWF.ClassName, "", true)]
    [ACPropertyEntity(2, "SourceACClassProperty", "en{'Source Property'}de{'Quelleigenschaft'}", Const.ContextDatabaseIPlus + "\\" + ACClassProperty.ClassName, "", true)]
    [ACPropertyEntity(3, "TargetACClassWF", "en{'Target Workflow'}de{'Zielworkflow'}", Const.ContextDatabaseIPlus + "\\" + ACClassWF.ClassName, "", true)]
    [ACPropertyEntity(4, "TargetACClassProperty", "en{'Target Property'}de{'Zieleigenschaft'}", Const.ContextDatabaseIPlus + "\\" + ACClassProperty.ClassName, "", true)]
    [ACPropertyEntity(5, "ConnectionTypeIndex", "en{'Connectiontype'}de{'Verbindungsart'}", typeof(Global.ConnectionTypes), "", "", true)]
    [ACPropertyEntity(9999, "ACClassMethod", "en{'Method'}de{'Methode'}", Const.ContextDatabaseIPlus + "\\" + ACClassMethod.ClassName, "", true)]
    [ACPropertyEntity(9999, Const.ACIdentifierPrefix, "en{'Identifier'}de{'Identifizierer'}","", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassWFEdge.ClassName, "en{'Workflowedge'}de{'Workflowbeziehung'}", typeof(ACClassWFEdge), ACClassWFEdge.ClassName, "", Const.ACIdentifierPrefix)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassWFEdge>) })]
    public partial class ACClassWFEdge : IACObjectEntity, IACWorkflowEdge, IACClassEntity, ICloneable
    {
        public const string ClassName = "ACClassWFEdge";
        public const string NoColumnName = "WFEdgeNo";
        public const string FormatNewNo = "WFEdge{0}";

        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="secondaryKey"></param>
        /// <returns>ACClassWFEdge.</returns>
        public static ACClassWFEdge NewACObject(Database database, IACObject parentACObject, string secondaryKey)
        {
            ACClassWFEdge entity = new ACClassWFEdge();
            entity.Database = database;
            entity.ACClassWFEdgeID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            if (parentACObject is ACClassMethod)
            {
                entity.ACClassMethod = parentACObject as ACClassMethod;
            }
            entity.ACIdentifier = secondaryKey;
            entity.XName = entity.ACIdentifier;
            entity.BranchNo = 0;
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

        #endregion

        #region IVisualEdge Members
        /// <summary>
        /// Reference to the From-Node (Source)
        /// </summary>
        /// <value>Reference to the From-Node (Source)</value>
        public IACWorkflowNode FromWFNode
        {
            get
            {
                return SourceACClassWF;
            }
            set
            {
                SourceACClassWF = value as ACClassWF;
            }
        }

        /// <summary>
        /// Reference to the To-Node (Destination)
        /// </summary>
        /// <value>Reference to the To-Node (Destination)</value>
        public IACWorkflowNode ToWFNode
        {
            get
            {
                return TargetACClassWF;
            }
            set
            {
                TargetACClassWF = value as ACClassWF;
            }
        }

        /// <summary>
        /// Gets or sets the type of the connection.
        /// </summary>
        /// <value>The type of the connection.</value>
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
        /// WPF-x:Name of the FromWFNode + \\ + SourceACClassProperty.ACIdentifier
        /// for indentify the connector in the Logical-Tree
        /// </summary>
        /// <value>WPF-x:Name of the FromWFNode + \\ + SourceACClassProperty.ACIdentifier</value>
        public string SourceACConnector
        {
            get
            {
                string sourceACConnector = "";

                ACClassWF parent = FromWFNode as ACClassWF;

                using (ACMonitor.Lock(Database.QueryLock_1X000))
                {
                    while (parent != null)
                    {
                        sourceACConnector = parent.XName + "\\" + sourceACConnector;
                        parent = parent.ACClassWF1_ParentACClassWF as ACClassWF;
                    }
                }

                if (this.SourceACClassProperty == null)
                    return sourceACConnector;

                sourceACConnector += this.SourceACClassProperty.ACIdentifier;
                return sourceACConnector;
            }
        }


        /// <summary>
        /// WPF-x:Name of the ToWFNode + \\ + TargetACClassProperty.ACIdentifier
        /// for indentify the connector in the Logical-Tree
        /// </summary>
        /// <value>WPF-x:Name of the ToWFNode + \\ + TargetACClassProperty.ACIdentifier</value>
        public string TargetACConnector
        {
            get
            {
                string targetACConnector = "";

                ACClassWF parent = ToWFNode as ACClassWF;

                using (ACMonitor.Lock(Database.QueryLock_1X000))
                {
                    while (parent != null)
                    {
                        targetACConnector = parent.XName + "\\" + targetACConnector;
                        parent = parent.ACClassWF1_ParentACClassWF as ACClassWF;
                    }
                }

                if (this.TargetACClassProperty == null)
                    return targetACConnector;

                targetACConnector += this.TargetACClassProperty.ACIdentifier;
                return targetACConnector;
            }
        }
        #endregion

        #region IVisual Members

        /// <summary>
        /// Returns ACClassMethod
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to ACClassMethod</value>
        [ACPropertyInfo(9999)]
        public override IACObject ParentACObject
        {
            get
            {
                return ACClassMethod;
            }
        }

        /// <summary>
        /// Unique ID of the Workflow Edge
        /// </summary>
        /// <value>Returns ACClassWFEdgeID</value>
        public Guid WFObjectID
        {
            get
            {
                return ACClassWFEdgeID;
            }
            set
            {
                ACClassWFEdgeID = value;
            }
        }

        public override string ToString()
        {
            return SourceACClassWF.XName + "<->" + TargetACClassWF.XName;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        public override string ACCaption
        {
            get
            {
                if (SourceACClassWF == null || TargetACClassWF == null) return null;
                return SourceACClassWF.ACCaption + "<->" + TargetACClassWF.ACCaption;
            }
            set
            {

            }
        }

        /// <summary>
        /// Reference to the parent Workflow-Node that groups more child-nodes together
        /// </summary>
        /// <value>Parent Workflow-Node (Group)</value>
        public IACWorkflowNode WFGroup
        {
            get
            {
                return SourceACClassWF.WFGroup;
            }
            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the height of the visual.
        /// </summary>
        /// <value>The height of the visual.</value>
        public double VisualHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of the visual.
        /// </summary>
        /// <value>The width of the visual.</value>
        public double VisualWidth { get; set; }
#endregion

        #region Others

        /// <summary>
        /// ACIdentifier of the FromWFNode
        /// </summary>
        /// <value>ACIdentifier of the FromWFNode</value>
        public string SourceACName
        {
            get
            {
                return SourceACClassWF != null ? SourceACClassWF.ACIdentifier : "";
            }
        }

        /// <summary>
        /// ACIdentifier of the ToWFNode
        /// </summary>
        /// <value>ACIdentifier of the ToWFNode</value>
        public string TargetACName
        {
            get
            {
                return TargetACClassWF != null ? TargetACClassWF.ACIdentifier : "";
            }
        }

        public Database Database { get; private set; } = null;

        void IACClassEntity.OnObjectMaterialized(Database db)
        {
            if (Database == null)
                Database = db;
        }


        #endregion

        #region Clone

        public object Clone()
        {
            ACClassWFEdge clonedObject = ACClassWFEdge.NewACObject(this.Database, null, "");
            clonedObject.ACClassWFEdgeID = this.ACClassWFEdgeID;
            clonedObject.ACClassMethodID = this.ACClassMethodID;
            clonedObject.XName = this.XName;
            clonedObject.ACIdentifier = this.ACIdentifier;
            clonedObject.SourceACClassWFID = this.SourceACClassWFID;
            clonedObject.SourceACClassPropertyID = this.SourceACClassPropertyID;
            clonedObject.SourceACClassMethodID = this.SourceACClassMethodID;
            clonedObject.TargetACClassWFID = this.TargetACClassWFID;
            clonedObject.TargetACClassPropertyID = this.TargetACClassPropertyID;
            clonedObject.TargetACClassMethodID = this.TargetACClassMethodID;
            clonedObject.ConnectionTypeIndex = this.ConnectionTypeIndex;
            clonedObject.BranchNo = this.BranchNo;

            return clonedObject;
        }

#endregion      
    }
}
