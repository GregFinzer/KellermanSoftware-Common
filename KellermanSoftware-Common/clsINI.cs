using System.Text;


namespace KellermanSoftware.Common
{
    /// <summary>
    /// Work with a specific INI file
    /// </summary>
	public class IniFile
	{
        private bool _iniPathWritable = true;
        private bool _iniPathChecked = false;
		private string _path;


        /// <summary>
        /// Returns the path to the ini file
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                if (_path != value)
                {
                    _iniPathChecked = false;
                }

                _path = value;
                
            }
        }

        /// <summary>
        /// Returns True if the INI path is writable
        /// </summary>
        public bool INIPathWritable
        {
            get
            {
                CheckINIPath();
                return _iniPathWritable;
            }
        }

		/// <summary>
		/// INIFile Constructor.
		/// </summary>
		/// <PARAM name="INIPath">File Path to the INI File</PARAM>
		public IniFile(string INIPath)
		{
			_path = INIPath;
		}
		
		/// <summary>
		/// Write Data to the INI File
		/// </summary>
		/// <param name="section">Section Name</param>
		/// <param name="key">Key Name</param>
		/// <param name="value">Value Name</param>
		public bool IniWriteValue(string section,string key,string value)
		{
            try
            {
                CheckINIPath();

                if (_iniPathWritable == false)
                {
                    return false;
                }

                IniReaderWriter.WritePrivateProfileString(section, key, value, Path);
            }
            catch
            {
                return false;
            }

            return true;    
		}
        
        /// <summary>
        /// Ensure the directory exists for the 
        /// </summary>
        private void CheckINIPath()
        {
            try
            {
                if (_iniPathChecked == true)
                {
                    return;
                }

                if (_path.Length == 0)
                {
                    _iniPathWritable = false;
                }
                else
                {
                    string directory = FileUtil.ExtractPath(_path);

                    //Create the directory if it does not exist
                    if (System.IO.Directory.Exists(directory) == false)
                    {
                        System.IO.Directory.CreateDirectory(directory);
                    }

                    _iniPathWritable = FileUtil.Writable(directory);
                }
            }
            catch //This is a failure when we could not create the directory
            {
                _iniPathWritable = false;
            }

            _iniPathChecked = true;


        }

		/// <summary>
		/// Read Data from an INI File
		/// </summary>
		/// <param name="section">Section Name</param>
		/// <param name="key">Key Name</param>
		/// <returns>Value</returns>
		public string IniReadValue(string section,string key)
		{
            try
            {
                StringBuilder temp = new StringBuilder(255);
                IniReaderWriter.GetPrivateProfileString(section, key, string.Empty, temp, 255, Path);
                return temp.ToString();
            }
            catch
            {
                return string.Empty;
            }
		}
	}
}