using MonoScript.Analytics;
using MonoScript.Models;
using MonoScript.Script;
using MonoScript.Script.Basic;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using System;
using System.Collections.Generic;
using System.Text;
using MonoScript.Models.Analytics;
using System.Linq;

namespace MonoScript.Script.Types
{
    public abstract class MonoType : MonoObject, IModifier, IObjectParent
    {
        public object ParentObject { get; protected set; }
        public List<string> Modifiers { get; set; } = new List<string>();
        public virtual List<string[]> AllowedModifierGroups { get; set; } = new List<string[]>();

        public List<Method> Methods { get; set; } = new List<Method>();
        public List<Method> OverloadOperators { get; set; } = new List<Method>();
        public List<Field> Fields { get; set; } = new List<Field>();

        public void AddModifiers(List<string> modifiers)
        {
            Modifiers.AddRange(modifiers);

            bool hasError = false;

            foreach (var group in AllowedModifierGroups)
            {
                int count = 0;
                foreach (var modifier in modifiers)
                {
                    if (!hasError && !group.Contains(modifier))
                        hasError = true;

                    if (group.Contains(modifier))
                        count++;
                }

                if (!hasError || count == modifiers.Count)
                    return;
            }

            if (hasError)
                MLog.AppErrors.Add(new AppMessage("Invalid modifier group.", $"Path {FullPath}"));
        }
        public void TryAddConstructor()
        {
            foreach (var obj in Methods)
            {
                if (obj.Name == Name)
                    return;
            }

            Methods.Add(new Method(IPath.CombinePath(Name, FullPath), this) { Content = string.Empty });
        }
    }
}
