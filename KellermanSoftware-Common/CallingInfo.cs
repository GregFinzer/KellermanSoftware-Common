namespace KellermanSoftware.Common
{
    /// <summary>
    /// Information about the calling method
    /// </summary>
    public class CallingInfo
    {
        /// <summary>
        /// The name of the assembly the method is in
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// The class name of the method
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// The namespace of the method
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// The line number where the call was made from
        /// </summary>
        public string LineNumber { get; set; }

        /// <summary>
        /// The name of the method
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// The file name of the assembly
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The assembly version
        /// </summary>
        public string AssemblyVersion { get; set; }
    }
}
