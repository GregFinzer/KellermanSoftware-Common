using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// Super simple logger that logs to a file, memory, console, or event log
    /// </summary>
    public static class SimpleLog
    {
        public static bool _logConsole = false;
        public static String _logFile = null;
        public static MemoryStream _logMemory = null;
        public static long _logMemoryOffset = 0;
        public static StringBuilder _logStringBuilder = null;

        /// <summary>
        /// Log a data table
        /// </summary>
        /// <param name="dt"></param>
        public static void Log(DataTable dt)
        {
            Log(GetDataTableLines(dt,1000,"    "));
        }

        /// <summary>
        /// Get all the lines for an exception and log them
        /// </summary>
        /// <param name="e"></param>
        public static void Log(Exception e)
        {
            Log(GetExceptionLines(e, string.Empty));
        }

        /// <summary>
        /// Construct a text string in the correct format for logging an exception
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="indent">The current indent level</param>
        /// <returns>A string that is structured for logging</returns>
        private static string GetExceptionLines(Exception ex, string indent)
        {
            if (ex == null)
                return "\r\nException object is null\r\n";

            string message = ex.Message;
            string source = ex.Source;
            string targetSite = "";
            string stackTrace = ex.StackTrace;
            string exceptionName = ex.ToString();
            string data = "";
            indent += "    ";

            foreach (object value in ex.Data.Keys)
            {
                data += "Key: " + value.ToString();
                data += "Value: " + ex.Data[value];
                data += "\r\n";
            }

            if (message == null || message.Trim().Length == 0) message = "(null)";
            if (ex.TargetSite == null || ex.TargetSite.ToString().Trim().Length == 0) targetSite = "(null)";
            if (ex.Data == null || ex.Data.ToString().Trim().Length == 0) data = "(null)";
            if (source == null || source.Trim().Length == 0) source = "(null)";
            if (stackTrace == null || stackTrace.Trim().Length == 0) stackTrace = "(null)";
            if (exceptionName == null || exceptionName.Trim().Length == 0) exceptionName = "(null)";

            message = "\r\n" + indent + "Message: " + message;
            source = indent + "Source: " + source;
            targetSite = indent + "Target Site: " + targetSite;
            stackTrace = indent + "Stack Trace: " + stackTrace;
            data = indent + "Data: " + data;
            string details = message + "\r\n" + source + "\r\n" + targetSite + "\r\n" + stackTrace + "\r\n" + data;
            if (ex.InnerException != null) details += "\r\n" + GetExceptionLines(ex.InnerException, indent + " ");
            return details;
        }

        /// <summary>
        /// Create and log to a custom event log
        /// </summary>
        /// <param name="entry">The entry containing the information for the event</param>
        /// <returns>The success of the call</returns>
        public static bool LogToEventLog(string message, EventLogEntryType logType, string eventLogName)
        {
            const int retentionDays = 7;
            const string defaultEventLog = "Application";
            EventLog elogger = null;

            try
            {
                if (eventLogName.Length == 0)
                {
                    eventLogName = defaultEventLog;
                }

                try
                {
                    if (!EventLog.SourceExists(eventLogName))
                    {
                        EventLog.CreateEventSource(eventLogName, eventLogName);

                        elogger = new EventLog();
                        elogger.Source = eventLogName;
                        elogger.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, retentionDays);
                    }
                    else
                    {
                        elogger = new EventLog();
                        elogger.Source = eventLogName;

                        if (elogger.OverflowAction != OverflowAction.OverwriteAsNeeded
                            && elogger.MinimumRetentionDays != retentionDays)
                        {
                            elogger.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, retentionDays);
                        }
                    }
                }
                catch
                {
                    //We couldn't create a custom event log so try to use the application log
                    if (eventLogName != defaultEventLog)
                    {
                        elogger = new EventLog();
                        elogger.Source = defaultEventLog;
                    }
                }

                elogger.WriteEntry(message, logType);
                elogger.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                try
                {
                    if (elogger != null)
                    {
                        elogger.Close();
                    }
                }
                catch
                {
                }

                return false;
            }
        }

        /// <summary>
        /// Log to the application event log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logType"></param>
        /// <returns></returns>
        public static bool LogToEventLog(string message, EventLogEntryType logType)
        {
            return LogToEventLog(message, logType, string.Empty);
        }

        /// <summary>
        /// Construct a text string in the correct format for logging rows within a data table
        /// </summary>
        /// <param name="table">The data table to log</param>
        /// <param name="maxRows">The maximum number of rows to display</param>
        /// <param name="indent">The current indention</param>
        /// <returns>A string that is structured for logging</returns>
        private static string GetDataTableLines(DataTable table, int maxRows, string indent)
        {
            int iColLength = 0;

            if (maxRows <= 0)
                throw new ArgumentOutOfRangeException("maxRows must be greater than zero");

            if (table == null)
            {
                return "\r\nDataTable is null\r\n";
            }

            if (table.Rows.Count == 0)
            {
                return "\r\nNo Rows\r\n";
            }

            StringBuilder sb = new StringBuilder(table.Rows.Count * 256);
            sb.Append("\r\n");

            //Sort the column names
            string[] sorted = new string[table.Columns.Count];

            for (int i = 0; i < table.Columns.Count; i++)
            {
                sorted[i] = table.Columns[i].Caption.ToString();
                iColLength = Math.Max(sorted[i].Length, iColLength);
            }

            Array.Sort(sorted, 0, sorted.Length);

            int lines = 0;

            foreach (DataRow row in table.Rows)
            {
                //** Row 1 (Added)
                sb.Append(indent);
                sb.Append("**Row ");
                sb.Append(lines.ToString());
                sb.Append(" (");
                sb.Append(row.RowState.ToString());
                sb.Append(")\r\n");

                for (int i = 0; i < sorted.Length; i++)
                {
                    //Column Name
                    sb.Append(indent);
                    sb.Append(sorted[i].PadRight(iColLength, ' '));

                    sb.Append(" = ");

                    //Value
                    if (row[sorted[i]] == System.DBNull.Value)
                        sb.Append("(null)");
                    else
                        sb.Append(row[sorted[i]].ToString());

                    //Data Type
                    sb.Append(" [");
                    sb.Append(row[sorted[i]].GetType().ToString());
                    sb.Append("]\r\n");
                }

                lines += 1;

                if (lines >= maxRows)
                {
                    break;
                }

            }

            return sb.ToString();
        }

        /// <summary>
        /// Log strings to either a console, file or a memory stream depending on the user settings
        /// </summary>
        public static void Log(String line)
        {
            if (line == null)
                return;

            line += Environment.NewLine;

            if (_logConsole)
            {
                Console.Write(line);
            }

            if (_logFile != null)
            {
                lock (_logFile)
                {
                    File.AppendAllText(_logFile, line);
                }
            }

            if (_logStringBuilder != null)
            {
                _logStringBuilder.Append(line);
            }

            if (_logMemory != null)
            {
                lock (_logMemory)
                {
                    UnicodeEncoding uniEncoding = new UnicodeEncoding();
                    byte[] lineBytes = uniEncoding.GetBytes(line);
                    _logMemory.Seek(_logMemoryOffset, 0);
                    _logMemory.Write(lineBytes, 0, lineBytes.Length);
                    _logMemoryOffset = _logMemory.Position;
                }
            }
        }
    }
}
