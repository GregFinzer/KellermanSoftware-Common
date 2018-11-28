using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// Compress files and directories using GZip
    /// </summary>
    public class GZip
    {
        public delegate void ProgressDelegate(string message);

        /// <summary>
        /// Compress a file
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="relativePath"></param>
        /// <param name="zipStream"></param>
        public void CompressFile(string directory, string relativePath, GZipStream zipStream)
        {
            //Compress file name
            char[] chars = relativePath.ToCharArray();
            zipStream.Write(BitConverter.GetBytes(chars.Length), 0, sizeof(int));
            foreach (char c in chars)
                zipStream.Write(BitConverter.GetBytes(c), 0, sizeof(char));

            //Compress file content
            byte[] bytes = File.ReadAllBytes(Path.Combine(directory, relativePath));
            zipStream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
            zipStream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Decompress a file using GZip
        /// </summary>
        /// <param name="inputDirectory"></param>
        /// <param name="zipStream"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public bool DecompressFile(string inputDirectory, GZipStream zipStream, ProgressDelegate progress)
        {
            //Decompress file name
            byte[] bytes = new byte[sizeof(int)];
            int Readed = zipStream.Read(bytes, 0, sizeof(int));
            if (Readed < sizeof(int))
                return false;

            int iNameLen = BitConverter.ToInt32(bytes, 0);
            bytes = new byte[sizeof(char)];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < iNameLen; i++)
            {
                zipStream.Read(bytes, 0, sizeof(char));
                char c = BitConverter.ToChar(bytes, 0);
                sb.Append(c);
            }
            string sFileName = sb.ToString();
            if (progress != null)
                progress(sFileName);

            //Decompress file content
            bytes = new byte[sizeof(int)];
            zipStream.Read(bytes, 0, sizeof(int));
            int iFileLen = BitConverter.ToInt32(bytes, 0);

            bytes = new byte[iFileLen];
            zipStream.Read(bytes, 0, bytes.Length);

            string sFilePath = Path.Combine(inputDirectory, sFileName);
            string sFinalDir = Path.GetDirectoryName(sFilePath);
            if (!Directory.Exists(sFinalDir))
                Directory.CreateDirectory(sFinalDir);

            using (FileStream outFile = new FileStream(sFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                outFile.Write(bytes, 0, iFileLen);

            return true;
        }

        /// <summary>
        /// Compress a directory using GZip
        /// </summary>
        /// <param name="inputDirectory"></param>
        /// <param name="outputFile"></param>
        /// <param name="progress"></param>
        public void CompressDirectory(string inputDirectory, string outputFile, ProgressDelegate progress)
        {
            string[] sFiles = Directory.GetFiles(inputDirectory, "*.*", SearchOption.AllDirectories);
            int iDirLen = inputDirectory[inputDirectory.Length - 1] == Path.DirectorySeparatorChar ? inputDirectory.Length : inputDirectory.Length + 1;

            using (FileStream outFile = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None))
            using (GZipStream str = new GZipStream(outFile, CompressionMode.Compress))
                foreach (string sFilePath in sFiles)
                {
                    string sRelativePath = sFilePath.Substring(iDirLen);
                    if (progress != null)
                        progress(sRelativePath);
                    CompressFile(inputDirectory, sRelativePath, str);
                }
        }

        /// <summary>
        /// Decompress a file to a directory
        /// </summary>
        /// <param name="compressedFile"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="progress"></param>
        public void DecompressToDirectory(string compressedFile, string outputDirectory, ProgressDelegate progress)
        {
            using (FileStream inFile = new FileStream(compressedFile, FileMode.Open, FileAccess.Read, FileShare.None))
            using (GZipStream zipStream = new GZipStream(inFile, CompressionMode.Decompress, true))
                while (DecompressFile(outputDirectory, zipStream, progress)) ;
        }

        /// <summary>
        /// Decompress a stream to a directory
        /// </summary>
        /// <param name="compressedStream"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="progress"></param>
        public void DecompressToDirectory(Stream compressedStream, string outputDirectory, ProgressDelegate progress)
        {
            using (GZipStream zipStream = new GZipStream(compressedStream, CompressionMode.Decompress, true))
                while (DecompressFile(outputDirectory, zipStream, progress)) ;
        }

    }
}