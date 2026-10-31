// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.bso.iplus
{
    /// <summary>
    /// Partial class of BSOiPlusStudio: starts the OPC UA Browser (BSOOPCUABrowser in
    /// gip.core.communication) for the currently selected subscription class. The browser
    /// BSO is resolved by name (TACBSOGlobal), so no compile time reference to
    /// gip.core.communication is needed (gip.core.communication references gip.bso.iplus).
    /// </summary>
    public partial class BSOiPlusStudio
    {
        /// <summary>
        /// Opens the OPC UA Browser for the currently selected class. Enabled if the class
        /// has a configuration point of type OPCItemConfig (i.e. it is an OPC subscription).
        /// </summary>
        [ACMethodInfo("ConfigACClassProperty", "en{'OPC UA Browser'}de{'OPC UA Browser'}", 499, true)]
        public void StartOPCUABrowser()
        {
            if (!IsEnabledStartOPCUABrowser())
                return;

            ACMethod acMethod = gip.core.datamodel.Database.Root.ACType.ACType.ACUrlACTypeSignature(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + BSOOPCUABrowserName);
            if (acMethod == null)
                return;
            acMethod.ParameterValueList["AutoLoad"] = CurrentACClass.GetACUrl();
            this.Root.RootPageWPF.ACUrlCommand(Const.CmdStartBusinessobject, Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + BSOOPCUABrowserName, acMethod.ParameterValueList);
        }

        public bool IsEnabledStartOPCUABrowser()
        {
            if (CurrentACClass == null)
                return false;
            // The class must have a configuration point of type OPCItemConfig
            // (defined on OPCClientACSubscr, inherited by OPCUAClientACSubscr instances)
            return CurrentACClass.Properties.Any(c =>
                c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConfigPointProperty
                && c.ConfigACClass != null
                && c.ConfigACClass.ACIdentifier == "OPCItemConfig");
        }

        public const string BSOOPCUABrowserName = "BSOOPCUABrowser";
    }
}
