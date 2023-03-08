using System;
using System.Reflection;
using System.Linq;

namespace gip.iplus.startup
{
	/// <summary>
	/// The Function class contains the C# function definition that is compiled into the <see cref="ScriptEngine"/>
	/// </summary>
	public class Script 
	{
		#region private members

		internal ScriptEngine myEngine = null;
        string _SourceCode = string.Empty;
        string _ACMethodName = string.Empty;
        bool _ContionueByError = false;
        int _SortIndex = 0;
		#endregion

        #region c'tors
        public Script(string acMethodName)
        {
            _ACMethodName = acMethodName;
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
			if (myEngine == null) throw new ApplicationException("Function has not been compiled");
			if (!myEngine.ExistsScript(this.ACMethodName)) throw new ApplicationException("Function does not exist in ScriptEngine");

            // Wenn noch nicht kompiliert wurde, dann jetzt automatisch
            if (!myEngine.IsCompiled)
            {
                myEngine.Compile();
            }
			if (!myEngine.IsCompiled) throw new ApplicationException("Function has not been compiled");

            //Type t = myEngine.myResults.CompiledAssembly.GetType("RulesScript.ScriptFunctions");
            Type t = myEngine.compilation.GetSemanticModel(myEngine.compileUnit).Compilation.GetTypeByMetadataName("RulesScript.ScriptFunctions").GetType();
            MethodInfo info = t.GetMethod(this.ACMethodName);
			object o = null;
			if (parms == null) parms = new object[0];
            try
            {
                //o = Invoke(parms);
                if (info != null)
                    o = info.Invoke(null, parms);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;
            }
			return o;
		}
		
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

        public string Sourcecode
        {
            get
            {
                return _SourceCode;
            }
            set
            {
                _SourceCode = value;
            }
        }

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
      
        public bool ContionueByError
        {
            get
            {
                return _ContionueByError;
            }
            set
            {
                _ContionueByError = value;
            }
        }

        public ScriptMode InstallScriptMode
        {
            get;
            set;
        }

        public DateTime ScriptDateTime
        {
            get;
            set;
        }
        #endregion

    }
}
