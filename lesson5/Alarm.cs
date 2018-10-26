using System;
using System.Collections.Generic;

namespace lesson5
{
    public class AlarmCollection
    {
        private string _next;
        private string _previous;
        private string _self;

        public int Total { get; set; }

        private static string StripLeadingSlash(string value)
        {
            return string.IsNullOrEmpty(value) || value[0] != '/' ? value : value.Substring(0);
        }

        public string Next
        {
            get => _next;
            set => _next = StripLeadingSlash(value);
        }

        public string Previous 
        {  
            get => _previous;
            set => _previous = StripLeadingSlash(value);
        }

        public string Self
        {
            get => _self;
            set => _self = StripLeadingSlash(value);
        }

        public IList<Alarm> Items { get; set; }

        public Alarm this[int index] => Items[index];
    }

    public class Alarm
    {
        public Guid Id { get; set; }
        public string ItemReference { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public string @Object { get; set; }
        public int Priority { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class Login
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
    }
}