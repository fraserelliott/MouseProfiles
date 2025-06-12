using System.CodeDom;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        //static extern Boolean SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, UInt32 pvParam, UInt32 fWinIni);

        const UInt32 SPI_GETMOUSESPEED = 0x0070;
        const UInt32 SPI_SETMOUSESPEED = 0x0071;
        const uint SPIF_UPDATEINIFILE = 0x01;
        const uint SPIF_SENDCHANGE = 0x02;

        string saveLocation;
        string filename = "Settings.xml";
        int activeProfile = 0;

        public struct SliderData
        {
            public double Slider1Value { get; set; }
            public double Slider2Value { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();

            saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MouseProfiles\\";
            System.Diagnostics.Debug.WriteLine(saveLocation);

            var dpi = VisualTreeHelper.GetDpi(this); // 'this' must be a Visual or derived from it

            FormattedText formattedTitle = new FormattedText(
                this.Title,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
                this.FontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip // The required parameter for DPI scaling
            );

            this.MinWidth = formattedTitle.Width + 200;
            this.ResizeMode = ResizeMode.NoResize;

            LoadSettings();
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
                XmlSerializer serializer = new XmlSerializer(typeof(SliderData));

                try
                {
                    using (FileStream stream = new FileStream(saveLocation + filename, FileMode.Open))
                    {
                        object? deserialized = serializer.Deserialize(stream);

                        if (deserialized is SliderData data)
                        {
                            Slider_Profile1.Value = data.Slider1Value;
                            Slider_Profile2.Value = data.Slider2Value;
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
                            throw new Exception("SliderData not in expected format");
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



            

            System.Diagnostics.Debug.WriteLine(GetMouseSpeed());
        }
        private void SaveSettings()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SliderData));
            SliderData data = new SliderData
            {
                Slider1Value = Slider_Profile1.Value,
                Slider2Value = Slider_Profile2.Value
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
    }
}