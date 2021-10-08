using gip.core.autocomponent;
using gip.core.datamodel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Objects;
using System.Linq;

namespace gip.core.reporthandler
{
    public abstract class PAOrderInfoManagerBase : PAClassAlarmingBase
    {
        public PAOrderInfoManagerBase(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
           : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public abstract bool IsResponsibleFor(PAOrderInfo orderInfo);
    }

    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPrintManager'}de{'ACPrintManager'}", Global.ACKinds.TPARole, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACPrintManager : PARole
    {

        #region const
        public const string Const_KeyACUrl_ConfiguredPrintersConfig = ".\\ACClassProperty(ConfiguredPrintersConfig)";
        public const string MN_Print = "Print";
        #endregion

        #region c'tors
        public ACPrintManager(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
           : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }
        public const string C_DefaultServiceACIdentifier = "ACPrintManager";


        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool baseACInit = base.ACInit(startChildMode);
            return baseACInit;
        }
        #endregion

        #region Attach / Deattach
        public static ACComponent GetServiceInstance(ACComponent requester)
        {
            return GetServiceInstance<ACComponent>(requester, C_DefaultServiceACIdentifier, CreationBehaviour.Default);
        }

        public static ACRef<ACComponent> ACRefToServiceInstance(ACComponent requester)
        {
            ACComponent serviceInstance = GetServiceInstance(requester) as ACComponent;
            if (serviceInstance != null)
                return new ACRef<ACComponent>(serviceInstance, requester);
            return null;
        }
        #endregion

        #region Properties


        #region Properties -> ConfiguredPrinters

        [ACPropertyPointConfig(9999, "", typeof(PrinterInfo), "en{'Configured printers'}de{'Konfigurierte Drucker'}")]
        public List<gip.core.datamodel.ACClassConfig> ConfiguredPrintersConfig
        {
            get
            {
                List<gip.core.datamodel.ACClassConfig> result = null;
                ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                {
                    try
                    {
                        ACTypeFromLiveContext.ACClassConfig_ACClass.Load(MergeOption.OverwriteChanges);
                        var query = ACTypeFromLiveContext.ACClassConfig_ACClass.Where(c => c.KeyACUrl == Const_KeyACUrl_ConfiguredPrintersConfig);
                        if (query.Any())
                            result = query.ToList();
                        else
                            result = new List<gip.core.datamodel.ACClassConfig>();
                    }
                    catch (Exception e)
                    {
                        Messages.LogException(this.GetACUrl(), "ConfiguredPrintersConfig", e.Message);
                    }
                });
                return result;
            }
        }

        //static List<gip.core.datamodel.ACClassConfig> ConfiguredPrintersConfig(context)

        #endregion

        #endregion

        #region Methods

        #region Methods -> Public

        [ACMethodInfo("Print", "en{'Reload Printer Config'}de{'Druckerkonfiguration nachladen'}", 201, false)]
        public void ReloadConfig()
        {
        }

        [ACMethodInfo("Print", "en{'Print on server'}de{'Auf Server drucken'}", 200, true)]
        public virtual Msg Print(PAOrderInfo pAOrderInfo, int copyCount)
        {
            Msg msg = null;
            try
            {
                PAOrderInfoManagerBase orderInfoManager = FindChildComponents<PAOrderInfoManagerBase>(c => c is PAOrderInfoManagerBase && (c as PAOrderInfoManagerBase).IsResponsibleFor(pAOrderInfo)).FirstOrDefault();
                //orderInfoManager.GetPrintingInfo(); // returns infor about BSO and Design that shoulnd be printed
                // TODO: think about PAShowDlgManagerVBBase

                //string bsoName = "";
                string designName = "";
                gip.core.datamodel.ACClass bsoACClass = null;

                // SASA TODO: More flexible and individual logic how businessobject is determined that should be use for printing
                //if (pAOrderInfo.Entities.Any(c => c.EntityName == FacilityCharge.ClassName))
                //{
                //    bsoACClass = this.Root.Database.ContextIPlus.GetACType("BSOFacilityBookCharge");
                //    bsoName = "\\Businessobjects#BSOFacilityBookCharge";
                //    designName = "LabelQR";
                //}

                PrinterInfo printerInfo = GetPrinterInfo(pAOrderInfo);
                if (printerInfo == null)
                    msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = "Print(113) fail! No mandatory printer found!" };

                if (String.IsNullOrEmpty(printerInfo.PrinterACUrl))
                {
                    ACBSO bso = null;
                    try
                    {
                        //ACBSO bso = this.Root.ACUrlCommand(bsoName) as ACBSO;
                        bso = StartComponent(bsoACClass, bsoACClass, new ACValueList()) as ACBSO;
                        if (bso == null)
                            msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = "Print(110) fail! No mandatory BSO found!" };
                        if (msg == null)
                        {
                            msg = bso.SetDataFromPAOrderInfo(pAOrderInfo);
                            if (msg == null)
                                msg = bso.PrintViaOrderInfo(designName, printerInfo.PrinterName, (short)copyCount);
                        }

                        if (msg != null)
                            Root.Messages.LogError(this.GetACUrl(), "Print(119)", msg.Message);
                    }
                    catch (Exception e)
                    {
                        // TODO: Alarm
                        Messages.LogException(this.GetACUrl(), "Print(10)", e);
                    }
                    finally
                    {
                        try
                        {
                            // BSO must be stopped!
                            if (bso != null)
                                bso.Stop();
                        }
                        catch (Exception e)
                        {
                            // TODO: Alarm
                            Messages.LogException(this.GetACUrl(), "Print(20)", e);
                        }
                    }
                }
                else
                {
                    IACComponent printServer = Root.ACUrlCommand(printerInfo.PrinterACUrl) as IACComponent;
                    if (printServer == null)
                    {
                        // TODO SASA:
                        //Messages.Error("Printserver {0} is not configured or you don't have access-rights");
                        return msg;
                    }
                    if (printServer.ConnectionState == ACObjectConnectionState.DisConnected)
                    {
                        // TODO SASA:
                        //Messages.Error("Printserver is not available via network");
                        return msg;
                    }
                    // TODO SASA: Which acClassDesign must be printed?
                    //printServer.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + ACPrintServerBase.MN_Print, bsoACClass.ACClassID, acClassDesign.GetACUrl(), pAOrderInfo, copies);
                }
            }
            catch (Exception ex)
            {
                msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = "Print(120) fail! Error: " + ex.Message };
                Root.Messages.LogException(ACPrintManager.C_DefaultServiceACIdentifier, "Print(125)", ex);
            }
            return msg;
        }

        public virtual PrinterInfo GetPrinterInfo(PAOrderInfo pAOrderInfo)
        {
            PrinterInfo printerInfo = null;
            // TODO Sasa: Delegate Logic to another child component
            //using (DatabaseApp databaseApp = new DatabaseApp())
            //{
            //    if (pAOrderInfo.Entities.Any(c => c.EntityName == FacilityCharge.ClassName))
            //    {
            //        PAOrderInfoEntry entry = pAOrderInfo.Entities.FirstOrDefault(c => c.EntityName == FacilityCharge.ClassName);
            //        FacilityCharge facilityCharge = databaseApp.FacilityCharge.FirstOrDefault(c => c.FacilityChargeID == entry.EntityID);
            //        printerInfo = GetPrinterInfo(facilityCharge.Facility);
            //    }
            //}
            return printerInfo;
        }

        public List<PrinterInfo> GetPrintServers()
        {
            gip.core.datamodel.ACClass basePrintServerClass = gip.core.datamodel.Database.GlobalDatabase.GetACType(typeof(ACPrintServerBase));
            // TODO Sasa: Childcomponent taht do this work
            //IQueryable<gip.core.datamodel.ACClass> queryClasses = FacilityManager.s_cQry_GetAvailableModulesAsACClass(Database.ContextIPlus, basePrintServerClass.ACIdentifier);
            List<PrinterInfo> printServers = new List<PrinterInfo>();
            //if (queryClasses != null && queryClasses.Any())
            //{
            //    gip.core.datamodel.ACClass[] acClasses = queryClasses.ToArray();
            //    foreach (gip.core.datamodel.ACClass aCClass in acClasses)
            //    {
            //        PrinterInfo printerInfo = new PrinterInfo();
            //        printerInfo.Name = aCClass.ACIdentifier;
            //        printerInfo.PrinterACUrl = ACItem.FactoryACUrlComponent(aCClass);
            //        printServers.Add(printerInfo);
            //    }
            //}
            return printServers;
        }

        #endregion

        #region Methods -> Private

        //private PrinterInfo GetPrinterInfo(Facility facility)
        //{
        //    PrinterInfo printerInfo = ConfiguredPrintersConfig.Select(c => c.Value as PrinterInfo).FirstOrDefault(c => c.FacilityNo == facility.FacilityNo);
        //    if (printerInfo == null && facility.Facility1_ParentFacility != null)
        //        printerInfo = GetPrinterInfo(facility.Facility1_ParentFacility);
        //    return printerInfo;
        //}

        #endregion

        #endregion
    }
}
