using MonoScript.Collections;
using MonoScript.Script;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Application
{
    public class ScriptFile
    {
        public string Source { get; private set; }
        public string ImportName { get; private set; }
        public ScriptRoot Root { get; private set; }
        public List<ScriptFile> Imports { get; private set; } = new List<ScriptFile>();
        public List<Using> Usings { get; private set; } = new List<Using>();
        public List<Namespace> Namespaces { get; private set; } = new List<Namespace>();
        public List<Class> Classes { get; private set; } = new List<Class>();
        public List<Struct> Structs { get; private set; } = new List<Struct>();
        public List<MonoEnum> Enums { get; private set; } = new List<MonoEnum>();

        public ScriptFile(string source, string importName)
        {
            Source = source;
            ImportName = importName;
            Root = new ScriptRoot(ReservedCollection.RootNamespace, ReservedCollection.RootClass, ReservedCollection.RootMethod);
        }

        public void SetRoots(string namespacePath, string className, string methodName)
        {
            Root.Namespace.FullPath = namespacePath;
            Root.Class.FullPath = IPath.CombinePath(className, namespacePath);
            Root.Method.FullPath = IPath.CombinePath(methodName, Root.Class.FullPath);
        }

        public static string ImportRegex { get; } = Extensions.GetPrefixRegex("import") + "\\s+((\".*\")|('.*'))\\s*;?";
    }
}
