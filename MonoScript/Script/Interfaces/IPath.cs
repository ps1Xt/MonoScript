using MonoScript.Analytics;
using MonoScript.Script.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoScript.Script.Interfaces
{
    public interface IPath
    {
        public string FullPath { get; set; }
        public string Name { get; }
        public string Path { get; }

        public static bool ContainsWithLevelAccess(string path1, string path2)
        {
            string[] splitPath1 = SplitPath(path1);
            string[] splitPath2 = SplitPath(path2);

            for (int i = 0; i < splitPath1.Length; i++)
            {
                if (i >= splitPath2.Length)
                    return false;

                if (splitPath1[i] != splitPath2[i])
                {
                    if (i + 1 == splitPath2.Length)
                        return true;

                    return false;
                }

                if (i + 1 == splitPath2.Length)
                {
                    if (splitPath1[i] == splitPath2[i])
                        return true;

                    return false;
                }

                if (i + 1 == splitPath1.Length && i + 1 <= splitPath2.Length && splitPath1[i] == splitPath2[i])
                {
                    if (i + 1 == splitPath2.Length || i + 2 == splitPath2.Length)
                        return true;

                    return false;
                }
            }

            return false;
        }
        public static string GetPathWithoutName(string path)
        {
            var splitPath = SplitPath(path);

            if (splitPath.Length > 0)
                return CombinePath(splitPath.Take(splitPath.Length - 1).ToArray());

            return null;
        }
        public static string NameFromPath(string path)
        {
            string[] splits = SplitPath(path);

            if (splits.Length == 0)
                return path;

            if (splits.Length > 1)
                return splits[splits.Length - 1];

            return splits[0];
        }
        public static string Normalize(string value)
        {
            return Regex.Replace(value, "[\\s;]", "");
        }
        public static string CombinePath(string path, string parentPath)
        {
            if (path == null)
            {
                if (parentPath != null)
                    return parentPath;

                return null;
            }

            if (parentPath == null)
                return path;

            if (path.StartsWith(parentPath) && parentPath.IndexOf('.') != -1)
                return string.Format("{0}", path);

            return string.Format("{0}.{1}", parentPath, path);
        }
        public static string CombinePath(string[] paths)
        {
            string path = null;

            for (int i = 0; i < paths.Length; i++)
            {
                if (i < paths.Length - 1)
                    path += paths[i].Trim(' ', '\r', '\n', '\t') + ".";
                else path += paths[i].Trim(' ', '\r', '\n', '\t');
            }

            return path;
        }
        public static string[] SplitPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return new string[0];

            string[] paths = path.Split('.').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            for (int i = 0; i < paths.Length; i++)
                paths[i] = paths[i].Trim(' ', '\r', '\n', '\t');

            return path.Split('.');
        }
    }
}
