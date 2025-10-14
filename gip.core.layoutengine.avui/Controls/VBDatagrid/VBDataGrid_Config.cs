using Avalonia.Controls;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the list of column sizes.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt die Liste der Spaltengrößen dar.
    /// </summary>
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ColumnSize'}de{'ColumnSize'}", Global.ACKinds.TACSimpleClass)]
    public class ColumnSizeList : List<ColumnSize>
    {
    }

    /// <summary>
    /// Represents the size of column.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt die Größe der Spalte dar.
    /// </summary>
    [ACSerializeableInfo]
    [DataContract]
    public class ColumnSize
    {
        /// <summary>
        /// The ACIdentifier.
        /// </summary>
        [DataMember]
        public string ACIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the column width.
        /// </summary>
        [DataMember]
        public double Width
        {
            get;
            set;
        }
    }

    public partial class VBDataGrid
    {
        /// <summary>
        /// Loads the configuration.
        /// </summary>
        /// <summary xml:lang="de">
        /// Lädt die Konfiguration.
        /// </summary>
        [ACMethodInteraction("", "en{'Load Configuration...'}de{'Konfiguration laden...'}", 902, false)]
        public void LoadConfig()
        {
            if ((bool)BSOACComponent.ACUrlCommand("VBBSOQueryDialog!QueryLoadDlg", new object[] { ACQueryDefinition.RootACQueryDefinition }))
            {
                UpdateColumns();
                LoadDataGridConfig();
            }
        }

        /// <summary>
        /// Determines is LoadConfig enabled or disabled.
        /// </summary>
        /// <returns>Returns true if is LoadConfig enabled otherwise false.</returns>
        public bool IsEnabledLoadConfig()
        {
            if (!ACContentList.Any() || ACQueryDefinition == null)
                return false;
            ACColumnItem acColumnItem = ACContentList.Where(c => c is ACColumnItem).FirstOrDefault() as ACColumnItem;
            if (acColumnItem == null)
                return false;
            return true;
        }

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        /// <summary xml:lang="de">
        /// Speichert die Konfiguration.
        /// </summary>
        [ACMethodInteraction("", "en{'Save Configuration'}de{'Konfiguration speichern'}", 903, false)]
        public void SaveConfig()
        {
            UpdateACQueryDefinition();
            if (ACQueryDefinition.RootACQueryDefinition.SaveConfig(false, Global.ConfigSaveModes.User))
            {
                SaveDataGridConfig();
            }
        }

        /// <summary>
        /// Determines is SaveConfig enabled or disabled.
        /// </summary>
        /// <returns>Returns true if is SaveCofing enabled, otherwise false.</returns>
        public bool IsEnabledSaveConfig()
        {
            if (!ACContentList.Any() || ACQueryDefinition == null)
                return false;
            ACColumnItem acColumnItem = ACContentList.Where(c => c is ACColumnItem).FirstOrDefault() as ACColumnItem;
            if (acColumnItem == null)
                return false;
            return true;
        }

        /// <summary>
        /// Saves the configuration as.
        /// </summary>
        /// <summary xml:lang="de">
        /// Speichert die Konfiguration als.
        /// </summary>
        [ACMethodInteraction("", "en{'Save Configuration as...'}de{'Konfiguration speichern unter...'}", 904, false)]
        public void SaveConfigAs()
        {
            UpdateACQueryDefinition();
            var fileName = BSOACComponent.ACUrlCommand("VBBSOQueryDialog!QuerySaveDlg", new object[] { ACQueryDefinition.RootACQueryDefinition }) as string;
            if (!string.IsNullOrEmpty(fileName))
            {
                SaveDataGridConfig();
            }
        }

        /// <summary>
        /// Determines is SaveConfigAs enabled or disabled.
        /// </summary>
        /// <returns>Returns true is if SaveConfigAs enabled otherwise false.</returns>
        public bool IsEnabledSaveConfigAs()
        {
            if (!ACContentList.Any() || ACQueryDefinition == null)
                return false;
            ACColumnItem acColumnItem = ACContentList.Where(c => c is ACColumnItem).FirstOrDefault() as ACColumnItem;
            if (acColumnItem == null)
                return false;
            return true;
        }

        /// <summary>
        /// Saves DataGrid configuration.
        /// </summary>
        public void SaveDataGridConfig()
        {
            string fileName = GetFileName();
            try
            {
                ColumnSizeList columnSizeList = new ColumnSizeList();
                foreach (var gridColumn in this.Columns.OrderBy(c => c.DisplayIndex))
                {
                    IGriColumn iGridColumn = gridColumn as IGriColumn;
                    if (iGridColumn == null || iGridColumn.ACColumnItem == null)
                        continue;

                    columnSizeList.Add(new ColumnSize { ACIdentifier = (gridColumn as IGriColumn).ACColumnItem.ACIdentifier, Width = gridColumn.ActualWidth });
                }
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<ColumnSize>));

                using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000)) // ACType ist immer vom globalen Datenbankkontext
                {
                    Database database = Database.GlobalDatabase;

                    ACClass typeACClass = ACQueryDefinition.RootACQueryDefinition.TypeACClass as ACClass;
                    var query = typeACClass.ConfigurationEntries.Where(c => c.LocalConfigACUrl == fileName);

                    IACConfig acConfig;
                    if (!query.Any())
                    {
                        acConfig = typeACClass.NewACConfig(null, database.GetACType(typeof(ColumnSizeList)));
                        acConfig.LocalConfigACUrl = fileName;
                        acConfig[Const.Value] = columnSizeList;
                    }
                    else
                    {
                        acConfig = query.First();
                        acConfig[Const.Value] = columnSizeList;
                    }
                    database.ACSaveChanges();
                }
            }
            catch (Exception ex)
            {
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDataGrid", "SaveDataGridConfig", ex.Message);
            }
        }

        /// <summary>
        /// Loads DataGrid configuration.
        /// </summary>
        public void LoadDataGridConfig()
        {
            try
            {
                string fileName = GetFileName();
                if (ACQueryDefinition == null || String.IsNullOrEmpty(fileName))
                    return;

                ACClass typeACClass = ACQueryDefinition.RootACQueryDefinition.TypeACClass as ACClass;
                var query = typeACClass.ConfigurationEntries.Where(c => c.LocalConfigACUrl == fileName);
                if (!query.Any())
                    return;
                var acConfig = query.First();
                ColumnSizeList columnSizeList = acConfig[Const.Value] as ColumnSizeList;
                if (columnSizeList == null)
                    return;

                int gridColumnCount = this.Columns.Count;
                foreach (var gridColumn in this.Columns)
                {
                    IGriColumn iGridColumn = gridColumn as IGriColumn;
                    if (iGridColumn == null || iGridColumn.ACColumnItem == null)
                        continue;
                    ColumnSize colSize = columnSizeList.Where(c => c.ACIdentifier == iGridColumn.ACColumnItem.ACIdentifier).FirstOrDefault();
                    if (colSize == null)
                        continue;
                    gridColumn.Width = new DataGridLength(colSize.Width);
                    int displayIndex = columnSizeList.IndexOf(colSize);
                    if (gridColumn.DisplayIndex != displayIndex)
                    {
                        var maxIndex = (gridColumnCount == 0) ? 0 : gridColumnCount - 1;
                        gridColumn.DisplayIndex = (displayIndex <= maxIndex) ? displayIndex : maxIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDataGrid", "LoadDataGridConfig", ex.Message);
            }
        }

        private string GetFileName()
        {
            if (ACQueryDefinition == null)
                return "";

            string fileName = "VBDataGrid_" + ACQueryDefinition.RootACQueryDefinition.LocalConfigACUrl;

            if (ACQueryDefinition != ACQueryDefinition.RootACQueryDefinition)
            {
                ACQueryDefinition acQueryDefinition = ACQueryDefinition;
                string postFix = "";
                while (acQueryDefinition != ACQueryDefinition.RootACQueryDefinition && acQueryDefinition != null)
                {
                    postFix = "_" + acQueryDefinition.ACIdentifier + postFix;
                    acQueryDefinition = acQueryDefinition.ParentACObject as ACQueryDefinition;
                }
                fileName += postFix;
            }
            return fileName;
        }

        /// <summary>
        /// Updates ACQuery definition.
        /// </summary>
        /// <param name="configName">The configuration name.</param>
        public void UpdateACQueryDefinition(string configName = "")
        {
//            int i = 0;
//            // ACColumns
//            List<ACColumnItem> acColumnItemList = ACQueryDefinition.ACColumns.ToList();
//            ACQueryDefinition.ACColumns.Clear();
//            foreach (var gridColumn in this.Columns.OrderBy(c => c.DisplayIndex).Select(c => c as IGriColumn))
//            {
//#if DEBUG

//                System.Diagnostics.Debug.WriteLine(gridColumn.VBContent);
//#endif
//                var query2 = acColumnItemList.Where(c => c.ACIdentifier == gridColumn.ACColumnItem.ACIdentifier);
//                if (query2.Any())
//                {
//                    ACQueryDefinition.ACColumns.Add(query2.First());
//                    acColumnItemList.Remove(query2.First());
//                }
//                else
//                {
//                    ACColumnItem acColumn = new ACColumnItem(gridColumn.ACColumnItem.ACIdentifier);
//                    ACQueryDefinition.ACColumns.Add(acColumn);
//                }
//                i++;
//            }

//            bool sortColumnsCleared = false;
//            // ACSortColumns
//            var query = this.Columns
//                //.Where(c => c.SortDirection != null)
//                .OrderBy(c => c.DisplayIndex);
//            if (query.Any())
//            {
//                foreach (var gridColumn in this.Columns
//                    //.Where(c => c.Header.CurrentSortingState != null)
//                    .OrderBy(c => c.DisplayIndex))
//                {
//                    IGriColumn iGridColumn = gridColumn as IGriColumn;

//                    // Überprüfe ob das eine Eigenschaft ist die in der Datenbank bzw. des EntityObjektes ist, nur dann darf sortiert werden
//                    // weil sonst ACQuery.SearchWithEntitySQL eine Abfrage baut auf ein Feld das nicht in der Datenbank existiert
//                    if (iGridColumn != null
//                        && iGridColumn.ColACType != null
//                        && iGridColumn.ColACType.ACKind == Global.ACKinds.PSProperty
//                        && !String.IsNullOrEmpty(iGridColumn.VBContent))
//                    {
//                        //gridColumn.Header.CurrentSortingState
//                        Type entityType = iGridColumn.ColACType.ACClass.ObjectType;
//                        if (entityType != null && typeof(VBEntityObject).IsAssignableFrom(entityType))
//                        {
//                            //[NotMapped]
//                            PropertyInfo propInfo = entityType.GetProperty(iGridColumn.ColACType.ACIdentifier);

//                            if (propInfo != null && propInfo.IsDatabaseField())
//                            {
//                                if (!sortColumnsCleared)
//                                {
//                                    ACQueryDefinition.ACSortColumns.Clear();
//                                    sortColumnsCleared = true;
//                                }
//                                ACSortItem acSortItem = new ACSortItem(
//                                    iGridColumn.VBContent,
//                                    gridColumn.SortDirection.Value == ListSortDirection.Ascending ? Global.SortDirections.ascending : Global.SortDirections.descending,
//                                    true);
//                                ACQueryDefinition.ACSortColumns.Add(acSortItem);
//#if DEBUG
//                                System.Diagnostics.Debug.WriteLine(gridColumn.SortMemberPath + " => " + gridColumn.SortDirection);
//#endif
//                            }
//                        }
//                    }
//                }
//            }
        }
    }
}
