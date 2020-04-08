using System;

namespace ChatApp.Events
{
    public class ChatMessageEventArgs : EventArgs
    {
        public string Username;
        public string Message;

        public ChatMessageEventArgs(string username, string message)
        {
            Username = username;
            Message = message;
        }
    }
}
