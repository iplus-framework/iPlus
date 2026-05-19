// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Styling;
using AvaloniaEdit.Highlighting;
using gip.core.datamodel;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace gip.core.reporthandler.avui
{
    public enum HtmlEditorThemeMode : short
    {
        Auto,
        Dark,
        Light
    }

    /// <summary>
    /// Simple HTML template editor used by the report editor when a Scryber template is loaded.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBHtmlEditor'}de{'VBHtmlEditor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBHtmlEditor : gip.core.layoutengine.avui.VBTextEditor
    {
        private object _textMateInstallationInstance;
        private IDisposable _textMateInstallation;
        private object _textMateRegistryOptions;
        private Type _textMateThemeNameType;
        private MethodInfo _setThemeMethod;
        private MethodInfo _loadThemeMethod;
        private string _appliedTextMateThemeName;

        private HtmlEditorThemeMode _ThemeMode = HtmlEditorThemeMode.Auto;
        [Category("VBControl")]
        public HtmlEditorThemeMode ThemeMode
        {
            get
            {
                return _ThemeMode;
            }
            set
            {
                _ThemeMode = value;
                ApplyTextMateTheme();
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            EnsureHtmlHighlighting();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            // ActualThemeVariant can change before OnInitialized() during logical tree attach.
            // Only re-apply TextMate theme after TextMate was successfully installed.
            if (ThemeMode == HtmlEditorThemeMode.Auto
                && _textMateInstallationInstance != null
                && change.Property != null
                && change.Property.Name == "ActualThemeVariant")
                ApplyTextMateTheme();
        }

        public override void DeInitVBControl(IACComponent bso)
        {
            DisposeTextMate();
            base.DeInitVBControl(bso);
        }

        private void EnsureHtmlHighlighting()
        {
            if (TryInstallTextMate())
                return;

            if (SyntaxHighlighting != null)
                return;

            SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("HTML")
                ?? HighlightingManager.Instance.GetDefinition("XML");
        }

        private bool TryInstallTextMate()
        {
            if (_textMateInstallation != null)
                return true;

            try
            {
                Assembly textMateAssembly = TryLoadAssembly("AvaloniaEdit.TextMate");
                Assembly grammarsAssembly = TryLoadAssembly("TextMateSharp.Grammars");

                if (textMateAssembly == null || grammarsAssembly == null)
                    return false;

                Type textMateType = textMateAssembly.GetType("AvaloniaEdit.TextMate.TextMate");
                Type registryOptionsType = grammarsAssembly.GetType("TextMateSharp.Grammars.RegistryOptions");
                Type themeNameType = grammarsAssembly.GetType("TextMateSharp.Grammars.ThemeName");

                if (textMateType == null || registryOptionsType == null || themeNameType == null)
                    return false;

                object theme = ResolveTheme(themeNameType);
                object registryOptions = Activator.CreateInstance(registryOptionsType, theme);
                if (registryOptions == null)
                    return false;

                MethodInfo installMethod = textMateType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(c => c.Name == "InstallTextMate" && c.GetParameters().Length == 4);

                if (installMethod == null)
                    return false;

                object installation = installMethod.Invoke(null, new object[] { this, registryOptions, true, null });
                if (installation == null)
                    return false;

                string scopeName = ResolveHtmlScopeName(registryOptions);
                MethodInfo setGrammarMethod = installation.GetType().GetMethod("SetGrammar", new[] { typeof(string) });
                setGrammarMethod?.Invoke(installation, new object[] { scopeName });

                _textMateInstallationInstance = installation;
                _textMateInstallation = installation as IDisposable;
                _textMateRegistryOptions = registryOptions;
                _textMateThemeNameType = themeNameType;
                _setThemeMethod = installation.GetType().GetMethod("SetTheme");
                _loadThemeMethod = registryOptionsType.GetMethod("LoadTheme", new[] { themeNameType });
                ApplyTextMateTheme();
                return _textMateInstallationInstance != null;
            }
            catch
            {
                DisposeTextMate();
                return false;
            }
        }

        private static string ResolveHtmlScopeName(object registryOptions)
        {
            if (registryOptions == null)
                return "text.html.basic";

            Type optionsType = registryOptions.GetType();
            MethodInfo getLanguageByExtension = optionsType.GetMethod("GetLanguageByExtension", new[] { typeof(string) });
            MethodInfo getScopeByLanguageId = optionsType.GetMethod("GetScopeByLanguageId", new[] { typeof(string) });

            if (getLanguageByExtension == null || getScopeByLanguageId == null)
                return "text.html.basic";

            object language = getLanguageByExtension.Invoke(registryOptions, new object[] { ".html" })
                ?? getLanguageByExtension.Invoke(registryOptions, new object[] { ".htm" });

            if (language == null)
                return "text.html.basic";

            PropertyInfo idProperty = language.GetType().GetProperty("Id");
            string languageId = idProperty?.GetValue(language)?.ToString();

            if (string.IsNullOrWhiteSpace(languageId))
                return "text.html.basic";

            string scopeName = getScopeByLanguageId.Invoke(registryOptions, new object[] { languageId }) as string;
            return string.IsNullOrWhiteSpace(scopeName) ? "text.html.basic" : scopeName;
        }

        private object ResolveTheme(Type themeNameType)
        {
            string preferredTheme = ResolveThemeName();

            if (Enum.GetNames(themeNameType).Any(c => c == preferredTheme))
                return Enum.Parse(themeNameType, preferredTheme);

            Array values = Enum.GetValues(themeNameType);
            return values.Length > 0 ? values.GetValue(0) : Activator.CreateInstance(themeNameType);
        }

        private string ResolveThemeName()
        {
            bool useLightTheme;
            if (ThemeMode == HtmlEditorThemeMode.Light)
            {
                useLightTheme = true;
            }
            else if (ThemeMode == HtmlEditorThemeMode.Dark)
            {
                useLightTheme = false;
            }
            else if (ActualThemeVariant == ThemeVariant.Light)
            {
                useLightTheme = true;
            }
            else if (ActualThemeVariant == ThemeVariant.Dark)
            {
                useLightTheme = false;
            }
            else
            {
                useLightTheme = gip.core.layoutengine.avui.ControlManager.WpfTheme == gip.core.layoutengine.avui.eWpfTheme.Light;
            }

            return useLightTheme
                ? "LightPlus"
                : "DarkPlus";
        }

        private void ApplyTextMateTheme()
        {
            if (_textMateInstallationInstance == null || _textMateRegistryOptions == null || _textMateThemeNameType == null || _setThemeMethod == null || _loadThemeMethod == null)
                return;

            try
            {
                string themeName = ResolveThemeName();
                if (themeName == _appliedTextMateThemeName)
                    return;

                if (!Enum.GetNames(_textMateThemeNameType).Any(c => c == themeName))
                    return;

                object themeEnum = Enum.Parse(_textMateThemeNameType, themeName);
                object rawTheme = _loadThemeMethod.Invoke(_textMateRegistryOptions, new object[] { themeEnum });
                if (rawTheme == null)
                    return;

                _setThemeMethod.Invoke(_textMateInstallationInstance, new[] { rawTheme });
                _appliedTextMateThemeName = themeName;
            }
            catch
            {
            }
        }

        private static Assembly TryLoadAssembly(string assemblyName)
        {
            Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(c => string.Equals(c.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));

            if (loadedAssembly != null)
                return loadedAssembly;

            try
            {
                return Assembly.Load(assemblyName);
            }
            catch
            {
                foreach (string assemblyFile in GetAssemblyCandidatePaths(assemblyName))
                {
                    try
                    {
                        if (File.Exists(assemblyFile))
                            return Assembly.LoadFrom(assemblyFile);
                    }
                    catch
                    {
                    }
                }

                return null;
            }
        }

        private static string[] GetAssemblyCandidatePaths(string assemblyName)
        {
            string fileName = assemblyName + ".dll";

            string appBase = AppContext.BaseDirectory;
            string assemblyDir = Path.GetDirectoryName(typeof(VBHtmlEditor).Assembly.Location);

            string config = null;
            string tfm = null;
            DirectoryInfo appBaseInfo = null;

            try
            {
                appBaseInfo = new DirectoryInfo(appBase.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                tfm = appBaseInfo.Name;
                config = appBaseInfo.Parent?.Name;
            }
            catch
            {
            }

            string sharedBinPath = null;
            try
            {
                DirectoryInfo projectDir = appBaseInfo?.Parent?.Parent?.Parent;
                DirectoryInfo workspaceDir = projectDir?.Parent;
                if (workspaceDir != null && !string.IsNullOrWhiteSpace(config) && !string.IsNullOrWhiteSpace(tfm))
                    sharedBinPath = Path.Combine(workspaceDir.FullName, "bin", config, tfm, fileName);
            }
            catch
            {
            }

            return new[]
            {
                Path.Combine(appBase, fileName),
                string.IsNullOrWhiteSpace(assemblyDir) ? null : Path.Combine(assemblyDir, fileName),
                sharedBinPath
            };
        }

        private void DisposeTextMate()
        {
            try
            {
                _textMateInstallation?.Dispose();
            }
            finally
            {
                _textMateInstallationInstance = null;
                _textMateInstallation = null;
                _textMateRegistryOptions = null;
                _textMateThemeNameType = null;
                _setThemeMethod = null;
                _loadThemeMethod = null;
                _appliedTextMateThemeName = null;
            }
        }
    }
}
