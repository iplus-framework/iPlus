using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IWSResponse
    {
        bool Suceeded { get; }
        Msg Message { get;  }
    }

    [DataContract]
    public class WSResponse<T> : IWSResponse
    {
        public WSResponse()
        {
        }

        public WSResponse(T data)
        {
            _Data = data;
        }

        public WSResponse(T data, Msg msg)
        {
            _Data = data;
            _Message = msg;
        }

        [IgnoreDataMember]
        public bool Suceeded
        {
            get
            {
                return Message == null || Message.MessageLevel < eMsgLevel.Warning;
            }
        }

        [IgnoreDataMember]
        private Msg _Message;

        [DataMember()]
        public Msg Message
        {
            get { return _Message; }
            set { _Message = value; }
        }


        [IgnoreDataMember]
        private T _Data;

        [DataMember()]
        public T Data
        {
            get { return _Data; }
            set { _Data = value; }
        }
    }
}
