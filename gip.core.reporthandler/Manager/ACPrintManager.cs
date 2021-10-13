using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;

namespace gip.core.reporthandler
{

    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPrintManager'}de{'ACPrintManager'}", Global.ACKinds.TPARole, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACPrintManager : PARole
    {

        #region const
        public const string Const_KeyACUrl_ConfiguredPrintersConfig = ".\\ACClassProperty(ConfiguredPrintersConfig)";
        public const string MN_Print = "Print";
        #endregion

        #region c'tors
        public ACPrintManager(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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

        [ACPropertyBindingSource(305, "Error", "en{'Print Manager Alarm'}de{'Druckmanager-Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> PrintManagerAlarm { get; set; }


        #region Properties -> ConfiguredPrinters

        [ACPropertyPointConfig(9999, "", typeof(PrinterInfo), "en{'Configured printers'}de{'Konfigurierte Drucker'}")]
        public List<ACClassConfig> ConfiguredPrintersConfig
        {
            get
            {
                List<ACClassConfig> result = null;
                ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                {
                    try
                    {
                        ACTypeFromLiveContext.ACClassConfig_ACClass.Load(MergeOption.OverwriteChanges);
                        var query = ACTypeFromLiveContext.ACClassConfig_ACClass.Where(c => c.KeyACUrl == Const_KeyACUrl_ConfiguredPrintersConfig);
                        if (query.Any())
                            result = query.ToList();
                        else
                            result = new List<ACClassConfig>();
                    }
                    catch (Exception e)
                    {
                        Messages.LogException(this.GetACUrl(), "ConfiguredPrintersConfig", e.Message);
                    }
                });
                return result;
            }
        }

        //static List<ACClassConfig> ConfiguredPrintersConfig(context)

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
                if (orderInfoManager == null)
                {
                    msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = "Print(108) PAOrderInfoManager is null !" };
                    Messages.LogMessageMsg(msg);
                    return msg;
                }

                PAOrderInfoDestination pAOrderInfoDestination = orderInfoManager.GetOrderInfoDestination(pAOrderInfo);
                if (pAOrderInfoDestination == null)
                {
                    msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = "Print(113) PAOrderInfoDestination is null !" };
                    Messages.LogMessageMsg(msg);
                    return msg;
                }

                PrinterInfo printerInfo = orderInfoManager.GetPrinterInfo(pAOrderInfo);
                if (printerInfo == null)
                {
                    msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = "Print(124) fail! No mandatory printer found!" };
                    Messages.LogMessageMsg(msg);
                    return msg;
                }

                // first fetch a BSO
                ACBSO bso = null;
                // TODO: @aagincic place for implement BSO Pool
                bso = this.Root.ACUrlCommand(pAOrderInfoDestination.BSOACUrl) as ACBSO;
                //bso = StartComponent(bsoACClass, bsoACClass, new ACValueList()) as ACBSO;
                if (bso == null)
                {
                    msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = "Print(139) fail! No mandatory BSO found!" };
                    Messages.LogMessageMsg(msg);
                    return msg;
                }

                if (String.IsNullOrEmpty(printerInfo.PrinterACUrl))
                {
                    try
                    {
                        if (msg == null)
                        {
                            msg = bso.SetOrderInfo(pAOrderInfo);
                            if (msg == null)
                            {
                                msg = bso.PrintViaOrderInfo(pAOrderInfoDestination.ReportACIdentifier, printerInfo.PrinterName, (short)copyCount);
                                Messages.LogMessageMsg(msg);
                                return msg;
                            }
                        }

                        if (msg != null)
                            Root.Messages.LogError(this.GetACUrl(), "Print(119)", msg.Message);
                    }
                    catch (Exception e)
                    {
                        msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = "Print(162) Error by printing: " + e.Message };
                        PrintManagerAlarm.ValueT = PANotifyState.AlarmOrFault;
                        if (IsAlarmActive(PrintManagerAlarm, e.Message) == null)
                            Messages.LogException(this.GetACUrl(), "Print(166)", e);
                        OnNewAlarmOccurred(PrintManagerAlarm, msg, true);
                    }
                    finally
                    {
                        try
                        {
                            if (bso != null)
                                bso.Stop();
                        }
                        catch (Exception e)
                        {
                            msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = "Print(177) Error by stopping BSO: " + e.Message };
                            PrintManagerAlarm.ValueT = PANotifyState.AlarmOrFault;
                            if (IsAlarmActive(PrintManagerAlarm, e.Message) == null)
                                Messages.LogException(this.GetACUrl(), "Print(181)", e);
                            OnNewAlarmOccurred(PrintManagerAlarm, msg, true);
                        }
                    }
                }
                else
                {
                    IACComponent printServer = Root.ACUrlCommand(printerInfo.PrinterACUrl) as IACComponent;
                    if (printServer == null)
                    {
                        msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = String.Format("Print(190) Printserver {0} is not configured or you don't have access-rights!", GetACUrl()) };
                        Messages.LogMessageMsg(msg);
                        return msg;
                    }
                    if (printServer.ConnectionState == ACObjectConnectionState.DisConnected)
                    {
                        msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = String.Format("Print(190) Printserver {0} is disconnected!", GetACUrl()) };
                        Messages.LogMessageMsg(msg);
                        return msg;
                    }

                    printServer.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + ACPrintServerBase.MN_Print, bso.ACType.ValueTypeACClass.ACClassID, pAOrderInfoDestination.ReportACIdentifier, pAOrderInfo, copyCount);
                }
            }
            catch (Exception ex)
            {
                msg = new Msg() { MessageLevel = eMsgLevel.Error, Message = "Print(120) fail! Error: " + ex.Message };
                Root.Messages.LogException(ACPrintManager.C_DefaultServiceACIdentifier, "Print(125)", ex);
            }
            return msg;
        }

        public static List<PrinterInfo> GetPrintServers(Database database)
        {
            ACClass basePrintServerClass = database.GetACType(typeof(ACPrintServerBase));
            IQueryable<ACClass> queryClasses = ACClassManager.s_cQry_GetAvailableModulesAsACClass(database, basePrintServerClass.ACIdentifier);
            List<PrinterInfo> printServers = new List<PrinterInfo>();
            if (queryClasses != null && queryClasses.Any())
            {
                ACClass[] acClasses = queryClasses.ToArray();
                foreach (ACClass aCClass in acClasses)
                {
                    PrinterInfo printerInfo = new PrinterInfo();
                    printerInfo.Name = aCClass.ACIdentifier;
                    printerInfo.PrinterACUrl = ACItem.FactoryACUrlComponent(aCClass);
                    printServers.Add(printerInfo);
                }
            }
            return printServers;
        }

        public static List<PrinterInfo> GetConfiguredPrinters(Database database, Guid acClassID, bool onlyMachines)
        {

            List<ACClassConfig> configs =
                database
                .ACClass
                .Where(c => c.ACClassID == acClassID)
                .SelectMany(c => c.ACClassConfig_ACClass)
                .Where(c => c.KeyACUrl == Const_KeyACUrl_ConfiguredPrintersConfig)
                .ToList();
            List<PrinterInfo> printerInfos = new List<PrinterInfo>();
            foreach (ACClassConfig cClassConfig in configs)
            {
                PrinterInfo printerInfo = cClassConfig.Value as PrinterInfo;
                printerInfo.ACClassConfigID = cClassConfig.ACClassConfigID;
                if (!onlyMachines || printerInfo.FacilityID == Guid.Empty)
                    printerInfos.Add(printerInfo);
            }
            return printerInfos;
        }

        #endregion



        #endregion
    }
}
