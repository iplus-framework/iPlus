// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Manager für Anwendungsobjekte
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Businessobject-Manager'}de{'Anwendungsobjekt-Manager'}", Global.ACKinds.TACBusinessobjects, Global.ACStorableTypes.Required, false, false)]
    public class Businessobjects : ACComponentManager, IBusinessobjects
    {
        public const string ClassName = "Businessobjects";

        #region c´tors
        public Businessobjects(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region IACBSO

        /// <summary>
        /// WPF-Control that register itself and the bounded object (in most cases a ACComponentProxy-Object) to this Reference-Point
        /// </summary>
        /// <param name="hashOfDepObj">Hashcode of the calling WPF-Control</param>
        /// <param name="boundedObject">IACObject which is bound via WPF-Binding to the WPF-Control</param>
        public void AddWPFRef(int hashOfDepObj, IACObject boundedObject)
        {
            if (this.ReferencePoint == null)
                return;
            this.ReferencePoint.AddWPFRef(hashOfDepObj, boundedObject);
        }


        /// <summary>WPF-Control that removes itself</summary>
        /// <param name="hashOfDepObj">Hashcode of the calling WPF-Control</param>
        /// <param name="searchInChilds"></param>
        /// <returns>true if WPF-Control was remove from ReferencePoint</returns>
        public bool RemoveWPFRef(int hashOfDepObj, bool searchInChilds = false)
        {
            if (this.ReferencePoint == null)
                return false;
            bool done = this.ReferencePoint.RemoveWPFRef(hashOfDepObj);
            if (!done && searchInChilds && InitState == ACInitState.Initialized)
            {
                foreach (var child in this.ACComponentChilds)
                {
                    IACBSO childBSO = child as IACBSO;
                    if (childBSO != null)
                    {
                        if (childBSO.RemoveWPFRef(hashOfDepObj, true))
                            return true;
                    }
                }
            }
            return done;
        }

        public virtual object Clone()
        {
            return null;
        }

        public bool FindAndRemoveWPFRef(int hashOfDepObj)
        {
            return RemoveWPFRef(hashOfDepObj, true);
        }

        /// <summary>
        /// Its invoked from a WPF-Itemscontrol that wants to refresh its CollectionView because the user has changed the LINQ-Expressiontree in the ACQueryDefinition-Property of IAccess. 
        /// The BSO should execute the query on the database first, to get the new results for refreshing the CollectionView of the control.
        /// If the bso don't want to handle this request or manipulate the ACQueryDefinition it returns false. The WPF-control invokes then the IAccess.NavSearch()-Method itself.  
        /// </summary>
        /// <param name="acAccess">Reference to IAccess that contains the changed query (Property NavACQueryDefinition)</param>
        /// <returns>True if the bso has handled this request and queried the database context. Otherwise it returns false.</returns>
        public bool ExecuteNavSearch(IAccess acAccess)
        {
            return false;
        }

        /// <summary>When the database context has changed, a dialog is opened that asks the user whether they want to save the changes. If yes then the OnSave()-Method will be invoked. If not then ACUndoChanges() will be invoked. If cancelled then nothing will happen.</summary>
        /// <returns>Fals, if user has cancelled saving or undoing.</returns>
        public bool ACSaveOrUndoChanges()
        {
            return ACSaveChanges();
        }

        #endregion


        ACClassConfig[] _ClassConfigCache = null;
        public string GetDerivedClassnameIfConfigured(string acClassName)
        {
            //object result = null;
            if (ACTypeFromLiveContext == null)
                return acClassName;
            ACClassConfig[] classConfigCache = null;

                using (ACMonitor.Lock(_20015_LockValue))
            {
                classConfigCache = _ClassConfigCache;
            }
            if (classConfigCache == null)
            {
                ACClassTaskQueue.TaskQueue.ProcessAction(() => 
                {
                    ACTypeFromLiveContext.ACClassConfig_ACClass.AutoLoad(ACTypeFromLiveContext.ACClassConfig_ACClassReference, ACTypeFromLiveContext);
                    classConfigCache = ACTypeFromLiveContext.ACClassConfig_ACClass.Where(c => c.KeyACUrl == null).ToArray();
                });
            }

                using (ACMonitor.Lock(_20015_LockValue))
            {
                if (_ClassConfigCache == null && classConfigCache != null)
                    _ClassConfigCache = classConfigCache;
            }

            if (classConfigCache == null || !classConfigCache.Any())
                return acClassName;
            var acClassConfig = classConfigCache.Where(c => c.LocalConfigACUrl == acClassName).FirstOrDefault();
            if (acClassConfig == null)
                return acClassName;

            string acClassNameDerived = acClassConfig[Const.Value] as string;
            if (!String.IsNullOrEmpty(acClassNameDerived))
                return acClassNameDerived;
            return acClassName;
        }

    }
}
