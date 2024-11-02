// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.manager;
using gip.core.autocomponent;
using Microsoft.EntityFrameworkCore;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSOiPlusHelp
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Info about iPlus'}de{'Info über iPlus'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class BSOiPlusHelp : ACBSO
    {
        #region c´tors

        public BSOiPlusHelp(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }
        #endregion

        #region Properties

        #region Statistics

        [ACPropertyInfo(200, "", "en{'Largest number of concurrent database connections'}de{'Größte Anzahl gleichzeitiger Datenbankverbindungen'}")]
        public int MaxDBConnectionCount
        {
            get
            {
                return Root.Environment.MaxDBConnectionCount;
            }
        }

        [ACPropertyInfo(201, "", "en{'Current number of database connections'}de{'Aktuelle Anzahl von Datenbankverbindungen'}")]
        public int? CurrentDBConnectionCount
        {
            get
            {
                return Root.Environment.CurrentDBConnectionCount;
            }
        }

        [ACPropertyInfo(202, "", "en{'Largest number of concurrent network connections'}de{'Größte Anzahl gleichzeitiger Netzwerkverbindungen'}")]
        public int MaxWCFConnectionCount
        {
            get
            {
                return Root.Environment.MaxWCFConnectionCount;
            }
        }

        [ACPropertyInfo(203, "", "en{'Largest number of concurrent windows sessions'}de{'Größte Anzahl gleichzeitiger Windows-Sitzungen'}")]
        public int MaxWinSessionCount
        {
            get
            {
                return Root.Environment.MaxWinSessionCount;
            }
        }

        [ACPropertyInfo(204, "", "en{'Current number of concurrent windows sessions'}de{'Aktuelle Anzahl von Windows-Sitzungen'}")]
        public int CurrentWinSessionCount
        {
            get
            {
                return Root.Environment.CurrentWinSessionCount;
            }
        }

        [ACPropertyInfo(205, "", "en{'Maximum number of licensed windows sessions'}de{'Maximale lizenzierte Windows-Sitzungen'}")]
        public int MaxLicensedWinSessions
        {
            get
            {
                return Root.Environment.MaxLicensedWinSessions;
            }
        }

        [ACPropertyInfo(206, "", "en{'Maximum number of windows sessions is exceeded'}de{'Maximale Windows-Sitzungen sind überschritten'}")]
        public bool IsMaxWinSessionsExceeded
        {
            get
            {
                return Root.Environment.IsMaxWinSessionsExceeded;
            }
        }

        [ACPropertyInfo(207, "", "en{'Maximum number of licensed network connections'}de{'Maximale lizenzierte Netzwerk-Verbindungen'}")]
        public int MaxLicensedWCFConnections
        {
            get
            {
                return Root.Environment.MaxLicensedWCFConnections;
            }
        }

        VBUserSessionInfo _SelectedVBUserSessionInfo;
        [ACPropertySelected(260, "VBUserSessionInfo")]
        public VBUserSessionInfo SelectedVBUserSessionInfo
        {
            get
            {
                return _SelectedVBUserSessionInfo;
            }
            set
            {
                _SelectedVBUserSessionInfo = value;
                OnPropertyChanged("SelectedVBUserSessionInfo");
            }
        }

        private IEnumerable<VBUserSessionInfo> _VBUserSessionInfoList;
        [ACPropertyList(261, "VBUserSessionInfo")]
        public IEnumerable<VBUserSessionInfo> VBUserSessionInfoList
        {
            get
            {
                if (_VBUserSessionInfoList != null)
                    return _VBUserSessionInfoList;
                //_VBUserSessionInfoList = db.VBUserInstance.Where(c => c.SessionCount > 0)
                //    .ToArray()
                //    .SelectMany(c => c.Sessions)
                //    .Where(c => !c.IsLoggedOut);
                List<VBUserSessionInfo> sessions = new List<VBUserSessionInfo>();
                using (core.datamodel.Database db = new core.datamodel.Database())
                {
                    var queryLoggedOnUsers = db.VBUserInstance.Include(c => c.VBUser)
                                            .Where(c => c.SessionCount > 0)
                                            .OrderBy(c => c.VBUser.Initials);
                    foreach (var instance in queryLoggedOnUsers)
                    {
                        foreach (var session in instance.Sessions.Where(c => !c.IsLoggedOut))
                        {
                            session.UserName = String.Format("{0} ({1})", instance.VBUser.Initials, instance.VBUser.VBUserName);
                            sessions.Add(session);
                        }
                    }
                }
                _VBUserSessionInfoList = sessions;
                return _VBUserSessionInfoList;
            }
        }

        #endregion

        #region Assembly-Info
        ACAssembly _SelectedACAssembly;
        [ACPropertySelected(250, ACAssembly.ClassName)]
        public ACAssembly SelectedACAssembly
        {
            get
            {
                return _SelectedACAssembly;
            }
            set
            {
                _SelectedACAssembly = value;
                OnPropertyChanged("SelectedACAssembly");
            }
        }

        private IEnumerable<ACAssembly> _ACAssemblyList;
        [ACPropertyList(251, ACAssembly.ClassName)]
        public IEnumerable<ACAssembly> ACAssemblyList
        {
            get
            {
                if (_ACAssemblyList != null)
                    return _ACAssemblyList;
                using (core.datamodel.Database db = new core.datamodel.Database())
                {
                    _ACAssemblyList = db.ACAssembly.OrderBy(c => c.AssemblyName).ToArray();
                }
                return _ACAssemblyList;
            }
        }
        #endregion

        #endregion

        #region BSO->ACMethod
        /// <summary>
        /// Shows the info.
        /// </summary>
        [ACMethodInfo("Help", "en{'Info'}de{'Info'}", 401)]
        public void ShowInfo()
        {
            ShowDialog(this, "Info");
        }

        /// <summary>
        /// Shows the help.
        /// </summary>
        [ACMethodInfo("Help", "en{'Help'}de{'Help'}", 402)]
        public void ShowHelp()
        {
            ShowDialog(this, "Help");
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        [ACMethodInfo("Help", "en{'Close'}de{'Schließen'}", 403)]
        public void Close()
        {
            CloseTopDialog();
            this.ParentACComponent.StopComponent(this);
        }
        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"ShowInfo":
                    ShowInfo();
                    return true;
                case"ShowHelp":
                    ShowHelp();
                    return true;
                case"Close":
                    Close();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }
}
