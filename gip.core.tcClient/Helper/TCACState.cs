using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.tcClient
{
    public enum TCACState : short
    {
        SMIdle = 0,
	    SMStarting = 10,
	    SMRunning = 11,

		SMPausing = 20,
		SMPaused = 21,
		SMResuming = 22,

		SMHolding = 30,
		SMHeld = 31,
		SMRestarting = 32,

		SMAborting = 40,
		SMAborted = 41,

		SMStopping = 50,
		SMStopped = 51,
		
	    SMCompleted = 90,
	    SMResetting = 99	
    }
}
