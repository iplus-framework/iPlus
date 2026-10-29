// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;
using System.Runtime.Serialization;

namespace gip.core.media
{
    /// <summary>
    /// Transportable result of a media service call. Carries the raw file content as
    /// <see cref="byte"/> array together with the information about the underlying format,
    /// so that a client (desktop, browser or mobile) can deserialize and display it.
    /// </summary>
    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'MediaContentResult'}de{'MediaContentResult'}", Global.ACKinds.TACSimpleClass)]
    public class MediaContentResult : IVBFileContent
    {
        [IgnoreDataMember]
        private byte[] _Content;
        [DataMember]
        public byte[] Content
        {
            get { return _Content; }
            set { _Content = value; }
        }

        [IgnoreDataMember]
        private string _FileName;
        [DataMember]
        public string FileName
        {
            get { return _FileName; }
            set { _FileName = value; }
        }

        [IgnoreDataMember]
        private string _FilePath;
        /// <summary>
        /// Path of the stored file relative to the media root folder. Relative paths keep
        /// media items portable between Windows and Linux installations.
        /// </summary>
        [DataMember]
        public string FilePath
        {
            get { return _FilePath; }
            set { _FilePath = value; }
        }

        [IgnoreDataMember]
        private string _Extension;
        /// <summary>
        /// File extension including the leading dot, e.g. ".jpg" or ".pdf".
        /// </summary>
        [DataMember]
        public string Extension
        {
            get { return _Extension; }
            set { _Extension = value; }
        }

        [IgnoreDataMember]
        private string _MimeType;
        /// <summary>
        /// MIME type of <see cref="Content"/>, e.g. "image/jpeg" or "application/pdf".
        /// </summary>
        [DataMember]
        public string MimeType
        {
            get { return _MimeType; }
            set { _MimeType = value; }
        }

        [IgnoreDataMember]
        private bool _Success;
        [DataMember]
        public bool Success
        {
            get { return _Success; }
            set { _Success = value; }
        }

        [IgnoreDataMember]
        private Msg _Message;
        [DataMember]
        public Msg Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        public static MediaContentResult Failed(string message, eMsgLevel level = eMsgLevel.Error)
        {
            return new MediaContentResult
            {
                Success = false,
                Message = new Msg { MessageLevel = level, Message = message }
            };
        }
    }
}
