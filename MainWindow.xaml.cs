using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace MouseProfiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        static extern Boolean SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int SetCurrentProcessExplicitAppUserModelID(string AppID);

        const UInt32 SPI_GETMOUSESPEED = 0x0070;
        const UInt32 SPI_SETMOUSESPEED = 0x0071;
        const uint SPIF_UPDATEINIFILE = 0x01;
        const uint SPIF_SENDCHANGE = 0x02;

        private NotifyIcon notifyIcon;
        private bool isExit;

        string saveLocation;
        string filename = "Settings.xml";
        int activeProfile = 0;

        public MainWindow()
        {
            InitializeComponent();
            CreateNotifyIcon();

            SetAppUserModelId("FraserElliott.MouseProfiles");

            saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MouseProfiles\\";

            var dpi = VisualTreeHelper.GetDpi(this); // 'this' must be a Visual or derived from it

            FormattedText formattedTitle = new FormattedText(
                this.Title,
                CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
                this.FontSize,
                System.Windows.Media.Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip // The required parameter for DPI scaling
            );

            this.MinWidth = formattedTitle.Width + 200;
            this.ResizeMode = ResizeMode.NoResize;

            LoadSettings();
        }

        private void CreateNotifyIcon()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("MouseProfiles.ico");
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += (s, args) => ShowMainWindow();

            notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add("Open", null, (s, e) => ShowMainWindow());
            notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e) => ExitApplication());
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!isExit)
            {
                e.Cancel = true;
                Hide();

                if (DisableNotifications.IsChecked == false)
                {
                    var toast = new ToastWindow("Minimised to tray", "MouseProfiles is still running here.");
                    toast.Show();
                }
            }
            else
            {
                notifyIcon.Dispose();
            }
            
            base.OnClosing(e);
        }

        private void ShowMainWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ExitApplication()
        {
            isExit = true;
            notifyIcon.Dispose();
            Close();
        }

        private void LoadSettings()
        {
            if (!Directory.Exists(saveLocation))
            {
                Directory.CreateDirectory(saveLocation);
            }

            if (!File.Exists(saveLocation + filename))
            {
                File.Create(saveLocation + filename);
            } else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SaveData));

                try
                {
                    using (FileStream stream = new FileStream(saveLocation + filename, FileMode.Open))
                    {
                        object? deserialized = serializer.Deserialize(stream);

                        if (deserialized is SaveData data)
                        {
                            Slider_Profile1.Value = data.Slider1Value;
                            Slider_Profile2.Value = data.Slider2Value;
                            DisableNotifications.IsChecked = data.DisableNotifications;
                            int speed = GetMouseSpeed();
                            if (speed == (int)data.Slider1Value)
                            {
                                activeProfile = 1;
                                BtnProfile1.IsEnabled = false;
                            }
                            else if (speed == (int)data.Slider2Value)
                            {
                                activeProfile = 2;
                                BtnProfile2.IsEnabled = false;
                            }
                        }
                        else
                        {
                            throw new Exception("SaveData not in expected format");
                        }

                    }
                }
                catch (InvalidOperationException e)
                {
                    System.Diagnostics.Debug.WriteLine($"Deserialization error: {e.Message}");
                }
                catch (IOException e)
                {
                    System.Diagnostics.Debug.WriteLine($"File IO error: {e.Message}");
                }
                
            }
        }
        private void SaveSettings()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));

            bool disablenotifications = DisableNotifications.IsChecked == null ? false : (bool)DisableNotifications.IsChecked;

            SaveData data = new SaveData
            {
                Slider1Value = Slider_Profile1.Value,
                Slider2Value = Slider_Profile2.Value,
                DisableNotifications = disablenotifications
            };

            using (FileStream stream = new FileStream(saveLocation + filename, FileMode.Open))
            {
                serializer.Serialize(stream, data);
            }
        }

        public static unsafe int GetMouseSpeed()
        {
            IntPtr speedPtr = Marshal.AllocHGlobal(sizeof(int));

            if (SystemParametersInfo(SPI_GETMOUSESPEED, 0, speedPtr, 0))
            {
                return Marshal.ReadInt32(speedPtr);
            }
            throw new Exception("Unable to get mouse speed.");
        }

        public static unsafe void SetMouseSpeed(int speed)
        {
            if (speed < 1 || speed > 20)
            {
                throw new ArgumentOutOfRangeException("speed", "Mouse speed must be between 1 and 20.");
            }

            IntPtr speedPtr = new IntPtr(speed);

            if (!SystemParametersInfo(SPI_SETMOUSESPEED, 0, speedPtr, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE))
            {
                throw new Exception("Unable to set mouse speed.");
            }
        }

        private void UpdateProfile(int profile, int speed)
        {
            activeProfile = profile;
            SetMouseSpeed(speed);
            if (profile == 1)
            {
                BtnProfile1.IsEnabled = false;
                BtnProfile2.IsEnabled = true;
            } else if (profile == 2) {
                BtnProfile1.IsEnabled = true;
                BtnProfile2.IsEnabled = false;
            }
            SaveSettings();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UpdateProfile(1, (int)Slider_Profile1.Value);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            UpdateProfile(2, (int)Slider_Profile2.Value);
        }

        private void SliderValueChanged(int profile, int value)
        {
            if (profile == activeProfile)
            {
                SetMouseSpeed(value);
                SaveSettings();
            }
        }

        private void Slider_Profile1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderValueChanged(1, (int)Slider_Profile1.Value);
        }

        private void Slider_Profile2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SliderValueChanged(2, (int)Slider_Profile2.Value);
        }

        public static void SetAppUserModelId(string appId)
        {
            SetCurrentProcessExplicitAppUserModelID(appId);
        }

        private void DisableNotifications_Checked(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void DisableNotifications_Unchecked(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
    }
}