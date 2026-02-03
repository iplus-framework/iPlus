// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OCR Cam'}de{'OCR Kamera'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAECameraOCR : PAEScanner
    {
        #region Constructors

        static PAECameraOCR()
        {
            RegisterExecuteHandler(typeof(PAEScanner), HandleExecuteACMethod_PAECameraOCR);
        }

        public PAECameraOCR(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }


        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        { 
            return await base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Public

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            //switch (acMethodName)
            //{

            //}
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

#endregion
       

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAECameraOCR(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEScanner(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }

}
