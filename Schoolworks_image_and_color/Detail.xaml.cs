using System.Windows;
using System.Reflection;

namespace Schoolworks_image_and_color
{
    /// <summary>
    /// Test.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    // Detail information of color
    public partial class Detail : Window
    {
        public Detail()
        {
            InitializeComponent();
        }

        // For Re-open Window
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (typeof(Window)).GetField("_isClosing", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sender, false);
            e.Cancel = true;
            (sender as Window).Hide();
        }

        public void setDetail(string[] color)
        {
            this.box_red.Text = color[0];
            this.box_green.Text = color[1];
            this.box_blue.Text = color[2];
            this.box_freq.Text = color[3];
        }
    }
}
