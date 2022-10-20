using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.tool.installerAndUpdater
{
    public class UserStatusInfo
    {
        public bool IsUsernamePassOk { get; set; }
        public UserStateEnum UserState { get; set; }
    }

    public enum UserStateEnum
    {
        Ready = 1,
        Timeout = 2,
        Locked = 3,
        PasswordChangeProcess = 4,
        PasswordChanged = 5,
        OutOfValidPeriod = 6,
        None = 999
    }
}
