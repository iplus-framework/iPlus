using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.bso.iplus.VarioBatch.Helper
{
    public class BuildLogger : ILogger
    {
        public List<string> Messages { get; private set; }

        public BuildLogger()
        {
            Messages = new List<string>();
        }

        private static string Format(DateTime data)
        {
            return data.ToShortTimeString();
        }

        public void Initialize(IEventSource eventSource)
        {
            eventSource.AnyEventRaised += new AnyEventHandler((object sender, BuildEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Build event " + e.Message);
            });
            eventSource.BuildFinished += new BuildFinishedEventHandler((object sender, BuildFinishedEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Build finished " + e.Message);
            });
            eventSource.BuildStarted += new BuildStartedEventHandler((object sender, BuildStartedEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Build started " + e.Message);
            });
            eventSource.CustomEventRaised += new CustomBuildEventHandler((object sender, CustomBuildEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Build custom event " + e.Message);
            });
            eventSource.ErrorRaised += new BuildErrorEventHandler((object sender, BuildErrorEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Build error " + e.Message);
            });
            eventSource.MessageRaised += new BuildMessageEventHandler((object sender, BuildMessageEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Build message " + e.Message);
            });
            eventSource.ProjectFinished += new ProjectFinishedEventHandler((object sender, ProjectFinishedEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Project finished " + e.Message);
            });
            eventSource.ProjectStarted += new ProjectStartedEventHandler((object sender, ProjectStartedEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Project started " + e.Message);
            });
            eventSource.StatusEventRaised += new BuildStatusEventHandler((object source, BuildStatusEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Build event " + e.Message);
            });
            eventSource.TargetFinished += new TargetFinishedEventHandler((object sender, TargetFinishedEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Target finished " + e.Message);
            });
            eventSource.TargetStarted += new TargetStartedEventHandler((object sender, TargetStartedEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Target started " + e.Message);
            });
            eventSource.TaskFinished += new TaskFinishedEventHandler((object sender, TaskFinishedEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Task finished " + e.Message);
            });
            eventSource.TaskStarted += new TaskStartedEventHandler((object sender, TaskStartedEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Task started " + e.Message);
            });
            eventSource.WarningRaised += new BuildWarningEventHandler((object sender, BuildWarningEventArgs e) =>
            {
                Messages.Add(Format(e.Timestamp) + " Build warning " + e.Message);
            });
        }

        public string Parameters { get; set; }

        public void Shutdown()
        {
        }

        public LoggerVerbosity Verbosity { get; set; }
    }
}
