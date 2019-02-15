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
        #region Class Variables
        private readonly object _locker = new object();

        /// <summary>
        /// Name for the global section
        /// </summary>
        public const string GLOBAL_SECTION_NAME = "GLOBAL";
        #endregion

        #region Constructors
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
        #endregion

        #region Properties
        /// <summary>
        /// Override the file encoding.  The default is Encoding.Default.<br/>
        /// See http://msdn.microsoft.com/en-us/library/system.text.encoding.aspx
        /// </summary>
        public Encoding FileEncoding { get; set; }

        /// <summary>
        /// If true, escape characters will be processed.  The default is false.<br/>
        /// See http://en.wikipedia.org/wiki/INI_file
        /// </summary>
        public bool IsProcessEscapeCharactersEnabled { get; set; }

        /// <summary>
        /// The comment character.  By default it is a semicolon
        /// </summary>
        public string CommentCharacter { get; set; }

        /// <summary>
        /// The delimiter between settings and values.  By default it is an equals sign
        /// </summary>
        public string Delimiter { get; set; }
        #endregion

        #region Managed Version of INI Functions


        /// <summary>
        /// Managed version of GetPrivateProfileString<br/>
        /// No COM Interop is used<br/>
        /// If the file does not exist or the value is not in the file, the defaultValue is used.<br/>
        /// See:  http://msdn.microsoft.com/en-us/library/windows/desktop/ms724348%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="sectionName">The INI Section Name. Use GLOBAL for the global section.</param>
        /// <param name="settingName">The INI Setting Name</param>
        /// <param name="defaultValue">The default value if there is no value</param>
        /// <param name="returnedString">Output of the string</param>
        /// <param name="size">The number of buffer characters (not used but here for backward compatibility)</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <returns>Number of characters returned</returns>
        /// <exception caption="" cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        public static int GetPrivateProfileString(string sectionName, string settingName, string defaultValue, out string returnedString, int size, string filePath)
        {
            IniReaderWriter iniReaderWriter = new IniReaderWriter();
            returnedString = iniReaderWriter.GetSetting(sectionName, settingName, filePath) ?? defaultValue;

            if (returnedString == null)
                return 0;

            return returnedString.Length;
        }

        /// <summary>
        /// Managed version of GetPrivateProfileString<br/>
        /// No COM Interop is used<br/>
        /// If the file does not exist or the value is not in the file, the defaultValue is used.<br/>
        /// See:  http://msdn.microsoft.com/en-us/library/windows/desktop/ms724348%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="sectionName">The INI Section Name.  Use GLOBAL for the global section.</param>
        /// <param name="settingName">The INI Setting Name</param>
        /// <param name="defaultValue">The default value if there is no value</param>
        /// <param name="returnedBuffer">StrinbBuilder Output of the string</param>
        /// <param name="size">The number of buffer characters (not used but here for backward compatibility)</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <returns>Number of characters returned</returns>
        /// <exception cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        public static int GetPrivateProfileString(string sectionName, string settingName, string defaultValue, StringBuilder returnedBuffer, int size, string filePath)
        {
            IniReaderWriter iniReaderWriter = new IniReaderWriter();

            string result = iniReaderWriter.GetSetting(sectionName, settingName, filePath) ?? defaultValue;

            if (result == null)
            {
                return 0;
            }

            returnedBuffer.Append(result);
            return result.Length;
        }

        /// <summary>
        /// Managed version of WritePrivateProfileString<br/>
        /// No COM Interop is used<br/>
        /// If the file does not exist it will be created.  If the section does not exist it will be created.  If the setting already exists it will be updated.  If the setting does not exist, it will be added.<br/>
        /// See:  http://msdn.microsoft.com/en-us/library/windows/desktop/ms724348%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="sectionName">The INI Section Name. Use GLOBAL for the global section.</param>
        /// <param name="settingName">The INI Setting Name</param>
        /// <param name="settingValue">The INI Setting Value</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <exception cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        /// <returns>True if the setting was set successfully</returns>
        /// <exception caption="" cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        public static bool WritePrivateProfileString(string sectionName, string settingName, string settingValue, string filePath)
        {
            IniReaderWriter iniReaderWriter = new IniReaderWriter();
            return iniReaderWriter.SaveSetting(sectionName, settingName, settingValue, filePath);
        }

        /// <summary>
        /// Managed version of GetPrivateProfileInt<br/>
        /// No COM Interop is used<br/>
        /// If the file does not exist or the value is not in the file, the defaultValue is used.<br/>
        /// See:  http://msdn.microsoft.com/en-us/library/windows/desktop/ms724348%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="sectionName">The INI Section Name. Use GLOBAL for the global section.</param>
        /// <param name="settingName">The INI Setting Name</param>
        /// <param name="defaultValue">The default value if there is no value</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <returns>The integer</returns>
        /// <exception caption="" cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        public static int GetPrivateProfileInt(string sectionName, string settingName, int defaultValue, string filePath)
        {
            int result = defaultValue;
            IniReaderWriter iniReaderWriter = new IniReaderWriter();

            string value = iniReaderWriter.GetSetting(sectionName, settingName, filePath);

            if (String.IsNullOrEmpty(value))
                return result;

            Int32.TryParse(value, out result);
            return result;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a list of the sections in an INI file. The global section will be returned as GLOBAL.  See also GLOBAL_SECTION_NAME<br/>
        /// No COM Interop is used<br/>
        /// If the file does not exist, no sections will be returned.
        /// </summary>
        /// <param name="filePath">The path to the INI file</param>
        /// <returns>A list of the section names</returns>
        /// <exception caption="" cref="ArgumentNullException">Occurs when filePath is null</exception>
        public List<string> GetSectionNames(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            if (!File.Exists(filePath))
                return new List<string>();

            List<string> result = new List<string>();
            List<string> lines = ReadLines(filePath);
            bool namedSectionFound = false;
            bool globalSectionAdded = false;

            foreach (var line in lines)
            {
                if (String.IsNullOrEmpty(line) || line.StartsWith(CommentCharacter))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    result.Add(line.Substring(1, line.Length - 2));
                    namedSectionFound = true;
                }
                else if (!namedSectionFound && !globalSectionAdded)
                {
                    result.Add(GLOBAL_SECTION_NAME);
                    globalSectionAdded = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Get a dictionary of the key value pairs for a section in an INI file<br/>
        /// No COM Interop is used<br/>
        /// If the file does not exist, no items will be returned.
        /// </summary>
        /// <param name="sectionName">The section name.  Use GLOBAL for the global section.</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <returns>A list of the settings and their values</returns>
        /// <exception caption="" cref="ArgumentNullException">Occurs when sectionName, or filePath is null</exception>
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

            bool inGlobalSection = sectionName == GLOBAL_SECTION_NAME;
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
                    inGlobalSection = false;
                }
                //We found the setting in the section
                else if (inDesiredSection || inGlobalSection)
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
        /// Return true if a setting exists<br/>
        /// No COM Interop is used<br/>
        /// If the file does not exist or the setting does not exist, the value returned will be false.
        /// </summary>
        /// <param name="sectionName">The section name.  Use GLOBAL for the global section.</param>
        /// <param name="settingName">The setting name</param>
        /// <param name="filePath">The path to the INI File</param>
        /// <returns></returns>
        /// <exception caption="" cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
        public bool SettingExists(string sectionName, string settingName, string filePath)
        {
            return GetSetting(sectionName, settingName, filePath) != null;
        }

        /// <summary>
        /// Get the value for a setting in a section<br/>
        /// No COM Interop is used<br/>
        /// If the file does not exist or the setting does not exist, the value returned will be null.
        /// </summary>
        /// <param name="sectionName">The section name.  Use GLOBAL for the global section.</param>
        /// <param name="settingName">The setting name</param>
        /// <param name="filePath">The path to the INI File</param>
        /// <returns></returns>
        /// <exception caption="" cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
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
        /// Save a value to an INI file<br/>
        /// No COM Interop is used<br/>
        /// If the file does not exist it will be created.  If the section does not exist it will be created.  If the setting already exists it will be updated.  If the setting does not exist, it will be added.<br/></summary>
        /// <param name="sectionName">The section name. Use GLOBAL for the global section.</param>
        /// <param name="settingName">The setting name</param>
        /// <param name="settingValue">The value to set</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <returns>True if it was saved successfully</returns>
        /// <exception caption="" cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
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
        /// Delete a setting from an INI file<br/>
        /// No COM Interop is used<br/>
        /// If the file does not exist or the value does not exist, false will be returned.
        /// </summary>
        /// <param name="sectionName">The name of the section.  Use GLOBAL for the global section.</param>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="filePath">The path to the INI file</param>
        /// <returns>True if the setting was deleted</returns>
        /// <exception caption="" cref="ArgumentNullException">Occurs when sectionName, settingName or filePath is null</exception>
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
            if (sectionName == GLOBAL_SECTION_NAME)
                AddGlobalSectionSetting(lines, settingName, settingValue);
            else
                AddNamedSectionSetting(lines, sectionName, settingName, settingValue);
        }



        private void AddGlobalSectionSetting(List<string> lines, string settingName, string settingValue)
        {
            string newSetting = string.Format("{0}{1}{2}", settingName, Delimiter, settingValue);

            //Empty File
            if (lines.Count == 0)
            {
                lines.Add(newSetting);
                return;
            }

            //No global section, add it at the top
            if (!IsThereAGlobalSection(lines))
            {
                lines.Insert(0,string.Empty);
                lines.Insert(0, newSetting);
                return;
            }

            string lowerSettingName = settingName.ToLower();
            int lastSetting = 0;

            //We have a global section, look for the value
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                string lowerLine = line.ToLower();

                //This is an update
                //There is an equals after the setting name
                //The setting name matches after it is trimmed
                if (lowerLine.StartsWith(lowerSettingName)
                    && lowerLine.IndexOf(Delimiter, lowerSettingName.Length) > 0
                    && lowerLine.Substring(0, lowerLine.IndexOf(Delimiter)).Trim() == lowerSettingName)
                {
                    lines[i] = newSetting;
                    return;
                }

                //Skip blank lines and comments
                if (String.IsNullOrEmpty(line) || line.StartsWith(CommentCharacter))
                {
                    continue;
                }

                //We have hit a section name
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    //Add a blank line if there is not one after the global section
                    if (!string.IsNullOrEmpty(lines[lastSetting+1]))
                        lines.Insert(lastSetting + 1, string.Empty);

                    lines.Insert(lastSetting + 1, newSetting);
                    break;
                }

                //If we are here, we are on a valid setting
                lastSetting = i;
            }
        }

        private bool IsThereAGlobalSection(List<string> lines)
        {
            foreach (var line in lines)
            {
                //Skip blank lines and comments
                if (String.IsNullOrEmpty(line) || line.StartsWith(CommentCharacter))
                {
                    continue;
                }

                if (lines[0].StartsWith("[") && lines[0].EndsWith("]"))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private void AddNamedSectionSetting(List<string> lines, string sectionName, string settingName, string settingValue)
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
                    && lowerLine.IndexOf(Delimiter, lowerSettingName.Length) > 0
                    && lowerLine.Substring(0, lowerLine.IndexOf(Delimiter)).Trim() == lowerSettingName)
                {
                    lines[i] = newSetting;
                    return;
                }

                //Add the setting before the first blank line or before the next section
                if (inDesiredSection
                    && (String.IsNullOrEmpty(line)
                        || (line.StartsWith("[") && line.EndsWith("]"))))
                {
                    lines.Insert(i, newSetting);
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
                lines.Add(string.Format("[{0}]", sectionName));

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
            bool inGlobalSection = sectionName == GLOBAL_SECTION_NAME;

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
                    inGlobalSection = false;
                }

                //We found the setting in the section
                //There is an equals after the setting name
                //The setting name matches after it is trimmed
                else if ((inDesiredSection || inGlobalSection)
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
