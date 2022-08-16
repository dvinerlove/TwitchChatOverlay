using System;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchChat
{
    class Bot
    {
       private TwitchClient? _client;



        public Bot(string username, string accessToken)
        {
            ConnectionCredentials credentials = new ConnectionCredentials(username, /*"82ay3e6etf28pe7cht0447fdvmgjl5"*/accessToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, Properties.Settings.Default.BroadcasterId);

            _client.OnLog += Client_OnLog;
            _client.OnJoinedChannel += Client_OnJoinedChannel;
            _client.OnMessageReceived += Client_OnMessageReceived;
            _client.OnWhisperReceived += Client_OnWhisperReceived;
            _client.OnNewSubscriber += Client_OnNewSubscriber;
            _client.OnConnected += Client_OnConnected;
            _client.OnConnectionError += Client_OnConnectionError;
            _client.OnError += Client_OnError; 
            _client.OnNoPermissionError += Client_OnNoPermissionError; 
            _client.OnUserJoined += Client_OnUserJoined;
            _client.OnUserLeft += Client_OnUserLeft;
            var isConnected = _client.Connect();
            MyDebug.WriteLine($"!!!!!\nis connected - {isConnected}\n!!!!!!!!!!");
        }

        public void Close()
        {
            _client.Disconnect();
            _client = null;
        }

        private void Client_OnUserLeft(object? sender, OnUserLeftArgs e)
        {
            MyDebug.UserLeft(e.Username);
        }

        private void Client_OnUserJoined(object? sender, OnUserJoinedArgs e)
        {
            MyDebug.UserJoined(e.Username);
        }

        private void Client_OnNoPermissionError(object? sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Client_OnError(object? sender, TwitchLib.Communication.Events.OnErrorEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Client_OnConnectionError(object? sender, OnConnectionErrorArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            MyDebug.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            MyDebug.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            //MyDebug.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
            _client.SendMessage(e.Channel, "хмммм...");
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            MyDebug.WriteLine($"Client_OnMessageReceived");
            MyDebug.WriteLine(e.ChatMessage, "message");
            MyDebug.WriteLine($"Client_OnMessageReceived");

            //if (e.ChatMessage.Message.Contains("badword"))
            //    client.TimeoutUser(e.ChatMessage.Channel, e.ChatMessage.Username, TimeSpan.FromMinutes(1), "Bad word! 1 minute timeout!");
        }

        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            MyDebug.WriteLine($"Client_OnWhisperReceived");

            if (e.WhisperMessage.Username == "my_friend")
                _client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }

        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            MyDebug.WriteLine($"Client_OnNewSubscriber");

            //if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
            //    client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
            //else
            //    client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
        }
    }
}