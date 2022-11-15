using System.Collections.Generic;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{

    [ACClassInfo(Const.PackName_VarioSystem, "en{'ActionResult'}de{'ActionResult'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    [DataContract]
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
