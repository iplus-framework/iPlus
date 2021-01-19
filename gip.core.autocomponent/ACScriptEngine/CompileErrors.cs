using System;
using System.Collections;
using gip.core.datamodel;

namespace gip.core.autocomponent
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
