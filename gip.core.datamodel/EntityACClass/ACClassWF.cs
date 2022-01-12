// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 12-14-2012
// ***********************************************************************
// <copyright file="ACClassWF.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACClassWF represents a Workflow-Node.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Workflow'}de{'Workflow'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(2, Const.ACIdentifierPrefix, "en{'Identifier'}de{'Identifizierer'}","", "", true)]
    [ACPropertyEntity(3, "XName", "en{'XName'}de{'XName'}","", "", true)]
    [ACPropertyEntity(4, "ACInstanceNo", "en{'Instance No'}de{'Instanznr'}","", "", true)]
    [ACPropertyEntity(5, "ACClassMethod", "Method'}de{'Methode'}", Const.ContextDatabaseIPlus + "\\" + ACClassMethod.ClassName, "", true)]
    [ACPropertyEntity(6, "PWACClass", "en{'Workflowclass'}de{'Workflowklasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(7, "RefPAACClass", "en{'Application Class'}de{'Anwendungsklasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(8, "RefPAACClassMethod", "en{'Application Method'}de{'Anwendungsmethode'}", Const.ContextDatabaseIPlus + "\\" + ACClassMethod.ClassName, "", true)]
    [ACPropertyEntity(9, "PhaseIdentifier", "en{'Phase'}de{'Phase'}","", "", true)]
    [ACPropertyEntity(10, "Comment", "en{'Comment'}de{'Bemerkung'}","", "", true)]
    [ACPropertyEntity(9999, "ParentACClassWF", "en{'Parent Workflow'}de{'Elternworkflow'}", Const.ContextDatabaseIPlus + "\\" + ACClassWF.ClassName, "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassWF.ClassName, "en{'Workflow'}de{'Workflow'}", typeof(ACClassWF), ACClassWF.ClassName, Const.ACCaptionPrefix, Const.ACIdentifierPrefix)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassWF>) })]
    public partial class ACClassWF : IACObjectEntity, IACWorkflowNode, IACEntityProperty, IACClassDesignProvider, IACClassEntity, ICloneable, IACConfigURL
    {
        public const string ClassName = "ACClassWF";
        public const string NoColumnName = "WFNo";
        public const string FormatNewNo = "WF{0}";

        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="secondaryKey"></param>
        /// <returns>ACClassWF.</returns>
        public static ACClassWF NewACObject(Database database, IACObject parentACObject, string secondaryKey)
        {
            ACClassWF entity = new ACClassWF();
            entity.Database = database;
            entity.ACClassWFID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            if (parentACObject is ACClassMethod)
            {
                entity.ACClassMethod = parentACObject as ACClassMethod;
            }

            entity.ACIdentifier = secondaryKey;
            entity.XName = entity.ACIdentifier;
            entity.BranchNo = 0;
            entity.Comment = "";
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        /// <summary>
        /// News the AC class WF.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acClassMethod">The ac class method.</param>
        /// <param name="pwACClass">The pw AC class.</param>
        /// <param name="paACClass">The pa AC class.</param>
        /// <param name="paACClassMethod">The pa AC class method.</param>
        /// <param name="groupVisualClass">The group visual class.</param>
        /// <param name="secondaryKey"></param>
        /// <returns>ACClassWF.</returns>
        public static ACClassWF NewACClassWF(Database database, ACClassMethod acClassMethod, ACClass pwACClass, ACClass paACClass, ACClassMethod paACClassMethod, IACWorkflowNode groupVisualClass, string secondaryKey)
        {
            ACClassWF entity = ACClassWF.NewACObject(database, acClassMethod, secondaryKey);
            entity.WFGroup = groupVisualClass;
            entity.PWACClass = pwACClass;
            entity.RefPAACClass = paACClass;
            entity.RefPAACClassMethod = paACClassMethod;

            ACClassDesign acClassDesign = entity.PWACClass.ACType.GetDesign(entity.PWACClass, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);

            string acIdentifier = "";
            if (entity.RefPAACClassMethod != null)
            {
                acIdentifier = entity.RefPAACClassMethod.ACIdentifier;
            }
            else if (entity.RefPAACClass != null)
            {
                acIdentifier = entity.RefPAACClass.ACIdentifier;
            }
            else
            {
                switch (entity.PWACClass.ACKind)
                {
                    case Global.ACKinds.TPWNodeStart:
                        entity.ACIdentifier = Const.TPWNodeStart;
                        return entity;
                    case Global.ACKinds.TPWNodeEnd:
                        entity.ACIdentifier = Const.TPWNodeEnd;
                        return entity;
                    default:
                            acIdentifier = entity.PWACClass.ACIdentifier;
                        break;
                }
            }

            int nextInstanceNo = 0;

            if (entity.ACClassWF1_ParentACClassWF != null)
            {
                var query = entity.ACClassWF1_ParentACClassWF.ACClassWF_ParentACClassWF.ToList()
                    .Where(c => (c.ACClassWFID != entity.ACClassWFID) && (c.ACIdentifierPrefix == acIdentifier))
                    .Select(c => c.ACInstanceNo);
                if (query.Any())
                    nextInstanceNo = query.Max() + 1;
                entity.ACIdentifier = acIdentifier + "(" + nextInstanceNo.ToString() + ")";
            }
            else
            {
                entity.ACIdentifier = acIdentifier;
            }

            return entity;
        }

        /// <summary>
        /// Extracts the name of the type.
        /// </summary>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <returns>System.String.</returns>
        static public string ExtractTypeName(string acIdentifier)
        {
            if (String.IsNullOrEmpty(acIdentifier))
                return acIdentifier;
            int pos = acIdentifier.IndexOf('_');
            if (pos == -1)
            {
                return acIdentifier;
            }
            else
            {
                return acIdentifier.Substring(0, pos);
            }
        }

        /// <summary>
        /// Gets the AC instance no.
        /// </summary>
        /// <value>The AC instance no.</value>
        public Int32 ACInstanceNo
        {
            get
            {
                int p1 = ACIdentifier.IndexOf('(');
                int p2 = ACIdentifier.IndexOf(')');
                if ( p1 > 0 && p2 > p1)
                {
                    Int32 result;
                    if (Int32.TryParse(ACIdentifier.Substring(p1+1, p2-p1-1), out result))
                        return result;
                }
                return 0;
            }
        }

        /// <summary>
        /// Gets the AC identifier prefix.
        /// </summary>
        /// <value>The AC identifier prefix.</value>
        public string ACIdentifierPrefix
        {
            get
            {
                int p1 = ACIdentifier.IndexOf('(');
                if ( p1 > 0 )
                {
                    return ACIdentifier.Substring(0, p1);
                }
                else
                {
                    return ACIdentifier;
                }
            }
        }
        #endregion

        #region IACObjectEntity Members

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

        #region IEntityProperty Members

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

        #endregion

        #region IMethodWF Members

        /// <summary>
        /// Returns ParentACClassWF or ACClassMethod if root
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to ParentACClassWF or ACClassMethod if root</value>
        [ACPropertyInfo(9999)]
        public override IACObject ParentACObject
        {
            get
            {
                if (ACClassWF1_ParentACClassWF != null)
                    return ACClassWF1_ParentACClassWF;
                return ACClassMethod;
            }
        }

        /// <summary>
        /// Returns a related EF-Object which is in a Child-Relationship to this.
        /// </summary>
        /// <param name="className">Classname of the Table/EF-Object</param>
        /// <param name="filterValues">Search-Parameters</param>
        /// <returns>A Entity-Object as IACObject</returns>
        public override IACObject GetChildEntityObject(string className, params string[] filterValues)
        {
            if (filterValues.Any() && className == ACClassWF.ClassName)
            {

                using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                {
                    return ACClassWF_ParentACClassWF.Where(c => c.ACIdentifier == filterValues[0]).FirstOrDefault();
                }
            }

            return null;
        }


        /// <summary>
        /// All edges that starts from this node
        /// </summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value>List of edges</value>
        public IEnumerable<IACWorkflowEdge> GetOutgoingWFEdges(IACWorkflowContext context)
        {
            return this.ACClassWFEdge_SourceACClassWF;
        }

        /// <summary>
        /// If this Node is a Workflow-Group, this method returns all outgoing-edges belonging to this node.
        /// </summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value>List of edges</value>
        public IEnumerable<IACWorkflowEdge> GetOutgoingWFEdgesInGroup(IACWorkflowContext context)
        {
            if (this.EntityState != System.Data.EntityState.Added && !ACClassMethod.ACClassWFEdge_ACClassMethod.IsLoaded)
                ACClassMethod.ACClassWFEdge_ACClassMethod.Load();
            return GetOutgoingWFEdges(context).Where(c => ((ACClassWF)c.ToWFNode).ACClassWF1_ParentACClassWF.ACClassWFID == this.ACClassWF1_ParentACClassWF.ACClassWFID).Select(c => c);
        }

        /// <summary>
        /// All edges that ends in this node
        /// </summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value>List of edges</value>
        public IEnumerable<IACWorkflowEdge> GetIncomingWFEdges(IACWorkflowContext context)
        {
            return this.ACClassWFEdge_TargetACClassWF;
        }

        /// <summary>
        /// If this Node is a Workflow-Group, this method returns all incoming-edges belonging to this node.
        /// </summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value>List of edges</value>
        public IEnumerable<IACWorkflowEdge> GetIncomingWFEdgesInGroup(IACWorkflowContext context)
        {
            if (this.EntityState != System.Data.EntityState.Added && !ACClassMethod.ACClassWFEdge_ACClassMethod.IsLoaded)
                ACClassMethod.ACClassWFEdge_ACClassMethod.Load();
            return GetIncomingWFEdges(context).Where(c => ((ACClassWF)c.FromWFNode).ACClassWF1_ParentACClassWF.ACClassWFID == this.ACClassWF1_ParentACClassWF.ACClassWFID).Select(c => c);
        }

        /// <summary>
        /// Gets the parent AC class.
        /// </summary>
        /// <value>The parent AC class.</value>
        public ACClass ParentACClass 
        { 
            get
            {
                if ( WFGroup != null && WFGroup is ACClassWF)
                    return ((ACClassWF)WFGroup).RefPAACClass;
                return null;
            }
        }

        ACClassWF _GroupACClassWF = null;
        /// <summary>
        /// Reference to the parent Workflow-Node that groups more child-nodes together
        /// </summary>
        /// <value>Parent Workflow-Node (Group)</value>
        public IACWorkflowNode WFGroup
        {
            get
            {
                if (ACClassWF1_ParentACClassWF == null)
                    return null;
                if (_GroupACClassWF == null || _GroupACClassWF.WFObjectID != ACClassWF1_ParentACClassWF.ACClassWFID)
                    _GroupACClassWF = this.ACClassMethod.ACClassWF_ACClassMethod.Where(c => c.ACClassWFID == ACClassWF1_ParentACClassWF.ACClassWFID).FirstOrDefault();
                return _GroupACClassWF;
            }
            set
            {
                ACClassWF1_ParentACClassWF = value as ACClassWF;
                _GroupACClassWF = ACClassWF1_ParentACClassWF;
            }
        }


        /// <summary>
        /// If this Node is a Workflow-Group, this method returns all nnodes that are inside of this group.
        /// </summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value>List of nodes</value>
        public IEnumerable<IACWorkflowNode> GetChildWFNodes(IACWorkflowContext context)
        {
            return ACClassWF_ParentACClassWF;
        }

        /// <summary>
        /// Returns true if this Node is a Workflow-Group and is the most outer node.
        /// </summary>
        /// <param name="context">Workflowcontext. (Main Entity Class that saves the Workflow and the Nodes and Edges are related to)</param>
        /// <value><c>true</c> if this Node is a Workflow-Group and is the most outer node; otherwise, <c>false</c>.</value>
        public bool IsRootWFNode(IACWorkflowContext context)
        {
            return ACClassWF1_ParentACClassWF == null;
        }

        /// <summary>
        /// Returns the ACClassProperty that reprensents a Connection-Point where Edges can be connected to.
        /// </summary>
        /// <param name="acPropertyName">Name of the property.</param>
        /// <returns>ACClassProperty.</returns>
        public ACClassProperty GetConnector(string acPropertyName)
        {
            return PWACClass.GetPoint(acPropertyName);
        }

        /// <summary>
        /// Returns the ACUrl that a reel instance will have in runtime.
        /// Otherwise a empty string will be returned
        /// THREAD-SAFE (QueryLock_1X000)
        /// </summary>
        /// <param name="rootACObject">If null, then a absolute ACUrl will be returned. Else a relative url to the passed object.</param>
        /// <returns>ACUrl as string</returns>
        public override string GetACUrlComponent(IACObject rootACObject = null)
        {
            if (this.ACClassWF1_ParentACClassWF != null)
            {
                string parentUrl = this.ACClassWF1_ParentACClassWF.GetACUrlComponent(rootACObject);
                return parentUrl + "\\" + ACIdentifier;
            }
            else
                return ACIdentifier;            
        }
        #endregion

        #region IVisual Members

        /// <summary>
        /// Unique ID of the Workflow Node
        /// </summary>
        /// <value>Returns ACClassWFID</value>
        public Guid WFObjectID
        {
            get { return ACClassWFID; }
            set { ACClassWFID = value; }
        }

        public override string ToString()
        {
            return ACCaption;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(1, "", "en{'Description'}de{'Bezeichnung'}")]
        public override string ACCaption
        {
            get
            {
                string caption = "";
                if (this.ACClassWF1_ParentACClassWF == null && RefPAACClass != null)
                    caption = RefPAACClass.ACCaption + " (" + PWACClass.ACCaption + ")";
                if (String.IsNullOrEmpty(caption) && RefPAACClassMethod != null)
                    caption = RefPAACClassMethod.ACCaptionAttached;
                if (String.IsNullOrEmpty(caption) && RefPAACClass != null)
                    caption = RefPAACClass.ACCaption;
                if (String.IsNullOrEmpty(caption))
                    caption = PWACClass.ACCaption;
                if (!String.IsNullOrEmpty(this.Comment))
                    caption += Environment.NewLine + this.Comment;
                return caption;
            }
            set
            {
            }
        }

        /// <summary>Returns a ACClassDesign for presenting itself on the gui</summary>
        /// <param name="acUsage">Filter for selecting designs that belongs to this ACUsages-Group</param>
        /// <param name="acKind">Filter for selecting designs that belongs to this ACKinds-Group</param>
        /// <param name="vbDesignName">Optional: The concrete acIdentifier of the design</param>
        /// <returns>ACClassDesign</returns>
        public ACClassDesign GetDesign(Global.ACUsages acUsage, Global.ACKinds acKind, string vbDesignName = "")
        {
            if (PWACClass == null)
                return null;
            return PWACClass.ACType.GetDesign(PWACClass,acUsage, acKind, vbDesignName);
        }

        //public Double VisualBottom
        //{
        //    get
        //    {
        //        return VisualTop + VisualHeight;
        //    }
        //}

        //public Double VisualRight
        //{
        //    get
        //    {
        //        return VisualLeft + VisualWidth;
        //    }
        //}

        //public Double VisualMiddle
        //{
        //    get
        //    {
        //        return VisualLeft + (VisualWidth / 2);
        //    }
        //    set
        //    {
        //        VisualLeft = value - (VisualWidth / 2);
        //    }
        //}

        //public double VisualTopAbs
        //{
        //    get
        //    {
        //        double topAbs = 0;
        //        IACWorkflowNode rControlClass = this;
        //        do
        //        {
        //            topAbs += rControlClass.VisualTop;
        //            rControlClass = rControlClass.VisualGroup;
        //        }
        //        while (rControlClass != null);
        //        return topAbs;
        //    }
        //    set
        //    {
        //        double topAbs = 0;
        //        IACWorkflowNode rControlClass = this.VisualGroup;
        //        while (rControlClass != null)
        //        {
        //            topAbs += rControlClass.VisualTop;
        //            rControlClass = rControlClass.VisualGroup;
        //        }
        //        VisualTop = value - topAbs;
        //    }
        //}

        //public double VisualLeftAbs
        //{
        //    get
        //    {
        //        double leftAbs = 0;
        //        IACWorkflowNode rControlClass = this;
        //        do
        //        {
        //            leftAbs += rControlClass.VisualLeft;
        //            rControlClass = rControlClass.VisualGroup;
        //        }
        //        while (rControlClass != null);
        //        return leftAbs;
        //    }
        //    set
        //    {
        //        double leftAbs = 0;
        //        IACWorkflowNode rControlClass = this.VisualGroup;
        //        while (rControlClass != null)
        //        {
        //            leftAbs += rControlClass.VisualLeft;
        //            rControlClass = rControlClass.VisualGroup;
        //        }
        //        VisualLeft = value - leftAbs;
        //    }
        //}

        /// <summary>
        /// Returns a ACUrl, to be able to find this instance in the WPF-Logical-Tree.
        /// </summary>
        /// <value>ACUrl as string</value>
        public string VisualACUrl 
        {
            get
            {
                if (this.ACClassWF1_ParentACClassWF != null)
                    return this.ACClassWF1_ParentACClassWF.VisualACUrl + "\\" + ACIdentifier;
                return Const.VBPresenter_SelectedRootWFNode;
            }
        }

        #endregion

        #region Others
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
            Database db = this.GetObjectContext<Database>();
            //string secondaryKey = gip.core.datamodel.Database.Root.NoManager.GetNewNo(db, typeof(ACClassWF), ACClassWF.NoColumnName, ACClassWF.FormatNewNo, null);
            //ACClassWF clonedObject = ACClassWF.NewACObject(this.Database, null, secondaryKey);
            ACClassWF clonedObject = new ACClassWF();
            clonedObject.ACClassWFID = this.ACClassWFID;
            clonedObject.ACClassMethodID = this.ACClassMethodID;
            clonedObject.XName = this.XName;
            clonedObject.ACIdentifier = this.ACIdentifier;
            clonedObject.ParentACClassWFID = this.ParentACClassWFID;
            clonedObject.PWACClassID = this.PWACClassID;
            clonedObject.RefPAACClassID = this.RefPAACClassID;
            clonedObject.RefPAACClassMethodID = this.RefPAACClassMethodID;
            clonedObject.PhaseIdentifier = this.PhaseIdentifier;
            clonedObject.Comment = this.Comment;
            clonedObject.XMLConfig = this.XMLConfig;
            clonedObject.InsertDate = this.InsertDate;
            clonedObject.InsertName = this.InsertName;
            clonedObject.UpdateDate = this.UpdateDate;
            clonedObject.UpdateName = this.UpdateName;

            return clonedObject;
        }

        public ACClassWF Clone(Guid guid, string xname, Guid acClassMethodID)
        {
            //Database db = this.GetObjectContext<Database>();
            //string secondaryKey = gip.core.datamodel.Database.Root.NoManager.GetNewNo(db, typeof(ACClassWF), ACClassWF.NoColumnName, ACClassWF.FormatNewNo, null);
            //ACClassWF clonedObject = ACClassWF.NewACObject(this.Database, null, secondaryKey);
            ACClassWF clonedObject = new ACClassWF();
            clonedObject.ACClassWFID = guid;
            clonedObject.ACClassMethodID = acClassMethodID;
            clonedObject.XName = xname;
            clonedObject.ACIdentifier = this.ACIdentifier;
            clonedObject.ParentACClassWFID = this.ParentACClassWFID;
            clonedObject.PWACClassID = this.PWACClassID;
            clonedObject.RefPAACClassID = this.RefPAACClassID;
            clonedObject.RefPAACClassMethodID = this.RefPAACClassMethodID;
            clonedObject.PhaseIdentifier = this.PhaseIdentifier;
            clonedObject.Comment = this.Comment;
            clonedObject.XMLConfig = this.XMLConfig;
            clonedObject.Database = this.Database;
            clonedObject.UpdateDate = this.UpdateDate;
            clonedObject.UpdateName = this.UpdateName;
            clonedObject.InsertDate = this.InsertDate;
            clonedObject.InsertName = this.InsertName;
            return clonedObject;
        }

        #endregion

        #region IACConfigURL

        public string ConfigACUrl
        {
            get
            {
                if (ACClassWF1_ParentACClassWF != null)
                {
                    return ACClassWF1_ParentACClassWF.ConfigACUrl + "\\" + ACIdentifier;
                }
                else
                {
                    return ACIdentifier;
                }
            }
        }

        public string PreValueACUrl
        {
            get
            {
                return "";
            }
        }

        public string LocalConfigACUrl
        {
            get
            {
                return ACUrlHelper.BuildLocalConfigACUrl(this);
            }
        }

        public void RefreshRuleStates()
        {
        }
#endregion

        public IACObject GetParentACObject(IACObject context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ACClassWF> GetPWGroups()
        {
            return RefPAACClassMethod.ACClassWF_ACClassMethod.Where(c => c.PWACClass != null && c.PWACClass.ACKindIndex == (short)Global.ACKinds.TPWGroup);
        }
    }
}
