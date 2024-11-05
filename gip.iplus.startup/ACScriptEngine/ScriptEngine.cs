using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Linq;
using System.IO;

namespace gip.iplus.startup
{
    /// <summary>
    /// Die ScriptEngine kann C#-Funktionen compilieren und ausführen.
    /// 
    /// Die Funktion muss folgende Voraussetzungen erfüllen, damit ausreichend 
    /// Neutralität für die verschiedenen Anwendungsfälle gewährleistet ist:
    /// 1. Deklaration ist immer "public static"
    /// 2. Es darf immer nur einen Rückgabewert vom Typ bool geben. Nur so kann das Framework
    ///    das Gesamtverhalten bei Erfolg oder Mißerfolg steuern.
    /// 4. Übergebene Objekte oder Interfaces können in der Funktion manipuliert werden.
    /// 5. Es ist auch erlaubt ein komplettes BSO oder den AppContext zu übergeben.
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

    public class ScriptEngine
    {
        private static Dictionary<Guid, ScriptEngine> _ProxyEngines = new Dictionary<Guid, ScriptEngine>();
        private static Dictionary<Guid, ScriptEngine> _RealEngines = new Dictionary<Guid, ScriptEngine>();

        #region Private Members
        internal EmitResult emitResult;
        internal CSharpCompilation compilation;
        internal SyntaxTree compileUnit;
        List<string> mAsemblies;
        List<string> mNamespaces;
        ScriptList mScripts;
        bool myIsCompiled = false;
        List<string> mCompileErrors = null;
        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor for the <see cref="ScriptEngine"/> class.
        /// </summary>
        public ScriptEngine()
        {
            mScripts = new ScriptList(this);
            mAsemblies = new List<string>();
            mNamespaces = new List<string>();

            RegisterNamespace("System.Linq");
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
        /// Gets a <see cref="CompileErrors"/> collection containing <see cref="CompileError"/> objects
        /// when the compiled code has failures.
        /// </summary>
        public List<string> CompileErrors
        {
            get
            {
                return mCompileErrors;
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Compiles the C# functions contained in the <see cref="Functions"/> collection.
        /// </summary>
        /// <returns></returns>
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
            string wpfPath = dotNetPath.Replace("Microsoft.NETCore.App", "Microsoft.WindowsDesktop.App");
            if (!Directory.Exists(wpfPath))
                wpfPath = null;

            var references = new List<MetadataReference>();
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.dll"));
            if (wpfPath != null)
                references.Add(MetadataReference.CreateFromFile(wpfPath + "PresentationCore.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "WindowsBase.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Core.dll"));
            references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Data.dll"));
            //references.Add(MetadataReference.CreateFromFile(dotNetPath + "System.Data.Entity.dll"));
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
                    Console.WriteLine("ScriptEngine(10) Exception:" + e.Message);
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

                string assemblyNameAndPath = AppContext.BaseDirectory + refAssemblyTemp;
                if (!System.IO.File.Exists(assemblyNameAndPath))
                    assemblyNameAndPath = System.IO.Path.Combine(dotNetPath, refAssemblyTemp);

                if (!references.OfType<PortableExecutableReference>().Any(c => System.IO.Path.GetFileName(c.FilePath) == refAssemblyTemp))
                    references.Add(MetadataReference.CreateFromFile(assemblyNameAndPath));
            }

            try
            {
                // Get the code
                compileUnit = GetCode();
                // Compile the code

                //myResults = provider.CompileAssemblyFromDom(parms, compileUnit);

                compilation = CSharpCompilation.Create(execAssembly.FullName,
                                            new[] { compileUnit },
                                            references,
                                            compilationOptions);

                MemoryStream ms = new MemoryStream();
                emitResult = compilation.Emit(ms);

                if (!emitResult.Success)
                {
                    // There are errors
                    mCompileErrors = new List<string>();
                    foreach (Diagnostic err in emitResult.Diagnostics)
                    {
                        mCompileErrors.Add(String.Format("Error at Row {0}, Column {1}, ErrorNumber {2}, Message {3}", 
                            err.Location.GetLineSpan().StartLinePosition.Line + 1,
                            err.Location.GetLineSpan().StartLinePosition.Character,
                            err.Id,
                            err.GetMessage()));
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
            }
            catch (Exception e)
            {
                Console.WriteLine("ScriptEngine(20) Exception:"+e.Message);
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
        private SyntaxTree GetCode()
        {
            // Create a new CodeCompileUnit to contain the program graph
            //CodeCompileUnit compileUnit = new CodeCompileUnit();
            // Declare a new namespace 
            //CodeNamespace rulesScript = new CodeNamespace("RulesScript");
            NamespaceDeclarationSyntax rulesScript = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("RulesScript"));
            // Add the new namespace to the compile unit.
            //compileUnit.Namespaces.Add(rulesScript);

            // Add the new namespace using for the required namespaces.
            //rulesScript.Imports.Add(new CodeNamespaceImport("System"));
            rulesScript = rulesScript.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")));

            foreach (string mynamespace in mNamespaces)
            {
                rulesScript = rulesScript.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(mynamespace)));
            }

            // Declare a new type called ScriptFunctions.
            //CodeTypeDeclaration scriptFunctions = new CodeTypeDeclaration("ScriptFunctions");
            TypeDeclarationSyntax scriptFunctions = SyntaxFactory.ClassDeclaration("ScriptFunctions")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            // Add the code here
            //CodeSnippetTypeMember mem = new CodeSnippetTypeMember(mScripts.ToString());
            //scriptFunctions.Members.Add(mem);

            SyntaxTree tree = CSharpSyntaxTree.ParseText(mScripts.ToString());
            CompilationUnitSyntax root = (CompilationUnitSyntax)tree.GetRoot();
            SyntaxList<MemberDeclarationSyntax> members = scriptFunctions.Members.AddRange(root.Members);
            scriptFunctions = scriptFunctions.WithMembers(members);

            // Add the type to the namespace
            rulesScript = rulesScript.AddMembers(scriptFunctions);

            // Create a new CodeCompileUnit to contain the program graph
            CompilationUnitSyntax compileUnit = SyntaxFactory.CompilationUnit().AddMembers(rulesScript);

            return CSharpSyntaxTree.Create(compileUnit);
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

        public void RegisterScript(string acMethodName, string sourcecode, bool continueByError, ScriptMode scriptMode, DateTime scriptDate)
        {
            myIsCompiled = false;
            Script script = new Script(acMethodName);
            script.Sourcecode += "static " + sourcecode;
            script.ContionueByError = continueByError;
            script.InstallScriptMode = scriptMode;
            script.ScriptDateTime = scriptDate;
            mScripts.AddScript(script);
        }

        public void UnregisterScript(string acMethodName)
        {
            myIsCompiled = false;
            mScripts.Remove(acMethodName);
        }

        public bool ExistsScript(string acMethodName)
        {
            return mScripts.Contains(acMethodName);
        }

        public object Execute(string acMethodName, object[] parameters)
        {
            return mScripts[acMethodName].Invoke(parameters);
        }

        public bool Execute(ScriptMode scriptMode, string sourceInstallPath, string localInstallPath)
        {
            foreach (Script mScript in mScripts.Where(c => c.InstallScriptMode == scriptMode).OrderBy(c => c.ScriptDateTime).ToArray())
            {
                bool? scriptResult = mScript.Invoke(new object[] { sourceInstallPath, localInstallPath }) as bool?;
                if (!scriptResult.HasValue || !scriptResult.Value)
                    return false;
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
