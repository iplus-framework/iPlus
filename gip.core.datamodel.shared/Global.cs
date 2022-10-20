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

        #region ParamOption
        /// <summary>
        /// Enum für das Feld ParamOptionIndex
        /// </summary>
#if NETFRAMEWORK
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ParamOption'}de{'ParamOption'}", Global.ACKinds.TACEnum)]
#else
        [DataContract]
#endif
        public enum ParamOption : short
        {
            [EnumMember(Value = "NR")]
            NotRequired = 0,    // Nicht erforderlich
            [EnumMember(Value = "R")]
            Required = 1,       // Erforderlich
            [EnumMember(Value = "O")]
            Optional = 2,       // Optional
            [EnumMember(Value = "F")]
            Fix = 3             // Fester Wert, der nicht geändert werden kann
        }
        #endregion

    }
}
