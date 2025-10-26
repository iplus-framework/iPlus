using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using ClosedXML.Excel;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace gip.core.layoutengine.avui
{

    public partial class VBDataGrid
    {
        /// <summary>
        /// Displays ACUrl.
        /// </summary>
        /// <summary xml:lang="de">
        /// Zeigt ACUrl.
        /// </summary>
        [ACMethodInteraction("", "en{'Display ACUrl'}de{'Display ACUrl'}", (short)MISort.DisplayACUrl, false)]
        public virtual void DisplayACUrl()
        {
            if (!ACContentList.Any())
                return;
            IACObject content = ACContentList.Where(c => c is IACEntityProperty).FirstOrDefault();
            if (content == null)
                return;

            try
            {
                string acURL = content.GetACUrl();
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                clipboard.SetTextAsync(acURL);

                VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = acURL, MessageLevel = eMsgLevel.Info }, eMsgButton.OK, this);
                vbMessagebox.ShowMessageBox();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDataGrid", "DisplayACUrl", msg);
            }
        }

        /// <summary>
        /// Determines is DisplayACUrl enabled or disabled.
        /// </summary>
        /// <returns>Returns true if is DisplayACUrl enabled, otherwise false.</returns>
        public virtual bool IsEnabledDisplayACUrl()
        {
            if (!Database.Root.Environment.License.MayUserDevelop)
                return false;

            if (!ACContentList.Any())
                return false;

            if (ACContentList.First() is ACColumnItem)
                return false;

            var query = ACContentList.Where(c => c is IACEntityProperty);
            if (!query.Any())
                return false;

            using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
            {
                ACClass acClassBSOStudio = Database.GlobalDatabase.ACClass.Where(c => c.ACIdentifier == "BSOiPlusStudio").FirstOrDefault();
                if (acClassBSOStudio == null)
                    return false;

                if (acClassBSOStudio.GetRight(acClassBSOStudio) != Global.ControlModes.Enabled)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Copies the content of selected area to clipboard.
        /// </summary>
        /// <summary xml:lang="de">
        /// Kopiert den Inhalt des ausgewählten Bereichs in die Zwischenablage.
        /// </summary>
        [ACMethodInteraction("", "en{'Copy to clipboard'}de{'In Zwischenablage kopieren'}", (short)102, false)]
        public virtual void CopyToClipboard()
        {
            this.ProcessCopyKeyViaReflection(Avalonia.Input.KeyModifiers.Control);
            //_CopyToClipboard = true;
            //try
            //{
            //    bool canSelect = CanSelectMultipleItems;
            //    if (!canSelect)
            //        CanSelectMultipleItems = true;
            //    SelectAllCells();
            //    DataGridClipboardCopyMode lastMode = ClipboardCopyMode;
            //    ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            //    ApplicationCommands.Copy.Execute(null, this);
            //    ClipboardCopyMode = lastMode;
            //    UnselectAllCells();
            //    CanSelectMultipleItems = canSelect;
            //}
            //catch (Exception e)
            //{
            //    string msg = e.Message;
            //    if (e.InnerException != null && e.InnerException.Message != null)
            //        msg += " Inner:" + e.InnerException.Message;

            //    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
            //        datamodel.Database.Root.Messages.LogException("VBDataGrid", "CopyToClipboard", msg);
            //}
            //_CopyToClipboard = false;
        }

        /// <summary>
        /// Copies the content of selected row to clipboard.
        /// </summary>
        /// <summary xml:lang="de">
        /// Kopiert den Inhalt des ausgewählte Reihe in die Zwischenablage.
        /// </summary>
        [ACMethodInteraction("", "en{'Copy row to clipboard'}de{'Reihe in Zwischenablage kopieren'}", (short)102, false)]
        public virtual void CopyRowToClipboard()
        {
            _CopyToClipboard = true;
            try
            {
                DataGridClipboardCopyMode lastMode = ClipboardCopyMode;
                ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                this.ProcessCopyKeyViaReflection(Avalonia.Input.KeyModifiers.Control);
                //ApplicationCommands.Copy.Execute(null, this);
                ClipboardCopyMode = lastMode;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDataGrid", "CopyToClipboard", msg);
            }
            _CopyToClipboard = false;
        }

        private class ExportColumn
        {
            public Type _Type;
            public string _VBContent;
            public IGriColumn _Column;
        }

        [ACMethodInteraction("", "en{'Export to excel file'}de{'In Excel-Datei exportieren'}", (short)103, true)]
        public virtual void Export2Excel()
        {
            try
            {
                Type typeOfString = typeof(string);
                IDataGridCollectionView collectionView = this.GetDataGridCollectionViewViaReflection();
                if (collectionView == null)
                    return;

                List<ExportColumn> exportColumns = new List<ExportColumn>();
                DataTable dt = new DataTable();
                var exportableCols = Columns.Where(c => ((IGriColumn)c).ColACType != null);
                int i = 0;
                foreach (IGriColumn column in exportableCols)
                {
                    ExportColumn exportColumn = new ExportColumn() { _Column = column, _Type = column.ColACType.ObjectType, _VBContent = column.VBContent };
                    if (exportColumn._Type == null)
                        continue;
                    if (!IsExcelType(exportColumn._Type))
                    {
                        VBDataGridComboBoxColumn dcb = column as VBDataGridComboBoxColumn;
                        if (dcb == null || String.IsNullOrEmpty(dcb.DisplayMemberPath))
                            continue;
                        IACType subACType = column.ColACType.GetMember(dcb.DisplayMemberPath);
                        if (subACType == null)
                            continue;
                        Type subType = subACType.ObjectType;
                        if (!IsExcelType(subType))
                            continue;
                        exportColumn._Type = subType;
                        exportColumn._VBContent = exportColumn._VBContent + "\\" + dcb.DisplayMemberPath;
                    }
                    dt.Columns.Add(i.ToString() + " " + column.ColACType.ACCaption, exportColumn._Type);
                    exportColumns.Add(exportColumn);
                    i++;
                }
                foreach (var colViewEntry in collectionView)
                {
                    var row = dt.NewRow();
                    i = 0;
                    foreach (ExportColumn column in exportColumns)
                    {
                        object value = colViewEntry.GetValue(column._VBContent);
                        if (value == null)
                        {
                            row[i] = DBNull.Value;
                        }
                        else
                        {
                            if (value is IConvertible)
                                row[i] = value;
                            else if (column._Type == typeOfString)
                            {
                                string sValue = null;
                                try
                                {
                                    sValue = ACConvert.ObjectToXML(value, true);
                                }
                                catch (Exception)
                                {
                                }
                                if (String.IsNullOrEmpty(sValue))
                                    sValue = value.ToString();
                                row[i] = sValue;
                            }
                            else
                                row[i] = DBNull.Value;
                        }
                        i++;
                    }
                    dt.Rows.Add(row);
                }

                string fileName = Database.Root.RootPageWPF.SaveFileDialog("Excel Files (*.xlsx)|*.xlsx", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                if (!string.IsNullOrEmpty(fileName))
                {
                    XLWorkbook workBook = new XLWorkbook();
                    var worksheet = workBook.Worksheets.Add(dt, BSOACComponent.ACCaption);
                    if (worksheet != null)
                        worksheet.Columns().AdjustToContents();
                    workBook.SaveAs(fileName);
                }
            }
            catch (Exception e)
            {
                this.Root().Messages.LogException("VBDataGrid", "Export2Excel()", e);
                this.Root().Messages.Exception(this, e.Message, true);
            }
        }

        private static Type[] _ExcelTypes = new Type[] { typeof(string), typeof(DateTime), typeof(TimeSpan) };
        public bool IsExcelType(Type type)
        {
            if (type == null)
                return false;
            if (type.IsPrimitive)
                return true;
            return _ExcelTypes.Where(c => c.IsAssignableFrom(type)).Any();
        }

    }
}
