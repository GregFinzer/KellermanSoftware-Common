using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace KellermanSoftware.Common
{
    public static class ImageUtil
    {
        public static Size GetImageSize(string filePath)
        {
            Image img = new Bitmap(filePath);
            return img.Size;
        }
    }
}
