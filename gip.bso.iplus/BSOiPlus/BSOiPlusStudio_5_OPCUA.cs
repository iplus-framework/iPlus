// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using System.Threading.Tasks;
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
        /// Opens the OPC UA Browser for the currently selected class as a modal dialog. Enabled if the class
        /// has a configuration point of type OPCItemConfig (i.e. it is an OPC subscription).
        /// The browser BSO is started below Root.Businessobjects (pattern of PAShowDlgManagerVBBase.ShowDialogOrder)
        /// and the dialog is opened through the child BSO itself (childBSO.ShowDialogAsync with the design
        /// "BrowserDialog" of BSOOPCUABrowser), so that "!CloseTopDialog" inside the dialog finds the
        /// correct VBDockingManager (same DialogStack-broadcast chain).
        /// </summary>
        [ACMethodInfo("ConfigACClassProperty", "en{'OPC UA Browser'}de{'OPC UA Browser'}", 499, true)]
        public async Task StartOPCUABrowser()
        {
            if (!IsEnabledStartOPCUABrowser())
                return;

            ACMethod acMethod = gip.core.datamodel.Database.Root.ACType.ACType.ACUrlACTypeSignature(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + BSOOPCUABrowserName);
            if (acMethod == null)
                return;
            acMethod.ParameterValueList["AutoLoad"] = CurrentACClass.GetACUrl();

            // Start the global BSO below Root.Businessobjects (StartComponent converts a single
            // ACValueList element into the constructor parameters of ACClassConstructorInfo)
            ACComponent childBSO = this.Root.Businessobjects.StartComponent(BSOOPCUABrowserName, null, new object[] { acMethod.ParameterValueList }) as ACComponent;
            if (childBSO == null)
                return;

            // Open the dialog through the child BSO itself, NOT through this BSO (pattern of
            // BSOFacilityLotOverview.ShowDialogOrder): ShowDialogAsync resolves the VBDockingManager
            // via the child's own GUI-broadcast chain, so the dialog is registered in the DialogStack
            // of a manager that CloseTopDialog (invoked on the child BSO by the "!CloseTopDialog"
            // button of the BrowserDialog design) finds again on the same chain.
            await childBSO.ShowDialogAsync(childBSO, "BrowserDialog");
            await childBSO.Stop();
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
