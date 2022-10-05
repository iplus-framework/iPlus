using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.webservices
{
    public partial class CoreWebService : ICoreWebService
    {
        public WSResponse<VBUserRights> Login(string userName)
        {
            if (String.IsNullOrEmpty(userName))
                return new WSResponse<VBUserRights>(null, new Msg(eMsgLevel.Error, "Username is empty"));

            VBUserRights userRights = null;
            ACMenuItem mainMenu = null;
            using (ACMonitor.Lock(ACRoot.SRoot.Database.QueryLock_1X000))
            {
                VBUser vbUser = (ACRoot.SRoot.Database as Database).VBUser.Where(c => c.VBUserName.ToLower() == userName.ToLower()).FirstOrDefault();
                if (vbUser == null)
                    return new WSResponse<VBUserRights>(null, new Msg(eMsgLevel.Error, "User not found"));
                else if (!vbUser.MenuACClassDesignID.HasValue)
                    return new WSResponse<VBUserRights>(null, new Msg(eMsgLevel.Error, "No menu assigned to user"));
                mainMenu = vbUser.MenuACClassDesign.GetMenuEntryWithCheck(ACRoot.SRoot);
                if (mainMenu == null)
                    return new WSResponse<VBUserRights>(null, new Msg(eMsgLevel.Error, "Could not create a menu for user"));
                string defaultLanguage = VBLanguage.DefaultVBLanguage(ACRoot.SRoot.Database as Database).VBLanguageCode;
                userRights = new VBUserRights() { DefaultLanguage = defaultLanguage, Language = vbUser.VBLanguage.VBLanguageCode, UserName = vbUser.VBUserName, Initials = vbUser.Initials, Menu = new List<VBMenuItem>() };
            }

            if (userRights != null)
                TransformToVBMenuItems(mainMenu, userRights);

            Guid? sessionId = WSRestAuthorizationManager.CurrentSessionID;
            if (sessionId.HasValue)
            {
                userRights.SessionID = sessionId;
                PAJsonServiceHost myServiceHost = PAWebServiceBase.FindPAWebService<PAJsonServiceHost>(true);
                if (myServiceHost != null)
                    myServiceHost.AddSession(userRights);
            }

            List<Tuple<Type, Type>> knownTypes = new List<Tuple<Type, Type>>();
            OnGetKnownTypes4Translation(ref knownTypes);

            userRights.Translation = new WSTranslation();
            userRights.Translation.Classes = new List<ACClass>();
            var db = (ACRoot.SRoot.Database as Database);
            foreach (var matchType in knownTypes)
            {
                var iPlusACClass = db.GetACType(matchType.Item1);
                if (iPlusACClass != null)
                {
                    ACClass wsClass = new ACClass()
                    {
                        FullQName = matchType.Item2.FullName,
                        ACClassID = iPlusACClass.ACClassID,
                        ACIdentifier = iPlusACClass.ACIdentifier,
                        ACCaptionTranslation = iPlusACClass.ACCaptionTranslation,
                        ACUrlComponent = iPlusACClass.ACUrlComponent
                    };
                    userRights.Translation.Classes.Add(wsClass);
                    wsClass.Properties = new List<ACClassProperty>();
                    foreach (var iPlusProperty in iPlusACClass.Properties)
                    {
                        ACClassProperty wsProperty = new ACClassProperty()
                        {
                            ACClassPropertyID = iPlusProperty.ACClassPropertyID,
                            ACIdentifier = iPlusProperty.ACIdentifier,
                            ACCaptionTranslation = iPlusProperty.ACCaptionTranslation
                        };
                        wsClass.Properties.Add(wsProperty);
                    }
                }
            }

            return new WSResponse<VBUserRights>(userRights);
        }

        private void TransformToVBMenuItems(ACMenuItem parentMenu, VBUserRights vbUserRights)
        {
            if (parentMenu == null || parentMenu.Items == null || !parentMenu.Items.Any())
                return;
            foreach (ACMenuItem item in parentMenu.Items)
            {
                string bsoName = item.BSOName;
                if (!String.IsNullOrEmpty(bsoName))
                {
                    gip.core.datamodel.ACClass classOfBso = (ACRoot.SRoot.Database as Database).GetACType(bsoName);
                    if (classOfBso != null)
                        vbUserRights.Menu.Add(new VBMenuItem() { PageClassName = bsoName, Label = Translator.GetTranslation(classOfBso.ACIdentifier, classOfBso.ACCaptionTranslation, vbUserRights.Language) });
                }
                TransformToVBMenuItems(item, vbUserRights);
            }
        }

        public WSResponse<bool> Logout(string sessionID)
        {
            if (String.IsNullOrEmpty(sessionID))
                return new WSResponse<bool>(false, new Msg(eMsgLevel.Error, "sessionID is empty"));

            bool succ = false;
            Guid? sessionId = WSRestAuthorizationManager.CurrentSessionID;
            if (sessionId.HasValue)
            {
                PAJsonServiceHost myServiceHost = PAWebServiceBase.FindPAWebService<PAJsonServiceHost>(true);
                if (myServiceHost != null)
                    succ = myServiceHost.RemoveSession(sessionId.Value);
            }

            return new WSResponse<bool>(succ);
        }

        public WSResponse<ACClass> GetACClassByBarcode(string barcodeID)
        {
            if (string.IsNullOrEmpty(barcodeID))
                return new WSResponse<ACClass>(null, new Msg(eMsgLevel.Error, "barcodeID is empty"));

            Guid guid;
            if (!Guid.TryParse(barcodeID, out guid))
                return new WSResponse<ACClass>(null, new Msg(eMsgLevel.Error, "barcodeID is invalid"));

            gip.core.datamodel.ACClass acClass = (ACRoot.SRoot.Database as Database).GetACType(guid);
            if (acClass == null)
                return new WSResponse<ACClass>(null, new Msg(eMsgLevel.Error, "Invalid class"));

            return new WSResponse<ACClass>(new ACClass() { ACClassID = acClass.ACClassID, ACCaptionTranslation = acClass.ACCaptionTranslation, ACIdentifier = acClass.ACIdentifier, ACUrlComponent = acClass.ACUrlComponent });
        }

        protected virtual void OnGetKnownTypes4Translation(ref List<Tuple<Type,Type>> knownTypes)
        {
            knownTypes.Add(new Tuple<Type, Type>(typeof(datamodel.ACClass), typeof(webservices.ACClass)));
            knownTypes.Add(new Tuple<Type, Type>(typeof(datamodel.ACClassProperty), typeof(webservices.ACClassProperty)));
        }

        public VBUserRights InvokingUser
        {
            get
            {
                PAJsonServiceHost myServiceHost = PAWebServiceBase.FindPAWebService<PAJsonServiceHost>(true);
                return GetInvokingUser(myServiceHost);
            }
        }

        public VBUserRights GetInvokingUser(PAJsonServiceHost myServiceHost)
        {
            if (myServiceHost == null)
                return null;

            Guid? currentSessionID = WSRestAuthorizationManager.CurrentSessionID;
            if (currentSessionID.HasValue && myServiceHost != null)
            {
                return myServiceHost.GetRightsForSession(currentSessionID.Value);
            }
            return null;
        }

        public WSResponse<bool> DumpPerfLog(string perfLog)
        {
            if (!String.IsNullOrEmpty(perfLog))
            {
                PAJsonServiceHost myServiceHost = PAWebServiceBase.FindPAWebService<PAJsonServiceHost>(true);
                if (myServiceHost != null)
                {
                    try
                    {
                        string dumpFilePath = string.Format("{0}PerfLogMobile_{1:yyyyMMdd_HHmmss}.txt", myServiceHost.Messages.LogFilePath, DateTime.Now);
                        if (perfLog != null)
                            File.WriteAllText(dumpFilePath, perfLog);
                    }
                    catch (Exception e)
                    {
                        myServiceHost.Messages.LogException(myServiceHost.GetACUrl(), "DumpPerfLog", e);
                    }
                }
            }
            return new WSResponse<bool>(true) { Message = null };
        }
    }
}
