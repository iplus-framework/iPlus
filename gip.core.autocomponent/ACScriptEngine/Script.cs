// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Reflection;
using System.Linq;
using gip.core.datamodel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace gip.core.autocomponent
{
	/// <summary>
	/// Script with Method declaration, that is compiled into the <see cref="ScriptEngine"/>
	/// </summary>
	public class Script 
	{
        #region private members
        private ScriptEngine _ScriptEngine;
		#endregion


        #region c'tors
        public Script(string acMethodName, ScriptEngine engine, string sourceCode, bool continueOnError)
        {
            _ACMethodName = acMethodName;
            _ScriptEngine = engine;
            if (string.IsNullOrEmpty(sourceCode))
                throw new ArgumentNullException("sourceCode is empty");
            _SourceCode = ScriptEngine.RemovePrecompilerRegionAndMakeStatic(sourceCode);
            _ContinueOnError = continueOnError;
            _ScriptTrigger = ScriptTrigger.ScriptTriggers.Where(c => acMethodName.StartsWith(c.MethodNamePrefix)).FirstOrDefault();
        }
        #endregion


        #region Public Methods

        /// <summary>
		/// Returns a string containing the function text.
		/// </summary>
		/// <returns>A <see cref="string"/> that represents the function text.</returns>
		public override string ToString()
		{
			return _SourceCode;
		}
		
		
		/// <summary>
		/// Invokes the C# function with no parameters.
		/// </summary>
		/// <returns>An <see cref="object"/> representing the return type of the defined C# function.</returns>
		public object Invoke()
		{
			return Invoke(null);
		}

		/// <summary>
		/// Invokes the C# function with parameters.
		/// </summary>
		/// <returns>An <see cref="object"/> representing the return type of the defined C# function.</returns>
		public object Invoke(object[] parms)
		{
            if (_ScriptEngine == null)
                throw new ApplicationException("Function has not been compiled");
            if (!_ScriptEngine.ExistsScript(this.ACMethodName))
                throw new ApplicationException("Function does not exist in ScriptEngine");

            // Wenn noch nicht kompiliert wurde, dann jetzt automatisch
            if (!_ScriptEngine.IsCompiled)
                _ScriptEngine.Compile();
            if (!_ScriptEngine.IsCompiled)
                throw new ApplicationException("Function has not been compiled");

            Type t = _ScriptEngine.CompiledAssembly.GetType(ScriptEngine.C_TypeNameScriptStaticClass);
            MethodInfo info = t.GetMethod(this.ACMethodName);
            object o = null;
            if (parms == null) parms = new object[0];
            try
            {
                if (info != null)
                    o = info.Invoke(null, parms);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("Script", "Invoke", msg);
            }
            return o;
		}
        #endregion


        #region Properties
        string _ACMethodName;
        public string ACMethodName
        {
            get
            {
                return _ACMethodName;
            }
            set
            {
                _ACMethodName = value;
            }
        }

        string _SourceCode;
        public string Sourcecode
        {
            get
            {
                return _SourceCode;
            }
        }

        ScriptTrigger _ScriptTrigger;
        public ScriptTrigger ScriptTrigger
        {
            get
            {
                return _ScriptTrigger;
            }
        }

        int _SortIndex;
        public int SortIndex
        {
            get
            {
                return _SortIndex;
            }
            set
            {
                _SortIndex = value;
            }
        }


        bool _ContinueOnError;
        public bool ContionueOnError
        {
            get
            {
                return _ContinueOnError;
            }
        }
        #endregion

    }
}
