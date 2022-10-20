using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace gip.iplus.startup
{
	/// <summary>
	/// A collection class containing <see cref="Function"/> objects.
	/// </summary>
    public class ScriptList : List<Script> // DictionaryBase
    {
        #region Private Members

        private ScriptEngine myEngine = null;

        #endregion

        #region Constructors

        internal ScriptList(ScriptEngine engine)
        {
            myEngine = engine;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Used to add a new <see cref="Function"/> object to the <see cref="ScriptEngine"/>.
        /// </summary>
        /// <param name="function"></param>
        public void AddScript(Script function)
        {
            function.myEngine = myEngine;
            function.SortIndex = Count + 1;
            base.Add(function);
        }

        /// <summary>
        /// Used to remove a <see cref="Function"/> object from the <see cref="ScriptEngine"/>.
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
        /// Returns a string representation of all of the <see cref="Function"/> objects in the collection.
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
        /// <param name="functionName">The name of a <see cref="Function"/> object.</param>
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
                        startindex = f.Sourcecode.IndexOf("/// using ", startindex);
                        if (startindex > 0)
                        {
                            int to = f.Sourcecode.IndexOf(";", startindex);
                            int from = startindex + 10;
                            int length = to - from;
                            if (length > 1)
                                usingNamespaces.Add(f.Sourcecode.Substring(from, length));
                            startindex = to;
                        }
                    }
                }
                return usingNamespaces;
            }
        }

        public List<String> Assemblies
        {
            get
            {
                List<String> usingNamespaces = new List<string>();
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
                                usingNamespaces.Add(f.Sourcecode.Substring(from, length));
                            startindex = to;
                        }
                    }
                }
                return usingNamespaces;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Provides an indexer to override the returned object type to a <see cref="Function"/>
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
