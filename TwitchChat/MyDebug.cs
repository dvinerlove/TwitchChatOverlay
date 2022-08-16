using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace TwitchChat
{
    public static class MyDebug
    {
        public static event EventHandler OnLog;
        public static event EventHandler OnMessage;
        public static event EventDelegate OnEvent;
        public delegate void EventDelegate(string username,string message);

        internal static void WriteLine(object message, string type="")
        {
            switch (type)
            {
                case "message":
                    OnMessage?.Invoke(message, EventArgs.Empty);
                    break;
                default:
                    OnLog?.Invoke(message, EventArgs.Empty);
                    break;
            }
            Debug.WriteLine(message);

        }

        internal static void UserLeft(string username)
        {
            OnEvent?.Invoke(username, "Left the chat");
        }

        internal static void UserJoined(string username)
        {
            OnEvent?.Invoke(username, "Joined the chat");
        }
    }
}
