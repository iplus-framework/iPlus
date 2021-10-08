using gip.core.datamodel;
using gip.core.autocomponent;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using gip.core.reporthandler.Flowdoc;
using System.Windows.Documents;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPrintServerBase'}de{'ACPrintServerBase'}", Global.ACKinds.TACApplicationManager, Global.ACStorableTypes.Required, false, "", false)]
    public class ACPrintServerBase : PAClassAlarmingBase
    {

        #region c´tors
        public const string MN_Print = "Print";

        public ACPrintServerBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _DelegateQueue = new ACDelegateQueue(ACIdentifier);
            }
            _DelegateQueue.StartWorkerThread();

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            _DelegateQueue.StopWorkerThread();
            using (ACMonitor.Lock(_20015_LockValue))
            {
                _DelegateQueue = null;
            }

            return base.ACDeInit(deleteACClassTask);
        }

        protected static IACEntityObjectContext _CommonManagerContext;
        /// <summary>
        /// Returns a seperate and shared Database-Context "StaticACComponentManager".
        /// Because Businessobjects also inherit from this class all BSO's get this shared database context.
        /// If some custom BSO's needs its own context, then they have to override this property.
        /// Application-Managers that also inherit this class should override this property an use their own context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_CommonManagerContext == null)
                    _CommonManagerContext = ACObjectContextManager.GetOrCreateContext<Database>("StaticACComponentManager");
                return _CommonManagerContext;
            }
        }
        #endregion

        #region Properties

        [ACPropertyInfo(9999, DefaultValue = "localhost")]
        public string IPAddress
        {
            get;
            set;
        }

        [ACPropertyInfo(9999, DefaultValue = (Int16)502)]
        public Int16 Port
        {
            get;
            set;
        }

        [ACPropertyInfo(9999)]
        public Int32 SendTimeout
        {
            get;
            set;
        }


        [ACPropertyInfo(9999)]
        public Int32 ReceiveTimeout
        {
            get;
            set;
        }

        private ACDelegateQueue _DelegateQueue = null;
        public ACDelegateQueue DelegateQueue
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _DelegateQueue;
                }
            }
        }

        #endregion

        #region Methods

        [ACMethodInfo("Print", "en{'Print on server'}de{'Auf Server drucken'}", 200, true)]
        public virtual void Print(Guid bsoClassID, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies)
        {
            // Use Queue
            DelegateQueue.Add(() =>
            {
                ACBSO acBSO = null;
                try
                {
                    // PAOrderInfo => 
                    // ACPrintServer Step04 - Get server instance BSO and mandatory report design
                    acBSO = GetACBSO(bsoClassID, pAOrderInfo);
                    ACClassDesign aCClassDesign = acBSO.GetDesign(designACIdentifier);
                    // ACPrintServer Step05 - Prepare ReportData
                    ReportData reportData = GetReportData(acBSO, aCClassDesign);
                    // ACPrintServer Step06 - Write to stream
                    SendDataToPrinter(reportData);
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
                        if (acBSO != null)
                            acBSO.Stop();
                    }
                    catch (Exception e)
                    {
                        // TODO: Alarm
                        Messages.LogException(this.GetACUrl(), "Print(20)", e);
                    }
                }
            });
        }

        /// <summary>
        /// Factiry BSI abd setzo data frin PAPrderInfo
        /// </summary>
        /// <param name="componetACUrl"></param>
        /// <param name="pAOrderInfo"></param>
        /// <returns></returns>
        public virtual ACBSO GetACBSO(Guid bsoClassID, PAOrderInfo pAOrderInfo)
        {
            ACClass bsoACClass = Root.Database.ContextIPlus.GetACType(bsoClassID);
            ACBSO acBSO = StartComponent(bsoACClass, bsoACClass, new ACValueList()) as ACBSO;
            if (acBSO == null)
                return null;
            acBSO.SetDataFromPAOrderInfo(pAOrderInfo);
            return acBSO;
        }

        /// <summary>
        /// From prepared ACBSO produce ReportData
        /// </summary>
        /// <param name="aCBSO"></param>
        /// <param name="aCClassDesign"></param>
        /// <returns></returns>
        public virtual ReportData GetReportData(ACBSO aCBSO, ACClassDesign aCClassDesign)
        {
            bool cloneInstantiated = false;
            ACQueryDefinition aCQueryDefinition = Root.Queries.CreateQueryByClass(null, aCClassDesign.ACClass, "");
            ReportData reportData = ReportData.BuildReportData(out cloneInstantiated, Global.CurrentOrList.Current, aCBSO, aCQueryDefinition, aCClassDesign);
            return reportData;
        }

        /// <summary>
        /// Convert report data to stream
        /// </summary>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void SendDataToPrinter(ReportData reportData)
        {
            using (TcpClient tcpClient = new TcpClient(IPAddress, Port))
            {
                NetworkStream clientStream = tcpClient.GetStream();
                ASCIIEncoding encoder = new ASCIIEncoding();
                WriteToStream(clientStream, reportData);
                clientStream.Flush();
            }
        }

        /// <summary>
        /// Component specific implementation writing data from ReportData to network stream 
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void WriteToStream(NetworkStream clientStream, ReportData reportData)
        {
            // Need BSO
            // TODO: Create FlowDocument and Loop through model
            ReportDocument reportDocument = new ReportDocument("sasa xaml");
            FlowDocument flowDoc = reportDocument.CreateFlowDocument(reportData);
            
            // Recursive method
            //foreach (var block in flowDoc.Blocks)
            //{
            //    if (block is InlineValueBase)
            //    {
            //        InlineValueBase valueBase = block as InlineValueBase;
            //        OnRenderValue(valueBase);
            //    }
            //}
        }



        #endregion
    }
}
