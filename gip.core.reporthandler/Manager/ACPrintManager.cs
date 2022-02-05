using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Drawing.Printing;
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
        public const string C_ClassName = "ACPrintManager";


        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool baseACInit = base.ACInit(startChildMode);
            return baseACInit;
        }
        #endregion

        #region Attach / Deattach
        public static ACPrintManager GetServiceInstance(ACComponent requester)
        {
            return GetServiceInstance<ACPrintManager>(requester, C_DefaultServiceACIdentifier, CreationBehaviour.OnlyLocal);
        }

        public static ACRef<ACPrintManager> ACRefToServiceInstance(ACComponent requester)
        {
            ACPrintManager serviceInstance = GetServiceInstance(requester);
            if (serviceInstance != null)
                return new ACRef<ACPrintManager>(serviceInstance, requester);
            return null;
        }
        #endregion

        #region Properties


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
        public virtual Msg Print(PAOrderInfo pAOrderInfo, int copyCount, string vbUserName = null, int maxPrintJobsInSpooler = 0)
        {
            Msg msg = null;
            try
            {
                PAPrintInfo printInfo = GetPrintingInfo(pAOrderInfo, vbUserName);
                if (printInfo == null)
                {
                    // Error50488: Can't print because no printer was configured for printing. Open the businessobject for printer settings and assign a printer!
                    msg = new Msg(this, eMsgLevel.Error, C_ClassName, "Print", 1010, "Error50488");
                    return msg;
                }


                if (String.IsNullOrEmpty(printInfo.PrinterInfo.PrinterACUrl))
                {
                    // first fetch a BSO
                    ACBSO bso = null;
                    // TODO: @aagincic place for implement BSO Pool
                    try
                    {
                        bso = this.Root.ACUrlCommand(printInfo.BSOACUrl) as ACBSO;
                        if (bso == null)
                        {
                            // Error50489: Can't start Businessobject {0}.
                            msg = new Msg(this, eMsgLevel.Error, C_ClassName, "Print", 1020, "Error50489", printInfo.BSOACUrl);
                            return msg;
                        }

                        msg = bso.PrintByOrderInfo(pAOrderInfo, printInfo.PrinterInfo.PrinterName, (short)copyCount, printInfo.ReportACIdentifier, maxPrintJobsInSpooler);
                        if (msg != null)
                            return msg;
                        else
                        {
                            // Info50078 Successfully printed on {0}.
                            return new Msg(this, eMsgLevel.Info, C_ClassName, "Print", 1020, "Info50078", printInfo.PrinterInfo.PrinterName);
                        }
                    }
                    catch (Exception e)
                    {
                        msg = new Msg(e.Message, this, eMsgLevel.Exception, C_ClassName, "Print", 1040);
                        Messages.LogException(this.GetACUrl(), msg.ACIdentifier, e);
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
                            msg = new Msg(e.Message, this, eMsgLevel.Exception, C_ClassName, "Print", 1050);
                            Messages.LogException(this.GetACUrl(), msg.ACIdentifier, e);
                        }
                    }
                }
                else
                {
                    IACComponent printServer = Root.ACUrlCommand(printInfo.PrinterInfo.PrinterACUrl) as IACComponent;
                    if (printServer == null)
                    {
                        // Error50490, Printserver {0} is not configured.
                        msg = new Msg(this, eMsgLevel.Error, C_ClassName, "Print", 1060, "Error50490", printInfo.PrinterInfo.PrinterACUrl);
                        return msg;
                    }
                    if (printServer.ConnectionState == ACObjectConnectionState.DisConnected)
                    {
                        // Error50491, Printserver {0} is disconnected.
                        msg = new Msg(this, eMsgLevel.Error, C_ClassName, "Print", 1061, "Error50491", printInfo.PrinterInfo.PrinterACUrl);
                        return msg;
                    }

                    printServer.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + ACPrintServerBase.MN_PrintByACUrl, printInfo.BSOACUrl, printInfo.ReportACIdentifier, pAOrderInfo, copyCount);
                }
            }
            catch (Exception ex)
            {
                msg = new Msg(ex.Message, this, eMsgLevel.Exception, C_ClassName, "Print", 1100);
                Messages.LogException(this.GetACUrl(), msg.ACIdentifier, ex);
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

        public static List<PrinterInfo> GetWindowsPrinters()
        {
            System.Drawing.Printing.PrinterSettings.StringCollection windowsPrinters = PrinterSettings.InstalledPrinters;
            List<string> windowsPrintersList = new List<string>();
            foreach (string printer in windowsPrinters)
            {
                windowsPrintersList.Add(printer);
            }
            return
                windowsPrintersList
                .Select(c => new PrinterInfo() { PrinterName = c, Name = c })
                .ToList();
        }

        public Msg AssignPrinter(Database db, PrinterInfo printerToAssign)
        {
            if (db == null || printerToAssign == null)
                return new Msg(eMsgLevel.Error, "db and/or printerToAssign are/is null.");

            ACClass printManagerClass = ComponentClass.FromIPlusContext<ACClass>(db);
            ACClass propertyInfoType = db.ACClass.FirstOrDefault(c => c.ACIdentifier == "PrinterInfo");
            if (propertyInfoType == null)
            {
                return new Msg(eMsgLevel.Error, "PrinterInfo property type is null.");
            }

            ACClassConfig aCClassConfig = ACClassConfig.NewACObject(db, printManagerClass);
            aCClassConfig.ValueTypeACClassID = propertyInfoType.ACClassID;

            printerToAssign.ACClassConfigID = aCClassConfig.ACClassConfigID;

            aCClassConfig.KeyACUrl = ACPrintManager.Const_KeyACUrl_ConfiguredPrintersConfig;
            aCClassConfig.Value = printerToAssign;
            db.ACClassConfig.AddObject(aCClassConfig);
            return db.ACSaveChanges();
        }

        public Msg UnAssignPrinter(Database db, PrinterInfo printerToUnAssign)
        {
            if (db == null || printerToUnAssign == null)
            {
                return new Msg(eMsgLevel.Error, "db and/or printerToUnAssign are/is null!");
            }

            ACClassConfig aCClassConfig = db.ACClassConfig.FirstOrDefault(c => c.ACClassConfigID == printerToUnAssign.ACClassConfigID);
            if (aCClassConfig == null)
            {
                return new Msg(eMsgLevel.Error, "acClassConfig is null!");
            }

            Msg msg = aCClassConfig.DeleteACObject(db, false);
            if (msg == null)
            {
                msg = db.ACSaveChanges();
            }
            return msg;
        }

        #region Determine Printer

        public virtual PAPrintInfo GetPrintingInfo(PAOrderInfo pAOrderInfo, string vbUserName = null)
        {
            PrinterInfo printerInfo = OnGetPrinterInfo(pAOrderInfo, vbUserName);
            if (printerInfo == null)
                return null;
            ACClass bsoClass = OnResolveBSOForOrderInfo(pAOrderInfo);
            if (bsoClass == null)
            {
                // TODO error
                return null;
            }
            string acUrl = Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + bsoClass.ACIdentifier;
            return new PAPrintInfo(acUrl, printerInfo, "");
        }

        protected virtual PrinterInfo OnGetPrinterInfo(PAOrderInfo pAOrderInfo, string vbUserName = null)
        {
            Guid? aCClassID = null;
            if (pAOrderInfo.Entities.Any(c => c.EntityName == gip.core.datamodel.ACClass.ClassName))
                aCClassID = pAOrderInfo.Entities.Where(c => c.EntityName == gip.core.datamodel.ACClass.ClassName).Select(c => c.EntityID).FirstOrDefault();

            gip.core.datamodel.ACClass aCClass = null;
            List<PrinterInfo> configuredPrinters = null;
            using (Database database = new core.datamodel.Database())
            {
                configuredPrinters = ACPrintManager.GetConfiguredPrinters(database, ComponentClass.ACClassID, false);

                if (!string.IsNullOrEmpty(vbUserName))
                {
                    VBUser vbUser = database.VBUser.FirstOrDefault(c => c.VBUserName == vbUserName);
                    if (vbUser != null)
                    {
                        PrinterInfo printerForUser = configuredPrinters.FirstOrDefault(c => c.VBUserID == vbUser.VBUserID);
                        if (printerForUser != null)
                            return printerForUser;
                    }
                }

                if (aCClassID != null)
                    aCClass = database.ACClass.FirstOrDefault(c => c.ACClassID == aCClassID);
            }
            if (configuredPrinters == null || !configuredPrinters.Any())
                return null;

            return GetPrinterInfoFromMachine(aCClass, configuredPrinters, true);
        }

        protected virtual ACClass OnResolveBSOForOrderInfo(PAOrderInfo pAOrderInfo)
        {
            if (pAOrderInfo == null)
                return null;
            PAOrderInfoEntry entry = pAOrderInfo.Entities.FirstOrDefault();
            if (entry == null)
                return null;
            ACClass entityClass = entry.EntityACType;
            if (entityClass == null)
                return entityClass;
            return entityClass.ManagingBSO;
        }

        protected PrinterInfo GetPrinterInfoFromMachine(gip.core.datamodel.ACClass acClass, List<PrinterInfo> configuredPrinters, bool useFirstOrDefaultIfNotFoundForClass)
        {
            PrinterInfo printerInfo = null;
            if (configuredPrinters == null || !configuredPrinters.Any())
                return printerInfo;
            if (acClass != null)
                printerInfo = configuredPrinters.FirstOrDefault(c => c.MachineACUrl == acClass.ACURLComponentCached);
            if (printerInfo == null && useFirstOrDefaultIfNotFoundForClass)
                printerInfo = configuredPrinters.FirstOrDefault();
            return printerInfo;
        }


        #endregion

        #endregion

        #region Execute-Helper
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(Print):
                    result = Print(acParameter[0] as PAOrderInfo, 
                                    (int)acParameter[1], 
                                    acParameter.Count() > 2 ? acParameter[2] as string : null, 
                                    acParameter.Count() > 3 ? (int) acParameter[3] : 0);
                    return true;
                case nameof(ReloadConfig):
                    ReloadConfig();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #endregion
    }
}
