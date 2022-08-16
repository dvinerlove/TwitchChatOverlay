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
using TwitchLib.Client.Models;

namespace TwitchChat
{
    /// <summary>
    /// Логика взаимодействия для AlertModel.xaml
    /// </summary>
    public partial class AlertModel : UserControl
    {
        private ChatMessage? _chatMessage;

        public AlertModel()
        {
            InitializeComponent();

        }

        public AlertModel(ChatMessage chatMessage)
        {
            InitializeComponent();

            Text.Text = chatMessage.Message;

            Username.Text = chatMessage.DisplayName;
            Username.Text = Username.Text.Trim();
            if (string.IsNullOrEmpty(chatMessage.ColorHex) == false)
            {
                Color color;
                try
                {
                    color = (Color)System.Windows.Media.ColorConverter.ConvertFromString(chatMessage.ColorHex);
                }
                catch
                {
                    color = (Color.FromArgb(255, 255, 255, 255));
                }
                Username.Foreground = new SolidColorBrush(color);
            }
        }

        public AlertModel(string username, string message)
        {
            InitializeComponent();

            Text.Text = message;

            Username.Text = username;
            var color = (Color.FromArgb(255, 255, 255, 255));
            Username.Foreground = new SolidColorBrush(color);
        }
    }
}
