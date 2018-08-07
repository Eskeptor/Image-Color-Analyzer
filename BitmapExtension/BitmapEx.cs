using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BitmapExtension
{
    public class BitmapEx
    {
        // BitmapImage Convert to Bitmap;
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                outStream.Close();
                return new Bitmap(bitmap);
            }
        }

        // Copyright: http://frasergreenroyd.com/how-to-convert-opencv-cvmat-to-system-bitmap-to-system-imagesource/
        public static ImageSource BitmapToImageSource(Bitmap imToConvert)
        {
            Bitmap bmp = new Bitmap(imToConvert);
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            ImageSource sc = (ImageSource)image;
            ms.Close();
            bmp.Dispose();
            bmp = null;
            return sc;
        }
    }
}
