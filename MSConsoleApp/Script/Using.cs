using MonoScript.Analytics;
using MonoScript.Models.Analytics;
using MonoScript.Script.Basic;
using MonoScript.Script.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Script
{
    public class Using
    {
        string _path;
        string _asName;
        public string Path { get => _path; set { if (!MonoObject.IsCorrectPath(value)) MLog.AppErrors.Add(new AppMessage("Invalid Path 'Using'", value)); _path = value; } }
        public string AsName { get => _asName; set { if (!MonoObject.IsCorrectName(value)) MLog.AppErrors.Add(new AppMessage("Invalid AsName 'Using'", value)); _asName = value; } }

        public Using(string path) => Path = path;
        public Using(string path, string asname)
        {
            Path = path;
            AsName = asname;
        }

        public static string UsingRegex { get; } = Extensions.GetPrefixRegex("using") + $"\\s*{MonoObject.ObjectPathRegex}(\\s*as\\s*{MonoObject.ObjectPathRegex})?;?";
    }
}
