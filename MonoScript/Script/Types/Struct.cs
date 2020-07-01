using MonoScript.Models;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using System.Collections.Generic;
using System.Linq;
using MonoScript.Models.Application;

namespace MonoScript.Script.Types
{
    public class Struct : MonoType
    {
        public override List<string[]> AllowedModifierGroups { get; set; } = new List<string[]>()
        {
            new string[] { "public", "readonly" },
            new string[] { "private", "readonly" },
        };
        public Struct(string fullpath, object parentObject, params string[] modifiers)
        {
            FullPath = fullpath;
            ParentObject = parentObject;
            AddModifiers(modifiers.ToList());
        }
        public Struct CloneObject()
        {
            Struct obj = (Struct)MemberwiseClone();
            obj.Methods = Methods.ToList();
            obj.Fields = Fields.ToList();
            obj.OverloadOperators = OverloadOperators.ToList();
            obj.Modifiers = Modifiers.ToList();
            obj.AllowedModifierGroups = AllowedModifierGroups.ToList();

            return obj;
        }

        public static string CreateStructRegex { get; } = Extensions.GetPrefixRegex("struct") + $"\\s+{ObjectNameRegex}\\s*";
    }
}
