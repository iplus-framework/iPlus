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

namespace gip.core.autocomponent
{
    /// <summary>
    /// Die ScriptEngine kann C#-Funktionen compilieren und ausf�hren.
    /// 
    /// Die Funktion muss folgende Voraussetzungen erf�llen, damit ausreichend 
    /// Neutralit�t f�r die verschiedenen Anwendungsf�lle gew�hrleistet ist:
    /// 1. Deklaration ist immer "public static"
    /// 2. Es darf immer nur einen R�ckgabewert vom Typ bool geben. Nur so kann das Framework
    ///    das Gesamtverhalten bei Erfolg oder Mi�erfolg steuern.
    /// 4. �bergebene Objekte oder Interfaces k�nnen in der Funktion manipuliert werden.
    /// 5. Es ist auch erlaubt ein komplettes BSO oder den AppContext zu �bergeben.
    /// 6. Je EventTrigger (z.B. "PreExecuteCommand") kann es immer nur genau eine 
    ///    Funktionssignatur geben.
    /// 7. Alle Eventtrigger sind bei der ScriptEngine zu registrieren.
    ///    Die Registrierung erfolgt immer in der Klasse AppContext und der 
    ///    Funktion "RegisterEventtrigger()". 
    /// 
    /// 
    /// 
    /// Das Beispiel stammt noch aus der Prototypimplementierung:
    /// 
    ///     Taschenrechner tr = new Taschenrechner();
    ///
    ///     string code = @"public static int Berechne(ITaschenrechner tr, int value1, int value2, int value3)
    ///		{
    ///         int summe = tr.Addiere(value1, value2);
    ///         int differenz = tr.Subtrahiere(summe, value3);
    ///		    return differenz;
    ///	    }";
    ///
    ///     ScriptEngine se = new ScriptEngine();
    ///     Function f = new Function();
    ///     f.Name = "Berechne";
    ///     f.Text = code;
    ///     se.Functions.Add(f);
    ///     se.Compile();
    ///
    ///     int value1 = 10;
    ///     int value2 = 20;
    ///     int value3 = 5;
    ///
    ///     int s = (int)se.Functions["Berechne"].Invoke(new Object[] {tr, value1, value2, value3 });
    /// </summary>
    /// 

    public class ScriptEngine : IScriptEngine
    {
        private static Dictionary<Guid, ScriptEngine> _ProxyEngines = new Dictionary<Guid, ScriptEngine>();
        private static readonly ACMonitorObject _20091_LockProxyEngines = new ACMonitorObject(20091);
        private static Dictionary<Guid, ScriptEngine> _RealEngines = new Dictionary<Guid, ScriptEngine>();
        private static readonly ACMonitorObject _20092_LockRealEngines = new ACMonitorObject(20092);

        #region Private Members
        internal EmitResult emitResult;
        internal CSharpCompilation compilation;
        internal string compileUnit;
        internal SyntaxTree syntaxTree;
        List<string> mAsemblies;
        List<string> mNamespaces;
        ScriptList mScripts;
        bool myIsCompiled = false;
        List<Msg> mCompileErrors = null;
        IACType _ACType = null;
        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for the <see cref="ScriptEngine"/> class.
        /// </summary>
        public ScriptEngine(IACType acType)
        {
            _ACType = acType;
            mScripts = new ScriptList(this);
            mAsemblies = new List<string>();
            mNamespaces = new List<string>();

            RegisterNamespace("gip.core.datamodel");
            RegisterNamespace("gip.core.autocomponent");
            RegisterNamespace("System.Linq");
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

        /// <summary>
        /// Gets a boolean value indicating whether the code has been compiled successfully.
        /// </summary>
        public bool IsCompiled
        {
            get { return myIsCompiled; }
        }

        /// <summary>
        /// </summary>
        public List<Msg> CompileErrors
        {
            get
            {
                return mCompileErrors;
            }
        }


        #endregion

        #region Public Methods
        /// <summary>Compiles the C# functions contained in the collection.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        public bool Compile()
        {
            foreach(var script in mScripts)
            {
                script.Sourcecode = CommentPrecompilerRegion(script.Sourcecode);
            }

            //CodeDomProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            //CompilerParameters parms = new CompilerParameters();

            // Configure parameters
            //parms.GenerateExecutable = false;
            //parms.GenerateInMemory = true;
            //parms.IncludeDebugInformation = false;

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

            ACRoot.SRoot.Messages.LogDebug(_ACType.GetACUrl(), "ScriptEngine.Compile()", String.Format("Complier {0}, DotNetPath {1}", compilationOptions, dotNetPath));



            var references = new List<MetadataReference>();
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.dll"));
            //references.Add(MetadataReference.CreateFromFile(dotNetPath + "WPF\\PresentationCore.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "WindowsBase.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Core.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Data.dll"));
            //references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Data.Entity.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Runtime.dll"));
            foreach (Assembly classAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (classAssembly.GlobalAssemblyCache
                        || classAssembly.IsDynamic
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

            // Using deklarations in Script-Code
            foreach (string usingNamespace in mScripts.UsingNamespaces)
            {
                RegisterNamespace(usingNamespace);
            }

            // Assembly deklarations in Script-Code
            foreach (string refAssembly in mScripts.Assemblies)
            {
                string refAssemblyTemp = refAssembly;
                if (!refAssemblyTemp.EndsWith(".dll"))
                    refAssemblyTemp += ".dll";

                string assemblyNameAndPath = ACRoot.SRoot.Environment.Rootpath + refAssemblyTemp;
                if (!System.IO.File.Exists(assemblyNameAndPath))
                    assemblyNameAndPath = System.IO.Path.Combine(dotNetPath, refAssemblyTemp);
                if (!references.OfType<string>().Any(c => System.IO.Path.GetFileName(c) == refAssemblyTemp))
                    references.Add(MetadataReference.CreateFromFile(assemblyNameAndPath));
            }

            try
            {
                // Get the code
                compileUnit = GetCode();
                syntaxTree = CSharpSyntaxTree.ParseText(compileUnit);
                // Compile the code

                //myResults = provider.CompileAssemblyFromDom(parms, compileUnit);

                compilation = CSharpCompilation.Create(execAssembly.FullName,
                                            new[] { syntaxTree },
                                            references,
                                            compilationOptions);

                MemoryStream ms = new MemoryStream();
                emitResult = compilation.Emit(ms);
                if (!emitResult.Success)
                {
                    // There are errors
                    mCompileErrors = new List<Msg>();
                    foreach (Diagnostic err in emitResult.Diagnostics)
                    {
                        Msg ce = new Msg();
                        ce.Column = err.Location.GetLineSpan().StartLinePosition.Character;
                        ce.Row = err.Location.GetLineSpan().StartLinePosition.Line + 1; // Need to add 1 to get the correct row number
                        ce.ACIdentifier = err.Id;
                        ce.Message = err.GetMessage();
                        ce.MessageLevel = err.Severity == DiagnosticSeverity.Warning ? eMsgLevel.Warning : eMsgLevel.Error;
                        mCompileErrors.Add(ce);
                        ACRoot.SRoot.Messages.LogError(_ACType.GetACUrl(), "ScriptEngine.Compile()", String.Format("Error at Row {0}, Column {1}, ErrorNumber {2}, Message {3}", ce.Row, ce.Column, ce.ACIdentifier, ce.Message));
                    }
                    myIsCompiled = false;
                    return false;
                }
                else
                {
                    // Success
                    myIsCompiled = true;
                    return true;
                }

                //myResults = provider.CompileAssemblyFromDom(parms, compileUnit);
                //if (myResults.Errors.Count > 0)
                //{
                //    // There are errors
                //    mCompileErrors = new List<Msg>();
                //    foreach (CompilerError err in myResults.Errors)
                //    {
                //        Msg ce = new Msg();
                //        ce.Column = err.Column;
                //        ce.Row = err.Line - 20;  // TODO: Wie bekommt man die korrekte Zeilennummer heraus ?
                //        ce.ACIdentifier = err.ErrorNumber;
                //        ce.Message = err.ErrorText;
                //        ce.MessageLevel = err.IsWarning ? eMsgLevel.Warning : eMsgLevel.Error;
                //        mCompileErrors.Add(ce);
                //        ACRoot.SRoot.Messages.LogError(_ACType.GetACUrl(), "ScriptEngine.Compile()", String.Format("Error at Row {0}, Column {1}, ErrorNumber {2}, Message {3}", ce.Row, ce.Column, ce.ACIdentifier, ce.Message));
                //    }
                //    myIsCompiled = false;
                //    return false;
                //}
                
            }
            catch (Exception e)
            {
                ACRoot.SRoot.Messages.LogException(_ACType.GetACUrl(), "ScriptEngine.Compile()", String.Format("Exeption {0}", e.Message));
            }
            myIsCompiled = false;
            return false;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// This method creates the <see cref="SyntaxTree"/> containing the actual C# code that will be compiled.
        /// </summary>
        /// <returns>A <see cref="SyntaxTree"/> containing the code to be compiled.</returns>
        private string GetCode()
        {
            // Create a new CodeCompileUnit to contain the program graph
            CodeCompileUnit compileUnit = new CodeCompileUnit();

            // Declare a new namespace 
            CodeNamespace rulesScript = new CodeNamespace("RulesScript");

            // Add the new namespace to the compile unit.
            compileUnit.Namespaces.Add(rulesScript);

            // Add the new namespace using for the required namespaces.
            rulesScript.Imports.Add(new CodeNamespaceImport("System"));
            foreach (string mynamespace in mNamespaces)
            {
                rulesScript.Imports.Add(new CodeNamespaceImport(mynamespace));
            }

            // Declare a new type called ScriptFunctions.
            CodeTypeDeclaration scriptFunctions = new CodeTypeDeclaration("ScriptFunctions");

            // Add the code here
            CodeSnippetTypeMember mem = new CodeSnippetTypeMember(mScripts.ToString());
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
        #endregion

        #region IScriptEngine Member

        public void RegisterAssembly(string useAssembly)
        {
            myIsCompiled = false;
            if (!mAsemblies.Contains(useAssembly))
                mAsemblies.Add(useAssembly);
        }

        public void UnregisterAssembly(string useAssembly)
        {
            myIsCompiled = false;
            mAsemblies.Remove(useAssembly);
        }

        public void RegisterNamespace(string useNamespace)
        {
            myIsCompiled = false;
            if (!mNamespaces.Contains(useNamespace))
                mNamespaces.Add(useNamespace);
        }

        public void UnregisterNamespace(string useNamespace)
        {
            myIsCompiled = false;
            mNamespaces.Remove(useNamespace);
        }

        public void RegisterScript(string acMethodName, string sourcecode, bool continueByError)
        {
            myIsCompiled = false;
            Script script = new Script(acMethodName);
            //script.Sourcecode += "static " + sourcecode;
            script.ContionueByError = continueByError;
            mScripts.AddScript(script);
        }

        public void UnregisterScript(string acMethodName)
        {
            myIsCompiled = false;
            mScripts.Remove(acMethodName);
        }

        public bool ExistsScript(ScriptTrigger.Type triggerType, String methodNamePostfix)
        {
            if (mScripts == null || !mScripts.Any())
                return false;
            ScriptTrigger scriptTrigger = ScriptTrigger.ScriptTriggers[(int)triggerType];
            int pos = methodNamePostfix.IndexOf('!');
            if (pos == 0)
                methodNamePostfix = methodNamePostfix.Substring(1);
            var query = mScripts.GetTriggerScripts(scriptTrigger, methodNamePostfix);
            return (query != null && query.Any());
            //return mScripts.Contains(scriptTrigger.GetMethodName(methodNamePostfix));
        }

        public bool ExistsScript(string acMethodName)
        {
            return mScripts.Contains(acMethodName);
        }

        public object Execute(string acMethodName, object[] parameters)
        {
            return mScripts[acMethodName].Invoke(parameters);
        }

        public object TriggerScript(ScriptTrigger.Type triggerType, String methodNamePostFix, object[] parameters)
        {
            if (mScripts == null || !mScripts.Any())
                return true;

            ScriptTrigger scriptTrigger = ScriptTrigger.ScriptTriggers[(int)triggerType];
            int pos = methodNamePostFix.IndexOf('!');
            if (pos == 0)
                methodNamePostFix = methodNamePostFix.Substring(1);

            var scripts = mScripts.GetTriggerScripts(scriptTrigger, methodNamePostFix);
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
                                        if (script.ContionueByError)
                                            continue;

                                        return false;
                                    }
                                }
                                else if (scriptTrigger.MethodReturnSignature == "Msg" || scriptTrigger.MethodReturnSignature == "MsgWithDetails")
                                {
                                    object result = script.Invoke();
                                    if (result != null)
                                    {
                                        if (script.ContionueByError)
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
                                        if (script.ContionueByError)
                                            continue;

                                        return false;
                                    }
                                }
                                else if (scriptTrigger.MethodReturnSignature == "Msg" || scriptTrigger.MethodReturnSignature == "MsgWithDetails")
                                {
                                    object result = script.Invoke(parameters);
                                    if (result != null)
                                    {
                                        if (script.ContionueByError)
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
                        if (script.ContionueByError)
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

#region Static Members
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

        public static string CommentPrecompilerRegion(string source)
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

            return source.Replace(precompilerRegion, result);
        }

        #endregion
    }
}
