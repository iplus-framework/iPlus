// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACMenuBuilder.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for classes that provide Methods that can be invoked from GUI by a contextmenu.
    /// IACMenuBuilder returns a list of ACMenuItem's (that is a derivation of a ACCommand).
    /// A ACMenuItem contains a ACUrl of the Method that should be invoked.
    /// GetMenu() is called from gip.core.autocomponent.MenuManager.GetMenu()-Method. 
    /// The MenuManager calls GetMenu() at all instances that implement IACMenuBuilder and which have a relationship inside the MVVM-Pattern. 
    /// All ACMenuItemList's are afterwards merged together to one menu that is displayed as a contextmenu on the GUI.
    /// </summary>
    public interface IACMenuBuilder
    {
        /// <summary>
        /// A ACMenuItem contains a ACUrl of the Method that should be invoked.
        /// GetMenu() is called from gip.core.autocomponent.MenuManager.GetMenu()-Method. 
        /// The MenuManager calls GetMenu() at all instances that implement IACMenuBuilder and which have a relationship inside the MVVM-Pattern. 
        /// All ACMenuItemList's are afterwards merged together to one menu that is displayed as a contextmenu on the GUI.
        /// </summary>
        /// <param name="vbContent">VBContent of the WPF-Control where the user has requested the menu first</param>
        /// <param name="vbControl">Type.FullName of the WPF-Control where the user has requested the menu first</param>
        /// <returns>List of menu entries</returns>
        ACMenuItemList GetMenu(string vbContent, string vbControl);
    }

    /// <summary>
    /// Interface for WPF-Controls.
    /// WPF-Controls are nested in the logical tree. Therefore when a user klicks on a control maybe a a menu of the the parent's control should also be displayed.
    /// So every Control, that implements this interface should call AppendMenu inside it's implementation of the GetMenu()-Method.
    /// Inside the AppendMenu-Method the method VBLogicalTreeHelper.AppendMenu() should be invoked that interates through the logical tree an ivokes AppendMenu() at all 
    /// WPF-instances that implements IACMenuBuilderWPFTree.
    /// </summary>
    public interface IACMenuBuilderWPFTree : IACMenuBuilder
    {
        /// <summary>
        /// WPF-Controls are nested in the logical tree. Therefore when a user klicks on a control maybe a a menu of the the parent's control should also be displayed.
        /// So every Control, that implements this interface should call AppendMenu inside it's implementation of the GetMenu()-Method.
        /// Inside the AppendMenu-Method the method VBLogicalTreeHelper.AppendMenu() should be invoked that interates through the logical tree an ivokes AppendMenu() at all 
        /// WPF-instances that implements IACMenuBuilderWPFTree.
        /// </summary>
        /// <param name="vbContent">VBContent of the WPF-Control where the user has requested the menu first</param>
        /// <param name="vbControl">Type.FullName of the WPF-Control where the user has requested the menu first</param>
        /// <param name="acMenuItemList">Reference to the ACMenuItemList where additional Menu-Entries should be appended</param>
        void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList);
    }
}
