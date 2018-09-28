using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using Microsoft.Win32;
//using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KellermanSoftware.Common
{
    public static class SystemInfo
    {
        #region Class Variables
        const double MEM_KB = 1024;
        const double MEM_MB = MEM_KB * MEM_KB;

        [DllImport("kernel32"), SecurityCritical]
        private static extern long GetDiskFreeSpaceEx(string sDrive,
            ref long iFreeBytes,
            ref long iTotalBytes,
            ref long iTotalFree);

        #endregion

        /// <summary>
        /// Get the free Megabytes for the passed drive letter
        /// This procedure works with Windows 98,ME,2000,XP and above
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
		[SecuritySafeCritical]
        public static string GetFreeSpace(string drive)
        {
            double MB;
            string results = "Unknown Free Bytes";
            drive = drive.Replace("\\", "");
            long safeResults;
            long freeBytes = 0;
            long totalBytes = 0;
            long totalFree = 0;

            try
            {
                System.Management.ManagementObjectCollection myMOC;
                myMOC = new System.Management.ManagementObjectSearcher(new System.Management.SelectQuery("SELECT FreeSpace FROM Win32_LogicalDisk WHERE deviceID = '" + drive + "'")).Get();

                foreach (System.Management.ManagementObject myMO in myMOC)
                {
                    MB = ConvertUtil.cDbl(myMO.Properties["FreeSpace"].Value.ToString());
                    MB = MB / MEM_MB;
                    MB = Math.Floor(MB);

                    results = MB.ToString("n0") + " MB free.";

                    break;
                }
            }
            catch
            {
                try
                {
                    safeResults = GetDiskFreeSpaceEx(drive + "\\", ref freeBytes, ref totalBytes, ref totalFree);

                    if (safeResults != 0)
                    {
                        MB = freeBytes;
                        MB = MB / MEM_MB;
                        MB = Math.Floor(MB);

                        results = MB.ToString("n0") + " MB free.";
                    }
                }
                catch
                {
                }
            }

            return results;
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
                System.Management.ObjectQuery myQuery = new System.Management.ObjectQuery("SELECT * FROM Win32_PhysicalMemory");
                System.Management.ManagementObjectSearcher mySearcher = new System.Management.ManagementObjectSearcher(myQuery);

                foreach (System.Management.ManagementObject myObject in mySearcher.Get())
                {
                    ram += ConvertUtil.cDbl(myObject["Capacity"].ToString());
                }

                if (ram == 0)
                    return "Unknown RAM";
                else
                    return Convert.ToString(ram / MEM_MB) + " MB";
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
            string results = "Unknown OS";

            try
            {
                System.Management.ObjectQuery myQuery = new System.Management.ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                System.Management.ManagementObjectSearcher mySearcher = new System.Management.ManagementObjectSearcher(myQuery);

                foreach (System.Management.ManagementObject myObject in mySearcher.Get())
                {
                    results = myObject["caption"].ToString();
                }

                return results + ", " + GetServicePack();
            }
            catch
            {
                return GetOSVersionSafe();
            }
        }

        /// <summary>
        /// Get the operating system of the current computer
        /// </summary>
        /// <returns></returns>
        private static string GetOSVersionSafe()
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
                        switch (osInfo.Version.Major)
                        {
                            case 3:
                                results = "Windows NT 3.51";
                                break;
                            case 4:
                                results = "Windows NT 4.0";
                                break;
                            case 5:
                                if (osInfo.Version.Minor == 0)
                                    results = "Windows 2000";
                                else if (osInfo.Version.Minor == 1)
                                    results = "Windows XP";
                                else if (osInfo.Version.Minor == 2)
                                    results = "Windows Server 2003";
                                break;
                        }
                        break;
                }

                return results + ", " + GetServicePack();
            }
            catch
            {
                return results;
            }
        }

        /// <summary>
        /// Get the service pack installed on the current computer
        /// This WMI procedure works for Windows 2000 and above
        /// </summary>
        /// <returns></returns>
        public static string GetServicePack()
        {
            string results = "Unknown Service Pack";

            try
            {
                System.Management.ObjectQuery myQuery = new System.Management.ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                System.Management.ManagementObjectSearcher mySearcher = new System.Management.ManagementObjectSearcher(myQuery);

                foreach (System.Management.ManagementObject myObject in mySearcher.Get())
                {
                    results = "Service Pack " + myObject["ServicePackMajorVersion"].ToString() + "." + myObject["ServicePackMinorVersion"].ToString();
                }
            }
            catch
            {
            }

            return results;
        }

    }
}
