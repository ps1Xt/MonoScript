using MonoScript.Script;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Application
{
    public class ScriptRoot
    {
        public Namespace Namespace { get; private set; }
        public Class Class { get; private set; }
        public Method Method { get; private set; }

        public ScriptRoot(string namespaceName, string className, string methodName)
        {
            Namespace = new Namespace(namespaceName, null);
            Class = new Class(IPath.CombinePath(className, namespaceName), null);
            Method = new Method(IPath.CombinePath(methodName, Class.FullPath), null);
        }
    }
}
