using gip.core.datamodel;

namespace gip.core.reporthandler
{
    /// <summary>
    /// Control charachters used in message to divide message parts
    /// </summary>
    [ACSerializeableInfo]
    [ACClassInfo("gip.VarioSystem", "en{'LinxASCIControlCharacterEnum'}de{'LinxASCIControlCharacterEnum'}", Global.ACKinds.TACEnum, Global.ACStorableTypes.NotStorable, false, false, "", "", 9999)]
    public enum LinxASCIControlCharacterEnum : byte
    {
        /// <summary>
        /// ASCII text terminator
        /// </summary>
        NUL = 0x00,

        /// <summary>
        /// Alternative data start delimiter
        /// </summary>
        SOH = 0x01,

        /// <summary>
        /// Data start delimiter
        /// </summary>
        STX = 0x02,

        /// <summary>
        /// Data end delimiter
        /// </summary>
        ETX = 0x03,
        EOT = 0x04,

        /// <summary>
        /// Print trigger character
        /// </summary>
        ENQ = 0x05,

        /// <summary>
        /// Positive acknowledgement
        /// </summary>
        ACK = 0x06,
        BEL = 0x07,

        /// <summary>
        /// Default print delay start character
        /// </summary>
        BS = 0x08,
        HT = 0x09,
        LF = 0xA,
        VT = 0xB,
        FF = 0xC,
        CR = 0xD,
        SO = 0xE,

        /// <summary>
        /// Default print start character
        /// </summary>
        SI = 0xF,
        DLE = 0x10,

        /// <summary>
        /// Default XON character
        /// </summary>
        DC1 = 0x11,
        DC2 = 0x12,

        /// <summary>
        /// Default XOFF character
        /// </summary>
        DC3 = 0x13,
        DC4 = 0x14,

        /// <summary>
        /// Negative acknowledgement
        /// </summary>
        NAK = 0x15,
        SYN = 0x16,
        ETB = 0x17,
        CAN = 0x18,

        /// <summary>
        /// Default print end character
        /// </summary>
        EM = 0x19,
        SUB = 0x1A,

        /// <summary>
        /// Escape character
        /// </summary>
        ESC = 0x1B,
        FS = 0x1C,
        GS = 0x1D,
        RS = 0x1E,
        US = 0x1F,
    }
}
