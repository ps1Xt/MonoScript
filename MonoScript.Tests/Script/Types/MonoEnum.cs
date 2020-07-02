using MonoScript.Analytics;
using MonoScript.Models;
using MonoScript.Script.Basic;
using MonoScript.Script.Interfaces;
using MonoScript.Script.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoScript.Models.Application;
using MonoScript.Models.Analytics;

namespace MonoScript.Script.Types
{
    public class MonoEnum : MonoObject, IModifier, IObjectParent
    {
        public object ParentObject { get; }
        public List<EnumValue> Values { get; set; } = new List<EnumValue>();
        public List<string> Modifiers { get; set; } = new List<string>();
        public List<string[]> AllowedModifierGroups { get; set; } = new List<string[]>()
        {
            new string[] { "public" },
        };
        public MonoEnum(string fullpath, object parentObject, params string[] modifiers)
        {
            FullPath = fullpath;
            ParentObject = parentObject;
            AddModifiers(modifiers.ToList());
        }

        public MonoEnum CloneObject()
        {
            MonoEnum obj = (MonoEnum)MemberwiseClone();
            obj.Values = Values.ToList();
            obj.Modifiers = Modifiers.ToList();
            obj.AllowedModifierGroups = AllowedModifierGroups.ToList();

            return obj;
        }

        public void AddModifiers(List<string> modifiers)
        {
            Modifiers.AddRange(modifiers);

            if (!Modifiers.Contains("public", "protected", "private"))
                Modifiers.Add("public");

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

        public static List<EnumValue> GetEnumValues(string exfields, string parentPath, object parentObject)
        {
            List<EnumValue> values = new List<EnumValue>();

            int i = 0;
            foreach (string value in exfields.Split(','))
            {
                string fieldName = IPath.Normalize(value);

                if (fieldName != null)
                {
                    EnumValue enumValue = new EnumValue(IPath.CombinePath(fieldName, parentPath), parentPath, parentObject);
                    enumValue.Value = i;
                    values.Add(enumValue);
                }

                i++;
            }

            return values;
        }

        public static string CreateEnumRegex { get; } = /*IModifier.CreateRegexModifiers("public", "private", "new") +*/ Extensions.GetPrefixRegex("enum") + $"\\s+{ObjectNameRegex}\\s*" + "{\\s*" + $"({ObjectNameRegex}(\\s*,\\s*{ObjectNameRegex})*)?" + "\\s*}";

    }
}
