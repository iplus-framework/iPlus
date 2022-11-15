using gip.core.datamodel;

namespace gip.core.processapplication
{
    [ACSerializeableInfo]
    public class GlobalProcApp
    {
        /// <summary>
        /// Describes a state of machine for the determination of the Overall equipment effectiveness (OEE)
        /// https://en.wikipedia.org/wiki/Overall_equipment_effectiveness   
        /// </summary>
        [ACSerializeableInfo]
        [ACClassInfo(Const.PackName_VarioSystem, "en{'Availability-State for OEE'}de{'Verfügbarkeitsstatus für OEE'}", Global.ACKinds.TACEnum)]
        public enum AvailabilityState : short
        {
            /// <summary>
            /// Idle-State means,<para />
            /// for Process-Modules: Is not allocated/occupied from a Workflow-Group<para />
            /// for ApplicationManager: Not any workflow loaded in this Application<para />
            /// According OEE, this state should be considered as "Scheduled Downtime"
            /// </summary>
            Idle = 0,

            /// <summary>
            /// Standby-State means,<para />
            /// for Process-Modules: Is allocated/occupied from a Workflow-Group but no PAProcessFunction is active (TaskInvocationPoint is Empty)<para />
            /// for ApplicationManager: All active Process-Modules are in Standby-State<para />
            /// According OEE, this state should be considered as "Uptime"
            /// </summary>
            Standby = 1,

            /// <summary>
            /// Operating means,<para />
            /// for Process-Modules: Is allocated/occupied from a Workflow-Group but at least one PAProcessFunction is active (TaskInvocationPoint is not Empty)<para />
            /// for ApplicationManager: At least one Process-Module is in Operating-State<para />
            /// According OEE, this state should be considered as "Uptime"
            /// </summary>
            InOperation = 2,

            /// <summary>
            /// ScheduledBreak-State means,<para />
            /// for Process-Modules: All running PAProcessFunctions are set to SMPaused-State from the operator<para />
            /// for ApplicationManager: All active Process-Modules are in ScheduledBreak-State<para />
            /// According OEE, this state should be considered as "Scheduled Downtime"
            /// </summary>
            ScheduledBreak = 3,

            /// <summary>
            /// UnscheduledBreak-State means,<para />
            /// for Process-Modules: At least one of the running PAProcessFunctions are in Malfunction or is set to the SMPaused-State automatically<para />
            /// for ApplicationManager: All active Process-Modules are in UnscheduledBreak-State <para />
            /// According OEE, this state should be considered as "Unscheduled Downtime"
            /// </summary>
            UnscheduledBreak = 4,

            /// <summary>
            /// Retooling-State means,<para />
            /// for Process-Modules: A PAProcessFunction is active which is responsible for Retooling or the PWGroup which occupies this Process-Module runs a Retooling-Node<para />
            /// for ApplicationManager:  All active Process-Modules are in Retooling-State<para />
            /// According OEE, this state should be considered as "Uptime" or as "Scheduled Downtime"
            /// </summary>
            Retooling = 5,

            /// <summary>
            /// Maintenance-State means,<para />
            /// for Process-Modules: OperatingMode is in Global.OperatingMode.Maintenance<para />
            /// for ApplicationManager: Not any workflow loaded and at least one Process-Module mus be in Maintenance-Mode<para />
            /// According OEE, this state should be considered as "Scheduled Downtime"
            /// </summary>
            Maintenance = 6,
        }

        static ACValueItemList _AvailabilityStateList = null;
        /// <summary>
        /// Translations for AvailabilityState
        /// </summary>
        static public ACValueItemList AvailabilityStateList
        {
            get
            {
                if (GlobalProcApp._AvailabilityStateList == null)
                {

                    GlobalProcApp._AvailabilityStateList = new ACValueItemList("AvailabilityStateIndex");
                    GlobalProcApp._AvailabilityStateList.AddEntry((short)AvailabilityState.Idle, "en{'Idle'}de{'Inaktiv'}");
                    GlobalProcApp._AvailabilityStateList.AddEntry((short)AvailabilityState.Standby, "en{'Standby'}de{'Bereitschaft'}");
                    GlobalProcApp._AvailabilityStateList.AddEntry((short)AvailabilityState.InOperation, "en{'In Operation'}de{'In Betrieb'}");
                    GlobalProcApp._AvailabilityStateList.AddEntry((short)AvailabilityState.ScheduledBreak, "en{'Scheduled Break'}de{'Geplante Unterbrechung'}");
                    GlobalProcApp._AvailabilityStateList.AddEntry((short)AvailabilityState.UnscheduledBreak, "en{'Unscheduled Break'}de{'Ungeplante Unterbrechung'}");
                    GlobalProcApp._AvailabilityStateList.AddEntry((short)AvailabilityState.Retooling, "en{'Retooling'}de{'Umrüstung'}");
                    GlobalProcApp._AvailabilityStateList.AddEntry((short)AvailabilityState.Maintenance, "en{'Maintenance'}de{'Wartung'}");
                }
                return GlobalProcApp._AvailabilityStateList;
            }
        }

        public const string AvailabilityStatePropName = nameof(IPAOEEProvider.AvailabilityState);

        public const string AvailabilityStateGroupName = "en{'Availability state'}de{'Verfügbarkeitsstatus'}";
    }

}
