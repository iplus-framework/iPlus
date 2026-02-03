// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-09-2012
// ***********************************************************************
// <copyright file="BSOiPlusStudio_3_Design.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using gip.core.datamodel;
using gip.core.manager;
using gip.core.autocomponent;
using System.Collections.ObjectModel;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSOiPlusStudio
    /// </summary>
    public partial class BSOiPlusStudio
    {
        #region BSO->ACProperty
        #region 1.1.3 ACClassDesign
        /// <summary>
        /// The _ access AC class design
        /// </summary>
        ACAccess<ACClassDesign> _AccessACClassDesign;
        /// <summary>
        /// Gets the access AC class design.
        /// </summary>
        /// <value>The access AC class design.</value>
        [ACPropertyAccess(9999, "ACClassDesign")]
        public ACAccess<ACClassDesign> AccessACClassDesign
        {
            get
            {
                if (_AccessACClassDesign == null && ACType != null)
                {
                    ACQueryDefinition acQueryDefinition = AccessACClass.NavACQueryDefinition.ACUrlCommand(Const.QueryPrefix + ACClassDesign.ClassName) as ACQueryDefinition;
                    _AccessACClassDesign = acQueryDefinition.NewAccess<ACClassDesign>(ACClassDesign.ClassName, this);
                }
                return _AccessACClassDesign;
            }
        }

        /// <summary>
        /// The _ current AC class design
        /// </summary>
        ACClassDesign _CurrentACClassDesign;
        /// <summary>
        /// Gets or sets the current AC class design.
        /// </summary>
        /// <value>The current AC class design.</value>
        [ACPropertyCurrent(9999, "ACClassDesign")]
        public ACClassDesign CurrentACClassDesign
        {
            get
            {
                return _CurrentACClassDesign;
            }
            set
            {
                if (_CurrentACClassDesign != value)
                {
                    if (CurrentACClassDesign != null)
                        CurrentACClassDesign.PropertyChanged -= CurrentACClassDesign_PropertyChanged;

                    _CurrentACClassDesign = value;
                    OnPropertyChanged("CurrentACClassDesign");
                    OnPropertyChanged("CurrentDesignLayout");
                    OnPropertyChanged("IsDesignLayoutVisible");
                    OnPropertyChanged("IsDesignMenuVisible");
                    OnPropertyChanged("IsDesignBitmapVisible");

                    if (CurrentACClassDesign != null)
                    {
                        CurrentACClassDesign.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CurrentACClassDesign_PropertyChanged);
                    }
                    this.DesignManagerACClassDesign.CurrentDesign = value;
                }
                else
                {
                    this.DesignManagerACClassDesign.CurrentDesign = value;
                }
                if (CurrentACClass != null && CurrentACClassDesign != null)
                {
                    _IsDefaultACClassDesign = ProjectManager.IsDefaultACClassDesign(CurrentACClass, CurrentACClassDesign, null);
                }
                OnPropertyChanged("IsDefaultACClassDesign");
            }
        }

        /// <summary>
        /// The _ is default AC class design
        /// </summary>
        bool _IsDefaultACClassDesign = false;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is default AC class design.
        /// </summary>
        /// <value><c>true</c> if this instance is default AC class design; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(9999, "", "en{'Defaultdesign'}de{'Standarddesign'}")]
        public bool IsDefaultACClassDesign
        {
            get
            {
                return _IsDefaultACClassDesign;
            }
            set
            {
                if (CurrentACClass != null && CurrentACClassDesign != null)
                {
                    _IsDefaultACClassDesign = ProjectManager.SetDefaultACClassDesign(CurrentACClass, CurrentACClassDesign, value);
                }
                OnPropertyChanged("IsDefaultACClassDesign");
            }
        }

        /// <summary>
        /// The _ selected AC class design
        /// </summary>
        ACClassDesign _SelectedACClassDesign;
        /// <summary>
        /// Gets or sets the selected AC class design.
        /// </summary>
        /// <value>The selected AC class design.</value>
        [ACPropertySelected(9999, "ACClassDesign")]
        public ACClassDesign SelectedACClassDesign
        {
            get
            {
                return _SelectedACClassDesign;
            }
            set
            {
                _SelectedACClassDesign = value;
                OnPropertyChanged("SelectedACClassDesign");

            }
        }

        /// <summary>
        /// The _ AC class design list
        /// </summary>
        ObservableCollection<ACClassDesign> _ACClassDesignList = null;
        /// <summary>
        /// Gets the AC class design list.
        /// </summary>
        /// <value>The AC class design list.</value>
        [ACPropertyList(9999, "ACClassDesign")]
        public IEnumerable<ACClassDesign> ACClassDesignList
        {
            get
            {
                if (CurrentACClass == null)
                    return null;
                if (_ACClassDesignList == null)
                {
                    _ACClassDesignList = new ObservableCollection<ACClassDesign>(CurrentACClass.Designs);
                }
                return _ACClassDesignList;
            }
        }

        /// <summary>
        /// The _ current new AC class design
        /// </summary>
        ACClassDesign _CurrentNewACClassDesign = null;
        /// <summary>
        /// Gets or sets the current new AC class design.
        /// </summary>
        /// <value>The current new AC class design.</value>
        [ACPropertyCurrent(9999, "NewACClassDesign")]
        public ACClassDesign CurrentNewACClassDesign
        {
            get
            {
                return _CurrentNewACClassDesign;
            }
            set
            {
                _CurrentNewACClassDesign = value;
                OnPropertyChanged("CurrentNewACClassDesign");
            }
        }

        /// <summary>
        /// The _ refresh design
        /// </summary>
        private int _RefreshDesign;
        /// <summary>
        /// Gets the refresh design.
        /// </summary>
        /// <value>The refresh design.</value>
        [ACPropertyInfo(9999)]
        public int RefreshDesign
        {
            get
            {
                return _RefreshDesign;
            }
        }

        /// <summary>
        /// Refreshes the designer.
        /// </summary>
        public void RefreshDesigner()
        {
            _RefreshDesign++;
            OnPropertyChanged("RefreshDesign");
        }

        /// <summary>
        /// Gets or sets the bitmap filter.
        /// </summary>
        /// <value>The bitmap filter.</value>
        [ACPropertyInfo(9999, DefaultValue = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG;*.TIF;*.ICO)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIF;*.ICO")]
        public string BitmapFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the bitmap initial directory.
        /// </summary>
        /// <value>The bitmap initial directory.</value>
        [ACPropertyInfo(9999)]
        public string BitmapInitialDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the bitmap restore directory.
        /// </summary>
        /// <value>The bitmap restore directory.</value>
        [ACPropertyInfo(9999, DefaultValue = true)]
        public Boolean BitmapRestoreDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the bitmap file.
        /// </summary>
        /// <value>The name of the bitmap file.</value>
        [ACPropertyInfo(9999)]
        public string BitmapFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the bitmap file names.
        /// </summary>
        /// <value>The bitmap file names.</value>
        [ACPropertyInfo(9999)]
        public string[] BitmapFileNames
        {
            get;
            set;
        }


        #endregion

        #region 1.1.3.1 ACClassMenu
        /// <summary>
        /// The _ current menu entry
        /// </summary>
        ACMenuItem _CurrentMenuEntry;
        /// <summary>
        /// Gets or sets the current menu entry.
        /// </summary>
        /// <value>The current menu entry.</value>
        [ACPropertyCurrent(9999, "MenuEntry")]
        public ACMenuItem CurrentMenuEntry
        {
            get
            {
                return _CurrentMenuEntry;
            }
            set
            {
                _CurrentMenuEntry = value;
                OnPropertyChanged("CurrentMenuEntry");
            }
        }

        /// <summary>
        /// The _ current menu entry change info
        /// </summary>
        ChangeInfo _CurrentMenuEntryChangeInfo = null;
        /// <summary>
        /// Gets or sets the current menu entry change info.
        /// </summary>
        /// <value>The current menu entry change info.</value>
        [ACPropertyChangeInfo(9999, "MenuEntry")]
        public ChangeInfo CurrentMenuEntryChangeInfo
        {
            get
            {
                return _CurrentMenuEntryChangeInfo;
            }
            set
            {
                _CurrentMenuEntryChangeInfo = value;
                OnPropertyChanged("CurrentMenuEntryChangeInfo");
            }
        }

        /// <summary>
        /// The _ current selected menu entry
        /// </summary>
        ACMenuItem _CurrentSelectedMenuEntry = null;
        /// <summary>
        /// Gets or sets the current selected menu entry.
        /// </summary>
        /// <value>The current selected menu entry.</value>
        [ACPropertyCurrent(9999, "MenuEntry")]
        public ACMenuItem CurrentSelectedMenuEntry
        {
            get
            {
                return _CurrentSelectedMenuEntry;
            }
            set
            {
                if (_CurrentSelectedMenuEntry != value)
                {
                    if (CurrentSelectedMenuEntry != null)
                    {
                        CurrentSelectedMenuEntry.PropertyChanged -= CurrentSelectedMenuEntry_PropertyChanged;
                    }
                    _CurrentSelectedMenuEntry = value;
                    OnPropertyChanged("CurrentSelectedMenuEntry");
                    OnPropertyChanged("CurrentMethodLayout");
                    OnPropertyChanged("IsMethodInfoVisible");
                    OnPropertyChanged("IsScriptEditorVisible");
                    OnPropertyChanged("IsWFEditorVisible");
                    OnPropertyChanged("ParameterValueList");
                    OnPropertyChanged("CurrentMenuIcon");
                    if (CurrentSelectedMenuEntry != null)
                    {
                        CurrentSelectedMenuEntry.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(CurrentSelectedMenuEntry_PropertyChanged);
                    }
                }

                _CurrentSelectedMenuEntry = value;
                OnPropertyChanged("CurrentSelectedMenuEntry");
            }
        }

        /// <summary>
        /// The _ current parameter value
        /// </summary>
        ACValue _CurrentParameterValue;
        /// <summary>
        /// Gets or sets the current parameter value.
        /// </summary>
        /// <value>The current parameter value.</value>
        [ACPropertyCurrent(9999, "ParameterValue", "en{'Parameter'}de{'Parameter'}")]
        public ACValue CurrentParameterValue
        {
            get
            {
                return _CurrentParameterValue;
            }
            set
            {
                _CurrentParameterValue = value;
                OnPropertyChanged("CurrentParameterValue");
                OnPropertyChanged("CurrentParameterValueLayout");
            }
        }

        /// <summary>
        /// Gets the parameter value list.
        /// </summary>
        /// <value>The parameter value list.</value>
        [ACPropertyList(9999, "ParameterValue", "en{'Parameterlist'}de{'Parameterlist'}")]
        public IEnumerable<ACValue> ParameterValueList
        {
            get
            {
                if (CurrentSelectedMenuEntry == null)
                    return null;
                return CurrentSelectedMenuEntry.ParameterList;
            }
        }

        #endregion

        #region DesignManager
        /// <summary>
        /// The _ design manager AC class design
        /// </summary>
        VBDesignerXAML _DesignManagerACClassDesign;
        /// <summary>
        /// Gets the design manager AC class design.
        /// </summary>
        /// <value>The design manager AC class design.</value>
        [ACPropertyInfo(9999)]
        public VBDesignerXAML DesignManagerACClassDesign
        {
            get
            {
                if (_DesignManagerACClassDesign == null)
                {
                    _DesignManagerACClassDesign = ACUrlCommand("?VBDesignerXAML") as VBDesignerXAML;
                    if (_DesignManagerACClassDesign == null)
                        _DesignManagerACClassDesign = StartComponent("VBDesignerXAML", null, null) as VBDesignerXAML;
                    _DesignManagerACClassDesign.InitDesignManager("CurrentACClassDesign");
                }
                return _DesignManagerACClassDesign;
            }
        }
        #endregion
        #endregion

        #region BSO->ACMethod
        #region 1.1.3 ACClassDesign
        /// <summary>
        /// Loads the AC class design.
        /// </summary>
        [ACMethodInteraction("ACClassDesign", "en{'Load Design'}de{'Design laden'}", (short)MISort.Load, false, "SelectedACClassDesign")]
        public void LoadACClassDesign()
        {
            if (SelectedACClassDesign != null)
            {
                CurrentACClassDesign = SelectedACClassDesign;
                UpdateDesign();
            }
        }

        /// <summary>
        /// Updates the design.
        /// </summary>
        private async void UpdateDesign()
        {
            if (CurrentACClassDesign != null)
            {
                switch (CurrentACClassDesign.ACKind)
                {
                    case Global.ACKinds.DSDesignLayout:
                    case Global.ACKinds.DSDesignReport:
                        if (CurrentACClassDesign.XAMLDesign != null)
                        {
                            string layout = CurrentACClassDesign.XAMLDesign; //.FormatXML() muss nicht aufgerufen werden, weil Editor dies per "XmlWriterSettings.Indent = true" selsbt macht
                            if (layout == null)
                            {
                                await Messages.ErrorAsync(this, "Error00001");
                            }

                            if (!string.IsNullOrEmpty(layout) && layout != CurrentACClassDesign.XAMLDesign)
                                CurrentACClassDesign.XAMLDesign = layout;

                        }
                        else
                        {
                            CurrentACClassDesign.XAMLDesign = "";
                        }
                        break;
                    case Global.ACKinds.DSDesignMenu:
                        CurrentMenuEntry = CurrentACClassDesign.MenuEntry;
                        break;
                    //case Global.ACKinds.DSDesignReport:
                    //    // TODO:
                    //    break;
                    case Global.ACKinds.DSBitmapResource:
                        // TODO:
                        break;
                }
            }

        }

        /// <summary>
        /// Determines whether [is enabled load AC class design].
        /// </summary>
        /// <returns><c>true</c> if [is enabled load AC class design]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledLoadACClassDesign()
        {
            return SelectedACClassDesign != null;
        }

        /// <summary>
        /// News the AC class design.
        /// </summary>
        [ACMethodInteraction("ACClassDesign", "en{'New Design'}de{'Neues Design'}", (short)MISort.New, true, "SelectedACClassDesign", Global.ACKinds.MSMethodPrePost)]
        public void NewACClassDesign()
        {
            if (!PreExecute("NewACClassDesign")) return;
            string secondaryKey = Root.NoManager.GetNewNo(Database, typeof(ACClassDesign), ACClassDesign.NoColumnName, ACClassDesign.FormatNewNo, this);
            CurrentNewACClassDesign = ACClassDesign.NewACObject(Database.ContextIPlus, CurrentACClass, secondaryKey);
            CurrentNewACClassDesign.ACUsage = Global.ACUsages.DULayout;
            ShowDialog(this, "ACClassDesignNew");
            if (CurrentNewACClassDesign != null)
            {
                if (CurrentNewACClassDesign.ACUsage >= Global.ACUsages.DULLReport && CurrentNewACClassDesign.ACUsage <= Global.ACUsages.DUReport)
                    CurrentNewACClassDesign.ACKind = Global.ACKinds.DSDesignReport;
            }
            PostExecute("NewACClassDesign");
        }

        /// <summary>
        /// Determines whether [is enabled new AC class design].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new AC class design]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACClassDesign()
        {
            return ProjectManager.IsEnabledNewACClassDesign(CurrentACClass);
        }

        /// <summary>
        /// News the AC class design OK.
        /// </summary>
        [ACMethodCommand("ACClassDesign", Const.Ok, (short)MISort.Okay)]
        public void NewACClassDesignOK()
        {
            CloseTopDialog();
            switch (CurrentNewACClassDesign.ACUsage)
            {
                case Global.ACUsages.DUMainmenu:
                    CurrentNewACClassDesign.ACKind = Global.ACKinds.DSDesignMenu;
                    break;
                case Global.ACUsages.DUBitmap:
                case Global.ACUsages.DUIcon:
                    CurrentNewACClassDesign.ACKind = Global.ACKinds.DSBitmapResource;
                    break;
                case Global.ACUsages.DULLReport:
                case Global.ACUsages.DULLOverview:
                case Global.ACUsages.DULLList:
                case Global.ACUsages.DULLLabel:
                case Global.ACUsages.DULLFilecard:
                    CurrentNewACClassDesign.ACKind = Global.ACKinds.DSDesignReport;
                    break;
                case Global.ACUsages.DUMain:
                case Global.ACUsages.DULayout:
                case Global.ACUsages.DUControl:
                case Global.ACUsages.DUControlDialog:
                case Global.ACUsages.DUDiagnostic:
                case Global.ACUsages.DUVisualisation:
                default:
                    CurrentNewACClassDesign.ACKind = Global.ACKinds.DSDesignLayout;
                    break;
            }

            if (CurrentNewACClassDesign.ACUsage == Global.ACUsages.DUMain)
            {
                if (!CurrentACClass.ACClassDesign_ACClass.Where(c => c.ACUsageIndex == (Int16)Global.ACUsages.DUMain && CurrentNewACClassDesign.ACClassDesignID != c.ACClassDesignID).Any())
                {
                    CurrentNewACClassDesign.ACIdentifier = "Mainlayout";
                    CurrentNewACClassDesign.ACCaptionTranslation = CurrentACClass.ACCaptionTranslation;
                }
            }

            string defaultXAML = "";
            switch (CurrentNewACClassDesign.ACUsage)
            {
                case Global.ACUsages.DUControl:
                    {
                        defaultXAML += "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n";
                        defaultXAML += "<vb:VBViewbox " + ACxmlnsResolver.XMLNamespaces + ">\n";
                        defaultXAML += "    <vb:VBCanvas Width=\"100\" Height=\"100\" Background=\"Transparent\" Name=\"Canvas_0\">\n";
                        defaultXAML += "    </vb:VBCanvas>\n";
                        defaultXAML += "</vb:VBViewbox>";
                    }
                    break;
                case Global.ACUsages.DUMain:
                    {
                        string name = CurrentACClass.ACIdentifier;
                        if (name.StartsWith("BSO")) name = name.Substring(3);
                        if (name.StartsWith("MDBSO")) name = name.Substring(5);
                        defaultXAML += "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n";
                        defaultXAML += "<vb:VBDockingManager " + ACxmlnsResolver.XMLNamespaces + " x:Name=\"" + CurrentNewACClassDesign.ACClass.ACIdentifier + "\">\n";
                        defaultXAML += "    <vb:" + Const.VBDesign_ClassName + " VBContent=\"*" + name + "\" vb:VBDockingManager.IsCloseableBSORoot=\"False\" vb:VBDockingManager.Container=\"TabItem\" vb:VBDockingManager.DockState=\"Tabbed\" vb:VBDockingManager.DockPosition=\"Bottom\" vb:VBDockingManager.RibbonBarVisibility=\"Hidden\" vb:VBDockingManager.WindowSize=\"0,0\" />\n";
                        defaultXAML += "    <vb:" + Const.VBDesign_ClassName + " VBContent=\"*Explorer\" vb:VBDockingManager.IsCloseableBSORoot=\"False\" vb:VBDockingManager.Container=\"DockableWindow\" vb:VBDockingManager.DockState=\"AutoHideButton\" vb:VBDockingManager.DockPosition=\"Bottom\" vb:VBDockingManager.RibbonBarVisibility=\"Hidden\" vb:VBDockingManager.WindowSize=\"0,0\" />\n";
                        defaultXAML += "</vb:VBDockingManager>\n";
                    }
                    break;
                case Global.ACUsages.DULayout:
                    {
                        defaultXAML += "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n";
                        defaultXAML += "<vb:VBGrid " + ACxmlnsResolver.XMLNamespaces + ">\n";
                        defaultXAML += "    <Grid.ColumnDefinitions>\n";
                        defaultXAML += "        <ColumnDefinition></ColumnDefinition>\n";
                        defaultXAML += "            <ColumnDefinition></ColumnDefinition>\n";
                        defaultXAML += "    </Grid.ColumnDefinitions>\n";
                        defaultXAML += "    <Grid.RowDefinitions>\n";
                        defaultXAML += "        <RowDefinition Height=\"30\"></RowDefinition>\n";
                        defaultXAML += "        <RowDefinition Height=\"30\"></RowDefinition>\n";
                        defaultXAML += "    </Grid.RowDefinitions>\n";
                        defaultXAML += "    <vb:VBFrame Grid.ColumnSpan=\"2\" Grid.RowSpan=\"2\"></vb:VBFrame>\n";
                        defaultXAML += "</vb:VBGrid>\n";
                    }
                    break;
            }

            if (!String.IsNullOrEmpty(defaultXAML))
                CurrentNewACClassDesign.XAMLDesign = defaultXAML;
            CurrentNewACClassDesign.UpdateVBControlACClass(Database.ContextIPlus);

            CurrentACClassDesign = CurrentNewACClassDesign;
            Database.ContextIPlus.ACClassDesign.Add(CurrentACClassDesign);
            UpdateDesign();
            //_ACClassDesignList = null;
            if(_ACClassDesignList != null)
                _ACClassDesignList.Insert(0, CurrentACClassDesign);
            SelectedACClassDesign = CurrentNewACClassDesign;
            OnPropertyChanged("CurrentDesignLayout");
            OnPropertyChanged("IsDesignLayoutVisible");
            OnPropertyChanged("IsDesignMenuVisible");
            OnPropertyChanged("IsDesignBitmapVisible");
            OnPropertyChanged("ACClassDesignList");
        }

        /// <summary>
        /// Determines whether [is enabled new AC class design OK].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new AC class design OK]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACClassDesignOK()
        {
            if (CurrentNewACClassDesign == null || CurrentNewACClassDesign.ACUsage == Global.ACUsages.DUUndefined)
                return false;
            return true;
        }

        /// <summary>
        /// News the AC class design cancel.
        /// </summary>
        [ACMethodCommand("ACClassDesign", Const.Cancel, (short)MISort.Cancel)]
        public void NewACClassDesignCancel()
        {
            CloseTopDialog();
            if (CurrentNewACClassDesign != null)
            {
                Msg msg = CurrentNewACClassDesign.DeleteACObject(Database.ContextIPlus, true);
                if (msg != null)
                {
                    Messages.MsgAsync(msg);
                    return;
                }

            }
            CurrentNewACClassDesign = null;
        }

        /// <summary>
        /// Deletes the AC class design.
        /// </summary>
        [ACMethodInteraction("ACClassDesign", "en{'Delete Design'}de{'Design löschen'}", (short)MISort.Delete, true, "CurrentACClassDesign")]
        public void DeleteACClassDesign()
        {
            if (!PreExecute("DeleteACClassDesign")) return;
            Msg msg = CurrentACClassDesign.DeleteACObject(Database.ContextIPlus, true);
            if (msg != null)
            {
                Messages.MsgAsync(msg);
                return;
            }

            if(_ACClassDesignList != null)
            {
                _ACClassDesignList.Remove(CurrentACClassDesign);
                CurrentACClassDesign = _ACClassDesignList.FirstOrDefault();
            }
            //_ACClassDesignList = null;

            OnPropertyChanged("ACClassDesignList");
            PostExecute("DeleteACClassDesign");

        }

        /// <summary>
        /// Determines whether [is enabled delete AC class design].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete AC class design]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteACClassDesign()
        {
            if (CurrentACClassDesign == null || CurrentACClass == null)
                return false;
            if (CurrentACClassDesign.ACClass != CurrentACClass)
                return false;

            if (CurrentACClass.ACIdentifier == ACRoot.ClassName
                && CurrentACClassDesign.ACIdentifier.Contains("IPlus")
                && CurrentACClassDesign.ACKind == Global.ACKinds.DSDesignMenu)
                return false;

            return true;
        }

        [ACMethodInfo("", "en{'Expand'}de{'Expand'}", 999)]
        public void OnToolWindowItemExpand(ACObjectItem item)
        {
            this.DesignManagerACClassDesign.OnItemExpand(item);
        }

        [ACMethodInteraction("ACClassDesign", "en{'Duplicate Design'}de{'Design Duplizieren'}", (short)MISort.Load + 2, true, "SelectedACClassDesign")]
        public void DuplicateDesign()
        {
            string secondaryKey = Root.NoManager.GetNewNo(Database, typeof(ACClassDesign), ACClassDesign.NoColumnName, ACClassDesign.FormatNewNo, this);
            ACClassDesign duplDesign = ACClassDesign.NewACObject(Database.ContextIPlus, CurrentACClass, secondaryKey);
            CurrentACClassDesign.CopyFieldsTo(duplDesign);
            duplDesign.ACIdentifier = GetDuplicatedDesignACIdentifier();
            CurrentACClassDesign = duplDesign;
            Database.ContextIPlus.ACClassDesign.Add(duplDesign);

            if (_ACClassDesignList != null)
                _ACClassDesignList.Insert(0, CurrentACClassDesign);
            SelectedACClassDesign = duplDesign;
        }

        public bool IsEnabledDuplicateDesign()
        {
            return CurrentACClass != null && CurrentACClassDesign != null;
        }

        private string GetDuplicatedDesignACIdentifier()
        {
            string acIdentifier = CurrentACClassDesign.ACIdentifier;
            var possibleDuplicates = CurrentACClass.Designs.Select(c => c.ACIdentifier).ToArray();

            for (int i=1; i < 100; i++)
            {
                string duplACIdentifier = acIdentifier + i;
                if(!possibleDuplicates.Contains(duplACIdentifier))
                {
                    acIdentifier = duplACIdentifier;
                    break;
                }
            }

            return acIdentifier;
        }

        #region Bitmap

        /// <summary>
        /// Imports the bitmap.
        /// </summary>
        [ACMethodInteraction("ACClassDesign", "en{'Import Bitmap'}de{'Importiere Bitmap'}", 9999, true, "SelectedACClassDesign", Global.ACKinds.MSMethodPrePost)]
        public void ImportBitmap()
        {
            if (!IsEnabledImportBitmap())
                return;
            if (!PreExecute("ImportBitmap"))
                return;

            if (!String.IsNullOrEmpty(BitmapFileName))
            {
                try
                {
                    Byte[] result = File.ReadAllBytes(BitmapFileName);
                    CurrentACClassDesign.DesignBinary = result;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException("BSOiPlusStudio_3_Design", "ImportBitmap", msg);
                }
            }

            PostExecute("ImportBitmap");
        }

        /// <summary>
        /// Determines whether [is enabled import bitmap].
        /// </summary>
        /// <returns><c>true</c> if [is enabled import bitmap]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledImportBitmap()
        {
            if (CurrentACClassDesign == null)
                return false;
            if (CurrentACClassDesign.ACUsage != Global.ACUsages.DUBitmap && CurrentACClassDesign.ACUsage != Global.ACUsages.DUIcon)
                return false;
            return true;
        }

        [ACMethodCommand("", "en{'Generate Icon'}de{'Generiere Ikone'}", 401, true)]
        public void GenerateIcon()
        {
            if (!IsEnabledGenerateIcon())
                return;
            BroadcastToVBControls(Const.CmdPrintScreenToIcon, "*TabOverview", this, "CurrentVisualACClass_0");
        }

        public bool IsEnabledGenerateIcon()
        {
            return CurrentACClassDesign != null && CurrentACClassDesign.ACUsage == Global.ACUsages.DUIcon;
        }

        protected void OnIconGenerated(byte[] bytes)
        {
            if (!IsEnabledGenerateIcon())
                return;
            if (bytes != null && bytes.Length > 0)
                CurrentACClassDesign.DesignBinary = bytes;
        }
        #endregion

        /// <summary>
        /// Compile ACClassDesign into baml and save in database.
        /// </summary>
        [ACMethodInteraction("ACClassDesign", "en{'Compile Design'}de{'Design kompilieren'}", 9999, false, "SelectedACClassDesign")]
        public void CompileACClassDesign()
        {
            if (!string.IsNullOrEmpty(CurrentACClassDesign.XAMLDesign) && CurrentACClassDesign.ACUsage != Global.ACUsages.DUBitmap)
            {
                //byte[] bamlDesign = VarioBatch.Helper.BamlWriter.Save(CurrentACClassDesign.XMLDesign);
                byte[] bamlDesign = null;
                if (bamlDesign == null)
                {
                    Messages.ErrorAsync(this, "Error50057");
                    return;
                }
                CurrentACClassDesign.BAMLDesign = bamlDesign;
                Save();
                CurrentACClassDesign.BAMLDate = CurrentACClassDesign.UpdateDate;
                ACSaveChanges();
                CurrentACClassDesign.OnEntityPropertyChanged("IsDesignCompiled");
            }
        }

        /// <summary>
        /// Determines wheter is enabled compile ACClassDesign
        /// </summary>
        /// <returns><c>true</c> if [is enabled compile AC class design]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledCompileACClassDesign()
        {
            return CurrentACClassDesign != null && !CurrentACClassDesign.IsDesignCompiled;
        }


        #endregion

        #region 1.1.3.1 ACClassMenu
        /// <summary>
        /// Deletes the menu entry.
        /// </summary>
        [ACMethodInteraction("MenuEntry", "en{'Delete Menuentry'}de{'Menüeintrag löschen'}", (short)MISort.Delete, true, "CurrentSelectedMenuEntry")]
        public void DeleteMenuEntry()
        {
            if (CurrentSelectedMenuEntry != null && CurrentSelectedMenuEntry.ParentMenuEntry != null)
            {
                ACMenuItem menuEntry = CurrentSelectedMenuEntry;
                CurrentSelectedMenuEntry.ParentMenuEntry.Items.Remove(CurrentSelectedMenuEntry);
                CurrentACClassDesign.MenuEntry = CurrentMenuEntry;
                CurrentMenuEntryChangeInfo = new ChangeInfo(null, menuEntry, Const.CmdDeleteData);
                OnPropertyChanged("CurrentSelectedMenuEntry");
            }
        }

        /// <summary>
        /// Determines whether [is enabled delete menu entry].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete menu entry]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDeleteMenuEntry()
        {
            return CurrentACClassDesign != null && CurrentSelectedMenuEntry != null;
        }

        /// <summary>
        /// Inserts the menu entry.
        /// </summary>
        [ACMethodInteraction("MenuEntry", "en{'Insert Menuentry'}de{'Menüeintrag einfügen'}", (short)MISort.InsertData, true, "CurrentSelectedMenuEntry")]
        public void InsertMenuEntry()
        {
            if (CurrentSelectedMenuEntry != null && CurrentSelectedMenuEntry.ParentMenuEntry != null)
            {
                ACMenuItem menuEntry = new ACMenuItem(CurrentSelectedMenuEntry.ParentMenuEntry, "<Empty>", "", 0, null);

                int index = CurrentSelectedMenuEntry.ParentMenuEntry.Items.IndexOf(CurrentSelectedMenuEntry);

                CurrentSelectedMenuEntry.ParentMenuEntry.Items.Insert(index, menuEntry);
                CurrentMenuEntryChangeInfo = new ChangeInfo(null, menuEntry, Const.CmdInsertData);
                OnPropertyChanged("CurrentSelectedMenuEntry");
            }
        }

        /// <summary>
        /// Determines whether [is enabled insert menu entry].
        /// </summary>
        /// <returns><c>true</c> if [is enabled insert menu entry]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledInsertMenuEntry()
        {
            return CurrentACClassDesign != null && CurrentSelectedMenuEntry != null;
        }

        /// <summary>
        /// News the menu entry.
        /// </summary>
        [ACMethodInteraction("MenuEntry", "en{'Append Menuentry'}de{'Menüeintrag anfügen'}", (short)MISort.InsertData, true, "CurrentSelectedMenuEntry")]
        public void NewMenuEntry()
        {
            if (CurrentSelectedMenuEntry != null && CurrentSelectedMenuEntry.ParentMenuEntry != null)
            {
                ACMenuItem menuEntry = new ACMenuItem(CurrentSelectedMenuEntry.ParentMenuEntry, "<Empty>", "", 0, null);

                CurrentSelectedMenuEntry.ParentMenuEntry.Items.Add(menuEntry);
                CurrentMenuEntryChangeInfo = new ChangeInfo(null, menuEntry, Const.CmdNewData);
                OnPropertyChanged("CurrentSelectedMenuEntry");
            }
        }

        /// <summary>
        /// Determines whether [is enabled new menu entry].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new menu entry]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewMenuEntry()
        {
            return CurrentACClassDesign != null;
        }

        /// <summary>
        /// News the child menu entry.
        /// </summary>
        [ACMethodInteraction("MenuEntry", "en{'Insert Submenuentry'}de{'Untermenüeintrag einfügen'}", (short)MISort.New, true, "CurrentSelectedMenuEntry")]
        public void NewChildMenuEntry()
        {
            if (CurrentSelectedMenuEntry != null)
            {
                ACMenuItem menuEntry = new ACMenuItem(CurrentSelectedMenuEntry, "<Empty>", "", 0, null);
                CurrentSelectedMenuEntry.Items.Add(menuEntry);
                CurrentMenuEntryChangeInfo = new ChangeInfo(null, menuEntry, Const.CmdAddChildData);
                OnPropertyChanged("CurrentSelectedMenuEntry");
            }
        }

        /// <summary>
        /// Determines whether [is enabled new child menu entry].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new child menu entry]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewChildMenuEntry()
        {
            return CurrentACClassDesign != null && CurrentSelectedMenuEntry != null;
        }

        /// <summary>
        /// ACs the URL editor.
        /// </summary>
        [ACMethodCommand("MenuEntry", "en{'ACUrl-Editor'}de{'ACUrl-Editor'}", 9999)]
        public void ACUrlEditor()
        {
            ACMethod acSignature = (ACMethod)this.ACUrlCommand("VBBSOACUrlEditor!ACUrlEditorDlg", new object[] { Root.ACType as ACClass, CurrentSelectedMenuEntry.ACUrl, CurrentSelectedMenuEntry.ParameterList });
            if (acSignature != null)
            {
                // TODO:
                CurrentSelectedMenuEntry.ParameterList = acSignature.ParameterValueList;
                //CurrentSelectedMenuEntry.ACUrl = acSignature.ACIdentifier;
                CurrentACClassDesign.MenuEntry = CurrentMenuEntry;
                OnPropertyChanged("CurrentSelectedMenuEntry");
                OnPropertyChanged("ParameterValueList");
            }
        }

        /// <summary>
        /// Determines whether [is enabled AC URL editor].
        /// </summary>
        /// <returns><c>true</c> if [is enabled AC URL editor]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledACUrlEditor()
        {
            return CurrentSelectedMenuEntry != null;
        }

        ACClassDesign _CurrentMenuIcon;
        [ACPropertyCurrent(999, "MenuIcon", "en{'Menu icon'}de{'Menu icon'}")]
        public ACClassDesign CurrentMenuIcon
        {
            get
            {
                if (CurrentSelectedMenuEntry != null && !string.IsNullOrEmpty(CurrentSelectedMenuEntry.IconACUrl))
                    _CurrentMenuIcon = ACClassDesignList.Where(c => c.ACUsageIndex == (short)Global.ACUsages.DUIcon).ToList().FirstOrDefault(x => x.GetACUrl() == CurrentSelectedMenuEntry.IconACUrl);
                else
                    _CurrentMenuIcon = null;
                return _CurrentMenuIcon;
            }
            set
            {
                _CurrentMenuIcon = value;
                if (_CurrentMenuIcon == null)
                    CurrentSelectedMenuEntry.IconACUrl = null;
                else
                    CurrentSelectedMenuEntry.IconACUrl = _CurrentMenuIcon.ACUrl;
                CurrentSelectedMenuEntry.OnPropertyChanged("IconACUrl");
                OnPropertyChanged("CurrentMenuIcon");
            }
        }

        IEnumerable<ACClassDesign> _MenuIconList;
        [ACPropertyList(999, "MenuIcon")]
        public IEnumerable<ACClassDesign> MenuIconList
        {
            get
            {
                if (_MenuIconList == null)
                    _MenuIconList = ACClassDesignList.Where(c => c.ACUsageIndex == (short)Global.ACUsages.DUIcon);
                return _MenuIconList;
            }
        }

        #endregion

        #endregion

        #region Layout und Propertychanged
        /// <summary>
        /// Handles the PropertyChanged event of the CurrentACClassDesign control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void CurrentACClassDesign_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //switch (e.PropertyName)
            //{
            //    case Const.ACKindIndex:
            //        break;
            //}
        }

        /// <summary>
        /// Handles the PropertyChanged event of the CurrentSelectedMenuEntry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void CurrentSelectedMenuEntry_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "VBContent":

                    if (sender is ACMenuItem)
                    {
                        ACMenuItem menuEntry = sender as ACMenuItem;
                        if (!string.IsNullOrEmpty(menuEntry.GetACUrl()))
                        {
                            var query = Database.ContextIPlus.ACClass.Where(c => c.ACIdentifier == menuEntry.GetACUrl(null));
                            if (query.Any())
                            {
                                menuEntry.ACCaption = query.First().ACCaption;
                            }
                        }
                    }
                    CurrentACClassDesign.MenuEntry = CurrentMenuEntry;
                    break;

                case Const.ACUrlPrefix:
                case Const.ACCaptionPrefix:
                case "Parameter":
                case "UseACCaption":
                case "RibbonOff":
                case "IconACUrl":
                    CurrentACClassDesign.MenuEntry = CurrentMenuEntry;
                    break;

            }
        }

        /// <summary>
        /// Gets the current design layout.
        /// </summary>
        /// <value>The current design layout.</value>
        public string CurrentDesignLayout
        {
            get
            {
                string layoutXAML = null;
                if (CurrentACClassDesign == null)
                    return layoutXAML;
                switch (CurrentACClassDesign.ACKind)
                {
                    case Global.ACKinds.DSDesignReport:
                    case Global.ACKinds.DSDesignLayout:
                        layoutXAML = ACType.GetDesign("TabDesignLayout").XAMLDesign;
                        break;
                    case Global.ACKinds.DSDesignMenu:
                        layoutXAML = ACType.GetDesign("TabDesignMenu").XAMLDesign;
                        break;
                    //case Global.ACKinds.DSDesignReport:
                    //    layoutXAML = ACType.MyACClassDesign("TabDesignReport").XMLDesign;
                    //    break;
                    case Global.ACKinds.DSBitmapResource:
                        layoutXAML = ACType.GetDesign("TabDesignBitmap").XAMLDesign;
                        break;
                }
                return layoutXAML;
            }
        }

        [ACPropertyInfo(9999)]
        public bool IsDesignLayoutVisible
        {
            get
            {
                if (CurrentACClassDesign == null)
                    return false;
                return CurrentACClassDesign.ACKind == Global.ACKinds.DSDesignReport
                    || CurrentACClassDesign.ACKind == Global.ACKinds.DSDesignLayout;
            }
        }

        [ACPropertyInfo(9999)]
        public bool IsDesignMenuVisible
        {
            get
            {
                if (CurrentACClassDesign == null)
                    return false;
                return CurrentACClassDesign.ACKind == Global.ACKinds.DSDesignMenu;
            }
        }

        [ACPropertyInfo(9999)]
        public bool IsDesignBitmapVisible
        {
            get
            {
                if (CurrentACClassDesign == null)
                    return false;
                return CurrentACClassDesign.ACKind == Global.ACKinds.DSBitmapResource;
            }
        }

        #endregion
    }
}
