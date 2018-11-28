using System.Drawing;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// Helper utilities for dealing with images
    /// </summary>
    public static class ImageUtil
    {
        /// <summary>
        /// Get the size of an image
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Size GetImageSize(string filePath)
        {
            Image img = new Bitmap(filePath);
            return img.Size;
        }
    }
}
