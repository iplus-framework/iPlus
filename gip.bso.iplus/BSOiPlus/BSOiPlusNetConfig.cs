using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using gip.core.datamodel;
using gip.core.autocomponent;
namespace gip.bso.iplus
{
    /// <summary>
    /// Network configuration for Applications
    /// </summary>
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'Network Configuration'}de{'Netzwerk Konfiguration'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + VBUserInstance.ClassName)]
    public class BSOiPlusNetConfig : ACBSONav 
    {
        #region c´tors
        public BSOiPlusNetConfig(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            //DatabaseMode = DatabaseModes.OwnDB;
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            Search();
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool done = base.ACDeInit(deleteACClassTask);
            if (_AccessPrimary != null)
            {
                _AccessPrimary.ACDeInit(false);
                _AccessPrimary = null;
            }
            if (_LastUserInstance != null)
            {
                _LastUserInstance.PropertyChanged -= _LastUserInstance_PropertyChanged;
                _LastUserInstance = null;
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

        public override IAccessNav AccessNav { get { return AccessPrimary; } }
        ACAccessNav<VBUserInstance> _AccessPrimary;
        [ACPropertyAccessPrimary(590, "UserInstance")]
        public ACAccessNav<VBUserInstance> AccessPrimary
        {
            get
            {
                if (_AccessPrimary == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, PrimaryNavigationquery(), ACType.ACIdentifier);
                    _AccessPrimary = navACQueryDefinition.NewAccessNav<VBUserInstance>("UserInstance", this);
                    _AccessPrimary.NavSearchExecuting += _AccessPrimary_NavSearchExecuting;
                    (Database as Database).VBUserInstance.Where(c => c.VBUser.VBUserACProject_VBUser.Where(d => d.IsServer).Any());
                }
                return _AccessPrimary;
            }
        }

        IQueryable<VBUserInstance> _AccessPrimary_NavSearchExecuting(IQueryable<VBUserInstance> result)
        {
            ObjectQuery<VBUserInstance> query = result as ObjectQuery<VBUserInstance>;
            if (query != null)
                result = query.Where(c => c.VBUser.VBUserACProject_VBUser.Where(d => d.IsServer).Any());
            return result;
        }

        private VBUserInstance _LastUserInstance;
        [ACPropertyCurrent(501, "UserInstance")]
        public VBUserInstance CurrentUserInstance
        {
            get
            {
                return AccessPrimary.Current;
            }
            set
            {
                if (_LastUserInstance != null)
                    _LastUserInstance.PropertyChanged -= _LastUserInstance_PropertyChanged;
                AccessPrimary.Current = value;
                _LastUserInstance = value;
                if (_LastUserInstance != null)
                    _LastUserInstance.PropertyChanged += _LastUserInstance_PropertyChanged;
                OnPropertyChanged("CurrentUserInstance");
            }
        }

        void _LastUserInstance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ServiceAppEnbledHTTP" || e.PropertyName == "UseIPV6" || e.PropertyName == "NameResolutionOn")
                OnPropertyChanged("CurrentUserInstance");
        }


        [ACPropertySelected(502, "UserInstance")]
        public VBUserInstance SelectedUserInstance
        {
            get
            {
                return AccessPrimary.Selected;
            }
            set
            {
                AccessPrimary.Selected = value;
                OnPropertyChanged("SelectedUserInstance");
            }
        }

        [ACPropertyList(503, "UserInstance")]
        public IEnumerable<VBUserInstance> UserInstanceList
        {
            get
            {
                return AccessPrimary.NavList ;
            }
        }

        #endregion

        #region Methods
        [ACMethodCommand("UserInstance", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void Save()
        {
            OnSave();
        }

        public bool IsEnabledSave()
        {
            return OnIsEnabledSave();
        }

        [ACMethodCommand("UserInstance", "en{'Undo'}de{'Nicht speichern'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost)]
        public void UndoSave()
        {
            OnUndoSave();
        }

        public bool IsEnabledUndoSave()
        {
            return OnIsEnabledUndoSave();
        }

        [ACMethodInteraction("UserInstance", "en{'Load'}de{'Laden'}", (short)MISort.Load, false, "", Global.ACKinds.MSMethodPrePost)]
        public void Load(bool requery = false)
        {
            if (!PreExecute("Load"))
                return;
            LoadEntity<VBUserInstance>(requery, () => SelectedUserInstance, () => CurrentUserInstance, c => CurrentUserInstance = c,
                        Db.VBUserInstance.Where(c => c.VBUserInstanceID == SelectedUserInstance.VBUserInstanceID));
            PostExecute("Load");
        }

        public bool IsEnabledLoad()
        {
            return SelectedUserInstance != null;
        }

        [ACMethodInteraction("UserInstance", "en{'Delete'}de{'Löschen'}", (short)MISort.Delete, true)]
        public void Delete()
        {
            Msg msg = CurrentUserInstance.DeleteACObject(Db, true);
            if (msg != null)
            {
                Messages.Msg(msg);
                return;
            }

            Search();
        }

        public bool IsEnabledDelete()
        {
            return true;
        }

        [ACMethodCommand("UserInstance", "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            AccessPrimary.NavSearch(Db);
            OnPropertyChanged("UserInstanceList");
        }

        /// <summary>Called inside the GetControlModes-Method to get the Global.ControlModes from derivations.
        /// This method should be overriden in the derivations to dynmically control the presentation mode depending on the current value which is bound via VBContent</summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            if (vbControl == null)
                return base.OnGetControlModes(vbControl);

            Global.ControlModes result = base.OnGetControlModes(vbControl);
            if (result < Global.ControlModes.Enabled)
                return result;
            if (CurrentUserInstance != null)
            {
                switch (vbControl.VBContent)
                {
                    case "CurrentUserInstance\\ServicePortHTTP":
                            return CurrentUserInstance.ServiceAppEnbledHTTP ? Global.ControlModes.Enabled : Global.ControlModes.Disabled;
                    case "CurrentUserInstance\\ServerIPV6":
                            return CurrentUserInstance.UseIPV6 ? Global.ControlModes.Enabled : Global.ControlModes.Disabled;
                    case "CurrentUserInstance\\ServerIPV4":
                            return CurrentUserInstance.UseIPV6 ? Global.ControlModes.Disabled : Global.ControlModes.Enabled;
                    case "CurrentUserInstance\\Hostname":
                            return CurrentUserInstance.NameResolutionOn ? Global.ControlModes.Enabled : Global.ControlModes.Disabled;
                    case "CurrentUserInstance\\UseTextEncoding":
                    case "CurrentUserInstance\\ServicePortTCP":
                            return CurrentUserInstance.ServiceAppEnbledHTTP ? Global.ControlModes.Disabled : Global.ControlModes.Enabled;
                }
            }

            return result;
        }
        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"Save":
                    Save();
                    return true;
                case"IsEnabledSave":
                    result = IsEnabledSave();
                    return true;
                case"UndoSave":
                    UndoSave();
                    return true;
                case"IsEnabledUndoSave":
                    result = IsEnabledUndoSave();
                    return true;
                case"Load":
                    Load(acParameter.Count() == 1 ? (Boolean)acParameter[0] : false);
                    return true;
                case"IsEnabledLoad":
                    result = IsEnabledLoad();
                    return true;
                case"Delete":
                    Delete();
                    return true;
                case"IsEnabledDelete":
                    result = IsEnabledDelete();
                    return true;
                case"Search":
                    Search();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }
}
