// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.


using Avalonia.Input;

namespace gip.ext.design.avui.Extensions
{
	/// <summary>
	/// interface that can be implemented if a control is to be alerted of  KeyDown Event on DesignPanel
	/// </summary>
	public interface IKeyDown
	{
		/// <summary>
		/// Action to be performed on keydown on specific control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void KeyDownAction(object sender, KeyEventArgs e);

		/// <summary>
		/// if that control wants the default DesignPanel action to be suppressed, let this return false
		/// </summary>
		bool InvokeDefaultAction { get; }
	}

	/// <summary>
	/// interface that can be implemented if a control is to be alerted of  KeyUp Event on DesignPanel
	/// </summary>
	public interface IKeyUp
	{
		/// <summary>
		/// Action to be performed on keyup on specific control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void KeyUpAction(object sender, KeyEventArgs e);
	}

	
}
