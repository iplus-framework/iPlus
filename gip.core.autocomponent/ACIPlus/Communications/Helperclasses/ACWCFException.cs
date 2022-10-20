using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public class ACWCFException : Exception
    {
        public enum WCFErrorCode : short
        {
            Disconnected = 0,
            ConfigurationError = 1,
        }

        #region c'tors
        public ACWCFException(string message, WCFErrorCode errorCode)
            : base(message)
        {
            _ErrorCode = errorCode;
        }
        #endregion

        #region Properties
        private WCFErrorCode _ErrorCode = WCFErrorCode.Disconnected;
        public WCFErrorCode ErrorCode
        {
            get
            {
                return _ErrorCode;
            }
        }
        #endregion
    }
}
