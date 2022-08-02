using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.communication.ISOonTCP
{
    #region CPU_Type
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'CPU_Type'}de{'CPU_Type'}", Global.ACKinds.TACEnum)]
    public enum CPU_Type
    {
        S7200 = 0,
        S7300 = 10,
        S7400 = 20,
        S71500 = 30
    }
    #endregion

    #region WriteMode
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'WriteMode'}de{'WriteMode'}", Global.ACKinds.TACEnum)]
    public enum WriteMode
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

    #region DataType
    public enum DataType
    {
        Input = 129,
        Output = 130,
        Marker = 131,
        DataBlock = 132,
        Timer = 29,
        Counter = 28
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
        Array,
        Timer,
        Counter,
        Base64String
    }
    #endregion

    public enum DBNoSpecial : int
    {
        Inputs = 0,
        Outputs = -1,
        Marker = -2,
        Timer = -3,
        Counter = -4,
    }

    internal enum ReadWriteErrorCode : byte
    {
        Reserved = 0x00,
        HardwareFault = 0x01,
        AccessingObjectNotAllowed = 0x03,
        AddressOutOfRange = 0x05,
        DataTypeNotSupported = 0x06,
        DataTypeInconsistent = 0x07,
        ObjectDoesNotExist = 0x0a,
        Success = 0xff
    }

    internal enum ParameterErrorCode : ushort
    {
        NoError = 0x0000,
        HardwareFault = 0x0001,
        InvalidBlockTypeNumber = 0x0110,
        InvalidParameter = 0x0112,
        PgResourceError = 0x011A,
        PlcResourceError = 0x011B,
        ProtocolError = 0x011C,
        UserBufferTooShort = 0x011F,
        RequestError = 0x0141,
        VersionMismatch = 0x01C0,
        NotImplement = 0x01F0,
        L7InvalidCpuState = 0x8001,
        L7PduSizeError = 0x8500,
        L7InvalidSzlId = 0xD401,
        L7InvalidIndex = 0xD402,
        L7DgsConnectionAlreadyAnnounced = 0xD403,
        L7MaxUserNb = 0xD404,
        L7DgsFunctionParameterSyntaxError = 0xD405,
        L7NoInfo = 0xD406,
        L7PrtFunctionParameterSyntaxError = 0xD601,
        L7InvalidVariableAddress = 0xD801,
        this_job_does_not_exist = 0xD802,
        L7InvalidRequestStatus = 0xD803,
        object_access_not_allowed__occurs_when_access_to_timer_and_counter_data_type_is_set_to_signed_integer_and_not_BCD = 0x0003,
        context_not_supported = 0x0004,
        address_out_of_range__occurs_when_requesting_an_address_within_a_data_block_that_does_not_exist_or_is_out_of_range = 0x0005,
        address_out_of_range = 0x0006,
        write_data_size_mismatch = 0x0007,
        object_does_not_exist__occurs_when_trying_to_request_a_data_block_that_does_not_exist = 0x000A,
        communication_link_not_available = 0x0101,
        negative_acknowledge__time_out_error = 0x010A,
        data_does_not_exist_or_is_locked = 0x010C,
        unknown_error = 0x0200,
        wrong_interface_specified = 0x0201,
        too_many_interfaces = 0x0202,
        interface_already_initialized = 0x0203,
        interface_already_initialized_with_another_connection = 0x0204,
        interface_not_initialized__this_may_be_due_to_an_invalid_MPI_address_local_or_remote_ID_or_the_PLC_is_not_communicating_on_the_MPI_network = 0x0205,
        can_t_set_handle = 0x0206,
        data_segment_isn_t_locked = 0x0207,
        data_field_incorrect = 0x0209,
        block_size_is_too_small = 0x0302,
        block_boundary_exceeded = 0x0303,
        wrong_MPI_baud_rate_selected = 0x0313,
        highest_MPI_address_is_wrong = 0x0314,
        address_already_exists = 0x0315,
        not_connected_to_MPI_network = 0x031A,
        unknown_error_2 = 0x031B,
        hardware_error = 0x0320,
        hardware_error2 = 0x0381,
        communication_link_unknown = 0x4001,
        communication_link_not_available_ = 0x4002,
        MPI_communication_in_progress = 0x4003,
        MPI_connection_down__this_may_be_due_to_an_invalid_MPI_address_local_or_remote_ID_or_the_PLC_is_not_communicating_on_the_MPI_network = 0x4004,
        interface_is_busy = 0x8000,
        hardware_error_2 = 0x8101,
        access_to_object_not_permitted = 0x8103,
        context_not_supported_2 = 0x8104,
        address_invalid_This_may_be_due_to_a_memory_address_that_is_not_valid_for_the_PLC = 0x8105,
        data_type_not_supported = 0x8106,
        data_type_not_consistent = 0x8107,
        object_doesnt_exist_This_may_be_due_to_a_data_block_that_doesnt_exist_in_the_PLC = 0x810A,
        not_enough_memory_on_CPU = 0x8301,
        maybe_CPU_already_in_RUN_or_already_in_STOP = 0x8402,
        serious_error = 0x8404,
        wrong_PDU_response_data_size = 0x8500,
        address_not_valid = 0x8702,
        Step7__variant_of_command_is_illegal = 0xD002,
        Step7__status_for_this_command_is_illegal = 0xD004,
        Step7__function_is_not_allowed_in_the_current_protection_level = 0xD0A1,
        syntax_error__block_name = 0xD201,
        syntax_error__function_parameter = 0xD202,
        syntax_error__block_type = 0xD203,
        no_linked_data_block_in_CPU = 0xD204,
        object_already_exists = 0xD205,
        object_already_exists2 = 0xD206,
        data_block_in_EPROM = 0xD207,
        block_doesnt_exist = 0xD209,
        no_block_available = 0xD20E,
        block_number_too_large = 0xD210,
        coordination_rules_were_violated = 0xD240,
        protection_level_too_low = 0xD241,
        protection_violation_while_processing_F_blocks__F_blocks_can_only_be_processed_after_password_input = 0xD242,
        diagnosis__DP_error = 0xD409,
        maybe_invalid_BCD_code_or_Invalid_time_format = 0xDC01,
        wrong_ID2_cyclic_job_handle = 0xEF01,
        API_function_called_with_an_invalid_parameter = 0xFFCF,
        timeout_check_RS232_interface = 0xFFFF,

    }

    internal enum HeaderErrorClass : byte
    {
        NoError = 0x00,
        ApplicationRelationShipError = 0x81,
        ObjectDefiniationError = 0x82,
        NoResourcesAvailableError = 0x83,
        ErrorOnServiceProcessing = 0x84,
        ErrorOnSupplies = 0x85,
        AccessError = 0x87
    }

    internal enum FunctionCode : byte
    {
        Read = 0x04,
        Write = 0x05,
        CommunicationSetup = 0xf0
    }

    public enum Area : byte
    {
        S200SystemInfo = 0x03,
        S200SystemFlags = 0x05,
        S200AnalogInput = 0x06,
        S200AnalogOutput = 0x07,
        S7Counters = 0x1c,
        S7Timers = 0x1d,
        IecCounters = 0x1e,
        IecTimers = 0x1f,
        DirectPeripheralAccess = 0x80,
        Inputs = 0x81,
        Outputs = 0x82,
        Flags = 0x83,
        DataBlock = 0x84,
        InstanceDataBlock = 0x85,
        LocalData = 0x86,
        Unknown = 0x87
    }

    internal enum MessageType : byte
    {
        JobRequest = 0x01,
        Ack = 0x02,
        AckData = 0x03,
        UserData = 0x07
    }

}
