// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Services
{
	/// <summary>
	/// Mouse gesture for moving elements inside a container or between containers.
	/// Belongs to the PointerTool.
	/// </summary>
    public sealed class DragMoveMouseGesture : ClickOrDragMouseGesture
	{
		bool isDoubleClick;
        bool setSelectionIfNotMoving;
        MoveLogic moveLogic;
		
		public DragMoveMouseGesture(DesignItem clickedOn, bool isDoubleClick, bool setSelectionIfNotMoving = false)
		{
			Debug.Assert(clickedOn != null);
			
			this.isDoubleClick = isDoubleClick;
            this.setSelectionIfNotMoving = setSelectionIfNotMoving;
            this.positionRelativeTo = clickedOn.Services.DesignPanel;

            moveLogic = new MoveLogic(clickedOn);
		}
		
		protected override void OnDragStarted(PointerEventArgs e)
		{
			moveLogic.Start(startPoint);
		}
		
		protected override void OnMouseMove(object sender, PointerEventArgs e)
		{
			base.OnMouseMove(sender, e); // call OnDragStarted if min. drag distace is reached
			moveLogic.Move(e.GetPosition(positionRelativeTo as Visual));
		}
		
		protected override void OnMouseUp(object sender, PointerReleasedEventArgs e)
		{
			if (!_HasDragStarted) {
				if (isDoubleClick) {
					// user made a double-click
					Debug.Assert(moveLogic.Operation == null);
					moveLogic.HandleDoubleClick();
				} else if (setSelectionIfNotMoving) {
					services.Selection.SetSelectedComponents(new DesignItem[] { moveLogic.ClickedOn }, SelectionTypes.Auto);
				}
			}
			moveLogic.Stop();
			Stop(e);
		}
		
		protected override void OnStopped()
		{
			moveLogic.Cancel();
		}
	}
}
