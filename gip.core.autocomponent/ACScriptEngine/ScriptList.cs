// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
	/// <summary>
	/// A collection class containing script objects.
	/// </summary>
    public class ScriptList : List<Script> // DictionaryBase
    {
        #region Private Members
        private ScriptEngine _ScriptEngine = null;
        #endregion

        #region Constructors

        internal ScriptList(ScriptEngine engine)
        {
            _ScriptEngine = engine;
        }

        #endregion

        #region Public Methods

        /// <summary>Used to add a new function object to the <see cref="ScriptEngine" />.</summary>
        /// <param name="function"></param>
        public void AddScript(Script function)
        {
            function.SortIndex = Count + 1;
            base.Add(function);
        }

        /// <summary>
        /// Used to remove a function object from the <see cref="ScriptEngine"/>.
        /// </summary>
        /// <param name="functionName"></param>
        public void Remove(string functionName)
        {
            foreach (Script s in this)
            {
                if (s.ACMethodName == functionName)
                {
                    this.Remove(s);
                    break;
                }
            }
        }

        /// <summary>
        /// Returns a string representation of all of the function objects in the collection.
        /// </summary>
        /// <returns>a string representing the C# functions defined.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Script f in this)
            {
                sb.Append(f.Sourcecode);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets a boolean value indicating whether the function with the supplied name exists.
        /// </summary>
        /// <param name="functionName">The name of a function object.</param>
        /// <returns>true if the function exists in the collection, otherwise false.</returns>
        public bool Contains(string functionName)
        {
            foreach (Script s in this)
            {
                if (s.ACMethodName == functionName)
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<Script> GetTriggerScripts(ScriptTrigger scriptTrigger, string methodNamePostFix)
        {
            return this.Where(c =>     c.ScriptTrigger != null
                                    && c.ScriptTrigger == scriptTrigger
                                    && c.ACMethodName.StartsWith(scriptTrigger.GetMethodName(methodNamePostFix)))
                        .OrderBy(c => c.SortIndex);
        }

        #endregion

        #region Properties

        public List<String> UsingNamespaces
        {
            get
            {
                List<String> usingNamespaces = new List<string>();
                foreach (Script f in this)
                {
                    int startindex = 0;
                    while (startindex >= 0)
                    {
                        int teststartindex = f.Sourcecode.IndexOf("/// using ", startindex);
                        if (teststartindex > 0)
                            startindex = AddNamespace(usingNamespaces, f.Sourcecode, teststartindex);
                        else
                        {
                            teststartindex = f.Sourcecode.IndexOf("using ", startindex);
                            if (teststartindex > 0)
                                startindex = AddNamespace(usingNamespaces, f.Sourcecode, teststartindex);
                            else
                                break;
                        }
                    }
                }
                return usingNamespaces;
            }
        }

        private int AddNamespace(List<String> usingNamespaces, string source, int startIndex)
        {
            int to = source.IndexOf(";", startIndex);
            int from = startIndex + 10;
            int length = to - from;
            if (length > 1)
            {
                string sNamespace = source.Substring(from, length);
                if (!usingNamespaces.Contains(sNamespace))
                    usingNamespaces.Add(sNamespace);
            }
            return to;
        }

        public List<String> Assemblies
        {
            get
            {
                List<String> assemblies = new List<string>();
                foreach (Script f in this)
                {
                    int startindex = 0;
                    while (startindex >= 0)
                    {
                        startindex = f.Sourcecode.IndexOf("/// refassembly ", startindex);
                        if (startindex > 0)
                        {
                            int to = f.Sourcecode.IndexOf(";", startindex);
                            int from = startindex + 16;
                            int length = to - from;
                            if (length > 1)
                            {
                                string assName = f.Sourcecode.Substring(from, length);
                                if (!assemblies.Contains(assName))
                                    assemblies.Add(assName);
                            }
                            startindex = to;
                        }
                    }
                }
                return assemblies;
            }
        }

        /// <summary>
        /// Provides an indexer to override the returned object type to a function/>
        /// </summary>
        public Script this[string functionName]
        {
            get
            {
                foreach (Script s in this)
                {
                    if (s.ACMethodName == functionName)
                    {
                        return s;
                    }
                }
                return null;
            }
        }


        #endregion

    }
}
