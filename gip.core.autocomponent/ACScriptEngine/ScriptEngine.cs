// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using gip.core.datamodel;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.IO;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Text;
using System.Diagnostics;
using SkiaSharp;
using System.Runtime.Loader;
using System.Runtime.InteropServices;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Script Engine for exceuting extended static methods for ACComponents
    /// </summary>
    public class ScriptEngine
    {
        #region Private Members
        private static Dictionary<Guid, ScriptEngine> _ProxyEngines = new Dictionary<Guid, ScriptEngine>();
        private static readonly ACMonitorObject _20091_LockProxyEngines = new ACMonitorObject(20091);
        private static Dictionary<Guid, ScriptEngine> _RealEngines = new Dictionary<Guid, ScriptEngine>();
        private static readonly ACMonitorObject _20092_LockRealEngines = new ACMonitorObject(20092);
        private ScriptList _Scripts;
        IACType _ACType = null;
        #endregion

        #region Constants
        public const string C_ScriptNamespace = "Scripts";
        public const string C_ScriptStaticClassName = "ScriptMethods";
        public const string C_TypeNameScriptStaticClass = C_ScriptNamespace + "." + C_ScriptStaticClassName;
        #endregion

        /// <summary>
        /// Current UI mode of the application.
        /// </summary>
        public enum UiMode
        {
            Unknown,
            None,      // Headless / background service
            Wpf,
            Avalonia
        }

        #region Constructors

        /// <summary>
        /// Default constructor for the <see cref="ScriptEngine"/> class.
        /// </summary>
        public ScriptEngine(IACType acType)
        {
            _ACType = acType;
            _Scripts = new ScriptList(this);
        }

        public static ScriptEngine GetScriptEngine(IACType acType, bool isProxy)
        {
            if (acType == null)
                return null;
            ACClass acClass = acType as ACClass;
            if (acClass == null)
                return null;

            ScriptEngine engine = null;
            if (isProxy)
            {
                bool engineFound = false;

                using (ACMonitor.Lock(_20091_LockProxyEngines))
                {
                    engineFound = _ProxyEngines.TryGetValue(acClass.ACTypeID, out engine);
                }
                if (!engineFound)
                {
                    IEnumerable<ACClassMethod> query = acClass.MethodsCached.Where(c => c.ACKind == Global.ACKinds.MSMethodExtClient);
                    if (!query.Any())
                        return null;
                    ScriptEngine engine2 = new ScriptEngine(acClass);
                    foreach (ACClassMethod acClassMethod in query)
                    {
                        engine2.RegisterScript(acClassMethod.ACIdentifier, acClassMethod.Sourcecode, acClassMethod.ContinueByError, acClassMethod);
                    }

                    using (ACMonitor.Lock(_20091_LockProxyEngines))
                    {
                        if (!_ProxyEngines.TryGetValue(acClass.ACTypeID, out engine))
                        {
                            _ProxyEngines.Add(acClass.ACTypeID, engine2);
                            engine = engine2;
                        }
                    }
                }
            }
            else
            {
                bool engineFound = false;

                using (ACMonitor.Lock(_20092_LockRealEngines))
                {
                    engineFound = _RealEngines.TryGetValue(acClass.ACTypeID, out engine);
                }
                if (!engineFound)
                {
                    IEnumerable<ACClassMethod> query = acClass.MethodsCached.Where(c => c.ACKind == Global.ACKinds.MSMethodExt
                                                            || c.ACKind == Global.ACKinds.MSMethodExtTrigger
                                                            || c.ACKind == Global.ACKinds.MSMethodExtClient);
                    if (!query.Any())
                        return null;
                    ScriptEngine engine2 = new ScriptEngine(acClass);
                    foreach (ACClassMethod acClassMethod in query)
                    {
                        engine2.RegisterScript(acClassMethod.ACIdentifier, acClassMethod.Sourcecode, acClassMethod.ContinueByError, acClassMethod);
                    }

                    using (ACMonitor.Lock(_20092_LockRealEngines))
                    {
                        if (!_RealEngines.TryGetValue(acClass.ACTypeID, out engine))
                        {
                            _RealEngines.Add(acClass.ACTypeID, engine2);
                            engine = engine2;
                        }
                    }
                }

            }

            return engine;
        }

        #endregion

        #region Properties

        private Assembly _CompiledAssembly;
        public Assembly CompiledAssembly
        {
            get
            {
                return _CompiledAssembly;
            }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the code has been compiled successfully.
        /// </summary>
        public bool IsCompiled
        {
            get { return _CompiledAssembly != null; }
        }

        List<Msg> _CompileErrors = null;
        /// <summary>
        /// </summary>
        public List<Msg> CompileErrors
        {
            get
            {
                return _CompileErrors;
            }
        }


        #endregion

        #region Methods
        /// <summary>Compiles the C# functions contained in the collection.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        public bool Compile()
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            PortableExecutableKinds peKind;
            ImageFileMachine machine;
            Platform compilerPlatform = Platform.X64;
            execAssembly.ManifestModule.GetPEKind(out peKind, out machine);
            if ((peKind & PortableExecutableKinds.Required32Bit) == PortableExecutableKinds.Required32Bit)
                compilerPlatform = Platform.X86;
            else if ((peKind & PortableExecutableKinds.PE32Plus) == PortableExecutableKinds.PE32Plus)
                compilerPlatform = Platform.X64;

            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                                           optimizationLevel: OptimizationLevel.Release,
                                           platform: compilerPlatform,
                                           assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default);

            // Determine current UI mode
            UiMode uiMode = GetCurrentUiMode();
            ACRoot.SRoot.Messages.LogDebug(_ACType.GetACUrl(), "ScriptEngine.Compile()", $"UI Mode: {uiMode}, Total scripts: {_Scripts.Count}");

            // Collect only compatible scripts and their precompiler directives
            var compatibleScripts = new List<string>();
            var scriptAssemblies = new List<string>();
            var scriptNamespaces = new List<string>();
            var scriptsNeedingTargetUpdate = new List<KeyValuePair<Script, ScriptTarget>>();

            foreach (Script script in _Scripts)
            {
                ScriptTarget target;
                string detectionMethod;

                // Try cached value from ACClassMethod.IsSourcecodeForUI first
                if (script.ACClassMethod != null && script.ACClassMethod.SourceCodeTargetFramework != ScriptTarget.Unknown)
                {
                    target = script.ACClassMethod.SourceCodeTargetFramework;
                    detectionMethod = "cached";
                }
                else
                {
                    // Fall back to code analysis
                    target = ParseScriptTarget(script.RawSource);
                    detectionMethod = "analyzed";

                    // Track scripts whose target was Unknown (will update after successful compilation)
                    if (script.ACClassMethod != null && script.ACClassMethod.SourceCodeTargetFramework == ScriptTarget.Unknown)
                    {
                        scriptsNeedingTargetUpdate.Add(new KeyValuePair<Script, ScriptTarget>(script, target));
                    }
                }

                bool isCompatible = IsScriptCompatible(target, uiMode);
                if (!isCompatible)
                {
                    ACRoot.SRoot.Messages.LogDebug(_ACType.GetACUrl(), "ScriptEngine.Compile()",
                        $"SKIP '{script.ACMethodName}' ({detectionMethod}): target={target}, uiMode={uiMode}");
                    continue;
                }

                ACRoot.SRoot.Messages.LogDebug(_ACType.GetACUrl(), "ScriptEngine.Compile()",
                    $"INCLUDE '{script.ACMethodName}' ({detectionMethod}): target={target}, uiMode={uiMode}");
                compatibleScripts.Add(script.Sourcecode);
                ParsePrecompilerDirectives(script.RawSource, scriptAssemblies, scriptNamespaces);
            }

            // CRITICAL: Filter out UI-specific namespaces that don't match the current UI mode
            // This prevents compilation errors when incompatible namespaces leak through
            FilterNamespacesForUiMode(scriptNamespaces, uiMode);

            ACRoot.SRoot.Messages.LogDebug(_ACType.GetACUrl(), "ScriptEngine.Compile()",
                $"Final: {compatibleScripts.Count} scripts, {scriptNamespaces.Count} namespaces from {_Scripts.Count} total");

            // Setup paths and base references
            string dotNetPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            string exePath = ACRoot.SRoot.Environment.Rootpath;

            ACRoot.SRoot.Messages.LogDebug(_ACType.GetACUrl(), "ScriptEngine.Compile()", String.Format("Compiler {0}, DotNetPath {1}, UI Mode: {2}", compilationOptions, dotNetPath, uiMode));

            var references = new List<MetadataReference>();
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "WindowsBase.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Core.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Data.dll"));
            references.Add(MetadataReference.CreateFromFile(exePath + "Microsoft.EntityFrameworkCore.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Runtime.dll"));

            // Add UI-framework-specific references based on UI mode
            if (uiMode == UiMode.Wpf)
            {
                string wpfPath = dotNetPath.Replace("Microsoft.NETCore.App", "Microsoft.WindowsDesktop.App");
                if (Directory.Exists(wpfPath) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    references.Add(MetadataReference.CreateFromFile(wpfPath + "PresentationCore.dll"));
                    ACRoot.SRoot.Messages.LogDebug(_ACType.GetACUrl(), "ScriptEngine.Compile()", "Added WPF PresentationCore.dll reference");
                }
            }

            // Add references from loaded assemblies
            foreach (Assembly classAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (classAssembly.IsDynamic
                        || classAssembly.EntryPoint != null
                        || String.IsNullOrEmpty(classAssembly.Location))
                        continue;
                    if (!references.Any(c => c.Display == classAssembly.Location))
                        references.Add(MetadataReference.CreateFromFile(classAssembly.Location));
                }
                catch (Exception e)
                {
                    ACRoot.SRoot.Messages.LogDebug(_ACType.GetACUrl(), "ScriptEngine.Compile()", String.Format("Exception1 {0}", e.Message));
                }
            }

            // Add script-specific assembly references (only for compatible scripts)
            foreach (string refAssembly in scriptAssemblies)
            {
                // Skip WPF-specific assemblies when not in WPF mode
                if (refAssembly.Equals("PresentationCore.dll", StringComparison.OrdinalIgnoreCase) && uiMode != UiMode.Wpf)
                    continue;
                if (refAssembly.Equals("PresentationFramework.dll", StringComparison.OrdinalIgnoreCase) && uiMode != UiMode.Wpf)
                    continue;
                if (refAssembly.Equals("WindowsBase.dll", StringComparison.OrdinalIgnoreCase) && uiMode == UiMode.Avalonia)
                    continue;

                string refAssemblyTemp = refAssembly;
                if (!refAssemblyTemp.EndsWith(".dll"))
                    refAssemblyTemp += ".dll";

                string assemblyNameAndPath = ACRoot.SRoot.Environment.Rootpath + refAssemblyTemp;
                if (!System.IO.File.Exists(assemblyNameAndPath))
                    assemblyNameAndPath = System.IO.Path.Combine(dotNetPath, refAssemblyTemp);
                if (!references.OfType<PortableExecutableReference>().Any(c => System.IO.Path.GetFileName(c.FilePath) == refAssemblyTemp))
                {
                    try
                    {
                        references.Add(MetadataReference.CreateFromFile(assemblyNameAndPath));
                    }
                    catch (Exception ex)
                    {
                        ACRoot.SRoot.Messages.LogDebug(_ACType.GetACUrl(), "ScriptEngine.Compile()",
                            $"Could not add assembly reference '{refAssemblyTemp}' at '{assemblyNameAndPath}': {ex.Message}");
                    }
                }
            }

            try
            {
                _CompiledAssembly = null;
                // Get the code using filtered compatible scripts and namespaces
                AddDefaultNamespaces(scriptNamespaces);
                string compileUnit = GetCode(scriptNamespaces, compatibleScripts);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(compileUnit);
                // Compile the code
                CSharpCompilation compilation = CSharpCompilation.Create(execAssembly.FullName,
                                            new[] { syntaxTree },
                                            references,
                                            compilationOptions);

                MemoryStream ms = new MemoryStream();
                EmitResult emitResult = compilation.Emit(ms);
                if (!emitResult.Success)
                {
                    // There are errors
                    _CompileErrors = new List<Msg>();
                    foreach (Diagnostic err in emitResult.Diagnostics)
                    {
                        Msg ce = new Msg();
                        ce.Column = err.Location.GetLineSpan().StartLinePosition.Character;
                        ce.Row = err.Location.GetLineSpan().StartLinePosition.Line + 1; // Need to add 1 to get the correct row number
                        ce.ACIdentifier = err.Id;
                        ce.Message = err.GetMessage();
                        ce.MessageLevel = err.Severity == DiagnosticSeverity.Warning ? eMsgLevel.Warning : eMsgLevel.Error;
                        _CompileErrors.Add(ce);
                        ACRoot.SRoot.Messages.LogError(_ACType.GetACUrl(), "ScriptEngine.Compile()", String.Format("Error at Row {0}, Column {1}, ErrorNumber {2}, Message {3}", ce.Row, ce.Column, ce.ACIdentifier, ce.Message));
                    }
                    return false;
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    _CompiledAssembly = Assembly.Load(ms.ToArray());

                    Database dbNeedSave = null;
                    // Cache the ScriptTarget for scripts that had Unknown status
                    foreach (var kvp in scriptsNeedingTargetUpdate)
                    {
                        Script script = kvp.Key;
                        ScriptTarget detectedTarget = kvp.Value;
                        if (script.ACClassMethod != null && script.ACClassMethod.SourceCodeTargetFramework == ScriptTarget.Unknown)
                        {
                            if (script.ACClassMethod.Database == Database.GlobalDatabase)
                            {
                                using (Database db = new Database())
                                {
                                    ACClassMethod method = db.ACClassMethod.Where(m => m.ACClassMethodID == script.ACClassMethod.ACClassMethodID).FirstOrDefault();
                                    if (method != null)
                                    {
                                        method.SourceCodeTargetFramework = detectedTarget;
                                        db.SaveChanges();
                                    }
                                }
                            }
                            else
                            {
                                script.ACClassMethod.SourceCodeTargetFramework = detectedTarget;
                                dbNeedSave = script.ACClassMethod.Database;
                            }
                            ACRoot.SRoot.Messages.LogDebug(_ACType.GetACUrl(), "ScriptEngine.Compile()",
                                $"Cached ScriptTarget '{script.ACMethodName}': {detectedTarget}");
                        }
                    }

                    if (dbNeedSave != null)
                    {
                        using (ACMonitor.Lock(dbNeedSave.QueryLock_1X000))
                        {
                            dbNeedSave.ACSaveChanges();
                        }
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                ACRoot.SRoot.Messages.LogException(_ACType.GetACUrl(), "ScriptEngine.Compile()", String.Format("Exeption {0}", e.Message));
            }
            return _CompiledAssembly != null;
        }

        /// <summary>
        /// This method creates the <see cref="SyntaxTree"/> containing the actual C# code that will be compiled.
        /// </summary>
        /// <param name="namespaces">The namespaces to include in the generated code.</param>
        /// <param name="scriptMethods">The preprocessed script method source codes to compile.</param>
        /// <returns>A string containing the complete C# code to be compiled.</returns>
        private string GetCode(IEnumerable<string> namespaces, List<string> scriptMethods)
        {
            var sb = new StringBuilder();

            // Add using statements
            sb.AppendLine("using System;");
            foreach (string namespaceName in namespaces)
            {
                sb.AppendLine($"using {namespaceName};");
            }

            sb.AppendLine();

            // Add namespace declaration
            sb.AppendLine($"namespace {C_ScriptNamespace}");
            sb.AppendLine("{");

            // Add static class declaration
            sb.AppendLine($"    public static class {C_ScriptStaticClassName}");
            sb.AppendLine("    {");

            // Add the actual script methods
            if (scriptMethods != null && scriptMethods.Any())
            {
                foreach (string scriptMethod in scriptMethods)
                {
                    if (!string.IsNullOrEmpty(scriptMethod))
                    {
                        // Indent each line of the script methods
                        var lines = scriptMethod.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string line in lines)
                        {
                            sb.AppendLine($"        {line}");
                        }
                    }
                }
            }

            sb.AppendLine("    }"); // Close class
            sb.AppendLine("}");     // Close namespace

            return sb.ToString();

            /* Obosolete Code for CodeDom
            // Create a new CodeCompileUnit to contain the program graph
            CodeCompileUnit compileUnit = new CodeCompileUnit();

            // Declare a new namespace 
            CodeNamespace rulesScript = new CodeNamespace(C_ScriptNamespace);

            // Add the new namespace to the compile unit.
            compileUnit.Namespaces.Add(rulesScript);

            // Add the new namespace using for the required namespaces.
            rulesScript.Imports.Add(new CodeNamespaceImport("System"));
            foreach (string mynamespace in namespaces)
            {
                rulesScript.Imports.Add(new CodeNamespaceImport(mynamespace));
            }

            // Declare a new type called ScriptFunctions.
            CodeTypeDeclaration scriptFunctions = new CodeTypeDeclaration(C_ScriptStaticClassName);

            // Add the code here
            CodeSnippetTypeMember mem = new CodeSnippetTypeMember(_Scripts.ToString());
            scriptFunctions.Members.Add(mem);

            // Add the type to the namespace
            rulesScript.Types.Add(scriptFunctions);

            // Generate the code as C# source code
            StringBuilder sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                CSharpCodeProvider codeProvider = new CSharpCodeProvider();
                codeProvider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
            }

            return sb.ToString();
            */
        }

        public void RegisterScript(string acMethodName, string sourcecode, bool continueOnError)
        {
            RegisterScript(acMethodName, sourcecode, continueOnError, null);
        }

        public void RegisterScript(string acMethodName, string sourcecode, bool continueOnError, ACClassMethod acClassMethod)
        {
            _CompiledAssembly = null;
            Script script = new Script(acMethodName, this, sourcecode, continueOnError, acClassMethod);
            _Scripts.AddScript(script);
        }

        public void AddScript(string acMethodName)
        {
            _CompiledAssembly = null;
            _Scripts.Remove(acMethodName);
        }

        public bool ExistsScript(ScriptTrigger.Type triggerType, String methodNamePostfix)
        {
            if (_Scripts == null || !_Scripts.Any())
                return false;
            ScriptTrigger scriptTrigger = ScriptTrigger.ScriptTriggers[(int)triggerType];
            int pos = methodNamePostfix.IndexOf('!');
            if (pos == 0)
                methodNamePostfix = methodNamePostfix.Substring(1);
            var query = _Scripts.GetTriggerScripts(scriptTrigger, methodNamePostfix);
            return (query != null && query.Any());
        }

        public bool ExistsScript(string acMethodName)
        {
            return _Scripts.Contains(acMethodName);
        }

        public object Execute(string acMethodName, object[] parameters)
        {
            return _Scripts[acMethodName].Invoke(parameters);
        }

        public object TriggerScript(ScriptTrigger.Type triggerType, String methodNamePostFix, object[] parameters)
        {
            if (_Scripts == null || !_Scripts.Any())
                return true;

            ScriptTrigger scriptTrigger = ScriptTrigger.ScriptTriggers[(int)triggerType];
            int pos = methodNamePostFix.IndexOf('!');
            if (pos == 0)
                methodNamePostFix = methodNamePostFix.Substring(1);

            var scripts = _Scripts.GetTriggerScripts(scriptTrigger, methodNamePostFix);
            if (scripts != null && scripts.Any())
            {
                foreach (Script script in scripts)
                {
                    try
                    {
                        if (parameters == null)
                        {
                            if (!String.IsNullOrEmpty(scriptTrigger.MethodReturnSignature))
                            {
                                string returnType = scriptTrigger.MethodReturnSignature.ToLower();
                                if (returnType == "bool")
                                {
                                    if (!(bool)script.Invoke())
                                    {
                                        if (script.ContionueOnError)
                                            continue;

                                        return false;
                                    }
                                }
                                else if (scriptTrigger.MethodReturnSignature == "Msg" || scriptTrigger.MethodReturnSignature == "MsgWithDetails")
                                {
                                    object result = script.Invoke();
                                    if (result != null)
                                    {
                                        if (script.ContionueOnError)
                                            continue;

                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                script.Invoke();
                                return null;
                            }
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(scriptTrigger.MethodReturnSignature))
                            {
                                string returnType = scriptTrigger.MethodReturnSignature.ToLower();
                                if (returnType == "bool")
                                {
                                    if (!(bool)script.Invoke(parameters))
                                    {
                                        if (script.ContionueOnError)
                                            continue;

                                        return false;
                                    }
                                }
                                else if (scriptTrigger.MethodReturnSignature == "Msg" || scriptTrigger.MethodReturnSignature == "MsgWithDetails")
                                {
                                    object result = script.Invoke(parameters);
                                    if (result != null)
                                    {
                                        if (script.ContionueOnError)
                                            continue;

                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                script.Invoke(parameters);
                                return null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (script.ContionueOnError)
                            continue;

                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ScriptEngine", "TriggerScript", msg);

                        throw new Exception();
                    }
                }
            }
            return true;
        }

#endregion

        #region Static Methods
        /// <summary>
        /// Extrahiert den Funktionsnamen aus der Funktionssignatur
        /// </summary>
        /// <param name="functionCall"></param>
        /// <returns></returns>
        public static string ExtractFunctionName(string functionCall)
        {
            int pos1 = functionCall.IndexOf('=');
            if (pos1 == -1) pos1 = 0;
            int pos2 = functionCall.IndexOf('(');
            if (pos2 == -1) return "";

            return functionCall.Substring(pos1 + 1, pos2 - (pos1 + 1));
        }

        public static string RemovePrecompilerRegionAndMakeStatic(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            string preCompStart = "/// <Precompiler>";
            string preCompEnd = "/// </Precompiler>";

            int startPosition = source.IndexOf(preCompStart);
            int endPosition = source.IndexOf(preCompEnd);

            if (startPosition < 0 || endPosition < 0)
                return source;

            string precompilerRegion = source.Substring(startPosition + preCompStart.Length, endPosition - startPosition - preCompStart.Length);
            IEnumerable<string> precompilierLines = precompilerRegion.Trim().Split(';');

            string result = System.Environment.NewLine;

            foreach (string line in precompilierLines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                string newLine = line.Replace("\n", "");
                newLine = newLine.Replace("\r", "");

                if (newLine.Trim().StartsWith("///"))
                    result += newLine + ";" + System.Environment.NewLine;
                else
                    result += newLine.Insert(0, "/// ") + ";" + System.Environment.NewLine;
            }

            source = source.Replace(precompilerRegion, result); // Declare Method as static
            return "static" + System.Environment.NewLine + source;
        }

        public static void AddDefaultNamespaces(List<string> namespaces)
        {
            if (!namespaces.Contains("gip.core.datamodel"))
                namespaces.Add("gip.core.datamodel");
            if (!namespaces.Contains("gip.core.autocomponent"))
                namespaces.Add("gip.core.autocomponent"); 
            if (!namespaces.Contains("System.Linq"))
                namespaces.Add("System.Linq");
        }

        #endregion

        #region UI Detection and Script Filtering

        /// <summary>
        /// Removes UI-specific namespaces that don't match the current UI mode.
        /// </summary>
        private static void FilterNamespacesForUiMode(List<string> namespaces, UiMode uiMode)
        {
            if (namespaces == null) return;

            // Namespaces to exclude when NOT in WPF mode
            string[] wpfNamespaces = new[]
            {
                "System.Windows",
                "System.Windows.Media",
                "System.Windows.Controls",
                "System.Windows.Input",
                "System.Windows.Shapes",
                "System.Windows.Documents",
                "System.Windows.Media.Imaging",
                "System.Windows.Media.Animation",
            };

            // Namespaces to exclude when NOT in Avalonia mode
            string[] avaloniaNamespaces = new[]
            {
                "Avalonia",
                "Avalonia.Media",
                "Avalonia.Controls",
                "Avalonia.Input",
                "Avalonia.Rendering",
                "Avalonia.Styling",
                "Avalonia.Data",
            };

            if (uiMode == UiMode.Wpf)
            {
                // Remove Avalonia namespaces
                foreach (string ns in avaloniaNamespaces)
                {
                    namespaces.RemoveAll(n => n.Equals(ns, StringComparison.OrdinalIgnoreCase) ||
                                              n.StartsWith(ns + ".", StringComparison.OrdinalIgnoreCase));
                }
            }
            else if (uiMode == UiMode.Avalonia)
            {
                // Remove WPF namespaces
                foreach (string ns in wpfNamespaces)
                {
                    namespaces.RemoveAll(n => n.Equals(ns, StringComparison.OrdinalIgnoreCase) ||
                                              n.StartsWith(ns + ".", StringComparison.OrdinalIgnoreCase));
                }
            }
            else if (uiMode == UiMode.None)
            {
                // Headless mode - remove ALL UI namespaces
                foreach (string ns in wpfNamespaces.Concat(avaloniaNamespaces))
                {
                    namespaces.RemoveAll(n => n.Equals(ns, StringComparison.OrdinalIgnoreCase) ||
                                              n.StartsWith(ns + ".", StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        /// <summary>
        /// Determines the current UI mode of the application.
        /// </summary>
        /// <returns>The current UI mode (None, Wpf, or Avalonia).</returns>
        private UiMode GetCurrentUiMode()
        {
            // Check if ACRoot is available and has UI services
            if (ACRoot.SRoot == null)
                return UiMode.None;

            try
            {
                if (ACRoot.SRoot.IsAvaloniaUI)
                    return UiMode.Avalonia;
                
                // Check if WPF services are available
                if (ACRoot.SRoot.WPFServices != null && !ACRoot.SRoot.IsAvaloniaUI)
                    return UiMode.Wpf;

                // No UI detected (headless mode, background service, etc.)
                return UiMode.None;
            }
            catch
            {
                // If we can't determine the UI mode, assume headless
                return UiMode.None;
            }
        }

        /// <summary>
        /// Parses a script's raw source code to determine its target platform.
        /// </summary>
        /// <param name="rawSource">The original source code with &lt;Precompiler&gt; directives.</param>
        /// <returns>The determined script target platform.</returns>
        public static ScriptTarget ParseScriptTarget(string rawSource)
        {
            if (string.IsNullOrEmpty(rawSource))
                return ScriptTarget.Any;

            bool hasWpfReferences = false;
            bool hasAvaloniaReferences = false;

            // Log first 500 chars of raw source for debugging
            string sourcePreview = rawSource.Length > 500 ? rawSource.Substring(0, 500) + "..." : rawSource;
            ACRoot.SRoot.Messages.LogDebug("ScriptEngine", "ParseScriptTarget()",
                $"Analyzing source preview:\n{sourcePreview}");

            // Check for WPF indicators - ONLY truly WPF-specific markers
            // IMPORTANT: Do NOT include "Brushes." or "SolidColorBrush" - these exist in BOTH frameworks!
            // Include both regular and ///-prefixed versions (precompiler directives have /// prefix)
            string[] wpfIndicators = new[]
            {
                "refassembly PresentationCore.dll",
                "/// refassembly PresentationCore.dll",
                "refassembly PresentationFramework.dll",
                "/// refassembly PresentationFramework.dll",
                "using System.Windows;",
                "/// using System.Windows;",
                "using System.Windows.Media;",
                "/// using System.Windows.Media;",
                "using System.Windows.Controls;",
                "/// using System.Windows.Controls;",
                "using System.Windows.Input;",
                "/// using System.Windows.Input;",
                "using System.Windows.Shapes;",
                "/// using System.Windows.Shapes;",
                "using System.Windows.Documents;",
                "/// using System.Windows.Documents;",
                "using System.Windows.Media.Imaging;",
                "/// using System.Windows.Media.Imaging;",
                "using System.Windows.Media.Animation;",
                "/// using System.Windows.Media.Animation;",
                "System.Windows.Brushes",
                "System.Windows.Visibility",
            };

            // Check for Avalonia indicators
            // Include both regular and ///-prefixed versions (precompiler directives have /// prefix)
            string[] avaloniaIndicators = new[]
            {
                "using Avalonia.Media;",
                "/// using Avalonia.Media;",
                "using Avalonia.Controls;",
                "/// using Avalonia.Controls;",
                "using Avalonia.Input;",
                "/// using Avalonia.Input;",
                "using Avalonia.Rendering;",
                "/// using Avalonia.Rendering;",
                "using Avalonia.Styling;",
                "/// using Avalonia.Styling;",
                "using Avalonia.Data;",
                "/// using Avalonia.Data;",
                "ISolidColorBrush",
                "Avalonia.Media.Brushes",
            };

            string matchedWpfIndicator = null;
            foreach (string indicator in wpfIndicators)
            {
                if (rawSource.Contains(indicator))
                {
                    hasWpfReferences = true;
                    matchedWpfIndicator = indicator;
                    break;
                }
            }

            string matchedAvaloniaIndicator = null;
            foreach (string indicator in avaloniaIndicators)
            {
                if (rawSource.Contains(indicator))
                {
                    hasAvaloniaReferences = true;
                    matchedAvaloniaIndicator = indicator;
                    break;
                }
            }

            // Log detection results
            ACRoot.SRoot.Messages.LogDebug("ScriptEngine", "ParseScriptTarget()",
                $"Result: hasWpf={hasWpfReferences} (matched: {matchedWpfIndicator ?? "none"}), hasAvalonia={hasAvaloniaReferences} (matched: {matchedAvaloniaIndicator ?? "none"})");

            // Determine target based on findings
            if (hasWpfReferences && !hasAvaloniaReferences)
            {
                ACRoot.SRoot.Messages.LogDebug("ScriptEngine", "ParseScriptTarget()", $"-> ScriptTarget.Wpf");
                return ScriptTarget.Wpf;
            }
            if (hasAvaloniaReferences && !hasWpfReferences)
            {
                ACRoot.SRoot.Messages.LogDebug("ScriptEngine", "ParseScriptTarget()", $"-> ScriptTarget.Avalonia");
                return ScriptTarget.Avalonia;
            }
            if (hasWpfReferences && hasAvaloniaReferences)
            {
                // Mixed - treat as WPF by default (original behavior)
                // This case should be rare; ideally scripts are separated
                ACRoot.SRoot.Messages.LogDebug("ScriptEngine", "ParseScriptTarget()", $"-> ScriptTarget.Wpf (mixed)");
                return ScriptTarget.Wpf;
            }
            ACRoot.SRoot.Messages.LogDebug("ScriptEngine", "ParseScriptTarget()", $"-> ScriptTarget.Any");
            return ScriptTarget.Any;
        }

        /// <summary>
        /// Determines if a script with the given target is compatible with the current UI mode.
        /// </summary>
        /// <param name="target">The script's target platform.</param>
        /// <param name="uiMode">The current UI mode.</param>
        /// <returns>True if the script can be compiled in the current UI mode.</returns>
        public static bool IsScriptCompatible(ScriptTarget target, UiMode uiMode)
        {
            // Scripts with no UI dependencies are always compatible
            if (target == ScriptTarget.Any)
                return true;

            // In headless mode, only compile scripts with no UI dependencies
            if (uiMode == UiMode.None)
                return false;

            // WPF scripts only compile in WPF mode
            if (target == ScriptTarget.Wpf)
                return uiMode == UiMode.Wpf;

            // Avalonia scripts only compile in Avalonia mode
            if (target == ScriptTarget.Avalonia)
                return uiMode == UiMode.Avalonia;

            return false;
        }

        /// <summary>
        /// Parses the &lt;Precompiler&gt; directives from a script's raw source code.
        /// </summary>
        /// <param name="rawSource">The original source code with &lt;Precompiler&gt; directives.</param>
        /// <param name="assemblies">Collection to add discovered assembly references to.</param>
        /// <param name="namespaces">Collection to add discovered namespace imports to.</param>
        public static void ParsePrecompilerDirectives(string rawSource, List<string> assemblies, List<string> namespaces)
        {
            if (string.IsNullOrEmpty(rawSource))
                return;

            string preCompStart = "/// <Precompiler>";
            string preCompEnd = "/// </Precompiler>";

            int startPos = rawSource.IndexOf(preCompStart);
            while (startPos >= 0)
            {
                int endPos = rawSource.IndexOf(preCompEnd, startPos);
                if (endPos < 0)
                    break;

                string region = rawSource.Substring(startPos + preCompStart.Length, endPos - startPos - preCompStart.Length);
                string[] lines = region.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    string trimmed = line.Trim();
                    // Remove leading /// comment prefix
                    if (trimmed.StartsWith("///"))
                        trimmed = trimmed.Substring(3).Trim();

                    // Parse refassembly directive
                    if (trimmed.StartsWith("refassembly "))
                    {
                        string asmName = trimmed.Substring(12).TrimEnd(';');
                        if (!string.IsNullOrEmpty(asmName) && !assemblies.Contains(asmName))
                            assemblies.Add(asmName);
                    }
                    // Parse using directive
                    else if (trimmed.StartsWith("using "))
                    {
                        string nsName = trimmed.Substring(6).TrimEnd(';');
                        if (!string.IsNullOrEmpty(nsName) && !namespaces.Contains(nsName))
                            namespaces.Add(nsName);
                    }
                }

                // Look for next Precompiler block
                startPos = rawSource.IndexOf(preCompStart, endPos);
            }
        }

        #endregion
    }
}
