using System;

namespace gip.core.datamodel
{
#if !EFCR
    [JsonObject(MemberSerialization.OptIn)]
#endif
    public interface IMessage
    {
#if !EFCR
        [JsonProperty]
#endif
        MessageLevelEnum MessageLevel { get; set; }

#if !EFCR
        [JsonProperty]
#endif
        string Message { get; set; }

#if !EFCR
        [JsonProperty]
#endif
        string MessageNo { get; set; }

#if !EFCR
        [JsonProperty]
#endif
        DateTime Time { get; set; }
    }
}
