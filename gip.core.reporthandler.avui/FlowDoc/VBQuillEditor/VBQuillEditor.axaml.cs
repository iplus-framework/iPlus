// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.reporthandler.avui
{
    /// <summary>
    /// WYSIWYG HTML editor powered by Quill.js rendered inside a WebView.
    /// Embeds Quill + quill-table-better as local assets for offline use.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBQuillEditor'}de{'VBQuillEditor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public partial class VBQuillEditor : UserControl, IACObject
    {
        private static string _assetsDirectory = null;
        private NativeWebView _webView;
        private bool _isReady;
        private bool _pageLoaded;
        private bool _navigationStarted;
        private bool _suppressNextChange;
        private int _activateCounter;
        private int _navigationCounter;
        private int _reloadCounter;
        private string _lastMessageType;
        private DateTime _lastMessageUtc = DateTime.MinValue;
        private readonly SemaphoreSlim _activateGate = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Stores the original full Scryber HTML template (with DOCTYPE, head, etc.)
        /// so that body-only HTML from Quill can be re-wrapped into a valid document.
        /// </summary>
        private string _originalTemplate;

        /// <summary>
        /// Resolved path to the bundled Quill assets (extracted once on first use).
        /// </summary>
        private static string AssetsDirectory
        {
            get
            {
                if (_assetsDirectory == null)
                {
                    _assetsDirectory = ExtractAssets();
                }
                return _assetsDirectory;
            }
        }

        public VBQuillEditor()
        {
            InitializeComponent();
            VBTextProperty.Changed.Subscribe(args =>
            {
                string newValue = args.NewValue.Value;
                // When the initial full template arrives (from the binding), remember it.
                if (IsFullHtmlDocument(newValue))
                {
                    _originalTemplate = newValue;
                    Trace("Trace00001", $"Stored full template from binding (len={newValue?.Length ?? 0}).");
                }
            });

            Trace("Trace00002", "Constructor initialized.");
        }

        private void EnsureWebViewCreated()
        {
            if (_webView != null)
                return;

            _webView = new NativeWebView();
            _webView.WebMessageReceived += WebView_WebMessageReceived;
            _webView.NavigationCompleted += WebView_NavigationCompleted;
            _webView.EnvironmentRequested += WebView_EnvironmentRequested;

            WebViewHost.Children.Clear();
            WebViewHost.Children.Add(_webView);

            Trace("Trace00045", "NativeWebView created and handlers attached.");
        }

        private void DestroyWebView()
        {
            if (_webView == null)
                return;

            try
            {
                _webView.WebMessageReceived -= WebView_WebMessageReceived;
                _webView.NavigationCompleted -= WebView_NavigationCompleted;
                _webView.EnvironmentRequested -= WebView_EnvironmentRequested;
                _webView.Source = null;
            }
            catch
            {
            }

            WebViewHost.Children.Remove(_webView);
            _webView = null;
            Trace("Trace00046", "NativeWebView destroyed.");
        }

        private void Trace(string code, string message)
        {
            this.Root()?.Messages.LogDebug("VBQuillEditor", code, message);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsVisibleProperty && !change.GetNewValue<bool>())
            {
                // When the hosting tab hides this control, the native page may be torn down.
                // Reset flags so ActivateAsync forces a fresh navigation next time.
                Trace("Trace00003", $"IsVisible=false. Reset state (pageLoaded={_pageLoaded}, navStarted={_navigationStarted}).");
                _pageLoaded = false;
                _navigationStarted = false;
            }
            else if (change.Property == IsVisibleProperty && change.GetNewValue<bool>())
            {
                Trace("Trace00004", $"IsVisible=true (pageLoaded={_pageLoaded}, navStarted={_navigationStarted}).");
            }
        }

        #region Properties

        public static readonly StyledProperty<string> VBTextProperty =
            AvaloniaProperty.Register<VBQuillEditor, string>(nameof(VBText));

        /// <summary>
        /// Gets or sets the HTML content of the editor.
        /// Two-way bound to the underlying Quill document.
        /// </summary>
        [Category("VBControl")]
        public string VBText
        {
            get => GetValue(VBTextProperty);
            set => SetValue(VBTextProperty, value);
        }

        #endregion

        #region Lifecycle

        protected override async void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            Trace("Trace00005", $"OnLoaded start (pageLoaded={_pageLoaded}, navStarted={_navigationStarted}).");

            EnsureWebViewCreated();

            // Do not call ActivateAsync() from here: VBReportEditor already triggers it
            // when tab activation changes. Calling it in both places creates overlapping
            // activation flows and race conditions.
            Trace("Trace00006", $"OnLoaded end (pageLoaded={_pageLoaded}, navStarted={_navigationStarted}, isReady={_isReady}).");
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            Trace("Trace00007", $"OnUnloaded (pageLoaded={_pageLoaded}, navStarted={_navigationStarted}, isReady={_isReady}, lastMsg={_lastMessageType ?? "<none>"}).");
            // Intentionally keep EnvironmentRequested subscribed for the control lifetime.
            // This control is unloaded/reloaded on tab switches; detaching here would leave
            // subsequent navigations without the Linux host/environment preference hook.
            // Do not hard-cleanup here: tab switches trigger unload/load frequently.
            // A full reset here (especially Source = null) can destabilize the WebView bridge
            // after repeated cycles. We validate bridge health in ActivateAsync instead.
            _isReady = false;
            // Force a fresh navigate/handshake on next activation.
            _pageLoaded = false;
            _navigationStarted = false;

            DestroyWebView();
        }

        #endregion

        private void WebView_EnvironmentRequested(object sender, WebViewEnvironmentRequestedEventArgs e)
        {
            Trace("Trace00008", $"EnvironmentRequested ({e?.GetType().Name ?? "null"}).");

            if (e is LinuxWpeWebViewEnvironmentRequestedEventArgs wpeArgs)
            {
                // Prefer WPE when available (matches Avalonia docs for NativeWebView on Linux).
                wpeArgs.PreferWebKitGtkInstead = false;
            }
            else if (e is GtkWebViewEnvironmentRequestedEventArgs gtkArgs)
            {
                // If WPE isn't available, fallback to GTK native host instead of experimental offscreen path.
                gtkArgs.ExperimentalOffscreen = false;
            }
        }

        #region Navigation

        private async Task NavigateToEditor()
        {
            if (_navigationStarted && _webView != null && _webView.Source != null)
            {
                Trace("Trace00009", "NavigateToEditor skipped (already started and source set).");
                return;
            }

            EnsureWebViewCreated();

            if (_webView == null)
            {
                Trace("Trace00047", "NavigateToEditor aborted: webView was not created.");
                return;
            }

            string htmlPath = Path.Combine(AssetsDirectory, "quill-editor.html");
            if (!File.Exists(htmlPath))
            {
                this.Root()?.Messages.LogError("VBQuillEditor", "Error00001", $"Editor HTML not found: {htmlPath}");
                return;
            }

            // Force a genuine navigation on repeated tab switches:
            // 1) clear previous source to avoid "same value" short-circuit
            // 2) append cache-busting query so URI is always unique
            if (_webView.Source != null)
            {
                Trace("Trace00042", "NavigateToEditor clearing existing WebView.Source before re-navigation.");
                _webView.Source = null;
                await Task.Delay(25);
            }

            _navigationStarted = true;
            _navigationCounter++;
            string navUrl = $"file://{htmlPath.Replace('\\', '/')}?nav={_navigationCounter}&ts={DateTime.UtcNow.Ticks}";
            var uri = new Uri(navUrl);
            Trace("Trace00010", $"NavigateToEditor start #{_navigationCounter}: {uri}");
            _webView.Source = uri;
        }

        private async void WebView_NavigationCompleted(object sender, WebViewNavigationCompletedEventArgs e)
        {
            _pageLoaded = true;
            Trace("Trace00011", $"NavigationCompleted (isSuccess={e.IsSuccess}, pageLoaded={_pageLoaded}, vbTextLen={VBText?.Length ?? 0}).");

            // Small delay to let Quill initialize
            await Task.Delay(500);

            // If we already had content, push it to the editor
            if (!string.IsNullOrEmpty(VBText))
            {
                Trace("Trace00012", $"NavigationCompleted pushing initial content (len={VBText.Length}).");
                await SetContentToEditor(VBText);
            }
        }

        #endregion

        #region Message Bridge

        private void WebView_WebMessageReceived(object sender, WebMessageReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Body))
            {
                Trace("Trace00013", "WebMessageReceived empty body.");
                return;
            }

            try
            {
                using var doc = JsonDocument.Parse(e.Body);
                string messageType = doc.RootElement.GetProperty("type").GetString();
                _lastMessageType = messageType;
                _lastMessageUtc = DateTime.UtcNow;
                Trace("Trace00014", $"WebMessageReceived type={messageType}, bodyLen={e.Body.Length}");

                switch (messageType)
                {
                    case "initialized":
                        this.Root()?.Messages.LogDebug("VBQuillEditor", "Info00001", $"Quill initialized v{doc.RootElement.GetProperty("version").GetString()}");
                        break;

                    case "ready":
                        _isReady = true;
                        Trace("Trace00015", "Ready ack received from JS.");
                        break;

                    case "content-changed":
                        _suppressNextChange = true;
                        string html = doc.RootElement.GetProperty("html").GetString();
                        Trace("Trace00016", $"Content-changed received (htmlLen={html?.Length ?? 0}).");
                        string effectiveHtml = ReconstructFullDocument(html);
                        if (effectiveHtml != VBText)
                        {
                            VBText = effectiveHtml;
                            Trace("Trace00017", $"VBText updated from content-changed (effectiveLen={effectiveHtml?.Length ?? 0}).");
                        }
                        _suppressNextChange = false;
                        break;

                    case "trace":
                        Trace("Trace00018", $"JS trace: {doc.RootElement.GetProperty("message").GetString()}");
                        break;

                    default:
                        Trace("Trace00019", $"Unhandled message type={messageType}");
                        break;
                }
            }
            catch (JsonException)
            {
                Trace("Trace00020", $"WebMessageReceived JSON parse failed. RawLen={e.Body?.Length ?? 0}");
            }
            catch (Exception ex)
            {
                this.Root()?.Messages.LogException("VBQuillEditor", nameof(WebView_WebMessageReceived), ex);
            }
        }

        #endregion

        #region Template Reconstruction

        /// <summary>
        /// Returns true if the HTML looks like a full document (starts with DOCTYPE or &lt;html&gt;).
        /// </summary>
        private static bool IsFullHtmlDocument(string html)
        {
            if (string.IsNullOrEmpty(html))
                return false;

            string trimmed = html.TrimStart();
            string lower = trimmed.ToLowerInvariant();
            return lower.StartsWith("<!doctype") || lower.StartsWith("<html");
        }

        /// <summary>
        /// If <paramref name="bodyHtml"/> is body-only content (as Quill emits),
        /// re-wraps it using the originally-stored full template.
        /// If it's already a full document, returns it unchanged.
        /// </summary>
        private string ReconstructFullDocument(string bodyHtml)
        {
            // Already a full document — nothing to do
            if (IsFullHtmlDocument(bodyHtml))
                return bodyHtml;

            // No original template stored — return as-is
            if (string.IsNullOrEmpty(_originalTemplate))
                return bodyHtml;

            // Extract the wrapper (everything up to and including </head>)
            // and the body content from the original template,
            // then splice in the new body content.
            string wrapperOpen = ExtractWrapperOpen(_originalTemplate);
            return wrapperOpen + "\n<body>\n" + bodyHtml + "\n</body>\n</html>";
        }

        /// <summary>
        /// Extracts the document prologue from a full HTML template:
        /// everything from the start through the closing &lt;/head&gt; tag.
        /// Falls back to everything before &lt;body&gt; if &lt;/head&gt; is absent.
        /// </summary>
        private static string ExtractWrapperOpen(string fullHtml)
        {
            int headEndIdx = fullHtml.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
            if (headEndIdx > -1)
            {
                return fullHtml.Substring(0, headEndIdx + 7); // include </head>
            }

            // Fallback: find <body
            int bodyStartIdx = fullHtml.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
            if (bodyStartIdx > -1)
            {
                return fullHtml.Substring(0, bodyStartIdx);
            }

            // No recognizable boundary — return the whole thing
            return fullHtml;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Sets the editor content programmatically (called from C#).
        /// Does nothing if the page hasn't loaded yet.
        /// </summary>
        private async Task SetContentToEditor(string html)
        {
            if (!_pageLoaded)
            {
                Trace("Trace00021", "SetContentToEditor skipped because page not loaded.");
                return;
            }

            if (_webView == null)
            {
                Trace("Trace00048", "SetContentToEditor skipped because webView is null.");
                return;
            }

            try
            {
                Trace("Trace00022", $"SetContentToEditor invoke (len={html?.Length ?? 0}).");
                // Escape the HTML for safe embedding in a JSON string
                string escapedHtml = JsonSerializer.Serialize(html);
                string script = $@"
                    (() => {{
                        const msg = {{ type: 'setContent', html: {escapedHtml} }};
                        window.dispatchEvent(new MessageEvent('message', {{ data: msg }}));
                    }})();
                ";
                await InvokeScriptWithTimeoutAsync(script, 2000, "SetContentToEditor");
            }
            catch (Exception ex)
            {
                Trace("Trace00023", "SetContentToEditor invoke failed.");
                this.Root()?.Messages.LogException("VBQuillEditor", nameof(SetContentToEditor), ex);
            }
        }

        /// <summary>
        /// Requests focus on the editor.
        /// Does nothing if the page hasn't loaded yet.
        /// </summary>
        public async Task FocusEditor()
        {
            if (!_pageLoaded)
            {
                Trace("Trace00024", "FocusEditor skipped because page not loaded.");
                return;
            }

            if (_webView == null)
            {
                Trace("Trace00049", "FocusEditor skipped because webView is null.");
                return;
            }

            try
            {
                Trace("Trace00025", "FocusEditor invoke.");
                string script = @"
                    (() => {
                        const msg = { type: 'focus' };
                        window.dispatchEvent(new MessageEvent('message', { data: msg }));
                    })();
                ";
                await InvokeScriptWithTimeoutAsync(script, 2000, "FocusEditor");
            }
            catch (Exception ex)
            {
                Trace("Trace00026", "FocusEditor invoke failed.");
                this.Root()?.Messages.LogException("VBQuillEditor", nameof(FocusEditor), ex);
            }
        }

        /// <summary>
        /// Ensures the editor page is loaded and synchronized when the WYSIWYG tab is activated.
        /// This avoids first-activation races where WebView becomes visible before content initialization.
        /// </summary>
        public async Task ActivateAsync()
        {
            EnsureWebViewCreated();

            if (!await _activateGate.WaitAsync(0))
            {
                Trace("Trace00036", "ActivateAsync skipped because another activation is in progress.");
                return;
            }

            try
            {
            _activateCounter++;
            Trace("Trace00027", $"ActivateAsync start #{_activateCounter} (pageLoaded={_pageLoaded}, navStarted={_navigationStarted}, isReady={_isReady}).");

            if (!_pageLoaded)
            {
                await NavigateToEditor();
                for (var i = 0; i < 120 && !_pageLoaded; i++)
                {
                    await Task.Delay(50);
                }

                Trace("Trace00028", $"ActivateAsync post-wait (pageLoaded={_pageLoaded}, navStarted={_navigationStarted}).");

                if (!_pageLoaded)
                {
                    Trace("Trace00043", "ActivateAsync navigation wait timed out; forcing hard reload.");
                    await ForceReloadAsync();
                }
            }

            // Verify JS -> C# message bridge; if broken, force one hard reload.
            if (_pageLoaded)
            {
                bool bridgeReady = await EnsureBridgeReadyAsync();
                Trace("Trace00029", $"Bridge health result: {bridgeReady}");
                if (!bridgeReady)
                {
                    await ForceReloadAsync();
                }
            }

            if (_pageLoaded && !string.IsNullOrEmpty(VBText))
            {
                await SetContentToEditor(VBText);
            }

            if (_pageLoaded)
            {
                await FocusEditor();
            }

            if (_webView != null)
            {
                _webView.InvalidateMeasure();
                _webView.InvalidateArrange();
            }
            WebViewHost.InvalidateMeasure();
            WebViewHost.InvalidateArrange();
            InvalidateVisual();
            Trace("Trace00030", $"ActivateAsync end #{_activateCounter} (pageLoaded={_pageLoaded}, isReady={_isReady}, lastMsg={_lastMessageType ?? "<none>"}).");
            }
            finally
            {
                _activateGate.Release();
            }
        }

        /// <summary>
        /// Sends a ready ping through the in-page message bus and waits for the
        /// corresponding JS -> C# "ready" ack in WebView_WebMessageReceived.
        /// </summary>
        private async Task<bool> EnsureBridgeReadyAsync()
        {
            if (!_pageLoaded)
                return false;

            // If we just received any JS message, treat bridge as alive.
            if (_lastMessageUtc != DateTime.MinValue)
            {
                double recentMs = (DateTime.UtcNow - _lastMessageUtc).TotalMilliseconds;
                if (recentMs >= 0 && recentMs <= 2500)
                {
                    Trace("Trace00038", $"EnsureBridgeReadyAsync short-circuit via recent JS message ({_lastMessageType ?? "<none>"}, {recentMs:F0}ms ago).");
                    _isReady = true;
                    return true;
                }
            }

            _isReady = false;
            Trace("Trace00031", "EnsureBridgeReadyAsync ping start.");
            DateTime pingStartUtc = DateTime.UtcNow;

            try
            {
                string script = @"
                    (() => {
                        const msg = { type: 'ready' };
                        window.dispatchEvent(new MessageEvent('message', { data: msg }));
                    })();
                ";
                await InvokeScriptWithTimeoutAsync(script, 2000, "EnsureBridgeReadyAsync");
            }
            catch
            {
                Trace("Trace00032", "EnsureBridgeReadyAsync ping invoke failed.");
                return false;
            }

            for (var i = 0; i < 40; i++)
            {
                if (_isReady)
                {
                    Trace("Trace00039", $"EnsureBridgeReadyAsync ready=true after {i + 1} checks.");
                    return true;
                }

                // If any message arrived during the ping window, the bridge is alive.
                if (_lastMessageUtc >= pingStartUtc)
                {
                    Trace("Trace00040", $"EnsureBridgeReadyAsync observed JS traffic during ping ({_lastMessageType ?? "<none>"}).");
                    _isReady = true;
                    return true;
                }

                await Task.Delay(50);
            }

            Trace("Trace00033", "EnsureBridgeReadyAsync timed out waiting for ready ack.");
            return false;
        }

        private async Task InvokeScriptWithTimeoutAsync(string script, int timeoutMs, string caller)
        {
            if (_webView == null)
            {
                Trace("Trace00050", $"InvokeScript skipped in {caller} because webView is null.");
                throw new InvalidOperationException("WebView is null.");
            }

            Task invokeTask = _webView.InvokeScript(script);
            Task completed = await Task.WhenAny(invokeTask, Task.Delay(timeoutMs));
            if (completed != invokeTask)
            {
                Trace("Trace00037", $"InvokeScript timeout in {caller} after {timeoutMs}ms.");
                throw new TimeoutException($"InvokeScript timeout in {caller}.");
            }

            await invokeTask;
        }

        /// <summary>
        /// Performs a hard WebView reload and waits until the bridge responds again.
        /// </summary>
        private async Task ForceReloadAsync()
        {
            _reloadCounter++;
            Trace("Trace00034", $"ForceReloadAsync start #{_reloadCounter}.");

            _pageLoaded = false;
            _navigationStarted = false;
            _isReady = false;

            try
            {
                if (_webView != null)
                    _webView.Source = null;
            }
            catch
            {
                // ignore; we'll still try to navigate below
            }

            await Task.Delay(50);
            await NavigateToEditor();

            for (var i = 0; i < 120 && !_pageLoaded; i++)
            {
                await Task.Delay(50);
            }

            if (!_pageLoaded)
            {
                Trace("Trace00044", "ForceReloadAsync timed out waiting for NavigationCompleted.");
            }

            if (_pageLoaded)
            {
                bool readyAfterReload = await EnsureBridgeReadyAsync();
                Trace("Trace00041", $"ForceReloadAsync bridge result after reload: {readyAfterReload}");
            }

            Trace("Trace00035", $"ForceReloadAsync end #{_reloadCounter} (pageLoaded={_pageLoaded}, isReady={_isReady}).");
        }

        #endregion

        #region Asset Extraction

        /// <summary>
        /// Extracts embedded Quill assets to a persistent temp directory.
        /// Called once per application lifetime.
        /// </summary>
        private static string ExtractAssets()
        {
            string basePath = Path.Combine(
                Path.GetTempPath(),
                "iplus_quill_assets_" + Assembly.GetExecutingAssembly().GetName().Version?.ToString().Replace('.', '_'));

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            string[] assetNames = new[]
            {
                "quill.min.js",
                "quill.snow.css",
                "quill-table-better.js",
                "quill-table-better.css",
                "quill-editor.html"
            };

            Assembly assembly = Assembly.GetExecutingAssembly();
            string nsPrefix = $"{assembly.GetName().Name}.FlowDoc.VBQuillEditor.Assets.";

            foreach (string assetName in assetNames)
            {
                string resourceName = nsPrefix + assetName;
                string destPath = Path.Combine(basePath, assetName);

                Stream stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    // Try alternate prefix patterns
                    stream = assembly.GetManifestResourceStream("gip.core.reporthandler.avui.FlowDoc.VBQuillEditor.Assets." + assetName);
                }

                if (stream != null)
                {
                    using (stream)
                    using (var fs = new FileStream(destPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fs);
                    }
                }
                else
                {
                    throw new FileNotFoundException("Embedded resource '" + resourceName + "' not found.");
                }
            }

            return basePath;
        }

        #endregion

        #region Cleanup

        private void Cleanup()
        {
            _isReady = false;
            _pageLoaded = false;
            _navigationStarted = false;
            if (_webView != null)
                _webView.Source = null;
        }

        #endregion

        #region IVBContent / IACObject

        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        public string ACIdentifier
        {
            get => Name;
            set => Name = value;
        }

        public string ACCaption
        {
            get; set;
        }

        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }

        public System.Collections.Generic.IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
            }
        }

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return true;
        }

        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        public void DeInitVBControl(IACComponent bso)
        {
        }

        #endregion
    }
}
