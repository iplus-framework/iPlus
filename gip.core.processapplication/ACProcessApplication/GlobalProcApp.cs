using gip.core.datamodel;

namespace gip.core.processapplication
{
    [ACSerializeableInfo]
    public class GlobalProcApp
    {
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
