// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;

namespace gip.ext.design.avui
{
	/// <summary>
	/// Stores information about the placement of an individual item during a <see cref="PlacementOperation"/>.
	/// </summary>
	public sealed class PlacementInformation
	{
		/// <summary>
		/// The designer rounds bounds to this number of digits to avoid floating point errors.
		/// Value: 8
		/// </summary>
		public const int BoundsPrecision = 8;
		
		Rect originalBounds, bounds;
		readonly DesignItem item;
		readonly PlacementOperation operation;
		
		internal PlacementInformation(DesignItem item, PlacementOperation operation)
		{
			this.item = item;
			this.operation = operation;
		}
		
		/// <summary>
		/// The item being placed.
		/// </summary>
		public DesignItem Item {
			get { return item; }
		}
		
		/// <summary>
		/// The <see cref="PlacementOperation"/> to which this PlacementInformation belongs.
		/// </summary>
		public PlacementOperation Operation {
			get { return operation; }
		}
		
		/// <summary>
		/// Gets/sets the original bounds.
		/// </summary>
		public Rect OriginalBounds {
			get { return originalBounds; }
			set { originalBounds = value; }
		}
		
		/// <summary>
		/// Gets/sets the current bounds of the item.
		/// </summary>
		public Rect Bounds {
			get { return bounds; }
			set { bounds = value; }
		}

		/// <summary>
		/// Gets/sets the alignment of the resize thumb used to start the operation.
		/// </summary>
		public PlacementAlignment ResizeThumbAlignment { get; set; }
		
		/// <inheritdoc/>
		public override string ToString()
		{
			return "[PlacementInformation OriginalBounds=" + originalBounds + " Bounds=" + bounds + " Item=" + item + "]";
		}
	}
}
