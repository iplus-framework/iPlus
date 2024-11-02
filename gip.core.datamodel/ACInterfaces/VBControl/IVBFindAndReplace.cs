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
// <copyright file="IVBFindAndReplace.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Enum FindAndReplaceResult
    /// </summary>
    public enum FindAndReplaceResult : short
    {
        /// <summary>
        /// The word not found
        /// </summary>
        WordNotFound = 0,
        /// <summary>
        /// The word found
        /// </summary>
        WordFound = 1,
        /// <summary>
        /// The replaced_ next word not found
        /// </summary>
        Replaced_NextWordNotFound = 2,
        /// <summary>
        /// The replaced_ next word found
        /// </summary>
        Replaced_NextWordFound = 3,
        /// <summary>
        /// The selection required
        /// </summary>
        SelectionRequired = 4,
        /// <summary>
        /// The replaced all
        /// </summary>
        ReplacedAll = 5,
    }

    /// <summary>
    /// Inteface which connects the Find-and-Replace-dialog with a FindAndReplace-Handler
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IVBFindAndReplace'}de{'IVBFindAndReplace'}", Global.ACKinds.TACInterface)]
    public interface IVBFindAndReplace
    {
        /// <summary>
        /// To reset the Postion aof the current Search
        /// </summary>
        void NewStartOfFind();

        /// <summary>
        /// Called when User Clicks Next-Button
        /// </summary>
        /// <returns>FindAndReplaceResult</returns>
        FindAndReplaceResult FindNext();

        /// <summary>
        /// Called when User Clicks Replace-Button
        /// </summary>
        /// <returns>FindAndReplaceResult</returns>
        FindAndReplaceResult Replace();

        /// <summary>
        /// Called when User Clicks Replace-All-Button
        /// </summary>
        /// <returns>FindAndReplaceResult</returns>
        FindAndReplaceResult ReplaceAll();

        /// <summary>
        /// Gets the text in selection.
        /// </summary>
        /// <returns>System.String.</returns>
        string GetTextInSelection();

        /// <summary>
        /// Value will be set, before FindNext oder Replace-Method will be called
        /// </summary>
        /// <value>The word to find.</value>
        string wordToFind { get; set; }

        /// <summary>
        /// Value will be set, before FindNext oder Replace-Method will be called
        /// </summary>
        /// <value>The word replace with.</value>
        string wordReplaceWith { get; set; }

        /// <summary>
        /// Value will be set, before FindNext oder Replace-Method will be called
        /// </summary>
        /// <value><c>true</c> if [option case sensitive]; otherwise, <c>false</c>.</value>
        bool OptionCaseSensitive { get; set; }

        /// <summary>
        /// Value will be set, before FindNext oder Replace-Method will be called
        /// </summary>
        /// <value><c>true</c> if [option find complete word]; otherwise, <c>false</c>.</value>
        bool OptionFindCompleteWord { get; set; }

        /// <summary>
        /// Value will be set, before FindNext oder Replace-Method will be called
        /// </summary>
        /// <value><c>true</c> if [option is regular expr]; otherwise, <c>false</c>.</value>
        bool OptionIsRegularExpr { get; set; }

        /// <summary>
        /// Value will be set, before FindNext oder Replace-Method will be called
        /// </summary>
        /// <value><c>true</c> if [option is wildcard]; otherwise, <c>false</c>.</value>
        bool OptionIsWildcard { get; set; }
    }
}
