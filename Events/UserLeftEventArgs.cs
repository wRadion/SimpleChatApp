using System;

namespace ChatApp.Events
{
    public class UserLeftEventArgs : EventArgs
    {
        public string Username;

        public UserLeftEventArgs(string username)
        {
            Username = username;
        }
    }
}
