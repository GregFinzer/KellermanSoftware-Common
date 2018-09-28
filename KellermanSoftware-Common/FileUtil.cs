using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace KellermanSoftware.Common
{
    public static class FileUtil
    {
        private static readonly object _locker = new object();

		public static void TouchFile(string filePath)
        {
            if (File.Exists(filePath))
                System.IO.File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow);
        }
		
        /// <summary>
        /// Returns true for a UNC Path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsUNCPath(string path)
        {
            return new Uri(path).IsUnc;
        }

        /// <summary>
        /// Get a sum of the file sizes for a path and sub directories
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long GetSumOfFileSizeForPath(string path)
        {
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            long sum = 0;

            foreach (var file in files)
            {
                sum += GetFileSize(file);
            }

            return sum;
        }

        /// <summary>
        /// Get a count of the number of files in a path and sub directories
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static int GetFileCountForPath(string path)
        {
            return Directory.GetFiles(path, "*.*",SearchOption.AllDirectories).Length;
        }

        /// <summary>
        /// Get a list of removable drives that are ready (flash drive, SD Card, but not CD ROM/DVD)
        /// </summary>
        /// <returns></returns>
        public static List<string> GetRemovableDrives()
        {
            List<string> drives = new List<string>();

            foreach (DriveInfo currentDrive in DriveInfo.GetDrives())
            {
                if (currentDrive.DriveType == DriveType.Removable
                    && currentDrive.IsReady)
                {
                    drives.Add(currentDrive.Name);
                }
            }

            return drives;
        }

        /// <summary>
        /// Get the available free space for a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long GetDriveAvailableFreeSpace(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (IsUNCPath(path))
                throw new NotSupportedException("Cannot find free space for UNC path " + path);

            string drive = Path.GetPathRoot(path);

            if (string.IsNullOrEmpty(drive))
                throw new Exception("No drive letter found for: " + path);

            foreach (DriveInfo currentDrive in DriveInfo.GetDrives())
            {
                if (currentDrive.Name ==drive && currentDrive.IsReady)
                {
                    return currentDrive.AvailableFreeSpace;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get the available total size for a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long GetDriveTotalSize(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (IsUNCPath(path))
                throw new NotSupportedException("Cannot find free space for UNC path " + path);

            string drive = Path.GetPathRoot(path);

            if (string.IsNullOrEmpty(drive))
                throw new Exception("No drive letter found for: " + path);

            foreach (DriveInfo currentDrive in DriveInfo.GetDrives())
            {
                if (currentDrive.Name == drive && currentDrive.IsReady)
                {
                    return currentDrive.TotalSize;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get a temporary file
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetTempFileName(string extension)
        {
            return Path.GetTempPath() + Guid.NewGuid() + extension;
        }

	    /// <summary>
        /// Create and return a temporary directory
        /// </summary>
        /// <returns></returns>
        public static string GetTemporaryDirectory()
        {
            string tempDirectoryName = string.Empty
                                       + DateTime.Now.Year
                                       + DateTime.Now.Month
                                       + DateTime.Now.Day
                                       + DateTime.Now.Hour
                                       + DateTime.Now.Minute
                                       + DateTime.Now.Second
                                       + DateTime.Now.Millisecond
                                       + System.Diagnostics.Process.GetCurrentProcess().Id;

            string tempDirectory = Path.Combine(Path.GetTempPath(), tempDirectoryName);
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
		
        /// <summary>
        /// Backup the specified file 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        /// <returns>String.Empty or the file that it was backed up to</returns>
        public static string BackupFile(string sourceFile, string destFile, int numberOfBackups)
        {
            string destPath;
            string destBase;
            string destExt;
            string sourceName;
            string backupName = string.Empty;
            int destNumber;

            if (FileExists(sourceFile) == false)
                return string.Empty;

            destPath = ExtractPath(destFile);
            destBase = System.IO.Path.GetFileNameWithoutExtension(destFile);
            destExt = System.IO.Path.GetExtension(destFile);

            //Create the destination if it does not exist
            if (System.IO.Directory.Exists(destPath) == false)
                System.IO.Directory.CreateDirectory(destPath);

            destPath = PathSlash(destPath);

            //Move around old backups
            for (int i = numberOfBackups - 1; i >= 1; i--)
            {
                sourceName = destPath + destBase + i.ToString().PadLeft(3, '0') + destExt;
                destNumber = i + 1;
                backupName = destPath + destBase + destNumber.ToString().PadLeft(3, '0') + destExt;

                if (FileExists(sourceName))
                {
                    try
                    {
                        System.IO.File.Copy(sourceName, backupName, true);
                    }
                    catch
                    {
                    }
                }
            }

            //Copy from the source to the first backup
            try
            {
                backupName = destPath + destBase + "001" + destExt;
                System.IO.File.Copy(sourceFile, backupName, true);
                return backupName;
            }
            catch
            {
            }

            return string.Empty;
        }

        /// <summary>
        /// Extract Filename from a path
        /// </summary>
        /// <param name="sFullPath">A fully qualified path ending in a filename</param>
        /// <returns>The extracted file name</returns>
        public static string ExtractFileName(string sFullPath)
        {
            return System.IO.Path.GetFileName(sFullPath);
        }

        /// <summary>
        /// Returns true if a word is found in a file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="word"></param>
        /// <param name="caseInsensitive"></param>
        /// <returns></returns>
        public static bool FindWordInFile(string file, string word, bool caseInsensitive)
        {
            string fileText = System.IO.File.ReadAllText(file);

            if (caseInsensitive)
            {
                fileText = fileText.ToLower();
                word = word.ToLower();
            }

            return fileText.Contains(word);
        }

        /// <summary>
        /// Check to see if a file exists
        /// </summary>
        /// <param name="sPath">The file path</param>
        /// <returns>True if it exists</returns>
        public static bool FileExists(string sPath)
        {
            return System.IO.File.Exists(sPath);
        }

        /// <summary>
        /// Returns true if there is a file that exists with the pattern
        /// </summary>
        /// <param name="path"></param>
        /// <param name="wildcard"></param>
        /// <returns></returns>
        public static bool FileExists(string path, string wildcard)
        {            
            string[] files = System.IO.Directory.GetFiles(path, wildcard);
            return files.Length > 0;

        } 
        /// <summary>
        /// Renames the file if one exists on the same folder.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string RenameIfFileExists(string path)
        {
            int i = 1;
            string directoryName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            while (File.Exists(path))
            {
                int lastIndexForOpenBracket = path.LastIndexOf('(');
                int lastIndexForBackSlash = path.LastIndexOf('/');
                if (fileName.LastIndexOf('(') > 0)
                {
                    fileName = fileName.Remove(fileName.LastIndexOf('('));
                    fileName = fileName + "(" + i++ + ")";
                    path = Path.Combine(directoryName, fileName) + extension;
                } 
                else 
                {
                    fileName = fileName + "(" + i++ + ")";
                    path = Path.Combine(directoryName, fileName) + extension;
                }
            }
            return path;
        }

        /// <summary>
        /// Check to see if the program can create, update, and delete in the specified path.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the path is writable.</returns>
        public static bool Writable(string path)
        {
            lock (_locker)
            {
                string directoryName;
                string fileName;
                string workPath;
                bool writable = false;

                if (path.Length == 0)
                    throw new ArgumentNullException("path");

                try
                {
                    directoryName = "TSTDIR";
                    workPath = System.IO.Path.Combine(path, directoryName);

                    if (System.IO.Directory.Exists(workPath))
                    {
                        System.IO.Directory.Delete(workPath, true);
                    }

                    System.IO.Directory.CreateDirectory(workPath);
                    fileName = System.IO.Path.Combine(workPath, "TSTFILE.TXT");

                    System.IO.FileStream fs = new System.IO.FileStream(fileName, FileMode.OpenOrCreate);
                    StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                    sw.Write("This is a test\n");
                    sw.Close();

                    System.IO.File.Delete(fileName);
                    Directory.Delete(workPath);
                    writable = true;
                }
                catch
                {
                    writable = false;
                }

                return writable;
            }
        }

        public static long GetFileSize(string path)
        {
            return new System.IO.FileInfo(path).Length;
        }

        /// <summary>
        /// Ensure the passed string ends with a directory separator character unless the string is blank.
        /// </summary>
        /// <param name="path">The string to append the backslash to.</param>
        /// <returns>String with a "/" on the end</returns>
        public static String PathSlash(string path)
        {
            string separator = Convert.ToString(Path.DirectorySeparatorChar);

            if (path.Length == 0)
                return path;
            else if (path.EndsWith(separator))
                return path;
            else
                return path + separator;
        }

        /// <summary>
        /// Return the parent directory for the specified path
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string GetParentDirectory(string fullPath)
        {
            return System.IO.Path.GetDirectoryName(ExtractPath(fullPath));
        }

        /// <summary>
        /// Extract the path from a path ending in a filename 
        /// </summary>
        /// <param name="fullPath">A fully qualified path ending in a filename</param>
        /// <returns>The extacted path</returns>
        public static string ExtractPath(string fullPath)
        {
            if (fullPath.Length == 0)
                throw new ArgumentNullException("fullPath");

            //Account for already in form of path
            if (System.IO.Path.GetFileName(fullPath).Length == 0
                || System.IO.Path.GetExtension(fullPath).Length == 0)
            {
                return fullPath;
            }
            else
            {
                return System.IO.Path.GetDirectoryName(fullPath);
            }
        }

        public static string ReplaceDriveLetterWithCurrent(string filePath)
        {
            Regex regex = new Regex(@"^[a-zA-Z]:\\");

            var match = regex.Match(filePath);

            if (match.Success)
            {
                return filePath.Replace(match.Value, GetCurrentDriveLetter() + @":\");
            }
            return filePath;
        }

        public static string GetCurrentDriveLetter()
        {
            string currentDirectory = GetCurrentDirectory();

            string[] words = currentDirectory.Split(':');

            if (words.Length == 0)
                throw new Exception("No drive letter found for: " + currentDirectory);

            return words[0];
        }

        /// <summary>
        /// Get the current directory of the executing assembly
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDirectory()
        {
            return PathSlash(AppDomain.CurrentDomain.BaseDirectory);
            //return PathSlash(ExtractPath(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }

        /// <summary>
        /// Gets the relative directory.
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="pathToAppend">The path to append.</param>
        /// <returns>System.String.</returns>
        public static string GetRelativeDirectory(string projectName, string pathToAppend)
        {
            string assemblyDirectory = PathSlash(ExtractPath(System.Reflection.Assembly.GetExecutingAssembly().Location));

            if (!assemblyDirectory.Contains(projectName))
                throw new Exception(String.Format("GetRelativeDirectory: {0} is not found in path {1}", projectName, assemblyDirectory));

            string rootDirectory = assemblyDirectory.Substring(0, assemblyDirectory.IndexOf(projectName));

            return Path.Combine(rootDirectory, pathToAppend);
        }

        public static void CopyDirectory(string fromPath, string toPath, bool clearDestFirst)
        {
            if (!Directory.Exists(fromPath))
            {
                throw new Exception("Path does not exist " + fromPath);
            }

            if (clearDestFirst)
            {
                DeleteDirectory(toPath);
            }

            CopyDirectory(fromPath, toPath);
        }

        public static void CopyDirectory(string source, string dest)
        {
            CopyDirectory(source,dest,new List<string>());
        }

        public static void CopyDirectory(string source, string dest, string searchPattern)
        {
            CopyDirectory(source, dest, searchPattern, new List<string>());
        }

        public static void CopyDirectory(string source, string dest, List<string> exclude)
        {
            CopyDirectory(source,dest,"*.*", exclude);
        }

        public static void CopyDirectory(string source, string dest, string searchPattern, List<string> exclude)
        {
            String[] files;

            if (dest[dest.Length - 1] != Path.DirectorySeparatorChar)
            {
                dest += Path.DirectorySeparatorChar;
            }

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            files = Directory.GetFileSystemEntries(source, searchPattern);

            foreach (string item in files)
            {
                string itemName = item.Substring(item.LastIndexOf("\\", StringComparison.Ordinal)+1);
                
                if (StringUtil.MatchWildcards(itemName, exclude))                
                    continue;
                
                // Sub directories
                if (Directory.Exists(item))
                {
                    CopyDirectory(item, dest + Path.GetFileName(item),searchPattern, exclude);
                }
                else // Files in directory
                {
                    File.Copy(item, dest + Path.GetFileName(item), true);
                }
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            Directory.Delete(path,true);
        }

        public static void DeleteDirectoryAndWait(string path)
        {
            for (int i = 1; i < 10; i++)
            {
                try
                {
                    DeleteDirectory(path);
                    break;
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(3000);
                }
            }
        }

        public static void CreateEmptyFile(string path)
        {
            using(new FileStream(path,FileMode.OpenOrCreate))
            {
            }
        }

        public static string OpenWithAssociatedEditor(string filePath)
        {
            string defaultEditPath = "notepad.exe %1";
            string editPath = null;

            try
            {
                // Obtain the extension to make the look-up
                string ext = Path.GetExtension(filePath);
                string extClass = null;

                // Open the CLASS key
                RegistryKey rkExt = Registry.ClassesRoot.OpenSubKey(ext);
                RegistryKey rkVerb = null;

                if (rkExt != null)
                {
                    // Obtain the related key, e.g. mp3file
                    extClass = (string)rkExt.GetValue(null, "");

                    rkExt.Close();
                    rkExt = Registry.ClassesRoot.OpenSubKey(string.Format(@"{0}\shell", extClass));

                    // Test its existence
                    if (rkExt != null)
                    {
                        // Get the EDIT command, or OPEN if the former does not exist
                        rkVerb = rkExt.OpenSubKey(@"edit\command");

                        if (rkVerb == null)
                            rkVerb = rkExt.OpenSubKey(@"open\command");

                        if (rkVerb != null)
                        {
                            editPath = (string)rkVerb.GetValue(null, "");

                            rkVerb.Close();
                        }

                        rkExt.Close();
                    }
                }
            }
            catch { }

            // Fall back to NOTEPAD is nothing was found
            if (editPath == null
                || editPath.Length == 0
                || filePath.ToLower().EndsWith(".xml"))
                editPath = defaultEditPath;

            // Remove application-specific support of ID Lists
            editPath = editPath.Replace(@" /idlist,%I,%L", "");

            //GTF
            editPath = editPath.Replace(@" /dde", "");

            // Ensure that we can insert the file path
            if (editPath.Contains("%1"))
            {
                // Insert it, replacing "%1"
                editPath = editPath.Replace(@"""%1""", @"%1");
                editPath = editPath.Replace(@" %1", "");
            }

            editPath = StringUtil.TakeOffBeginning(StringUtil.TakeOffEnd(editPath, "\""), "\"");

            ProcessUtil.Shell(editPath, "\"" + filePath + "\"", ProcessWindowStyle.Normal, false);
            //ProcessUtil.Shell(editPath, "\"" + filePath + "\"", ProcessWindowStyle.Normal, false);
            return editPath;
        }

        /// <summary>
        /// Returns a valid filename, ignoring invalid characters. Turn spaces into underscores
        /// </summary>
        /// <param name="sFileName"></param>
        /// <returns></returns>
        public static string FilterFileName(string sFileName)
        {
            return FilterFileName(sFileName, false);
        }

        /// <summary>
        /// Returns a valid filename, ignoring invalid characters
        /// </summary>
        /// <param name="sFileName"></param>
        /// <param name="allowSpaces">If false, spaces will be turned into underscores</param>
        /// <returns></returns>
        public static string FilterFileName(string sFileName, bool allowSpaces)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(sFileName.Length);
            string sChar;
            string sInvalid = "";

            for (int i = 0; i < System.IO.Path.GetInvalidFileNameChars().GetUpperBound(0); i++)
                sInvalid += System.IO.Path.GetInvalidFileNameChars()[i].ToString();

            for (int i = 0; i < System.IO.Path.GetInvalidPathChars().GetUpperBound(0); i++)
                sInvalid += System.IO.Path.GetInvalidPathChars()[i].ToString();

            sInvalid += System.IO.Path.VolumeSeparatorChar.ToString();
            sInvalid += System.IO.Path.PathSeparator.ToString();
            sInvalid += System.IO.Path.DirectorySeparatorChar.ToString();
            sInvalid += System.IO.Path.AltDirectorySeparatorChar.ToString();

            for (int i = 0; i < sFileName.Length; i++)
            {
                sChar = sFileName.Substring(i, 1);

                if (!allowSpaces && sChar == " ")
                    sChar = "_";

                if (sChar == "," || sChar == "'")
                {
                    sChar = "";
                }
                else if (sInvalid.IndexOf(sChar) < 0)
                    sb.Append(sChar);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Serialize an object into XML and save to a file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="filePath"></param>
        public static void SerializeToXml<T>(T value, string filePath) where T : class
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(T));
                x.Serialize(stream, value);
            }
        }

        /// <summary>
        /// Deserialize an XML File into an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T DeserializeFromXml<T>(string filePath) where T : class
        {
            using (StreamReader stream = new StreamReader(filePath))
            {
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(T));
                return (T)x.Deserialize(stream);
            }
        }

        /// <summary>
        /// Clears the read only attributes for all files in a directory
        /// </summary>
        /// <param name="source">The source directory</param>
        public static void ClearReadOnlyAttributes(string source)
        {
            string[] files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                FileAttributes attrs = File.GetAttributes(file);
                if ((attrs & FileAttributes.ReadOnly) != 0)
                    File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
            }
        }
    }
}
