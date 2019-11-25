using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Security;
using System.Text;
using Microsoft.Win32;
//using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.Devices;

namespace KellermanSoftware.Common
{

    /// <summary>
    /// Get system information using unmanaged code
    /// </summary>
    public static class SystemInfo
    {
        #region Class Variables
        const double MEM_KB = 1024;
        const double MEM_MB = MEM_KB * MEM_KB;


        #endregion

        /// <summary>
        /// Get the free Megabytes for the passed drive letter
        /// This procedure works with Windows 98,ME,2000,XP and above
        /// </summary>
        /// <param name="driveName"></param>
        /// <returns></returns>
        public static string GetFreeSpace(string driveName)
        {
            if (!driveName.EndsWith(":\\"))
                driveName += ":\\";

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    var bytes = drive.AvailableFreeSpace;
                    double mb = bytes / MEM_MB;
                    mb = Math.Floor(mb);
                    return $"{mb} MB free.";
                }
            }

            return "Unknown Free Bytes";
        }

        /// <summary>
        /// Get the total ram available 
        /// </summary>
        /// <returns></returns>
        public static string GetTotalRAM()
        {
            double ram = 0;

            try
            {
                ComputerInfo CI = new ComputerInfo();
                ulong mem = ulong.Parse(CI.TotalPhysicalMemory.ToString());
                return (mem / (1024 * 1024) + " MB").ToString();
            }
            catch
            {
                return "Unknown RAM";
            }
        }

        /// <summary>
        /// Get the CPU Information from the registry
        /// This procedure works for any OS
        /// </summary>
        /// <returns></returns>
        public static string GetCPUInfo()
        {
            string key = "";
            string results = "";
            RegistryKey oKey;
            string MHZ;

            try
            {
                //Try to get the Processor Name String
                key = "HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0";
                oKey = Registry.LocalMachine.OpenSubKey(key, false);

                try
                {
                    if (oKey != null)
                        results = oKey.GetValue("ProcessorNameString", "").ToString().Trim();
                }
                catch
                {
                }

                if (results.Length == 0)
                {

                    try
                    {
                        if (oKey != null)
                            results = oKey.GetValue("Identifier", "").ToString();
                    }
                    catch
                    {
                    }

                    try
                    {
                        if (oKey != null)
                        {
                            MHZ = oKey.GetValue("~MHz", "").ToString();
                            if (MHZ.Length > 0)
                            {
                                results += ", " + MHZ + " Mhz";
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            if (results.Length == 0)
                results = "Unknown";

            return results;
        }

        /// <summary>
        /// Get the operating System version 
        /// </summary>
        /// <returns></returns>
        public static string GetOSVersion()
        {
            string results = "Unknown OS: " + System.Environment.OSVersion.ToString();

            try
            {
                // Get OperatingSystem information from the system namespace.
                System.OperatingSystem osInfo = System.Environment.OSVersion;

                // Determine the platform.
                switch (osInfo.Platform)
                {
                    //Platform is a Windows CE device
                    case System.PlatformID.WinCE:
                        results = "Windows CE";
                        break;

                    // Platform is Windows 95, Windows 98, 
                    // Windows 98 Second Edition, or Windows Me.
                    case System.PlatformID.Win32Windows:
                        switch (osInfo.Version.Minor)
                        {
                            case 0:
                                results = "Windows 95";
                                break;
                            case 10:
                                if (osInfo.Version.Revision.ToString() == "2222A")
                                    results = "Windows 98 Second Edition";
                                else
                                    results = "Windows 98";
                                break;
                            case 90:
                                results = "Windows Me";
                                break;
                        }
                        break;

                    // Platform is Windows NT 3.51, Windows NT 4.0, Windows 2000,
                    // or Windows XP.
                    case System.PlatformID.Win32NT:
                        results = GetOperatingSystemFromRegistry();
                        break;
                }

                return results;
            }
            catch
            {
                return results;
            }
        }

        public static string GetServicePack()
        {
            try
            {
                OperatingSystem os = Environment.OSVersion;
                return os.ServicePack;
            }
            catch (Exception)
            {
                return "Unknown Service Pack";
            }
        }


        private static string HKLM_GetString(string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(path);
                if (rk == null) return "";
                return (string)rk.GetValue(key);
            }
            catch { return ""; }
        }

        private static string GetOperatingSystemFromRegistry()
        {
            string productName = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");
            string csdVersion = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CSDVersion");
            if (productName != "")
            {
                return (productName.StartsWith("Microsoft") ? "" : "Microsoft ") + productName +
                       (csdVersion != "" ? " " + csdVersion : "");
            }
            return "";
        }



    }
}
