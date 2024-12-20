// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;

namespace gip.core.autocomponent
{
    /// <summary>
    /// PWPointEventSubscr is the base class for Subscription-Points that are used by Worfklow-Classes.
    /// It reads the ACClassWFEdge-Table to subscribe the Events of the Source-Workflownodes.
    /// </summary>
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWPointEventSubscr'}de{'PWPointEventSubscr'}", Global.ACKinds.TACClass)]
    public class PWPointEventSubscr : ACPointNetEventSubscrBase<ACComponent>, IACPointEventSubscr
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public PWPointEventSubscr()
            : this(null, (ACClassProperty)null, 0)
        {
        }

        /// <summary>
        /// Constructor for Reflection-Instantiation
        /// </summary>
        /// <param PWPointSubscr="parent"></param>
        public PWPointEventSubscr(PWBase parent, IACType acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
        }

        public PWPointEventSubscr(PWBase parent, string propertyName, uint maxCapacity)
            : this(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
        }

        #endregion

        private class SafeWFNodeEdgeResult
        {
            public ACClassWFEdge Edge { get; set; }
            public ACClassWF SourceACClassWF { get; set; }
            public ACClassWF TargetACClassWF { get; set; }
            public ACClassProperty SourceACClassProperty { get; set; }
            public ACClassProperty TargetACClassProperty { get; set; }
        }


        /// <summary>
        /// This method reads the ACClassWFEdge-Table to subscribe the Events of the Source-Workflownodes.
        /// The parameter asyncCallbackDelegate is used to set the Callback-Method for the Event-Subscription.
        /// </summary>
        /// <param name="asyncCallbackDelegate">The asynchronous callback delegate.</param>
        public void SubscribeACClassWFEdgeEvents(ACPointNetEventDelegate asyncCallbackDelegate)
        {
            if (_ACRefParent.ValueT == null)
                return;
            ACPointEventAbsorber eventAbsorber = (_ACRefParent.ValueT.Root as ACRoot).EventAbsorber;
            // If PWNode has been reinitialized by loading a new workflow, then the ConnectionList is empty.
            // Otherwise, the ConnectionList was automatically restored from ACClassTask value.
            using (ACMonitor.Lock(LockConnectionList_20040))
            {
                if (!ConnectionList.Any())
                {
                    PWBase pwNode = _ACRefParent.ValueT as PWBase;
                    if (pwNode != null && pwNode.ContentACClassWF != null)
                    {
                        SafeWFNodeEdgeResult[] edges = null;
                        // Reload if workflow changes have been made on the client side
                        try
                        {
                            bool mustRefreshEdges = pwNode.ContentACClassWF.ACClassMethod.MustRefreshACClassWF;
                            bool edgesLoaded = pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF_IsLoaded;
                            IEnumerable<ACClassWFEdge> edgesArray = null;
                            if (mustRefreshEdges || !edgesLoaded)
                            {
                                using (ACMonitor.Lock(pwNode.ContextLockForACClassWF))
                                {
                                    edgesArray = pwNode.ContentACClassWF.Database.ACClassWFEdge
                                        .Include(c => c.SourceACClassWF)
                                        .Include(c => c.TargetACClassWF)
                                        .Include(c => c.SourceACClassProperty)
                                        .Include(c => c.TargetACClassProperty)
                                        .Where(c => c.TargetACClassWFID == pwNode.ContentACClassWF.ACClassWFID)
                                        .ToArray();

                                    if (edgesLoaded && mustRefreshEdges)
                                    {
                                        edgesArray.SynchronizeCollections<ACClassWFEdge>(pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF);
                                        //pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF.AutoRefresh();
                                        //pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF.AutoLoad(pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWFReference, pwNode.ContentACClassWF);
                                    }
                                    //else
                                    //    pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWFReference.Load();


                                    //edges = pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF.CreateSourceQuery()
                                    //    .Include(c => c.SourceACClassWF)
                                    //    .Include(c => c.TargetACClassWF)
                                    //    .Include(c => c.SourceACClassProperty)
                                    //    .Include(c => c.TargetACClassProperty)
                                    //    .Select(c => new SafeWFNodeEdgeResult()
                                    //    {
                                    //        Edge = c,
                                    //        SourceACClassWF = c.SourceACClassWF,
                                    //        TargetACClassWF = c.TargetACClassWF,
                                    //        SourceACClassProperty = c.SourceACClassProperty,
                                    //        TargetACClassProperty = c.TargetACClassProperty
                                    //    })
                                    //    .ToArray();
                                }
                            }
                            else
                            {
                                edgesArray = pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF
                                    .ToArray();
                                foreach (var edge in edgesArray)
                                {
                                    if (   !edge.SourceACClassWF_IsLoaded
                                        || !edge.TargetACClassWF_IsLoaded
                                        || !edge.SourceACClassProperty_IsLoaded
                                        || !edge.TargetACClassProperty_IsLoaded)
                                    {
                                        mustRefreshEdges = true;
                                        break;
                                    }
                                }
                                if (mustRefreshEdges)
                                {
                                    using (ACMonitor.Lock(pwNode.ContextLockForACClassWF))
                                    {
                                        edgesArray = pwNode.ContentACClassWF.Database.ACClassWFEdge
                                            .Include(c => c.SourceACClassWF)
                                            .Include(c => c.TargetACClassWF)
                                            .Include(c => c.SourceACClassProperty)
                                            .Include(c => c.TargetACClassProperty)
                                            .Where(c => c.TargetACClassWFID == pwNode.ContentACClassWF.ACClassWFID)
                                            .ToArray();
                                    }
                                }
                            }
                            edges = edgesArray.Select(c => new SafeWFNodeEdgeResult()
                            {
                                Edge = c,
                                SourceACClassWF = c.SourceACClassWF,
                                TargetACClassWF = c.TargetACClassWF,
                                SourceACClassProperty = c.SourceACClassProperty,
                                TargetACClassProperty = c.TargetACClassProperty
                            })
                            .ToArray();
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("PWPointEventSubscr", "SubscribeACClassWFEdgeEvents", msg);
                        }

                        if (edges != null && edges.Any())
                        {
                            // ((gip.core.datamodel.ACClassProperty)this.ACType).ACClass.ACClass1_BasedOnACClass.GetMember(this.ACType.ACIdentifier)
                            foreach (SafeWFNodeEdgeResult edge in edges)
                            {
                                if (pwNode.RootPW != null 
                                        && (   (edge.TargetACClassProperty.ACClassPropertyID == this.ACType.ACTypeID)
                                            // Temporary Workaround, because PWGroup doesn't have a PWPointIn-Property. But in the Past it has and therefore old Databases has this wrong entry.
                                            // A deletion is only possible when no ACClassTask is active.
                                            // Therefore the Workflows must be unloaded first and then the following Script has to be executed:
                                            // delete ACClassProperty where ACClassPropertyID  = '61b790c9-a379-483d-b3af-1594b567a730';
                                            || (   ((gip.core.datamodel.ACClassProperty)this.ACType).ACClass.ACIdentifier == "PWGroup" 
                                                && ((gip.core.datamodel.ACClassProperty)this.ACType).ACClass.BaseClass.GetMember(this.ACType.ACIdentifier)?.ACTypeID == edge.TargetACClassProperty.ACClassPropertyID)
                                         )
                                   )
                                {
                                    IACComponent sourceComponent = pwNode.RootPW.WFDictionary.GetPWComponent(edge.SourceACClassWF);
                                    if (sourceComponent != null)
                                    {
                                        ACPointEventSubscrWrap<ACComponent> eventSubscrEntry = null;

                                        eventSubscrEntry = SubscribeEvent(sourceComponent, edge.SourceACClassProperty.ACIdentifier, asyncCallbackDelegate);
                                        if ((eventSubscrEntry != null) && (edge.Edge.ConnectionType == Global.ConnectionTypes.StartTriggerDirect))
                                            eventSubscrEntry.IsTriggerDirect = true;
                                        if (eventAbsorber != null)
                                            eventAbsorber.RedirectEventSubscr(eventSubscrEntry);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (eventAbsorber != null)
                {
                    foreach (ACPointEventSubscrWrap<ACComponent> eventSubscrEntry in ConnectionList)
                    {
                        eventAbsorber.RedirectEventSubscr(eventSubscrEntry);
                    }
                }
            }
        }


        /// <summary>This method reads the ACClassWFEdge-Table to subscribe the Events of the Source-Workflownodes.</summary>
        public void SubscribeACClassWFEdgeEvents()
        {
            if (_ACRefParent.ValueT == null)
                return;
            ACPointEventAbsorber eventAbsorber = (_ACRefParent.ValueT.Root as ACRoot).EventAbsorber;


            using (ACMonitor.Lock(LockConnectionList_20040))
            {
                // If PWNode has been reinitialized by loading a new workflow, then the ConnectionList is empty.
                // Otherwise, the ConnectionList was automatically restored from ACClassTask value.
                if (!ConnectionList.Any())
                {
                    String callbackMethod = (this.ACType as ACClassProperty).CallbackMethodName;
                    if (String.IsNullOrEmpty(callbackMethod))
                        return;

                    PWBase pwNode = _ACRefParent.ValueT as PWBase;
                    if (pwNode != null && pwNode.ContentACClassWF != null)
                    {
                        SafeWFNodeEdgeResult[] edges = null;
                        // Reload if workflow changes have been made on the client side
                        try
                        {
                            bool mustRefreshEdges = pwNode.ContentACClassWF.ACClassMethod.MustRefreshACClassWF;
                            bool edgesLoaded = pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF_IsLoaded;
                            IEnumerable<ACClassWFEdge> edgesArray = null;
                            if (mustRefreshEdges || !edgesLoaded)
                            {

                                using (ACMonitor.Lock(pwNode.ContextLockForACClassWF))
                                {
                                    edgesArray = pwNode.ContentACClassWF.Database.Entry(pwNode).Collection(c => c.ContentACClassWF.ACClassWFEdge_TargetACClassWF)
                                        .Query()
                                        .Include(c => c.SourceACClassWF)
                                        .Include(c => c.TargetACClassWF)
                                        .Include(c => c.SourceACClassProperty)
                                        .Include(c => c.TargetACClassProperty)
                                        .Where(c => c.TargetACClassWFID == pwNode.ContentACClassWF.ACClassWFID)
                                        .ToArray();

                                    if (edgesLoaded && mustRefreshEdges)
                                    {
                                        edgesArray.SynchronizeCollections<ACClassWFEdge>(pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF);
                                        //pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF.AutoRefresh();
                                        //pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF.AutoLoad(pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWFReference, pwNode.ContentACClassWF);
                                    }
                                    //else
                                    //    pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWFReference.Load();


                                    //edges = pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF.CreateSourceQuery()
                                    //    .Include(c => c.SourceACClassWF)
                                    //    .Include(c => c.TargetACClassWF)
                                    //    .Include(c => c.SourceACClassProperty)
                                    //    .Include(c => c.TargetACClassProperty)
                                    //    .Select(c => new SafeWFNodeEdgeResult()
                                    //    {
                                    //        Edge = c,
                                    //        SourceACClassWF = c.SourceACClassWF,
                                    //        TargetACClassWF = c.TargetACClassWF,
                                    //        SourceACClassProperty = c.SourceACClassProperty,
                                    //        TargetACClassProperty = c.TargetACClassProperty
                                    //    })
                                    //    .ToArray();
                                }
                            }
                            else
                            {
                                edgesArray = pwNode.ContentACClassWF.ACClassWFEdge_TargetACClassWF
                                    .ToArray();
                                foreach (var edge in edgesArray)
                                {
                                    if (!edge.SourceACClassWF_IsLoaded
                                        || !edge.TargetACClassWF_IsLoaded
                                        || !edge.SourceACClassProperty_IsLoaded
                                        || !edge.TargetACClassProperty_IsLoaded)
                                    {
                                        mustRefreshEdges = true;
                                        break;
                                    }
                                }
                                if (mustRefreshEdges)
                                {
                                    using (ACMonitor.Lock(pwNode.ContextLockForACClassWF))
                                    {
                                        edgesArray = pwNode.ContentACClassWF.Database.ACClassWFEdge
                                            .Include(c => c.SourceACClassWF)
                                            .Include(c => c.TargetACClassWF)
                                            .Include(c => c.SourceACClassProperty)
                                            .Include(c => c.TargetACClassProperty)
                                            .Where(c => c.TargetACClassWFID == pwNode.ContentACClassWF.ACClassWFID)
                                            .ToArray();
                                    }
                                }
                            }
                            edges = edgesArray.Select(c => new SafeWFNodeEdgeResult()
                            {
                                Edge = c,
                                SourceACClassWF = c.SourceACClassWF,
                                TargetACClassWF = c.TargetACClassWF,
                                SourceACClassProperty = c.SourceACClassProperty,
                                TargetACClassProperty = c.TargetACClassProperty
                            })
                            .ToArray();
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("PWPointEventSubscr", "SubscribeACClassWFEdgeEvents(20)", msg);
                        }

                        if (edges != null && edges.Any())
                        {
                            foreach (SafeWFNodeEdgeResult edge in edges)
                            {
                                if (pwNode.RootPW != null
                                        && ((edge.TargetACClassProperty.ACClassPropertyID == this.ACType.ACTypeID)
                                            // Temporary Workaround, because PWGroup doesn't have a PWPointIn-Property. But in the Past it has and therefore old Databases has this wrong entry.
                                            // A deletion is only possible when no ACClassTask is active.
                                            // Therefore the Workflows must be unloaded first and then the following Script has to be executed:
                                            // delete ACClassProperty where ACClassPropertyID  = '61b790c9-a379-483d-b3af-1594b567a730';
                                            || (((gip.core.datamodel.ACClassProperty)this.ACType).ACClass.ACIdentifier == "PWGroup"
                                                && ((gip.core.datamodel.ACClassProperty)this.ACType).ACClass.BaseClass.GetMember(this.ACType.ACIdentifier)?.ACTypeID == edge.TargetACClassProperty.ACClassPropertyID)
                                         )
                                   )
                                {
                                    IACComponent sourceComponent = pwNode.RootPW.WFDictionary.GetPWComponent(edge.SourceACClassWF);
                                    if (sourceComponent != null)
                                    {
                                        ACPointEventSubscrWrap<ACComponent> eventSubscrEntry = null;
                                        eventSubscrEntry = SubscribeEvent(sourceComponent, edge.SourceACClassProperty.ACIdentifier, (this.ACType as ACClassProperty).CallbackMethodName);
                                        if ((eventSubscrEntry != null) && (edge.Edge.ConnectionType == Global.ConnectionTypes.StartTriggerDirect))
                                            eventSubscrEntry.IsTriggerDirect = true;
                                        if (eventAbsorber != null)
                                            eventAbsorber.RedirectEventSubscr(eventSubscrEntry);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (eventAbsorber != null)
                {
                    foreach (ACPointEventSubscrWrap<ACComponent> eventSubscrEntry in ConnectionList)
                    {
                        eventAbsorber.RedirectEventSubscr(eventSubscrEntry);
                    }
                }
            }
        }
    }


    [DataContract]
    internal class PWPointEventSubscrProxy : ACPointNetEventSubscrProxy<ACComponent>, IACPointEventSubscr
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public PWPointEventSubscrProxy()
            : this(null, null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public PWPointEventSubscrProxy(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
        }
        #endregion

        public override void OnPointReceivedRemote(IACPointNetBase receivedPoint)
        {
            base.OnPointReceivedRemote(receivedPoint);
        }
    }

}
