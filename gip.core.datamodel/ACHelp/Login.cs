// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using Newtonsoft.Json;

namespace gip.core.datamodel
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Login'}de{'Login'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class Login
    {
        [JsonProperty]
        [ACPropertyInfo(9999, "Email", "en{'Email'}de{'Email'}")]
        public string Email { get; set; }

        [JsonProperty]
        [ACPropertyInfo(9999, "Password", "en{'Password'}de{'Kennwort'}")]
        public string Password { get; set; }

        [ACPropertyInfo(9999, "RememberLogin", "en{'Remeber login'}de{'Login speichern'}")]
        public bool RememberLogin { get; set; }
    }
}
