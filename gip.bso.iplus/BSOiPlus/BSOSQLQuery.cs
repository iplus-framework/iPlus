using ClosedXML.Excel;
using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static gip.core.datamodel.Global;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'SQL Query'}de{'SQL Query'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACClassMethod.ClassName)]

    public class BSOSQLQuery : ACBSONav
    {
        #region const

        public short[] SQLMethodKindIndexes = new short[] { (short)Global.ACKinds.MSMethodExt, (short)Global.ACKinds.MSMethodExtClient };

        #endregion

        #region c´tors

        /// <summary>
        /// Initializes a new instance of the <see cref="BSOVisualisationStudio"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOSQLQuery(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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

            AccessNav.NavSearch();

            if (SQLScriptList != null && SQLScriptList.Any())
            {
                SelectedSQLScript = SQLScriptList.FirstOrDefault();
            }

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_SelectedSQLScript != null)
            {
                CorrectEmptyScript(_SelectedSQLScript);
            }
            ACSaveChanges();
            return base.ACDeInit(deleteACClassTask);
        }


        #endregion

        #region Propertes

        public SQLScriptResult SQLScriptResult { get; set; }

        #region SQLScript
        public const string SQLScript = "SQLScript";

        public override IAccessNav AccessNav { get { return AccessPrimary; } }
        /// <summary>
        /// The _ access primary
        /// </summary>
        ACAccessNav<ACClassMethod> _AccessSQLScript;
        /// <summary>
        /// Gets the access primary.
        /// </summary>
        /// <value>The access primary.</value>
        [ACPropertyAccessPrimary(9999, nameof(SQLScript))]
        public ACAccessNav<ACClassMethod> AccessPrimary
        {
            get
            {
                if (_AccessSQLScript == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, PrimaryNavigationquery(), ACType.ACIdentifier);
                    if (navACQueryDefinition != null)
                    {
                        _AccessSQLScript = navACQueryDefinition.NewAccessNav<ACClassMethod>(ACClassMethod.ClassName, this);
                        navACQueryDefinition.CheckAndReplaceFilterColumnsIfDifferent(NavigationqueryDefaultFilter);
                    }
                }
                return _AccessSQLScript;
            }
        }


        private List<ACFilterItem> NavigationqueryDefaultFilter
        {
            get
            {
                List<ACFilterItem> aCFilterItems = new List<ACFilterItem>();

                ACFilterItem acClassFilter = new ACFilterItem(FilterTypes.filter, nameof(ACClassMethod.ACClass) + "\\" + nameof(ACIdentifier), LogicalOperators.equal, Operators.and, ComponentClass.ACIdentifier, true);
                aCFilterItems.Add(acClassFilter);

                ACFilterItem phOpen = new ACFilterItem(Global.FilterTypes.parenthesisOpen, null, Global.LogicalOperators.none, Global.Operators.and, null, true);
                aCFilterItems.Add(phOpen);

                ACFilterItem partslistNoFilter = new ACFilterItem(FilterTypes.filter, nameof(ACClassMethod.ACKindIndex), LogicalOperators.contains, Operators.or, ((short)Global.ACKinds.MSMethodExt).ToString(), true);
                aCFilterItems.Add(partslistNoFilter);

                ACFilterItem filterPartslistName = new ACFilterItem(FilterTypes.filter, nameof(ACClassMethod.ACKindIndex), LogicalOperators.contains, Operators.or, ((short)Global.ACKinds.MSMethodExtClient).ToString(), true);
                aCFilterItems.Add(filterPartslistName);

                ACFilterItem phClose = new ACFilterItem(Global.FilterTypes.parenthesisClose, null, Global.LogicalOperators.none, Global.Operators.and, null, true);
                aCFilterItems.Add(phClose);

                return aCFilterItems;
            }
        }


        private ACClassMethod _SelectedSQLScript;
        /// <summary>
        /// Selected property for ACClassMethod
        /// </summary>
        /// <value>The selected SQLScript</value>
        [ACPropertySelected(9999, nameof(SQLScript), "en{'SelectedSQLScript'}de{'SelectedSQLScript'}")]
        public ACClassMethod SelectedSQLScript
        {
            get
            {
                return _SelectedSQLScript;
            }
            set
            {
                if (_SelectedSQLScript != value)
                {
                    if(_SelectedSQLScript != null)
                    {
                        CorrectEmptyScript(_SelectedSQLScript);
                    }
                    _SelectedSQLScript = value;
                    OnPropertyChanged(nameof(SelectedSQLScript));
                    SQLScriptResult = null;
                    OnPropertyChanged(nameof(SQLScriptResult));
                }
            }
        }

        /// <summary>
        /// List property for ACClassMethod
        /// </summary>
        /// <value>The SQLScript list</value>
        [ACPropertyList(9999, nameof(SQLScript))]
        public List<ACClassMethod> SQLScriptList
        {
            get
            {
                return AccessPrimary.NavList.ToList();
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Methods -> ACMethods

        /// <summary>
        /// Saves this instance.
        /// </summary>
        [ACMethodCommand(nameof(Save), "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void Save()
        {
            OnSave();
        }

        /// <summary>
        /// Undoes the save.
        /// </summary>
        [ACMethodCommand(nameof(UndoSave), "en{'Undo'}de{'Rückgängig'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost)]
        public void UndoSave()
        {
            if (!PreExecute()) return;
            OnUndoSave();

            AccessNav.NavSearch();
            OnPropertyChanged(nameof(SQLScriptList));
        }

        /// <summary>
        /// Determines whether [is enabled save].
        /// </summary>
        /// <returns><c>true</c> if [is enabled save]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledSave()
        {
            return OnIsEnabledSave();
        }

        /// <summary>
        /// Source Property: Execute
        /// </summary>
        [ACMethodInfo(nameof(ExecuteSQL), "en{'Execute SQL'}de{'SQL ausführen'}", 999)]
        public void ExecuteSQL()
        {
            if (!IsEnabledExecuteSQL())
                return;
            SQLScriptResult = null;
            OnPropertyChanged(nameof(SQLScriptResult));

            BackgroundWorker.RunWorkerAsync(nameof(ExecuteSQL));
            ShowDialog(this, DesignNameProgressBar);
        }

        public bool IsEnabledExecuteSQL()
        {
            return SelectedSQLScript != null && !string.IsNullOrEmpty(SelectedSQLScript.Sourcecode);
        }

        private string excelFileName;
        /// <summary>
        /// Source Property: ExportToExcel
        /// </summary>
        [ACMethodInfo(nameof(ExportToExcel), "en{'Export to excel file'}de{'In Excel-Datei exportieren'}", 999)]
        public void ExportToExcel()
        {
            if (!IsEnabledExportToExcel())
                return;

            excelFileName = Root.RootPageWPF.SaveFileDialog("Excel Files (*.xlsx)|*.xlsx");
            if (!string.IsNullOrEmpty(excelFileName))
            {
                BackgroundWorker.RunWorkerAsync(nameof(ExportToExcel));
                ShowDialog(this, DesignNameProgressBar);
            }
        }

        public bool IsEnabledExportToExcel()
        {
            return SQLScriptResult != null && SQLScriptResult.DataTable != null;
        }

        [ACMethodInteraction(nameof(NewSQLScript), Const.New, (short)MISort.New, true, nameof(SelectedSQLScript), Global.ACKinds.MSMethodPrePost)]
        public void NewSQLScript()
        {
            if (!IsEnabledNewSQLScript())
            {
                return;
            }

            Database database = Database as Database;
            ACClass cl = database.ACClass.Where(c => c.ACClassID == ComponentClass.ACClassID).FirstOrDefault();

            ACClassMethod acClassMethod = ACClassMethod.NewScriptACClassMethod(Database as Database, cl, Global.ACKinds.MSMethodExtClient);
            acClassMethod.SortIndex = Convert.ToInt16(cl.ACClassMethod_ACClass.Count + 1);
            acClassMethod.Sourcecode = "select getdate() as CurrentTime;";
            cl.AddNewACClassMethod(acClassMethod);
            AccessPrimary.NavList.Add(acClassMethod);
            OnPropertyChanged(nameof(SQLScriptList));
            SelectedSQLScript = acClassMethod;
        }

        public bool IsEnabledNewSQLScript()
        {
            return true;
        }

        [ACMethodInteraction(nameof(DeleteSQLScript), "en{'Delete'}de{'Löschen'}", (short)MISort.New, true, nameof(SelectedSQLScript), Global.ACKinds.MSMethodPrePost)]
        public void DeleteSQLScript()
        {
            ClearMessages();
            if (!IsEnabledDeleteSQLScript())
            {
                return;
            }
            ACClassMethod mth = SelectedSQLScript;
            Msg msg = mth.DeleteACObject(Database, true);
            if (msg != null)
            {
                SendMessage(msg);
                return;
            }
            AccessPrimary.NavList.Remove(mth);
            OnPropertyChanged(nameof(SQLScriptList));
            SelectedSQLScript = SQLScriptList.FirstOrDefault();
        }



        public bool IsEnabledDeleteSQLScript()
        {
            return SelectedSQLScript != null;
        }

        #endregion

        #region Methods -> Overrides

        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            if (vbControl == null)
                return base.OnGetControlModes(vbControl);

            Global.ControlModes result = base.OnGetControlModes(vbControl);
            if (result < Global.ControlModes.Enabled)
                return result;
            switch (vbControl.VBContent)
            {
                case nameof(SelectedSQLScript) + "\\" + nameof(ACClassMethod.ACIdentifier):
                    if (SelectedSQLScript != null && !string.IsNullOrEmpty(SelectedSQLScript.ACIdentifier))
                    {
                        result = Global.ControlModes.Disabled;
                    }
                    break;
            }

            return result;
        }

        #endregion


        #region Methods -> Private

        #region Methods -> Private -> Connection

        private string GetCleanedUpConnectionString(Configuration configuration, string connectionStringName)
        {
            string defaultConnectionString = "name=iPlusV4_Entities";
            if (configuration != null && configuration.ConnectionStrings != null)
            {
                try
                {
                    ConnectionStringSettings setting = configuration.ConnectionStrings.ConnectionStrings[connectionStringName];
                    defaultConnectionString = setting.ConnectionString;
                    if (!string.IsNullOrEmpty(defaultConnectionString))
                    {
                        defaultConnectionString = defaultConnectionString.Replace(System.Environment.NewLine, "").Replace("        ", "");
                        defaultConnectionString = ConnectionStringRemoveEntityPart(defaultConnectionString);
                    }
                }
                catch (Exception ec)
                {
                    Console.WriteLine(@"Error getting DefaultConnectionString: {0}", ec.Message);
                }
            }
            return defaultConnectionString;
        }

        private string ConnectionStringRemoveEntityPart(string connString)
        {
            string rawConnString = "";
            string[] regexes = new string[] { regexConnectionStringPartIplus, regexConnectionStringPartMES };
            foreach (string regex in regexes)
            {
                rawConnString = ExcapeConnectionPartContent(regex, connString);
                if (!string.IsNullOrEmpty(rawConnString))
                    break;
            }
            return rawConnString;
        }

        string ExcapeConnectionPartContent(string regex, string connString)
        {
            var match = Regex.Match(connString, regex);
            if (match.Groups.Count > 0)
                return match.Groups[match.Groups.Count - 1].Value;
            return null;
        }

        static string regexConnectionStringPartIplus = @"connection string=(["",\',\s,\&quot;]+)(.*)iPlus_db";
        static string regexConnectionStringPartMES = @"connection string=(["",\',\s,\&quot;]+)(.*)Framework";

        #endregion

        #region Methods -> Private -> DoBackgroundWork

        private SQLScriptResult DoGetDataTable()
        {
            SQLScriptQuery query = GetSQLScriptQuery(Database as Database, SelectedSQLScript);

            SQLScriptResult result = new SQLScriptResult();
            var dt = new DataTable();

            var da = new SqlDataAdapter();
            var sb = new StringBuilder();

            using (var cn = new SqlConnection(query.ConnectionString))
            {
                try
                {
                    cn.Open();

                    var timer = Stopwatch.StartNew();

                    cn.InfoMessage += delegate (object sender, SqlInfoMessageEventArgs e)
                    {
                        sb.AppendLine(e.Message);
                    };

                    var cmd = new SqlCommand(query.Script, cn)
                    {
                        CommandType = CommandType.Text,
                        CommandTimeout = 0
                    };

                    da.SelectCommand = cmd;
                    da.Fill(dt);

                    dt.TableName = query.TableName;


                    timer.Stop();

                    result.DataTable = dt;
                    result.Duration = timer.Elapsed;
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                }
            }

            return result;
        }

        private Msg DoExportToExcel()
        {
            Msg msg = null;
            try
            {
                XLWorkbook workBook = new XLWorkbook();
                var worksheet = workBook.Worksheets.Add(SQLScriptResult.DataTable, SelectedSQLScript.ACIdentifier);
                if (worksheet != null)
                {
                    worksheet.Columns().AdjustToContents();
                }
                workBook.SaveAs(excelFileName);
            }
            catch (Exception ex)
            {
                msg = new Msg(this, eMsgLevel.Exception, nameof(BSOSQLQuery), nameof(ExportToExcel), 239, ex.Message);
            }
            return msg;
        }

        #endregion

        #region Methods -> Private -> Helpers
        private List<ACClassMethod> LoadSQLScriptList()
        {
            return
                ComponentClass
                .ACClassMethod_ACClass
                .AsEnumerable()
                .Where(c => SQLMethodKindIndexes.Contains(c.ACKindIndex))
                .OrderBy(c => c.ACIdentifier)
                .ToList();
        }

        private SQLScriptQuery GetSQLScriptQuery(Database database, ACClassMethod method)
        {
            SQLScriptQuery query = new SQLScriptQuery();
            query.TableName = "table";
            query.ConnectionString = GetCleanedUpConnectionString(CommandLineHelper.ConfigCurrentDir, "iPlusV4_Entities");
            query.Database = database.Connection.Database;
            query.Script = method.Sourcecode;
            return query;
        }


        private void CorrectEmptyScript(ACClassMethod mth)
        {
            if (string.IsNullOrEmpty(mth.Sourcecode))
            {
                mth.Sourcecode = "--";
            }
        }
        #endregion

        #endregion

        #endregion

        #region Messages

        /// <summary>
        /// The _ current MSG
        /// </summary>
        Msg _CurrentMsg;
        /// <summary>
        /// Gets or sets the current MSG.
        /// </summary>
        /// <value>The current MSG.</value>
        [ACPropertyCurrent(405, "Message", "en{'Message'}de{'Meldung'}")]
        public Msg CurrentMsg
        {
            get
            {
                return _CurrentMsg;
            }
            set
            {
                _CurrentMsg = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Msg> msgList;
        /// <summary>
        /// Gets the MSG list.
        /// </summary>
        /// <value>The MSG list.</value>
        [ACPropertyList(406, "Message", "en{'Messagelist'}de{'Meldungsliste'}")]
        public ObservableCollection<Msg> MsgList
        {
            get
            {
                if (msgList == null)
                    msgList = new ObservableCollection<Msg>();
                return msgList;
            }
        }

        public void SendMessage(Msg msg)
        {
            Messages.Msg(msg);
            MsgList.Add(msg);
            OnPropertyChanged(nameof(MsgList));
        }

        private void ClearMessages()
        {
            MsgList.Clear();
            OnPropertyChanged(nameof(MsgList));
        }

        #endregion

        #region BackgroundWorker
        /// <summary>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        public override void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            base.BgWorkerDoWork(sender, e);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            string command = e.Argument.ToString();
            switch (command)
            {
                case nameof(ExecuteSQL):
                    e.Result = DoGetDataTable();
                    break;
                case nameof(ExportToExcel):
                    e.Result = DoExportToExcel();
                    break;
            }
        }


        public override void BgWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ClearMessages();
            base.BgWorkerCompleted(sender, e);
            CloseWindow(this, DesignNameProgressBar);
            ACBackgroundWorker worker = sender as ACBackgroundWorker;
            string command = worker.EventArgs.Argument.ToString();

            if (e.Cancelled)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Info, Message = string.Format(@"Operation {0} canceled by user!", command) });
            }
            else if (e.Error != null)
            {
                SendMessage(new Msg() { MessageLevel = eMsgLevel.Error, Message = string.Format(@"Error by doing {0}! Message:{1}", command, e.Error.Message) });
            }
            else
            {
                switch (command)
                {
                    case nameof(ExecuteSQL):
                        SQLScriptResult = e.Result as SQLScriptResult;
                        OnPropertyChanged(nameof(SQLScriptResult));
                        if (!SQLScriptResult.Success && SQLScriptResult.Exception != null)
                        {
                            SendMessage(new Msg() { MessageLevel = eMsgLevel.Exception, Message = SQLScriptResult.Exception.Message });
                        }
                        break;
                    case nameof(ExportToExcel):
                        object result = e.Result;
                        if (e.Result != null)
                        {
                            Msg msg = e.Result as Msg;
                            SendMessage(msg);
                        }
                        break;
                }
            }
        }

        #endregion
    }
}
