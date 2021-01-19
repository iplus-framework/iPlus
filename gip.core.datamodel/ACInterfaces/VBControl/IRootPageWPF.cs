using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace gip.core.datamodel
{
    /// <summary>
    /// Enum ControlSelectionState
    /// </summary>
    public enum ControlSelectionState : short
    {
        /// <summary>
        /// The off
        /// </summary>
        Off = 0,
        /// <summary>
        /// The frame search
        /// </summary>
        FrameSearch = 1,
        /// <summary>
        /// The frame selected
        /// </summary>
        FrameSelected = 2,
    }

    public enum FocusBSOResult : short
    {
        NotFocusable = 0,
        AlreadyFocused = 1,
        SelectionSwitched = 2,
    }

    /// <summary>
    /// Class WPFControlSelectionEventArgs
    /// </summary>
	public class WPFControlSelectionEventArgs : EventArgs
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="WPFControlSelectionEventArgs"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public WPFControlSelectionEventArgs(ControlSelectionState state)
        {
            _ControlSelectionState = state;
        }

        /// <summary>
        /// The _ control selection state
        /// </summary>
        private ControlSelectionState _ControlSelectionState;
        /// <summary>
        /// Gets the state of the control selection.
        /// </summary>
        /// <value>The state of the control selection.</value>
        public ControlSelectionState ControlSelectionState
        {
            get
            {
                return _ControlSelectionState;
            }
        }
	}

    /// <summary>
    /// Interface IRootPageWPF
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IRootPageWPF'}de{'IRootPageWPF'}", Global.ACKinds.TACInterface)]
    public interface IRootPageWPF : IACObject
    {
        /// <summary>
        /// Gets or sets the current AC component.
        /// </summary>
        /// <value>The current AC component.</value>
        IACComponent CurrentACComponent { get; set; }

        /// <summary>
        /// Starts the businessobject by AC command.
        /// </summary>
        /// <param name="acCommand">The ac command.</param>
        void StartBusinessobjectByACCommand(ACCommand acCommand);

        /// <summary>
        /// Starts the businessobject.
        /// </summary>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="parameterList">The parameter list.</param>
        /// <param name="acCaption">The ac caption.</param>
        void StartBusinessobject(string acUrl, ACValueList parameterList, string acCaption = "");

        /// <summary>
        /// Occurs when [VB docking manager freezing event].
        /// </summary>
        WPFControlSelectionEventArgs VBDockingManagerFreezing { get; set; }
        /// <summary>
        /// Dockings the manager freezed.
        /// </summary>
        /// <param name="dockingManager">The docking manager.</param>
        void DockingManagerFreezed(object dockingManager);

        /// <summary>
        /// Occurs when [VB design editing event].
        /// </summary>
        WPFControlSelectionEventArgs VBDesignEditing { get; set; }
        /// <summary>
        /// VBs the design editing activated.
        /// </summary>
        /// <param name="vbDesign">The vb design.</param>
        void VBDesignEditingActivated(object vbDesign);

        /// <summary>
        /// Gets the WPF application.
        /// </summary>
        /// <value>The WPF application.</value>
        object WPFApplication { get; }

        /// <summary>
        /// Shows the MSG box.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="msgButton">The MSG button.</param>
        /// <returns>Global.MsgResult.</returns>
        Global.MsgResult ShowMsgBox(Msg msg, eMsgButton msgButton);

        /// <summary>
        /// Stores the settings WND pos.
        /// </summary>
        /// <param name="settingsVBDesignWndPos">The settings VB design WND pos.</param>
        void StoreSettingsWndPos(object settingsVBDesignWndPos);
        /// <summary>
        /// Res the store settings WND pos.
        /// </summary>
        /// <param name="ACIdentifier">The AC identifier.</param>
        /// <returns>System.Object.</returns>
        object ReStoreSettingsWndPos(string ACIdentifier);

        /// <summary>
        /// Opens the file dialog.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="initialDirectory">The initial directory.</param>
        /// <param name="restoreDirectory">if set to <c>true</c> [restore directory].</param>
        /// <returns>System.String.</returns>
        string OpenFileDialog(string filter, string initialDirectory = null, bool restoreDirectory = true);

        /// <summary>
        /// Saves the file dialog.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="initialDirectory">The initial directory.</param>
        /// <param name="restoreDirectory">if set to <c>true</c> [restore directory].</param>
        /// <returns>System.String.</returns>
        string SaveFileDialog(string filter, string initialDirectory = null, bool restoreDirectory = true);

        /// <summary>
        /// Closes the Window
        /// </summary>
        void CloseWindowFromThread();

        double Zoom { get; }

        bool InFullscreen { get; }

        FocusBSOResult FocusBSO(IACBSO bso);
    }
}
