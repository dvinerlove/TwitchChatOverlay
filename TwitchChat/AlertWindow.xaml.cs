using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TwitchLib.Client.Models;
using Wpf.Ui.Controls;

namespace TwitchChat
{

    internal enum AccentState
    {
        ACCENT_DISABLED = 1,
        ACCENT_ENABLE_GRADIENT = 0,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }
    /// <summary>
    /// Логика взаимодействия для AlertWindow.xaml
    /// </summary>
    public partial class AlertWindow : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);


        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = (-20);
        private Timer _timer;

        public double MaxOpacity { get; private set; }
        public bool IsPinned { get; private set; } = true;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);


        public AlertWindow()
        {
            InitializeComponent();
            Loaded += AlertWindow_Loaded;
            MyDebug.OnMessage += MyDebug_OnMessage;
            MyDebug.OnEvent +=
            NewMessage;
            Closing += AlertWindow_Closing;
        }

        private void AlertWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {

            Loaded -= AlertWindow_Loaded;
            MyDebug.OnMessage -= MyDebug_OnMessage;
            MyDebug.OnEvent -= NewMessage;
            Closing -= AlertWindow_Closing;
        }

        internal void EnableBlur(bool v)
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            if (v)
            {
                accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            }
            else
            {
                accent.AccentState = AccentState.ACCENT_INVALID_STATE;

            }

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);
            if (v)
            {
            }
            else
            {

            }

        }
        public void ResetTimer()
        {
            //EnableBlur(false);

            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = new Timer();
            _timer.Interval =  Properties.Settings.Default.InactiveTimeout;
            _timer.Elapsed += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (IsPinned)
                    {
                        Opacity -= 1;
                        if (Opacity <= 0)
                        {
                            _timer.Stop();
                        }
                    }
                });
            };
            _timer.Start();
        }

        private void AlertWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            hwnd = new WindowInteropHelper(App.Current.MainWindow).Handle;
            extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            Show();
        }

        private void MyDebug_OnMessage(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        UpdateColor();
                        //AlertWindow_Loaded(sender, new RoutedEventArgs());
                        Show();
                        NewMessage((ChatMessage)sender);
                        Scroll.ScrollToBottom();
                        //_alertWindow.Show();
                        //_alertWindow.Run((ChatMessage)sender);
                    }
                    catch (Exception ex)
                    {
                        //MyDebug.WriteLine(ex.Message);
                    }
                });


            }
        }

        internal void SetMargin(double value)
        {
            Card.Margin = new Thickness(value, value, value, 54 + value);
        }

        internal void SetPin(bool value)
        {
            IsPinned = value;
            if (value)
            {
                TitleBar.Visibility = Visibility.Collapsed;
                WindowState = WindowState.Maximized;
                Topmost = true;
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            }
            else
            {
                TitleBar.Visibility = Visibility.Visible;
                WindowState = WindowState.Normal;
                Topmost = false;
                Background = new UiWindow().Background;

            }
            UpdateColor();

        }
        byte _color = 40;
        public void SetBackgroundOpacity(double opacity = 1)
        {
            MaxOpacity = 1;
            _color = (byte)(255 * opacity);
        }
        public void SetTextOpacity(double opacity = 1)
        {
            MessageStack.Opacity = opacity;
        }
        public void UpdateColor()
        {
            ResetTimer();
            Opacity = IsPinned ? MaxOpacity : 1;
            byte color = 40;
            Card.Background = new SolidColorBrush(Color.FromArgb(_color, color, color, color));

            _timer.Start();
        }

        internal void NewMessage(ChatMessage chatMessage)
        {
            WindowState = WindowState.Maximized;
            MessageStack.Children.Add(new AlertModel(chatMessage));
            while (MessageStack.Children.Count > Properties.Settings.Default.MaxMessageCount)
            {
                MessageStack.Children.RemoveAt(0);
            }
            ResetTimer();
        }
        internal void NewMessage(string username, string message)
        {
            Dispatcher.Invoke(() =>
            {
                WindowState = WindowState.Maximized;
                MessageStack.Children.Add(new AlertModel(username, message));
                while (MessageStack.Children.Count > Properties.Settings.Default.MaxMessageCount)
                {
                    MessageStack.Children.RemoveAt(0);
                }
                ResetTimer();
            });
        }
    }
}
