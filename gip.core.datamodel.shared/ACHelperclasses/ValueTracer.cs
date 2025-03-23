using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Runtime.CompilerServices;

namespace gip.core.datamodel
{
    public class ValueTracer
    {
        StringBuilder _sb = null;
        public ValueTracer(bool trace, [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (trace)
            {
                _sb = new StringBuilder();
                _sb.AppendFormat("Starting at line [{0}];", sourceLineNumber);
            }
        }

        public void Set<T>(ref T variable, 
            T value, 
            [CallerArgumentExpression("variable")] string varName = "", 
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (_sb != null)
                _sb.AppendFormat("{0}[{1}]={2};", varName, sourceLineNumber, value);
            variable = value;
        }

        public void LogVariable(string value, string varName, [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (_sb != null)
                _sb.AppendFormat("{0}[{1}]={2};", varName, sourceLineNumber, value);
        }

        public bool IsTracing
        {
            get
            {
                return _sb != null;
            }
        }

        public string Trace
        {
            get
            {
                return _sb?.ToString();
            }
        }
    }
}
