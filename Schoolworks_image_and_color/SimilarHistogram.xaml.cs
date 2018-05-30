using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Schoolworks_image_and_color
{
    public enum HistogramTYPE
    {
        Target, Result
    }

    /// <summary>
    /// SimilarHistogram.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SimilarHistogram : System.Windows.Window
    {
        private string mPrevOriginURL;
        public SimilarHistogram()
        {
            InitializeComponent();
        }

        public void SetHistogram(string originURL, HistogramTYPE type)
        {
            if (mPrevOriginURL == null || !mPrevOriginURL.Equals(originURL))
            {
                if (File.Exists(originURL))
                {
                    mPrevOriginURL = originURL;
                    int histWidth = 268;
                    int histHeight = 159;
                    Mat[] colorHist = new Mat[Constants.HISTOGRAM_MAX];
                    Mat histImage = new Mat(new OpenCvSharp.Size(histWidth, histHeight), MatType.CV_8UC3, Scalar.All(255));
                    int[] dimensions = { 256 };
                    Rangef[] ranges = { new Rangef(0, 256) };

                    double minVal, maxVal;
                    for (int i = 0; i < Constants.HISTOGRAM_MAX; i++)
                    {
                        colorHist[i] = new Mat();
                        Cv2.CalcHist(
                            images: new[] { new Mat(originURL) },
                            channels: new[] { i },
                            mask: null,
                            hist: colorHist[i],
                            dims: 1,
                            histSize: dimensions,
                            ranges: ranges);
                        Cv2.Normalize(colorHist[i], colorHist[i]);
                        Cv2.MinMaxLoc(colorHist[i], out minVal, out maxVal);
                        colorHist[i] *= (maxVal != 0 ? histHeight / maxVal : 0.0);
                    }

                    int binWidth;
                    for (int i = 0; i < dimensions[0]; i++)
                    {
                        binWidth = (int)((double)histWidth / dimensions[0]);

                        Cv2.Line(histImage, new OpenCvSharp.Point(i * binWidth, histImage.Rows - (int)colorHist[0].Get<float>(i)), 
                            new OpenCvSharp.Point((i + 1) * binWidth, histImage.Rows), 
                            new Scalar(255,0,0));
                        Cv2.Line(histImage, new OpenCvSharp.Point(i * binWidth, histImage.Rows - (int)colorHist[1].Get<float>(i)),
                            new OpenCvSharp.Point((i + 1) * binWidth, histImage.Rows),
                            new Scalar(0, 255, 0));
                        Cv2.Line(histImage, new OpenCvSharp.Point(i * binWidth, histImage.Rows - (int)colorHist[2].Get<float>(i)),
                            new OpenCvSharp.Point((i + 1) * binWidth, histImage.Rows),
                            new Scalar(0, 0, 255));
                    }

                    string histURL = System.IO.Directory.GetCurrentDirectory() + @"\histo" + type + ".png";
                    if (File.Exists(histURL))
                    {
                        File.Delete(histURL);
                    }
                    Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(histImage);
                    bitmap.Save(histURL, System.Drawing.Imaging.ImageFormat.Png);
                    bitmap.Dispose();
                    bitmap = null;

                    BitmapImage tmpImage = new BitmapImage();
                    tmpImage.BeginInit();
                    tmpImage.CacheOption = BitmapCacheOption.OnLoad;
                    tmpImage.UriSource = new Uri(histURL);
                    tmpImage.EndInit();

                    Dispatcher.Invoke(new Action(delegate ()
                    {
                        similar_histogram_img.Source = tmpImage;
                    }));
                }
            }
        }

        // For Re-open Window
        private void ClosingWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (typeof(System.Windows.Window)).GetField("_isClosing", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sender, false);
            e.Cancel = true;
            (sender as System.Windows.Window).Hide();
        }
    }
}
