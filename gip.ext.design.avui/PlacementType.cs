// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.avui
{
    /// <summary>
    /// Describes how a placement is done.
    /// </summary>
    public sealed class PlacementType
    {
        /// <summary>
        /// Placement is done by Moving a inner Point (for Example on Path, Line, ...)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlacementType MovePoint = Register("MovePoint");

        /// <summary>
        /// Placement is done by resizing an element in a drag'n'drop operation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlacementType Resize = Register("Resize");

        /// <summary>
        /// Placement is done by moving an element in a drag'n'drop operation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlacementType Move = Register("Move");

        /// <summary>
        /// Placement is done by moving an element for Example via Keyboard!
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlacementType MoveAndIgnoreOtherContainers = Register("MoveAndIgnoreOtherContainers");

        /// <summary>
        /// Adding an element to a specified position in the container.
        /// AddItem is used when dragging a toolbox item to the design surface.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlacementType AddItem = Register("AddItem");

        /// <summary>
        /// Not a "real" placement, but deleting the element.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlacementType Delete = Register("Delete");

        /// <summary>
        /// Placement is done by moving an element in a drag'n'drop operation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlacementType MoveDependentItems = Register("MoveDependentItems");

        /// <summary>
        /// Inserting from Cliboard
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlacementType PasteItem = Register("PasteItem");

        /// <summary>
        /// Inserting via DragDropFromOutside
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly PlacementType DragDropFromOutside = Register("DragDropFromOutside");

        readonly string name;

        private PlacementType(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Creates a new unique PlacementKind.
        /// </summary>
        /// <param name="name">The name to return from a ToString() call.
        /// Note that two PlacementTypes with the same name are NOT equal!</param>
        public static PlacementType Register(string name)
        {
            return new PlacementType(name);
        }

        /// <summary>
        /// Gets the name used to register this PlacementType.
        /// </summary>
        public override string ToString()
        {
            return name;
        }
    }
}
