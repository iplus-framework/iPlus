using System;

namespace gip.core.datamodel
{
#if !EFCR
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
#endif
}
