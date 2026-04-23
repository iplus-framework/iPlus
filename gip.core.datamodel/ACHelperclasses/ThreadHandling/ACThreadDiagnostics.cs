// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace gip.core.datamodel
{
    public class ACThreadDiagnosticSnapshot
    {
        public int ManagedThreadId;
        public string ThreadName;
        public string Section;
        public string Context;
        public DateTime TimestampUtc;
        public DateTime StackTimestampUtc;
        public string StackTrace;
    }

    public static class ACThreadDiagnostics
    {
        private static readonly ConcurrentDictionary<int, ACThreadDiagnosticSnapshot> _snapshots =
            new ConcurrentDictionary<int, ACThreadDiagnosticSnapshot>();

        private static bool? _Enabled;
        public static bool Enabled
        {
            get
            {
                if (!_Enabled.HasValue)
                    return true;
                return _Enabled.Value;
            }
            set
            {
                if (_Enabled.HasValue)
                    return;
                _Enabled = value;
            }
        }

        // Stack unwinding is expensive; throttle captures per thread unless section changes.
        public static int MinStackCaptureIntervalMs = 2000;
        public static bool CaptureFileInfo = false;

        public static void MarkCurrentThread(string section, string context = null, bool captureStack = false)
        {
            if (!Enabled)
                return;

            Thread current = Thread.CurrentThread;
            ACThreadDiagnosticSnapshot previous;
            _snapshots.TryGetValue(current.ManagedThreadId, out previous);

            bool shouldCaptureStack = ShouldCaptureStack(captureStack, section, previous);
            string stackTrace = shouldCaptureStack
                ? CaptureCurrentStackTrace()
                : previous != null ? previous.StackTrace : null;

            DateTime stackTimestampUtc = previous != null ? previous.StackTimestampUtc : DateTime.MinValue;
            if (shouldCaptureStack)
                stackTimestampUtc = DateTime.UtcNow;

            var snapshot = new ACThreadDiagnosticSnapshot
            {
                ManagedThreadId = current.ManagedThreadId,
                ThreadName = current.Name,
                Section = section,
                Context = context,
                TimestampUtc = DateTime.UtcNow,
                StackTimestampUtc = stackTimestampUtc,
                StackTrace = stackTrace
            };

            _snapshots[current.ManagedThreadId] = snapshot;
        }

        private static bool ShouldCaptureStack(bool captureStack, string section, ACThreadDiagnosticSnapshot previous)
        {
            if (!captureStack)
                return false;

            if (previous == null)
                return true;

            if (!String.Equals(previous.Section, section, StringComparison.Ordinal))
                return true;

            if (previous.StackTimestampUtc == DateTime.MinValue)
                return true;

            double elapsedMs = (DateTime.UtcNow - previous.StackTimestampUtc).TotalMilliseconds;
            return elapsedMs >= MinStackCaptureIntervalMs;
        }

        public static bool TryGet(int managedThreadId, out ACThreadDiagnosticSnapshot snapshot)
        {
            return _snapshots.TryGetValue(managedThreadId, out snapshot);
        }

        public static string FormatForThread(int managedThreadId)
        {
            if (!Enabled)
                return "Cooperative thread diagnostics are disabled by switch 'CoreConfiguration.UseCooperativeThreadDiagnostics'.";

            ACThreadDiagnosticSnapshot snapshot;
            if (!TryGet(managedThreadId, out snapshot) || snapshot == null)
                return "No cooperative diagnostics snapshot available.";

            StringBuilder sb = new StringBuilder();
            sb.Append("Section: ").Append(snapshot.Section);
            if (!String.IsNullOrEmpty(snapshot.Context))
                sb.Append(", Context: ").Append(snapshot.Context);
            sb.Append(", Utc: ").Append(snapshot.TimestampUtc.ToString("O"));

            if (!String.IsNullOrEmpty(snapshot.StackTrace))
            {
                sb.AppendLine();
                sb.Append(snapshot.StackTrace);
            }

            return sb.ToString();
        }

        private static string CaptureCurrentStackTrace()
        {
            try
            {
                return new StackTrace(1, CaptureFileInfo).ToString();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;
                return "<stack-trace-unavailable>: " + msg;
            }
        }
    }
}