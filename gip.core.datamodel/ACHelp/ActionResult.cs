﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace gip.core.datamodel
{
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
                if (messages == null) messages = new List<IMessage>();
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
