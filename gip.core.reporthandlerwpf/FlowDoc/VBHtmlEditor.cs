// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    /// <summary>
    /// Simple HTML template editor used by the report editor when a Scryber template is loaded.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBHtmlEditor'}de{'VBHtmlEditor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBHtmlEditor : gip.core.layoutengine.VBXMLEditor
    {
    }
}