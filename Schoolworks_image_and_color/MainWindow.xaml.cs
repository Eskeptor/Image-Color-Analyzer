using System;
using System.Windows;
using System.Windows.Media.Imaging;


namespace Schoolworks_image_and_color
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        // Main Bitmap image;
        private BitmapImage mMainImage = null;
        // Original Bitmap image width, height backup;
        private double mWidthOrigin, mHeightOrigin;
        private Analyzer mWindowAnalyzer;
        private About_Window mWindowAbout;
        private SimilarWindow mWindowSimilar;
        private bool mIsChangedImage;

        public MainWindow()
        {
            InitializeComponent();
            main_form.Width = Constants.WIDTH;
            main_form.Height = Constants.HEIGHT;
            main_form.MinWidth = Constants.MIN_WIDTH;
            main_form.MinHeight = Constants.MIN_HEIGHT;
            mWidthOrigin = main_form.Width;
            mHeightOrigin = main_form.Height;
            mWindowAnalyzer = new Analyzer();
            mWindowAbout = new About_Window();
            mWindowSimilar = new SimilarWindow();
            mIsChangedImage = false;
        }

        /* 파일 메뉴(File Menu) -> 열기(Open) */
        private void ActionMenuOpen(object sender, RoutedEventArgs e)
        {           
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpeg";
            dlg.Filter = "JPEG/JPG Files (*.jpeg, *.jpg)|*.jpeg;*.jpg|GIF Files (*.gif)|*.gif|Bitmap Files (*.bmp)|*.bmp|PNG Files (*.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                menu_analyze.IsEnabled = true;
                menu_close.IsEnabled = true;
                menu_similar.IsEnabled = true;
                mMainImage = new BitmapImage();
                mMainImage.BeginInit();
                mMainImage.UriSource = new Uri(dlg.FileName);
                mMainImage.EndInit();
                image.Source = mMainImage;
                mWidthOrigin = mMainImage.Width;
                mHeightOrigin = mMainImage.Height + Constants.OPTIMIZER_UD_MARGIN;
                ActionMenuWindowOptimize();
                mIsChangedImage = true;
            }
            dlg.Reset();
        }

        /* 파일 메뉴(File Menu) -> 파일 닫기(Close) */
        private void ActionMenuClose(object sender, RoutedEventArgs e)
        {
            image.Source = null;
            menu_analyze.IsEnabled = false;
            menu_close.IsEnabled = false;
            menu_similar.IsEnabled = false;
            main_form.Width = Constants.WIDTH;
            main_form.Height = Constants.HEIGHT;
        }

        /* 파일 메뉴(File Menu) -> 종료(Exit) */
        private void ActionMenuExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /* 창 메뉴(Window Menu) -> 창크기조절 활성화(Window Resize Enable) */
        private void ActionMenuResize(object sender, RoutedEventArgs e)
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
        private void ActionMenuReset(object sender, RoutedEventArgs e)
        {
            main_form.Width = Constants.WIDTH;
            main_form.Height = Constants.HEIGHT;
        }

        /* 창 메뉴(Window Menu) -> 창 크기 최적화(Window Size Optimize) */
        private void ActionMenuOptimize(object sender, RoutedEventArgs e)
        {
            if(menu_win_optimize.IsChecked == true)
            {
                menu_win_optimize.IsChecked = false;
                ActionMenuWindowOptimize();
            }
            else
            {
                menu_win_optimize.IsChecked = true;
                ActionMenuWindowOptimize();
            }
        }

        /* 도구 메뉴(Tools Menu) -> 히스토그램 분석(Histogram Analysis) */
        private void ActionMenuAnalyzer(object sender, RoutedEventArgs e)
        {
            mWindowAnalyzer.LockImage(mMainImage);
            mIsChangedImage = mWindowAnalyzer.ImageChangeCheck(mIsChangedImage);
            mWindowAnalyzer.Owner = this;
            mWindowAnalyzer.Show();
        }

        /* 도구 메뉴(Tools Menu) -> 이미지 유사도 비교(Comparative analysis of image similarity) */
        private void ActionMenuSimilar(object sender, RoutedEventArgs e)
        {
            mWindowSimilar.SetOriginImage(mMainImage);
            mWindowSimilar.Owner = this;
            mWindowSimilar.Show();
        }

        /* 도움말 메뉴(Help Menu) -> 프로그램 정보(About Program) */
        private void ActionMenuAbout(object sender, RoutedEventArgs e)
        {
            mWindowAbout.Owner = this;
            mWindowAbout.Show();
        }

        // Window Size Optimizer Function
        private void ActionMenuWindowOptimize()
        {
            if (menu_win_optimize.IsChecked == true)
            {
                double width = mWidthOrigin;
                double height = mHeightOrigin;
                while (width > Constants.OPTIMIZER_WIDTH || height > Constants.OPTIMIZER_HEIGHT)
                {
                    width *= Constants.OPTIMIZER_PERCENT;
                    height *= Constants.OPTIMIZER_PERCENT;
                }
                main_form.Width = width;
                main_form.Height = height;
            }
            else
            {
                main_form.Width = mWidthOrigin;
                main_form.Height = mHeightOrigin;
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
