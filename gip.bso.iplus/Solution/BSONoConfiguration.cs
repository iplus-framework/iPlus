// ***********************************************************************
// Assembly         : gip.bso.masterdata
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="BSONoConfiguration.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSONoConfiguration
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Number Configuration'}de{'Nummern Konfiguration'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + VBNoConfiguration.ClassName)]
    class BSONoConfiguration : ACBSONav
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="BSONoConfiguration"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSONoConfiguration(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            //DatabaseMode = DatabaseModes.OwnDB;
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

            // Weitere Initialisierung, damit ein sauberer Anfangszustand 
            // erreicht wird
            Search();
            
            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            bool done = await base.ACDeInit(deleteACClassTask);
            if (_AccessPrimary != null)
            {
                await _AccessPrimary.ACDeInit(false);
                _AccessPrimary = null;
            }
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }

            return done;
        }
        #endregion

        #region DB

        private Database _BSODatabase = null;
        /// <summary>
        /// Overriden: Returns a separate database context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_BSODatabase == null)
                    _BSODatabase = ACObjectContextManager.GetOrCreateContext<Database>(this.GetACUrl());
                return _BSODatabase;
            }
        }

        public Database Db
        {
            get
            {
                return Database as Database;
            }
        }

        #endregion

        #region Properties

        #region Properties AccessNav -> COnfiguration
        public override IAccessNav AccessNav { get { return AccessPrimary; } }
        /// <summary>
        /// Dies sind die vom BSO verwendeten Datenentitäten.
        /// Für die Bereitstellung für einen Service, sind diese mittels AddEntityData
        /// zu registrieren
        /// </summary>
        ACAccessNav<VBNoConfiguration> _AccessPrimary;
        /// <summary>
        /// Gets the access primary.
        /// </summary>
        /// <value>The access primary.</value>
        [ACPropertyAccessPrimary(590, "VBNoConfiguration")]
        public ACAccessNav<VBNoConfiguration> AccessPrimary
        {
            get
            {
                if (_AccessPrimary == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, PrimaryNavigationquery(), ACType.ACIdentifier);
                    _AccessPrimary = navACQueryDefinition.NewAccessNav<VBNoConfiguration>("VBNoConfiguration", this);
                }
                return _AccessPrimary;
            }
        }

        /// <summary>
        /// Gets or sets the current no configuration.
        /// </summary>
        /// <value>The current no configuration.</value>
        [ACPropertyCurrent(501, "VBNoConfiguration")]
        public VBNoConfiguration CurrentNoConfiguration
        {
            get
            {
                if (AccessPrimary == null) return null; return AccessPrimary.Current;
            }
            set
            {
                if (AccessPrimary == null) return; AccessPrimary.Current = value;
                OnPropertyChanged("CurrentNoConfiguration");
            }
        }

        /// <summary>
        /// Gets or sets the selected no configuration.
        /// </summary>
        /// <value>The selected no configuration.</value>
        [ACPropertySelected(502, "VBNoConfiguration")]
        public VBNoConfiguration SelectedNoConfiguration
        {
            get
            {
                if (AccessPrimary == null) return null; return AccessPrimary.Selected;
            }
            set
            {
                if (AccessPrimary == null) return; AccessPrimary.Selected = value;
                OnPropertyChanged("SelectedNoConfiguration");
            }
        }

        /// <summary>
        /// Gets the no configuration list.
        /// </summary>
        /// <value>The no configuration list.</value>
        [ACPropertyList(503, "VBNoConfiguration")]
        public IEnumerable<VBNoConfiguration> NoConfigurationList
        {
            get
            {
                return AccessPrimary.NavList;
            }
        }

        #endregion

        #endregion

        #region Methods

        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            if (vbControl == null)
                return base.OnGetControlModes(vbControl);

            Global.ControlModes result = base.OnGetControlModes(vbControl);
            if (result < Global.ControlModes.Enabled)
                return result;
            switch (vbControl.VBContent)
            {
                case "CurrentNoConfiguration\\MinCounter":
                    {
                        if (CurrentNoConfiguration == null)
                            result = Global.ControlModes.Disabled;
                        else if ((CurrentNoConfiguration.MinCounter < 0) || (CurrentNoConfiguration.MaxCounter <= CurrentNoConfiguration.MinCounter))
                            result = Global.ControlModes.EnabledWrong;
                        break;
                    }
                case "SelectedNoConfiguration\\MinCounter":
                    {
                        if (SelectedNoConfiguration == null)
                            result = Global.ControlModes.Disabled;
                        else if ((SelectedNoConfiguration.MinCounter < 0) || (SelectedNoConfiguration.MaxCounter <= SelectedNoConfiguration.MinCounter))
                            result = Global.ControlModes.EnabledWrong;
                        break;
                    }
                case "CurrentNoConfiguration\\MaxCounter":
                    {
                        if (CurrentNoConfiguration == null)
                            result = Global.ControlModes.Disabled;
                        else if ((CurrentNoConfiguration.MaxCounter < 0) || (CurrentNoConfiguration.MaxCounter <= CurrentNoConfiguration.MinCounter))
                            result = Global.ControlModes.EnabledWrong;
                        break;
                    }
                case "SelectedNoConfiguration\\MaxCounter":
                    {
                        if (SelectedNoConfiguration == null)
                            result = Global.ControlModes.Disabled;
                        else if ((SelectedNoConfiguration.MaxCounter < 0) || (SelectedNoConfiguration.MaxCounter <= SelectedNoConfiguration.MinCounter))
                            result = Global.ControlModes.EnabledWrong;
                        break;
                    }
            }

            return result;
        }


        /// <summary>
        /// Diese Methoden sind nur innerhalb der Serverseite verfügbar und können nicht
        /// direkt über Services bereitgestellt werden.
        /// Für die Bereitstellung für einen Service, sind diese mittels AddCommand
        /// zu registrieren
        /// </summary>
        [ACMethodCommand("VBNoConfiguration", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public async Task Save()
        {
            await OnSave();
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
        /// Undoes the save.
        /// </summary>
        [ACMethodCommand("VBNoConfiguration", "en{'Undo'}de{'Nicht speichern'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost)]
        public void UndoSave()
        {
            OnUndoSave();
        }

        /// <summary>
        /// Determines whether [is enabled undo save].
        /// </summary>
        /// <returns><c>true</c> if [is enabled undo save]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledUndoSave()
        {
            return OnIsEnabledUndoSave();
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        [ACMethodInteraction("VBNoConfiguration", "en{'Load'}de{'Laden'}", (short)MISort.Load, false, "SelectedNoConfiguration")]
        public async void Load(bool requery = false)
        {
            if (SelectedNoConfiguration != null && await ACSaveOrUndoChanges())
            {
                CurrentNoConfiguration = (Root.NoManager as ACVBNoManager).LoadNoConfiguration(SelectedNoConfiguration.VBNoConfigurationID, Db);
                ACState = Const.SMEdit;

            }
        }

        /// <summary>
        /// Determines whether [is enabled load].
        /// </summary>
        /// <returns><c>true</c> if [is enabled load]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledLoad()
        {
            return SelectedNoConfiguration != null;
        }

        /// <summary>
        /// Loads the no configuration item.
        /// </summary>
        [ACMethodCommand("VBNoConfiguration", "en{'Load Configuration'}de{'Konfiguration laden'}", (short)MISort.Load)]
        public void LoadNoConfigurationItem()
        {
            if (!IsEnabledLoadNoConfigurationItem())
                return;
            CurrentNoConfiguration = (Root.NoManager as ACVBNoManager).LoadNoConfiguration(SelectedNoConfiguration.VBNoConfigurationID, Db);
        }

        /// <summary>
        /// Determines whether [is enabled load no configuration item].
        /// </summary>
        /// <returns><c>true</c> if [is enabled load no configuration item]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledLoadNoConfigurationItem()
        {
            return SelectedNoConfiguration != null;
        }

        /// <summary>
        /// News this instance.
        /// </summary>
        [ACMethodCommand("VBNoConfiguration", Const.New, (short)MISort.New, true)]
        public void New()
        {
            CurrentNoConfiguration = (Root.NoManager as ACVBNoManager).NewNoConfiguration(Db,false);
            ACState = Const.SMNew;
        }

        /// <summary>
        /// Determines whether [is enabled new].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNew()
        {
            return true;
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        [ACMethodInteraction("VBNoConfiguration", Const.Delete, (short)MISort.Delete, true, "CurrentNoConfiguration")]
        public void Delete()
        {
            Msg msg = CurrentNoConfiguration.DeleteACObject(Db, true);
            if (msg != null)
            {
                Messages.MsgAsync(msg);
                return;
            }

            ACSaveChanges();

            if (AccessPrimary == null) return; AccessPrimary.NavList.Remove(CurrentNoConfiguration);
            SelectedNoConfiguration = AccessPrimary.NavList.FirstOrDefault();
            Load();
        }

        /// <summary>
        /// Determines whether [is enabled delete].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDelete()
        {
            return CurrentNoConfiguration != null;
        }

        /// <summary>
        /// Searches this instance.
        /// </summary>
        [ACMethodCommand("VBNoConfiguration", "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            AccessPrimary.NavSearch(Db);
            OnPropertyChanged("NoConfigurationList");
        }

        #endregion


        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(Save):
                    _ = Save();
                    return true;
                case nameof(IsEnabledSave):
                    result = IsEnabledSave();
                    return true;
                case nameof(UndoSave):
                    UndoSave();
                    return true;
                case nameof(IsEnabledUndoSave):
                    result = IsEnabledUndoSave();
                    return true;
                case nameof(Load):
                    Load(acParameter.Count() == 1 ? (Boolean)acParameter[0] : false);
                    return true;
                case nameof(IsEnabledLoad):
                    result = IsEnabledLoad();
                    return true;
                case nameof(LoadNoConfigurationItem):
                    LoadNoConfigurationItem();
                    return true;
                case nameof(IsEnabledLoadNoConfigurationItem):
                    result = IsEnabledLoadNoConfigurationItem();
                    return true;
                case nameof(New):
                    New();
                    return true;
                case nameof(IsEnabledNew):
                    result = IsEnabledNew();
                    return true;
                case nameof(Delete):
                    Delete();
                    return true;
                case nameof(IsEnabledDelete):
                    result = IsEnabledDelete();
                    return true;
                case nameof(Search):
                    Search();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }
}
