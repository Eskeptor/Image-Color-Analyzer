using System;
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


// Lockbits Documentation MSDN : https://msdn.microsoft.com/en-us/library/5ey6h79d(v=vs.110).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-1 
// Color.FromArgb Documentation MSDN : https://msdn.microsoft.com/en-us/library/1hstcth9(v=vs.110).aspx
// Window.Closing Event Documemtation MSDN : https://msdn.microsoft.com/en-us/library/system.windows.window.closing(v=vs.110).aspx
// Window.Activated Event Documentation MSDN : https://msdn.microsoft.com/en-us/library/system.windows.application.activated(v=vs.110).aspx
// BitmapImage2Bitmap Source by http://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa 
// ResizeArray Source by http://www.source-code.biz/snippets/csharp/1.htm


static class Constants
{
    public const int LIST_MAX = 16;

    public const int STRING_DIVIDE_RED = 0;
    public const int STRING_DIVIDE_GREEN = 1;
    public const int STRING_DIVIDE_BLUE = 2;
    public const int STRING_DIVIDE_FREQUENCY = 3;

    public const double RECTANGLE_MAX_HEIGHT = 300;

    public const int HISTOGRAM_MAX = 3;
}

// ColorC class for ColorCount and Analyze Color;
class ColorC
{
    public string color_code;  // for 
    public int cnt;
    public ColorC(string code)
    {
        color_code = code;
        cnt = 1;
    }
}

class RGB
{
    public int red;
    public int green;
    public int blue;
    public int frequency;
    public RGB(string red, string green, string blue, string frequency)
    {
        this.red = int.Parse(red);
        this.green = int.Parse(green);
        this.blue = int.Parse(blue);
        this.frequency = int.Parse(frequency);
    }
}

namespace Schoolworks_image_and_color
{
    /// <summary>
    /// Analyzer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Analyzer : System.Windows.Window
    {
        private Bitmap image;
        private string imageURL;
        private Rectangle rect;
        // For LockBits
        private System.Drawing.Imaging.BitmapData bmpData;
        // Detail window object;
        private Detail detail;
        // rectangle array is Rectagle array for this.rect_colors;
        private System.Windows.Shapes.Rectangle[] rectangle;

        // Check Analyze;
        private bool has_analyze;
        // Check image changed;
        private bool change_image;

        private Thread topColorThread;
        private Thread colorHistogramThread;
        delegate void TopColorThreadDelegate(bool check);
        delegate void SetRectangleColorsDelegate(RGB[] colors, int max);
        delegate void ColorHistogramDelegate();

        public Analyzer()
        {
            InitializeComponent();
            detail = new Detail();
            rectangle = new System.Windows.Shapes.Rectangle[16];
            has_analyze = false;
            change_image = false;
        }

        // Lock the bitmap's bits;
        public void LockImage(BitmapImage image)
        {
            this.image = BitmapImage2Bitmap(image);
            imageURL = image.UriSource.LocalPath;
            filename_block.Text = imageURL;
            txt_status.Text = "";
            rect = new System.Drawing.Rectangle(0, 0, this.image.Width, this.image.Height);
            bmpData = this.image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        }

        // Image Change Check function;
        public bool ImageChangeCheck(bool check)
        {
            change_image = check;
            return false;
        }

        // The Analyze button action;
        private void Analyze_click(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            topColorThread = new Thread(() =>
            {
                sw.Start();
                Dispatcher.Invoke(new TopColorThreadDelegate(Analyze_ButtonDisabler), false);
                BitmapRGBConvert();
                ColorCounter();
                ViewTopColors();
               
                change_image = false;
                has_analyze = true;
                Dispatcher.Invoke(new TopColorThreadDelegate(Analyze_ButtonDisabler), true);
                sw.Stop();
                MessageBox.Show(sw.ElapsedMilliseconds.ToString() + "ms");
            });
            topColorThread.Start();

            colorHistogramThread = new Thread(() =>
            {
                GetHistogram(new Mat(imageURL));
            });
            colorHistogramThread.Start();
        }

        private void GetHistogram(Mat source)
        {
            Mat[] histogram = new Mat[Constants.HISTOGRAM_MAX];    // 0 = blue, 1 = green, 2 = red
            Mat[] histoImage = new Mat[Constants.HISTOGRAM_MAX];
            const int width = 260, height = 200;      
            int[] dimensions = { 256 };         
            Rangef[] ranges = { new Rangef(0, 256) }; 

            // channel
            // 0 = blue
            // 1 = green
            // 2 = red
            double minVal, maxVal;
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

                //Cv2.Normalize(histogram[i], histogram[i], 0, 1, NormTypes.MinMax, -1, null);
                Cv2.MinMaxLoc(histogram[i], out minVal, out maxVal);
                histogram[i] = histogram[i] * (maxVal != 0 ? height / maxVal : 0.0);
                histoImage[i] = new Mat(new OpenCvSharp.Size(width, height), MatType.CV_8UC3, Scalar.All(255));
            }

            int binWidth;
            for (int i = 0; i < dimensions[0]; i++)
            {
                binWidth = (int)((double)width / dimensions[0]);
               
                Cv2.Line(histoImage[0], new OpenCvSharp.Point(i * binWidth, histoImage[0].Rows),
                    new OpenCvSharp.Point((i + 1) * binWidth, histoImage[0].Rows - (int)(histogram[0].Get<float>(i))), 
                    new Scalar(255, 0, 0));

                Cv2.Line(histoImage[1], new OpenCvSharp.Point(i * binWidth, histoImage[1].Rows),
                    new OpenCvSharp.Point((i + 1) * binWidth, histoImage[1].Rows - (int)(histogram[1].Get<float>(i))),
                    new Scalar(0, 255, 0));

                Cv2.Line(histoImage[2], new OpenCvSharp.Point(i * binWidth, histoImage[2].Rows),
                    new OpenCvSharp.Point((i + 1) * binWidth, histoImage[2].Rows - (int)(histogram[2].Get<float>(i))),
                    new Scalar(0, 0, 255));
            }

            for (int i = 0; i < Constants.HISTOGRAM_MAX; i++)
            {
                OpenCvSharp.Extensions.BitmapConverter.ToBitmap(histoImage[i]).Save(System.IO.Directory.GetCurrentDirectory() + @"\" + i + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }

            Dispatcher.Invoke(new ColorHistogramDelegate(DrawColorHistogram));
        }

        private void DrawColorHistogram()
        {
            string cur = System.IO.Directory.GetCurrentDirectory();
            BitmapImage[] images = new BitmapImage[Constants.HISTOGRAM_MAX];
            for (int i = 0; i < Constants.HISTOGRAM_MAX; i++)
            {
                images[i] = new BitmapImage();
                images[i].BeginInit();
                images[i].UriSource = new Uri(cur + @"\" + i + ".png");
                images[i].EndInit();
            }
            img_color_histogram_blue.Source = images[0];
            img_color_histogram_green.Source = images[1];
            img_color_histogram_red.Source = images[2];
        }

        private void Analyze_ButtonDisabler(bool check)
        {
            btn_analyze.IsEnabled = check;
        }

        // Detail_clicks button actions show the details(Color details);
        private void Detail_click1(object sender, RoutedEventArgs e)
        {
            Detail_Window(0);
        }
        private void Detail_click2(object sender, RoutedEventArgs e)
        {
            Detail_Window(1);
        }
        private void Detail_click3(object sender, RoutedEventArgs e)
        {
            Detail_Window(2);
        }
        private void Detail_click4(object sender, RoutedEventArgs e)
        {
            Detail_Window(3);
        }
        private void Detail_click5(object sender, RoutedEventArgs e)
        {
            Detail_Window(4);
        }
        private void Detail_click6(object sender, RoutedEventArgs e)
        {
            Detail_Window(5);
        }
        private void Detail_click7(object sender, RoutedEventArgs e)
        {
            Detail_Window(6);
        }
        private void Detail_click8(object sender, RoutedEventArgs e)
        {
            Detail_Window(7);
        }
        private void Detail_click9(object sender, RoutedEventArgs e)
        {
            Detail_Window(8);
        }
        private void Detail_click10(object sender, RoutedEventArgs e)
        {
            Detail_Window(9);
        }
        private void Detail_click11(object sender, RoutedEventArgs e)
        {
            Detail_Window(10);
        }
        private void Detail_click12(object sender, RoutedEventArgs e)
        {
            Detail_Window(11);
        }
        private void Detail_click13(object sender, RoutedEventArgs e)
        {
            Detail_Window(12);
        }
        private void Detail_click14(object sender, RoutedEventArgs e)
        {
            Detail_Window(13);
        }
        private void Detail_click15(object sender, RoutedEventArgs e)
        {
            Detail_Window(14);
        }
        private void Detail_click16(object sender, RoutedEventArgs e)
        {
            Detail_Window(15);
        }

        // This function is convert bitmapimage to rgb values output "analyzer.dat";
        // and return bytes(Math.Abs(bmpData.Stride) * image.Height)
        private void BitmapRGBConvert()
        {
            StreamWriter streamwriter = new StreamWriter("analyzer.dat");

            // Get the address of the first line;
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap;
            int bytes = Math.Abs(bmpData.Stride) * image.Height;
            byte[] rgbValues = new byte[bytes];
            // ↑ rgbValues[i] -> i%3==0(R), i%3==1(G), i%3==2(B)

            try
            {
                // Copy the RGB values into the array;
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                int numBytes = 0;
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        // numBytes for 24Bit Color;
                        numBytes = ((y * image.Height) + x) * 3;
                        if (numBytes + 2 < bytes)
                            streamwriter.WriteLine(rgbValues[numBytes + 2] + "-" + rgbValues[numBytes + 1] + "-" + rgbValues[numBytes]);
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, ee.Message, "Exception");
            }
            finally
            {
                image.UnlockBits(bmpData);
                streamwriter.Close();
            }
        }

        // It shows the RGB frequency using analyzer.dat and output colorcount.dat;
        // Caution!! : This function is very very heavy. Because the high frequency of the cull of 16 colors.
        private void ColorCounter()
        {
            // Read file "analyzer.dat" for analyze frequency and save new file "colorcount.dat";
            StreamReader streamreader = new StreamReader("analyzer.dat");
            // Save new file "colorcount.dat";
            StreamWriter streamwriter = new StreamWriter("colorcount.dat");
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
                    if(tmpColor.ContainsKey(line))
                    {
                        tmpColor[line].cnt++;
                    }
                    else
                    {
                        tmpColor.Add(line, new ColorC(line));
                    }
                }
                
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "(ColorCounter)Exception : " + e.Message, "Exception");
            }
            finally
            {
                foreach (KeyValuePair<string, ColorC> tmp in tmpColor)
                {
                    streamwriter.WriteLine(tmp.Value.color_code + "-" + tmp.Value.cnt);
                }
                streamwriter.Close();
                streamreader.Close();
                File.Delete("analyzer.dat");
            }
        }

        // Analyze colorcount.dat and drawing color histogram
        private void ViewTopColors()
        {
            StreamReader streamreader = new StreamReader("colorcount.dat");
            //This code(streamwriter) is test code(verify that codes for storage -> frequency array)
            StreamWriter streamwriter = new StreamWriter("frequency.dat");

            RGB[] color = new RGB[16];
           
            int min;
            int max;

            // just counting
            int colorCount = 0;
            string line;
            try
            {
                while ((line = streamreader.ReadLine()) != null)
                {
                    string[] lineSplit = line.Split('-');

                    if(colorCount == Constants.LIST_MAX)
                    {
                        for(int i = 0; i < Constants.LIST_MAX - 1; i++)
                        {
                            if(color[i].frequency > color[i + 1].frequency)
                            {
                                RGB tmp = color[i];
                                color[i] = color[i + 1];
                                color[i + 1] = tmp;
                            }
                        }
                    }

                    if(colorCount >= Constants.LIST_MAX)
                    {
                        for(int i = 0; i < Constants.LIST_MAX; i++)
                        {
                            if(color[i].frequency < int.Parse(lineSplit[Constants.STRING_DIVIDE_FREQUENCY]))
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

            }
            catch (Exception e)
            {
                MessageBox.Show(this, "(ColorHistogram)Exception : " + e.Message, "Exception");
            }
            finally
            {
                for (int i = 0; i < Constants.LIST_MAX; i++)
                {
                    streamwriter.WriteLine(color[i].red + "-" + color[i].green + "-" + color[i].blue + "-" + color[i].frequency);
                }
                streamwriter.Close();
                streamreader.Close();
                File.Delete("colorcount.dat");
            }

            max = color[0].frequency;
            min = max;
            for(int i = 0; i < Constants.LIST_MAX; i++)
            {
                if (max < color[i].frequency)
                {
                    max = color[i].frequency;
                }
                else if (min > color[i].frequency)
                {
                    min = color[i].frequency;
                }
            }

            // Show rectangles of the frequency;
            rectangle[0] = rect_color1;
            rectangle[1] = rect_color2;
            rectangle[2] = rect_color3;
            rectangle[3] = rect_color4;
            rectangle[4] = rect_color5;
            rectangle[5] = rect_color6;
            rectangle[6] = rect_color7;
            rectangle[7] = rect_color8;
            rectangle[8] = rect_color9;
            rectangle[9] = rect_color10;
            rectangle[10] = rect_color11;
            rectangle[11] = rect_color12;
            rectangle[12] = rect_color13;
            rectangle[13] = rect_color14;
            rectangle[14] = rect_color15;
            rectangle[15] = rect_color16;

            // According to result, Fill the rectangle;
            Dispatcher.Invoke(new SetRectangleColorsDelegate(SetRectangleColor), color, max);
        }

        private void SetRectangleColor(RGB[] color, int max)
        {
            for (int i = 0; i < 16; i++)
            {
                rectangle[i].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)color[i].red, (byte)color[i].green, (byte)color[i].blue));
                double height = (double)color[i].frequency;
                rectangle[i].Height = height / max * Constants.RECTANGLE_MAX_HEIGHT;
            }
            btn_color1.IsEnabled = true;
            btn_color2.IsEnabled = true;
            btn_color3.IsEnabled = true;
            btn_color4.IsEnabled = true;
            btn_color5.IsEnabled = true;
            btn_color6.IsEnabled = true;
            btn_color7.IsEnabled = true;
            btn_color8.IsEnabled = true;
            btn_color9.IsEnabled = true;
            btn_color10.IsEnabled = true;
            btn_color11.IsEnabled = true;
            btn_color12.IsEnabled = true;
            btn_color13.IsEnabled = true;
            btn_color14.IsEnabled = true;
            btn_color15.IsEnabled = true;
            btn_color16.IsEnabled = true;
            txt_status.Text = "Analyze Complete";
        }

        // BitmapImage Convert to Bitmap;
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        // Copyright: http://frasergreenroyd.com/how-to-convert-opencv-cvmat-to-system-bitmap-to-system-imagesource/
        private ImageSource BitmapToImageSource(Bitmap imToConvert)
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

            return sc;
        }

        // Show Detail information of color;
        private void Detail_Window(int idx)
        {
            StreamReader streamreader = new StreamReader("frequency.dat");
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
            detail.setDetail(color);
            detail.Owner = this;
            detail.Show();
        }

        // For Re-open Window;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (typeof(System.Windows.Window)).GetField("_isClosing", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sender, false);
            e.Cancel = true;
            (sender as System.Windows.Window).Hide();
        }

        // When this window re-open, it is determined whether or not initialization;
        private void Window_Activated(object sender, EventArgs e)
        {
            if (change_image && has_analyze)
            {
                for (int i = 0; i < 16; i++)
                {
                    rectangle[i].Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                    rectangle[i].Height = 0;
                }
                btn_color1.IsEnabled = false;
                btn_color2.IsEnabled = false;
                btn_color3.IsEnabled = false;
                btn_color4.IsEnabled = false;
                btn_color5.IsEnabled = false;
                btn_color6.IsEnabled = false;
                btn_color7.IsEnabled = false;
                btn_color8.IsEnabled = false;
                btn_color9.IsEnabled = false;
                btn_color10.IsEnabled = false;
                btn_color11.IsEnabled = false;
                btn_color12.IsEnabled = false;
                btn_color13.IsEnabled = false;
                btn_color14.IsEnabled = false;
                btn_color15.IsEnabled = false;
                btn_color16.IsEnabled = false;
                has_analyze = false;
            }
        }
    }
}
