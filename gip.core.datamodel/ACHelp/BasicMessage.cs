using System;

namespace gip.core.datamodel
{
    [Serializable]
#if !EFCR
    [JsonObject(MemberSerialization.OptIn)]
#endif
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
#endif
        public MessageLevelEnum MessageLevel { get; set; }

#if !EFCR
        [JsonProperty]
#endif
        public string MessageNo { get; set; }

#if !EFCR
        [JsonProperty]
#endif
        public string Message { get; set; }

#if !EFCR
        [JsonProperty]
#endif
        public DateTime Time { get; set; }

        #endregion

        #region Methods & operators
        public override string ToString()
        {
            return string.Format("[{0}] [{1}] (No:{2}) {3}", Time.ToShortTimeString(), MessageLevel.ToString(), MessageNo, Message);
        }

        public static explicit operator Msg(BasicMessage msg)
        {
            eMsgLevel level = eMsgLevel.Error;
            if (msg.MessageLevel == MessageLevelEnum.Success || msg.MessageLevel == MessageLevelEnum.Info)
            {
                level = eMsgLevel.Info;
            }
            return new Msg()
            {
                MessageLevel = level,
                Message = msg.Message
            };
        }

        #endregion
    }
}
