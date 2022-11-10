using System.Collections.Generic;

namespace gip.core.datamodel
{
#if !EFCR
    [JsonObject(MemberSerialization.OptIn)]
#endif
    public class ActionResult
    {
        private List<IMessage> messages;

#if !EFCR
        [JsonProperty]
#endif
        public bool Success { get; set; }

#if !EFCR
        [JsonProperty]
#endif
        public string BackURL { get; set; }

#if !EFCR
        [JsonProperty]
#endif
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
#if !EFCR
        [JsonProperty]
#endif
        public T Item { get; set; }
    }
}
