using System;
using Microsoft.Win32;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// Additional System Information
    /// </summary>
    public static class SystemUtil
    {
        /// <summary>
        /// Save the key to the registry
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SaveToRegistry(string registryPath, string key, string value)
        {
            try
            {
                Microsoft.Win32.RegistryKey regKey;
                regKey = Registry.CurrentUser.CreateSubKey(registryPath);
                regKey.SetValue(key, value, RegistryValueKind.String);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Read the key from the registry
        /// </summary>
        /// <param name="registryPath"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ReadFromRegistry(string registryPath, string key)
        {
            try
            {
                RegistryKey regKey;
                regKey = Registry.CurrentUser.CreateSubKey(registryPath);
                return regKey.GetValue(key, string.Empty).ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the user name of the logged in user
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {
            if (System.Threading.Thread.CurrentPrincipal != null
                && System.Threading.Thread.CurrentPrincipal.Identity != null
                && System.Threading.Thread.CurrentPrincipal.Identity.Name.Length > 0)
            {
                return System.Threading.Thread.CurrentPrincipal.Identity.Name;
            }
            else
            {
                return System.Environment.UserName;
            }
        }

        /// <summary>
        /// Return the name of the current time zone accounting for Daylight Savings Time
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentTimeZone()
        {
            if (System.TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now))
                return System.TimeZone.CurrentTimeZone.DaylightName;
            else
                return System.TimeZone.CurrentTimeZone.StandardName;
        }

        /// <summary>
        /// Return the name of the company using an information from registry or 
        /// the given default value.
        /// </summary>
        /// <param name="defaultName"></param>
        /// <returns></returns>
        public static string GetCompanyName(string defaultName)
        {
            string companyName = null;

            try
            {
                string keyPath = @"Software\Microsoft\Windows\CurrentVersion";
                using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("RegisteredOrganization");
                        companyName = value == null ? null : value.ToString();
                    }
                }

                if (companyName == null)
                {
                    keyPath = @"Software\Microsoft\Windows NT\CurrentVersion";
                    using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
                    {
                        if (key != null)
                        {
                            object value = key.GetValue("RegisteredOrganization");
                            companyName = value == null ? null : value.ToString();
                        }
                    }
                }
            }
            catch
            {
                return defaultName;
            }

            return string.IsNullOrEmpty(companyName) ? defaultName : companyName;
        }
    }
}
