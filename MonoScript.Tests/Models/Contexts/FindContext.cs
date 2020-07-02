using MonoScript.Models.Application;
using MonoScript.Models.Script;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Contexts
{
    public class FindContext
    {
        public bool IsStaticContext { get; set; }
        public ScriptFile ScriptFile { get; set; }
        public MonoType MonoType { get; set; }
        public LocalSpace LocalSpace { get; set; }
        public FindContextType SearchResult { get; set; }

        public FindContext(bool isStaticContext)
        {
            IsStaticContext = isStaticContext;
        }
    }

    public enum FindContextType
    {
        Empty, LocalSpace, MonoType, ScriptFileWithMonoType
    }
}
