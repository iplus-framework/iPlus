// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Runtime.Serialization;

namespace gip.ext.xamldom
{
	/// <summary>
	/// Exception class used for xaml loading failures.
	/// </summary>
	[Serializable]
	public class XamlLoadException : Exception
	{
		/// <summary>
		/// Create a new XamlLoadException instance.
		/// </summary>
		public XamlLoadException()
		{
		}
		
		/// <summary>
		/// Create a new XamlLoadException instance.
		/// </summary>
		public XamlLoadException(string message) : base(message)
		{
		}
		
		/// <summary>
		/// Create a new XamlLoadException instance.
		/// </summary>
		public XamlLoadException(string message, Exception innerException) : base(message, innerException)
		{
		}
		
		/// <summary>
		/// Create a new XamlLoadException instance.
		/// </summary>
		protected XamlLoadException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
