using System;
using System.Windows;
using System.Windows.Media.Imaging;

// Just Constant;
static class Constant
{
    public const int WIDTH = 400;
    public const int HEIGHT = 225;
    public const int MIN_WIDTH = 400;
    public const int MIN_HEIGHT = 225;
    public const int OPTIMIZER_WIDTH = 1200;
    public const int OPTIMIZER_HEIGHT = 1200;
    public const double OPTIMIZER_PERCENT = 0.9;
}

namespace Schoolworks_image_and_color
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        // Main Bitmap image;
        BitmapImage mainImage = null;
        // Original Bitmap image width, height backup;
        double width_backup, height_backup;
        Analyzer analyzer;
        About_Window about;
        bool change_image;
        public MainWindow()
        {
            InitializeComponent();
            main_form.Width = Constant.WIDTH;
            main_form.Height = Constant.HEIGHT;
            main_form.MinWidth = Constant.MIN_WIDTH;
            main_form.MinHeight = Constant.MIN_HEIGHT;
            width_backup = main_form.Width;
            height_backup = main_form.Height;
            analyzer = new Analyzer();
            about = new About_Window();
            change_image = false;
        }

        /* 파일 메뉴(File Menu) -> 열기(Open) */
        private void Open_click(object sender, RoutedEventArgs e)
        {           
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpeg";
            dlg.Filter = "JPEG/JPG Files (*.jpeg, *.jpg)|*.jpeg;*.jpg|GIF Files (*.gif)|*.gif|Bitmap Files (*.bmp)|*.bmp|PNG Files (*.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                menu_analyze.IsEnabled = true;
                menu_close.IsEnabled = true;
                mainImage = new BitmapImage();
                mainImage.BeginInit();
                mainImage.UriSource = new Uri(dlg.FileName);
                mainImage.EndInit();
                image.Source = mainImage;
                width_backup = mainImage.Width;
                height_backup = mainImage.Height;
                Window_Optimizer();
                change_image = true;
            }
            dlg.Reset();
        }

        /* 파일 메뉴(File Menu) -> 파일 닫기(Close) */
        private void Close_click(object sender, RoutedEventArgs e)
        {
            image.Source = null;
            menu_analyze.IsEnabled = false;
            menu_close.IsEnabled = false;
            main_form.Width = Constant.WIDTH;
            main_form.Height = Constant.HEIGHT;
        }

        /* 파일 메뉴(File Menu) -> 종료(Exit) */
        private void Exit_click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /* 창 메뉴(Window Menu) -> 창크기조절 활성화(Window Resize Enable) */
        private void Resize_click(object sender, RoutedEventArgs e)
        {
            if (main_form.ResizeMode == ResizeMode.NoResize)
            {
                main_form.ResizeMode = ResizeMode.CanResize;
                menu_resize.IsChecked = true;
            }
            else
            {
                main_form.ResizeMode = ResizeMode.NoResize;
                menu_resize.IsChecked = false;
            }
        }

        /* 창 메뉴(Window Menu) -> 창 크기 초기화(Window Size Reset) */
        private void Reset_click(object sender, RoutedEventArgs e)
        {
            main_form.Width = Constant.WIDTH;
            main_form.Height = Constant.HEIGHT;
        }

        /* 창 메뉴(Window Menu) -> 창 크기 최적화(Window Size Optimize) */
        private void Optimize_click(object sender, RoutedEventArgs e)
        {
            if(menu_win_optimize.IsChecked == true)
            {
                menu_win_optimize.IsChecked = false;
                Window_Optimizer();
            }
            else
            {
                menu_win_optimize.IsChecked = true;
                Window_Optimizer();
            }
        }

        /* 도구 메뉴(Tools Menu) -> 프로그램 정보(About Program) */
        private void Analyze_click(object sender, RoutedEventArgs e)
        {
            analyzer.LockImage(mainImage);
            change_image = analyzer.ImageChangeCheck(change_image);
            analyzer.Owner = this;
            analyzer.Show();
        }

        /* 도움말 메뉴(Help Menu) -> 프로그램 정보(About Program) */
        private void About_click(object sender, RoutedEventArgs e)
        {
            about.Owner = this;
            about.Show();
        }

        // Window Size Optimizer Function
        private void Window_Optimizer()
        {
            if (menu_win_optimize.IsChecked == true)
            {
                double width = width_backup;
                double height = height_backup;
                while (width > Constant.OPTIMIZER_WIDTH || height > Constant.OPTIMIZER_HEIGHT)
                {
                    width *= Constant.OPTIMIZER_PERCENT;
                    height *= Constant.OPTIMIZER_PERCENT;
                }
                main_form.Width = width;
                main_form.Height = height;
            }
            else
            {
                main_form.Width = width_backup;
                main_form.Height = height_backup;
            }
        }

        // When program is close, All Window close. (Detail window is dispatched, so it should be directly terminated);
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            for (int intCounter = Application.Current.Windows.Count - 1; intCounter > 0; intCounter--)
                Application.Current.Windows[intCounter].Close();
            Application.Current.Shutdown();
        }
    }
}
