// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Diagnostics.Tracing;

namespace gip.core.datamodel
{
#if DIAGNOSE
    [EventSource(Name = "IPlusEvent")]
    public class IPlusEventSource : EventSource
    {
        public static IPlusEventSource Log = new IPlusEventSource();

        public class Keywords
        {
            public const EventKeywords ACState = (EventKeywords)0x1;
            public const EventKeywords LINQ = (EventKeywords)0x2;
        }

        public class Tasks
        {
            public const EventTask ACState = (EventTask)0x1;
            public const EventTask LINQ = (EventTask)0x2;
        }

        [Event(1, Message = "ACState-Change at {0} to {1} in line {2}", Opcode = EventOpcode.Info, Task = Tasks.ACState, Keywords = Keywords.ACState, Level = EventLevel.Informational)]
        public void LogACStateChange(string acUrl, string value, string line)
        {
            WriteEvent(1, acUrl, value, line);
        }

        [Event(2, Message = "LINQ started at {0} in {1} line {2}", Opcode = EventOpcode.Start, Task = Tasks.LINQ, Keywords = Keywords.LINQ, Level = EventLevel.Informational)]
        public void LINQStart(string acUrl, string classname, string line)
        {
            WriteEvent(2, acUrl, classname, line);
        }

        [Event(3, Message = "LINQ stopped at {0} in {1} line {2}", Opcode = EventOpcode.Start, Task = Tasks.LINQ, Keywords = Keywords.LINQ, Level = EventLevel.Informational)]
        public void LINQStop(string acUrl, string classname, string line)
        {
            WriteEvent(3, acUrl, classname, line);
        }
    }
#endif
}
