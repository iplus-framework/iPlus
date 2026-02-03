// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-19-2013
// ***********************************************************************
// <copyright file="BSOiPlusPackage.cs" company="gip mbh, Oftersheim, Germany">
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
using gip.core.manager;
using gip.core.autocomponent;
using System.Collections.ObjectModel;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'iPlus Packages'}de{'iPlus Pakete'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACPackage.ClassName)]
    public class BSOiPlusPackage : ACBSONav 
    {
        #region c´tors

        /// <summary>
        /// Initializes a new instance of the <see cref="BSOiPlusPackage" /> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOiPlusPackage(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
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
            Search();
            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            this._CurrentACClass = null;
            bool done = await base.ACDeInit(deleteACClassTask);
            if (_AccessPrimary != null)
            {
                _AccessPrimary.ACDeInit(false);
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

        #region BSO->ACProperty

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

        public override IAccessNav AccessNav { get { return AccessPrimary; } }
        /// <summary>
        /// The _ access primary
        /// </summary>
        ACAccessNav<ACPackage> _AccessPrimary;
        /// <summary>
        /// Gets the access primary.
        /// </summary>
        /// <value>The access primary.</value>
        [ACPropertyAccessPrimary(590, "ACPackage")]
        public ACAccessNav<ACPackage> AccessPrimary
        {
            get
            {
                if (_AccessPrimary == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, PrimaryNavigationquery(), ACType.ACIdentifier);
                    _AccessPrimary = navACQueryDefinition.NewAccessNav<ACPackage>(ACPackage.ClassName, this);
                }
                return _AccessPrimary;
            }
        }

        /// <summary>
        /// Gets or sets the selected AC package.
        /// </summary>
        /// <value>The selected AC package.</value>
        [ACPropertySelected(501, "ACPackage")]
        public ACPackage SelectedACPackage
        {
            get
            {
                return AccessPrimary.Selected;
            }
            set
            {
                AccessPrimary.Selected = value;
                OnPropertyChanged("SelectedACPackage");
            }
        }

        /// <summary>
        /// Gets or sets the current AC package.
        /// </summary>
        /// <value>The current AC package.</value>
        [ACPropertyCurrent(502, "ACPackage")]
        public ACPackage CurrentACPackage
        {
            get
            {
                return AccessPrimary.Current;
            }
            set
            {
                AccessPrimary.Current = value;
                OnPropertyChanged("CurrentACPackage");
                OnPropertyChanged("ACClassList");
            }
        }

        /// <summary>
        /// Gets the AC package list.
        /// </summary>
        /// <value>The AC package list.</value>
        [ACPropertyList(503, "ACPackage")]
        public IEnumerable<ACPackage> ACPackageList
        {
            get
            {
                return AccessPrimary.NavList;
            }
        }

        /// <summary>
        /// The _ current AC class
        /// </summary>
        ACClass _CurrentACClass;
        /// <summary>
        /// Gets or sets the current AC class.
        /// </summary>
        /// <value>The current AC class.</value>
        [ACPropertyCurrent(504, "ACClass")]
        public ACClass CurrentACClass
        {
            get
            {
                return _CurrentACClass;
            }
            set
            {
                _CurrentACClass = value;
                OnPropertyChanged("CurrentACClass");
            }
        }

        /// <summary>
        /// Gets the AC class list.
        /// </summary>
        /// <value>The AC class list.</value>
        [ACPropertyList(505, "ACClass")]
        public ObservableCollection<ACClass> ACClassList
        {
            get
            {
                if (CurrentACPackage == null)
                    return null;
                return new ObservableCollection<ACClass>(CurrentACPackage.ACClass_ACPackage);
            }
        }
        #endregion

        #region BSO->ACMethod
        /// <summary>
        /// Saves this instance.
        /// </summary>
        [ACMethodCommand("ACPackage", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void Save()
        {
            OnSave();
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
        [ACMethodCommand("ACPackage", "en{'Undo'}de{'Nicht speichern'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost)]
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
        [ACMethodInteraction("ACPackage", "en{'Load'}de{'Laden'}", (short)MISort.Load, false, "SelectedACPackage", Global.ACKinds.MSMethodPrePost)]
        public void Load(bool requery = false)
        {
            if (!PreExecute("Load"))
                return;
            LoadEntity<ACPackage>(requery, () => SelectedACPackage, () => CurrentACPackage, c => CurrentACPackage = c,
                        Db.ACPackage.Where(c => c.ACPackageID == SelectedACPackage.ACPackageID));
            PostExecute("Load");
        }

        /// <summary>
        /// Determines whether [is enabled load].
        /// </summary>
        /// <returns><c>true</c> if [is enabled load]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledLoad()
        {
            return SelectedACPackage != null;
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        [ACMethodInteraction("ACPackage", Const.Delete, (short)MISort.Delete, true, "CurrentACPackage", Global.ACKinds.MSMethodPrePost)]
        public async void Delete()
        {
            if (!PreExecute("Delete")) return;
            Msg msg = CurrentACPackage.DeleteACObject(Db, true);
            if (msg != null)
            {
                await Messages.MsgAsync(msg);
                return;
            }

            PostExecute("Delete");
        }

        /// <summary>
        /// Determines whether [is enabled delete].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDelete()
        {
            return CurrentACPackage != null && !CurrentACPackage.ACClass_ACPackage.Any();
        }

        /// <summary>
        /// Searches this instance.
        /// </summary>
        [ACMethodCommand("ACPackage", "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            AccessPrimary.NavSearch(Db);
            OnPropertyChanged("ACPackageList");
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
