using gip.core.autocomponent;
using gip.core.datamodel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;

namespace gip.core.webservices
{
    /// <summary>
    /// Provides context information for HttpListener-based web services.
    /// Replaces WebOperationContext for non-WCF environments.
    /// </summary>
    public static class HttpListenerWebContext
    {
        [ThreadStatic]
        private static Guid? _currentSessionID;

        [ThreadStatic]
        private static int? _currentServicePort;

        public static Guid? CurrentSessionID
        {
            get => _currentSessionID;
            set => _currentSessionID = value;
        }

        public static int? CurrentServicePort
        {
            get => _currentServicePort;
            set => _currentServicePort = value;
        }

        public static void Clear()
        {
            _currentSessionID = null;
            _currentServicePort = null;
        }
    }

    /// <summary>
    /// Represents a route to a web service operation
    /// </summary>
    public class WebServiceRoute
    {
        public string HttpMethod { get; set; }
        public string UriTemplate { get; set; }
        public Regex UriRegex { get; set; }
        public List<string> ParameterNames { get; set; }
        public MethodInfo Method { get; set; }
        public string OperationName { get; set; }

        public WebServiceRoute()
        {
            ParameterNames = new List<string>();
        }

        /// <summary>
        /// Attempts to match the route against a URL and extracts parameters
        /// </summary>
        public bool TryMatch(string httpMethod, string path, out Dictionary<string, string> parameters)
        {
            parameters = new Dictionary<string, string>();

            if (!HttpMethod.Equals(httpMethod, StringComparison.OrdinalIgnoreCase))
                return false;

            var match = UriRegex.Match(path);
            if (!match.Success)
                return false;

            for (int i = 0; i < ParameterNames.Count; i++)
            {
                parameters[ParameterNames[i]] = match.Groups[i + 1].Value;
            }

            return true;
        }
    }

    /// <summary>
    /// Registry for web service routes with reflection-based routing
    /// </summary>
    public class WebServiceRouteRegistry
    {
        private List<WebServiceRoute> _routes = new List<WebServiceRoute>();

        /// <summary>
        /// Registers all routes from a service interface type
        /// </summary>
        public void RegisterRoutes(Type serviceInterfaceType)
        {
            // Register methods from the interface itself
            RegisterRoutesFromType(serviceInterfaceType);

            // Register methods from all base interfaces
            foreach (var baseInterface in serviceInterfaceType.GetInterfaces())
            {
                RegisterRoutesFromType(baseInterface);
            }
        }

        private void RegisterRoutesFromType(Type interfaceType)
        {
            foreach (var method in interfaceType.GetMethods())
            {
                // Search for WebGet or WebInvoke attributes
                var webGetAttr = method.GetCustomAttribute<WebGetAttribute>();
                var webInvokeAttr = method.GetCustomAttribute<WebInvokeAttribute>();

                if (webGetAttr != null)
                {
                    RegisterRoute("GET", webGetAttr.UriTemplate, method);
                }
                else if (webInvokeAttr != null)
                {
                    string httpMethod = webInvokeAttr.Method ?? "POST";
                    RegisterRoute(httpMethod, webInvokeAttr.UriTemplate, method);
                }
            }
        }

        private void RegisterRoute(string httpMethod, string uriTemplate, MethodInfo method)
        {
            if (string.IsNullOrEmpty(uriTemplate))
                return;

            var route = new WebServiceRoute
            {
                HttpMethod = httpMethod,
                UriTemplate = uriTemplate,
                Method = method,
                OperationName = method.Name
            };

            // Parse UriTemplate and create regex
            // e.g. "Login/{userName}" -> "^Login/(?<userName>[^/]+)$"
            string pattern = "^" + uriTemplate;
            var paramMatches = Regex.Matches(uriTemplate, @"\{([^}]+)\}");
            
            foreach (Match match in paramMatches)
            {
                string paramName = match.Groups[1].Value;
                route.ParameterNames.Add(paramName);
                pattern = pattern.Replace("{" + paramName + "}", "([^/]+)");
            }
            pattern += "$";

            route.UriRegex = new Regex(pattern, RegexOptions.IgnoreCase);
            _routes.Add(route);
        }

        /// <summary>
        /// Finds a matching route for HTTP method and path
        /// </summary>
        public WebServiceRoute FindRoute(string httpMethod, string path, out Dictionary<string, string> parameters)
        {
            parameters = null;

            foreach (var route in _routes)
            {
                if (route.TryMatch(httpMethod, path, out parameters))
                    return route;
            }

            return null;
        }
    }

    //[ACClassInfo(Const.PackName_VarioSystem, "en{'Json Host Listener'}de{'Json Host Listener'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
    //public class PAJsonServiceHostListener : PAWebServiceBase
    public partial class PAJsonServiceHost : PAWebServiceBase
    {
        private HttpListener _listener;
        private CancellationTokenSource _cancellationToken;
        protected JsonSerializerSettings _jsonSettings;
        protected WebServiceRouteRegistry _routeRegistry;

        public void PAJsonServiceHostListener(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
        }

        private void InitPAJsonServiceHostListener()
        { 
            _jsonSettings = CreateDefaultJsonSettings();
            _routeRegistry = new WebServiceRouteRegistry();
        }

        /// <summary>
        /// Creates the default JSON serialization settings.
        /// Override this method to configure custom settings.
        /// </summary>
        protected virtual JsonSerializerSettings CreateDefaultJsonSettings()
        {
            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
                ContractResolver = new DefaultContractResolver()
            };
        }

        /// <summary>
        /// Hook method for configuring serialization for specific endpoints.
        /// Equivalent to OnAddKnownTypesToOperationContract in WCF version.
        /// </summary>
        /// <param name="operationName">Name of the invoked operation</param>
        /// <param name="settings">JSON serializer settings that can be customized</param>
        protected virtual void OnConfigureJsonSerialization(string operationName, JsonSerializerSettings settings)
        {
            // Default implementation - override in derived classes
        }

        public ServiceHost CreateHttpListenerService()
        {
            if (_routeRegistry == null || _jsonSettings == null)
                InitPAJsonServiceHostListener();

            int servicePort = ServicePort;
            if (servicePort <= 0)
            {
                servicePort = 8730;
                ServicePort = servicePort;
            }

            _listener = new HttpListener();
            _cancellationToken = new CancellationTokenSource();

            // Register all routes from the service interface
            _routeRegistry.RegisterRoutes(ServiceInterfaceType);

            // Try different prefixes for Wine compatibility
            string[] prefixes = new[]
            {
                $"http://+:{servicePort}/",
                $"http://*:{servicePort}/",
                $"http://localhost:{servicePort}/"
            };

            bool started = false;
            foreach (var prefix in prefixes)
            {
                try
                {
                    _listener.Prefixes.Clear();
                    _listener.Prefixes.Add(prefix);
                    _listener.Start();
                    Messages.LogInfo(this.GetACUrl(), "CreateService()", $"HttpListener gestartet auf {prefix}");
                    started = true;
                    break;
                }
                catch (Exception ex)
                {
                    Messages.LogDebug(this.GetACUrl(), "CreateService()", $"Prefix {prefix} fehlgeschlagen: {ex.Message}");
                }
            }

            if (!started)
                throw new Exception("Konnte HttpListener auf keinem Prefix starten");

            // Start listener loop
            Task.Run(() => ListenAsync(_cancellationToken.Token));

            return null; // No WCF ServiceHost
        }

        private async Task ListenAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context), token);
                }
                catch (HttpListenerException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Messages.LogException(this.GetACUrl(), "ListenAsync()", ex);
                }
            }
        }

        protected virtual async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                // Set context for CoreWebService (replaces WebOperationContext)
                HttpListenerWebContext.CurrentSessionID = GetSessionIDFromCookie(context);
                HttpListenerWebContext.CurrentServicePort = GetServicePort(context);

                // CORS Headers
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                // Check authentication
                if (!CheckAuthorization(context))
                {
                    response.StatusCode = 401;
                    response.AddHeader("WWW-Authenticate", "Basic realm=\"PAJsonServiceHostListener\"");
                    response.Close();
                    return;
                }

                // Route request based on UriTemplate matching with reflection
                string path = request.Url.AbsolutePath.TrimStart('/');
                object result = null;
                string operationName = string.Empty;

                // Test endpoint
                if (path.Equals("test", StringComparison.OrdinalIgnoreCase))
                {
                    result = new { status = "running", timestamp = DateTime.Now, service = ServiceType.Name };
                }
                else
                {
                    // Find matching route
                    Dictionary<string, string> urlParameters;
                    var route = _routeRegistry.FindRoute(request.HttpMethod, path, out urlParameters);

                    if (route != null)
                    {
                        operationName = route.OperationName;
                        result = await InvokeServiceMethod(route, urlParameters, request);
                        
                        // Special handling for Login
                        if (operationName == "Login")
                        {
                            var loginResponse = result as WSResponse<VBUserRights>;
                            if (loginResponse != null && loginResponse.Data != null && loginResponse.Data.SessionID.HasValue)
                            {
                                AddSession(loginResponse.Data);
                                SetSessionCookie(context, loginResponse.Data.SessionID.Value);
                            }
                        }
                        // Special handling for Logout
                        else if (operationName == "Logout" && urlParameters.ContainsKey("sessionID"))
                        {
                            if (Guid.TryParse(urlParameters["sessionID"], out Guid sessionGuid))
                            {
                                RemoveSession(sessionGuid);
                            }
                        }
                    }
                    else
                    {
                        response.StatusCode = 404;
                        result = new { error = "Endpoint not found", path = path, method = request.HttpMethod };
                    }
                }

                // Configure JSON settings for this operation
                var operationSettings = CloneJsonSettings(_jsonSettings);
                OnConfigureJsonSerialization(operationName, operationSettings);

                // JSON Response senden
                string json = JsonConvert.SerializeObject(result, operationSettings);
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                response.ContentType = "application/json; charset=utf-8";
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 200;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "HandleRequestAsync()", ex);
                try
                {
                    context.Response.StatusCode = 500;
                    var errorResult = new { error = ex.Message, stackTrace = ex.StackTrace };
                    string errorJson = JsonConvert.SerializeObject(errorResult);
                    byte[] errorBuffer = Encoding.UTF8.GetBytes(errorJson);
                    context.Response.ContentType = "application/json; charset=utf-8";
                    context.Response.ContentLength64 = errorBuffer.Length;
                    await context.Response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                    context.Response.Close();
                }
                catch { }
            }
            finally
            {
                // Clean up context
                HttpListenerWebContext.Clear();
            }
        }

        /// <summary>
        /// Invokes a service method via reflection
        /// </summary>
        protected virtual async Task<object> InvokeServiceMethod(WebServiceRoute route, Dictionary<string, string> urlParameters, HttpListenerRequest request)
        {
            var methodParams = route.Method.GetParameters();
            object[] args = new object[methodParams.Length];

            // Read body for POST/PUT
            string requestBody = null;
            if (request.HttpMethod == "POST" || request.HttpMethod == "PUT")
            {
                requestBody = ReadRequestBody(request);
            }

            // Prepare parameters
            for (int i = 0; i < methodParams.Length; i++)
            {
                var param = methodParams[i];
                
                // Try to get parameter from URL
                if (urlParameters.ContainsKey(param.Name))
                {
                    args[i] = ConvertParameter(urlParameters[param.Name], param.ParameterType);
                }
                // Try to deserialize parameter from request body
                else if (!string.IsNullOrEmpty(requestBody))
                {
                    // Configure JSON settings for this operation
                    var deserializeSettings = CloneJsonSettings(_jsonSettings);
                    OnConfigureJsonSerialization(route.OperationName, deserializeSettings);

                    // For a single parameter: deserialize directly
                    if (methodParams.Length == 1 || i == 0)
                    {
                        try
                        {
                            args[i] = JsonConvert.DeserializeObject(requestBody, param.ParameterType, deserializeSettings);
                        }
                        catch
                        {
                            // Fallback: string parameter
                            if (param.ParameterType == typeof(string))
                                args[i] = requestBody;
                            else
                                args[i] = ConvertParameter(requestBody, param.ParameterType);
                        }
                    }
                }
                // Default value
                else
                {
                    args[i] = param.ParameterType.IsValueType ? Activator.CreateInstance(param.ParameterType) : null;
                }
            }

            // Get service instance
            object serviceInstance = GetWebServiceInstance();

            // Invoke method
            object result = route.Method.Invoke(serviceInstance, args);

            // If method is async (Task<T>), await
            if (result is Task)
            {
                await (Task)result;
                var resultProperty = result.GetType().GetProperty("Result");
                if (resultProperty != null)
                    result = resultProperty.GetValue(result);
            }

            return result;
        }

        /// <summary>
        /// Converts a string parameter to the target type
        /// </summary>
        protected object ConvertParameter(string value, Type targetType)
        {
            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(Guid))
                return Guid.Parse(value);

            if (targetType == typeof(int))
                return int.Parse(value);

            if (targetType == typeof(DateTime))
                return DateTime.Parse(value);

            if (targetType == typeof(bool))
                return bool.Parse(value);

            if (targetType == typeof(short))
                return short.Parse(value);

            if (targetType == typeof(double))
                return double.Parse(value);

            // Nullable types
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrEmpty(value) || value == "*" || value == "null")
                    return null;

                var underlyingType = Nullable.GetUnderlyingType(targetType);
                return ConvertParameter(value, underlyingType);
            }

            // Fallback: TypeConverter
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
            if (converter.CanConvertFrom(typeof(string)))
                return converter.ConvertFromString(value);

            return value;
        }

        protected bool CheckAuthorization(HttpListenerContext context)
        {
            // Check session cookie first
            Guid? sessionId = GetSessionIDFromCookie(context);
            if (sessionId.HasValue)
            {
                var userRights = GetRightsForSession(sessionId.Value);
                if (userRights != null)
                    return true; // Valid session exists
            }

            // No valid session cookie -> check Authorization header
            string authHeader = context.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader))
                return false;

            try
            {
                // Basic Auth: "Basic base64(username:password)"
                if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    return false;

                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Substring(6))).Split(':');
                if (credentials.Length != 2)
                    return false;

                string userName = credentials[0];
                string password = credentials[1];

                ACStartUpRoot startUpRoot = new ACStartUpRoot();
                string errorMessage = "";
                bool loggedIn = startUpRoot.CheckLogin(userName, password, ref errorMessage) != null;

                if (loggedIn && !sessionId.HasValue)
                {
                    // Create new session and set cookie
                    Guid newSessionId = Guid.NewGuid();
                    SetSessionCookie(context, newSessionId);
                }

                return loggedIn;
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "CheckAuthorization()", ex);
                return false;
            }
        }

        /// <summary>
        /// Extracts the session ID from the cookie header
        /// </summary>
        protected Guid? GetSessionIDFromCookie(HttpListenerContext context)
        {
            string cookieHeader = context.Request.Headers["Cookie"];
            if (string.IsNullOrEmpty(cookieHeader))
                return null;

            return CoreWebServiceConst.DecodeSessionIdFromCookieHeader(cookieHeader);
        }

        /// <summary>
        /// Sets a session cookie in the response
        /// </summary>
        protected void SetSessionCookie(HttpListenerContext context, Guid sessionId)
        {
            string cookieValue = CoreWebServiceConst.BuildSessionIdForCookieHeader(sessionId);
            context.Response.Headers.Add("Set-Cookie", cookieValue);
        }

        /// <summary>
        /// Extracts the current service port from the HttpListenerContext
        /// </summary>
        protected int? GetServicePort(HttpListenerContext context)
        {
            int? servicePort = context.Request.Url?.Port;
            // Port forwarding: If port > 10000, then -10000
            if (servicePort.HasValue && servicePort >= 10000)
                servicePort -= 10000;
            return servicePort;
        }

        protected string ReadRequestBody(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        protected string ExtractOperationName(string path)
        {
            // Extracts the last part of the path as operation name
            var parts = path.TrimEnd('/').Split('/');
            return parts.Length > 0 ? parts[parts.Length - 1] : string.Empty;
        }

        protected JsonSerializerSettings CloneJsonSettings(JsonSerializerSettings source)
        {
            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = source.ReferenceLoopHandling,
                NullValueHandling = source.NullValueHandling,
                TypeNameHandling = source.TypeNameHandling,
                ContractResolver = source.ContractResolver,
                SerializationBinder = source.SerializationBinder,
                Converters = new List<JsonConverter>(source.Converters)
            };
        }

        protected override void OnServiceHostClosed()
        {
            _cancellationToken?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            base.OnServiceHostClosed();
        }
    }
}
