using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Analytics
{
    public class AppMessage
    {
        public string Message { get; }
        public string Source { get; }

        public AppMessage(string message, string source)
        {
            Message = message;
            Source = source;
        }
    }
}
