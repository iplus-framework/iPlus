// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACComponentPool.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACComponentPool
    /// </summary>
    public class ACComponentPool 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACComponentPool"/> class.
        /// </summary>
        public ACComponentPool()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether [pooling off].
        /// </summary>
        /// <value><c>true</c> if [pooling off]; otherwise, <c>false</c>.</value>
        internal bool PoolingOff
        {
            get;
            set;
        }

        /// <summary>
        /// Clears the pool.
        /// </summary>
        internal void ClearPool()
        {

            using (ACMonitor.Lock(_10015_LockPool))
            {
                _Pool = new Dictionary<Guid, Queue<PoolReference>>();
            }
        }

        /// <summary>
        /// Pops the specified ac class.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns>IACComponent.</returns>
        internal IACComponent Pop(ACClass acClass)
        {
            if (acClass == null)
                return null;

            using (ACMonitor.Lock(_10015_LockPool))
            {
                Queue<PoolReference> poolList;
#if !EFCR
                if (_Pool.TryGetValue(acClass.ACClassID, out poolList))
                {
                    if (poolList.Count <= 0)
                        return null;
                    while (poolList.Count > 0)
                    {
                        PoolReference weakRef = poolList.Peek();
                        if (!weakRef.IsAgedAndCanBeUsed)
                            return null;
                        weakRef = poolList.Dequeue();
                        IACComponent component = null;
                        if (weakRef.TryGetTarget(out component))
                            return component;
                    }
                }
#endif
            }
            return null;
        }

        /// <summary>
        /// CanBePooled
        /// </summary>
        /// <param name="component"></param>
        /// <param name="ignoreInitState"></param>
        /// <returns></returns>
        internal bool CanBePooled(IACComponent component, bool ignoreInitState = false)
        {
            if (component == null || component is IRoot || PoolingOff || !component.IsPoolable || (!ignoreInitState && component.InitState != ACInitState.Initialized))
                return false;
            ACClass acClass = component.ComponentClass;
            if (acClass == null)
                return false;
            return true;
        }

        /// <summary>
        /// Pushes the specified component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="ignoreInitState">The ignoreInitState.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
#if !EFCR
        internal bool Push(IACComponent component, bool ignoreInitState = false)
        {
            if (!CanBePooled(component, ignoreInitState))
                return false;
            ACClass acClass = component.ComponentClass;

            using (ACMonitor.Lock(_10015_LockPool))
            {
                Queue<PoolReference> poolList;
                if (!_Pool.TryGetValue(acClass.ACClassID, out poolList))
                {
                    poolList = new Queue<PoolReference>();
                    _Pool.Add(acClass.ACClassID, poolList);
                }
                poolList.Enqueue(new PoolReference(component));
                return true;
            }
        }
#endif

        /// <summary>
        /// The _ pool
        /// </summary>
        private readonly ACMonitorObject _10015_LockPool = new ACMonitorObject(10015);
        private Dictionary<Guid, Queue<PoolReference>> _Pool = new Dictionary<Guid, Queue<PoolReference>>();
    }

    internal class PoolReference
    {
        internal PoolReference(IACComponent component)
        {
            _Time = DateTime.Now;
            if (UseWeekRef || component.IsProxy)
                _WeekReference = new WeakReference<IACComponent>(component);
            else
                _StrongReference = component;
        }

        WeakReference<IACComponent> _WeekReference = null;
        IACComponent _StrongReference = null;
        private DateTime _Time;
        private bool _IsAged = false;
        public bool IsAgedAndCanBeUsed
        {
            get
            {
                if (_IsAged)
                    return _IsAged;
                _IsAged = (DateTime.Now - _Time).TotalSeconds >= MinAgingSeconds;
                return _IsAged;
            }
        }

        // Length of MinAgingSeconds is 5 seconds, because the WorkCycleThreads could take longer than some seconds to finsh their code.
        private const int MinAgingSeconds = 5;

        private static bool? _UseWeekRef;
        public static bool UseWeekRef
        {
            get
            {
                if (!_UseWeekRef.HasValue)
                    return false;
                return _UseWeekRef.Value;
            }
            set
            {
                if (_UseWeekRef.HasValue)
                    return;
                _UseWeekRef = value;
            }
        }


        internal bool TryGetTarget(out IACComponent target)
        {
            if (_WeekReference != null)
                return _WeekReference.TryGetTarget(out target);
            else
            {
                target = _StrongReference;
                return true;
            }
        }
    }
}
