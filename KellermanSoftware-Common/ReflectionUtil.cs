using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;


namespace KellermanSoftware.Common
{
    /// <summary>
    /// Reflection helping methods
    /// </summary>
    public static class ReflectionUtil
    {
        /// <summary>
        /// Get the calling method name of the caller
        /// </summary>
        /// <returns></returns>
        public static CallingInfo GetCallingInfo()
        {
            CallingInfo info = new CallingInfo();

            List<string> callingClasses = new List<string>()
            {
                "KellermanSoftware.Common.ReflectionUtil",
                "KellermanSoftware_Common_WPF.GuiException",
                "KellermanSoftware.Common.Gui.GuiException"
            };

            StackFrame frame = GetCallingStackFrame(callingClasses);

            if (frame != null)
            {
                MethodBase methodBase = frame.GetMethod();
                info.FileName = frame.GetFileName();

                if (methodBase.ReflectedType != null)
                {
                    info.AssemblyName =
                        System.IO.Path.GetFileNameWithoutExtension(
                            methodBase.ReflectedType.Assembly.ManifestModule.ScopeName);

                    info.AssemblyVersion = methodBase.ReflectedType.Assembly.GetName().Version.ToString();

                    info.ClassName = methodBase.ReflectedType.Name;
                    info.Namespace = methodBase.ReflectedType.Namespace;
                }

                info.LineNumber = frame.GetFileLineNumber().ToString(CultureInfo.InvariantCulture);
                info.MethodName = methodBase.Name;
                
            }

            return info;
        }

        /// <summary>
        /// Get the stack frame of the calling method
        /// </summary>
        /// <param name="excludeCallingClass"></param>
        /// <returns></returns>
        private static StackFrame GetCallingStackFrame(List<string> excludeCallingClass)
        {
            StackTrace stackTrace = new StackTrace(0, true);
            int frameIndex = 0;

            while (frameIndex < stackTrace.FrameCount)
            {
                StackFrame frame = stackTrace.GetFrame(frameIndex);
                var declaringType = frame.GetMethod().DeclaringType;
                if (declaringType != null && excludeCallingClass.Contains(declaringType.FullName))
                {
                    break;
                }
                frameIndex++;
            }

            while (frameIndex < stackTrace.FrameCount)
            {
                StackFrame frame = stackTrace.GetFrame(frameIndex);
                var declaringType = frame.GetMethod().DeclaringType;
                if (declaringType != null && !excludeCallingClass.Contains(declaringType.FullName))
                {
                    break;
                }
                frameIndex++;
            }

            if (frameIndex < stackTrace.FrameCount)
            {
                return stackTrace.GetFrame(frameIndex);
            }

            return null;
        }

    }
}
