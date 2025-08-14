// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ClassRightManager.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ClassRightManager
    /// </summary>
    public class ClassRightManager : ConcurrentDictionary<Guid, Global.ControlModes>
    {
        /// <summary>
        /// The _ is superuser
        /// </summary>
        bool _IsSuperuser = false;
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassRightManager"/> class.
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <param name="User">The user.</param>
        public ClassRightManager(ACClass acClass, VBUser User)
        {
            if (!User.IsSuperuser)
            {
                List<VBGroupRight> vbGroupRightList = new List<VBGroupRight>();

                using (ACMonitor.Lock(acClass.Database.QueryLock_1X000))
                {
                    foreach (VBUserGroup vbUserGroup in User.VBUserGroup_VBUser)
                    {
                        var query = acClass.VBGroupRight_ACClass.Where(c => c.VBGroupID == vbUserGroup.VBGroupID);
                        foreach (VBGroupRight vbGroupRight in query)
                        {
                            vbGroupRightList.Add(vbGroupRight);
                        }
                    }
                }
                var query1 = vbGroupRightList.Where(c => c.ACClassDesignID == null && c.ACClassMethodID == null && c.ACClassPropertyID == null);
                if (!query1.Any())
                {
                    this[acClass.ACTypeID] = acClass.IsRightmanagement ? Global.ControlModes.Hidden : Global.ControlModes.Enabled;
                }
                else
                {
                    this[acClass.ACTypeID] = query1.Max(c => c.ControlMode);
                }

                var queryProperty = vbGroupRightList.Where(c => c.ACClassPropertyID != null).GroupBy(c => c.ACClassPropertyID);
                foreach (IGrouping<Guid?, VBGroupRight> item in queryProperty)
                {
                    this[item.Key.Value] = item.Max(c => c.ControlMode);
                }

                var queryMethod = vbGroupRightList.Where(c => c.ACClassMethodID != null).GroupBy(c => c.ACClassMethodID);
                foreach (IGrouping<Guid?, VBGroupRight> item in queryMethod)
                {
                    this[item.Key.Value] = item.Max(c => c.ControlMode);
                }

                var queryDesign = vbGroupRightList.Where(c => c.ACClassDesignID != null).GroupBy(c => c.ACClassDesignID);
                foreach (IGrouping<Guid?, VBGroupRight> item in queryDesign)
                {
                    this[item.Key.Value] = item.Max(c => c.ControlMode);
                }
            }
            else
            {
                _IsSuperuser = true;
            }
        }

        /// <summary>
        /// Gets or sets the AC class ID.
        /// </summary>
        /// <value>The AC class ID.</value>
        Guid ACClassID { get; set; }

        /// <summary>
        /// Gets the control mode.
        /// </summary>
        /// <param name="rightItem">The right item.</param>
        /// <returns>Global.ControlModes.</returns>
        public Global.ControlModes GetControlMode(IACType rightItem)
        {
            if (_IsSuperuser || rightItem == null || !rightItem.IsRightmanagement)
                return Global.ControlModes.Enabled;
            Global.ControlModes controlMode;
            if (TryGetValue(rightItem.ACTypeID, out controlMode))
                return controlMode;
            return rightItem.IsRightmanagement ? Global.ControlModes.Hidden : Global.ControlModes.Enabled;
        }
    }
}
