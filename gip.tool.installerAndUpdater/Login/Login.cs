// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace gip.tool.installerAndUpdater
{
    public class LoginManager
    {
        public bool IsUserLogged = false;
        internal static string UserStatusUrl = "";
        internal static string RemoteServer = @"https://iplus-framework.com";
        internal static string ControllerName = @"Deploy";

        internal static string Username;
        internal static string Password;
        
        public async Task<bool> LoginAsync(string username, string password)
        {
            string getUserStatusMehtodName = @"GetUserStatus";
            UserStatusUrl = RemoteServer + string.Format(@"/en/{0}/{1}", ControllerName, getUserStatusMehtodName);

            username = username.Trim();
            password = password.Trim();

            Username = username;
            Password = password;

            var values = new Dictionary<string, string>()
            {
                {"username", username},
                {"password", password}
            };
            using (var client = new HttpClient())
            {
                UserStatusInfo info = null;
                try
                {
                    var content = new FormUrlEncodedContent(values);
                    var responseGetUserStatus = await client.PostAsync(UserStatusUrl, content);
                    string responseString = await responseGetUserStatus.Content.ReadAsStringAsync();
                    info = JsonConvert.DeserializeObject<UserStatusInfo>(responseString);
                }
                catch(Exception)
                {
                    return false;
                }
                
                if (info != null && info.IsUsernamePassOk)
                {
                    IsUserLogged = info.IsUsernamePassOk;
                    return true;
                }
                else
                    return false;
            }
        }
    }
}
