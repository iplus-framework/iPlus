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
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Text;
using System.Diagnostics;
using SkiaSharp;
using System.Runtime.Loader;

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
                        engine2.RegisterScript(acClassMethod.ACIdentifier, acClassMethod.Sourcecode, acClassMethod.ContinueByError);
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
                        engine2.RegisterScript(acClassMethod.ACIdentifier, acClassMethod.Sourcecode, acClassMethod.ContinueByError);
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

            // TODO: Add references Should these be configurable?
            string dotNetPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            string wpfPath = dotNetPath.Replace("Microsoft.NETCore.App", "Microsoft.WindowsDesktop.App");
            if (!Directory.Exists(wpfPath))
                wpfPath = null;
            string exePath = ACRoot.SRoot.Environment.Rootpath;

            ACRoot.SRoot.Messages.LogDebug(_ACType.GetACUrl(), "ScriptEngine.Compile()", String.Format("Complier {0}, DotNetPath {1}", compilationOptions, dotNetPath));

            var references = new List<MetadataReference>();
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.dll"));
            if (wpfPath != null)
                references.Add(MetadataReference.CreateFromFile(wpfPath + "PresentationCore.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "WindowsBase.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Core.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Data.dll"));
            references.Add(MetadataReference.CreateFromFile(exePath + "Microsoft.EntityFrameworkCore.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Runtime.dll"));
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

            // Assembly deklarations in Script-Code
            foreach (string refAssembly in _Scripts.Assemblies)
            {
                string refAssemblyTemp = refAssembly;
                if (!refAssemblyTemp.EndsWith(".dll"))
                    refAssemblyTemp += ".dll";

                string assemblyNameAndPath = ACRoot.SRoot.Environment.Rootpath + refAssemblyTemp;
                if (!System.IO.File.Exists(assemblyNameAndPath))
                    assemblyNameAndPath = System.IO.Path.Combine(dotNetPath, refAssemblyTemp);
                if (!references.OfType<PortableExecutableReference>().Any(c => System.IO.Path.GetFileName(c.FilePath) == refAssemblyTemp))
                    references.Add(MetadataReference.CreateFromFile(assemblyNameAndPath));
            }

            try
            {
                _CompiledAssembly = null;
                // Get the code
                List<string> namespaces = _Scripts.UsingNamespaces;
                AddDefaultNamespaces(namespaces);
                string compileUnit = GetCode(namespaces);
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
        /// <returns>A <see cref="SyntaxTree"/> containing the code to be compiled.</returns>
        private string GetCode(IEnumerable<string> namespaces)
        {
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
        }

        public void RegisterScript(string acMethodName, string sourcecode, bool continueOnError)
        {
            _CompiledAssembly = null;
            Script script = new Script(acMethodName, this, sourcecode, continueOnError);
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
    }
}
