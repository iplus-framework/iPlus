// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using gip.ext.design.avui.Extensions;

namespace gip.ext.design.avui.Adorners
{
	// Some classes that derive from AdornerProvider to specify a certain ExtensionServer.
	
	/// <summary>
	/// An adorner extension that is attached permanently.
	/// </summary>
	[ExtensionServer(typeof(DefaultExtensionServer.PermanentWithDesignPanel))]
	public abstract class PermanentAdornerProvider : AdornerProvider
	{
		
	}
	
	/// <summary>
	/// An adorner extension that is attached to selected components.
	/// </summary>
	[ExtensionServer(typeof(SelectionExtensionServer))]
	public abstract class SelectionAdornerProvider : AdornerProvider
	{
		
	}
	
	/// <summary>
	/// An adorner extension that is attached to the primary selection.
	/// </summary>
	[ExtensionServer(typeof(PrimarySelectionExtensionServer))]
	public abstract class PrimarySelectionAdornerProvider : AdornerProvider
	{
		
	}
	
	/// <summary>
	/// An adorner extension that is attached to the secondary selection.
	/// </summary>
	[ExtensionServer(typeof(SecondarySelectionExtensionServer))]
	public abstract class SecondarySelectionAdornerProvider : AdornerProvider
	{
		
	}
}
