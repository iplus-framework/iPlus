// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-14-2013
// ***********************************************************************
// <copyright file="IACComponentDesignManager.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface IACComponentDesignManagerHost
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACComponentDesignManagerHost'}de{'IACComponentDesignManagerHost'}", Global.ACKinds.TACInterface)]
    public interface IACComponentDesignManagerHost
    {
        /// <summary>
        /// Gets the current design context.
        /// </summary>
        /// <value>The current design context.</value>
        IACObject CurrentDesignContext { get; }
    }

    /// <summary>
    /// Interface IACComponentDesignManager
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACComponentDesignManager'}de{'IACComponentDesignManager'}", Global.ACKinds.TACInterface)]
    public interface IACComponentDesignManager : IACBSO
    {
        #region Presentation objects

        /// <summary>
        /// Design that is currently presented in the designer (Its a class that implements IACObjectDesign: ACClassDesign, ACClassMethod, Partslist, PWOfflineNode, IACComponentPWNode etc.)
        /// </summary>
        /// <value>Reference to a instance of IACObjectDesign that propvides the XAML-Code that should be presented.</value>
        IACObjectDesign CurrentDesign { get; set; }


        /// <summary>The design context is the root datacontext for the XAML-Code where Bindings are related to. The Path in the VBContent is relative to this design context.</summary>
        /// <value>Datacontext for XAML-Code</value>
        IACObject CurrentDesignContext { get; }


        /// <summary>
        /// Reference to the WPF-Control, that presents the XAML-Code of the CurrentDesign-Property
        /// </summary>
        /// <value>Reference to a WPF-Control that implements IVBContent (System.Windows.Controls.ContentControl)</value>
        IVBContent VBDesignControl { get; set; }


        /// <summary>
        /// When the XAML-Code is loaded, the logical tree is assigned to the Content-Property of the VBDesignControl.
        /// When the user selects a child from this logical tree, which is a IVBContent, then this property contains a reference to it.
        /// </summary>
        /// <value>The selected WPF-Control that implements IVBContent</value>
        IVBContent SelectedVBControl { get; set; }

        #endregion


        #region Designer, States and Appearance

        /// <summary>
        /// Gets a value indicating whether this instance is design mode.
        /// </summary>
        /// <value><c>true</c> if this instance is design mode; otherwise, <c>false</c>.</value>
        bool IsDesignMode { get; }


        /// <summary>
        /// Switches the this designer to design mode and the Designer-Tool (WPF-Control) appears on the gui.
        /// </summary>
        /// <param name="dockingManagerName">Name of the parent docking manager.</param>
        void ShowDesignManager(string dockingManagerName = "");


        /// <summary>
        /// Switches the designer off and the Designer-Tool (WPF-Control) disappears on the gui.
        /// </summary>
        void HideDesignManager();


        /// <summary>
        /// The Designer-Control that enables the graphically designing of the gui.
        /// (Manipulates also the XAML-Code)
        /// </summary>
        /// <value>Reference to the Designer-Tool (WPF-Control)</value>
        IACInteractiveObject VBDesignEditor { get; }


        /// <summary>
        /// Controls if the "XAML-Editor"-Tab in the Designer-Control should be visible
        /// </summary>
        /// <value><c>true</c> if XAML-Editor is visible; otherwise, <c>false</c>.</value>
        bool ShowXMLEditor { get; }


        /// <summary>
        /// The ToolService is the current graphical tool that the designer works with (Pointer, Line, Rectangle, Circle....)
        /// </summary>
        /// <value>Refence to the current tool</value>
        object ToolService { get; }


        /// <summary>
        /// Informs this design manager, that the Designer-Control (VBDesignEditor) was laoded on the gui
        /// </summary>
        /// <param name="designEditor">Reference to the Designer-Control (VBDesignEditor)</param>
        /// <param name="reverseToXaml">if set to <c>true</c> [reverse to xaml].</param>
        void OnDesignerLoaded(IVBContent designEditor, bool reverseToXaml);

        #endregion


        #region Tool Window

        /// <summary>
        /// Returns a reference to the tool-window (WPF-Control)
        /// </summary>
        /// <value>Reference to the tool-window</value>
        IACObject ToolWindow { get; }

        #endregion


        #region Property Window

        /// <summary>
        /// Returns a reference to a window (WPF-Control) that shows the Dependency-Properties of the selected WFP-Control in the designer (SelectedVBControl).
        /// These dependency-properties can also be manipulated.
        /// </summary>
        /// <value>Reference to the tool-window</value>
        IACObject PropertyWindow { get; }


        /// <summary>
        /// Controls if the Property-Window in the Designer-Control should be visible
        /// </summary>
        /// <value><c>true</c> if Property-Window is visible; otherwise, <c>false</c>.</value>
        bool PropertyWindowVisible { get; }


        /// <summary>
        /// Opens the property window if it's closed.
        /// </summary>
        /// <param name="dockingManagerName">Name of the parent docking manager.</param>
        void ShowPropertyWindow(string dockingManagerName = "");

        
        /// <summary>
        /// Closes the property window.
        /// </summary>
        void ClosePropertyWindow();

        #endregion


        #region Logical-Tree Window

        /// <summary>
        /// Returns a reference to a window (WPF-Control) that shows the logical tree of the current design.
        /// </summary>
        /// <value>Reference to the logical tree window</value>
        IACObject LogicalTreeWindow { get; }


        /// <summary>
        /// Controls if the logical tree window in the Designer-Control should be visible
        /// </summary>
        /// <value><c>true</c> if Property-Window is visible; otherwise, <c>false</c>.</value>
        bool LogicalTreeWindowVisible { get; }


        /// <summary>
        /// Opens the logical tree window if it's closed.
        /// </summary>
        /// <param name="dockingManagerName">Name of the parent docking manager.</param>
        void ShowLogicalTreeWindow(string dockingManagerName = "");


        /// <summary>
        /// Closes the logical tree window.
        /// </summary>
        void CloseLogicalTreeWindow();

        #endregion


        #region Adding and removing of visual elements
        /// <summary>
        /// Creates a Edge between two points
        /// </summary>
        /// <param name="sourceVBConnector">The source VB connector.</param>
        /// <param name="targetVBConnector">The target VB connector.</param>
        void CreateEdge(IVBConnector sourceVBConnector, IVBConnector targetVBConnector);

        /// <summary>
        /// Checks if a Edge can be created between two points
        /// </summary>
        /// <param name="sourceVBConnector">The source VB connector.</param>
        /// <param name="targetVBConnector">The target VB connector.</param>
        /// <returns><c>true</c> if is enabled; otherwise, <c>false</c>.</returns>
        bool IsEnabledCreateEdge(IVBConnector sourceVBConnector, IVBConnector targetVBConnector);

        /// <summary>
        /// Removes a WPF-Element from the design
        /// </summary>
        /// <param name="item">Item for delete.</param>
        /// <param name="isFromDesigner">If true, then call is invoked from this manager, else from gui</param>
        /// <returns><c>true</c> if is removed; otherwise, <c>false</c>.</returns>
        bool DeleteItem(object item, bool isFromDesigner=true);


        /// <summary>Asks this design manager if he can create edges</summary>
        /// <returns><c>true</c> if this instance can create edges; otherwise, <c>false</c>.</returns>
        bool CanManagerCreateEdges();

        #endregion


        #region Misc
        
        /// <summary>
        /// Builds the VBContent-String for the passed acUrl relatively to the CurrentDesignContext
        /// </summary>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="acObject">The ac object.</param>
        /// <returns>VBContent System.String</returns>
        string BuildVBContentFromACUrl(string acUrl, IACObject acObject);


        /// <summary>
        /// Gets the type for the passed acUrl
        /// </summary>
        /// <param name="acUrl">acUrl</param>
        /// <returns>IACType</returns>
        IACType GetTypeFromDBACUrl(string acUrl);
        
        #endregion

    }
}
