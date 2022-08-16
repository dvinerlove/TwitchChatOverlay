using Newtonsoft.Json.Linq;
using NHttp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using System.Windows.Threading;
using TwitchChat.Tabs;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using Wpf.Ui;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace TwitchChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private HttpServer? _webServer;
        private string clientID = "";
        private string clientSecret = "";
        private string redirectUri = "http://localhost:8082";
        private List<string> Scopes = new List<string>() {
        "chat:read",
            "whispers:read",
            "channel:moderate",
            "chat:edit",
            "whispers:edit",
            "analytics:read:extensions",
            "analytics:read:games",
            "bits:read",
            "channel:edit:commercial",
            "channel:manage:broadcast",
            "channel:manage:extensions",
            "channel:manage:moderators",
            "channel:manage:polls",
            "channel:manage:predictions",
            "moderation:read",
            "moderator:manage:blocked_terms",
            "moderator:manage:banned_users",
            "moderator:manage:blocked_terms",
            "moderator:manage:chat_settings",
            "user:edit",
            "user:edit:follows",
            "user:manage:chat_color",
            "user:manage:whispers",
        };

        //cached
        private string CachedOwnerOfChannelAccessToken = "non";
        private AlertWindow _alertWindow = new AlertWindow();
        private Bot _bot;

        public string? TwitchChannelName { get; private set; }
        public string? TwitchChannelId { get; private set; }
        public TwitchClient? OwnerOfChennelConnection { get; private set; }
        public DisplaySettingsTab ViewSettingsTab { get; }

        public MainWindow()
        {
            InitializeComponent();
            MyDebug.OnLog += MyDebug_OnLog;
            MyDebug.OnMessage += MyDebug_OnMessage;
            ViewSettingsTab = new DisplaySettingsTab();
            SettingsTab.Content = ViewSettingsTab;
            ViewSettingsTab.SetAlertWindow(_alertWindow);
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            if (File.Exists("clientID.txt".ToLower()))
            {
                clientID = File.ReadAllText("clientID.txt".ToLower());
            }

            if (File.Exists("clientSecret.txt".ToLower()))
            {
                clientSecret = File.ReadAllText("clientSecret.txt".ToLower());
            }

            if (string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(clientSecret))
            {
                var result = MessageBox.Show("Twitch clientID or clientSecret are empty\n" +
                    "create clientID.txt and clientSecret.txt files in root directory\n" +
                    "and put the Twitch client ID and client secret in there", "Empty Tokens Error");
                App.Current.Shutdown();
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            Closing -= MainWindow_Closing;
            MyDebug.OnLog -= MyDebug_OnLog;
            MyDebug.OnMessage -= MyDebug_OnMessage;
            if (_bot != null)
            {
                _bot.Close();
            }

            if (_webServer != null)
            {
                _webServer.Stop();
            }
            if (_alertWindow != null)
            {
                _alertWindow.Close();
            }
            Application.Current.Shutdown();

            Process.GetCurrentProcess().Kill();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MyDebug.WriteLine(new ChatMessage("#ffffff", null, "#ffffff", "#ffffff", "#ffffff", new System.Drawing.Color(), null, "#ffffff", TwitchLib.Client.Enums.UserType.Admin, "#ffffff", "#ffffff", false, 1, "#ffffff", false, false, false, false, false, false, false, TwitchLib.Client.Enums.Noisy.NotSet, "#ffffff", "#ffffff", new List<KeyValuePair<string, string>>(), null, 1, 1), "message"); ;
        }

        private void MyDebug_OnMessage(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {


                        //_alertWindow.Show();
                        //_alertWindow.Run((ChatMessage)sender);
                    }
                    catch (Exception ex)
                    {
                        MyDebug.WriteLine(ex.Message);
                    }
                });


            }
        }
        private void MyDebug_OnLog(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                 {
                     StackPanel1.Children.Add(new TextBlock() { Text = sender.ToString() });
                     while (StackPanel1.Children.Count > 30)
                     {
                         StackPanel1.Children.RemoveAt(0);
                     }
                 });
            }
        }
        void InitializeWebServer()
        {
            if (_webServer != null)
            {
                return;
            }

            _webServer = new HttpServer();
            _webServer.EndPoint = new IPEndPoint(IPAddress.Loopback, 8082);

            _webServer.RequestReceived += async (s, e) =>
            {

                using (var writer = new StreamWriter(e.Response.OutputStream))
                {
                    if (e.Request.QueryString.AllKeys.Any("code".Contains))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            WindowState = WindowState.Minimized;
                            WindowState = WindowState.Normal;
                            Topmost = true;
                            this.Focus();
                            Topmost = false;
                            LogTab.Focus();
                            StartButton.IsEnabled = true;
                        });
                        var code = e.Request.QueryString["code"];
                        var ownerOfTheChannelAccessAndRefresh = await GetAccessAndRefreshTokens(code);
                        CachedOwnerOfChannelAccessToken = ownerOfTheChannelAccessAndRefresh.Item1;
                        SetNameAndIDByOauthedUser(CachedOwnerOfChannelAccessToken).Wait();
                        InitializeOwnerOfChannelConnection(TwitchChannelName, CachedOwnerOfChannelAccessToken);
                        InitializeTwitchAPI(CachedOwnerOfChannelAccessToken);

                    }
                }

            };
            _webServer.Start();
            MyDebug.WriteLine($"WebServer started on {_webServer.EndPoint}");

        }

        private void InitializeOwnerOfChannelConnection(string username, string accessToken)
        {
            OwnerOfChennelConnection = new TwitchClient();
            OwnerOfChennelConnection.Initialize(new ConnectionCredentials(username, accessToken));

            OwnerOfChennelConnection.Connect();
            _bot = new Bot(username, accessToken);
            //BotApi botApi = new BotApi(clientID, accessToken);
            //await botApi.ExampleCallsAsync();
        }

        private void InitializeTwitchAPI(string accessToken)
        {
            //TheTwichApi = new TwitchApi();
            //TheTwichApi.Settings.ClientID = clientID;
            //TheTwichApi.Settings.AccessToken = accessToken;

        }

        private async Task SetNameAndIDByOauthedUser(string accessToken)
        {
            var api = new TwitchLib.Api.TwitchAPI();
            api.Settings.ClientId = clientID;
            api.Settings.AccessToken = accessToken;

            var oauthUser = await api.Helix.Users.GetUsersAsync();
            TwitchChannelName = oauthUser.Users[0].Login;
            TwitchChannelId = oauthUser.Users[0].Id;
        }

        private async Task<Tuple<string, string>> GetAccessAndRefreshTokens(string code)
        {
            HttpClient httpClient = new HttpClient();

            var values = new Dictionary<string, string>
            {
                { "client_id",clientID},
                { "client_secret",clientSecret},
                { "code",code},
                { "grant_type","authorization_code"},
                { "redirect_uri",redirectUri},
            };

            var content = new FormUrlEncodedContent(values);

            var response = await httpClient.PostAsync("https://id.twitch.tv/oauth2/token", content);

            var responseString = await response.Content.ReadAsStringAsync();

            var json = JObject.Parse(responseString);
            return new Tuple<string, string>(json["access_token"]!.ToString(), json["refresh_token"]!.ToString());
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.BroadcasterId))
            {
                SettingsTab.Focus();
                return;
            }
            StartButton.IsEnabled = false;
            InitializeWebServer();
            var authUrl = $"https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={clientID}&redirect_uri={redirectUri}&scope={String.Join("+", Scopes)}";

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //this.DragMove();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MyDebug.WriteLine(new ChatMessage("#ffffff", null, "#ffffff", "#ffffff", "", new System.Drawing.Color(), null, "#ffffff", TwitchLib.Client.Enums.UserType.Admin, "#ffffff", "#ffffff", false, 1, "#ffffff", false, false, false, false, false, false, false, TwitchLib.Client.Enums.Noisy.NotSet, "#ffffff", "#ffffff", new List<KeyValuePair<string, string>>(), null, 1, 1), "message"); ;
        }


    }
}
