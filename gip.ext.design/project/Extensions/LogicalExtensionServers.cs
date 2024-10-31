// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.Extensions
{
	/// <summary>
	/// Combines two extension servers using a logical OR.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
	public sealed class LogicalOrExtensionServer<A, B> : DefaultExtensionServer
		where A : ExtensionServer
		where B : ExtensionServer
	{
		ExtensionServer _a;
		ExtensionServer _b;
		
		/// <summary/>
		protected override void OnInitialized()
		{
			base.OnInitialized();
			_a = Context.Services.ExtensionManager.GetExtensionServer(new ExtensionServerAttribute(typeof(A)));
			_b = Context.Services.ExtensionManager.GetExtensionServer(new ExtensionServerAttribute(typeof(B)));
			_a.ShouldApplyExtensionsInvalidated += OnShouldApplyExtensionsInvalidated;
			_b.ShouldApplyExtensionsInvalidated += OnShouldApplyExtensionsInvalidated;
		}
		
		void OnShouldApplyExtensionsInvalidated(object sender, DesignItemCollectionEventArgs e)
		{
			ReapplyExtensions(e.Items);
		}
		
		/// <summary/>
		public override bool ShouldApplyExtensions(DesignItem extendedItem)
		{
			return _a.ShouldApplyExtensions(extendedItem) || _b.ShouldApplyExtensions(extendedItem);
		}
	}
	
	/// <summary>
	/// Combines two extension servers using a logical AND.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
	public sealed class LogicalAndExtensionServer<A, B> : DefaultExtensionServer
		where A : ExtensionServer
		where B : ExtensionServer
	{
		ExtensionServer _a;
		ExtensionServer _b;
		
		/// <summary/>
		protected override void OnInitialized()
		{
			base.OnInitialized();
			_a = Context.Services.ExtensionManager.GetExtensionServer(new ExtensionServerAttribute(typeof(A)));
			_b = Context.Services.ExtensionManager.GetExtensionServer(new ExtensionServerAttribute(typeof(B)));
			_a.ShouldApplyExtensionsInvalidated += OnShouldApplyExtensionsInvalidated;
			_b.ShouldApplyExtensionsInvalidated += OnShouldApplyExtensionsInvalidated;
		}
		
		void OnShouldApplyExtensionsInvalidated(object sender, DesignItemCollectionEventArgs e)
		{
			ReapplyExtensions(e.Items);
		}
		
		/// <summary/>
		public override bool ShouldApplyExtensions(DesignItem extendedItem)
		{
			return _a.ShouldApplyExtensions(extendedItem) && _b.ShouldApplyExtensions(extendedItem);
		}
	}
}
