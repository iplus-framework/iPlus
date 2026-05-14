// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    /// <summary>
    /// HTML preview browser for Scryber templates in the report editor.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBWebBrowser'}de{'VBWebBrowser'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBWebBrowser : gip.core.layoutengine.VBWebBrowser
    {
    }
}