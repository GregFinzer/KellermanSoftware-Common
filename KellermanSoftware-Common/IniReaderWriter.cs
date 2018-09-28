using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// Fully Managed INI File Reader and Writer without any COM Interop
    /// See:  http://en.wikipedia.org/wiki/INI_file
    /// See:  http://msdn.microsoft.com/en-us/library/windows/desktop/ms724348%28v=vs.85%29.aspx
    /// </summary>
    public class IniReaderWriter
    {
        private readonly object _locker = new object();

        public IniReaderWriter()
        {
            Setup();
        }

        private void Setup()
        {
            FileEncoding = Encoding.Default;
            CommentCharacter = ";";
            Delimiter = "=";
        }

        /// <summary>
        /// Override the file encoding.  The default is Encoding.Default
        /// </summary>
        public Encoding FileEncoding { get; set; }

        /// <summary>
        /// If true, escape characters will be processed.  The default is false.
        /// See http://en.wikipedia.org/wiki/INI_file
        /// </summary>
        public bool IsProcessEscapeCharactersEnabled { get; set; }

        /// <summary>
        /// The comment character.  By default it is a semicolon
        /// </summary>
        public string CommentCharacter { get; set; }

        /// <summary>
        /// The delimiter between settings and values.  By default it is a equals sign
        /// </summary>
        public string Delimiter { get; set; }

        #region Managed Version of INI Functions


        /// <summary>
        /// Managed version of GetPrivateProfileString<br />
        /// No COM Interop is used<br />
        /// If the file does not exist or the value is not in the file, the defaultValue is used.<br />
        /// See:  http://msdn.microsoft.com/en-us/library/windows/desktop/ms724348%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="sectionName">The INI Section Name</param>
        /// <param name="settingName">The INI Setting Name</param>
        /// <param name="defaultValue">The default value if there is no value</param>
        /// <param name="returnedString">Output of the string</param>
        /// <param name="size">The number of buffer characters (not used but here for backward compatibility)</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <exception cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        /// <returns>Number of characters returned</returns>
        public int GetPrivateProfileString(string sectionName, string settingName, string defaultValue, out string returnedString, int size, string filePath)
        {            
            returnedString= GetSetting(settingName, settingName, filePath) ?? defaultValue;

            if (returnedString == null)
                return 0;

            return returnedString.Length;
        }

        /// <summary>
        /// Managed version of GetPrivateProfileString<br />
        /// No COM Interop is used<br />
        /// If the file does not exist or the value is not in the file, the defaultValue is used.<br />
        /// See:  http://msdn.microsoft.com/en-us/library/windows/desktop/ms724348%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="sectionName">The INI Section Name</param>
        /// <param name="settingName">The INI Setting Name</param>
        /// <param name="defaultValue">The default value if there is no value</param>
        /// <param name="returnedBuffer">StrinbBuilder Output of the string</param>
        /// <param name="size">The number of buffer characters (not used but here for backward compatibility)</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <exception cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        /// <returns>Number of characters returned</returns>
        public int GetPrivateProfileString(string sectionName, string settingName, string defaultValue, StringBuilder returnedBuffer, int size, string filePath)
        {
            string result = GetSetting(settingName, settingName, filePath) ?? defaultValue;

            if (result == null)
            {
                return 0;
            }

            returnedBuffer.Append(result);
            return result.Length;
        }

        /// <summary>
        /// Managed version of WritePrivateProfileString<br />
        /// No COM Interop is used<br />
        /// If the file does not exist it will be created.  If the section does not exist it will be created.  If the setting already exists it will be updated.  If the setting does not exist, it will be added.<br />
        /// See:  http://msdn.microsoft.com/en-us/library/windows/desktop/ms724348%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="sectionName">The INI Section Name</param>
        /// <param name="settingName">The INI Setting Name</param>
        /// <param name="settingValue">The INI Setting Value</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <exception cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        /// <returns>True if the setting was set successfully</returns>
        public bool WritePrivateProfileString(string sectionName, string settingName, string settingValue, string filePath)
        {
            return SaveSetting(sectionName, sectionName, settingValue, filePath);
        }

        /// <summary>
        /// Managed version of GetPrivateProfileInt<br />
        /// No COM Interop is used<br />
        /// If the file does not exist or the value is not in the file, the defaultValue is used.<br />
        /// See:  http://msdn.microsoft.com/en-us/library/windows/desktop/ms724348%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="sectionName">The INI Section Name</param>
        /// <param name="settingName">The INI Setting Name</param>
        /// <param name="defaultValue">The default value if there is no value</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <exception cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        /// <returns>The integer</returns>
        public int GetPrivateProfileInt(string sectionName, string settingName, int defaultValue, string filePath)
        {
            int result = defaultValue;
            string value = GetSetting(settingName, settingName, filePath);

            if (String.IsNullOrEmpty(value))
                return result;

            Int32.TryParse(value, out result);
            return result;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a list of the sections in an INI file<br />
        /// No COM Interop is used<br />
        /// If the file does not exist, no sections will be returned.
        /// </summary>
        /// <param name="filePath">The path to the INI file</param>
        /// <exception cref="ArgumentNullException">Occurs when filePath is null</exception>
        /// <returns>A list of the section names</returns>
        public List<string> GetSectionNames(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            if (!File.Exists(filePath))
                return new List<string>();

            List<string> result = new List<string>();
            List<string> lines = ReadLines(filePath);

            foreach (var line in lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                    result.Add(line.Substring(1,line.Length-2));
            }

            return result;
        }

        /// <summary>
        /// Get a dictionary of the key value pairs for a section in an INI file<br />
        /// No COM Interop is used<br />
        /// If the file does not exist, no items will be returned.
        /// </summary>
        /// <param name="sectionName">The section name</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <exception cref="ArgumentNullException">Occurs when sectionName, or filePath is null</exception>
        /// <returns>A list of the settings and their values</returns>
        public Dictionary<string, string> GetSectionValues(string sectionName, string filePath)
        {
            if (string.IsNullOrEmpty(sectionName))
                throw new ArgumentNullException("sectionName");

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            if (!File.Exists(filePath))
                return new Dictionary<string, string>();

            Dictionary<string, string> result = new Dictionary<string, string>();
            List<string> lines = ReadLines(filePath);

            bool inDesiredSection = false;
            string lowerSectionName = "[" + sectionName.ToLower() + "]";

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                //Skip blank lines and comments
                if (String.IsNullOrEmpty(line) || line.StartsWith(CommentCharacter))
                    continue;

                string lowerLine = line.ToLower();

                //We found a section
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    inDesiredSection = lowerLine == lowerSectionName;
                }
                //We found the setting in the section
                else if (inDesiredSection)
                {
                    int delimiterPosition = line.IndexOf(Delimiter);

                    if (delimiterPosition < 0)
                    {
                        result.Add(line,null);
                    }
                    else
                    {
                        result.Add(line.Substring(0,delimiterPosition).Trim(),ProcessValue(line.Substring(delimiterPosition+1)));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Return true if a setting exists<br />
        /// No COM Interop is used<br />
        /// If the file does not exist or the setting does not exist, the value returned will be false.
        /// </summary>
        /// <param name="sectionName">The section name</param>
        /// <param name="settingName">The setting name</param>
        /// <param name="filePath">The path to the INI File</param>
        /// <exception cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        /// <returns></returns>
        public bool SettingExists(string sectionName, string settingName, string filePath)
        {
            return GetSetting(sectionName, settingName, filePath) != null;
        }

        /// <summary>
        /// Get the value for a setting in a section<br />
        /// No COM Interop is used<br />
        /// If the file does not exist or the setting does not exist, the value returned will be null.
        /// </summary>
        /// <param name="sectionName">The section name</param>
        /// <param name="settingName">The setting name</param>
        /// <param name="filePath">The path to the INI File</param>
        /// <exception cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        /// <returns></returns>
        public string GetSetting(string sectionName, string settingName, string filePath)
        {
            if (string.IsNullOrEmpty(sectionName))
                throw new ArgumentNullException("sectionName");

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            if (!File.Exists(filePath))
                return null;

            List<string> lines = ReadLines(filePath);
            int index = GetSettingIndex(lines, sectionName, settingName);

            if (index < 0)
                return null;

            return ProcessValue(ParseSetting(lines[index], settingName));
        }

        /// <summary>
        /// Save a value to an INI file<br />
        /// No COM Interop is used<br />
        /// If the file does not exist it will be created.  If the section does not exist it will be created.  If the setting already exists it will be updated.  If the setting does not exist, it will be added.<br />
        /// </summary>
        /// <param name="sectionName">The section name</param>
        /// <param name="settingName">The setting name</param>
        /// <param name="settingValue">The value to set</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <exception cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        /// <returns>True if it was saved successfully</returns>
        public bool SaveSetting(string sectionName, string settingName, string settingValue, string filePath)
        {
            if (string.IsNullOrEmpty(sectionName))
                throw new ArgumentNullException("sectionName");

            if (string.IsNullOrEmpty(settingName))
                throw new ArgumentNullException("settingName");

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            CreateFileIfItDoesntExist(filePath);                
            List<string> lines = ReadLines(filePath);
            AddSetting(lines, sectionName, settingName, CreateValue(settingValue));
            SaveFile(filePath, lines);
            return true;
        }

        /// <summary>
        /// Delete a setting from an INI file<br />
        /// No COM Interop is used<br />
        /// If the file does not exist or the value does not exist, false will be returned.
        /// </summary>
        /// <param name="sectionName">The name of the section</param>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <exception cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        /// <returns>True if the setting was deleted</returns>
        public bool DeleteSetting(string sectionName, string settingName, string filePath)
        {
            if (string.IsNullOrEmpty(sectionName))
                throw new ArgumentNullException("sectionName");

            if (string.IsNullOrEmpty(settingName))
                throw new ArgumentNullException("settingName");

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            if (!File.Exists(filePath))
                return false;

            List<string> lines = ReadLines(filePath);
            int index = GetSettingIndex(lines, sectionName, settingName);

            if (index < 0)
                return false;

            lines.RemoveAt(index);
            SaveFile(filePath, lines);
            return true;
        }
        #endregion

        #region Private Methods

        private string CreateValue(string settingValue)
        {
            if (String.IsNullOrEmpty(settingValue))
                return settingValue;

            //Preserve space by enclosing in quotes
            if (settingValue.StartsWith(" ") || settingValue.EndsWith(" "))
                settingValue = "\"" + settingValue + "\"";

            return CreateEscapeCharacters(settingValue);
        }

        private string CreateEscapeCharacters(string settingValue)
        {
            if (String.IsNullOrEmpty(settingValue) || !IsProcessEscapeCharactersEnabled)
                return settingValue;

            StringBuilder sb = new StringBuilder(settingValue);

            sb.Replace("\\", @"\\");
            sb.Replace("\0", @"\0");
            sb.Replace("\a", @"\a");
            sb.Replace("\t", @"\t");
            sb.Replace("\r", @"\r");
            sb.Replace("\n", @"\n");
            sb.Replace(";", @"\;");
            sb.Replace("#", @"\#");
            sb.Replace("=", @"\=");
            sb.Replace(":", @"\:");            

            return sb.ToString();
        }

        private string ProcessValue(string settingValue)
        {
            if (String.IsNullOrEmpty(settingValue))
                return settingValue;

            //Trim space by default
            settingValue = settingValue.Trim();

            if (settingValue.StartsWith("\"") && settingValue.EndsWith("\""))
                settingValue = settingValue.Substring(1, settingValue.Length - 2);

            return ProcessEscapeCharacters(settingValue);
        }

        private string ProcessEscapeCharacters(string settingValue)
        {
            if (String.IsNullOrEmpty(settingValue) || !IsProcessEscapeCharactersEnabled)
                return settingValue;

            StringBuilder sb = new StringBuilder(settingValue);

            sb.Replace(@"\0", "\0");
            sb.Replace(@"\a", "\a");
            sb.Replace(@"\t", "\t");
            sb.Replace(@"\r", "\r");
            sb.Replace(@"\n", "\n");
            sb.Replace(@"\;", ";");
            sb.Replace(@"\#", "#");
            sb.Replace(@"\=", "=");
            sb.Replace(@"\:", ":");
            sb.Replace(@"\\", "\\");

            return sb.ToString();
        }

        private void AddSetting(List<string> lines, string sectionName, string settingName, string settingValue)
        {
            bool inDesiredSection = false;

            string lowerSectionName = "[" + sectionName.ToLower() + "]";
            string lowerSettingName = settingName.ToLower();
            string newSetting = string.Format("{0}{1}{2}", settingName, Delimiter, settingValue);

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                string lowerLine = line.ToLower();

                //This is an update
                //There is an equals after the setting name
                //The setting name matches after it is trimmed
                if (inDesiredSection 
                    && lowerLine.StartsWith(lowerSettingName)
                    && lowerLine.IndexOf(Delimiter,lowerSettingName.Length) > 0
                    && lowerLine.Substring(0,lowerLine.IndexOf(Delimiter)).Trim() == lowerSettingName)                
                {
                    lines[i] = newSetting;
                    return;
                }

                //Add the setting before the first blank line or before the next section
                if (inDesiredSection
                    && (String.IsNullOrEmpty(line)
                        || (line.StartsWith("[") && line.EndsWith("]"))))
                {                    
                    lines.Insert(i,newSetting);
                    return;
                }

                //Skip blank lines and comments
                if (String.IsNullOrEmpty(line) || line.StartsWith(CommentCharacter))
                    continue;
                
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    inDesiredSection = lowerLine == lowerSectionName;
                }
            }

            //We found the desired section but we are at the end of the file
            if (inDesiredSection)
            {
                lines.Add(newSetting);
            }
            else //This is a brand new section at the end
            {
                //Only add a blank line if there are other lines
                if (lines.Count > 0)
                    lines.Add(string.Empty);

                //Add the section
                lines.Add(string.Format("[{0}]",sectionName));

                //Add the new setting
                lines.Add(newSetting);
            }
        }

        private void SaveFile(string filePath, List<string> lines)
        {
            lock(_locker)
                File.WriteAllLines(filePath,lines.ToArray());
        }

        private void CreateFileIfItDoesntExist(string filePath)
        {
            lock (_locker)
            {
                if (!File.Exists(filePath))
                {
                    using (new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                    }
                }
            }
        }



        private string ParseSetting(string line, string settingName)
        {
            //There is nothing after the setting
            if (line.IndexOf(Delimiter) + 1 >= line.Length)
                return null;

            string settingValue = line.Substring(line.IndexOf(Delimiter) + 1);
            return settingValue;
        }

        private List<string> ReadLines(string filePath)
        {
            lock (_locker)
                return new List<string>(File.ReadAllLines(filePath, FileEncoding));
        }

        private int GetSettingIndex(List<string> lines, string sectionName, string settingName)
        {
            bool inDesiredSection = false;
            string lowerSectionName = "[" + sectionName.ToLower() + "]";
            string lowerSettingName = settingName.ToLower();

            for (int i=0;i<lines.Count;i++)
            {
                string line = lines[i];

                //Skip blank lines and comments
                if (String.IsNullOrEmpty(line) || line.StartsWith(CommentCharacter))
                    continue;

                string lowerLine = line.ToLower();

                //We found a section
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    inDesiredSection = lowerLine == lowerSectionName;
                }
                //We found the setting in the section
                //There is an equals after the setting name
                //The setting name matches after it is trimmed
                else if (inDesiredSection
                    && lowerLine.StartsWith(lowerSettingName)
                    && lowerLine.IndexOf(Delimiter,lowerSettingName.Length) > 0
                    && lowerLine.Substring(0,lowerLine.IndexOf(Delimiter)).Trim() == lowerSettingName)
                {
                    return i;
                }
            }

            return -1;
        }

        #endregion
    }
}
