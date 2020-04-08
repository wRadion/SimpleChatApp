using System;

namespace ChatApp.Events
{
    public class UserJoinedEventArgs : EventArgs
    {
        public string Username;

        public UserJoinedEventArgs(string username)
        {
            Username = username;
        }
    }
}
