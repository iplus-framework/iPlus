// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.avui
{
	/// <summary>
	/// Base class for change groups.
	/// </summary>
	public abstract class ChangeGroup : IDisposable
	{
		string title;
		
		/// <summary>
		/// Gets/Sets the title of the change group.
		/// </summary>
		public string Title {
			get { return title; }
			set { title = value; }
		}
		
		/// <summary>
		/// Commits the change group.
		/// </summary>
		public abstract void Commit();
		
		/// <summary>
		/// Aborts the change group.
		/// </summary>
		public abstract void Abort();
		
		/// <summary>
		/// Is called when the change group is disposed. Should Abort the change group if it is not already committed.
		/// </summary>
		protected abstract void Dispose();
		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		void IDisposable.Dispose()
		{
			Dispose();
		}
	}
}
