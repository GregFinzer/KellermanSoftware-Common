using System;
using System.Collections.Generic;
using System.Text;

namespace KellermanSoftware.Common
{
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
