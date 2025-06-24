using System;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.communication
{
    /// <summary>
    /// Rest-Client with Digest Authentication Support
    /// </summary>
    public partial class ACRestClient
    {
        #region Additional Properties for Digest Authentication

        private string _AuthenticationType;
        [ACPropertyInfo(true, 204, "", "en{'Authentication Type'}de{'Authentifizierungs-Typ'}", "", true)]
        public string AuthenticationType
        {
            get
            {
                return _AuthenticationType ?? "Basic"; // Default to Basic for backward compatibility
            }
            set
            {
                _AuthenticationType = value;
                OnPropertyChanged("AuthenticationType");
            }
        }

        private string _Realm;
        [ACPropertyInfo(false, 205, "", "en{'Realm'}de{'Realm'}", "", true)]
        public string Realm
        {
            get
            {
                return _Realm;
            }
            set
            {
                _Realm = value;
                OnPropertyChanged("Realm");
            }
        }

        private string _Nonce;
        [ACPropertyInfo(false, 206, "", "en{'Nonce'}de{'Nonce'}", "", true)]
        public string Nonce
        {
            get
            {
                return _Nonce;
            }
            set
            {
                _Nonce = value;
                OnPropertyChanged("Nonce");
            }
        }

        private string _DigestQop;
        [ACPropertyInfo(false, 207, "", "en{'Digest QOP'}de{'Digest QOP'}", "", true)]
        public string DigestQop
        {
            get
            {
                return _DigestQop;
            }
            set
            {
                _DigestQop = value;
                OnPropertyChanged("DigestQop");
            }
        }

        private string _DigestOpaque;
        [ACPropertyInfo(false, 208, "", "en{'Digest Opaque'}de{'Digest Opaque'}", "", true)]
        public string DigestOpaque
        {
            get
            {
                return _DigestOpaque;
            }
            set
            {
                _DigestOpaque = value;
                OnPropertyChanged("DigestOpaque");
            }
        }

        private int _NonceCounter = 1;

        #endregion

        #region Enhanced Authentication Methods

        /// <summary>
        /// Enhanced method to create default request headers with support for multiple authentication types
        /// </summary>
        /// <param name="client">HttpClient instance</param>
        protected virtual void OnCreateDefaultRequestHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            
            // Clear any existing authorization header
            client.DefaultRequestHeaders.Authorization = null;

            switch (AuthenticationType?.ToUpper())
            {
                case "BEARER":
                    if (!String.IsNullOrEmpty(BearerToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
                    }
                    break;

                case "BASIC":
                    if (!String.IsNullOrEmpty(User) && !String.IsNullOrEmpty(Password))
                    {
                        client.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue(
                                    "Basic", Convert.ToBase64String(
                                        System.Text.ASCIIEncoding.ASCII.GetBytes(User + ":" + Password)));
                    }
                    break;

                case "DIGEST":
                    // Digest authentication is handled per request, not as a default header
                    break;

                default:
                    // Default behavior for backward compatibility
                    if (!String.IsNullOrEmpty(BearerToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
                    }
                    else if (!String.IsNullOrEmpty(User) && !String.IsNullOrEmpty(Password))
                    {
                        client.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue(
                                    "Basic", Convert.ToBase64String(
                                        System.Text.ASCIIEncoding.ASCII.GetBytes(User + ":" + Password)));
                    }
                    break;
            }
        }

        /// <summary>
        /// Creates digest authentication header
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="uri">Request URI</param>
        /// <param name="challengeResponse">Digest challenge from server</param>
        /// <returns>Digest authorization header value</returns>
        private string CreateDigestAuthorizationHeader(string method, string uri, string challengeResponse)
        {
            if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Password))
                return null;

            var challengeData = ParseDigestChallenge(challengeResponse);
            
            if (!challengeData.ContainsKey("realm") || !challengeData.ContainsKey("nonce"))
                return null;

            var realm = challengeData["realm"];
            var nonce = challengeData["nonce"];
            var qop = challengeData.ContainsKey("qop") ? challengeData["qop"] : null;
            var opaque = challengeData.ContainsKey("opaque") ? challengeData["opaque"] : null;

            // Update properties for future use
            Realm = realm;
            Nonce = nonce;
            DigestQop = qop;
            DigestOpaque = opaque;

            var nc = string.Format("{0:x8}", _NonceCounter++);
            var cnonce = GenerateClientNonce();

            var ha1 = CalculateMD5Hash($"{User}:{realm}:{Password}");
            var ha2 = CalculateMD5Hash($"{method}:{uri}");

            string response;
            if (!string.IsNullOrEmpty(qop))
            {
                response = CalculateMD5Hash($"{ha1}:{nonce}:{nc}:{cnonce}:{qop}:{ha2}");
            }
            else
            {
                response = CalculateMD5Hash($"{ha1}:{nonce}:{ha2}");
            }

            var authHeader = new StringBuilder();
            authHeader.Append($"Digest username=\"{User}\"");
            authHeader.Append($", realm=\"{realm}\"");
            authHeader.Append($", nonce=\"{nonce}\"");
            authHeader.Append($", uri=\"{uri}\"");
            authHeader.Append($", response=\"{response}\"");

            if (!string.IsNullOrEmpty(opaque))
                authHeader.Append($", opaque=\"{opaque}\"");

            if (!string.IsNullOrEmpty(qop))
            {
                authHeader.Append($", qop={qop}");
                authHeader.Append($", nc={nc}");
                authHeader.Append($", cnonce=\"{cnonce}\"");
            }

            return authHeader.ToString();
        }

        /// <summary>
        /// Parses digest challenge response from WWW-Authenticate header
        /// </summary>
        /// <param name="challenge">Challenge string</param>
        /// <returns>Dictionary of challenge parameters</returns>
        private Dictionary<string, string> ParseDigestChallenge(string challenge)
        {
            var result = new Dictionary<string, string>();
            
            if (string.IsNullOrEmpty(challenge) || !challenge.StartsWith("Digest", StringComparison.OrdinalIgnoreCase))
                return result;

            var challengeData = challenge.Substring(6).Trim(); // Remove "Digest"
            var regex = new Regex(@"(\w+)=""?([^"",]+)""?");
            var matches = regex.Matches(challengeData);

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 3)
                {
                    var key = match.Groups[1].Value.ToLower();
                    var value = match.Groups[2].Value.Trim('"');
                    result[key] = value;
                }
            }

            return result;
        }

        /// <summary>
        /// Generates a client nonce for digest authentication
        /// </summary>
        /// <returns>Random client nonce</returns>
        private string GenerateClientNonce()
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Calculates MD5 hash of input string
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>MD5 hash as hex string</returns>
        private string CalculateMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        #endregion

        #region Enhanced HTTP Methods with Digest Support

        #region GET
        /// <summary>
        /// GET request with digest authentication
        /// </summary>
        /// <param name="uri">Request URI</param>
        /// <returns>Response</returns>
        protected WSResponse<string> GetWithDigestAuth(Uri uri)
        {
            try
            {
                HttpClient client = Client;
                if (client == null)
                    return new WSResponse<string>(new Msg(eMsgLevel.Error, "Disconnected"));
                // First request without authorization to get the challenge
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                var response = client.SendAsync(request).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var wwwAuthenticate = response.Headers.WwwAuthenticate.FirstOrDefault();
                    if (wwwAuthenticate != null && wwwAuthenticate.Scheme.Equals("Digest", StringComparison.OrdinalIgnoreCase))
                    {
                        // Create second request with digest authorization
                        var digestHeader = CreateDigestAuthorizationHeader("GET", uri.PathAndQuery, wwwAuthenticate.ToString());
                        if (!string.IsNullOrEmpty(digestHeader))
                        {
                            var authenticatedRequest = new HttpRequestMessage(HttpMethod.Get, uri);
                            authenticatedRequest.Headers.Add("Authorization", digestHeader);
                            
                            var authenticatedResponse = client.SendAsync(authenticatedRequest).Result;
                            
                            if (authenticatedResponse.IsSuccessStatusCode)
                            {
                                IsConnected.ValueT = true;
                                string result = authenticatedResponse.Content.ReadAsStringAsync().Result;
                                return new WSResponse<string>(result, authenticatedResponse.StatusCode);
                            }
                            else
                            {
                                return new WSResponse<string>(null, new Msg(eMsgLevel.Failure, 
                                    $"{authenticatedResponse.ReasonPhrase},{authenticatedResponse.StatusCode}"));
                            }
                        }
                    }
                }
                else if (response.IsSuccessStatusCode)
                {
                    // Server didn't require authentication
                    IsConnected.ValueT = true;
                    string result = response.Content.ReadAsStringAsync().Result;
                    return new WSResponse<string>(result, response.StatusCode);
                }

                return new WSResponse<string>(null, new Msg(eMsgLevel.Failure, 
                    $"{response.ReasonPhrase},{response.StatusCode}"));
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "GetWithDigestAuth", ex);
                return new WSResponse<string>(msg);
            }
        }

        protected async Task<WSResponse<TResult>> GetAsyncWithDigestAuth<TResult>(Uri uri)
        {
            try
            {
                HttpClient client = Client;
                if (client == null)
                    return await Task.FromResult(new WSResponse<TResult>(new Msg(eMsgLevel.Error, "Disconnected")));
                // First request without authorization to get the challenge
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                var response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var wwwAuthenticate = response.Headers.WwwAuthenticate.FirstOrDefault();
                    if (wwwAuthenticate != null && wwwAuthenticate.Scheme.Equals("Digest", StringComparison.OrdinalIgnoreCase))
                    {
                        // Create second request with digest authorization
                        var digestHeader = CreateDigestAuthorizationHeader("GET", uri.PathAndQuery, wwwAuthenticate.ToString());
                        if (!string.IsNullOrEmpty(digestHeader))
                        {
                            var authenticatedRequest = new HttpRequestMessage(HttpMethod.Get, uri);
                            authenticatedRequest.Headers.Add("Authorization", digestHeader);

                            var authenticatedResponse = await client.SendAsync(authenticatedRequest);

                            if (authenticatedResponse.IsSuccessStatusCode)
                            {
                                IsConnected.ValueT = true;
                                string json = await authenticatedResponse.Content.ReadAsStringAsync();
                                var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                                return new WSResponse<TResult>(result, response.StatusCode);
                            }
                            else
                            {
                                return new WSResponse<TResult>(new Msg(eMsgLevel.Failure,
                                    $"{authenticatedResponse.ReasonPhrase},{authenticatedResponse.StatusCode}"));
                            }
                        }
                    }
                }
                else if (response.IsSuccessStatusCode)
                {
                    // Server didn't require authentication
                    IsConnected.ValueT = true;
                    string json = await response.Content.ReadAsStringAsync();
                    var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                    return new WSResponse<TResult>(result, response.StatusCode);
                }

                return new WSResponse<TResult>(new Msg(eMsgLevel.Failure,
                    $"{response.ReasonPhrase},{response.StatusCode}"));
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "GetWithDigestAuth", ex);
                return new WSResponse<TResult>(msg);
            }
        }
        #endregion

        #region POST
        /// <summary>
        /// POST request with digest authentication
        /// </summary>
        /// <param name="content">Request content</param>
        /// <param name="uri">Request URI</param>
        /// <returns>Response</returns>
        protected WSResponse<string> PostWithDigestAuth(StringContent content, Uri uri)
        {
            try
            {
                HttpClient client = Client;
                if (client == null)
                    return new WSResponse<string>(new Msg(eMsgLevel.Error, "Disconnected"));

                // Save the content as a string before it's disposed
                string contentString = content.ReadAsStringAsync().Result;
                string mediaType = content.Headers.ContentType?.MediaType ?? "application/json";
                Encoding encoding = content.Headers.ContentType?.CharSet != null ?
                    Encoding.GetEncoding(content.Headers.ContentType.CharSet) : Encoding.UTF8;

                // First request without authorization to get the challenge
                var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
                var response = client.SendAsync(request).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var wwwAuthenticate = response.Headers.WwwAuthenticate.FirstOrDefault();
                    if (wwwAuthenticate != null && wwwAuthenticate.Scheme.Equals("Digest", StringComparison.OrdinalIgnoreCase))
                    {
                        // Create second request with digest authorization
                        var digestHeader = CreateDigestAuthorizationHeader("POST", uri.PathAndQuery, wwwAuthenticate.ToString());
                        if (!string.IsNullOrEmpty(digestHeader))
                        {
                            // Create a NEW StringContent with the same data
                            var newContent = new StringContent(contentString, encoding, mediaType);
                            var authenticatedRequest = new HttpRequestMessage(HttpMethod.Post, uri) { Content = newContent };
                            authenticatedRequest.Headers.Add("Authorization", digestHeader);
                            
                            var authenticatedResponse = client.SendAsync(authenticatedRequest).Result;
                            
                            if (authenticatedResponse.IsSuccessStatusCode)
                            {
                                IsConnected.ValueT = true;
                                string result = authenticatedResponse.Content.ReadAsStringAsync().Result;
                                return new WSResponse<string>(result, authenticatedResponse.StatusCode);
                            }
                            else
                            {
                                return new WSResponse<string>(null, new Msg(eMsgLevel.Failure, 
                                    $"{authenticatedResponse.ReasonPhrase},{authenticatedResponse.StatusCode}"));
                            }
                        }
                    }
                }
                else if (response.IsSuccessStatusCode)
                {
                    // Server didn't require authentication
                    IsConnected.ValueT = true;
                    string result = response.Content.ReadAsStringAsync().Result;
                    return new WSResponse<string>(result, response.StatusCode);
                }

                return new WSResponse<string>(null, new Msg(eMsgLevel.Failure, 
                    $"{response.ReasonPhrase},{response.StatusCode}"));
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "PostWithDigestAuth", ex);
                return new WSResponse<string>(null, msg);
            }
        }

        protected async Task<WSResponse<TResult>> PostAsyncWithDigestAuth<TResult>(StringContent content, Uri uri)
        {
            try
            {
                HttpClient client = Client;
                if (client == null)
                    return new WSResponse<TResult>(new Msg(eMsgLevel.Error, "Disconnected"));

                // Save the content as a string before it's disposed
                string contentString = content.ReadAsStringAsync().Result;
                string mediaType = content.Headers.ContentType?.MediaType ?? "application/json";
                Encoding encoding = content.Headers.ContentType?.CharSet != null ?
                    Encoding.GetEncoding(content.Headers.ContentType.CharSet) : Encoding.UTF8;

                // First request without authorization to get the challenge
                var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
                var response = await client.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var wwwAuthenticate = response.Headers.WwwAuthenticate.FirstOrDefault();
                    if (wwwAuthenticate != null && wwwAuthenticate.Scheme.Equals("Digest", StringComparison.OrdinalIgnoreCase))
                    {
                        // Create second request with digest authorization
                        var digestHeader = CreateDigestAuthorizationHeader("POST", uri.PathAndQuery, wwwAuthenticate.ToString());
                        if (!string.IsNullOrEmpty(digestHeader))
                        {
                            // Create a NEW StringContent with the same data
                            var newContent = new StringContent(contentString, encoding, mediaType);
                            var authenticatedRequest = new HttpRequestMessage(HttpMethod.Post, uri) { Content = newContent };
                            authenticatedRequest.Headers.Add("Authorization", digestHeader);
                            var authenticatedResponse = await client.SendAsync(authenticatedRequest);
                            if (authenticatedResponse.IsSuccessStatusCode)
                            {
                                IsConnected.ValueT = true;
                                string json = await authenticatedResponse.Content.ReadAsStringAsync();
                                var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                                return new WSResponse<TResult>(result, response.StatusCode);
                            }
                            else
                            {
                                return new WSResponse<TResult>(new Msg(eMsgLevel.Failure,
                                    $"{authenticatedResponse.ReasonPhrase},{authenticatedResponse.StatusCode}"));
                            }
                        }
                    }
                }
                else if (response.IsSuccessStatusCode)
                {
                    // Server didn't require authentication
                    IsConnected.ValueT = true;
                    string json = await response.Content.ReadAsStringAsync();
                    var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                    return new WSResponse<TResult>(result, response.StatusCode);
                }
                return new WSResponse<TResult>(new Msg(eMsgLevel.Failure,
                    $"{response.ReasonPhrase},{response.StatusCode}"));
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "PostWithDigestAuth", ex);
                return new WSResponse<TResult>(msg);
            }
        }

        protected async Task<WSResponse<TResult>> PostAsyncWithDigestAuth<TResult, TParam>(TParam item, string uriString)
        {
            return await PostAsyncWithDigestAuth<TResult, TParam>(item, new Uri(uriString, UriKind.Relative));
        }

        protected async Task<WSResponse<TResult>> PostAsyncWithDigestAuth<TResult, TParam>(TParam item, Uri uri)
        {
            var serializedItem = JsonConvert.SerializeObject(item, DefaultJsonSerializerSettings);
            using (StringContent content = new StringContent(serializedItem, Encoding.UTF8, "application/json"))
            {
                return await PostAsyncWithDigestAuth<TResult>(content, uri);
            }
        }
        #endregion

        #region PUT

        protected WSResponse<string> PutWithDigestAuth(StringContent content, Uri uri)
        {
            try
            {
                HttpClient client = Client;
                if (client == null)
                    return new WSResponse<string>(new Msg(eMsgLevel.Error, "Disconnected"));

                // First request without authorization to get the challenge
                var request = new HttpRequestMessage(HttpMethod.Put, uri) { Content = content };
                var response = client.SendAsync(request).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var wwwAuthenticate = response.Headers.WwwAuthenticate.FirstOrDefault();
                    if (wwwAuthenticate != null && wwwAuthenticate.Scheme.Equals("Digest", StringComparison.OrdinalIgnoreCase))
                    {
                        // Create second request with digest authorization
                        var digestHeader = CreateDigestAuthorizationHeader("PUT", uri.PathAndQuery, wwwAuthenticate.ToString());
                        if (!string.IsNullOrEmpty(digestHeader))
                        {
                            var authenticatedRequest = new HttpRequestMessage(HttpMethod.Put, uri) { Content = content };
                            authenticatedRequest.Headers.Add("Authorization", digestHeader);
                            
                            var authenticatedResponse = client.SendAsync(authenticatedRequest).Result;
                            
                            if (authenticatedResponse.IsSuccessStatusCode)
                            {
                                IsConnected.ValueT = true;
                                string result = authenticatedResponse.Content.ReadAsStringAsync().Result;
                                return new WSResponse<string>(result, authenticatedResponse.StatusCode);
                            }
                            else
                            {
                                return new WSResponse<string>(null, new Msg(eMsgLevel.Failure, 
                                    $"{authenticatedResponse.ReasonPhrase},{authenticatedResponse.StatusCode}"));
                            }
                        }
                    }
                }
                else if (response.IsSuccessStatusCode)
                {
                    // Server didn't require authentication
                    IsConnected.ValueT = true;
                    string result = response.Content.ReadAsStringAsync().Result;
                    return new WSResponse<string>(result, response.StatusCode);
                }
                return new WSResponse<string>(null, new Msg(eMsgLevel.Failure, 
                    $"{response.ReasonPhrase},{response.StatusCode}"));
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "PutWithDigestAuth", ex);
                return new WSResponse<string>(null, msg);
            }
        }

        protected async Task<WSResponse<TResult>> PutAsyncWithDigestAuth<TResult>(StringContent content, Uri uri)
        {
            try
            {
                HttpClient client = Client;
                if (client == null)
                    return new WSResponse<TResult>(new Msg(eMsgLevel.Error, "Disconnected"));

                // First request without authorization to get the challenge
                var request = new HttpRequestMessage(HttpMethod.Put, uri) { Content = content };
                var response = await client.SendAsync(request);
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var wwwAuthenticate = response.Headers.WwwAuthenticate.FirstOrDefault();
                    if (wwwAuthenticate != null && wwwAuthenticate.Scheme.Equals("Digest", StringComparison.OrdinalIgnoreCase))
                    {
                        // Create second request with digest authorization
                        var digestHeader = CreateDigestAuthorizationHeader("PUT", uri.PathAndQuery, wwwAuthenticate.ToString());
                        if (!string.IsNullOrEmpty(digestHeader))
                        {
                            var authenticatedRequest = new HttpRequestMessage(HttpMethod.Put, uri) { Content = content };
                            authenticatedRequest.Headers.Add("Authorization", digestHeader);
                            var authenticatedResponse = await client.SendAsync(authenticatedRequest);
                            
                            if (authenticatedResponse.IsSuccessStatusCode)
                            {
                                IsConnected.ValueT = true;
                                string json = await authenticatedResponse.Content.ReadAsStringAsync();
                                var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                                return new WSResponse<TResult>(result, response.StatusCode);
                            }
                            else
                            {
                                return new WSResponse<TResult>(new Msg(eMsgLevel.Failure,
                                    $"{authenticatedResponse.ReasonPhrase},{authenticatedResponse.StatusCode}"));
                            }
                        }
                    }
                }
                else if (response.IsSuccessStatusCode)
                {
                    // Server didn't require authentication
                    IsConnected.ValueT = true;
                    string json = await response.Content.ReadAsStringAsync();
                    var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                    return new WSResponse<TResult>(result, response.StatusCode);
                }
                return new WSResponse<TResult>(new Msg(eMsgLevel.Failure,
                    $"{response.ReasonPhrase},{response.StatusCode}"));
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "PutWithDigestAuth", ex);
                return new WSResponse<TResult>(msg);
            }
        }

        protected async Task<WSResponse<TResult>> PutAsyncWithDigestAuth<TResult, TParam>(TParam item, string uriString)
        {
            return await PutAsyncWithDigestAuth<TResult, TParam>(item, new Uri(uriString, UriKind.Relative));
        }

        protected async Task<WSResponse<TResult>> PutAsyncWithDigestAuth<TResult, TParam>(TParam item, Uri uri)
        {
            var serializedItem = JsonConvert.SerializeObject(item, DefaultJsonSerializerSettings);
            using (StringContent content = new StringContent(serializedItem, Encoding.UTF8, "application/json"))
            {
                return await PutAsyncWithDigestAuth<TResult>(content, uri);
            }
        }
        #endregion

        #endregion
    }
}