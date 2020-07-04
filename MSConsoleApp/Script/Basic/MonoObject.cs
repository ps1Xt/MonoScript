using MonoScript.Analytics;
using MonoScript.Models;
using MonoScript.Models.Analytics;
using MonoScript.Script.Elements;
using MonoScript.Script.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoScript.Script.Basic
{
    public abstract class MonoObject : IPath
    {
        string _fullpath;
        string _name;

        public string FullPath
        {
            get { return _fullpath; }
            set
            {
                _fullpath = value;
                _name = IPath.NameFromPath(value);
                Path = IPath.GetPathWithoutName(value);

                if (!MonoObject.IsCorrectPath(_fullpath))
                    MLog.AppErrors.Add(new AppMessage("Invalid path.", $"Path: {_fullpath}"));
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _fullpath = IPath.CombinePath(value, IPath.GetPathWithoutName(_fullpath));
                _name = value;
                Path = IPath.GetPathWithoutName(_fullpath);

                if (!MonoObject.IsCorrectName(_name))
                    MLog.AppErrors.Add(new AppMessage("Invalid name.", $"Path: {_fullpath}"));
            }
        }
        public string Path { get; private set; }

        public static string ObjectNameRegex { get; } = "[A-z@_][A-z0-9@_]*";
        public static string ObjectPathRegex { get; } = $"{ObjectNameRegex}(\\s*\\.\\s*{ObjectNameRegex})*";
        public static string NumberRegex { get; } = "[0-9]*.?[0-9]+";
        public static string SingleRemarkRegex { get; } = "#.*";
        public static string MultiRemarkRegex { get; } = "/\\*.*\\*/";

        public static bool IsCorrectName(string name)
        {
            if (name == null)
                name = string.Empty;

            switch (name)
            {
                case "namespace": return false;
                case "class": return false;
                case "struct": return false;
                case "enum": return false;
                case "def": return false;
                case "sdef": return false;
                case "for": return false;
                case "foreach": return false;
                case "switch": return false;
                case "while": return false;
                case "do": return false;
                case "if": return false;
                case "else": return false;
                case "public": return false;
                case "private": return false;
                case "protected": return false;
                case "const": return false;
                case "static": return false;
                case "readonly": return false;
                case "true": return false;
                case "false": return false;
                case "null": return false;
                case "import": return false;
                case "using": return false;
                case "this": return false;
                case "base": return false;
                case "virtual": return false;
                case "ovveride": return false;
                case "sealed": return false;
                case "new": return false;
                case "inherit": return false;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(Regex.Replace(name, ObjectNameRegex, "")))
                return false;

            return true;
        }
        public static bool IsCorrectPath(string path)
        {
            foreach (string name in IPath.SplitPath(path))
            {
                if (!IsCorrectName(name))
                    return false;
            }

            return true;
        }
    }
}
