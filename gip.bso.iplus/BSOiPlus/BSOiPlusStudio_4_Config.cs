// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="BSOiPlusStudio_4_Config.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSOiPlusStudio
    /// </summary>
    public partial class BSOiPlusStudio
    {
        #region BSO->ACProperty
        /// <summary>
        /// Gets or sets the current composition.
        /// </summary>
        /// <value>The current composition.</value>
        [ACPropertyCurrent(9999, "ConfigComposition")]
        public ACClass CurrentComposition
        {
            get
            {
                if (CurrentPointConfig == null)
                    return null;
                var currentPointConfig = CurrentPointConfig[Const.Value] as ACComposition;
                if (currentPointConfig == null)
                    return null;
                return currentPointConfig.GetComposition(Database.ContextIPlus) as ACClass;
            }
            set
            {
                if (CurrentPointConfig == null)
                    return;
                var currentPointConfig = CurrentPointConfig[Const.Value] as ACComposition;
                if (currentPointConfig == null)
                    return;
                currentPointConfig.SetComposition(value);
            }
        }

        /// <summary>
        /// Gets the composition AC class list.
        /// </summary>
        /// <value>The composition AC class list.</value>
        [ACPropertyList(9999, "ConfigComposition")]
        public IEnumerable<ACClass> CompositionACClassList
        {
            get
            {
                return Database.ContextIPlus.ACClass.Where(c => c.ACKindIndex == (Int16)Global.ACKinds.TACQRY || c.ACKindIndex == (Int16)Global.ACKinds.TACBSO).OrderBy(c => c.ACIdentifier).ToList();
            }
        }

        #region 1.1.2.3 ConfigPointACClassProperty
        /// <summary>
        /// The _ current config point AC class property
        /// </summary>
        IACType _CurrentConfigPointACClassProperty;
        /// <summary>
        /// Gets or sets the current config point AC class property.
        /// </summary>
        /// <value>The current config point AC class property.</value>
        [ACPropertyCurrent(9999, "ConfigPointACClassProperty", "en{'Configtype'}de{'Konfiguration'}")]
        public IACType CurrentConfigPointACClassProperty
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
                    OnPropertyChanged("CurrentConfigPointACClassProperty");
                    OnPropertyChanged("ConfigACClassPropertyList");
                    OnPropertyChanged("PointConfigList");
                    UpdateConfigPointACClassPropertyLayout();
                }
            }
        }

        /// <summary>
        /// Gets the config point AC class property list.
        /// </summary>
        /// <value>The config point AC class property list.</value>
        [ACPropertyList(9999, "ConfigPointACClassProperty")]
        public IEnumerable<IACType> ConfigPointACClassPropertyList
        {
            get
            {
                if (CurrentACClass == null)
                    return null;
                List<IACType> _ConfigPointACClassPropertyList = CurrentACClass.Properties.Where(c => 
                    c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConfigPointProperty || 
                    c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConfigPointConfig).Select(c => c as IACType).ToList();
                _ConfigPointACClassPropertyList.Add(CurrentACClass);
                return _ConfigPointACClassPropertyList;
            }
        }
        #endregion

        #region 1.1.2.3.1 ConfigPointProperty
        /// <summary>
        /// The _ current config AC class property
        /// </summary>
        ACClassProperty _CurrentConfigACClassProperty;
        /// <summary>
        /// Gets or sets the current config AC class property.
        /// </summary>
        /// <value>The current config AC class property.</value>
        [ACPropertyCurrent(9999, "ConfigACClassProperty")]
        public ACClassProperty CurrentConfigACClassProperty
        {
            get
            {
                return _CurrentConfigACClassProperty;
            }
            set
            {
                _CurrentConfigACClassProperty = value;
                OnPropertyChanged("CurrentConfigACClassProperty");
                //OnPropertyChanged("CurrentPointPropertyLayout");
            }
        }

        /// <summary>
        /// Gets the config AC class property list.
        /// </summary>
        /// <value>The config AC class property list.</value>
        [ACPropertyList(9999, "ConfigACClassProperty")]
        public IEnumerable<ACClassProperty> ConfigACClassPropertyList
        {
            get
            {
                if (CurrentConfigPointACClassProperty == null || !(CurrentConfigPointACClassProperty is ACClassProperty))
                    return null;
                ACClassProperty acClassProperty = CurrentConfigPointACClassProperty as ACClassProperty;

                // Test
                var x  = this.CurrentACClass.ACClassProperty_ACClass.Where(c => c.ParentACClassPropertyID == acClassProperty.ACClassPropertyID).ToList();
                if (acClassProperty.ACPropUsage != Global.ACPropUsages.ConfigPointProperty || acClassProperty.ConfigACClass == null)
                    return null;

                return this.CurrentACClass.ACClassProperty_ACClass.Where(c => c.ParentACClassPropertyID == acClassProperty.ACClassPropertyID).OrderBy(c => c.SortIndex);
            }
        }
        #endregion

        #region 1.1.2.3.2 ConfigPointConfig
        /// <summary>
        /// The _ current point config
        /// </summary>
        ACClassConfig _CurrentPointConfig;
        /// <summary>
        /// Gets or sets the current point config.
        /// </summary>
        /// <value>The current point config.</value>
        [ACPropertyCurrent(9999, "ACConfig")]
        public ACClassConfig CurrentPointConfig
        {
            get
            {
                return _CurrentPointConfig;
            }
            set
            {
                if (_CurrentPointConfig != null)
                    _CurrentPointConfig.PropertyChanged -= _CurrentPointConfig_PropertyChanged;

                _CurrentPointConfig = value;
                OnPropertyChanged("CurrentPointConfig");
                OnPropertyChanged("CurrentPointConfigLayout");

                if (_CurrentPointConfig != null)
                    _CurrentPointConfig.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_CurrentPointConfig_PropertyChanged); 
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the _CurrentPointConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void _CurrentPointConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConfigACClassID":
                case "ValueTypeACClassID":
                    {
                        ACClassConfig acClassConfig = sender as ACClassConfig;
                        acClassConfig.SetDefaultValue();
                        OnPropertyChanged("CurrentPointConfigLayout");
                        OnPropertyChanged("CurrentPointConfig");
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets the point config list.
        /// </summary>
        /// <value>The point config list.</value>
        [ACPropertyList(9999, "ACConfig")]
        public IEnumerable<ACClassConfig> PointConfigList
        {
            get
            {
                if (CurrentACClass == null || CurrentConfigPointACClassProperty == null)
                    return null;

                return CurrentConfigPointACClassProperty.GetConfigListOfType(CurrentACClass).Select(c => c as ACClassConfig).ToList();
            }
        }
        #endregion

        #endregion

        #region BSO->ACMethod
        #region 1.1.2.3.1 ConfigPointProperty
        /// <summary>
        /// News the config AC class property.
        /// </summary>
        [ACMethodInfo("ConfigACClassProperty", "en{'New Configuration'}de{'Neue Konfiguration'}", (short)MISort.New, true)]
        public void NewConfigACClassProperty()
        {
            if (!PreExecute("NewConfigACClassProperty")) return;

            ACClassProperty pointACClassProperty = CurrentConfigPointACClassProperty as ACClassProperty;

            // Einfügen einer neuen Eigenschaft und der aktuellen Eigenschaft zuweisen
            ACClassProperty acClassProperty = ACClassProperty.NewACObject(Database.ContextIPlus, CurrentACClass);
            acClassProperty.ConfigACClass = pointACClassProperty.ConfigACClass;
            acClassProperty.ACClassProperty1_ParentACClassProperty = pointACClassProperty;

            Type t = acClassProperty.ConfigACClass.ObjectType;
            var mi = t.GetMethod("InitProperty", Global.bfInvokeMethodStatic);
            if (mi != null)
            {
                mi.Invoke(t, new object[] { Database.ContextIPlus, acClassProperty });
            }

            OnPropertyChanged("ConfigACClassPropertyList");
            PostExecute("NewConfigACClassProperty");
            // Creates a Configuration-Value, don't remove var x
            var x = acClassProperty.ConfigValue;
            CurrentConfigACClassProperty = acClassProperty;
        }

        /// <summary>
        /// Determines whether [is enabled new config AC class property].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new config AC class property]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewConfigACClassProperty()
        {

            if (CurrentConfigPointACClassProperty == null || !(CurrentConfigPointACClassProperty is ACClassProperty))
                return false;

            ACClassProperty acClassProperty = CurrentConfigPointACClassProperty as ACClassProperty;
            return acClassProperty.ConfigACClass != null && acClassProperty.ACPropUsage == Global.ACPropUsages.ConfigPointProperty;
        }

        /// <summary>
        /// Deletes the config AC class property.
        /// </summary>
        [ACMethodInfo("ConfigACClassProperty", "en{'Delete Configuration'}de{'Konfiguration löschen'}", (short)MISort.Delete, true)]
        public void DeleteConfigACClassProperty()
        {
            if (!PreExecute("DeleteConfigACClassProperty")) return;
            Msg msg = CurrentConfigACClassProperty.DeleteACObject(Database.ContextIPlus, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }

            OnPropertyChanged("ConfigACClassPropertyList");
            PostExecute("DeleteConfigACClassProperty");
        }

        /// <summary>
        /// Determines whether [is enabled delete config AC class property].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete config AC class property]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteConfigACClassProperty()
        {
            return CurrentConfigACClassProperty != null;
        }
        #endregion

        #region 1.1.2.3.2 ConfigPointConfig
        /// <summary>
        /// News the point config.
        /// </summary>
        [ACMethodInteraction("PointConfig", "en{'New Config'}de{'Neue Konfiguration'}", (short)MISort.New, true, "CurrentPointConfig", Global.ACKinds.MSMethodPrePost)]
        public void NewPointConfig()
        {
            if (!PreExecute("NewPointConfig")) return;
            // Einfügen einer neuen Eigenschaft und der aktuellen Eigenschaft zuweisen
            IACConfigStore acConfigHandler = CurrentConfigPointACClassProperty as IACConfigStore;
            ACClassConfig acConfig = acConfigHandler.NewACConfig(CurrentACClass) as ACClassConfig;
            Database database = acConfig.GetObjectContext<Database>();
            acConfig.ValueTypeACClass = database.GetACType(typeof(Int32)) as ACClass;
            acConfig.Value = 0;

            OnPropertyChanged("PointConfigList");
            CurrentPointConfig = acConfig;
            PostExecute("NewPointConfig");
        }

        /// <summary>
        /// Determines whether [is enabled new point config].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new point config]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewPointConfig()
        {
            if (CurrentACClass == null || CurrentConfigPointACClassProperty == null || !(CurrentConfigPointACClassProperty is IACConfigStore))
                return false;

            return true;
        }

        /// <summary>
        /// Deletes the point config.
        /// </summary>
        [ACMethodInteraction("PointConfig", "en{'Delete configuration'}de{'Konfiguration löschen'}", (short)MISort.Delete, true, "CurrentPointConfig", Global.ACKinds.MSMethodPrePost)]
        public void DeletePointConfig()
        {
            if (!PreExecute("DeletePointConfig"))
                return;
            if (!IsEnabledDeletePointConfig())
                return;

            IACConfigStore acConfigHandler = CurrentConfigPointACClassProperty as IACConfigStore;
            acConfigHandler.RemoveACConfig(CurrentPointConfig);
            CurrentACClass.RemoveACConfig(CurrentPointConfig);

            //Msg msg = CurrentPointConfig.DeleteACObject(Database.ContextIPlus, true);
            //if (msg != null)
            //{
            //    Messages.Msg(msg);
            //    return;
            //}

            OnPropertyChanged("PointConfigList");
            PostExecute("DeletePointConfig");
        }

        /// <summary>
        /// Determines whether [is enabled delete point config].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete point config]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeletePointConfig()
        {
            if (CurrentConfigPointACClassProperty == null || !(CurrentConfigPointACClassProperty is IACConfigStore))
                return false;
            return CurrentACClass != null && CurrentPointConfig != null;
        }
        #endregion
        #endregion

        #region Layout und Propertychanged
        #region TabConfiguration
        /// <summary>
        /// Updates the config point AC class property layout.
        /// </summary>
        public void UpdateConfigPointACClassPropertyLayout()
        {

            if (CurrentConfigPointACClassProperty == null)
            {
                ControlModePointProperty = Global.ControlModes.Collapsed;
                ControlModePointConfig = Global.ControlModes.Collapsed;
            }
            else
            {
                if (CurrentConfigPointACClassProperty is ACClass)
                {
                    ControlModePointProperty = Global.ControlModes.Collapsed;
                    ControlModePointConfig = Global.ControlModes.Enabled;
                }
                else
                {
                    switch ((CurrentConfigPointACClassProperty as ACClassProperty).ACPropUsage)
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
        /// Gets the current point config layout.
        /// </summary>
        /// <value>The current point config layout.</value>
        public string CurrentPointConfigLayout
        {
            get
            {
                if (CurrentPointConfig == null)
                    return LayoutHelper.VBDockPanelEmpty();

                string xaml = LayoutHelper.VBDockPanelBegin();
                xaml += LayoutHelper.VBGridBegin("", "*", "30,30,30,30,30,30,30,30,30,30,*");

                var x = CurrentPointConfig.ValueTypeACClass.GetDesign(CurrentPointConfig.ValueTypeACClass, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                if (CurrentPointConfig.ValueTypeACClass != null)
                {
                    switch (CurrentPointConfig.ValueTypeACClass.ObjectType.Name)
                    {
                        case Const.TNameInt16:
                        case Const.TNameInt32:
                        case Const.TNameInt64:
                        case Const.TNameDouble:
                        case Const.TNameSingle:
                        case Const.TNameString:
                            xaml += LayoutHelper.VBGridAdd("<vb:VBTextBox VBContent=\"CurrentPointConfig\\Value\"  Grid.Row=\"4\" ACCaption=\"" + CurrentPointConfig.LocalConfigACUrl + "\"></vb:VBTextBox>");
                            break;
                        case Const.TNameBoolean:
                            xaml += LayoutHelper.VBGridAdd("<vb:VBCheckBox VBContent=\"CurrentPointConfig\\Value\"  Grid.Row=\"4\" ACCaption=\"" + CurrentPointConfig.LocalConfigACUrl + "\"></vb:VBCheckBox>");
                            break;
                        case Const.TNameDateTime:
                            xaml += LayoutHelper.VBGridAdd("<vb:VBDateTimePicker VBContent=\"CurrentPointConfig\\Value\"  Grid.Row=\"4\" ACCaption=\"" + CurrentPointConfig.LocalConfigACUrl + "\"></vb:VBDateTimePicker>");
                            break;
                        case "ACComposition":
                            xaml += LayoutHelper.VBGridAdd("<vb:VBComboBox Grid.Row=\"4\" ACCaption=\"Reference\" VBContent=\"CurrentComposition\" VBSource=\"CompositionACClassList\"></vb:VBComboBox>");
                            xaml += LayoutHelper.VBGridAdd("<vb:VBTextBox  Grid.Row=\"5\" ACCaption=\"Appendix\" VBContent=\"CurrentPointConfig\\Value\\Appendix\"></vb:VBTextBox>");
                            xaml += LayoutHelper.VBGridAdd("<vb:VBCheckBox Grid.Row=\"6\" ACCaption=\"Assembly\" VBContent=\"CurrentPointConfig\\Value\\IsSystem\" DisabledModes=\"Disabled\"></vb:VBCheckBox>");
                            xaml += LayoutHelper.VBGridAdd("<vb:VBCheckBox Grid.Row=\"7\" ACCaption=\"Primary\" VBContent=\"CurrentPointConfig\\Value\\IsPrimary\" DisabledModes=\"Disabled\"></vb:VBCheckBox>");
                            break;
                        default:
                            if (typeof(VBEntityObject).IsAssignableFrom(CurrentPointConfig.ValueTypeACClass.ObjectType))
                            {
                                xaml += LayoutHelper.VBGridAdd("<vb:VBComboBox VBContent=\"CurrentPointConfig\\Value\"  Grid.Row=\"4\" ACCaption=\"" + CurrentPointConfig.LocalConfigACUrl + "\" VBSource=\"" + Const.ContextDatabase + "\\" + CurrentPointConfig.ValueTypeACClass.ObjectType.Name + "\"></vb:VBComboBox>");
                            }
                            break;
                    }
                }

                xaml += LayoutHelper.VBGridEnd();
                xaml += LayoutHelper.VBDockPanelEnd();

                return xaml;
            }
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
                    OnPropertyChanged("ControlModePointProperty");
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
                    OnPropertyChanged("ControlModePointConfig");
                }
            }
        }

        #endregion

        //private ACClass _LastPropertyExtACClass = null;
        //public string CurrentPointPropertyLayout
        //{
        //    get
        //    {
        //        if (CurrentACClass == null || CurrentConfigACClassProperty == null)
        //        {
        //            _LastPropertyExtACClass = null;
        //            return LayoutHelper.VBDockPanelEmpty();
        //        }

        //        _LastPropertyExtACClass = CurrentConfigACClassProperty.ConfigACClass;
        //        if (_LastPropertyExtACClass == null)
        //            return LayoutHelper.VBDockPanelEmpty();

        //        if ((ACClass.GetACType(typeof(ACClassProperty)).MyACClassPropertyExtList == null) || !(ACClass.GetACType(typeof(ACClassProperty)).MyACClassPropertyExtList.Any()))
        //            return LayoutHelper.VBDockPanelEmpty();
        //        var query = ACClass.GetACType(typeof(ACClassProperty)).MyACClassPropertyExtList.Where(c => c.ValueTypeACClass.ACClassID == _LastPropertyExtACClass.ACClassID);
        //        if (query.Any())
        //        {
        //            var query2 = _LastPropertyExtACClass.ACClassDesign_ACClass.Where(c => c.ACUsageIndex == (int)Global.ACUsages.DUControl);
        //            if (query2.Any())
        //                return query2.First().XMLDesign;
        //        }
        //        return LayoutHelper.VBDockPanelEmpty();
        //    }
        //}
        #endregion
    }
}
