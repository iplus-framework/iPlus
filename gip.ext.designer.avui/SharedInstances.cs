// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using gip.ext.design.avui;

namespace gip.ext.designer.avui
{
	static class SharedInstances
	{
		internal static readonly object BoxedTrue = true;
		internal static readonly object BoxedFalse = false;
		internal static readonly object[] EmptyObjectArray = new object[0];
		internal static readonly DesignItem[] EmptyDesignItemArray = new DesignItem[0];
		
		internal static object Box(bool value)
		{
			return value ? BoxedTrue : BoxedFalse;
		}
	}
}
