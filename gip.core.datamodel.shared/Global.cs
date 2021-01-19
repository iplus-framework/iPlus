using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace gip.core.datamodel
{
    public partial class Global
    {
#if NETFRAMEWORK
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'Global.MsgResult'}de{'Global.MsgResult'}", Global.ACKinds.TACEnum)]
#endif
        [DataContract]
        public enum MsgResult : short
        {
            // Summary:
            //     The message box returns no result.
            [EnumMember(Value = "N0")]
            None = 0,
            //
            // Summary:
            //     The result value of the message box is OK.
            [EnumMember(Value = "O")]
            OK = 1,
            //
            // Summary:
            //     The result value of the message box is Cancel.
            [EnumMember(Value = "C")]
            Cancel = 2,
            //
            // Summary:
            //     The result value of the message box is Yes.
            [EnumMember(Value = "Y")]
            Yes = 3,
            //
            // Summary:
            //     The result value of the message box is No.
            [EnumMember(Value = "N1")]
            No = 4,
        }
    }
}
