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
// <copyright file="ACObjectItemWCheckBox.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACObjectItemWCheckBox
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACObjectItemWCheckBox'}de{'ACObjectItemWCheckBox'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    // 1 ACCaption
    // 2 IsChecked
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACObjectItemWCheckBox", "en{'ACObjectItemWCheckBox'}de{'ACObjectItemWCheckBox'}", typeof(ACObjectItemWCheckBox), "ACObjectItemWCheckBox", Const.ACCaptionPrefix, Const.ACCaptionPrefix)]
    public class ACObjectItemWCheckBox : ACObjectItem, IVBDataCheckbox
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACObjectItemWCheckBox"/> class.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="isChecked">if set to <c>true</c> [is checked].</param>
        public ACObjectItemWCheckBox(string caption, bool isChecked = true) : 
            base(caption)
        {
            DataContentCheckBox = "IsChecked";
            IsEnabled = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACObjectItemWCheckBox"/> class.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="isChecked">if set to <c>true</c> [is checked].</param>
        /// <param name="acURLRelative">The ac URL relative.</param>
        public ACObjectItemWCheckBox(IACObject acObject, string caption, bool isChecked = true, string acURLRelative = "") :
            base(acObject, caption, acURLRelative)
        {
            IsEnabled = true;
        }
        #endregion

        #region IVBDataCheckbox Member
        /// <summary>
        /// Gets or sets the data content check box.
        /// </summary>
        /// <value>The data content check box.</value>
        public string DataContentCheckBox
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(2, "", "en{'Selected'}de{'Ausgewählt'}")]
        public virtual bool IsChecked
        {
            get;
            set;
        }

        public bool IsEnabled { get; set; }

        #endregion

    }
}
