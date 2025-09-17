// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.designer.avui.Services
{
	/// <summary>
	/// Contains a set of options regarding the default designer components.
	/// </summary>
	public sealed class OptionService
	{
		/// <summary>
		/// Gets/Sets whether the design surface should be grayed out while dragging/selection.
		/// </summary>
		public bool GrayOutDesignSurfaceExceptParentContainerWhenDragging = true;

        /// <summary>
        /// Gets/Sets if the Values should be rounded when using Snapline Placement.
        /// </summary>
        public bool SnaplinePlacementRoundValues = false;
    }
}
