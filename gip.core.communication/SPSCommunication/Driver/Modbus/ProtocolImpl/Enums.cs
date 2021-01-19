using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.communication.modbus
{
    #region WriteMode
    [ACClassInfo(Const.PackName_VarioSystem, "en{'WriteMode modbus'}de{'WriteMode modbus'}", Global.ACKinds.TACEnum)]
    public enum WriteModeModbus
    {
        // Send each item seperately. Only if the sequence of changed Items is a Region without gaps (Items are neigbours in Memory) the send them together to the plc
        // Usable, when Ram-Areas in Datablock are not exclusive written by IPlus, an all changes should be send to plc
        Separately = 0,
        // Build Groups for all changed Items, which are Neigbours in Memory. Send aech Group to PLC
        // Usable, when Ram-Areas in Datablock are not exclusive written by IPlus, but only last changed Value is valid (e.g. the same Item is changed more times an the current value is valid)
        AllNeigboursWithLatestValue = 1,
        // Useable, when DataBlock is exclusively written by VarioBatch and last changed Value is valid
        DataBlockLatestValue = 2,
    }
    #endregion

    #region Error Codes
    public enum ErrorCode
    {
        NoError = 0,
        WrongCPU_Type = 1,
        ConnectionError = 2,
        IPAdressNotAvailable = 3,
        WrongVarFormat = 4,
        WrongNumberReceivedBytes = 5,
        DBNotExist = 6,
        DBRangeToSmall = 7,
        WriteData = 8,
        ReadData = 8,
        Exception = 99,
    }
    #endregion

    #region TableType
    public enum TableType : int
    {
        Input = 1, // Discrete Inputs
        Output = 2, // Coil
        ReadOnlyRegister = 3, // Input Registers, Readonly
        ReadWriteRegister = 4, // Holding Registers, Read/Write
    }
    #endregion

    #region VarType
    public enum VarType
    {
        Bit,
        Byte,
        Word,
        DWord,
        Int,
        DInt,
        Real,
        String,
        Base64String,
    }
    #endregion
}
