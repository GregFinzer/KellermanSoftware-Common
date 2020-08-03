using System;
using System.Diagnostics;
using System.Text;
using System.Web;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// Helper methods for dealing with processes
    /// </summary>
    public static class ProcessUtil
    {
        /// <summary>
        /// Returns true if we are running in medium trust
        /// </summary>
        /// <returns></returns>
        public static bool IsMediumTrustOrLess()
        {
            AspNetHostingPermissionLevel permissionLevel = GetCurrentTrustLevel();

            return permissionLevel == AspNetHostingPermissionLevel.Medium
                || permissionLevel == AspNetHostingPermissionLevel.Low
                || permissionLevel == AspNetHostingPermissionLevel.Minimal
                || permissionLevel == AspNetHostingPermissionLevel.None;
        }

        /// <summary>
        /// Get the current trust level
        /// </summary>
        /// <returns></returns>
        public static AspNetHostingPermissionLevel GetCurrentTrustLevel()
        {
            foreach (AspNetHostingPermissionLevel trustLevel in
                    new AspNetHostingPermissionLevel[] {
            AspNetHostingPermissionLevel.Unrestricted,
            AspNetHostingPermissionLevel.High,
            AspNetHostingPermissionLevel.Medium,
            AspNetHostingPermissionLevel.Low,
            AspNetHostingPermissionLevel.Minimal 
        })
            {
                try
                {
                    new AspNetHostingPermission(trustLevel).Demand();
                }
                catch (System.Security.SecurityException)
                {
                    continue;
                }

                return trustLevel;
            }

            return AspNetHostingPermissionLevel.None;
        }

        /// <summary>
        /// Launch the associated email application
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public static void LaunchEmail(string to, string subject, string body)
        {
            const int maxHTMLGetOperationCharacters = 2083;

            System.Text.StringBuilder sb = new StringBuilder(2083);

            sb.Append("mailto:");
            sb.Append(to);
            sb.Append("?Subject=");
            sb.Append(StringUtil.URLEscape(subject));
            sb.Append("&body=");
            sb.Append(StringUtil.URLEscape(body));

            string link = StringUtil.Left(sb.ToString(), maxHTMLGetOperationCharacters);
            System.Diagnostics.Process.Start(link);
        }

        /// <summary>
        /// Runs the passed executable
        /// </summary>
        /// <param name="processExecutableFilePath">Path to the process executable.</param>
        /// <param name="arguments">(Optional) The arguments.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">arguments</exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void ShellCatchErrors(string processExecutableFilePath, string arguments)
        {
            arguments = arguments ?? "";
            if (string.IsNullOrEmpty(processExecutableFilePath))
            {
                throw new ArgumentNullException("processExecutableFilePath");
            }

            var process = new Process();
            process.StartInfo.FileName = processExecutableFilePath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = arguments;
            //process.StartInfo.Verb = "runas";
            process.Start();

            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();
            int result = process.ExitCode;
            process.Close();
            if (result != 0)
            {
                throw new InvalidOperationException(error);
            }
        }

        /// <summary>
        /// Execute an external program.
        /// </summary>
        /// <param name="sExecutablePath">Path and filename of the executable.</param>
        /// <param name="sArguments">Arguments to pass to the executable.</param>
        /// <param name="myWindowStyle">Window style for the process (hidden, minimized, maximized, etc).</param>
        /// <param name="bWaitUntilFinished">Wait for the process to finish.</param>
        /// <returns>Exit Code</returns>
        public static int Shell(string sExecutablePath, string sArguments, ProcessWindowStyle myWindowStyle, bool bWaitUntilFinished)
        {
            string sFileName = "";

            try
            {
                bool bDebug = false;
                Process p = new Process();
                string sAssemblyPath = FileUtil.PathSlash(FileUtil.GetCurrentDirectory()) + FileUtil.ExtractFileName(sExecutablePath);

                //Look for the file in the executing assembly directory
                if (FileUtil.FileExists(sAssemblyPath))
                {
                    sFileName = sAssemblyPath;
                    p.StartInfo.FileName = sAssemblyPath;
                }
                else // if there is no path to the file, an error will be thrown
                {
                    sFileName = sExecutablePath;
                    p.StartInfo.FileName = sExecutablePath;
                }

                p.StartInfo.Arguments = sArguments;

                if (bDebug)
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                }
                else
                {
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.WindowStyle = myWindowStyle;
                }

                //Start the Process
                p.Start();

                if (bWaitUntilFinished)
                {
                    p.WaitForExit();

                    while (!p.HasExited)
                        System.Threading.Thread.Sleep(500);
                }

                if (bWaitUntilFinished == true)
                    return p.ExitCode;
                else
                    return 0;
            }
            catch
            {
                string sMsg = "Shell Fail:  " + sFileName + "\n";
                sMsg += "Arguments:  " + sArguments;
                throw new Exception(sMsg);
            }
        }
    }
}
