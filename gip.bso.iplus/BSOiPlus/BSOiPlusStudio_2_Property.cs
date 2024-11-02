// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 12-17-2012
// ***********************************************************************
// <copyright file="BSOiPlusStudio_2_Property.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.manager;
using gip.core.autocomponent;
using System.ComponentModel;
using System.Collections.ObjectModel;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSOiPlusStudio
    /// </summary>
    public partial class BSOiPlusStudio
    {
        #region BSO->ACProperty
        #region 1.1.2 ACClassProperty
        /// <summary>
        /// The _ access AC class property
        /// </summary>
        ACAccess<ACClassProperty> _AccessACClassProperty;
        /// <summary>
        /// Gets the access AC class property.
        /// </summary>
        /// <value>The access AC class property.</value>
        [ACPropertyAccess(9999, "ACClassProperty")]
        public ACAccess<ACClassProperty> AccessACClassProperty
        {
            get
            {
                if (_AccessACClassProperty == null && ACType != null)
                {
                    ACQueryDefinition acQueryDefinition = AccessACClass.NavACQueryDefinition.ACUrlCommand(Const.QueryPrefix + ACClassProperty.ClassName) as ACQueryDefinition;
                    _AccessACClassProperty = acQueryDefinition.NewAccess<ACClassProperty>(ACClassProperty.ClassName, this);
                    if (BSOiPlusStudio._ColumnsProperties == null)
                    {
                        BSOiPlusStudio._ColumnsProperties = _AccessACClassProperty.NavACQueryDefinition.ACColumnsAsString;
                    }
                }
                return _AccessACClassProperty;
            }
        }

        /// <summary>
        /// The _ current AC class property
        /// </summary>
        ACClassProperty _CurrentACClassProperty;
        /// <summary>
        /// Gets or sets the current AC class property.
        /// </summary>
        /// <value>The current AC class property.</value>
        [ACPropertyCurrent(9999, "ACClassProperty")]
        public ACClassProperty CurrentACClassProperty
        {
            get
            {
                return _CurrentACClassProperty;
            }
            set
            {
                //if (_CurrentACClassProperty != value)
                {
                    if (value != null)
                    {
                        // Wenn in dieser ACClass überschrieben, dann das überschriebene ACClassProperty verwenden
                        Guid basedACClassPropertyID = value.ACClassProperty1_BasedOnACClassProperty.ACClassPropertyID;
                        ACClassProperty acClassProp = CurrentACClass.ACClassProperty_ACClass.FirstOrDefault(c => c.BasedOnACClassPropertyID == basedACClassPropertyID);
                        if (acClassProp != null)
                            value = acClassProp;
                    }
                    if (CurrentACClassPropertyTemp != null)
                        CurrentACClassPropertyTemp.PropertyChanged -= CurrentACClassPropertyTemp_PropertyChanged;
                    if (CurrentACClassProperty != null)
                        CurrentACClassProperty.PropertyChanged -= CurrentACClassProperty_PropertyChanged;
                    if (CurrentPropBindingToSource != null)
                        CurrentPropBindingToSource.PropertyChanged -= CurrentPropBindingToSource_PropertyChanged;

                    _CurrentACClassProperty = value;

                    if (_CurrentACClassProperty != null)
                    {
                        _CurrentACClassPropertyTemp = new ACClassProperty();
                        _CurrentACClassPropertyTemp.ACClassPropertyID = _CurrentACClassProperty.ACClassPropertyID;
                        _CurrentACClassPropertyTemp.CopyBaseValues(_CurrentACClassProperty);
                        Database.ContextIPlus.Detach(_CurrentACClassPropertyTemp);
                        if (CurrentACClass != null)
                            CurrentPropBindingToSource = _CurrentACClassProperty.GetMyPropertyBindingToSource(CurrentACClass);
                        else
                            CurrentPropBindingToSource = null;
                    }
                    else
                    {
                        _CurrentACClassPropertyTemp = null;
                        CurrentPropBindingToSource = null;
                    }


                    _ACClassPropertyControlList = null;
                    _ACClassPropertyIconList = null;
                    if (value == null)
                    {
                        _CurrentACClassPropertyControl = null;
                        _CurrentACClassPropertyIcon = null;
                    }
                    else
                    {
                        _CurrentACClassPropertyControl = _CurrentACClassProperty.GetDesign(_CurrentACClassProperty, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                        _CurrentACClassPropertyIcon = _CurrentACClassProperty.GetDesign(_CurrentACClassProperty, Global.ACUsages.DUIcon, Global.ACKinds.DSBitmapResource);
                    }
                    if (CurrentACClassPropertyTemp != null)
                    {
                        CurrentACClassPropertyTemp.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CurrentACClassPropertyTemp_PropertyChanged);
                    }
                    if (CurrentACClassProperty != null)
                    {
                        CurrentACClassProperty.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CurrentACClassProperty_PropertyChanged);
                    }
                    if (CurrentPropBindingToSource != null)
                        CurrentPropBindingToSource.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CurrentPropBindingToSource_PropertyChanged);

                    OnPropertyChanged("CurrentACClassProperty");
                    OnPropertyChanged("CurrentACClassPropertyTemp");
                    OnPropertyChanged("CurrentOnlineValue");
                    OnPropertyChanged("CurrentACClassPropertyControl");
                    OnPropertyChanged("CurrentACClassPropertyIcon");
                    OnPropertyChanged("ACClassPropertyControlList");
                    OnPropertyChanged("ACClassPropertyIconList");
                    OnPropertyChanged("PropertyRelationFromList");
                    OnPropertyChanged("PropertyRelationToList");
                    OnPropertyChanged("ConfigACClassPropertyList");
                    OnPropertyChanged("PointStateInfoList");
                }
            }
        }

        /// <summary>
        /// The _ AC class property list
        /// </summary>
        ObservableCollection<ACClassProperty> _ACClassPropertyList = null;
        /// <summary>
        /// Gets the AC class property list.
        /// </summary>
        /// <value>The AC class property list.</value>
        [ACPropertyList(9999, "ACClassProperty")]
        public IEnumerable<ACClassProperty> ACClassPropertyList
        {
            get
            {
                if (CurrentACClass == null)
                    return null;
                if (_ACClassPropertyList == null)
                {
                    switch (CurrentPropertyModeEnum)
                    {
                        //case PropertyModes.Configuration:
                        //    return _CurrentPBSourceACClass.ACClassPropertyTopBaseList.Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConfigPointProperty);
                        case Global.PropertyModes.Connections:
                            _ACClassPropertyList = new ObservableCollection<ACClassProperty>(CurrentACClass.Properties.Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConnectionPoint));
                            break;
                        case Global.PropertyModes.Events:
                            _ACClassPropertyList = new ObservableCollection<ACClassProperty>(CurrentACClass.Properties.Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.EventPoint || c.ACPropUsageIndex == (Int16)Global.ACPropUsages.EventPointSubscr));
                            break;
                        case Global.PropertyModes.Bindings:
                            _ACClassPropertyList = new ObservableCollection<ACClassProperty>(CurrentACClass.Properties.Where(c => c.IsProxyProperty && c.IsBroadcast));
                            break;
                        //case Global.PropertyModes.Properties:
                        //case Global.PropertyModes.Relations:
                        //case Global.PropertyModes.Joblists:
                        default:
                            _ACClassPropertyList = new ObservableCollection<ACClassProperty>(CurrentACClass.Properties);
                            break;
                    }

                    CurrentACClassProperty = _ACClassPropertyList.FirstOrDefault();
                }
                return _ACClassPropertyList;
            }
        }

        /// <summary>
        /// The _ current AC class property temp
        /// </summary>
        ACClassProperty _CurrentACClassPropertyTemp = null;
        /// <summary>
        /// CurrentACClassPropertyTemp dient zum überschreiben von überschreiben von folgenden ACClassProperty-Attributen:
        /// -InputMask
        /// -Value
        /// -LogRefreshRateIndex
        /// -LogFilter
        /// -MinLength
        /// -MaxLength
        /// -MinValue
        /// -MaxValue
        /// </summary>
        /// <value>The current AC class property temp.</value>
        [ACPropertyInfo(9999)]
        public ACClassProperty CurrentACClassPropertyTemp
        {
            get
            {
                return _CurrentACClassPropertyTemp;
            }
        }

        /// <summary>
        /// The _ current AC class property control
        /// </summary>
        ACClassDesign _CurrentACClassPropertyControl;
        /// <summary>
        /// Gets or sets the current AC class property control.
        /// </summary>
        /// <value>The current AC class property control.</value>
        [ACPropertyCurrent(9999, "ACClassPropertyControl", "en{'Control'}de{'Steuerelement'}")]
        public ACClassDesign CurrentACClassPropertyControl
        {
            get
            {
                return _CurrentACClassPropertyControl;
            }
            set
            {
                _CurrentACClassPropertyControl = value;
                ProjectManager.SetDefaultPropertyACClassDesign(CurrentACClassProperty, value, Global.ACKinds.DSDesignLayout, Global.ACUsages.DUControl);
                OnPropertyChanged("CurrentACClassPropertyControl");
            }
        }

        /// <summary>
        /// The _ AC class property control list
        /// </summary>
        List<ACClassDesign> _ACClassPropertyControlList = null;
        /// <summary>
        /// Gets the AC class property control list.
        /// </summary>
        /// <value>The AC class property control list.</value>
        [ACPropertyList(9999, "ACClassPropertyControl")]
        public IEnumerable<ACClassDesign> ACClassPropertyControlList
        {
            get
            {
                if (CurrentACClass == null || CurrentACClassProperty == null)
                    return null;
                if (_ACClassPropertyControlList == null)
                {
                    _ACClassPropertyControlList = CurrentACClassProperty.Designs.Where(c => c.ACUsageIndex == (int)Global.ACUsages.DUControl).ToList();
                }
                return _ACClassPropertyControlList;
            }
        }

        /// <summary>
        /// The _ current AC class property icon
        /// </summary>
        ACClassDesign _CurrentACClassPropertyIcon;
        /// <summary>
        /// Gets or sets the current AC class property icon.
        /// </summary>
        /// <value>The current AC class property icon.</value>
        [ACPropertyCurrent(9999, "ACClassPropertyIcon", "en{'Icon'}de{'Symbol'}")]
        public ACClassDesign CurrentACClassPropertyIcon
        {
            get
            {
                return _CurrentACClassPropertyIcon;
            }
            set
            {
                _CurrentACClassPropertyIcon = value;
                ProjectManager.SetDefaultPropertyACClassDesign(CurrentACClassProperty, value, Global.ACKinds.DSBitmapResource, Global.ACUsages.DUIcon);
                OnPropertyChanged("CurrentACClassPropertyIcon");
            }
        }

        /// <summary>
        /// The _ AC class property icon list
        /// </summary>
        List<ACClassDesign> _ACClassPropertyIconList = null;
        /// <summary>
        /// Gets the AC class property icon list.
        /// </summary>
        /// <value>The AC class property icon list.</value>
        [ACPropertyList(9999, "ACClassPropertyIcon")]
        public IEnumerable<ACClassDesign> ACClassPropertyIconList
        {
            get
            {
                if (CurrentACClass == null || CurrentACClassProperty == null)
                    return null;
                if (_ACClassPropertyIconList == null)
                {
                    var myACClassDesignList = CurrentACClassProperty.Designs;
                    if (myACClassDesignList != null)
                        _ACClassPropertyIconList = myACClassDesignList.Where(c => c.ACUsageIndex == (int)Global.ACUsages.DUIcon).ToList();
                    else
                        _ACClassPropertyIconList = new List<ACClassDesign>();
                    var query = Root.Environment.ACType.Designs.Where(c => c.ACUsageIndex == (int)Global.ACUsages.DUIcon);
                    foreach (var acClassDesign in query)
                    {
                        _ACClassPropertyIconList.Add(acClassDesign);
                    }
                }
                return _ACClassPropertyIconList;
            }
        }

        private ACClassPropertyRelation _CurrentPropBindingToSource;
        [ACPropertyCurrent(9999, "CurrentPropBindingToSource", "en{'Current binding to source'}de{'Aktuelle Bindung zur Quelle'}")]
        public ACClassPropertyRelation CurrentPropBindingToSource
        {
            get
            {
                return _CurrentPropBindingToSource;
            }
            set
            {
                _CurrentPropBindingToSource = value;
                OnPropertyChanged("CurrentPropBindingToSource");
                OnPropertyChanged("CurrentACClassProperty");
            }
        }
        #endregion

        #region 1.1.2.2 ACClassPropertyRelation
        /// <summary>
        /// Current Connection from this point to another point
        /// </summary>
        ACObjectItem _CurrentPropertyRelationTo = null;
        /// <summary>
        /// Current Connection from this point to another point
        /// </summary>
        /// <value>The current property relation.</value>
        [ACPropertyCurrent(9999, "PropertyRelationTo")]
        public ACObjectItem CurrentPropertyRelationTo
        {
            get
            {
                return _CurrentPropertyRelationTo;
            }
            set
            {
                if (_CurrentPropertyRelationTo != value)
                {
                    _CurrentPropertyRelationTo = value;
                    OnPropertyChanged("CurrentPropertyRelationTo");
                }
            }
        }

        /// <summary>
        /// Connections from this Point to other points
        /// </summary>
        List<ACObjectItem> _PropertyRelationToList = null;
        /// <summary>
        /// Connections from this Point to other points
        /// </summary>
        /// <value>The property relation list.</value>
        [ACPropertyList(9999, "PropertyRelationTo", "en{'Connections to'}de{'Beziehungen nach'}")]
        public IEnumerable<ACObjectItem> PropertyRelationToList
        {
            get
            {
                if (CurrentACClassProperty == null)
                    return null;
                _PropertyRelationToList = new List<ACObjectItem>();
                foreach (ACClassPropertyRelation acClassPropertyRelation in CurrentACClassProperty.ACClassPropertyRelation_SourceACClassProperty
                                                                            .Where(c => c.SourceACClass.ACClassID == CurrentACClass.ACClassID)
                                                                            .AsEnumerable()
                                                                            .OrderBy(c => c.ConnectionTypeIndex)
                                                                            .ThenBy(c => c.SourceACClass.GetACUrlComponent()))
                {
                    _PropertyRelationToList.Add(new ACObjectItem(acClassPropertyRelation, acClassPropertyRelation.ConnectionType.ToString()) 
                                                    { ACUrl = String.Format("{0}\\{1}", acClassPropertyRelation.TargetACClass.GetACUrlComponent(), acClassPropertyRelation.TargetACClassProperty.ACIdentifier) });
                }
                return _PropertyRelationToList;
            }
        }

        /// <summary>
        /// Current Connection from this point to another point
        /// </summary>
        ACObjectItem _CurrentPropertyRelationFrom = null;
        /// <summary>
        /// Current Connection from this point to another point
        /// </summary>
        /// <value>The current property relation.</value>
        [ACPropertyCurrent(9999, "PropertyRelationFrom")]
        public ACObjectItem CurrentPropertyRelationFrom
        {
            get
            {
                return _CurrentPropertyRelationFrom;
            }
            set
            {
                if (_CurrentPropertyRelationFrom != value)
                {
                    _CurrentPropertyRelationFrom = value;
                    OnPropertyChanged("CurrentPropertyRelationFrom");
                }
            }
        }

        /// <summary>
        /// Connections from other points to this Point
        /// </summary>
        List<ACObjectItem> _PropertyRelationFromList = null;
        /// <summary>
        /// Connections from other points to this Point
        /// </summary>
        /// <value>The property relation FromList.</value>
        [ACPropertyList(9999, "PropertyRelationFrom", "en{'Connections from'}de{'Beziehungen von'}")]
        public IEnumerable<ACObjectItem> PropertyRelationFromList
        {
            get
            {
                if (CurrentACClassProperty == null)
                    return null;
                _PropertyRelationFromList = new List<ACObjectItem>();
                foreach (ACClassPropertyRelation acClassPropertyRelation in CurrentACClassProperty.ACClassPropertyRelation_TargetACClassProperty
                                                        .Where(c => c.TargetACClass.ACClassID == CurrentACClass.ACClassID)
                                                        .AsEnumerable()
                                                        .OrderBy(c => c.ConnectionTypeIndex)
                                                        .ThenBy(c => c.SourceACClass.GetACUrlComponent()))
                {                    
                    _PropertyRelationFromList.Add(new ACObjectItem(acClassPropertyRelation, acClassPropertyRelation.ConnectionType.ToString())
                    { ACUrl = String.Format("{0}\\{1}", acClassPropertyRelation.SourceACClass.GetACUrlComponent(), acClassPropertyRelation.SourceACClassProperty.ACIdentifier) });
                }
                return _PropertyRelationFromList;
            }
        }

        /// <summary>
        /// The _ relation AC class list
        /// </summary>
        IEnumerable<ACClass> _RelationACClassList = null;
        /// <summary>
        /// Gets the relation AC class list.
        /// </summary>
        /// <value>The relation AC class list.</value>
        [ACPropertyList(9999, "RelationACClass")]
        public IEnumerable<ACClass> RelationACClassList
        {
            get
            {
                if (_RelationACClassList == null && CurrentACProject != null)
                    _RelationACClassList = CurrentACProject.ACClass_ACProject.OrderBy(c => c.ACIdentifier).ToList();
                return _RelationACClassList;
            }
        }
        #endregion

        #region 1.1.2.4 PointStateInfo
        /// <summary>
        /// The _ selected point state info
        /// </summary>
        ACClassPropertyRelation _SelectedPointStateInfo;
        /// <summary>
        /// Gets or sets the selected point state info.
        /// </summary>
        /// <value>The selected point state info.</value>
        [ACPropertySelected(9999, "PointStateInfo")]
        public ACClassPropertyRelation SelectedPointStateInfo
        {
            get
            {
                return _SelectedPointStateInfo;
            }
            set
            {
                _SelectedPointStateInfo = value;
                OnPropertyChanged("SelectedPointStateInfo");
            }
        }

        /// <summary>
        /// The _ point state info list
        /// </summary>
        List<ACClassPropertyRelation> _PointStateInfoList = null;
        /// <summary>
        /// Gets the point state info list.
        /// </summary>
        /// <value>The point state info list.</value>
        [ACPropertyList(9999, "PointStateInfo")]
        public IEnumerable<ACClassPropertyRelation> PointStateInfoList
        {
            get
            {
                if ((CurrentACClass == null) || (CurrentACClassProperty == null) || (CurrentACClassProperty.ACPropUsage != Global.ACPropUsages.ConnectionPoint))
                    return null;
                _PointStateInfoList = new List<ACClassPropertyRelation>();
                var queryPointStates = CurrentACClassProperty.TopBaseACClassProperty.ACClassPropertyRelation_SourceACClassProperty.Where(c => c.ConnectionTypeIndex == (short)Global.ConnectionTypes.PointState);
                foreach (ACClassPropertyRelation rel in queryPointStates)
                {
                    ACClass superClass = CurrentACClass;
                    while (superClass != null)
                    {
                        if (superClass == rel.SourceACClass)
                        {
                            _PointStateInfoList.Add(rel);
                            break;
                        }
                        superClass = superClass.ACClass1_BasedOnACClass;
                    }
                }
                return _PointStateInfoList;
            }
        }

        /// <summary>
        /// The _ current point state info
        /// </summary>
        ACClassPropertyRelation _CurrentPointStateInfo = null;
        /// <summary>
        /// Gets or sets the current point state info.
        /// </summary>
        /// <value>The current point state info.</value>
        [ACPropertyCurrent(9999, "PointStateInfo")]
        public ACClassPropertyRelation CurrentPointStateInfo
        {
            get
            {
                return _CurrentPointStateInfo;
            }
            set
            {
                _CurrentPointStateInfo = value;
                OnPropertyChanged("CurrentPointStateInfo");
            }
        }
        #endregion

        #region Binding
        #region Project with Source-Properties

        ACProjectManager _PBProjectManager = null; // Für Property-Binding
        protected ACProjectManager PBProjectManager
        {
            get
            {
                if (_PBProjectManager == null)
                {
                    _PBProjectManager = new ACProjectManager(Database.ContextIPlus, Root);
                    _PBProjectManager.PropertyChanged += _PBProjectManager_PropertyChanged;
                    if (CurrentPBSourceProject != null)
                        _PBProjectManager.LoadACProject_AppDefinition(CurrentPBSourceProject, PBProjectPresentationMode, PBProjectVisibilityFilter);
                }
                return _PBProjectManager;
            }
        }

        private void _PBProjectManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ACProjectManager.CurrentProjectItemRootPropName)
                OnPropertyChanged("CurrentPBSourceItemRoot");
        }


        /// <summary>
        /// Liste von Anwendungsprojekten. Nur bei Anwendungsprojekten macht ein Property-Binding Sinn
        /// "PB":PropertyBinding
        /// </summary>
        /// <value>The PB source project list.</value>
        [ACPropertyList(9999, "PBProject")]
        public IEnumerable<ACProject> PBSourceProjectList
        {
            get
            {
                return PBProjectManager.PBSourceProjectList;
            }
        }

        /// <summary>
        /// Aktuell ausgewähltes Anwendungs-Projekt, für Drag-And-Drop-Property-Binding
        /// </summary>
        /// <value>The current PB source project.</value>
        [ACPropertySelected(9999, "PBProject", "en{'Projects'}de{'Projekte'}")]
        public ACProject CurrentPBSourceProject
        {
            get
            {
                return this.PBProjectManager.CurrentACProject;
            }
            set
            {
                bool changed = PBProjectManager.CurrentACProject != value;
                if (changed)
                {
                    _SearchPBSourceItem = null;
                    OnPropertyChanged(nameof(SearchPBSourceItem));
                    _SearchACUrlPBSourceItem = null;
                    OnPropertyChanged(nameof(SearchACUrlPBSourceItem));
                    RefreshPBSourceProjectTree(value);
                }
                CurrentPBSourceItem = null;
                CurrentPBSourceACClass = null;
            }
        }

        private void RefreshPBSourceProjectTree(ACProject newProject)
        {
            if (PBProjectManager != null)
                PBProjectManager.LoadACProject(newProject != null ? newProject : PBProjectManager.CurrentACProject, PBProjectPresentationMode, PBProjectVisibilityFilter);
        }

        protected ACClassInfoWithItems.VisibilityFilters PBProjectVisibilityFilter
        {
            get
            {
                ACClassInfoWithItems.VisibilityFilters filter = new ACClassInfoWithItems.VisibilityFilters()
                            { 
                                    SearchText = this.SearchPBSourceItem,
                                    SearchACUrlComponent = this.SearchACUrlPBSourceItem
                };
                return filter;
            }
        }

        protected ACProjectManager.PresentationMode PBProjectPresentationMode
        {
            get
            {
                ACProjectManager.PresentationMode mode = new ACProjectManager.PresentationMode();
                return mode;
            }
        }


        /// <summary>
        /// Aktuell ausgewähltes Anwendungs-Projekt, als TreeEntry-Root-Objekt
        /// </summary>
        /// <value>The current PB source item root.</value>
        [ACPropertyCurrent(9999, "CurrentPBSourceItemRoot")]
        public ACClassInfoWithItems CurrentPBSourceItemRoot
        {
            get
            {
                return PBProjectManager.CurrentProjectItemRoot;
            }
        }

        /// <summary>
        /// Aktuell ausgewähltes "Source-ACClass"-Element zum "Property-Binden"
        /// </summary>
        ACClassInfoWithItems _CurrentPBSourceItem = null;
        /// <summary>
        /// Gets or sets the current PB source item.
        /// </summary>
        /// <value>The current PB source item.</value>
        [ACPropertyCurrent(9999, "CurrentPBSourceItem")]
        public ACClassInfoWithItems CurrentPBSourceItem
        {
            get
            {
                return _CurrentPBSourceItem;
            }
            set
            {
                if (_CurrentPBSourceItem != value)
                {
                    _CurrentPBSourceItem = value;
                    if (_CurrentPBSourceItem != null && CurrentPBSourceItem.ValueT != null)
                    {
                        if (CurrentPBSourceACClass != CurrentPBSourceItem.ValueT)
                        {
                            CurrentPBSourceACClass = CurrentPBSourceItem.ValueT;
                        }
                        OnPropertyChanged("CurrentPBSourceItem");
                    }
                }
            }
        }

        /// <summary>
        /// The _ search class text
        /// </summary>
        string _SearchPBSourceItem = "";
        /// <summary>
        /// Gets or sets the search class text.
        /// </summary>
        /// <value>The search class text.</value>
        [ACPropertyInfo(9999, "CLProjectItem", "en{'Search Class'}de{'Suche Klasse'}")]
        public string SearchPBSourceItem
        {
            get
            {
                return _SearchPBSourceItem;
            }
            set
            {
                if (_SearchPBSourceItem != value)
                {
                    _SearchPBSourceItem = value;
                    OnPropertyChanged();
                    RefreshPBSourceProjectTree(null);
                }
            }
        }

        /// <summary>
        /// The _ search class text
        /// </summary>
        string _SearchACUrlPBSourceItem = "";
        /// <summary>
        /// Gets or sets the search class text.
        /// </summary>
        /// <value>The search class text.</value>
        [ACPropertyInfo(9999, "CLProjectItem", "en{'Search ACUrl'}de{'Suche ACUrl'}")]
        public string SearchACUrlPBSourceItem
        {
            get
            {
                return _SearchACUrlPBSourceItem;
            }
            set
            {
                if (_SearchACUrlPBSourceItem != value)
                {
                    _SearchACUrlPBSourceItem = value;
                    OnPropertyChanged();
                    RefreshPBSourceProjectTree(null);
                }
            }
        }
        #endregion

        #region Source-AClass und Properties
        /// <summary>
        /// The _ current PB source AC class
        /// </summary>
        ACClass _CurrentPBSourceACClass = null;
        /// <summary>
        /// Gets or sets the current PB source AC class.
        /// </summary>
        /// <value>The current PB source AC class.</value>
        [ACPropertyCurrent(9999, "PBSourceACClass")]
        public ACClass CurrentPBSourceACClass
        {
            get
            {
                return _CurrentPBSourceACClass;
            }
            set
            {
                if (_CurrentPBSourceACClass != value)
                {
                    _CurrentPBSourceACClass = value;
                    CurrentPBSourceACClassProperty = null;
                    this.ProjectManager.CurrentPBSourceACClass = value;
                    OnPropertyChanged("CurrentPBSourceACClass");
                    OnPropertyChanged("PBSourceACClassPropertyList");
                }
                else
                    this.ProjectManager.CurrentPBSourceACClass = value;
            }
        }

        /// <summary>
        /// Gets the PB source AC class property list.
        /// </summary>
        /// <value>The PB source AC class property list.</value>
        [ACPropertyList(9999, "PBSourceACClassProperty")]
        public IEnumerable<ACClassProperty> PBSourceACClassPropertyList
        {
            get
            {
                return this.ProjectManager.GetPBSourceACClassPropertyList(CurrentPropertyModeEnum);
            }
        }

        /// <summary>
        /// The _ current PB source AC class property
        /// </summary>
        ACClassProperty _CurrentPBSourceACClassProperty = null;
        /// <summary>
        /// Gets or sets the current PB source AC class property.
        /// </summary>
        /// <value>The current PB source AC class property.</value>
        [ACPropertyCurrent(9999, "PBSourceACClassProperty")]
        public ACClassProperty CurrentPBSourceACClassProperty
        {
            get
            {
                return _CurrentPBSourceACClassProperty;
            }
            set
            {
                if (_CurrentPBSourceACClassProperty != value)
                {
                    _CurrentPBSourceACClassProperty = value;
                    OnPropertyChanged("CurrentPBSourceACClassProperty");
                }
            }
        }
        #endregion
        #endregion

        #region OnlineValue
        /// <summary>
        /// The _ current AC component
        /// </summary>
        ACRef<IACComponent> _CurrentACComponent = null;
        /// <summary>
        /// Gets the current AC component.
        /// </summary>
        /// <value>The current AC component.</value>
        protected IACComponent CurrentACComponent
        {
            get
            {
                if (CurrentACClass == null)
                    return null;
                if ((CurrentVisualACClass != null) && (CurrentVisualACClass is IACComponent))
                    return CurrentVisualACClass as IACComponent;

                string acUrlComponent = CurrentACClass.GetACUrlComponent();
                if (acUrlComponent == null)
                    return null;
                if ((_CurrentACComponent != null) && (_CurrentACComponent.GetACUrl() != acUrlComponent))
                {
                    _CurrentACComponent.Detach();
                    _CurrentACComponent = null;
                }
                if (_CurrentACComponent == null)
                {
                    if (CurrentACClass.ACUrl == "\\")
                        return null;
                    object result = ACUrlCommand("?" + acUrlComponent);
                    if (result == null)
                        return null;
                    if (!(result is IACComponent))
                        return null;
                    IACComponent onlineTest = (IACComponent)result;
                    if (onlineTest != null)
                        _CurrentACComponent = new ACRef<IACComponent>(onlineTest, this);
                }
                if (_CurrentACComponent == null)
                    return null;
                return _CurrentACComponent.ValueT;
            }
        }

        /// <summary>
        /// The _ access online value
        /// </summary>
        ACAccess<IACMember> _AccessOnlineValue;
        /// <summary>
        /// Gets the access online value.
        /// </summary>
        /// <value>The access online value.</value>
        [ACPropertyAccess(9999, "OnlineValue")]
        public ACAccess<IACMember> AccessOnlineValue
        {
            get
            {
                if (_AccessOnlineValue == null && ACType != null)
                {
                    ACQueryDefinition acQueryDefinition = Root.Queries.CreateQuery(null, Const.QueryPrefix + "ACMember", Const.QueryPrefix + "ACMember");
                    _AccessOnlineValue = acQueryDefinition.NewAccess<IACMember>("OnlineValue", this);
                }
                return _AccessOnlineValue;
            }
        }

        /// <summary>
        /// The _ current AC property
        /// </summary>
        IACPropertyBase _CurrentACProperty = null;
        /// <summary>
        /// Gets or sets the current online value.
        /// </summary>
        /// <value>The current online value.</value>
        [ACPropertyCurrent(9999, "CurrentOnlineValue", "en{'Live-Value'}de{'Live-Wert'}")]
        public string CurrentOnlineValue
        {
            get
            {
                if ((CurrentACClassProperty == null) || (CurrentACComponent == null))
                    return "";
                if ((_CurrentACProperty == null)
                    || (_CurrentACProperty.ACIdentifier != CurrentACClassProperty.ACIdentifier)
                    || (_CurrentACProperty.ParentACComponent != CurrentACComponent))
                {
                    IACMember member = CurrentACComponent.GetMember(CurrentACClassProperty.ACIdentifier);
                    if ((member != null) && (member is IACPropertyBase))
                        _CurrentACProperty = (IACPropertyBase)member;
                }
                if ((_CurrentACProperty == null) || (_CurrentACProperty.Value == null))
                    return "";
                if (!(_CurrentACProperty.Value is IConvertible) && !(_CurrentACProperty.Value is IFormattable))
                    return "";
                return ACConvert.ChangeType(_CurrentACProperty.Value, typeof(string), true, this.Database) as string;
            }
            set
            {
                if (_CurrentACProperty == null)
                    return;
                _CurrentACProperty.Value = value;
                OnPropertyChanged("CurrentOnlineValue");

                if (_CurrentACProperty.Value == null)
                {
                    if (!typeof(IConvertible).IsAssignableFrom(_CurrentACProperty.ACType.ObjectType))
                        return;
                }
                else if (!(_CurrentACProperty.Value is IConvertible) && !(_CurrentACProperty.Value is IFormattable))
                    return;
                try
                {
                    //if (_CurrentACProperty.ACType.ObjectType.IsEnum)
                    //    _CurrentACProperty.Value = Enum.Parse(_CurrentACProperty.ACType.ObjectType, value);
                    //else
                    //{
                    //    //object myValue = Convert.ChangeType(value, _CurrentACProperty.ACType.ObjectType);
                    //    //_CurrentACProperty.Value = myValue;
                    //    _CurrentACProperty.Value = value;
                    //}
                    _CurrentACProperty.Value = ACConvert.ChangeType(value, _CurrentACProperty.ACType.ObjectType, true, this.Database);
                    OnPropertyChanged("CurrentOnlineValue");
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("BSOiPlusStudio_2_Property", "CurrentValueOnline", msg);
                }
            }
        }

        /// <summary>
        /// Gets the online value list.
        /// </summary>
        /// <value>The online value list.</value>
        [ACPropertyList(9999, "OnlineValue", "en{'Live-Value'}de{'Live-Wert'}")]
        public IEnumerable<IACMember> OnlineValueList
        {
            get
            {
                if (CurrentACComponent == null)
                    return null;

                List<IACMember> copiedList = null;

                using (ACMonitor.Lock(CurrentACComponent.LockMemberList_20020))
                {
                    copiedList = CurrentACComponent.ACMemberList.ToList();
                }

                // Abfrage erst später auf Liste, damit es zu keinem Deadlock kommt.
                //return copiedList.AsParallel().Where(c => !string.IsNullOrEmpty(c.ACCaption)
                return copiedList.Where(c => !string.IsNullOrEmpty(c.ACCaption)
                                                                        && c.ACIdentifier != "ACDiagnoseInfo"
                                                                        && c.ACIdentifier != "ACDiagnoseXMLDoc"
                                                                        && c is IACPropertyNetBase
                                                                        && ((c.Value != null && (c.Value is IConvertible || c.Value.GetType().IsEnum || c.Value is IFormattable))
                                                                            || (c.Value == null && (typeof(IConvertible).IsAssignableFrom(c.ACType.ObjectType) || c.ACType.ObjectType.IsEnum || typeof(IFormattable).IsAssignableFrom(c.ACType.ObjectType)))
                                                                            )
                                                                        ).OrderBy(c => c.ACCaption).ToList();
            }
        }

        /// <summary>
        /// The _ selected online value
        /// </summary>
        IACMember _SelectedOnlineValue;
        /// <summary>
        /// Gets or sets the selected online value.
        /// </summary>
        /// <value>The selected online value.</value>
        [ACPropertySelected(9999, "OnlineValue", "en{'Live-Value'}de{'Live-Wert'}")]
        public IACMember SelectedOnlineValue
        {
            get
            {
                return _SelectedOnlineValue;
            }
            set
            {
                if (_SelectedOnlineValue != value)
                {
                    _SelectedOnlineValue = value;
                    OnPropertyChanged("SelectedOnlineValue");
                }
            }
        }

        /// <summary>
        /// Updates the online value.
        /// </summary>
        [ACMethodCommand("ACProject", "en{'Refresh Live-Values'}de{'Aktualisiere Live-Werte'}", 9999)]
        public void UpdateOnlineValue()
        {
            OnPropertyChanged("OnlineValueList");
        }
#endregion
#endregion

#region BSO->ACMethod
#region 1.1.2 ACClassProperty
        /// <summary>
        /// News the AC class property.
        /// </summary>
        [ACMethodInteraction("ACClassProperty", "en{'New Property'}de{'Neue Eigenschaft'}", (short)MISort.New, true, "CurrentACClassProperty", Global.ACKinds.MSMethodPrePost)]
        public void NewACClassProperty()
        {
            if (!PreExecute("NewACClassProperty")) return;
            // Einfügen einer neuen Eigenschaft und der aktuellen Eigenschaft zuweisen
            ACClassProperty acClassProperty = ACClassProperty.NewACObject(Database.ContextIPlus, CurrentACClass);
            CurrentACClass.ACClassProperty_ACClass.Add(acClassProperty);
            if (_ACClassPropertyList != null)
                _ACClassPropertyList.Insert(0, acClassProperty);
            //_ACClassPropertyList = null;
            OnPropertyChanged("ACClassPropertyList");
            PostExecute("NewACClassProperty");
            CurrentACClassProperty = acClassProperty;
        }

        /// <summary>
        /// Determines whether [is enabled new AC class property].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new AC class property]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACClassProperty()
        {
            if (CurrentACClass == null)
                return false;
            return ProjectManager.IsEnabledNewACClassProperty(CurrentACClass);
        }

        /// <summary>
        /// Deletes the AC class property.
        /// </summary>
        [ACMethodInteraction("ACClassProperty", "en{'Delete Property'}de{'Eigenschaft löschen'}", (short)MISort.Delete, true, "CurrentACClassProperty", Global.ACKinds.MSMethodPrePost)]
        public void DeleteACClassProperty()
        {
            if (!PreExecute("DeleteACClassProperty")) 
                return;
            if (CurrentACClassProperty != null)
            {
                var taskValueList = CurrentACClassProperty.ACClassTaskValue_ACClassProperty.ToArray();
                if (taskValueList != null && taskValueList.Any())
                {
                    var result = Messages.Question(this, "This Property is already used in a active component. Deleting it can lead to an inconsistent state. Are you sure?", Global.MsgResult.No, true);
                    if (result == Global.MsgResult.No)
                        return;
                    foreach (ACClassTaskValue taskValue in CurrentACClassProperty.ACClassTaskValue_ACClassProperty.ToArray())
                    {
                        taskValue.DeleteACObject(Database.ContextIPlus, true);
                    }
                }
                Msg msg = CurrentACClassProperty.DeleteACObject(Database.ContextIPlus, true);
                if (msg != null)
                {
                    Messages.Msg(msg);
                    return;
                }
            }

            if (_ACClassPropertyList != null)
            {
                _ACClassPropertyList.Remove(CurrentACClassProperty);
                CurrentACClassProperty = _ACClassPropertyList.FirstOrDefault();
            }
            //_ACClassPropertyList = null;
            OnPropertyChanged("ACClassPropertyList");
            PostExecute("DeleteACClassProperty");
        }

        /// <summary>
        /// Determines whether [is enabled delete AC class property].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete AC class property]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteACClassProperty()
        {
            return ProjectManager.IsEnabledDeleteACClassProperty(CurrentACClass, CurrentACClassProperty);
        }
#endregion

#region 1.1.2.1 ACClassPropertyRelation
        /// <summary>
        /// Removes the property relation.
        /// </summary>
        [ACMethodInteraction("ACClassProperty", "en{'Remove to-Connection'}de{'Nach-Beziehung entfernen'}", (short)MISort.Delete, false, "CurrentPropertyRelationTo", Global.ACKinds.MSMethodPrePost)]
        public void RemovePropertyRelationTo()
        {
            if (!PreExecute("RemovePropertyRelationTo")) return;
            Msg msg = ProjectManager.RemoveACClassPropertyRelation(CurrentACClass, CurrentPropertyRelationTo.ACObject as ACClassPropertyRelation);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }
            OnPropertyChanged("PropertyRelationToList");
            PostExecute("RemovePropertyRelationTo");
        }

        /// <summary>
        /// Determines whether [is enabled remove property relation].
        /// </summary>
        /// <returns><c>true</c> if [is enabled remove property relation]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledRemovePropertyRelationTo()
        {
            if (CurrentPropertyRelationTo == null || !(CurrentPropertyRelationTo.ACObject is ACClassPropertyRelation))
                return false;

            return ProjectManager.IsEnabledRemoveACClassPropertyRelation(CurrentACClass, CurrentPropertyRelationTo.ACObject as ACClassPropertyRelation);
        }

        /// <summary>
        /// Removes the property relation.
        /// </summary>
        [ACMethodInteraction("ACClassProperty", "en{'Remove from-Connection'}de{'Von-Beziehung entfernen'}", (short)MISort.Delete, false, "CurrentPropertyRelationFrom", Global.ACKinds.MSMethodPrePost)]
        public void RemovePropertyRelationFrom()
        {
            if (!PreExecute("RemovePropertyRelationFrom")) return;
            Msg msg = ProjectManager.RemoveACClassPropertyRelation(CurrentACClass, CurrentPropertyRelationFrom.ACObject as ACClassPropertyRelation);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }
            OnPropertyChanged("PropertyRelationFromList");
            PostExecute("RemovePropertyRelationFrom");
        }

        /// <summary>
        /// Determines whether [is enabled remove property relation].
        /// </summary>
        /// <returns><c>true</c> if [is enabled remove property relation]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledRemovePropertyRelationFrom()
        {
            if (CurrentPropertyRelationFrom == null || !(CurrentPropertyRelationFrom.ACObject is ACClassPropertyRelation))
                return false;

            return ProjectManager.IsEnabledRemoveACClassPropertyRelation(CurrentACClass, CurrentPropertyRelationFrom.ACObject as ACClassPropertyRelation);
        }


        /// <summary>
        /// Removes the property binding.
        /// </summary>
        [ACMethodInteraction("ACClassProperty", "en{'Remove Binding'}de{'Bindung entfernen'}", (short)MISort.Delete, false, "CurrentACClassProperty", Global.ACKinds.MSMethodPrePost)]
        public void RemovePropertyBinding()
        {
            if (!PreExecute("RemovePropertyBinding")) return;
            ProjectManager.RemovePropertyBinding(CurrentACClassProperty, CurrentACClass);
            OnPropertyChanged("CurrentACClassProperty");
            PostExecute("RemovePropertyBinding");
        }

        /// <summary>
        /// Determines whether [is enabled remove property binding].
        /// </summary>
        /// <returns><c>true</c> if [is enabled remove property binding]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledRemovePropertyBinding()
        {
            return ProjectManager.IsEnabledRemovePropertyBinding(CurrentACClassProperty, CurrentACClass);
        }
#endregion


#region 1.1.2.4 PointStateInfo
        /// <summary>
        /// News the point state info.
        /// </summary>
        [ACMethodInteraction("PointStateInfo", "en{'New Point stateinfo'}de{'Neue Statusinformation'}", (short)MISort.New, true, "CurrentPointStateInfo", Global.ACKinds.MSMethodPrePost)]
        public void NewPointStateInfo()
        {
            if (!IsEnabledNewPointStateInfo())
                return;
            if (!PreExecute("NewPointStateInfo")) return;

            // TODO: Hier kann die "Konfiguration" für einen Connector-Point gespeichert werden
            // var x = CurrentACClassProperty.NewACConfig();

            object[] valueList = new object[] { "", "", false };
            string[] captionList = new string[] { "Relative Url", Const.Value, "logical OR" };
            object[] resultList = Messages.InputBoxValues("Info00018", valueList, captionList);

            if (resultList == null)
                return;

            string propertyRelativeUrl = resultList[0] as string;
            string valueOfState = resultList[1] as string;
            if (String.IsNullOrEmpty(propertyRelativeUrl) || String.IsNullOrEmpty(valueOfState))
                return;

            ACPointStateInfo stateInfo = new ACPointStateInfo(propertyRelativeUrl, valueOfState, (Boolean)resultList[2] ? Global.Operators.or : Global.Operators.and);
            if (stateInfo.InsertOrUpdate(Database.ContextIPlus, CurrentACClass, CurrentACClassProperty))
            {
                OnPropertyChanged("PointStateInfoList");
            }

            PostExecute("NewPointStateInfo");
        }

        /// <summary>
        /// Determines whether [is enabled new point state info].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new point state info]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewPointStateInfo()
        {
            if (CurrentACClass == null)
                return false;
            if ((CurrentACClassProperty == null) || (CurrentACClassProperty.ACPropUsage != Global.ACPropUsages.ConnectionPoint))
                return false;
            return true;
        }

        /// <summary>
        /// Deletes the point state info.
        /// </summary>
        [ACMethodInteraction("PointStateInfo", "en{'Delete Point stateinfo'}de{'Statusinformation löschen'}", (short)MISort.Delete, true, "CurrentPointStateInfo", Global.ACKinds.MSMethodPrePost)]
        public void DeletePointStateInfo()
        {
            if (!IsEnabledDeletePointStateInfo())
                return;

            if (!PreExecute("DeletePointStateInfo")) return;
            Msg msg = CurrentPointStateInfo.DeleteACObject(Database.ContextIPlus, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }

            _PointStateInfoList = null;
            OnPropertyChanged("PointStateInfoList");
            PostExecute("DeletePointStateInfo");
        }

        /// <summary>
        /// Determines whether [is enabled delete point state info].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete point state info]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeletePointStateInfo()
        {
            if (CurrentACClass == null)
                return false;
            if ((CurrentACClassProperty == null) || (CurrentACClassProperty.ACPropUsage != Global.ACPropUsages.ConnectionPoint) || (CurrentPointStateInfo == null))
                return false;
            if (CurrentPointStateInfo.SourceACClassID != CurrentACClass.ACClassID)
                return false;
            return true;
        }
#endregion

#region Binding Drag and Drop
        /// <summary>
        /// Drops the PBAC class property.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="dropObject">The drop object.</param>
        /// <param name="targetVBDataObject">The target VB data object.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        [ACMethodInfo("ACClassProperty", "en{'Drop Property'}de{'Drop Property'}", 9999, false, Global.ACKinds.MSMethodPrePost)]
        public void DropPBACClassProperty(Global.ElementActionType action, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            if (!PreExecute("DropPBACClassProperty")) return;
            bool dropDone = false;
            switch (action)
            {
                case Global.ElementActionType.Drop:
                    dropDone = ProjectManager.DropPBACClassProperty(action, dropObject, targetVBDataObject, CurrentACClass, x, y, CurrentPropertyModeEnum);
                    switch (CurrentPropertyModeEnum)
                    {
                        case Global.PropertyModes.Connections:
                            OnPropertyChanged("PropertyRelationFromList");
                            OnPropertyChanged("PropertyRelationToList");
                            break;
                        case Global.PropertyModes.Bindings:
                            if (dropDone)
                            {
                                ACClassProperty targetProp = targetVBDataObject.GetACValue(typeof(ACClassProperty)) as ACClassProperty;
                                if (targetProp != null)
                                {
                                    targetProp.RaiseOnPropertyChanged("TopBaseACClassProperty");
                                    if (CurrentACClassProperty != targetProp)
                                        CurrentACClassProperty = targetProp;
                                }
                            }
                            break;
                    }
                    break;
                case Global.ElementActionType.Move: // TODO:
                    break;
                default:
                    break;
            }

            if (dropDone)
            {
                PostExecute("DropPBACClassProperty");
            }
        }
        /// <summary>
        /// Determines whether [is enabled drop PBAC class property] [the specified action].
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="dropObject">The drop object.</param>
        /// <param name="targetVBDataObject">The target VB data object.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if [is enabled drop PBAC class property] [the specified action]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDropPBACClassProperty(Global.ElementActionType action, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            return ProjectManager.IsEnabledDropPBACClassProperty(action, dropObject, targetVBDataObject, CurrentACClass, x, y, CurrentPropertyModeEnum);
        }
#endregion
#endregion

#region Layout und Propertychanged
        /// <summary>
        /// Aktualisieren des ACClassProperty, wenn Eigenschaften geändert werden, welche in Ableitungen unterschiedlich sein können
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void CurrentACClassPropertyTemp_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (CurrentACClassProperty != null)
                CurrentACClassProperty.PropertyChanged -= CurrentACClassProperty_PropertyChanged;

            ACClassProperty acClassProperty = ProjectManager.UpdateACClassPropertyTemp(CurrentACClass, CurrentACClassProperty, e.PropertyName, CurrentACClassPropertyTemp.ACUrlCommand(e.PropertyName), CurrentACClassProperty.ACUrlCommand(e.PropertyName));

            if (CurrentACClassProperty != null)
            {
                CurrentACClassProperty.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CurrentACClassProperty_PropertyChanged);
            }

            if (CurrentACClassProperty != acClassProperty)
            {
                CurrentACClassProperty = acClassProperty;
                OnPropertyChanged("CurrentACClassProperty");
            }
        }

        /// <summary>
        /// Aktualisieren des ACClassProperty, wenn Eigenschaften geändert werden, welche in allen Ableitungen identisch sein müssen
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void CurrentACClassProperty_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (CurrentACClassProperty != null)
            {
                CurrentACClassProperty.PropertyChanged -= CurrentACClassProperty_PropertyChanged;
                switch (e.PropertyName)
                {
                    case Const.ACIdentifierPrefix:
                        if (string.IsNullOrEmpty(CurrentACClassProperty.ACCaption) || CurrentACClassProperty.EntityState == EntityState.Added)
                        {
                            CurrentACClassProperty.ACCaption = CurrentACClassProperty.ACIdentifier;
                            CurrentACClassProperty.OnEntityPropertyChanged(Const.ACCaptionPrefix);
                        }
                        break;
                }
            }

            ACClassProperty acClassProperty = ProjectManager.UpdateACClassProperty(CurrentACClass, CurrentACClassProperty, e.PropertyName);
            if (CurrentACClassProperty != null)
            {
                CurrentACClassProperty.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CurrentACClassProperty_PropertyChanged);
            }
            if (acClassProperty == null && CurrentACClassProperty != null && CurrentACClassProperty.EntityState == EntityState.Added && e.PropertyName == "ACIdentifier")
            {
                //OnPropertyChanged("CurrentACClassProperty");
            }
            if (CurrentACClassProperty != acClassProperty)
            {
                CurrentACClassProperty = acClassProperty;
                OnPropertyChanged("CurrentACClassProperty");
            }
        }

        void CurrentPropBindingToSource_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("CurrentACClassProperty");
        }

#endregion

#region TabProperty
        /// <summary>
        /// The _ property mode
        /// </summary>
        Global.PropertyModes _PropertyMode = Global.PropertyModes.Livevalues;
        /// <summary>
        /// Gets or sets the current property mode enum.
        /// </summary>
        /// <value>The current property mode enum.</value>
        public Global.PropertyModes CurrentPropertyModeEnum
        {
            get
            {
                return _PropertyMode;
            }
            set
            {
                if (_PropertyMode != value)
                {
                    _PropertyMode = value;
                    switch (_PropertyMode)
                    {
                        case Global.PropertyModes.Properties:
                            AccessACClassProperty.NavACQueryDefinition.ACColumns = AccessACClassProperty.NavACQueryDefinition.GetACColumns(_ColumnsProperties);
                            AccessACClassProperty.NavACQueryDefinition.ClearFilter();
                            AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.parenthesisOpen, Global.Operators.and));
                            AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, "ACPropUsageIndex", Global.LogicalOperators.equal, Global.Operators.or, ((Int16)Global.ACPropUsages.ConfigPointProperty).ToString(), false));
                            AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, "ConfigACClass", Global.LogicalOperators.isNull, Global.Operators.and, "", false));
                            AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.parenthesisClose, Global.Operators.and));
                            HidePropertyInfo();
                            ControlModePropertyModes = Global.ControlModes.Enabled;
                            ShowPointStateInfos = Global.ControlModes.Collapsed;
                            ShowBindingInfos = Global.ControlModes.Collapsed;
                            break;
                        //case Global.PropertyModes.Configuration:
                        //    _AccessACClassProperty.NavACQueryDefinition.ACColumns = _AccessACClassProperty.NavACQueryDefinition.GetACColumns(_ColumnsConfigurationpoints);
                        //    _AccessACClassProperty.NavACQueryDefinition.ClearFilter();
                        //    _AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, "ACPropUsageIndex", Global.LogicalOperators.equal, Global.Operators.and, ((Int16)Global.ACPropUsages.ConfigPointProperty).ToString(), false));
                        //    HidePropertyInfo();
                        //    ControlModePropertyModes = Global.ControlModes.Collapsed;
                        //    ControlModePropertyDetail = Global.ControlModes.Enabled;
                        //    ShowPointStateInfos = Global.ControlModes.Collapsed;
                        //    break;
                        case Global.PropertyModes.Connections:
                            _AccessACClassProperty.NavACQueryDefinition.ACColumns = _AccessACClassProperty.NavACQueryDefinition.GetACColumns(_ColumnsConnectionpoints);
                            _AccessACClassProperty.NavACQueryDefinition.ClearFilter();
                            _AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, "ACPropUsageIndex", Global.LogicalOperators.equal, Global.Operators.and, ((Int16)Global.ACPropUsages.ConnectionPoint).ToString(), false));
                            ShowPropertyInfo();
                            ControlModePropertyModes = Global.ControlModes.Collapsed;
                            ShowPointStateInfos = Global.ControlModes.Enabled;
                            ShowBindingInfos = Global.ControlModes.Collapsed;
                            break;
                        case Global.PropertyModes.Relations:
                            _AccessACClassProperty.NavACQueryDefinition.ACColumns = _AccessACClassProperty.NavACQueryDefinition.GetACColumns(_ColumnsReleations);
                            _AccessACClassProperty.NavACQueryDefinition.ClearFilter();
                            _AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, "ACPropUsageIndex", Global.LogicalOperators.equal, Global.Operators.and, ((Int16)Global.ACPropUsages.RelationPoint).ToString(), false));
                            HidePropertyInfo();
                            ControlModePropertyModes = Global.ControlModes.Collapsed;
                            ShowPointStateInfos = Global.ControlModes.Collapsed;
                            ShowBindingInfos = Global.ControlModes.Collapsed;
                            break;
                        case Global.PropertyModes.Events:
                            _AccessACClassProperty.NavACQueryDefinition.ACColumns = _AccessACClassProperty.NavACQueryDefinition.GetACColumns(_ColumnsEvents);
                            _AccessACClassProperty.NavACQueryDefinition.ClearFilter();
                            _AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, "ACPropUsageIndex", Global.LogicalOperators.equal, Global.Operators.or, ((Int16)Global.ACPropUsages.EventPoint).ToString(), false));
                            _AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, "ACPropUsageIndex", Global.LogicalOperators.equal, Global.Operators.and, ((Int16)Global.ACPropUsages.EventPointSubscr).ToString(), false));
                            ShowPropertyInfo();
                            ControlModePropertyModes = Global.ControlModes.Collapsed;
                            ShowPointStateInfos = Global.ControlModes.Collapsed;
                            ShowBindingInfos = Global.ControlModes.Collapsed;
                            break;
                        case Global.PropertyModes.Joblists:
                            _AccessACClassProperty.NavACQueryDefinition.ACColumns = _AccessACClassProperty.NavACQueryDefinition.GetACColumns(_ColumnsMethodstacks);
                            _AccessACClassProperty.NavACQueryDefinition.ClearFilter();
                            _AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, "ACPropUsageIndex", Global.LogicalOperators.equal, Global.Operators.and, ((Int16)Global.ACPropUsages.AsyncMethodPoint).ToString(), false));
                            HidePropertyInfo();
                            ControlModePropertyModes = Global.ControlModes.Collapsed;
                            ShowPointStateInfos = Global.ControlModes.Collapsed;
                            ShowBindingInfos = Global.ControlModes.Collapsed;
                            break;
                        case Global.PropertyModes.Bindings:
                            _AccessACClassProperty.NavACQueryDefinition.ACColumns = _AccessACClassProperty.NavACQueryDefinition.GetACColumns(_ColumnsBindings);
                            _AccessACClassProperty.NavACQueryDefinition.ClearFilter();
                            _AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, "IsProxyProperty", Global.LogicalOperators.equal, Global.Operators.and, "true", false));
                            _AccessACClassProperty.NavACQueryDefinition.ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, "IsBroadcast", Global.LogicalOperators.equal, Global.Operators.and, "true", false));
                            ControlModePropertyModes = Global.ControlModes.Collapsed;
                            ShowPointStateInfos = Global.ControlModes.Collapsed;
                            ShowBindingInfos = Global.ControlModes.Enabled;
                            ShowPropertyInfo();
                            break;
                        case Global.PropertyModes.Livevalues:
                            _AccessACClassProperty.NavACQueryDefinition.ClearFilter();
                            ShowPropertyInfo();
                            ControlModePropertyModes = Global.ControlModes.Collapsed;
                            ShowPointStateInfos = Global.ControlModes.Collapsed;
                            ShowBindingInfos = Global.ControlModes.Collapsed;
                            break;
                    }

                    BroadcastToVBControls(Const.CmdUpdateControlMode, "CurrentACClassProperty");
                    BroadcastToVBControls(Const.CmdUpdateVBContent, "CurrentACClassProperty");
                    //OnPropertyChanged("ACQueryDefinition:CurrentACClassProperty");
                    OnPropertyChanged("CurrentPropertyMode");
                    _ACClassPropertyList = null;
                    OnPropertyChanged("ACClassPropertyList");
                    OnPropertyChanged("CurrentPropertyConfigLayout");
                    OnPropertyChanged("PBSourceACClassPropertyList");
                }
            }
        }

        /// <summary>
        /// The _ current property mode
        /// </summary>
        ACValueItem _CurrentPropertyMode;
        /// <summary>
        /// Gets or sets the current property mode.
        /// </summary>
        /// <value>The current property mode.</value>
        [ACPropertyCurrent(9999, "PropertyMode", "en{'Propertymode'}de{'Eigenschaftsmodus'}", "", false)]
        public ACValueItem CurrentPropertyMode
        {
            get
            {
                return _CurrentPropertyMode;
            }
            set
            {
                if (_CurrentPropertyMode != value && value != null)
                {
                    _CurrentPropertyMode = value;
                    CurrentPropertyModeEnum = (Global.PropertyModes)(Int16)value.Value;
                    OnPropertyChanged("CurrentPropertyMode");
                }
            }
        }

        /// <summary>
        /// The _ property mode list
        /// </summary>
        ACValueItemList _PropertyModeList;
        /// <summary>
        /// Gets the property mode list.
        /// </summary>
        /// <value>The property mode list.</value>
        [ACPropertyList(9999, "PropertyMode")]
        public IEnumerable<ACValueItem> PropertyModeList
        {
            get
            {
                if (CurrentACClass != null && _PropertyModeList == null)
                {
                    _PropertyModeList = new ACValueItemList(typeof(Global.MethodModes).Name);
                    _PropertyModeList.AddEntry((short)Global.PropertyModes.Properties, Global.PropertyModes.Properties.ToString());

                    var propertyList = CurrentACClass.Properties;
                    if (propertyList.Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConnectionPoint).Any())
                        _PropertyModeList.AddEntry((short)Global.PropertyModes.Connections, Global.PropertyModes.Connections.ToString());

                    if (propertyList.Where(c => c.IsProxyProperty && c.IsBroadcast).Any())
                        _PropertyModeList.AddEntry((short)Global.PropertyModes.Bindings, Global.PropertyModes.Bindings.ToString());

                    if (propertyList.Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.EventPoint || c.ACPropUsageIndex == (Int16)Global.ACPropUsages.EventPointSubscr).Any())
                        _PropertyModeList.AddEntry((short)Global.PropertyModes.Events, Global.PropertyModes.Events.ToString());

                    if (CurrentACClass.ACKind == Global.ACKinds.TACDBA)
                        _PropertyModeList.AddEntry((short)Global.PropertyModes.Relations, Global.PropertyModes.Relations.ToString());

                    if (propertyList.Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.AsyncMethodPoint).Any())
                        _PropertyModeList.AddEntry((short)Global.PropertyModes.Joblists, Global.PropertyModes.Joblists.ToString());

                    if (!CurrentACClass.IsMultiInstance && (CurrentACProject.ACProjectType == Global.ACProjectTypes.Application || CurrentACProject.ACProjectType == Global.ACProjectTypes.Service))
                        _PropertyModeList.AddEntry((short)Global.PropertyModes.Livevalues, Global.PropertyModes.Livevalues.ToString());
                }
                return _PropertyModeList;
            }
        }

        /// <summary>
        /// Gets the current property config layout.
        /// </summary>
        /// <value>The current property config layout.</value>
        public string CurrentPropertyConfigLayout
        {
            get
            {
                switch (CurrentPropertyModeEnum)
                {
                    case Global.PropertyModes.Bindings:
                        return ACType.GetDesign("PropertyBindings").XMLDesign;
                    case Global.PropertyModes.Connections:
                        return ACType.GetDesign("PropertyConnectionpoints").XMLDesign;
                    case Global.PropertyModes.Events:
                        return ACType.GetDesign("PropertyEvents").XMLDesign;
                    case Global.PropertyModes.Livevalues:
                        return ACType.GetDesign("PropertyLivevalues").XMLDesign;
                    //case Global.PropertyModes.Configuration:
                    case Global.PropertyModes.Joblists:
                    case Global.PropertyModes.Properties:
                    case Global.PropertyModes.Relations:
                    default:
                        return LayoutHelper.VBDockPanelEmpty();
                }
            }
        }

        /// <summary>
        /// TabProperty
        /// </summary>
        Global.ControlModes _ControlModePropertyModes = Global.ControlModes.Enabled;
        /// <summary>
        /// Gets or sets the control mode property modes.
        /// </summary>
        /// <value>The control mode property modes.</value>
        [ACPropertyInfo(9999)]
        public Global.ControlModes ControlModePropertyModes
        {
            get
            {
                return _ControlModePropertyModes;
            }
            set
            {
                if (_ControlModePropertyModes != value)
                {
                    _ControlModePropertyModes = value;
                    OnPropertyChanged("ControlModePropertyModes");
                }
            }
        }

        /// <summary>
        /// The _ show point state infos
        /// </summary>
        Global.ControlModes _ShowPointStateInfos = Global.ControlModes.Collapsed;
        /// <summary>
        /// Gets or sets the show point state infos.
        /// </summary>
        /// <value>The show point state infos.</value>
        [ACPropertyInfo(9999)]
        public Global.ControlModes ShowPointStateInfos
        {
            get
            {
                return _ShowPointStateInfos;
            }
            set
            {
                if (_ShowPointStateInfos != value)
                {
                    _ShowPointStateInfos = value;
                    OnPropertyChanged("ShowPointStateInfos");
                }
            }
        }

        /// <summary>
        /// The _ show binding infos
        /// </summary>
        Global.ControlModes _ShowBindingInfos = Global.ControlModes.Collapsed;
        /// <summary>
        /// Gets or sets the show binding infos.
        /// </summary>
        /// <value>The show binding infos.</value>
        [ACPropertyInfo(9999)]
        public Global.ControlModes ShowBindingInfos
        {
            get
            {
                return _ShowBindingInfos;
            }
            set
            {
                if (_ShowBindingInfos != value)
                {
                    _ShowBindingInfos = value;
                    OnPropertyChanged("ShowBindingInfos");
                }
            }
        }

        /// <summary>
        /// Shows the property info.
        /// </summary>
        public void ShowPropertyInfo()
        {
            IACObject window = FindGui("", "", "*PropertyInfo", this.ACIdentifier);
            if (window != null)
                return;

            ShowWindow(this, "PropertyInfo", false,
                Global.VBDesignContainer.DockableWindow,
                Global.VBDesignDockState.Docked,
                Global.VBDesignDockPosition.Right);
        }

        /// <summary>
        /// Hides the property info.
        /// </summary>
        public void HidePropertyInfo()
        {
            CloseWindow(this, "PropertyInfo");
        }

        /// <summary>
        /// Closes the property info.
        /// </summary>
        public void ClosePropertyInfo()
        {
            IACObject window = FindGui("", "", "*PropertyInfo", this.ACIdentifier);
            if (window != null)
                return;

            ShowWindow(this, "PropertyBindingSource", false,
                Global.VBDesignContainer.DockableWindow,
                Global.VBDesignDockState.Docked,
                Global.VBDesignDockPosition.Right);
        }

        /// <summary>
        /// The _ columns properties
        /// </summary>
        //static string _ColumnsProperties = Const.ACIdentifierPrefix + ",ValueTypeACClass,GenericType,IsNullable," + Const.ACCaptionPrefix + "," + ACClass.ClassName + ",SortIndex," + Const.ACKindIndex + "," + Const.ACGroup + ",ACPropUsageIndex,IsEnumerable,LogRefreshRateIndex,IsRightmanagement,IsInteraction,IsBroadcast,ForceBroadcast,IsProxyProperty,IsInput,IsOutput,IsPersistable,MinLength,MaxLength,MinValue,MaxValue,IsSerializable";
        // Less fields, because rendering of datagrid costs to much time
        //static string _ColumnsProperties = Const.ACIdentifierPrefix + ",ValueTypeACClass,GenericType,IsNullable," + Const.ACCaptionPrefix + "," + ACClass.ClassName + ",SortIndex," + Const.ACKindIndex + "," + Const.ACGroup + ",ACPropUsageIndex,IsPersistable";
        static string _ColumnsProperties = Const.ACIdentifierPrefix + ",ValueTypeACClass," + Const.ACCaptionPrefix + "," + ACClass.ClassName + ",SortIndex," + Const.ACKindIndex;
        /// <summary>
        /// The _ columns connectionpoints
        /// </summary>
        static string _ColumnsConnectionpoints = Const.ACIdentifierPrefix + "," + Const.ACCaptionPrefix;
        /// <summary>
        /// The _ columns releations
        /// </summary>
        static string _ColumnsReleations = Const.ACIdentifierPrefix + ",ValueTypeACClass\\ACIdentifier," + Const.ACCaptionPrefix;
        /// <summary>
        /// The _ columns events
        /// </summary>
        static string _ColumnsEvents = Const.ACIdentifierPrefix + "," + Const.ACCaptionPrefix;
        /// <summary>
        /// The _ columns methodstacks
        /// </summary>
        static string _ColumnsMethodstacks = Const.ACIdentifierPrefix + "," + Const.ACCaptionPrefix;
        /// <summary>
        /// The _ columns bindings
        /// </summary>
        static string _ColumnsBindings = Const.ACIdentifierPrefix + ",ValueTypeACClass\\ACIdentifier,!GetBindingSourceACUrl(#TopBaseACClassProperty#),!GetBindingSourceMultiplier(#TopBaseACClassProperty#),!GetBindingSourceDivisor(#TopBaseACClassProperty#),!GetBindingSourceExprT(#TopBaseACClassProperty#),!GetBindingSourceExprS(#TopBaseACClassProperty#)";

        /// <summary>
        /// Gets the binding source AC URL.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <returns>System.String.</returns>
        [ACMethodInfo("", "en{'Binding source'}de{'Bindungsquelle'}", 9999, false)]
        public string GetBindingSourceACUrl(object param)
        {
            if (param == null)
                return "";
            ACClassProperty currentProp = (param as ACClassProperty);
            ACClassPropertyRelation binding = currentProp.GetMyPropertyBindingToSource(CurrentACClass);
            if (binding == null)
                return "";
            return binding.SourceACUrl;
        }

        [ACMethodInfo("", "en{'Multiplier source->target'}de{'Multiplikator Quelle->Ziel'}", 9999, false)]
        public double? GetBindingSourceMultiplier(object param)
        {
            if (param == null)
                return null;
            ACClassProperty currentProp = (param as ACClassProperty);
            ACClassPropertyRelation binding = currentProp.GetMyPropertyBindingToSource(CurrentACClass);
            if (binding == null)
                return null;
            return binding.Multiplier;
        }

        [ACMethodInfo("", "en{'Divisor source->target'}de{'Teiler Quelle->Ziel'}", 9999, false)]
        public double? GetBindingSourceDivisor(object param)
        {
            if (param == null)
                return null;
            ACClassProperty currentProp = (param as ACClassProperty);
            ACClassPropertyRelation binding = currentProp.GetMyPropertyBindingToSource(CurrentACClass);
            if (binding == null)
                return null;
            return binding.Divisor;
        }

        [ACMethodInfo("", "en{'Expression source->target'}de{'Formel Quelle->Ziel'}", 9999, false)]
        public string GetBindingSourceExprT(object param)
        {
            if (param == null)
                return "";
            ACClassProperty currentProp = (param as ACClassProperty);
            ACClassPropertyRelation binding = currentProp.GetMyPropertyBindingToSource(CurrentACClass);
            if (binding == null)
                return "";
            return binding.ConvExpressionT;
        }

        [ACMethodInfo("", "en{'Expression target->source'}de{'Formel Ziel->Quelle'}", 9999, false)]
        public string GetBindingSourceExprS(object param)
        {
            if (param == null)
                return "";
            ACClassProperty currentProp = (param as ACClassProperty);
            ACClassPropertyRelation binding = currentProp.GetMyPropertyBindingToSource(CurrentACClass);
            if (binding == null)
                return "";
            return binding.ConvExpressionS;
        }


        /// <summary>
        /// Gets the binding targets AC URL.
        /// </summary>
        /// <param name="param">The param.</param>
        /// <returns>System.String.</returns>
        [ACMethodInfo("", "en{'GetBindingTargetsACUrl'}de{'GetBindingTargetsACUrl'}", 9999, false)]
        public string GetBindingTargetsACUrl(object param)
        {
            if (param == null)
                return "";
            ACClassProperty currentProp = (param as ACClassProperty);
            if (currentProp == null)
                return "";
            if (currentProp.ACClassPropertyRelation_SourceACClassProperty.Count <= 0)
                return "";
            string targets = "";
            foreach (ACClassPropertyRelation rel in currentProp.ACClassPropertyRelation_SourceACClassProperty)
            {
                targets += rel.TargetACUrl + " \r\n";
            }
            return targets;
        }
#endregion
    }
}
