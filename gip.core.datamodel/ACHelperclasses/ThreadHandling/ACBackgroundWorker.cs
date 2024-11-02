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
// <copyright file="ACBackgroundWorker.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Class ACBackgroundWorker
    /// </summary>
    public class ACBackgroundWorker : BackgroundWorker
    {

        /// <summary>
        /// Gets or sets the event args.
        /// </summary>
        /// <value>The event args.</value>
        public DoWorkEventArgs EventArgs
        {
            get;
            set;
        }

        #region ProgressInfo

        /// <summary>
        /// The _ progress info
        /// </summary>
        private ProgressInfo _ProgressInfo = null;
        /// <summary>
        /// Gets or sets the progress info.
        /// </summary>
        /// <value>The progress info.</value>
        public ProgressInfo ProgressInfo
        {
            get
            {
                if (_ProgressInfo == null)
                    _ProgressInfo = new ProgressInfo(this);
                return _ProgressInfo;
            }
        }

        /// <summary>
        /// Gets the progress info.
        /// </summary>
        /// <param name="e">The <see cref="ProgressChangedEventArgs"/> instance containing the event data.</param>
        /// <returns>ProgressInfo.</returns>
        public static ProgressInfo GetProgressInfo(ProgressChangedEventArgs e)
        {
            if (e == null || e.UserState == null) return null;
            if (e.UserState.GetType() != typeof(ProgressInfo)) return null;
            return e.UserState as ProgressInfo;
        }

        #endregion


        
    }
}