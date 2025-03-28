// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Net;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IWSResponse
    {
        bool Suceeded { get; }
        Msg Message { get;  }
    }

#if NETFRAMEWORK
    [ACClassInfo(Const.PackName_VarioSystem, "en{'WSResponse'}de{'WSResponse'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
#endif
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

        public WSResponse(T data, HttpStatusCode httpStatusCode)
        {
            _Data = data;
            HttpStatusCode = httpStatusCode;
        }

        public WSResponse(T data, Msg msg)
        {
            _Data = data;
            _Message = msg;
        }

        public WSResponse(T data, Msg msg, HttpStatusCode httpStatusCode)
        {
            _Data = data;
            _Message = msg;
            HttpStatusCode = httpStatusCode;
        }

        public WSResponse(Msg msg)
        {
            _Data = default(T);
            _Message = msg;
        }

        public WSResponse(Msg msg, HttpStatusCode httpStatusCode)
        {
            _Data = default(T);
            _Message = msg;
            HttpStatusCode = httpStatusCode;
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
        public HttpStatusCode HttpStatusCode { get;set;}

        [IgnoreDataMember]
        private Msg _Message;

#if NETFRAMEWORK
        [ACPropertyInfo(101, "en{'Message'}de{'Message'}")]
#endif
        [DataMember()]
        public Msg Message
        {
            get { return _Message; }
            set { _Message = value; }
        }


        [IgnoreDataMember]
        private T _Data;

#if NETFRAMEWORK
        [ACPropertyInfo(100, "en{'Data'}de{'Data'}")]
#endif
        [DataMember()]
        public T Data
        {
            get { return _Data; }
            set { _Data = value; }
        }

        public static Msg LoginAgainMessage
        {
            get =>  new Msg() { Row = 9999, Column = 9999, ACIdentifier = "LoginAgain" };
        }
    }
}
