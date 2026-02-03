// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACHelpManager'}de{'ACHelpManager'}", Global.ACKinds.TPARole, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACHelpManager : PARole
    {

        #region Configuration

        #region Configuration -> Properties

        private ACPropertyConfigValue<string> _SearchDocumentRelativeURL;
        [ACPropertyConfig("en{'Search Documents Relative URL'}de{'Dokumente durchsuchen Relative URL'}")]
        public string SearchDocumentRelativeURL
        {
            get
            {
                return _SearchDocumentRelativeURL.ValueT;
            }
            set
            {
                _SearchDocumentRelativeURL.ValueT = value;
            }
        }

        private ACPropertyConfigValue<string> _SearchRelativeURL;
        [ACPropertyConfig("en{'Search Relative URL'}de{'Suchen Relative URL'}")]
        public string SearchRelativeURL
        {
            get
            {
                return _SearchRelativeURL.ValueT;
            }
            set
            {
                _SearchRelativeURL.ValueT = value;
            }
        }

        private ACPropertyConfigValue<string> _DocumentRelativeURL;
        [ACPropertyConfig("en{'Document Relative URL'}de{'Dokumente Relative URL'}")]
        public string DocumentRelativeURL
        {
            get
            {
                return _DocumentRelativeURL.ValueT;
            }
            set
            {
                _DocumentRelativeURL.ValueT = value;
            }
        }

        #endregion


        #region Configuration -> Methods
        private void LoadConfiguration()
        {
            _SearchDocumentRelativeURL = new ACPropertyConfigValue<string>(this, "SearchDocumentRelativeURL", @"{lang}/documentation/Document/Get?filter.WorkspaceURLs={urls}");
            _SearchRelativeURL = new ACPropertyConfigValue<string>(this, "SearchRelativeURL", @"{lang}/Search/Json?filter.SearchPharse={searchPharse}");
            _DocumentRelativeURL = new ACPropertyConfigValue<string>(this, "DocumentRelativeURL", @"{lang}/Document/Json/{documentNo}");
        }

        private void UnloadConfiguration()
        {

        }

        #endregion

        #endregion

        #region c´tors
        public ACHelpManager(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        protected override void Construct(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            base.Construct(acType, content, parentACObject, parameter, acIdentifier);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            LoadConfiguration();
            return base.ACInit(startChildMode);
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            UnloadConfiguration();
            return await base.ACDeInit(deleteACClassTask);
        }

        public const string C_DefaultServiceACIdentifier = "HelpManager";

        #endregion

        #region General
        public string GetRootDocURL()
        {
            return null;
        }
        #endregion

        #region Attach / Detach
        public static ACHelpManager GetServiceInstance(ACComponent requester)
        {
            return GetServiceInstance<ACHelpManager>(requester, C_DefaultServiceACIdentifier, CreationBehaviour.OnlyLocal);
        }

        public static ACRef<ACHelpManager> ACRefToServiceInstance(ACComponent requester)
        {
            ACHelpManager serviceInstance = GetServiceInstance(requester);
            if (serviceInstance != null)
                return new ACRef<ACHelpManager>(serviceInstance, requester);
            return null;
        }
        #endregion

        #region Login & Register

        #region Login & Register -> Login

        

        /// <summary>
        ///  Authentification client method
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        public async Task<ActionResult<Tuple<Guid, string>>> Authenticate(Login loginModel)
        {
            ActionResult<Tuple<Guid, string>> actionResult = new ActionResult<Tuple<Guid, string>>();
            try
            {
                KeyValuePair<bool, List<BasicMessage>> loginValidation = ValidateLogin(loginModel);
                if (loginValidation.Key)
                {
                    HttpClient client = new HttpClient();
                    string baseAddress = HelpConfigSection.Settings.HelpPageRootURL +
                                                 HelpConfigSection.Settings.LoginRelativeURL.Replace("{lang}", "en");

                    // Create the query string
                    var queryString = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("Email", loginModel.Email),
                        new KeyValuePair<string, string>("Password", loginModel.Password)
                    });

                    // Combine base address and query string
                    string requestUri = $"{baseAddress}?{ queryString.ReadAsStringAsync()}";

                    // Send the request
                    HttpResponseMessage response = await client.GetAsync(requestUri);

                    if (response.IsSuccessStatusCode)
                    {
                        // Handle success
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseBody);
                        actionResult.Success = true;
                    }
                    else
                    {
                        // Handle error
                        Console.WriteLine($"Error: {response.StatusCode}");
                        actionResult.Success = false;
                    }
                }
                else
                {
                    actionResult.Messages.AddRange(loginValidation.Value);
                }
                // TODO: there is place to call service 
            }
            catch (Exception ec)
            {
                actionResult.Success = false;
                BasicMessage msg = new BasicMessage() { MessageLevel = MessageLevelEnum.Error, Message = ec.Message };
                actionResult.Messages.Add(msg);
            }
            return actionResult;
        }


        private KeyValuePair<bool, List<BasicMessage>> ValidateLogin(Login loginModel)
        {
            bool isValid = true;
            List<BasicMessage> msgList = new List<BasicMessage>();

            // email
            if (string.IsNullOrEmpty(loginModel.Email))
            {
                isValid = false;
                msgList.Add(new BasicMessage()
                {
                    MessageLevel = MessageLevelEnum.Error,
                    Message = @"Email is requiered!"
                });
            }
            else
            {
                try
                {
                    new MailAddress(loginModel.Email);
                }
                catch (Exception ec)
                {
                    isValid = false;
                    msgList.Add(new BasicMessage()
                    {
                        MessageLevel = MessageLevelEnum.Error,
                        Message = @"Email is not in valid format!"
                    });

                    string msgEc = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msgEc += " Inner:" + ec.InnerException.Message;

                    Messages.LogException("ACHelpManager", "ValidateLogin", msgEc);

                }
            }

            // password
            if (string.IsNullOrEmpty(loginModel.Password) || loginModel.Password.Length < 6 || loginModel.Password.Length > 20)
            {
                isValid = false;
                msgList.Add(new BasicMessage()
                {
                    MessageLevel = MessageLevelEnum.Error,
                    Message = @"Password length should be between 6 and 20 charachters!"
                });
            }

            return new KeyValuePair<bool, List<BasicMessage>>(isValid, msgList);
        }

        #endregion


        #region  Login & Register -> Register


        /// <summary>
        /// Registration client method
        /// </summary>
        /// <param name="registerModel"></param>
        /// <returns></returns>
        public ActionResult<Register> ActionRegistration(Register registerModel)
        {
            ActionResult<Register> actionResult = new ActionResult<Register>();
            try
            {
                KeyValuePair<bool, List<BasicMessage>> registerValidation = ValidateRegister(registerModel);
                if(registerValidation.Key)
                {
                    // TODO: there is place to call service
                    // Proceeed with registration
                    BasicMessage registerSuccessMessage = new BasicMessage()
                    {
                        MessageLevel = MessageLevelEnum.Success,
                        Message = "Registration was successfully! You will become mail for account activation intermediatley! After activation you can go to login!"
                    };
                    actionResult.Success = true; // Is temp so
                }
                else
                {
                    actionResult.Messages.AddRange(registerValidation.Value);
                }
            }
            catch (Exception ec)
            {
                actionResult.Success = false;
                BasicMessage msg = new BasicMessage() { MessageLevel = MessageLevelEnum.Error, Message = ec.Message };
                actionResult.Messages.Add(msg);
            }
            return actionResult;
        }

        public KeyValuePair<bool, List<BasicMessage>> ValidateRegister(Register registerModel)
        {
            bool isValid = true;
            List<BasicMessage> msgList = new List<BasicMessage>();

            // email
            if (string.IsNullOrEmpty(registerModel.email))
            {
                isValid = false;
                msgList.Add(new BasicMessage()
                {
                    MessageLevel = MessageLevelEnum.Error,
                    Message = @"Email is requiered!"
                });
            }
            else
            {
                try
                {
                    new MailAddress(registerModel.email);
                }
                catch (Exception ec)
                {
                    isValid = false;
                    msgList.Add(new BasicMessage()
                    {
                        MessageLevel = MessageLevelEnum.Error,
                        Message = @"Email is not in valid format!"
                    });
                    string msg = ec.Message;
                    if (ec.InnerException != null && ec.InnerException.Message != null)
                        msg += " Inner:" + ec.InnerException.Message;

                    Root.Messages.LogException("WcfAdsClientChannel", "DeinitWcfAdsClientChannel", msg);
                }
            }

            // password
            bool isPasswordValid = false;
            if (!isPasswordValid)
            {
                if (registerModel.password.Length < 6 || registerModel.password.Length > 20)
                {
                    isValid = false;
                    msgList.Add(new BasicMessage()
                    {
                        MessageLevel = MessageLevelEnum.Error,
                        Message = @"Password length should be between 6 and 20 charachters!"
                    });
                }
                else if (!PasswordComplexChecker.IsPasswordComplex(registerModel.password, 1, 1, 1))
                {
                    isValid = false;
                    msgList.Add(new BasicMessage()
                    {
                        MessageLevel = MessageLevelEnum.Error,
                        Message = @"Password should have one big letter, one number and one spec charachter!"
                    });
                }
            }

            // password repeated
            if (registerModel.password != registerModel.passwordRepeated)
            {
                isValid = false;
                msgList.Add(new BasicMessage()
                {
                    MessageLevel = MessageLevelEnum.Error,
                    Message = @"Repeated password don't match!"
                });
            }

            // Gender
            if (string.IsNullOrEmpty(registerModel.Gender) || (registerModel.Gender != "M" && registerModel.Gender != "F"))
            {
                isValid = false;
                msgList.Add(new BasicMessage()
                {
                    MessageLevel = MessageLevelEnum.Error,
                    Message = @"Gender is reqiuered!"
                });
            }

            // LangaugeCode
            if (string.IsNullOrEmpty(registerModel.LangaugeCode) || (registerModel.LangaugeCode != "en" && registerModel.LangaugeCode != "de"))
            {
                isValid = false;
                msgList.Add(new BasicMessage()
                {
                    MessageLevel = MessageLevelEnum.Error,
                    Message = @"User language is reqiered!"
                });
            }

            // First name
            if (string.IsNullOrEmpty(registerModel.FirstName))
            {
                isValid = false;
                msgList.Add(new BasicMessage()
                {
                    MessageLevel = MessageLevelEnum.Error,
                    Message = @"First name is requiered!"
                });
            }

            // Last name
            if (string.IsNullOrEmpty(registerModel.LastName))
            {
                isValid = false;
                msgList.Add(new BasicMessage()
                {
                    MessageLevel = MessageLevelEnum.Error,
                    Message = @"Last name is requiered!"
                });
            }

            // Validate address
            if (registerModel.AddressTypeID > 0)
            {
                if (string.IsNullOrEmpty(registerModel.PlaceName) ||
                    string.IsNullOrEmpty(registerModel.Longitude) ||
                    string.IsNullOrEmpty(registerModel.Latitude))
                {
                    isValid = false;
                    msgList.Add(new BasicMessage()
                    {
                        MessageLevel = MessageLevelEnum.Error,
                        Message = @"Address is not valid!"
                    });
                }
            }

            // Validate phone
            if (registerModel.PhoneType > 0)
            {
                if (string.IsNullOrEmpty(registerModel.PhonePrefix) ||
                    string.IsNullOrEmpty(registerModel.Phone))
                {
                    isValid = false;
                    msgList.Add(new BasicMessage()
                    {
                        MessageLevel = MessageLevelEnum.Error,
                        Message = @"Phone is not valid!"
                    });
                }
                if (string.IsNullOrEmpty(registerModel.CountryCode))
                {
                    isValid = false;
                    msgList.Add(new BasicMessage()
                    {
                        MessageLevel = MessageLevelEnum.Error,
                        Message = @"Country code for phone from address is not detected! Please define your address!"
                    });
                }
            }

            return new KeyValuePair<bool, List<BasicMessage>>(isValid, msgList);
        }

        #endregion

        #endregion

        #region Load help

        /*
         

            public ActionResult<List<ACObjectItem>> GetDocuments(string acURL)
        {
            ActionResult<List<ACObjectItem>> actionResult = new ActionResult<List<ACObjectItem>>();
            try
            {

                // TODO: there is place to call service 
            }
            catch (Exception ec)
            {
                actionResult.Success = false;
                BasicMessage msg = new BasicMessage() { MessageLevel = MessageLevelEnum.Error, Message = ec.Message };
                actionResult.Messages.Add(msg);
            }
            return actionResult;
        }
    */

        public ActionResult<ISearchResult<SearchTypeEnum>> Search(string searchPharse)
        {
            string url = SearchRelativeURL.Replace("{searchPharse}", searchPharse);
            throw new NotImplementedException();
        }

        public ActionResult<DocumentsSelectResult> SearchDocuments(List<string> acURLs)
        {
            string jsonACUrls = Newtonsoft.Json.JsonConvert.SerializeObject(acURLs);
            string url = SearchRelativeURL.Replace("{urls}", jsonACUrls);
            throw new NotImplementedException();
        }

        public ActionResult<OnePageTreeDocumentView> GetDocument(string documentNo)
        {
            string url = DocumentRelativeURL.Replace("{documentNo}", documentNo);
            throw new NotImplementedException();
        }

        public ActionResult<List<ACObjectItem>> GetFiles(string acURL)
        {
            ActionResult<List<ACObjectItem>> actionResult = new ActionResult<List<ACObjectItem>>();
            try
            {

                // TODO: there is place to call service 
            }
            catch (Exception ec)
            {
                actionResult.Success = false;
                BasicMessage msg = new BasicMessage() { MessageLevel = MessageLevelEnum.Error, Message = ec.Message };
                actionResult.Messages.Add(msg);
            }
            return actionResult;
        }

        #endregion

        #region Manupulate Files

        public void DownloadFile() {
            
        }
        public void UploadFile() { }
        public void RemoveFile() { }
        public void PreviewFile() { }

        #endregion

    }
}
