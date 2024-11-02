// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-27-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-27-2012
// ***********************************************************************
// <copyright file="BSOProgramWizard.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.autocomponent;
using gip.core.datamodel;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSOProgramWizard
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Programwizard'}de{'Programmwizard'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class BSOProgramWizard : ACBSOWizard
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="BSOProgramWizard"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOProgramWizard(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        /// <summary>
        /// ACs the init.
        /// </summary>
        /// <param name="startChildMode">The start child mode.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._WizardDesigns = null;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region BSO->ACProperty
        /// <summary>
        /// The _ wizard designs
        /// </summary>
        List<ACClassDesign> _WizardDesigns = null;
        /// <summary>
        /// Gets the wizard design list.
        /// </summary>
        /// <value>The wizard design list.</value>
        public override List<ACClassDesign> WizardDesignList
        {
            get
            {
                return _WizardDesigns;
            }
        }
        #endregion

        #region BSO->ACMethod
        /// <summary>
        /// Aufrufen des Wizzards
        /// </summary>
        /// <param name="acClassPWMethod">Null=Manuelle Auswahl</param>
        public void ShowProgramWizard(ACClass acClassPWMethod)
        {
            _WizardDesigns = new List<ACClassDesign>();
            _WizardDesigns.Add(this.GetDesign("Programtype"));
            _WizardDesigns.Add(this.GetDesign("Programdata"));

            ShowWizardDlg();
        }

        /// <summary>
        /// Checks the next page.
        /// </summary>
        /// <param name="isFinish">if set to <c>true</c> [is finish].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool CheckNextPage(bool isFinish)
        {
            return true;
        }

        /// <summary>
        /// Finishes this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool Finish()
        {
            return false;
        }
        #endregion
    }
}
