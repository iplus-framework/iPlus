// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.datamodel
{
    /// <summary>
    /// This is the ultimate base interface for implementing classes in the iPlus-Framework. It's comparable to the .net-Type System.Object. 
    /// This interface helps to present objects on a GUI with its description and to indentify them inside a generic list through its uniqe indentifer (ACIdentifier)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACObjectBase'}de{'IACObjectBase'}", Global.ACKinds.TACInterface)]
    public interface IACObjectBase
    {

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(1, "", "en{'Identifier'}de{'Identifizierer'}")]
        string ACIdentifier { get; }



        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(2, "", "en{'Description'}de{'Beschreibung'}")]
        string ACCaption { get; }

    }
}
