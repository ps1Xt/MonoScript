using MonoScript.Models.Application;
using MonoScript.Models.Script;
using MonoScript.Script.Elements;
using MonoScript.Script.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoScript.Models.Contexts
{
    public class FindContext
    {
        public LocalSpace LocalSpace { get; set; }
        public MonoType MonoType { get; set; }
        public ScriptFile ScriptFile { get; set; }
        public object ObjectContext { get; set; }

        public FindContext(object objectContext) => ObjectContext = objectContext;

        public bool IsStaticObject
        {
            get
            {
                if ((ObjectContext is Field && (ObjectContext as Field).Modifiers.Contains("static")) || (ObjectContext is Method && (ObjectContext as Method).Modifiers.Contains("static")))
                    return true;

                return false;
            }
        }
    }
}
