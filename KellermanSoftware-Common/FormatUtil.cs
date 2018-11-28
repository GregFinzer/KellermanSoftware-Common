using System;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// Helper functions for formatting
    /// </summary>
    public static class FormatUtil
    {
        /// <summary>
        /// Format a date in US Date format
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string FormatUSDateTime(DateTime dt)
        {
            return dt.ToString(new System.Globalization.CultureInfo("en-US"));
        }
    }
}
