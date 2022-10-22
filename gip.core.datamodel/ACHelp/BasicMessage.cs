using System;

namespace gip.core.datamodel
{
#if !EFCR
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class BasicMessage: IMessage
    {
    #region ctor's
        public BasicMessage()
        {
            Time = DateTime.Now;
        }

    #endregion

    #region properties
#if !EFCR
        [JsonProperty]
        public MessageLevelEnum MessageLevel { get; set; }

        [JsonProperty]
        public string MessageNo { get; set; }

        [JsonProperty]
        public string Message { get; set; }

        [JsonProperty]
        public DateTime Time { get; set; }
#endif
    #endregion

    #region Methods & operators
#if !EFCR
        public override string ToString()
        {
            return string.Format("[{0}] [{1}] (No:{2}) {3}", Time.ToShortTimeString(), MessageLevel.ToString(), MessageNo, Message);
        }
#endif
        public static explicit operator Msg(BasicMessage msg)
        {
            eMsgLevel level = eMsgLevel.Error;
#if !EFCR
            if (msg.MessageLevel == MessageLevelEnum.Success || msg.MessageLevel == MessageLevelEnum.Info)
            {
                level = eMsgLevel.Info;
            }
#endif
            return new Msg()
            {
                MessageLevel = level,
#if !EFCR
                Message = msg.Message
#endif
            };
        }

    #endregion
    }
#endif
}
