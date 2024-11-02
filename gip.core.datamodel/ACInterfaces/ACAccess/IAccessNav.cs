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
// <copyright file="IAccess.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>
    /// Extends IAccess with navigation features
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IAccessNav'}de{'IAccessNav'}", Global.ACKinds.TACInterface)]
    public interface IAccessNav : IAccess
    {
        /// <summary>
        /// The "Selected"-Property points to a EF-Object in the NavObjectList-Collection that is highlighted in a Items-Control on the GUI.
        /// Normally the "Current"-Property and the "Selected"-Property should point to the same object.
        /// </summary>
        /// <value>The selected nav object.</value>
        [ACPropertyInfo(9999)]
        object SelectedNavObject { get; set; }


        /// <summary>
        /// The "Current"-Property points to a EF-Object in the NavObjectList-Collection that is displayed on a Form-View on the GUI.
        /// Normally the "Current"-Property and the "Selected"-Property should point to the same object.
        /// </summary>
        /// <value>The current nav object.</value>
        [ACPropertyInfo(9999)]
        object CurrentNavObject { get; set; }


        /// <summary>
        /// Index of the CurrentNavObject in the NavObjectList-Collection.
        /// </summary>
        /// <value>Index from 0. If NavObjectList is empty "-1" is returned.</value>
        [ACPropertyInfo(9999)]
        int NavRowCurrent { get; set; }


        /// <summary>Controls the automatic invocation of the ACSaveOrUndoChanges()-Method when the CurrentNavObject should be changed (Navigation)</summary>
        /// <value>
        ///   <c>true</c> if ACSaveOrUndoChanges() should be automatically invoked; otherwise, <c>false</c>.</value>
        bool AutoSaveOnNavigation { get; set; }


        /// <summary>
        /// Navigates to the first entry in the NavObjectList-Property.
        /// </summary>
        [ACMethodCommand("Navigation", "en{'First'}de{'Erster'}", (short)MISort.NavigateFirst)]
        void NavigateFirst();


        /// <summary>Can navigate to the first entry of the CurrentNavObject</summary>
        /// <returns>
        ///   <c>true</c> if a first entry exist; otherwise, <c>false</c>.</returns>
        bool IsEnabledNavigateFirst();


        /// <summary>
        /// Navigates to the previous entry of the CurrentNavObject
        /// </summary>
        [ACMethodCommand("Navigation", "en{'Previous'}de{'Vorheriger'}", (short)MISort.NavigatePrev)]
        void NavigatePrev();


        /// <summary>Can navigate to the previous entry of the CurrentNavObject</summary>
        /// <returns>
        ///   <c>true</c> if a previous entry exist; otherwise, <c>false</c>.</returns>
        bool IsEnabledNavigatePrev();


        /// <summary>
        /// Navigates to the next entry of the CurrentNavObject
        /// </summary>
        [ACMethodCommand("Navigation", "en{'Next'}de{'Nächster'}", (short)MISort.NavigateNext)]
        void NavigateNext();


        /// <summary>Can navigate the next entry of the CurrentNavObject</summary>
        /// <returns>
        ///   <c>true</c> if a next entry exist; otherwise, <c>false</c>.</returns>
        bool IsEnabledNavigateNext();


        /// <summary>
        /// Navigates to the last entry in the NavObjectList-Property.
        /// </summary>
        [ACMethodCommand("Navigation", "en{'Last'}de{'Letzter'}", (short)MISort.NavigateLast)]
        void NavigateLast();


        /// <summary>Can navigate to the last entry of the CurrentNavObject</summary>
        /// <returns>
        ///   <c>true</c> if a last entry exist; otherwise, <c>false</c>.</returns>
        bool IsEnabledNavigateLast();
    }
}
