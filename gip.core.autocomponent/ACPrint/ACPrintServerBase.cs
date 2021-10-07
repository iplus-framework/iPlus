using gip.core.datamodel;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPrintServerBase'}de{'ACPrintServerBase'}", Global.ACKinds.TACApplicationManager, Global.ACStorableTypes.Required, false, "", false)]
    public class  ACPrintServerBase : PAClassAlarmingBase
    {

        #region c´tors

        public ACPrintServerBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
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
        #endregion

        #region Methods

        [ACMethodInfo("Print", "en{'Print on server'}de{'Auf Server drucken'}", 9999, true, Global.ACKinds.MSMethodPrePost)]
        public virtual void Print(string componetACUrl, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies)
        {
            // Use Queue
            ACClassTaskQueue.TaskQueue.ProcessAction(() =>
            {
                // PAOrderInfo => 
                // ACPrintServer Step04 - Get server instance BSO and mandatory report design
                ACBSO aCBSO = GetaCBSO(componetACUrl, pAOrderInfo);
                ACClassDesign aCClassDesign = aCBSO.GetDesign(designACIdentifier);
                // ACPrintServer Step05 - Prepare ReportData
                ReportData reportData = GetReportData(aCBSO, aCClassDesign);
                // ACPrintServer Step06 - Write to stream
                SendDataToPrinter(reportData);
            });
        }

        /// <summary>
        /// Factiry BSI abd setzo data frin PAPrderInfo
        /// </summary>
        /// <param name="componetACUrl"></param>
        /// <param name="pAOrderInfo"></param>
        /// <returns></returns>
        public virtual ACBSO GetaCBSO(string componetACUrl, PAOrderInfo pAOrderInfo)
        {
            IACComponent component = Root.ACUrlCommand(componetACUrl) as IACComponent;
            ACBSO aCBSO = component as ACBSO;
            aCBSO.SetDataFromPAOrderInfo(pAOrderInfo);
            return aCBSO;
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
            using(TcpClient tcpClient = new TcpClient(IPAddress, Port))
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
            throw new NotImplementedException();
        }

        #endregion
    }
}
