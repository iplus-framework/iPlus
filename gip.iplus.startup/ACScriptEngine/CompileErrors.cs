// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections;

namespace gip.iplus.startup
{
	/// <summary>
	/// A collection class containing the <see cref="CompileError"/> objects generated during a failed <see cref="ScriptEngine"/>
	/// compilation.
	/// </summary>
	public class CompileErrors : CollectionBase
	{
		#region Public Methods

		/// <summary>
		/// Adds a new <see cref="CompileError"/> object to the collection.
		/// </summary>
		/// <param name="err"></param>
		/// <returns></returns>
		public int Add(string err)
		{
			return base.InnerList.Add(err);
		}

		#endregion

		#region Properties

		/// <summary>
		/// The indexor used to access elements of the <see cref="CompileErrors"/> collection.
		/// </summary>
		public string this[int index]
		{
			get { return (string)base.InnerList[index]; }
		}

		#endregion
	}
}
