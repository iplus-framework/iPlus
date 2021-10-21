using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Container, der zu versendente Daten enthält die sich auf einen Empfänger bzw. Sender beziehen.
    /// Beispiel:
    ///     Falls Programminstanz mehrere ACServiceClients hat, die auf unterscheidliche Netzwerkinstanzen verweisen
    ///     -> ACRoot hat EINE LISTE von ACPDispatch pro Kommunikationspartner/ObserverClient-Instanz
    ///     
    ///     Falls Programminstanz auf einen oder mehrere ACServiceServer hat, 
    ///     -> ACRoot hat EINE INSTANZ von ACPDispatch
    /// 
    /// </summary>
    public abstract class ACPDispatchBase
    {
        #region c'tors
        public ACPDispatchBase()
        {
        }
        #endregion

        #region Members
        public virtual bool IsServer
        {
            get
            {
                return false;
            }
        }

        protected readonly ACMonitorObject _20055_LockProjectDispatchList = new ACMonitorObject(20055);
        protected List<ACPDispatchProj> _ProjectDispatchList = new List<ACPDispatchProj>();
        internal List<ACPDispatchProj> ProjectDispatchList
        {
            get
            {
                return _ProjectDispatchList;
            }
        }
        #endregion

        #region Methods

        private const string ACIdentifier = "ACPDispatchBase";
        public bool Enqueue(IACComponent Project, IACPropertyNetValueEvent eventArgs)
        {
            if ((Project == null) || (eventArgs == null))
                return false;
            // Falls kein Client, das ACObject abonniert hat, dann muss auch nicht verteilt werden
            if (IsServer)
            {
                if (ACPSubscrService.GetSharedACObject(eventArgs.ACUrl) == null)
                    return false;
            }

            bool succeeded = false;

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                ACPDispatchProj subscrProject = GetProject(Project.ACIdentifier, false);
                if (subscrProject == null)
                {
                    subscrProject = new ACPDispatchProj(Project.ACIdentifier);
                    _ProjectDispatchList.Add(subscrProject);
                }
                succeeded = subscrProject.Enqueue(eventArgs);
            }
            return succeeded;
        }

        public void RemoveAllEvents()
        {

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                _ProjectDispatchList = new List<ACPDispatchProj>();
            }
        }

        internal ACPDispatchProj GetProject(string ProjectACIdentifier, bool withCS)
        {

            using (ACMonitor.Lock(_20055_LockProjectDispatchList))
            {
                return _ProjectDispatchList.Where(c => c.ACIdentifier == ProjectACIdentifier).FirstOrDefault();
            }
        }

#endregion

    }
}
