using OpenCvSharp;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace Schoolworks_image_and_color
{
    class SimilarImage
    {
        public SimilarImage(int id, string url)
        {
            ID = id;
            URL = url;
        }
        public int ID { get; set; }
        public string URL { get; set; }
        public int Count { get; set; }
    }
    /// <summary>
    /// SimilarWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SimilarWindow : System.Windows.Window
    {
        private BitmapImage mOriginImage;
        private SimilarImage[] mSimilarImage;
        private string mOriginImageURL;
        private bool mIsInput;

        public SimilarWindow()
        {
            InitializeComponent();
            mSimilarImage = new SimilarImage[Constants.SIMILAR_MAX];
            mIsInput = false;
        }

        public void SetOriginImage(BitmapImage origin)
        {
            if (mOriginImage != null)
            {
                mOriginImage = null;
            }
            mOriginImage = origin;
            mOriginImageURL = origin.UriSource.LocalPath;
        }

        private void ActionAnalyze(object sender, RoutedEventArgs e)
        {
            if (mIsInput)
            {
                Stopwatch sw = new Stopwatch();
                Thread analyzeThread = new Thread(() =>
                {
                    sw.Start();
                    Analyze();
                    sw.Stop();
                    MessageBox.Show(sw.ElapsedMilliseconds.ToString() + "ms");
                });
                analyzeThread.Start();
            }
            else
            {
                MessageBox.Show((string)FindResource(Constants.STRING_MSG_SIMILAR));
            }
        }

        private void Analyze()
        {
            ArrayList position = CountImage();
            int length = position.Count;
            Mat[] srcImage = new Mat[length];
            Mat[] hsvImage = new Mat[length];
            Mat[] histogram = new Mat[length];

            Mat originSrcImage = new Mat(mOriginImageURL);
            Mat originHsvImage = new Mat();
            Mat originHistogram = new Mat();

            for (int i = 0; i < length; i++)
            {
                srcImage[i] = new Mat(mSimilarImage[(int)position[i]].URL);
                hsvImage[i] = new Mat();
                Cv2.CvtColor(srcImage[i], hsvImage[i], ColorConversionCodes.BGR2HSV);
            }
            Cv2.CvtColor(originSrcImage, originHsvImage, ColorConversionCodes.BGR2HSV);

            int hBins = 50;
            int sBins = 60;
            int[] histSize = { hBins, sBins };
            Rangef[] ranges = { new Rangef(0, 180), new Rangef(0, 256) };
            int[] channels = { 0, 1 };

            for (int i = 0; i < length; i++)
            {
                histogram[i] = new Mat();
                Cv2.CalcHist(
                    images: new[] { hsvImage[i] },
                    channels: channels,
                    mask: null,
                    hist: histogram[i],
                    dims: 1,
                    histSize: histSize,
                    ranges: ranges);
                Cv2.Normalize(histogram[i], histogram[i], 0, 1, NormTypes.MinMax);
            }
            Cv2.CalcHist(
                images: new[] { originHsvImage },
                channels: channels,
                mask: null,
                hist: originHistogram,
                dims: 1,
                histSize: histSize,
                ranges: ranges);
            Cv2.Normalize(originHistogram, originHistogram, 0, 1, NormTypes.MinMax);

            double maxValue = 0;
            int maxID = 0;
            for (int i = 0; i < length; i++)
            {
                double bass = Cv2.CompareHist(originHistogram, histogram[i], HistCompMethods.Correl);
                if (i == 0)
                {
                    maxValue = bass;
                    maxID = 0;
                }
                else
                {
                    if (maxValue < bass)
                    {
                        maxValue = bass;
                        maxID = i;
                    }
                }
                   
                Console.WriteLine("Method[" + i + "] Perfect: " + bass);
            }

            Console.WriteLine("가장 유사한 그림의 유사도: " + maxValue);
            Dispatcher.Invoke(new Action(delegate ()
            {
                BitmapImage tmpImage = new BitmapImage();
                tmpImage.BeginInit();
                tmpImage.CacheOption = BitmapCacheOption.OnLoad;
                tmpImage.UriSource = new Uri(mSimilarImage[(int)position[maxID]].URL);
                tmpImage.EndInit();
                similar_result_image.Source = tmpImage;
                similar_analyze.IsEnabled = false;
                similar_analyze.Content = FindResource(Constants.STRING_BUTTON_COMPLETE);
            }));
        }

        private ArrayList CountImage()
        {
            ArrayList imagePosition = new ArrayList();
            foreach (SimilarImage image in mSimilarImage)
            {
                if (image != null)
                {
                    imagePosition.Add(image.ID);
                }
            }
            return imagePosition;
        }

        private void Reset()
        {
            mIsInput = false;
            if (similar_result_image.Source != null)
            {
                similar_result_image.Source = null;
            }
            similar_comp_img1.Background = null;
            similar_comp_img1.Content = FindResource(Constants.STRING_BUTTON_SIMILAR);
            similar_comp_img2.Background = null;
            similar_comp_img2.Content = FindResource(Constants.STRING_BUTTON_SIMILAR);
            similar_comp_img3.Background = null;
            similar_comp_img3.Content = FindResource(Constants.STRING_BUTTON_SIMILAR);
            similar_comp_img4.Background = null;
            similar_comp_img4.Content = FindResource(Constants.STRING_BUTTON_SIMILAR);
            similar_comp_img5.Background = null;
            similar_comp_img5.Content = FindResource(Constants.STRING_BUTTON_SIMILAR);
            similar_comp_img6.Background = null;
            similar_comp_img6.Content = FindResource(Constants.STRING_BUTTON_SIMILAR);
            similar_comp_img7.Background = null;
            similar_comp_img7.Content = FindResource(Constants.STRING_BUTTON_SIMILAR);
            similar_comp_img8.Background = null;
            similar_comp_img8.Content = FindResource(Constants.STRING_BUTTON_SIMILAR);

            for (int i = 0; i < Constants.SIMILAR_MAX; i++)
            {
                if (mSimilarImage[i] != null)
                {
                    mSimilarImage[i] = null;
                }
            }
        }

        private void ActionSimImage1(object sender, RoutedEventArgs e)
        {
            OpenImage(sender as Button, 0);
        }
        private void ActionSimImage2(object sender, RoutedEventArgs e)
        {
            OpenImage(sender as Button, 1);
        }
        private void ActionSimImage3(object sender, RoutedEventArgs e)
        {
            OpenImage(sender as Button, 2);
        }
        private void ActionSimImage4(object sender, RoutedEventArgs e)
        {
            OpenImage(sender as Button, 3);
        }
        private void ActionSimImage5(object sender, RoutedEventArgs e)
        {
            OpenImage(sender as Button, 4);
        }
        private void ActionSimImage6(object sender, RoutedEventArgs e)
        {
            OpenImage(sender as Button, 5);
        }
        private void ActionSimImage7(object sender, RoutedEventArgs e)
        {
            OpenImage(sender as Button, 6);
        }
        private void ActionSimImage8(object sender, RoutedEventArgs e)
        {
            OpenImage(sender as Button, 7);
        }

        private void OpenImage(Button imageButton, int id)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpeg";
            dlg.Filter = "JPEG/JPG Files (*.jpeg, *.jpg)|*.jpeg;*.jpg|GIF Files (*.gif)|*.gif|Bitmap Files (*.bmp)|*.bmp|PNG Files (*.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                ImageBrush image = new ImageBrush();
                BitmapImage tmpImage = new BitmapImage();
                if (mSimilarImage[id] == null)
                {
                    mSimilarImage[id] = new SimilarImage(id, dlg.FileName);
                }
                else
                {
                    mSimilarImage[id].URL = dlg.FileName;
                }
                
                tmpImage.BeginInit();
                tmpImage.CacheOption = BitmapCacheOption.OnLoad;
                tmpImage.UriSource = new Uri(dlg.FileName);
                tmpImage.EndInit();
                image.ImageSource = tmpImage;

                mIsInput = true;

                Dispatcher.Invoke(new Action(delegate ()
                {
                    imageButton.Content = null;
                    imageButton.Background = image;
                }));
            }
        }

        // For Re-open Window;
        private void ClosingWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Reset();
            (typeof(System.Windows.Window)).GetField("_isClosing", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sender, false);
            e.Cancel = true;
            (sender as System.Windows.Window).Hide();
        }

        // When this window re-open, it is determined whether or not initialization;
        private void ActivatedWindow(object sender, EventArgs e)
        {
            if (mOriginImage != null)
            {
                similar_origin_image.Source = mOriginImage;
                similar_analyze.IsEnabled = true;
                similar_analyze.Content = FindResource(Constants.STRING_BUTTON_ANALYZE);
            }
        }
    }
}
