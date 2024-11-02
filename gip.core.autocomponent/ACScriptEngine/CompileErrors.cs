// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>A collection class containing the objects generated during a failed <see cref="ScriptEngine" />
    /// compilation.</summary>
    public class CompileErrors : CollectionBase
	{
        #region Public Methods

        /// <summary>Adds a new object to the collection.</summary>
        /// <param name="err"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        public int Add(Msg err)
		{
			return base.InnerList.Add(err);
		}

		#endregion

		#region Properties

		/// <summary>
		/// The indexor used to access elements of the <see cref="CompileErrors"/> collection.
		/// </summary>
		public Msg this[int index]
		{
			get { return (Msg)base.InnerList[index]; }
		}

		#endregion
	}
}
