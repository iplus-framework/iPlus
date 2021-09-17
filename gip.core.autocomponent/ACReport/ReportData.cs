using System;
using System.Collections.Generic;
using System.Data;
using gip.core.datamodel;
using System.Linq;
using System.Reflection;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Contains all report data
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ReportData'}de{'ReportData'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ReportData
    {
        private Dictionary<string, object> _reportDocumentValues = new Dictionary<string, object>();
        /// <summary>
        /// Dictionary that contains all data that can be printed in a report.
        /// If ReportData was generated for a Businessobject, than the first entry in this Dictionary is a clone of the Businessobject 
        /// and the key is the ACIdentifier of the class where the report belongs to. Usually it's the classname of the Businessobject itsself.
        /// If ReportData was generated with a passed ACQueryDefinition, than the first entry in this Dictionary is the ACQueryDefinition
        /// and the key is its ChildACUrl-Property-Value.
        /// You can add other objects to the dictionary and access them by setting the InlinePropertyValueBase.DictKey-Property in the report.
        /// </summary>
        public Dictionary<string, object> ReportDocumentValues
        {
            get { return _reportDocumentValues; }
        }

        private bool _showUnknownValues = true;
        /// <summary>
        /// Shows all unknown values on the page
        /// </summary>
        public bool ShowUnknownValues
        {
            get { return _showUnknownValues; }
            set { _showUnknownValues = value; }
        }

        private bool _hasClonedBSO = true;
        public bool HasClonedBSO
        {
            get { return _hasClonedBSO; }
            set { _hasClonedBSO = value; }
        }


        public IACEntityObjectContext Database
        {
            get;
            set;
        }

        private ACClassDesign _ACClassDesign = null;
        public ACClassDesign ACClassDesign
        {
            get { return _ACClassDesign; }
            set { _ACClassDesign = value; }
        }
        /// <summary>
        /// Gets a data table by table name
        /// </summary>
        /// <param name="tableName">table name (case insensitive)</param>
        /// <returns>null, if DataTable not found</returns>
        public DataTable GetDataTableByName(string dictKey)
        {
            object obj;
            ReportDocumentValues.TryGetValue(dictKey, out obj);
            return obj as DataTable;
        }

        public T GetValue<T>(string dictKey)
        {
            object obj;
            ReportDocumentValues.TryGetValue(dictKey, out obj);
            if (obj == null)
                return default(T);
            return obj is T ? (T)obj : default(T);
        }


        public void StopACComponents()
        {
            if (!_hasClonedBSO)
                return;
            if (ReportDocumentValues.Count <= 0)
                return;
            foreach (IACComponent component in ReportDocumentValues.Values.Where(c => c is IACComponent))
            {
                component.Stop();
            }
        }

        public void InformComponents(object reportEngine, ACPrintingPhase printingPhase)
        {
            if (ReportDocumentValues.Count <= 0)
                return;
            foreach (IACComponent component in ReportDocumentValues.Values.ToList().Where(c => c is IACComponent))
            {
                component.OnPrintingPhase(reportEngine, printingPhase);
            }
        }

        public static ReportData BuildReportData(out bool cloneInstantiated, Global.CurrentOrList selectMode, IACComponent acComponent, ACQueryDefinition acQueryDefinition, ACClassDesign acClassDesign, bool preventClone = false)
        {
            ReportData reportData = new ReportData();
            reportData._ACClassDesign = acClassDesign;
            cloneInstantiated = false;
            if (acQueryDefinition == null) //IsCurrentDesignFromParentBSO
            {
                IACComponent clonedComponent = null;
                ACBSO bso = acComponent as ACBSO;
                if (bso != null && !preventClone)
                {
                    clonedComponent = bso.Clone() as IACComponent;
                    SynchronizeCurrentProp(clonedComponent as ACBSO, bso, Global.CurrentOrList.Current);
                }
                if (clonedComponent == null)
                    clonedComponent = acComponent;
                else
                {
                    cloneInstantiated = true;
                    reportData._hasClonedBSO = true;
                    //SynchronizeCurrentProp(clonedComponent as ACBSO, bso, selectMode);
                }
                //reportData.ReportDocumentValues.Add(clonedComponent.GetType().Name, clonedComponent);
                reportData.ReportDocumentValues.Add(acClassDesign.ACClass.ACIdentifier, clonedComponent);
            }
            else
            {
                UpdateACQueryFilter(acComponent as ACBSONav, acQueryDefinition, selectMode);
                reportData.ReportDocumentValues.Add(acQueryDefinition.ChildACUrl, acQueryDefinition);
            }
            return reportData;
        }

        public static void SynchronizeCurrentProp(ACBSO cloneBSO, ACBSO origBSO, Global.CurrentOrList selectMode)
        {
            if (cloneBSO == null || origBSO == null)
                return;
            if (selectMode != Global.CurrentOrList.Current)
                return;
            ACClassProperty acTypePrimary = origBSO.ComponentClass.Properties.Where(c => c.ACPropUsageIndex == (short)Global.ACPropUsages.AccessPrimary).FirstOrDefault();
            if (acTypePrimary == null)
                return;
            string acGroup = acTypePrimary.ACGroup;
            ACClassProperty acTypeCurrent = origBSO.ComponentClass.Properties.Where(c => c.ACPropUsageIndex == (short)Global.ACPropUsages.Current && c.ACGroup == acGroup).FirstOrDefault();
            if (acTypeCurrent == null)
                return;
            cloneBSO.ACUrlCommand(acTypeCurrent.ACIdentifier, origBSO.ACUrlCommand(acTypeCurrent.ACIdentifier));
        }

        public static void UpdateACQueryFilter(ACBSONav bso, ACQueryDefinition acQuery, Global.CurrentOrList selectMode)
        {
            if (bso == null || bso.AccessNav == null || acQuery == null)
                return;
            //Filterbedingung setzen
            if (selectMode == Global.CurrentOrList.Current)
            {
                ACClassProperty acTypePrimary = bso.ComponentClass.Properties.Where(c => c.ACPropUsageIndex == (short)Global.ACPropUsages.AccessPrimary).FirstOrDefault();
                if (acTypePrimary == null)
                    return;
                string acGroup = acTypePrimary.ACGroup;
                ACClassProperty acTypeCurrent = bso.ComponentClass.Properties.Where(c => c.ACPropUsageIndex == (short)Global.ACPropUsages.Current && c.ACGroup == acGroup).FirstOrDefault();
                if (acTypeCurrent == null)
                    return;
                IACObject currentProperty = bso.ACUrlCommand(acTypeCurrent.ACIdentifier) as IACObject;
                if (currentProperty == null)
                    return;

                Type typeACObject = currentProperty.GetType();
                PropertyInfo pi = typeACObject.GetProperty("KeyACIdentifier");
                string dataIdentifier = pi.GetValue(currentProperty, null) as string;
                if (!string.IsNullOrEmpty(dataIdentifier))
                {
                    //List<ACFilterItem> filterItems = new List<ACFilterItem>();
                    acQuery.ACFilterColumns.Clear();
                    string[] dataIdentifiers = dataIdentifier.Split(',');
                    foreach (var identifier in dataIdentifiers)
                    {
                        ACFilterItem item = new ACFilterItem(Global.FilterTypes.filter, identifier, Global.LogicalOperators.equal, Global.Operators.and, currentProperty.ACUrlCommand(identifier).ToString(), false);
                        acQuery.ACFilterColumns.Add(item);
                    }
                    //acQuery.ACFilterColumns = filterItems;
                }
                else
                {
                    acQuery.ACFilterColumns.Clear();
                }
            }
            else
            {
                //Filter auf Listenbereich
                acQuery.ACFilterColumns.Clear();
                foreach (ACFilterItem item in bso.AccessNav.NavACQueryDefinition.ACFilterColumns)
                {
                    acQuery.ACFilterColumns.Add(item);
                }

                //// Sortierreihenfolge
                acQuery.ACSortColumns.Clear();
                foreach (ACSortItem item in bso.AccessNav.NavACQueryDefinition.ACSortColumns)
                {
                    acQuery.ACSortColumns.Add(item);
                }
            }
        }

    }
}
