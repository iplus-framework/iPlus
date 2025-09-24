// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia.Controls;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Draws border, background or both around another element
    /// </summary>
    /// <summary xml:lang="de">
    /// Zeichnet Rahmen, Hintergrund oder beides um ein anderes Element herum
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBBorder'}de{'VBBorder'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBBorder : Border
    {
    }
}
