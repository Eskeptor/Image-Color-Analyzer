﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using OpenCvSharp;
using System.Linq;


// Lockbits Documentation MSDN : https://msdn.microsoft.com/en-us/library/5ey6h79d(v=vs.110).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-1 
// Color.FromArgb Documentation MSDN : https://msdn.microsoft.com/en-us/library/1hstcth9(v=vs.110).aspx
// Window.Closing Event Documemtation MSDN : https://msdn.microsoft.com/en-us/library/system.windows.window.closing(v=vs.110).aspx
// Window.Activated Event Documentation MSDN : https://msdn.microsoft.com/en-us/library/system.windows.application.activated(v=vs.110).aspx
// BitmapImage2Bitmap Source by http://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa 
// ResizeArray Source by http://www.source-code.biz/snippets/csharp/1.htm
// Release Image from Image Source by : https://stackoverflow.com/questions/16908383/how-to-release-image-from-image-source-in-wpf


// ColorC class for ColorCount and Analyze Color;
class ColorC : IComparable
{
    public ColorC(string code)
    {
        ColorCode = code;
        Count = 1;
    }
    public string ColorCode { get; set; }
    public int Count { get; set; }

    public int CompareTo(object obj)
    {
        if (obj == null)
            return 1;
        if (obj is ColorC tmp)
            return Count.CompareTo(tmp.Count);
        else
            throw new ArgumentException("ColorC 오브젝트가 아닙니다.");
    }
}

struct RGB
{
    public RGB(string red, string green, string blue, string frequency)
    {
        Red = int.Parse(red);
        Green = int.Parse(green);
        Blue = int.Parse(blue);
        Frequency = int.Parse(frequency);
    }
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }
    public int Frequency { get; set; }
}

class CustomString
{
    public string Str { get; set; }
    public CustomString(string str = null)
    {
        Str = str;
    }
    public static int operator -(CustomString s1, CustomString s2)
    {
        int i1 = int.Parse(s1.Str);
        int i2 = int.Parse(s2.Str);
        return Math.Abs(i1 - i2);
    }
   
}

namespace Schoolworks_image_and_color
{
    /// <summary>
    /// Analyzer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Analyzer : System.Windows.Window
    {
        private string TEXT_RANGE = string.Format("{0} ~ {1}", Constants.ERROR_VALUE_LIMIT_MIN, Constants.ERROR_VALUE_LIMIT_MAX);

        private Bitmap mTargetImage;
        private string mTargetImageURL;
        private Rectangle mRect;
        // For LockBits
        private System.Drawing.Imaging.BitmapData mTargetBmpData;
        // Detail window object;
        private Detail mDetailWindow;
        // rectangle array is Rectagle array for this.rect_colors;
        private System.Windows.Shapes.Rectangle[] mColorList;
        private System.Windows.Controls.Button[] mDetailButtons;

        // Check image changed;
        private bool mIsChangedImage;

        private Thread topColorThread;
        private Thread colorHistogramThread;
        delegate void TopColorThreadDelegate(bool check, string reference);
        delegate void SetRectangleColorsDelegate(RGB[] colors, int max);
        delegate void ColorHistogramDelegate();

        private int imageAnalyzeRange;

        public Analyzer()
        {
            InitializeComponent();
            mDetailWindow = new Detail();
            mColorList = new System.Windows.Shapes.Rectangle[Constants.LIST_MAX];
            mDetailButtons = new System.Windows.Controls.Button[Constants.LIST_MAX];
            mIsChangedImage = false;

            mColorList[0] = rect_color1;
            mColorList[1] = rect_color2;
            mColorList[2] = rect_color3;
            mColorList[3] = rect_color4;
            mColorList[4] = rect_color5;
            mColorList[5] = rect_color6;
            mColorList[6] = rect_color7;
            mColorList[7] = rect_color8;
            mColorList[8] = rect_color9;
            mColorList[9] = rect_color10;
            mColorList[10] = rect_color11;
            mColorList[11] = rect_color12;
            mColorList[12] = rect_color13;
            mColorList[13] = rect_color14;
            mColorList[14] = rect_color15;
            mColorList[15] = rect_color16;

            mDetailButtons[0] = btn_color1;
            mDetailButtons[1] = btn_color2;
            mDetailButtons[2] = btn_color3;
            mDetailButtons[3] = btn_color4;
            mDetailButtons[4] = btn_color5;
            mDetailButtons[5] = btn_color6;
            mDetailButtons[6] = btn_color7;
            mDetailButtons[7] = btn_color8;
            mDetailButtons[8] = btn_color9;
            mDetailButtons[9] = btn_color10;
            mDetailButtons[10] = btn_color11;
            mDetailButtons[11] = btn_color12;
            mDetailButtons[12] = btn_color13;
            mDetailButtons[13] = btn_color14;
            mDetailButtons[14] = btn_color15;
            mDetailButtons[15] = btn_color16;

            txt_range.Foreground = System.Windows.Media.Brushes.Gray;
            txt_range.Text = "1 ~ 200";
        }

        // Lock the bitmap's bits;
        public void LockImage(BitmapImage image)
        {
            mTargetImage = BitmapExtension.BitmapEx.BitmapImage2Bitmap(image);
            mTargetImageURL = image.UriSource.LocalPath;
            filename_block.Text = mTargetImageURL;
            txt_status.Text = "";
            mRect = new System.Drawing.Rectangle(0, 0, mTargetImage.Width, mTargetImage.Height);
            mTargetBmpData = mTargetImage.LockBits(mRect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        }

        // Image Change Check function;
        public bool ImageChangeCheck(bool check)
        {
            mIsChangedImage = check;
            return false;
        }

        // The Analyze button action;
        private void ActionAnalyzeClick(object sender, RoutedEventArgs e)
        {
            if (txt_range.Text == TEXT_RANGE || txt_range.Text == string.Empty)
            {
                imageAnalyzeRange = Constants.ERROR_VALUE_LIMIT_DEFAULT;
                string message = string.Format((string)FindResource("message_warning_range_default"), Constants.ERROR_VALUE_LIMIT_DEFAULT);
                MessageBox.Show(message);
            }
            else 
            {
                imageAnalyzeRange = int.Parse(txt_range.Text);
                if (imageAnalyzeRange >= Constants.ERROR_VALUE_LIMIT_MIN &&
                imageAnalyzeRange <= Constants.ERROR_VALUE_LIMIT_MAX)
                {
                    string message = string.Format((string)FindResource("message_warning_range"), imageAnalyzeRange);
                    MessageBox.Show(message);
                }
                else
                {
                    string message = string.Format((string)FindResource("message_warning_range_error"), Constants.ERROR_VALUE_LIMIT_MIN, Constants.ERROR_VALUE_LIMIT_MAX);
                    MessageBox.Show(message);
                    return;
                }
            }
            

            Stopwatch sw = new Stopwatch();
            topColorThread = new Thread(() =>
            {
                sw.Start();
                Dispatcher.Invoke(new TopColorThreadDelegate(AnalyzeButtonDisabler), false, Constants.STRING_BUTTON_ANALYZE);
                MyLog.LogManager.Log("Analyze-상위 16색 분석 시작");

                BitmapRGBConvert();
                ColorCounter();
                ViewTopColors();

                mIsChangedImage = false;
                Dispatcher.Invoke(new TopColorThreadDelegate(AnalyzeButtonDisabler), false, Constants.STRING_BUTTON_COMPLETE);
                sw.Stop();
                MyLog.LogManager.Log("Analyze-상위 16색 분석 종료");
                MessageBox.Show(sw.ElapsedMilliseconds.ToString() + "ms");
            });
            topColorThread.Start();

            colorHistogramThread = new Thread(() =>
            {
                MyLog.LogManager.Log("Analyze-컬러 히스토그램 작업 시작");
                GetHistogram(new Mat(mTargetImageURL));
                MyLog.LogManager.Log("Analyze-컬러 히스토그램 작업 종료");
            });
            colorHistogramThread.Start();
        }

        private void GetHistogram(Mat source)
        {
            Mat[] histogram = new Mat[Constants.HISTOGRAM_MAX];    // 0 = blue, 1 = green, 2 = red
            Mat[] histoImage = new Mat[Constants.HISTOGRAM_MAX + 1];
            int[] dimensions = { 256 };         
            Rangef[] ranges = { new Rangef(0, 256) };

            // channel
            // 0 = blue
            // 1 = green
            // 2 = red

            double minVal, maxVal;
            MyLog.LogManager.Log("Analyze-histogram-컬러 히스토그램 정보 생성");
            for (int i = 0; i < Constants.HISTOGRAM_MAX; i++)
            {
                histogram[i] = new Mat();
                Cv2.CalcHist(
                    images: new[] { source },
                    channels: new[] { i }, 
                    mask: null,
                    hist: histogram[i],
                    dims: 1,
                    histSize: dimensions,
                    ranges: ranges);
                Cv2.Normalize(histogram[i], histogram[i]);
                Cv2.MinMaxLoc(histogram[i], out minVal, out maxVal);
                histogram[i] = histogram[i] * (maxVal != 0 ? Constants.HISTOGRAM_HEIGHT / maxVal : 0.0);
                histoImage[i] = new Mat(new OpenCvSharp.Size(Constants.HISTOGRAM_WIDTH, Constants.HISTOGRAM_HEIGHT), MatType.CV_8UC3, Scalar.All(255));
            }
            MyLog.LogManager.Log("Analyze-histogram-컬러 히스토그램 정보 생성완료");

            // Total 이미지
            histoImage[3] = new Mat(new OpenCvSharp.Size(Constants.HISTOGRAM_WIDTH, Constants.HISTOGRAM_HEIGHT), MatType.CV_8UC3, Scalar.All(255));

            int binWidth;

            MyLog.LogManager.Log("Analyze-histogram-Blue 그리기");
            // 파란색
            for (int i = 0; i < dimensions[0]; i++)
            {
                binWidth = (int)((double)Constants.HISTOGRAM_WIDTH / dimensions[0]);
               
                Cv2.Line(histoImage[0], new OpenCvSharp.Point(i * binWidth, histoImage[0].Rows - (int)(histogram[0].Get<float>(i))),
                    new OpenCvSharp.Point((i + 1) * binWidth, histoImage[0].Rows), 
                    new Scalar(255, 0, 0));

                Cv2.Line(histoImage[3], new OpenCvSharp.Point(i * binWidth, histoImage[3].Rows - (int)(histogram[0].Get<float>(i))),
                    new OpenCvSharp.Point((i + 1) * binWidth, histoImage[3].Rows),
                    new Scalar(255, 0, 0));
            }

            MyLog.LogManager.Log("Analyze-histogram-Green 그리기");
            // 초록색
            for (int i = 0; i < dimensions[0]; i++)
            {
                binWidth = (int)((double)Constants.HISTOGRAM_WIDTH / dimensions[0]);

                Cv2.Line(histoImage[1], new OpenCvSharp.Point(i * binWidth, histoImage[1].Rows - (int)(histogram[1].Get<float>(i))),
                    new OpenCvSharp.Point((i + 1) * binWidth, histoImage[1].Rows),
                    new Scalar(0, 255, 0));

                Cv2.Line(histoImage[3], new OpenCvSharp.Point(i * binWidth, histoImage[3].Rows - (int)(histogram[1].Get<float>(i))),
                    new OpenCvSharp.Point((i + 1) * binWidth, histoImage[3].Rows),
                    new Scalar(0, 255, 0));
            }

            MyLog.LogManager.Log("Analyze-histogram-Red 그리기");
            // 빨간색
            for (int i = 0; i < dimensions[0]; i++)
            {
                binWidth = (int)((double)Constants.HISTOGRAM_WIDTH / dimensions[0]);

                Cv2.Line(histoImage[2], new OpenCvSharp.Point(i * binWidth, histoImage[2].Rows - (int)(histogram[2].Get<float>(i))),
                    new OpenCvSharp.Point((i + 1) * binWidth, histoImage[2].Rows),
                    new Scalar(0, 0, 255));

                Cv2.Line(histoImage[3], new OpenCvSharp.Point(i * binWidth, histoImage[3].Rows - (int)(histogram[2].Get<float>(i))),
                    new OpenCvSharp.Point((i + 1) * binWidth, histoImage[3].Rows),
                    new Scalar(0, 0, 255));
            }

            MyLog.LogManager.Log("Analyze-histogram-컬러 히스토그램 이미지화 시작");
            for (int i = 0; i < Constants.HISTOGRAM_MAX + 1; i++)
            {
                string url = Directory.GetCurrentDirectory() + @"\" + i + ".png";
                if (File.Exists(url))
                {
                    File.Delete(url);
                }
                Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(histoImage[i]);
                bitmap.Save(url, System.Drawing.Imaging.ImageFormat.Png);
                bitmap.Dispose();
                bitmap = null;
            }
            MyLog.LogManager.Log("Analyze-histogram-컬러 히스토그램 이미지화 완료");
            Dispatcher.Invoke(new ColorHistogramDelegate(DrawColorHistogram));
        }

        private void DrawColorHistogram()
        {
            string cur = Directory.GetCurrentDirectory();
            BitmapImage[] images = new BitmapImage[Constants.HISTOGRAM_MAX + 1];
            for (int i = 0; i < Constants.HISTOGRAM_MAX + 1; i++)
            {
                images[i] = new BitmapImage();
                images[i].BeginInit();
                images[i].CacheOption = BitmapCacheOption.OnLoad;
                images[i].UriSource = new Uri(cur + @"\" + i + ".png");
                images[i].EndInit();
            }
            img_color_histogram_blue.Source = images[0];
            img_color_histogram_green.Source = images[1];
            img_color_histogram_red.Source = images[2];
            img_color_histogram_total.Source = images[3];
        }

        private void AnalyzeButtonDisabler(bool check, string reference)
        {
            btn_analyze.IsEnabled = check;
            btn_analyze.Content = FindResource(reference);
        }

        // Detail_clicks button actions show the details(Color details);
        private void ActionDetail1(object sender, RoutedEventArgs e)
        {
            Detail_Window(0);
        }
        private void ActionDetail2(object sender, RoutedEventArgs e)
        {
            Detail_Window(1);
        }
        private void ActionDetail3(object sender, RoutedEventArgs e)
        {
            Detail_Window(2);
        }
        private void ActionDetail4(object sender, RoutedEventArgs e)
        {
            Detail_Window(3);
        }
        private void ActionDetail5(object sender, RoutedEventArgs e)
        {
            Detail_Window(4);
        }
        private void ActionDetail6(object sender, RoutedEventArgs e)
        {
            Detail_Window(5);
        }
        private void ActionDetail7(object sender, RoutedEventArgs e)
        {
            Detail_Window(6);
        }
        private void ActionDetail8(object sender, RoutedEventArgs e)
        {
            Detail_Window(7);
        }
        private void ActionDetail9(object sender, RoutedEventArgs e)
        {
            Detail_Window(8);
        }
        private void ActionDetail10(object sender, RoutedEventArgs e)
        {
            Detail_Window(9);
        }
        private void ActionDetail11(object sender, RoutedEventArgs e)
        {
            Detail_Window(10);
        }
        private void ActionDetail12(object sender, RoutedEventArgs e)
        {
            Detail_Window(11);
        }
        private void ActionDetail13(object sender, RoutedEventArgs e)
        {
            Detail_Window(12);
        }
        private void ActionDetail14(object sender, RoutedEventArgs e)
        {
            Detail_Window(13);
        }
        private void ActionDetail15(object sender, RoutedEventArgs e)
        {
            Detail_Window(14);
        }
        private void ActionDetail16(object sender, RoutedEventArgs e)
        {
            Detail_Window(15);
        }

        // This function is convert bitmapimage to rgb values output "analyzer.dat";
        // and return bytes(Math.Abs(bmpData.Stride) * image.Height)
        private void BitmapRGBConvert()
        {
            MyLog.LogManager.Log("Analyze-frequency-비트맵 컨버팅 시작");
            using (StreamWriter streamwriter = new StreamWriter("analyzer.dat"))
            {
                // Get the address of the first line;
                IntPtr ptr = mTargetBmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap;
                int bytes = Math.Abs(mTargetBmpData.Stride) * mTargetImage.Height;
                byte[] rgbValues = new byte[bytes];
                // ↑ rgbValues[i] -> i%3==0(R), i%3==1(G), i%3==2(B)

                try
                {
                    // Copy the RGB values into the array;
                    System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                    int numBytes = 0;
                    for (int y = 0; y < mTargetImage.Height; y++)
                    {
                        for (int x = 0; x < mTargetImage.Width; x++)
                        {
                            // numBytes for 24Bit Color;
                            numBytes = ((y * mTargetImage.Height) + x) * 3;
                            if (numBytes + 2 < bytes)
                                streamwriter.WriteLine("{0}-{1}-{2}", rgbValues[numBytes + 2], rgbValues[numBytes + 1], rgbValues[numBytes]);
                        }
                    }
                    mTargetImage.UnlockBits(mTargetBmpData);
                }
                catch (Exception e)
                {
                    MyLog.LogManager.Log("오류: " + e.Message);
                }
            }
            MyLog.LogManager.Log("Analyze-frequency-비트맵 컨버팅 완료");
        }

        // It shows the RGB frequency using analyzer.dat and output colorcount.dat;
        // Caution!! : This function is very very heavy. Because the high frequency of the cull of 16 colors.
        private void ColorCounter()
        {
            MyLog.LogManager.Log("Analyze-frequency-비트맵 컬러 카운팅 시작");
            // Read file "analyzer.dat" for analyze frequency and save new file "colorcount.dat";
            using (StreamReader streamreader = new StreamReader("analyzer.dat"))
            {
                // Save new file "colorcount.dat";
                using (StreamWriter streamwriter = new StreamWriter("colorcount.dat"))
                {
                    // sameCheck is a variable for checking whether the values ​​at the current position.
                    // When sameCheck is true, raise the counter;
                    string line;
                    // tmpcolor is array for save the color; 
                    // tmpcolor's length is big -> loading speed is fast. but memory is bigger;
                    // tmpcolor's length is small -> loading speed is slow. but memory is lower;
                    Dictionary<string, ColorC> tmpColor = new Dictionary<string, ColorC>();
                    try
                    {
                        while ((line = streamreader.ReadLine()) != null)
                        {
                            if (tmpColor.ContainsKey(line))
                            {
                                tmpColor[line].Count++;
                            }
                            else
                            {
                                tmpColor.Add(line, new ColorC(line));
                            }
                        }

                        var list = from pair in tmpColor
                                   orderby pair.Value descending
                                   select pair;


                        int count = 0;
                        bool isSame = true;
                        KeyValuePair<string, ColorC>[] tmpPair = new KeyValuePair<string, ColorC>[16];
                        foreach (KeyValuePair<string, ColorC> tmp in list)
                        {
                            if (count == 0)
                            {
                                tmpPair[count] = tmp;
                                streamwriter.WriteLine("{0}-{1}", tmp.Value.ColorCode, tmp.Value.Count);
                                count++;
                            }
                            else if (count == Constants.LIST_MAX)
                                break;
                            else
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    if (!GetColorErrorLimit(tmpPair[i].Value.ColorCode, tmp.Value.ColorCode))
                                    {
                                        isSame = false;
                                        break;
                                    }
                                }
                                if (isSame)
                                {
                                    tmpPair[count] = tmp;
                                    streamwriter.WriteLine("{0}-{1}", tmp.Value.ColorCode, tmp.Value.Count);
                                    count++;
                                    
                                }
                                isSame = true;
                            }
                        }
                        
                        //foreach (KeyValuePair<string, ColorC> tmp in tmpColor)
                        //{
                        //    streamwriter.WriteLine(tmp.Value.ColorCode + "-" + tmp.Value.Count);
                        //}
                    }
                    catch (Exception e)
                    {
                        MyLog.LogManager.Log("오류: " + e.Message);
                    }
                    tmpColor.Clear();
                }
            }
            MyLog.LogManager.Log("Analyze-frequency-비트맵 컬러 카운팅 종료");
            File.Delete("analyzer.dat");
        }

        // Analyze colorcount.dat and drawing color histogram
        private void ViewTopColors()
        {
            RGB[] color = new RGB[16];

            int min = 0;
            int max = 0;

            MyLog.LogManager.Log("Analyze-frequency-색상 뿌리기 시작");
            using (StreamReader streamreader = new StreamReader("colorcount.dat"))
            {
                //This code(streamwriter) is test code(verify that codes for storage -> frequency array)
                using (StreamWriter streamwriter = new StreamWriter("frequency.dat"))
                {
                    // just counting
                    int colorCount = 0;
                    string line;
                    
                    try
                    {
                        while ((line = streamreader.ReadLine()) != null)
                        {
                            string[] lineSplit = line.Split('-');

                            if (colorCount == Constants.LIST_MAX)
                            {
                                for (int i = 0; i < Constants.LIST_MAX - 1; i++)
                                {
                                    if (color[i].Frequency > color[i + 1].Frequency)
                                    {
                                        RGB tmp = color[i];
                                        color[i] = color[i + 1];
                                        color[i + 1] = tmp;
                                    }
                                }
                            }

                            if (colorCount >= Constants.LIST_MAX)
                            {
                                for (int i = 0; i < Constants.LIST_MAX; i++)
                                {
                                    if (color[i].Frequency < int.Parse(lineSplit[Constants.STRING_DIVIDE_FREQUENCY]))
                                    {
                                        color[i] = new RGB(lineSplit[Constants.STRING_DIVIDE_RED], lineSplit[Constants.STRING_DIVIDE_GREEN],
                                            lineSplit[Constants.STRING_DIVIDE_BLUE], lineSplit[Constants.STRING_DIVIDE_FREQUENCY]);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                color[colorCount++] = new RGB(lineSplit[Constants.STRING_DIVIDE_RED], lineSplit[Constants.STRING_DIVIDE_GREEN],
                                    lineSplit[Constants.STRING_DIVIDE_BLUE], lineSplit[Constants.STRING_DIVIDE_FREQUENCY]);
                            }
                        }
                        for (int i = 0; i < Constants.LIST_MAX; i++)
                        {
                            streamwriter.WriteLine(color[i].Red + "-" + color[i].Green + "-" + color[i].Blue + "-" + color[i].Frequency);
                        }

                        max = color[0].Frequency;
                        min = max;
                        for (int i = 0; i < Constants.LIST_MAX; i++)
                        {
                            if (max < color[i].Frequency)
                            {
                                max = color[i].Frequency;
                            }
                            else if (min > color[i].Frequency)
                            {
                                min = color[i].Frequency;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MyLog.LogManager.Log("오류: " + e.Message);
                    }
                }
            }
            MyLog.LogManager.Log("Analyze-frequency-색상 뿌리기 종료");
            // File.Delete("colorcount.dat");
            // According to result, Fill the rectangle;
            Dispatcher.Invoke(new SetRectangleColorsDelegate(SetRectangleColor), color, max);
        }

        private bool GetColorErrorLimit(string str1, string str2)
        {
            string[] color1 = str1.Split('-');
            string[] color2 = str2.Split('-');
            CustomString cus1 = new CustomString();
            CustomString cus2 = new CustomString();
            for (int i = 0; i < color1.Length; i++)
            {
                cus1.Str = color1[i];
                cus2.Str = color2[i];
                if (Math.Abs(cus1 - cus2) >= imageAnalyzeRange)
                    return true;
            }
            return false;
        }

        private void SetRectangleColor(RGB[] color, int max)
        {
            double height;
            for (int i = 0; i < Constants.LIST_MAX; i++)
            {
                mColorList[i].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)color[i].Red, (byte)color[i].Green, (byte)color[i].Blue));
                height = color[i].Frequency;
                mColorList[i].Height = height / max * Constants.RECTANGLE_MAX_HEIGHT;
                mDetailButtons[i].IsEnabled = true;
            }
            txt_status.Text = "Analyze Complete";
        }

        // Show Detail information of color;
        private void Detail_Window(int idx)
        {
            MyLog.LogManager.Log("Analyze-DetailWindow-디테일 윈도우 생성");
            using (StreamReader streamreader = new StreamReader("frequency.dat"))
            {
                int cnt = 0;
                string[] color = new string[4];
                string line;
                while ((line = streamreader.ReadLine()) != null)
                {
                    if (cnt == idx)
                    {
                        break;
                    }
                    cnt++;
                }
                color = line.Split('-');
                mDetailWindow.setDetail(color);
            }

            mDetailWindow.Owner = this;
            mDetailWindow.Show();
        }

        // For Re-open Window;
        private void ClosingWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mTargetImage != null)
            {
                mTargetImage.Dispose();
                mTargetImage = null;
            }
            (typeof(System.Windows.Window)).GetField("_isClosing", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sender, false);
            e.Cancel = true;
            (sender as System.Windows.Window).Hide();
        }

        // When this window re-open, it is determined whether or not initialization;
        private void ActivatedWindow(object sender, EventArgs e)
        {
            if (mIsChangedImage)
            {
                for (int i = 0; i < Constants.LIST_MAX; i++)
                {
                    mColorList[i].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                    mColorList[i].Height = 0;
                    mDetailButtons[i].IsEnabled = false;
                }

                img_color_histogram_blue.Source = null;
                img_color_histogram_green.Source = null;
                img_color_histogram_red.Source = null;
                img_color_histogram_total.Source = null;
                AnalyzeButtonDisabler(true, Constants.STRING_BUTTON_ANALYZE);
            }
        }

        private void ActionRangeFocus(object sender, RoutedEventArgs e)
        {
            txt_range.Text = "";
            txt_range.Foreground = System.Windows.Media.Brushes.Black;
        }

        private void ActionRangeLostFocus(object sender, RoutedEventArgs e)
        {
            if (txt_range.Text == "")
            {
                txt_range.Foreground = System.Windows.Media.Brushes.Gray;
                txt_range.Text = TEXT_RANGE;
            }
        }
    }
}
