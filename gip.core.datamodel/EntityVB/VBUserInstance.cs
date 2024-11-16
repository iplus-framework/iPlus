// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Transactions;
using System.Xml;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Userinstance'}de{'Benutzerinstanz'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "ServerIPV4", "en{'Server IPV4'}de{'Server IPV4'}","", "", true)]
    [ACPropertyEntity(2, "UseIPV6", "en{'Use IPV6'}de{'Verwende IPV6'}", "", "", true)]
    [ACPropertyEntity(3, "ServerIPV6", "en{'Server IPV6'}de{'Server IPV6'}", "", "", true)]
    [ACPropertyEntity(4, "NameResolutionOn", "en{'Via name resolution'}de{'Per Namensauflösung'}", "", "", true, DefaultValue = true)]
    [ACPropertyEntity(5, "Hostname", "en{'Hostname'}de{'Hostname'}", "", "", true)]

    //TCP-Configuration
    [ACPropertyEntity(6, "ServicePortTCP", "en{'TCP-Service Portno.'}de{'TCP-Service Portnummer'}", "", "", true, DefaultValue = 8001)]
    [ACPropertyEntity(7, "UseTextEncoding", "en{'Use Textencoding'}de{'Verwende Textencoding'}", "", "", true)]
    
    // Http-Configuration
    [ACPropertyEntity(8, "ServiceAppEnbledHTTP", "en{'Enable http'}de{'Aktiviere http'}", "", "", true)]
    [ACPropertyEntity(9, "ServicePortHTTP", "en{'http-Service Portno.'}de{'http-Service Portnummer'}", "", "", true, DefaultValue = 8000)]

    [ACPropertyEntity(9999, "ServicePortObserverHTTP", "en{'deprecated-ServicePortObserverHTTP'}de{'Veraltet-ServicePortObserverHTTP'}", "", "", true)]
    [ACPropertyEntity(9999, "ServiceAppEnabledTCP", "en{'deprecated-ServiceAppEnabledTCP'}de{'Veraltet-ServiceAppEnabledTCP'}", "", "", true)]
    [ACPropertyEntity(9999, "ServiceWorkflowEnabledHTTP", "en{'deprecated-ServiceWorkflowEnabledHTTP'}de{'Veraltet-ServiceWorkflowEnabledHTTP'}", "", "", true)]
    [ACPropertyEntity(9999, "ServiceWorkflowEnabledTCP", "en{'deprecated-ServiceWorkflowEnabledTCP'}de{'Veraltet-ServiceWorkflowEnabledTCP'}", "", "", true)]
    [ACPropertyEntity(9999, "ServiceObserverEnabledTCP", "en{'deprecated-ServiceObserverEnabledTCP'}de{'Veraltet-ServiceObserverEnabledTCP'}", "", "", true)]

    [ACPropertyEntity(11, "IsUserDefined", "en{'No IP-Address change if 127.0.0.1 is set'}de{'Keine IP-Adresskorrektur wenn 127.0.0.1 gesetzt'}","", "", true)]
    [ACPropertyEntity(12, "LoginDate", "en{'Login Date'}de{'Login Datum'}","", "", true)]
    [ACPropertyEntity(13, "LogoutDate", "en{'Logout Date'}de{'Logout Datum'}","", "", true)]
    [ACPropertyEntity(14, "SessionCount", "en{'Session count'}de{'Anzahl Sitzungen'}", "", "", false)]
    [ACPropertyEntity(9999, "VBUser", "en{'User'}de{'Benutzer'}", Const.ContextDatabase + "\\VBUser", "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBUserInstance.ClassName, "en{'Userinstance'}de{'Benutzerinstanz'}", typeof(VBUserInstance), VBUserInstance.ClassName, "VBUser\\VBUserName", "VBUser\\VBUserName")]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBUserInstance>) })]
    [NotMapped]
    public partial class VBUserInstance : IACObjectEntity
    {
        public const string ClassName = "VBUserInstance";
        private const int C_SessionTimeOutDays = 5;

        #region New/Delete
        public static VBUserInstance NewACObject(Database database, IACObject parentACObject)
        {
            VBUserInstance entity = new VBUserInstance();
            entity.VBUserInstanceID = Guid.NewGuid();
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            if (parentACObject is VBUser)
            {
                entity.VBUser = parentACObject as VBUser;
            }

            entity.ServerIPV4 = "";
            entity.ServerIPV6 = "";
            entity.ServicePortHTTP = 8000;
            entity.ServicePortTCP = 8001;
            entity.ServicePortObserverHTTP = 4503;

            entity.ServiceAppEnbledHTTP = false;
            entity.ServiceAppEnabledTCP = false;
            entity.ServiceWorkflowEnabledHTTP = false;
            entity.ServiceWorkflowEnabledTCP = false;
            entity.ServiceObserverEnabledTCP = false;
            entity.Hostname = "localhost";
            entity.NameResolutionOn = true;
            entity.UseIPV6 = false;
            entity.UseTextEncoding = false;
            entity.SessionInfo = "";
            entity.SessionCount = 0;
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        #endregion

        #region IACUrl Member

        /// <summary>
        /// Returns VBUser
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to VBUser</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public override IACObject ParentACObject
        {
            get
            {
                return VBUser;
            }
        }

        #endregion

        #region IACObjectEntity Members
        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context)
        {
            base.EntityCheckAdded(user, context);
            if (VBUser == null)
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "Key",
                    Message = "Key",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "Key"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            return null;
        }

        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "VBUser\\VBUserName";
            }
        }

        #endregion

        #region SessionInfo

        [NotMapped]
        private ACMonitorObject _20030_LockSessionInfo = new ACMonitorObject(20030);

        [NotMapped]
        private List<VBUserSessionInfo> _Sessions = null;
        [NotMapped]
        private List<VBUserSessionInfo> SafeSessions
        {
            get
            {
                using (ACMonitor.Lock(_20030_LockSessionInfo))
                {
                    if (_Sessions != null)
                        return _Sessions;
                    if (String.IsNullOrEmpty(SessionInfo))
                    {
                        _Sessions = new List<VBUserSessionInfo>();
                        return _Sessions;
                    }
                    _Sessions = DeserializeSessions();
                    return _Sessions;
                }
            }
        }

        [ACPropertyInfo(20, "", "en{'Session collection'}de{'Sitzungsliste'}")]
        [NotMapped]
        public IEnumerable<VBUserSessionInfo> Sessions
        {
            get
            {
                return SafeSessions.ToArray();
            }
        }

        public IEnumerable<VBUserSessionInfo> ReloadSessions()
        {
            using (ACMonitor.Lock(_20030_LockSessionInfo))
            {
                _Sessions = null;
                return Sessions;
            }
        }

        public void LogIn(string computer, int sessionID)
        {
            using (ACMonitor.Lock(_20030_LockSessionInfo))
            {
                List<VBUserSessionInfo> sessions = SafeSessions;
                VBUserSessionInfo session = sessions.Where(c => c.Computer == computer && c.SessionID == sessionID).FirstOrDefault();
                if (session != null)
                {
                    session.LoginTime = DateTime.Now;
                    session.LogoutTime = session.LoginTime;
                }
                else
                {
                    session = new VBUserSessionInfo(computer, sessionID);
                    sessions.Add(session);
                }
                RemoveExpiredSessions(C_SessionTimeOutDays);
                SerializeSessions(sessions);
                OnPropertyChanged("Sessions");
            }
        }

        public void LogOut(string computer, int sessionID)
        {
            using (ACMonitor.Lock(_20030_LockSessionInfo))
            {
                List<VBUserSessionInfo> sessions = SafeSessions;
                VBUserSessionInfo session = sessions.Where(c => c.Computer == computer && c.SessionID == sessionID).FirstOrDefault();
                if (session != null)
                    session.LogoutTime = DateTime.Now;
                RemoveExpiredSessions(C_SessionTimeOutDays);
                SerializeSessions(sessions);
                OnPropertyChanged("Sessions");
            }
        }

        public void RemoveExpiredSessions(uint days, bool serialize = false)
        {
            DateTime dateTime = DateTime.Now.AddDays(days * -1);
            using (ACMonitor.Lock(_20030_LockSessionInfo))
            {
                List<VBUserSessionInfo> sessions = SafeSessions;
                sessions.RemoveAll(c => !c.IsLoggedOut && c.LoginTime <= dateTime);
                if (serialize)
                    SerializeSessions(sessions);
            }
        }

        private void RecalcSessionCount(List<VBUserSessionInfo> sessions)
        {
            if (sessions == null)
                SessionCount = 0;
            else
                SessionCount = sessions.Where(c => !c.IsLoggedOut).Count();
        }

        private List<VBUserSessionInfo> DeserializeSessions()
        {
            if (String.IsNullOrEmpty(SessionInfo))
                return new List<VBUserSessionInfo>();
            try
            {
                using (StringReader ms = new StringReader(SessionInfo))
                using (XmlTextReader xmlReader = new XmlTextReader(ms))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(List<VBUserSessionInfo>));
                    List<VBUserSessionInfo> result = serializer.ReadObject(xmlReader) as List<VBUserSessionInfo>;
                    if (result == null)
                        return new List<VBUserSessionInfo>();
                    return result;
                }
            }
            catch (Exception)
            {
                return new List<VBUserSessionInfo>();
            }
        }

        private void SerializeSessions(List<VBUserSessionInfo> sessions)
        {
            RecalcSessionCount(sessions);
            string xml = "";
            try
            {
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(List<VBUserSessionInfo>));
                    serializer.WriteObject(xmlWriter, sessions);
                    xml = sw.ToString();
                }
            }
            catch (Exception)
            {
            }
            SessionInfo = xml;
        }

        #endregion
    }
}
