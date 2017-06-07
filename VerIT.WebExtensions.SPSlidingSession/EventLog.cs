using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace VerIT.Utils
{
    /// <summary>
    /// Class to add Traces in Event Viewer
    /// </summary>
    public class EventLog
    {
        /// <summary>
        /// Ensure Event Log source exists
        /// </summary>
        public static void EnsureSource(string eventSource, string eventLog = "Application")
        {
            if (!System.Diagnostics.EventLog.SourceExists(eventSource))
            {
                System.Diagnostics.EventLog.CreateEventSource(eventSource, eventLog);
            }
        }
        /// <summary>
        /// Log Errors messages in Event Viewer
        /// </summary>
        public static void Error(Exception e, string eventSource, [CallerMemberName] string callerMethod = null)
        {
            try
            {
                if (!System.Diagnostics.EventLog.SourceExists(eventSource)) return;
                string message = string.Format("{0}\n{1}\n{2}\nInner Exception: {3}\n\nCaller: {4}", e.Message, e.Source, e.StackTrace, e.InnerException, callerMethod);
                System.Diagnostics.EventLog.WriteEntry(eventSource, message, EventLogEntryType.Error);
            }
            catch (Exception)
            {
              
            }
        }
        /// <summary>
        /// Log Info messages in Event Viewer
        /// </summary>
        public static void Info(string message, string eventSource)
        {
            try
            {
                if (!System.Diagnostics.EventLog.SourceExists(eventSource)) return;
                System.Diagnostics.EventLog.WriteEntry(eventSource, message, EventLogEntryType.Information);
            }
            catch (Exception)
            {
               
            }
        }
        /// <summary>
        /// Log Warning messages in Event Viewer
        /// </summary>
        public static void Warning(string message, string eventSource)
        {
            try
            {
                if (!System.Diagnostics.EventLog.SourceExists(eventSource)) return;
                System.Diagnostics.EventLog.WriteEntry(eventSource, message, EventLogEntryType.Warning);
            }
            catch (Exception)
            {

            }
        }
    }
}
