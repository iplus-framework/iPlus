// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Runtime.Serialization;

namespace gip.ext.design.avui
{
	/// <summary>
	/// Exception class used for designer failures.
	/// </summary>
	[Serializable]
	public class DesignerException : Exception
	{
		/// <summary>
		/// Create a new DesignerException instance.
		/// </summary>
		public DesignerException()
		{
		}
		
		/// <summary>
		/// Create a new DesignerException instance.
		/// </summary>
		public DesignerException(string message) : base(message)
		{
		}
		
		/// <summary>
		/// Create a new DesignerException instance.
		/// </summary>
		public DesignerException(string message, Exception innerException) : base(message, innerException)
		{
		}
		
		/// <summary>
		/// Create a new DesignerException instance.
		/// </summary>
		//protected DesignerException(SerializationInfo info, StreamingContext context)
		//	: base(info, context)
		//{
		//}
	}
}
