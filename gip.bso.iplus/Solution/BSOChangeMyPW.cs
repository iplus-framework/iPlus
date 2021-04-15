// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="BSOChangeMyPW.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Data;
using System.Linq;

namespace gip.bso.iplus
{
    /*
     DblClick="!AssignGroup" VBContent="SelectedGroupID"
     DblClick="!UnassignGroup" VBContent="SelectedAssignedUserGroupID"
     */
    /// <summary>
    /// Class BSOChangeMyPW
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Change Password'}de{'Passwort ändern'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
    public class BSOChangeMyPW : ACBSO
    {
        #region c´tors
        public BSOChangeMyPW(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            //DatabaseMode = DatabaseModes.OwnDB;
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            ACClass bsoCompanyACClass = Database.ContextIPlus.ACClass.FirstOrDefault(c => c.ACIdentifier == "BSOCompany");
            var test = FindChildComponents(bsoCompanyACClass);
            var queryChilds = FindChildComponents(bsoCompanyACClass);
            ACComponent childBSO = null;
            if(queryChilds != null && queryChilds.Any())
                childBSO = queryChilds[0] as ACComponent;
            if (childBSO == null)
                childBSO = StartComponent("BSOCompany_Child", null, new object[] { }) as ACComponent;
            if (childBSO != null)
                childBSO.ACUrlCommand("!" + "SetCurrentVBUser", Root.Environment.User);
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._CheckPassword = null;
            this._CheckUser = null;
            this._CheckVBUser = null;
            this._NewPassword = null;
            this._NewPasswordVerify = null;
            return base.ACDeInit(deleteACClassTask);
        }

        public override bool IsPoolable
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Properties

        #region Change Password
        string _NewPassword;
        [ACPropertyInfo(100, "VBUser", "en{'New Password'}de{'Neues Passwort'}")]
        public string NewPassword
        {
            get
            {
                return _NewPassword;
            }
            set
            {
                _NewPassword = value;
            }
        }

        string _NewPasswordVerify;
        [ACPropertyInfo(101, "VBUser", "en{'Confirm New Password'}de{'Neues Passwort bestätigen'}")]
        public string NewPasswordVerify
        {
            get
            {
                return _NewPasswordVerify;
            }
            set
            {
                _NewPasswordVerify = value;
            }
        }
        #endregion

        #region Check User
        string _CheckUser;
        [ACPropertyInfo(110, "CheckUser", "en{'User'}de{'Benutzer'}")]
        public string CheckUser
        {
            get
            {
                return _CheckUser;
            }
            set
            {
                _CheckUser = value;
            }
        }

        string _CheckPassword;
        [ACPropertyInfo(111, "VBUser", "en{'Password'}de{'Passwort'}")]
        public string CheckPassword
        {
            get
            {
                return _CheckPassword;
            }
            set
            {
                _CheckPassword = value;
            }
        }
        #endregion

        #endregion

        #region Methods

        #region Change Password
        /// <summary>
        [ACMethodCommand("VBUser", "en{'Change Password'}de{'Password ändern'}", 100, false, Global.ACKinds.MSMethod)]
        public void ChangePW()
        {
            if (!IsEnabledChangePW())
                return;
            Root.Environment.User.CryptPassword = NewPassword;
            Root.Database.ACSaveChanges();
            NewPassword = "";
            NewPasswordVerify = "";
        }

        public bool IsEnabledChangePW()
        {
            return !(String.IsNullOrEmpty(NewPassword) || String.IsNullOrEmpty(NewPasswordVerify) || NewPassword != NewPasswordVerify || !Root.Environment.User.AllowChangePW);
        }
        #endregion

        #region Check User
        public VBDialogResult DialogResult
        {
            get;
            set;
        }

        [ACMethodInfo("Dialog", "en{'Check User Dialog'}de{'Dialog Benutzerprüfung'}", 500)]
        public VBDialogResult ShowCheckUserDialog()
        {
            if (DialogResult == null)
                DialogResult = new VBDialogResult();
            DialogResult.SelectedCommand = eMsgButton.Cancel;
            ShowDialog(this, "CheckUser");
            this.ParentACComponent.StopComponent(this);
            return DialogResult;
        }

        [ACMethodCommand("Dialog", "en{'OK'}de{'OK'}", (short)MISort.Okay)]
        public void DialogCheckUserOK()
        {
            if (!IsEnabledDialogCheckUserOK())
                return;
            DialogResult = new VBDialogResult();
            DialogResult.SelectedCommand = eMsgButton.OK;
            _CheckVBUser = null;
            CheckUser = null;
            CheckPassword = null;
            CloseTopDialog();
        }

        private VBUser _CheckVBUser;
        public bool IsEnabledDialogCheckUserOK()
        {
            if (String.IsNullOrEmpty(CheckUser) || String.IsNullOrEmpty(CheckPassword))
                return false;
            if (_CheckVBUser == null
                || _CheckVBUser != null && _CheckVBUser.VBUserNo != CheckUser)
            {
                gip.core.datamodel.Database db = Database as gip.core.datamodel.Database;
                if (db == null)
                    return false;
                _CheckVBUser = db.VBUser.Where(c => c.VBUserName == CheckUser).FirstOrDefault();
            }
            if (_CheckVBUser == null)
                return false;
            return _CheckVBUser.CheckEnteredPassword(CheckPassword);
        }


        [ACMethodCommand("Dialog", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Cancel)]
        public void DialogCheckUserCancel()
        {
            DialogResult = new VBDialogResult();
            DialogResult.SelectedCommand = eMsgButton.Cancel;
            CloseTopDialog();
        }
        #endregion

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "ChangePW":
                    ChangePW();
                    return true;
                case "IsEnabledChangePW":
                    result = IsEnabledChangePW();
                    return true;
                case "ShowCheckUserDialog":
                    result = ShowCheckUserDialog();
                    return true;
                case "DialogCheckUserOK":
                    DialogCheckUserOK();
                    return true;
                case "IsEnabledDialogCheckUserOK":
                    result = IsEnabledDialogCheckUserOK();
                    return true;
                case "DialogCheckUserCancel":
                    DialogCheckUserCancel();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }
}
