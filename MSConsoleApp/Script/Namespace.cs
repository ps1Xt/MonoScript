using MonoScript.Models;
using MonoScript.Models.Application;
using MonoScript.Script.Basic;
using MonoScript.Script.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MonoScript.Script
{
    public class Namespace : MonoObject, IObjectParent
    {
        public object ParentObject { get; }
        public Namespace(string fullpath, object parentObject)
        {
            FullPath = fullpath;
            ParentObject = parentObject;
        }

        public static string CreateNamespaceRegex { get; } = Extensions.GetPrefixRegex("namespace") + $"\\s+{ObjectPathRegex}\\s*";
    }
}
