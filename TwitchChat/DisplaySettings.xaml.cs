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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TwitchChat.DS
{
    /// <summary>
    /// Логика взаимодействия для DisplaySettings.xaml
    /// </summary>
    public partial class DisplaySettings : UserControl
    {

        private AlertWindow _alertWindow { get; set; }
        public void SetAlertWindow(AlertWindow alertWindow)
        {
            _alertWindow = alertWindow;
            LoadSettings();
            MyDebug.OnMessage += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                });
                //SaveSettings(); LoadSettings();
            };
            Loaded += (s, e) =>
            {
                LoadSettings();
            };
        }

        private void LoadSettings()
        {
            try
            {
                BroadcasterID.Text = Properties.Settings.Default.BroadcasterId;

                var position = Properties.Settings.Default.Position.Split(" ");

                switch ( Properties.Settings.Default.Position)
                {
                    case "0 0":
                        TopLeftButton.IsChecked = true;
                        break;
                    case "1 0":
                        TopRightButton.IsChecked = true;
                        break;
                    case "0 1":
                        BottomLeftButton.IsChecked = true;
                        break;
                    case "1 1":
                        BottomRightButton.IsChecked = true;
                        break;
                }
                _alertWindow.Card.HorizontalAlignment = position[0] == "0" ? HorizontalAlignment.Left : HorizontalAlignment.Right;
                _alertWindow.Card.VerticalAlignment = position[1] == "0" ? VerticalAlignment.Top : VerticalAlignment.Bottom;

                BackgroundSlider.Value = Properties.Settings.Default.BackgroundSlider;
                ForegroundSlider.Value = Properties.Settings.Default.ForegroundSlider;
                MarginSlider.Value = Properties.Settings.Default.MarginSlider;

                MaxMessagesCount.Text = Properties.Settings.Default.MaxMessageCount.ToString();
                InactiveTimeout.Text = Properties.Settings.Default.InactiveTimeout.ToString();
                //SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void SaveSettings()
        {
            Properties.Settings.Default.BroadcasterId = BroadcasterID.Text;

            string position = "";

            position += _alertWindow.Card.HorizontalAlignment == HorizontalAlignment.Left ? "0" : "1";
            position += " ";
            position += _alertWindow.Card.VerticalAlignment == VerticalAlignment.Top ? "0" : "1";

            Properties.Settings.Default.Position = position;

            Properties.Settings.Default.BackgroundSlider = BackgroundSlider.Value;
            Properties.Settings.Default.ForegroundSlider = ForegroundSlider.Value;
            Properties.Settings.Default.MarginSlider = MarginSlider.Value;


            Properties.Settings.Default.MaxMessageCount = int.Parse(MaxMessagesCount.Text);
            Properties.Settings.Default.InactiveTimeout = int.Parse(InactiveTimeout.Text);

            Properties.Settings.Default.Save();
            _alertWindow.UpdateColor();
            _alertWindow.ResetTimer();
        }

        public DisplaySettings()
        {
            InitializeComponent();
        }

        private void TopLeft(object sender, RoutedEventArgs e)
        {
            _alertWindow.Card.HorizontalAlignment = HorizontalAlignment.Left;
            _alertWindow.Card.VerticalAlignment = VerticalAlignment.Top;
            _alertWindow.UpdateColor();
        }

        private void BottomLeft(object sender, RoutedEventArgs e)
        {
            _alertWindow.Card.HorizontalAlignment = HorizontalAlignment.Left;
            _alertWindow.Card.VerticalAlignment = VerticalAlignment.Bottom;
            _alertWindow.UpdateColor();
        }

        private void BottomRight(object sender, RoutedEventArgs e)
        {
            _alertWindow.Card.HorizontalAlignment = HorizontalAlignment.Right;
            _alertWindow.Card.VerticalAlignment = VerticalAlignment.Bottom;
            _alertWindow.UpdateColor();
        }

        private void TopRight(object sender, RoutedEventArgs e)
        {
            _alertWindow.Card.HorizontalAlignment = HorizontalAlignment.Right;
            _alertWindow.Card.VerticalAlignment = VerticalAlignment.Top;
            _alertWindow.UpdateColor();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_alertWindow != null)
            {
                _alertWindow.SetBackgroundOpacity((sender as Slider)!.Value);
                _alertWindow.UpdateColor();
            }
        }

        private void PinCB_Click(object sender, RoutedEventArgs e)
        {
            if (_alertWindow != null)
            {
                _alertWindow.SetPin((sender as CheckBox)!.IsChecked!.Value);
                _alertWindow.UpdateColor();
            }

        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_alertWindow != null)
            {
                _alertWindow.SetTextOpacity((sender as Slider)!.Value);
                _alertWindow.UpdateColor();
            }
        }

        private void Slider_ValueChanged_2(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_alertWindow != null)
            {
                _alertWindow.SetMargin((sender as Slider)!.Value);
                _alertWindow.UpdateColor();
            }
        }

        private void BroadcasterID_TextChanged(object sender, TextChangedEventArgs e)
        {
            //
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
    }
}
