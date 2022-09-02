using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    //AttachedAlarmHandler
    public interface IACAttachedAlarmHandler
    {
        IACContainerTNet<bool> HasAttachedAlarm
        {
            get;
            set;
        }

        SafeList<Msg> AttachedAlarms
        {
            get;
            set;
        }

        void AddAttachedAlarm(Msg msg);

        void AckAttachedAlarm(Guid msgID);

        void AckAllAttachedAlarms();

        MsgList GetAttachedAlarms();
    }
}
