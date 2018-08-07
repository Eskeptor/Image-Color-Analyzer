using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenCvSharp;

namespace Schoolworks_image_and_color
{
    /// <summary>
    /// ContourWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContourWindow : System.Windows.Window
    {
        private string mTargetImageURL;
        private string CONTOUR_IMAGE = Directory.GetCurrentDirectory() + @"\contour.png";

        public ContourWindow()
        {
            InitializeComponent();
        }

        public void SetImage(BitmapImage image, double width, double height)
        {
            mTargetImageURL = image.UriSource.LocalPath;
            contour_form.Width = width;
            contour_form.Height = height;

            Mat mat = new Mat(mTargetImageURL, ImreadModes.GrayScale);  // 원본 사진 흑백 변환
            mat = mat.Canny(75, 200, 3, true);                          // 외곽선 추출 함수

            if (File.Exists(CONTOUR_IMAGE))
                File.Delete(CONTOUR_IMAGE);
            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
            bitmap.Save(CONTOUR_IMAGE, System.Drawing.Imaging.ImageFormat.Png);
            bitmap.Dispose();
            bitmap = null;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(CONTOUR_IMAGE);
            bitmapImage.EndInit();

            contour_img.Source = bitmapImage;
        }

        private void ClosingWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            contour_img.Source = null;

            (typeof(System.Windows.Window)).GetField("_isClosing", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sender, false);
            e.Cancel = true;
            (sender as System.Windows.Window).Hide();
        }

        private void ActivatedWindow(object sender, EventArgs e)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(CONTOUR_IMAGE);
            bitmapImage.EndInit();

            contour_img.Source = bitmapImage;
        }
    }
}
