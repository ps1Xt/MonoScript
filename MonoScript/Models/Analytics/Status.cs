using MonoScript.Analytics;
using MonoScript.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Analytics
{
    public class Status
    {
        public string Message { get; }
        public bool Success { get; set; }

        public Status(string message, bool success)
        {
            Message = message;
            Success = success;
        }

        public static Status ErrorBuild { get { return new Status("Build error.", false); } }
        public static Status ErrorCompleted { get { return new Status("Errors occurred during program execution.", false); } }
        public static Status SuccessBuild { get { return new Status("Success build.", true); } }
        public static Status SuccessfullyCompleted { get { return new Status("Successfully completed.", true); } }
    }
}
