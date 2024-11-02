// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ActionResult'}de{'ActionResult'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ActionResult
    {
        private List<IMessage> messages;

        [JsonProperty]
        public bool Success { get; set; }

        [JsonProperty]
        public string BackURL { get; set; }

        [JsonProperty]
        public List<IMessage> Messages
        {
            get
            {
                if (messages == null) 
                    messages = new List<IMessage>();
                return messages;
            }
            set
            {
                messages = value;
            }
        }
    }

    public interface IActionResult<out T> { }

    public class ActionResult<T> : ActionResult, IActionResult<T>
    {
        [JsonProperty]
        public T Item { get; set; }
    }
}
