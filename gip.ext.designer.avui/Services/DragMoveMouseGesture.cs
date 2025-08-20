// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Services
{
	/// <summary>
	/// Mouse gesture for moving elements inside a container or between containers.
	/// Belongs to the PointerTool.
	/// </summary>
    [CLSCompliant(false)]
    public sealed class DragMoveMouseGesture : ClickOrDragMouseGesture
	{
		bool isDoubleClick;
		MoveLogic moveLogic;
		
		public DragMoveMouseGesture(DesignItem clickedOn, bool isDoubleClick)
		{
			Debug.Assert(clickedOn != null);
			
			this.isDoubleClick = isDoubleClick;
			this.positionRelativeTo = clickedOn.Services.DesignPanel;

			moveLogic = new MoveLogic(clickedOn);
		}
		
		protected override void OnDragStarted(MouseEventArgs e)
		{
			moveLogic.Start(startPoint);
		}
		
		protected override void OnMouseMove(object sender, MouseEventArgs e)
		{
			base.OnMouseMove(sender, e); // call OnDragStarted if min. drag distace is reached
			moveLogic.Move(e.GetPosition(positionRelativeTo));
		}
		
		protected override void OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (!_HasDragStarted && isDoubleClick) {
				// user made a double-click
				Debug.Assert(moveLogic.Operation == null);
				moveLogic.HandleDoubleClick();
			}
			moveLogic.Stop();
			Stop();
		}
		
		protected override void OnStopped()
		{
			moveLogic.Cancel();
		}
	}
}
