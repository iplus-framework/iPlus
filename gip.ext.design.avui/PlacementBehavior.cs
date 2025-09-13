// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using Avalonia;

namespace gip.ext.design.avui
{
    /// <summary>
    /// iplus Extension
    /// </summary>
    public interface IPlacementChildGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        object CreateDefaultNewChildInstance();
    }

    /// <summary>
    /// iplus Extension
    /// </summary>
    public interface IPlacementBehaviorBase
    {
        /// <summary>
        /// Starts placement mode for this container.
        /// </summary>
        void BeginPlacement(PlacementOperation operation);

        /// <summary>
        /// Updates the placement of the element specified in the placement operation.
        /// </summary>
        void SetPosition(PlacementInformation info);

        /// <summary>
        /// Ends placement mode for this container.
        /// </summary>
        void EndPlacement(PlacementOperation operation);
    }

	/// <summary>
	/// Behavior interface implemented by container elements to support resizing
	/// child elements.
	/// </summary>
    public interface IPlacementBehavior : IPlacementBehaviorBase
	{
		/// <summary>
		/// Gets if the child element can be resized.
		/// </summary>
		bool CanPlace(ICollection<DesignItem> childItems, PlacementType type, PlacementAlignment position);
				
		/// <summary>
		/// Gets the original position of the child item.
		/// </summary>
		Rect GetPosition(PlacementOperation operation, DesignItem child, bool verifyAndCorrectPosition);

		/// <summary>
		/// Is called before SetPosition is called for the placed items.
		/// This may update the bounds on the placement operation (e.g. when snaplines are enabled).
		/// </summary>
		void BeforeSetPosition(PlacementOperation operation);
				
		/// <summary>
		/// Gets if leaving this container is allowed for the specified operation.
		/// </summary>
		bool CanLeaveContainer(PlacementOperation operation);
		
		/// <summary>
		/// Remove the placed children from this container.
		/// </summary>
		void LeaveContainer(PlacementOperation operation);
		
		/// <summary>
		/// Gets if entering this container is allowed for the specified operation.
		/// </summary>
		bool CanEnterContainer(PlacementOperation operation, bool shouldAlwaysEnter);
		
		/// <summary>
		/// Let the placed children enter this container.
		/// </summary>
		void EnterContainer(PlacementOperation operation);

        /// <summary>
        /// Place Point.
        /// </summary>
        Point PlacePoint(Point point);

        /// <summary>
        /// Können die selektierten Steuerelemente um den Vektor verschoben werden
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        bool CanMoveVector(PlacementOperation operation, Vector vector);
	}

    /// <summary>
    /// iPlus-Extension
    /// </summary>
    public interface IDependentDrawingsBehavior : IPlacementBehaviorBase
    {
        /// <summary>
        /// Ends placement mode for this container.
        /// </summary>
        void PlacementAborted(PlacementOperation operation);
    }
	
	/// <summary>
	/// Behavior interface for root elements (elements where item.Parent is null).
	/// Is used instead of <see cref="IPlacementBehavior"/> to support resizing the root element.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces",
	                                                 Justification = "The root component might have both a PlacementBehavior and a RootPlacementBehavior, which must be distinguished by DesignItem.GetBehavior")]
	public interface IRootPlacementBehavior : IPlacementBehavior
	{
	}
}
