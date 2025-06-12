using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MouseProfiles
{
    /// <summary>
    /// Interaction logic for ToastWindow.xaml
    /// </summary>
    public partial class ToastWindow : Window
    {
        private int delay;
        private int offsetX;
        private int offsetY;
        private int fadeoutTime;

        public ToastWindow(string title, string message, int delay = 3000, int fadeoutTime = 500, int offsetX = 10, int offsetY = 10)
        {
            InitializeComponent();

            TitleText.Text = title;
            MessageText.Text = message;
            this.delay = delay;
            this.fadeoutTime = fadeoutTime;
            this.offsetX = offsetX;
            this.offsetY = offsetY;

            Loaded += ToastWindow_Loaded;
        }

        private async void ToastWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Position at bottom-right corner (system tray area)
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - offsetX;
            this.Top = desktopWorkingArea.Bottom - this.Height - offsetY;

            await Task.Delay(delay);

            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(fadeoutTime));
            fadeOut.Completed += (s, _) => this.Close();
            this.BeginAnimation(Window.OpacityProperty, fadeOut);
        }
    }
}
