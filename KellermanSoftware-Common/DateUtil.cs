using System;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// Utility functions for date and time 
    /// </summary>
    public static class DateUtil
    {
        /// <summary>
        /// Convert from milliseconds elapsed to a text string elapsed
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static String MillisecondsToTimeLapse(long milliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(milliseconds);

            string result;

            if ((long)ts.TotalDays == 1)
                result = string.Format("{0:n0} day, {1:n0} hrs, {2:n0} mins, {3:n0} secs, {4:n0} ms", ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            else if (ts.TotalDays >= 1)
                result = string.Format("{0:n0} days, {1:n0} hrs, {2:n0} mins, {3:n0} secs, {4:n0} ms", ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            else if ((long)ts.TotalHours == 1)
                result = string.Format("{0:n0} hr, {1:n0} mins, {2:n0} secs, {3:n0} ms", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            else if (ts.TotalHours >= 1)
                result = string.Format("{0:n0} hrs, {1:n0} mins, {2:n0} secs, {3:n0} ms", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            else if ((long)ts.TotalMinutes == 1)
                result = string.Format("{0:n0} min, {1:n0} secs, {2:n0} ms", ts.Minutes, ts.Seconds, ts.Milliseconds);
            else if (ts.TotalMinutes >= 1)
                result = string.Format("{0:n0} mins, {1:n0} secs, {2:n0} ms", ts.Minutes, ts.Seconds, ts.Milliseconds);
            else if ((long)ts.TotalSeconds == 1)
                result = string.Format("{0:n0} sec, {1:n0} ms", ts.Seconds, ts.Milliseconds);
            else if (ts.TotalSeconds >= 1)
                result = string.Format("{0:n0} secs, {1:n0} ms", ts.Seconds, ts.Milliseconds);
            else
                result = string.Format("{0:n0} ms", ts.Milliseconds);

            result = result.Replace(" 1 hrs", " 1 hr");
            result = result.Replace(" 1 mins", " 1 min");
            result = result.Replace(" 1 secs", " 1 sec");

            return result;
        }
    }
}
