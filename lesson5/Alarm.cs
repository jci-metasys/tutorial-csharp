using System;
using System.Collections.Generic;

namespace lesson5
{
    public class AlarmCollection
    {
        private string _next;

        public int Total { get; set; }

        public string Next
        {
            get => _next;
            set => _next = value.Length > 0 && value[0] == '/' ? value.Substring(0) : value;
        }
        public string Previous { get; set; }
        public IList<Alarm> Items { get; set; }
        public string Self { get; set; }


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