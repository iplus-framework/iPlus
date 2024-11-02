// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using Newtonsoft.Json;
using System;

namespace gip.core.datamodel
{
    [JsonObject(MemberSerialization.OptIn)]
    public interface IMessage
    {
        [JsonProperty]
        MessageLevelEnum MessageLevel { get; set; }

        [JsonProperty]
        string Message { get; set; }

        [JsonProperty]
        string MessageNo { get; set; }

        [JsonProperty]
        DateTime Time { get; set; }
    }
}
