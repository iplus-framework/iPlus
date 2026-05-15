using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Objects;
using System.Linq;
using static gip.core.datamodel.Global;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'Custom value item lists'}de{'Benutzerdefinierte Wertlisten'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACClass.ClassName)]
    public class BSOCustomValueItemList : ACBSONav
    {

        #region ctor's

        public BSOCustomValueItemList(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(ACStartTypes startChildMode = ACStartTypes.Automatic)
        {
            bool baseACInit = base.ACInit(startChildMode);
            if (AccessPrimary != null)
            {
                AccessPrimary.NavSearch(Database, MergeOption.OverwriteChanges);
                OnPropertyChanged(nameof(CurrentCustomList));
            }
            return baseACInit;
        }

        #endregion

        #region ACProperties

        #region ACProperties -> AccessNav

        #region ACProperties -> AccessNav -> Primary

        public readonly string CustomList = "CustomList";
        public override IAccessNav AccessNav { get { return AccessPrimary; } }
        /// <summary>
        /// The _ access primary
        /// </summary>
        ACAccessNav<ACClass> _AccessPrimary;
        /// <summary>
        /// Gets the access primary.
        /// </summary>
        /// <value>The access primary.</value>
        [ACPropertyAccessPrimary(9999, nameof(CustomList))]
        public ACAccessNav<ACClass> AccessPrimary
        {
            get
            {
                if (_AccessPrimary == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, PrimaryNavigationquery(), ACType.ACIdentifier);
                    if (navACQueryDefinition != null)
                    {
                        navACQueryDefinition.CheckAndReplaceSortColumnsIfDifferent(NavigationqueryDefaultSort);
                        navACQueryDefinition.CheckAndReplaceFilterColumnsIfDifferent(NavigationqueryDefaultFilter);
                        navACQueryDefinition.ACFilterColumns.ListChanged += ACFilterColumns_ListChanged;
                        _AccessPrimary = navACQueryDefinition.NewAccessNav<ACClass>(ACClass.ClassName, this);
                        _AccessPrimary.NavSearchExecuting += AccessPrimaryNavSearchExecuting;
                    }
                }
                return _AccessPrimary;
            }
        }

        private IQueryable<ACClass> AccessPrimaryNavSearchExecuting(IQueryable<ACClass> result)
        {
            return result;
        }

        #endregion

        #region ACProperties -> AccessNav -> Filter and Sort

        private List<ACSortItem> NavigationqueryDefaultSort
        {
            get
            {
                List<ACSortItem> acSortItems = new List<ACSortItem>();

                ACSortItem partslistNo = new ACSortItem(nameof(ACClass.ACIdentifier), SortDirections.ascending, true);
                acSortItems.Add(partslistNo);


                return acSortItems;
            }
        }

        public List<ACFilterItem> NavigationqueryDefaultFilter
        {
            get
            {
                List<ACFilterItem> aCFilterItems = new List<ACFilterItem>();

                ACFilterItem filterBaseClass = new ACFilterItem(Global.FilterTypes.filter, $"{nameof(ACClass.ACClass1_BasedOnACClass)}\\{nameof(ACClass.ACIdentifier)}", Global.LogicalOperators.startsWith, Global.Operators.and, nameof(ACValueItemList), true);
                aCFilterItems.Add(filterBaseClass);

                ACFilterItem filterACKindIndex = new ACFilterItem(Global.FilterTypes.filter, $"{nameof(ACClass.ACKindIndex)}", Global.LogicalOperators.equal, Global.Operators.and, ((short)Global.ACKinds.TACClass).ToString(), true);
                aCFilterItems.Add(filterACKindIndex);

                return aCFilterItems;
            }
        }

        private void ACFilterColumns_ListChanged(object sender, ListChangedEventArgs e)
        {

        }


        #endregion

        #region ACProperties -> AccessNav -> Current, Selected and List

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [ACPropertyList(9999, nameof(CustomList))]
        public IEnumerable<ACClass> CustomListList
        {
            get
            {
                return AccessPrimary.NavList;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        [ACPropertySelected(9999, nameof(CustomList))]
        public ACClass SelectedCustomList
        {
            get
            {
                if (AccessPrimary == null)
                    return null;
                return AccessPrimary.Selected;
            }
            set
            {
                if (AccessPrimary == null)
                    return;
                AccessPrimary.Selected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current facility location.
        /// </summary>
        /// <value>The current facility location.</value>
        [ACPropertyCurrent(9999, nameof(CustomList))]
        public ACClass CurrentCustomList
        {
            get
            {
                if (AccessPrimary == null)
                    return null;
                return AccessPrimary.Current;
            }
            set
            {
                if (AccessPrimary == null)
                    return;

                AccessPrimary.Current = value;
                SetCurrentConfigPointACClassProperty(AccessPrimary.Current);
                OnPropertyChanged();
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// The _ current config point AC class property
        /// </summary>
        IACType _CurrentConfigPointACClassProperty;
        /// <summary>
        /// Gets or sets the current config point AC class property.
        /// </summary>
        /// <value>The current config point AC class property.</value>
        [ACPropertyInfo(9999, nameof(ConfigPointACClassProperty), "en{'Configtype'}de{'Konfiguration'}")]
        public IACType ConfigPointACClassProperty
        {
            get
            {
                return _CurrentConfigPointACClassProperty;
            }
            set
            {
                if (_CurrentConfigPointACClassProperty != value)
                {
                    _CurrentConfigPointACClassProperty = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PointConfigList));
                    UpdateConfigPointACClassPropertyLayout();
                }
            }
        }

        #region 1.1.2.3.2 ConfigPointConfig
        public readonly string PointConfig = "PointConfig";
        /// <summary>
        /// The _ current point config
        /// </summary>
        ACClassConfig _CurrentPointConfig;
        /// <summary>
        /// Gets or sets the current point config.
        /// </summary>
        /// <value>The current point config.</value>
        [ACPropertyCurrent(9999, nameof(PointConfig))]
        public ACClassConfig CurrentPointConfig
        {
            get
            {
                return _CurrentPointConfig;
            }
            set
            {
                if (_CurrentPointConfig != null)
                {
                    _CurrentPointConfig.PropertyChanged -= CurrentPointConfig_PropertyChanged;
                }
                _CurrentPointConfig = value;
                if (_CurrentPointConfig != null)
                {
                    _CurrentPointConfig.PropertyChanged += CurrentPointConfig_PropertyChanged;
                }
                OnPropertyChanged();
            }
        }

        private void CurrentPointConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ACClassConfig.Value))
            {
                if (sender is ACClassConfig config)
                {
                    string localConfigACUrl = config.Value != null ? config.Value.ToString() : string.Empty;
                    if (!string.IsNullOrEmpty(localConfigACUrl))
                    {
                        localConfigACUrl = ACUrlHelper.GetTrimmedName(localConfigACUrl);
                    }
                    config.LocalConfigACUrl = $"{nameof(ACValueItem)}_{localConfigACUrl}";
                    config.ACCaption = config.Value != null ? config.Value.ToString() : " ";
                }
            }
        }
         

        /// <summary>
        /// Gets the point config list.
        /// </summary>
        /// <value>The point config list.</value>
        [ACPropertyList(9999, nameof(PointConfig))]
        public IEnumerable<ACClassConfig> PointConfigList
        {
            get
            {
                if (CurrentCustomList == null || ConfigPointACClassProperty == null)
                    return null;

                return
                    ConfigPointACClassProperty
                    .GetConfigListOfType(CurrentCustomList)
                    .Select(c => c as ACClassConfig)
                    .OrderBy(c => c.ACCaption)
                    .ToList();
            }
        }

        private void SetCurrentConfigPointACClassProperty(ACClass currentACClass)
        {
            List<IACType> list = null;
            if (currentACClass != null)
            {
                list = currentACClass.Properties.Where(c =>
                 c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConfigPointProperty ||
                 c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConfigPointConfig).Select(c => c as IACType).ToList();
            }
            list.Add(currentACClass);
            if (list.Any())
                ConfigPointACClassProperty = list.First();
            else
                ConfigPointACClassProperty = null;
        }

        /// <summary>
        /// TabConfiguration
        /// </summary>
        Global.ControlModes _ControlModePointProperty = Global.ControlModes.Collapsed;
        /// <summary>
        /// Gets or sets the control mode point property.
        /// </summary>
        /// <value>The control mode point property.</value>
        [ACPropertyInfo(9999)]
        public Global.ControlModes ControlModePointProperty
        {
            get
            {
                return _ControlModePointProperty;
            }
            set
            {
                //                if (_ControlModePointProperty != value)
                {
                    _ControlModePointProperty = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// TabConfiguration
        /// </summary>
        Global.ControlModes _ControlModePointConfig = Global.ControlModes.Collapsed;
        /// <summary>
        /// Gets or sets the control mode point config.
        /// </summary>
        /// <value>The control mode point config.</value>
        [ACPropertyInfo(9999)]
        public Global.ControlModes ControlModePointConfig
        {
            get
            {
                return _ControlModePointConfig;
            }
            set
            {
                //                if (_ControlModePointConfig != value)
                {
                    _ControlModePointConfig = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #endregion

        #region ACMethod

        #region ACMethod -> General

        /// <summary>
        /// Saves this instance.
        /// </summary>
        [ACMethodCommand(ACClass.ClassName, "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost, Description =
                         "Saves this instance.")]
        public void Save()
        {
            if (!PreExecute()) return;
            OnSave();
            PostExecute();
        }

        public bool IsEnabledSave()
        {
            return OnIsEnabledSave();
        }

        /// <summary>
        /// Undoes the save.
        /// </summary>
        [ACMethodCommand("Partslist", "en{'Undo'}de{'Rückgängig'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost, Description =
                         "Undoes the save.")]
        public void UndoSave()
        {
            if (!PreExecute()) return;
            OnUndoSave();
            PostExecute();
        }

        public bool IsEnabledUndoSave()
        {
            return OnIsEnabledUndoSave();
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        [ACMethodInteraction(ACClass.ClassName, "en{'Load'}de{'Laden'}", (short)MISort.Load, false, nameof(SelectedCustomList), Global.ACKinds.MSMethodPrePost)]
        public void Load(bool requery = false)
        {
            if (!PreExecute("Load"))
                return;

            LoadEntity<ACClass>(requery, () => SelectedCustomList, () => CurrentCustomList, c => CurrentCustomList = c,
                        (Database as gip.core.datamodel.Database).ACClass
                        .Where(c => c.ACClassID == SelectedCustomList.ACClassID));

            PostExecute("Load");
        }

        /// <summary>
        /// Searches this instance.
        /// </summary>
        [ACMethodCommand(ACClass.ClassName, "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            if (AccessPrimary != null)
            {
                AccessPrimary.NavSearch(Database, MergeOption.OverwriteChanges);
            }
            OnPropertyChanged(nameof(CustomListList));
        }

        #endregion

        #region ACMethod -> Edit config

        /// <summary>
        /// News the point config.
        /// </summary>
        [ACMethodInteraction(nameof(PointConfig), "en{'New Config'}de{'Neue Konfiguration'}", (short)MISort.New, true, nameof(CurrentPointConfig), Global.ACKinds.MSMethodPrePost)]
        public void NewPointConfig()
        {
            IACConfigStore acConfigHandler = ConfigPointACClassProperty as IACConfigStore;
            ACClassConfig acConfig = acConfigHandler.NewACConfig(CurrentCustomList) as ACClassConfig;
            Database database = acConfig.GetObjectContext<Database>();
            ACClassConfig otherConfig = PointConfigList.Where(c => c.ACClassConfigID != acConfig.ACClassConfigID).OrderBy(c => c.InsertDate).FirstOrDefault();
            if (otherConfig != null)
            {
                acConfig.ValueTypeACClass = otherConfig.ValueTypeACClass;
            }
            else
            {
                acConfig.ValueTypeACClass = database.GetACType(typeof(Int32)) as ACClass;
                acConfig.Value = 0;
            }
            acConfig.LocalConfigACUrl = $"{nameof(ACValueItem)}_";
            acConfig.Comment = "";
            acConfig.ACCaption = " ";
            OnPropertyChanged(nameof(PointConfigList));
            CurrentPointConfig = acConfig;
            PostExecute(nameof(NewPointConfig));
        }

        /// <summary>
        /// Determines whether [is enabled new point config].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new point config]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewPointConfig()
        {
            if (CurrentCustomList == null || ConfigPointACClassProperty == null || !(ConfigPointACClassProperty is IACConfigStore))
                return false;

            return true;
        }

        /// <summary>
        /// Deletes the point config.
        /// </summary>
        [ACMethodInteraction(nameof(PointConfig), "en{'Delete configuration'}de{'Konfiguration löschen'}", (short)MISort.Delete, true, nameof(CurrentPointConfig), Global.ACKinds.MSMethodPrePost)]
        public void DeletePointConfig()
        {
            if (!IsEnabledDeletePointConfig())
                return;

            IACConfigStore acConfigHandler = ConfigPointACClassProperty as IACConfigStore;
            acConfigHandler.RemoveACConfig(CurrentPointConfig);
            CurrentCustomList.RemoveACConfig(CurrentPointConfig);
            OnPropertyChanged(nameof(PointConfigList));
        }

        /// <summary>
        /// Determines whether [is enabled delete point config].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete point config]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeletePointConfig()
        {
            if (ConfigPointACClassProperty == null || !(ConfigPointACClassProperty is IACConfigStore))
                return false;
            return CurrentCustomList != null && CurrentPointConfig != null;
        }

        protected override Msg OnPreSave()
        {
            Msg baseResult = base.OnPreSave();
            if (CurrentCustomList != null)
            {
                var duplicates = PointConfigList.GroupBy(c => c.LocalConfigACUrl).Where(c => c.Count() > 1);
                if(duplicates.Any())
                {
                    string duplicateUrls = string.Join(", ", duplicates.Select(g => g.Key));
                    // Error50758
                    // BSOCustomValueItemList
                    // Not allowed to have multiple configurations with the same name. Duplicated configuration names: {0}
                    // Nicht erlaubt, mehrere Konfigurationen mit demselben Namen zu haben. Doppelte Konfigurationsnamen: {0}
                    baseResult = new Msg { ACIdentifier = this.ACCaption, Message = Root.Environment.TranslateMessage(this, "Error50758"), MessageLevel = eMsgLevel.Error };
                }

            }
            return baseResult;
        }

        public void UpdateConfigPointACClassPropertyLayout()
        {

            if (ConfigPointACClassProperty == null)
            {
                ControlModePointProperty = Global.ControlModes.Collapsed;
                ControlModePointConfig = Global.ControlModes.Collapsed;
            }
            else
            {
                if (ConfigPointACClassProperty is ACClass)
                {
                    ControlModePointProperty = Global.ControlModes.Collapsed;
                    ControlModePointConfig = Global.ControlModes.Enabled;
                }
                else
                {
                    switch ((ConfigPointACClassProperty as ACClassProperty).ACPropUsage)
                    {
                        case Global.ACPropUsages.ConfigPointConfig:
                            ControlModePointProperty = Global.ControlModes.Collapsed;
                            ControlModePointConfig = Global.ControlModes.Enabled;
                            break;
                        case Global.ACPropUsages.ConfigPointProperty:
                            ControlModePointProperty = Global.ControlModes.Enabled;
                            ControlModePointConfig = Global.ControlModes.Collapsed;
                            break;
                        default:
                            ControlModePointProperty = Global.ControlModes.Collapsed;
                            ControlModePointConfig = Global.ControlModes.Collapsed;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <param name="vbContent">Content of the vb.</param>
        /// <param name="value">The value.</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns>Msg.</returns>
        [ACMethodInfo(nameof(ValidateInput), "en{'Validate input'}de{'Überprüfe Eingabe'}", 9999, false)]
        public Msg ValidateInput(string vbContent, object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new Msg() { MessageLevel = eMsgLevel.Info };
            }
            else
            {
                String strValue = value as String;
                if (String.IsNullOrEmpty(strValue))
                {
                    return new Msg() { MessageLevel = eMsgLevel.Info };
                }
                if (strValue.ContainsACUrlDelimiters() || strValue.Contains(" "))
                {
                    // Warning50095
                    // BSOCustomValueItemList
                    // The ACIdentifier (Designname) mustn't contain special characters, which are used for ACURLCommand
                    // Der ACIdentifier (Designname) darf keine Sonder- und Leerzeichen enthalten, die für die ACURLCommand verwendet werden
                    Msg msg = new Msg { ACIdentifier = this.ACCaption, Message = Root.Environment.TranslateMessage(this, "Warning50095"), MessageLevel = eMsgLevel.Warning };
                    return msg;
                }
            }
            return new Msg() { MessageLevel = eMsgLevel.Info };
        }

        #endregion


        #endregion
    }
}
